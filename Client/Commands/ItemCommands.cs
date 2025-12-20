using System.Collections.Generic;
using System.Linq;
using MTA.Game;
using MTA.Network.GamePackets;
using MTA.Database;

namespace MTA.Client.Commands
{
    public static class ItemCommands
    {
        public static bool HandleCommand(GameState client, string[] data, string mess)
        {
            return (System.String)data[0] switch
            {
                "refinery" => HandleRefineryCommand(client, data, mess),
                "jar" => HandleJarCommand(client, data, mess),
                "soulp" => HandleSoulpCommand(client, data, mess),
                "effectitem" => HandleEffectItemCommand(client, data, mess),
                "item" => HandleItemCommand(client, data, mess),
                _ => false,
            };
        }

        private static bool HandleRefineryCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @refinery <level>", System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            var level = uint.Parse(data[1]);
            var baseInformations = new SafeDictionary<uint, Refinery.RefineryItem>();
            foreach (var item in Kernel.DatabaseRefinery.Values.Where(item => item.Level == level))
            {
                baseInformations.Add(item.Identifier, item);
            }

            var itemarray = baseInformations.Values.ToArray();
            foreach (var item in itemarray)
            {
                client.Inventory.Add(item.Identifier, 0, 1);
            }

            return true;
        }

