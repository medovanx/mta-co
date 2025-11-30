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
            switch (data[0])
            {
                case "refinery":
                    return HandleRefineryCommand(client, data, mess);

                case "jar":
                    return HandleJarCommand(client, data, mess);

                case "soulp":
                    return HandleSoulpCommand(client, data, mess);

                case "effectitem":
                    return HandleEffectItemCommand(client, data, mess);

                default:
                    return false;
            }
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
    }
}

