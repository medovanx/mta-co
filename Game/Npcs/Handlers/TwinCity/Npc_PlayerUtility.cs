using System;
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
                        ConquerItem[] inventory = new ConquerItem[client.Inventory.Objects.Length];
                        client.Inventory.Objects.CopyTo(inventory, 0);

                        foreach (ConquerItem item in inventory)
                        {
                            client.Inventory.Remove(item, MTA.Game.Enums.ItemUse.Remove);
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
                        dialog.Option("Title Points", 8);
                        dialog.Option("Back", 0);
                        dialog.Send();
                        break;
                    }
                case 6:
                    {
                        new TitleStorage().AddTitle(client, 6002, 21, false);
                        new TitleStorage().AddTitle(client, 6003, 22, false);
                        new TitleStorage().AddTitle(client, 6004, 23, false);
                        new TitleStorage().AddTitle(client, 6001, 20, false);
                        new TitleStorage().AddTitle(client, 6005, 6005, false);
                        new TitleStorage().AddTitle(client, 6009, 6009, false);
                        new TitleStorage().AddTitle(client, 6008, 6008, false);
                        new TitleStorage().AddTitle(client, 6007, 6007, false);
                        new TitleStorage().AddTitle(client, 6011, 6011, false);
                        new TitleStorage().AddTitle(client, 6012, 6012, false);
                        new TitleStorage().AddTitle(client, 6013, 6013, false);
                        new TitleStorage().AddTitle(client, 6014, 6014, false);
                        new TitleStorage().AddTitle(client, 6015, 6015, false);
                        new TitleStorage().AddTitle(client, 6016, 6016, false);
                        new TitleStorage().AddTitle(client, 6017, 6017, false);
                        break;
                    }
                case 7:
                    {
                        new TitleStorage().AddTitle(client, 1, 1000, false);
                        new TitleStorage().AddTitle(client, 2018, 1, false);
                        new TitleStorage().AddTitle(client, 2001, 2, false);
                        new TitleStorage().AddTitle(client, 2002, 3, false);
                        new TitleStorage().AddTitle(client, 2003, 4, false);
                        new TitleStorage().AddTitle(client, 2004, 5, false);
                        new TitleStorage().AddTitle(client, 2005, 6, false);
                        new TitleStorage().AddTitle(client, 2006, 7, false);
                        new TitleStorage().AddTitle(client, 2023, 2023, false);
                        new TitleStorage().AddTitle(client, 2022, 2022, false);
                        new TitleStorage().AddTitle(client, 2021, 2021, false);
                        new TitleStorage().AddTitle(client, 2020, 2020, false);
                        new TitleStorage().AddTitle(client, 2025, 2025, false);
                        new TitleStorage().AddTitle(client, 2028, 2028, false);
                        new TitleStorage().AddTitle(client, 2029, 2029, false);
                        new TitleStorage().AddTitle(client, 2030, 2030, false);
                        new TitleStorage().AddTitle(client, 2031, 2031, false);
                        new TitleStorage().AddTitle(client, 2027, 2027, false);
                        new TitleStorage().AddTitle(client, 2026, 2026, false);
                        new TitleStorage().AddTitle(client, 2032, 2032, false);
                        new TitleStorage().AddTitle(client, 2033, 2033, false);
                        new TitleStorage().AddTitle(client, 2034, 2034, false);
                        new TitleStorage().AddTitle(client, 2013, 14, false);
                        new TitleStorage().AddTitle(client, 2014, 15, false);
                        new TitleStorage().AddTitle(client, 2015, 16, false);
                        new TitleStorage().AddTitle(client, 2016, 17, false);
                        new TitleStorage().AddTitle(client, 2035, 2035, false);
                        new TitleStorage().AddTitle(client, 2036, 2036, false);
                        new TitleStorage().AddTitle(client, 2037, 2037, false);
                        new TitleStorage().AddTitle(client, 2038, 2038, false);
                        new TitleStorage().AddTitle(client, 2040, 2040, false);
                        new TitleStorage().AddTitle(client, 2041, 2041, false);
                        new TitleStorage().AddTitle(client, 2044, 2044, false);
                        new TitleStorage().AddTitle(client, 2045, 2045, false);
                        new TitleStorage().AddTitle(client, 2050, 2050, false);
                        new TitleStorage().AddTitle(client, 2051, 2051, false);
                        new TitleStorage().AddTitle(client, 2052, 2052, false);
                        new TitleStorage().AddTitle(client, 2053, 2053, false);
                        new TitleStorage().AddTitle(client, 2054, 2054, false);
                        new TitleStorage().AddTitle(client, 2057, 2057, false);
                        new TitleStorage().AddTitle(client, 2046, 2046, false);
                        new TitleStorage().AddTitle(client, 2047, 2047, false);
                        new TitleStorage().AddTitle(client, 2048, 2048, false);
                        new TitleStorage().AddTitle(client, 2049, 2049, false);
                        new TitleStorage().AddTitle(client, 2059, 2059, false);
                        new TitleStorage().AddTitle(client, 2060, 2060, false);
                        new TitleStorage().AddTitle(client, 2061, 2061, false);
                        new TitleStorage().AddTitle(client, 2062, 2062, false);
                        break;
                    }
                case 8:
                    {
                        if (client.Entity.WTitles != null)
                        {
                            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                            {
                                cmd.Update("titles").Set("Points", 2000).Where("Id", client.Entity.UID).Execute();
                            }
                            StorageManager.Load();
                            client.Entity.TitlePoints += 2500;
                        }
                        break;
                    }
            }
        }
    }
}