        private static bool HandleJarCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 3)
            {
                client.Send(new Message("Usage: @jar <durability> <max durability>", System.Drawing.Color.Red,
                    Message.Tip));
                return true;
            }

            var item = new ConquerItem(true)
            {
                ID = 750000,
                Durability = ushort.Parse(data[1]),
                MaximDurability = ushort.Parse(data[2])
            };
            client.Inventory.Add(item, Enums.ItemUse.CreateAndAdd);
            return true;
        }

        private static bool HandleSoulpCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @soulp <level>", System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            var level = uint.Parse(data[1]);
            var baseInformations = new SafeDictionary<uint, ConquerItemBaseInformation>();
            foreach (var item in ConquerItemInformation.BaseInformations.Values.Where(item =>
                         item.PurificationLevel == level))
            {
                baseInformations.Add(item.ID, item);
            }

            var itemarray = baseInformations.Values.ToArray();
            foreach (var item in itemarray)
            {
                client.Inventory.Add(item.ID, 0, 1);
            }

            return true;
        }

        private static bool HandleEffectItemCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 3)
            {
                client.Send(new Message("Usage: @effectitem <item id> <effect id>", System.Drawing.Color.Red,
                    Message.Tip));
                return true;
            }

            var newItem = new ConquerItem(true)
            {
                ID = uint.Parse(data[1])
            };
            var cibi = ConquerItemInformation.BaseInformations[newItem.ID];
            if (cibi == null)
                return true;
            newItem.Effect = (Enums.ItemEffect)uint.Parse(data[2]);
            newItem.Durability = cibi.Durability;
            newItem.MaximDurability = cibi.Durability;
            newItem.Color = (Enums.Color)Kernel.Random.Next(4, 8);
            client.Inventory.Add(newItem, Enums.ItemUse.CreateAndAdd);
            return true;
        }

        private static bool HandleItemCommand(GameState client, string[] data, string mess)
        {
            if (data.Length < 2)
            {
                client.Send(new Message("Usage: @item <item id or name> [quality] [plus] [bless] [enchant] [socket1] [socket2] [R] [G] [B]", System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            ConquerItemBaseInformation? CIBI = null;

            // Check if first parameter is a numeric ID
            if (uint.TryParse(data[1], out uint itemId))
            {
                // Direct item ID lookup
                if (!ConquerItemInformation.BaseInformations.TryGetValue(itemId, out CIBI))
                {
                    client.Send(new Message($"Item ID {itemId} not found.", System.Drawing.Color.Red, Message.Tip));
                    return true;
                }
            }
            else if (data.Length > 2)
            {
                // Item name lookup (original behavior)
                string ItemName = data[1].ToLower();
                Enums.ItemQuality Quality = Enums.ItemQuality.Fixed;
                switch (data[2].ToLower())
                {
                    case "fixed": Quality = Enums.ItemQuality.Fixed; break;
                    case "normal": Quality = Enums.ItemQuality.Normal; break;
                    case "normalv1": Quality = Enums.ItemQuality.NormalV1; break;
                    case "normalv2": Quality = Enums.ItemQuality.NormalV2; break;
                    case "normalv3": Quality = Enums.ItemQuality.NormalV3; break;
                    case "refined": Quality = Enums.ItemQuality.Refined; break;
                    case "unique": Quality = Enums.ItemQuality.Unique; break;
                    case "elite": Quality = Enums.ItemQuality.Elite; break;
                    case "super": Quality = Enums.ItemQuality.Super; break;
                    case "other": Quality = Enums.ItemQuality.Other; break;
                    default:
                        {
                            Quality = (Enums.ItemQuality)int.Parse(data[2]);
                            break;
                        }
                }
                foreach (ConquerItemBaseInformation infos in ConquerItemInformation.BaseInformations.Values)
                {
                    if (infos.LowerName == ItemName && Quality == (Enums.ItemQuality)(infos.ID % 10))
                    {
                        CIBI = infos;
                    }
                }
            }
            else
            {
                client.Send(new Message("Usage: @item <item id or name> [quality] [plus] [bless] [enchant] [socket1] [socket2] [R] [G] [B]", System.Drawing.Color.Red, Message.Tip));
                return true;
            }

            if (CIBI == null)
                return true;

            ConquerItem newItem = new ConquerItem(true)
            {
                ID = CIBI.ID,
                Durability = CIBI.Durability,
                MaximDurability = CIBI.Durability
            };

            // Handle optional parameters (plus, bless, enchant, sockets, etc.)
            int paramOffset = uint.TryParse(data[1], out _) ? 2 : 3; // Offset depends on whether we used ID or name
            if (data.Length > paramOffset)
            {
                byte.TryParse(data[paramOffset], out byte plus);
                newItem.Plus = System.Math.Min((byte)12, plus);
                if (data.Length > paramOffset + 1)
                {
                    byte.TryParse(data[paramOffset + 1], out byte bless);
                    newItem.Bless = System.Math.Min((byte)7, bless);
                    if (data.Length > paramOffset + 2)
                    {
                        byte.TryParse(data[paramOffset + 2], out byte ench);
                        newItem.Enchant = System.Math.Min((byte)255, ench);
                        if (data.Length > paramOffset + 3)
                        {
                            byte.TryParse(data[paramOffset + 3], out byte soc1);
                            if (System.Enum.IsDefined(typeof(Enums.Gem), soc1))
                            {
                                newItem.SocketOne = (Enums.Gem)soc1;
                            }
                            if (data.Length > paramOffset + 4)
                            {
                                byte.TryParse(data[paramOffset + 4], out byte soc2);
                                if (System.Enum.IsDefined(typeof(Enums.Gem), soc2))
                                {
                                    newItem.SocketTwo = (Enums.Gem)soc2;
                                }
                            }
                            if (data.Length > paramOffset + 7)
                            {
                                byte.TryParse(data[paramOffset + 5], out byte R);
                                byte.TryParse(data[paramOffset + 6], out byte G);
                                byte.TryParse(data[paramOffset + 7], out byte B);
                                newItem.SocketProgress = (uint)(B | (G << 8) | (R << 16));
                            }
                        }
                    }
                }
            }
            newItem.Color = (Enums.Color)Kernel.Random.Next(4, 8);
            if (client.Account.State == AccountTable.AccountState.GM)
                newItem.Bound = true;
            client.Inventory.Add(newItem, Enums.ItemUse.CreateAndAdd);
            return true;
        }
    }
}

