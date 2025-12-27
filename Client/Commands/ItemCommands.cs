using System.Collections.Generic;
using System.Linq;
using MTA.Game;
using MTA.Network.GamePackets;
using MTA.Database;
using static System.Byte;

namespace MTA.Client.Commands {
    public static class ItemCommands {
        public static bool HandleCommand(GameState client, string[] data, string mess) {
            return data[0] switch {
                "refinery" => HandleRefineryCommand(client, data),
                "jar" => HandleJarCommand(client, data),
                "soulp" => HandleSoulpCommand(client, data),
                "effectitem" => HandleEffectItemCommand(client, data),
                "item" => HandleItemCommand(client, data),
                _ => false,
            };
        }

        private static bool HandleRefineryCommand(GameState client, string[] data) {
            if (data.Length < 2) {
                client.Send(new Message("Usage: @refinery <level>", System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            var level = uint.Parse(data[1]);
            var baseInformations = new SafeDictionary<uint, Refinery.RefineryItem>();
            foreach (var item in Kernel.DatabaseRefinery.Values.Where(item => item.Level == level)) {
                baseInformations.Add(item.Identifier, item);
            }

            var itemarray = baseInformations.Values.ToArray();
            foreach (var item in itemarray) {
                client.Inventory.Add(item.Identifier, 0, 1);
            }

            return true;
        }

        private static bool HandleJarCommand(GameState client, string[] data) {
            if (data.Length < 3) {
                client.Send(new Message("Usage: @jar <durability> <max durability>", System.Drawing.Color.Red,
                    Message.Tip));
                return true;
            }

            var item = new ConquerItem(true) {
                ID = 750000,
                Durability = ushort.Parse(data[1]),
                MaximDurability = ushort.Parse(data[2])
            };
            client.Inventory.Add(item, Enums.ItemUse.CreateAndAdd);
            return true;
        }

        private static bool HandleSoulpCommand(GameState client, string[] data) {
            if (data.Length < 2) {
                client.Send(new Message("Usage: @soulp <level>", System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            var level = uint.Parse(data[1]);
            var baseInformations = new SafeDictionary<uint, ConquerItemBaseInformation>();
            foreach (var item in ConquerItemInformation.BaseInformations.Values.Where(item =>
                         item.PurificationLevel == level)) {
                baseInformations.Add(item.ID, item);
            }

            var itemarray = baseInformations.Values.ToArray();
            foreach (var item in itemarray) {
                client.Inventory.Add(item.ID, 0, 1);
            }

            return true;
        }

        private static bool HandleEffectItemCommand(GameState client, string[] data) {
            if (data.Length < 3) {
                client.Send(new Message("Usage: @effectitem <item id> <effect id>", System.Drawing.Color.Red,
                    Message.Tip));
                return true;
            }

            var newItem = new ConquerItem(true) {
                ID = uint.Parse(data[1])
            };
            var cibi = ConquerItemInformation.BaseInformations[newItem.ID];
            newItem.Effect = (Enums.ItemEffect)uint.Parse(data[2]);
            newItem.Durability = cibi.Durability;
            newItem.MaximDurability = cibi.Durability;
            newItem.Color = (Enums.Color)Kernel.Random.Next(4, 8);
            client.Inventory.Add(newItem, Enums.ItemUse.CreateAndAdd);
            return true;
        }

        private static bool HandleItemCommand(GameState client, string[] data) {
            if (data.Length < 2) {
                client.Send(new Message(
                    "Usage: @item <item id or name> [quantity] [quality] [plus] [bless] [enchant] [socket1] [socket2] [R] [G] [B]",
                    System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            ConquerItemBaseInformation? cibi = null;
            var isNumericId = false;
            byte quantity = 1;
            var paramOffset = 0;

            // Check if first parameter is a numeric ID
            if (uint.TryParse(data[1], out var itemId)) {
                isNumericId = true;
                // Direct item ID lookup
                if (!ConquerItemInformation.BaseInformations.TryGetValue(itemId, out cibi)) {
                    client.Send(new Message($"Item ID {itemId} not found.", System.Drawing.Color.Red, Message.Tip));
                    return true;
                }

                // Check if second parameter is quantity (when using numeric ID)
                if (data.Length > 2 && TryParse(data[2], out byte qty)) {
                    quantity = qty;
                    paramOffset = 3; // Skip: ID, quantity
                }
                else {
                    paramOffset = 2; // Skip: ID only
                }
            }
            else if (data.Length > 2) {
                // Item name lookup (original behavior)
                var itemName = data[1].ToLower();
                var quality = data[2].ToLower() switch {
                    "fixed" => Enums.ItemQuality.Fixed,
                    "normal" => Enums.ItemQuality.Normal,
                    "normalv1" => Enums.ItemQuality.NormalV1,
                    "normalv2" => Enums.ItemQuality.NormalV2,
                    "normalv3" => Enums.ItemQuality.NormalV3,
                    "refined" => Enums.ItemQuality.Refined,
                    "unique" => Enums.ItemQuality.Unique,
                    "elite" => Enums.ItemQuality.Elite,
                    "super" => Enums.ItemQuality.Super,
                    "other" => Enums.ItemQuality.Other,
                    _ => (Enums.ItemQuality)int.Parse(data[2])
                };

                foreach (var infos in ConquerItemInformation.BaseInformations.Values.Where(infos =>
                             infos.LowerName == itemName && quality == (Enums.ItemQuality)(infos.ID % 10))) {
                    cibi = infos;
                }
            }
            else {
                client.Send(new Message(
                    "Usage: @item <item id or name> [quality] [plus] [bless] [enchant] [socket1] [socket2] [R] [G] [B]",
                    System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            if (cibi == null)
                return true;

            switch (isNumericId) {
                // If using numeric ID with quantity and no customization, use simple Add
                case true when quantity > 1 && data.Length <= paramOffset:
                    client.Inventory.Add(itemId, 0, quantity);
                    return true;
                // Handle optional parameters (plus, bless, enchant, sockets, etc.)
                case false:
                    paramOffset = 3; // For name: skip name + quality
                    break;
            }

            // For numeric ID, paramOffset is already set (2 or 3 depending on quantity)

            // Parse customization parameters
            byte plus = 0, bless = 0, ench = 0, soc1 = 0, soc2 = 0, r = 0, g = 0, b = 0;
            if (data.Length > paramOffset) {
                TryParse(data[paramOffset], out plus);
                if (data.Length <= paramOffset + 1) { }
                else {
                    TryParse(data[paramOffset + 1], out bless);
                    if (data.Length > paramOffset + 2) {
                        TryParse(data[paramOffset + 2], out ench);
                        if (data.Length > paramOffset + 3) {
                            TryParse(data[paramOffset + 3], out soc1);
                            if (data.Length > paramOffset + 4) {
                                TryParse(data[paramOffset + 4], out soc2);
                            }

                            if (data.Length > paramOffset + 7) {
                                TryParse(data[paramOffset + 5], out r);
                                TryParse(data[paramOffset + 6], out g);
                                TryParse(data[paramOffset + 7], out b);
                            }
                        }
                    }
                }
            }

            // Add item quantity times (create new instance for each)
            for (var i = 0; i < quantity; i++) {
                var newItem = new ConquerItem(true) {
                    ID = cibi.ID,
                    Durability = cibi.Durability,
                    MaximDurability = cibi.Durability,
                    Plus = System.Math.Min((byte)12, plus),
                    Bless = System.Math.Min((byte)7, bless),
                    Enchant = System.Math.Min((byte)255, ench),
                    Color = (Enums.Color)Kernel.Random.Next(4, 8)
                };

                if (System.Enum.IsDefined(typeof(Enums.Gem), soc1)) {
                    newItem.SocketOne = (Enums.Gem)soc1;
                }

                if (System.Enum.IsDefined(typeof(Enums.Gem), soc2)) {
                    newItem.SocketTwo = (Enums.Gem)soc2;
                }

                if (data.Length > paramOffset + 7) {
                    newItem.SocketProgress = (uint)(b | (g << 8) | (r << 16));
                }
                client.Inventory.Add(newItem, Enums.ItemUse.CreateAndAdd);
            }

            return true;
        }
    }
}