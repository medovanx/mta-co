using System;
using System.Linq;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;
using MTA.Database;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// Player Utility NPC - Clears player inventory and PK points, and provides wardrobe items for free
    /// </summary>
    [NpcHandler(34227)]
    public static class Npc_PlayerUtility
    {
        // Wing definitions: (type, id)
        private static readonly (short Type, short Id)[] Wings =
        [
            (6002, 21), (6003, 22), (6004, 23), (6001, 20),
            (6005, 6005), (6009, 6009), (6008, 6008), (6007, 6007),
            (6011, 6011), (6012, 6012), (6013, 6013), (6014, 6014),
            (6015, 6015), (6016, 6016), (6017, 6017)
        ];

        // Title definitions: (type, id)
        private static readonly (short Type, short Id)[] Titles =
        [
            (1, 1000), (2018, 1), (2001, 2), (2002, 3), (2003, 4),
            (2004, 5), (2005, 6), (2006, 7), (2023, 2023), (2022, 2022),
            (2021, 2021), (2020, 2020), (2025, 2025), (2028, 2028), (2029, 2029),
            (2030, 2030), (2031, 2031), (2027, 2027), (2026, 2026), (2032, 2032),
            (2033, 2033), (2034, 2034), (2013, 14), (2014, 15), (2015, 16),
            (2016, 17), (2035, 2035), (2036, 2036), (2037, 2037), (2038, 2038),
            (2040, 2040), (2041, 2041), (2044, 2044), (2045, 2045), (2050, 2050),
            (2051, 2051), (2052, 2052), (2053, 2053), (2054, 2054), (2057, 2057),
            (2046, 2046), (2047, 2047), (2048, 2048), (2049, 2049), (2059, 2059),
            (2060, 2060), (2061, 2061), (2062, 2062)
        ];

        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Hello, how can I help you today?");
                        dialog.Option("Clear my inventory", 1);
                        dialog.Option("Clear all PK points", 3);
                        dialog.Option("Get wardrobe items", 5);
                        dialog.Option("I'm just passing by.", 255);
                        dialog.Send();
                        break;
                    }
                case 1:
                    {
                        dialog.Text("Are you sure you want to clear your inventory?");
                        dialog.Option("Yes, clear", 2);
                        dialog.Option("No, cancel", 255);
                        dialog.Send();
                        break;
                    }
                case 2:
                    {
                        var inventory = new ConquerItem[client.Inventory.Objects.Length];
                        client.Inventory.Objects.CopyTo(inventory, 0);
                        foreach (var item in inventory)
                        {
                            client.Inventory.Remove(item, ItemUse.Remove);
                        }
                        break;
                    }
                case 3:
                    {
                        if (client.Entity.PKPoints > 0)
                        {
                            dialog.Text("Are you sure you want to clear all PK points?");
                            dialog.Option("Yes, clear all PK", 4);
                            dialog.Option("No, cancel", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You don't have any PK points to clear.");
                            dialog.Option("I see.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 4:
                    {
                        if (client.Entity.PKPoints > 0)
                        {
                            client.Entity.PKPoints = 0;
                        }
                        break;
                    }
                case 5:
                    {
                        dialog.Text("I can give you wardrobe items. Which would you like?");
                        dialog.Option("Wings", 6);
                        dialog.Option("Titles", 7);
                        dialog.Option("Title Points +5000", 8);
                        dialog.Option("Back", 0);
                        dialog.Send();
                        break;
                    }
                case 6:
                    {
                        foreach (var (type, id) in Wings)
                        {
                            new TitleStorage().AddTitle(client, type, id, true);
                        }
                        break;
                    }
                case 7:
                    {
                        foreach (var (type, id) in Titles)
                        {
                            new TitleStorage().AddTitle(client, type, id, true);
                        }
                        break;
                    }
                case 8:
                    {
                        var titlePoints = 5000;
                        if (client.Entity.WTitles != null)
                        {
                            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                            {
                                cmd.Update("titles").Set("Points", titlePoints + client.Entity.WTitles.Points).Where("Id", client.Entity.UID).Execute();
                            }
                            StorageManager.Load();
                            client.Entity.TitlePoints += titlePoints;
                        }
                        break;
                    }
            }
        }
    }
}
