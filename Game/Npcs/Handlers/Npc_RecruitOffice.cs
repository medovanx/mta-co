using System;
using System.Collections.Generic;
using System.Linq;
using static MTA.Game.Enums;
using MTA.Network;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// Recruit Office NPC - Provides Bound Gears and Pack claims when players first join the server
    /// </summary>
    [NpcHandler(225587)]
    public static class Npc_RecruitOffice
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Welcome to MTA Conquer!\nI can help you claim your bound gears then teleport you to Twin City.");
                        dialog.Text("\nNow, which gears would you like to claim?");
                        dialog.Option("Ninja Pack.", 1);
                        dialog.Option("Trojan Pack.", 2);
                        dialog.Option("Monk Pack.", 3);
                        dialog.Option("Archer Pack.", 4);
                        dialog.Option("Fire Taoist Pack.", 5);
                        dialog.Option("Water Taoist Pack.", 6);
                        dialog.Option("Warrior Pack.", 7);
                        dialog.Option("Pirate Pack.", 8);
                        dialog.Option("Dragon Warrior Pack.", 9);
                        dialog.Option("Wind Walker Pack.", 10);
                        dialog.Send();
                        break;
                    }
                case 1:
                    {
                        {
                            //(720356, 0, 1);//MerryPack
                            dialog.Text("Are you sure you want the Ninja Pack?");
                            dialog.Option("Sure.", 55);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }

                        break;
                    }
                case 2:
                    {
                        {
                            //(720356, 0, 1);//MerryPack
                            dialog.Text("Are you sure you want the Trojan Pack?");
                            dialog.Option("Sure.", 15);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 3:
                    {
                        {
                            //(720356, 0, 1);//MerryPack
                            dialog.Text("Are you sure you want the Monk Pack?");
                            dialog.Option("Sure.", 65);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 4:
                    {
                        {
                            //(720356, 0, 1);//MerryPack
                            dialog.Text("Are you sure you want the Archer Pack?");
                            dialog.Option("Sure.", 45);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 5:
                    {
                        {
                            //(720356, 0, 1);//MerryPack
                            dialog.Text("Are you sure you want the FireTaoist Pack?");
                            dialog.Option("Sure.", 145);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 6:
                    {
                        {
                            //(720356, 0, 1);//MerryPack
                            dialog.Text("Are you sure you want the WaterTaoist Pack?");
                            dialog.Option("Sure.", 135);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 7:
                    {
                        {
                            //(720356, 0, 1);//MerryPack
                            dialog.Text("Are you sure you want the Warrior Pack?");
                            dialog.Option("Sure.", 25);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 8:
                    {
                        {
                            //(720356, 0, 1);//MerryPack
                            dialog.Text("Are you sure you want the Pirate Pack?");
                            dialog.Option("Sure.", 75);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 9:
                    {
                        {
                            //(720356, 0, 1);//MerryPack
                            dialog.Text("Are you sure you want the Dragon-Warrior Pack?");
                            dialog.Option("Sure.", 85);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 10:
                    {
                        {
                            //(720356, 0, 1);//MerryPack
                            dialog.Text("Are you sure you want the WindWalker Pack?");
                            dialog.Option("Sure.", 165);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region BoundItems Trojan
                case 15:
                    {
                        uint itemid = 202009;//Tower
                        uint itemid2 = 201009;//Fan
                        uint itemid3 = 160019;//Boot
                        uint itemid4 = 120029;//Necklace
                        uint itemid5 = 150019;//Ring
                        uint itemid6 = 117009;//Earrings
                        uint itemid7 = 130009;//Armor
                        uint itemid8 = 480019;//Club
                        uint itemid9 = 410019;//Blade
                        uint itemid10 = 204009;//TempestWing

                        if (client.Inventory.Count <= 30)
                        {
                            //  if (client.Inventory.Contains(720356, 1))
                            {
                                ConquerItem item2 = new ConquerItem(true);
                                item2.ID = itemid2;
                                item2.Color = MTA.Game.Enums.Color.White;
                                item2.Bound = true;
                                item2.Plus = 3;
                                item2.Durability = item2.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid2].Durability;
                                client.Inventory.Add(item2, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item3 = new ConquerItem(true);
                                item3.ID = itemid3;
                                item3.Color = MTA.Game.Enums.Color.White;
                                item3.Bound = true;
                                item3.Plus = 3;
                                item3.Durability = item3.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid3].Durability;
                                client.Inventory.Add(item3, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item4 = new ConquerItem(true);
                                item4.ID = itemid4;
                                item4.Color = MTA.Game.Enums.Color.White;
                                item4.Bound = true;
                                item4.Plus = 3;
                                item4.Durability = item4.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid4].Durability;
                                client.Inventory.Add(item4, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item5 = new ConquerItem(true);
                                item5.ID = itemid5;
                                item5.Color = MTA.Game.Enums.Color.White;
                                item5.Bound = true;
                                item5.Plus = 3;
                                item5.Durability = item5.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid5].Durability;
                                client.Inventory.Add(item5, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item6 = new ConquerItem(true);
                                item6.ID = itemid6;
                                item6.Color = MTA.Game.Enums.Color.White;
                                item6.Bound = true;
                                item6.Plus = 3;
                                item6.Durability = item6.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid6].Durability;
                                client.Inventory.Add(item6, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item7 = new ConquerItem(true);
                                item7.ID = itemid7;
                                item7.Color = MTA.Game.Enums.Color.White;
                                item7.Bound = true;
                                item7.Plus = 3;
                                item7.Durability = item7.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid7].Durability;
                                client.Inventory.Add(item7, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item8 = new ConquerItem(true);
                                item8.ID = itemid8;
                                item8.Color = MTA.Game.Enums.Color.White;
                                item8.Bound = true;
                                item8.Plus = 3;
                                item8.Durability = item8.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid8].Durability;
                                client.Inventory.Add(item8, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item9 = new ConquerItem(true);
                                item9.ID = itemid9;
                                item9.Color = MTA.Game.Enums.Color.White;
                                item9.Bound = true;
                                item9.Plus = 3;
                                item9.Durability = item9.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid9].Durability;
                                client.Inventory.Add(item9, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item10 = new ConquerItem(true);
                                item10.ID = itemid10;
                                item10.Color = MTA.Game.Enums.Color.White;
                                item10.Bound = true;
                                item10.Plus = 3;
                                //item10.SocketOne = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.SocketTwo = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.Bless = 1;
                                item10.Durability = item10.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid10].Durability;
                                client.Inventory.Add(item10, MTA.Game.Enums.ItemUse.CreateAndAdd);
                                ConquerItem item = new ConquerItem(true);
                                item.ID = itemid;
                                item.Color = MTA.Game.Enums.Color.White;
                                item.Bound = true;
                                item.Plus = 3;
                                item.Durability = item.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid].Durability;
                                if (client.Inventory.Add(item, MTA.Game.Enums.ItemUse.CreateAndAdd))
                                {
                                    client.Entity.Teleport(1002, 303, 278);
                                    Network.GamePackets.NpcReply npc = new Network.GamePackets.NpcReply(6, "Have fun in MTA Conquer!");
                                    npc.OptionID = 255;
                                    client.Send(npc.ToArray());
                                }
                                else
                                {
                                    dialog.Text("You need at least one free slot in your inventory.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }

                        }
                        else
                        {
                            dialog.Text("You need to make at least 10 free slots in your inventory.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region BoundItems Warrior
                case 25:
                    {
                        uint itemid = 202009;//Tower
                        uint itemid2 = 201009;//Fan
                        uint itemid3 = 160019;//Boot
                        uint itemid4 = 120029;//Necklace
                        uint itemid5 = 150019;//Ring
                        uint itemid6 = 117009;//Earrings
                        uint itemid7 = 131009;//Armor
                        uint itemid8 = 480019;//Club
                        uint itemid9 = 900009;//Shield
                        uint itemid10 = 204009;//TempestWing
                        if (client.Inventory.Count <= 30)
                        {
                            //  if (client.Inventory.Contains(720356, 1))
                            {
                                ConquerItem item2 = new ConquerItem(true);
                                item2.ID = itemid2;
                                item2.Color = MTA.Game.Enums.Color.White;
                                item2.Bound = true;
                                item2.Plus = 3;
                                item2.Durability = item2.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid2].Durability;
                                client.Inventory.Add(item2, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item3 = new ConquerItem(true);
                                item3.ID = itemid3;
                                item3.Color = MTA.Game.Enums.Color.White;
                                item3.Bound = true;
                                item3.Plus = 3;
                                item3.Durability = item3.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid3].Durability;
                                client.Inventory.Add(item3, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item4 = new ConquerItem(true);
                                item4.ID = itemid4;
                                item4.Color = MTA.Game.Enums.Color.White;
                                item4.Bound = true;
                                item4.Plus = 3;
                                item4.Durability = item4.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid4].Durability;
                                client.Inventory.Add(item4, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item5 = new ConquerItem(true);
                                item5.ID = itemid5;
                                item5.Color = MTA.Game.Enums.Color.White;
                                item5.Bound = true;
                                item5.Plus = 3;
                                item5.Durability = item5.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid5].Durability;
                                client.Inventory.Add(item5, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item6 = new ConquerItem(true);
                                item6.ID = itemid6;
                                item6.Color = MTA.Game.Enums.Color.White;
                                item6.Bound = true;
                                item6.Plus = 3;
                                item6.Durability = item6.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid6].Durability;
                                client.Inventory.Add(item6, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item7 = new ConquerItem(true);
                                item7.ID = itemid7;
                                item7.Color = MTA.Game.Enums.Color.White;
                                item7.Bound = true;
                                item7.Plus = 3;
                                item7.Durability = item7.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid7].Durability;
                                client.Inventory.Add(item7, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item8 = new ConquerItem(true);
                                item8.ID = itemid8;
                                item8.Color = MTA.Game.Enums.Color.White;
                                item8.Bound = true;
                                item8.Plus = 3;
                                item8.Durability = item8.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid8].Durability;
                                client.Inventory.Add(item8, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item9 = new ConquerItem(true);
                                item9.ID = itemid9;
                                item9.Color = MTA.Game.Enums.Color.White;
                                item9.Bound = true;
                                item9.Plus = 3;
                                item9.Durability = item9.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid9].Durability;
                                client.Inventory.Add(item9, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item10 = new ConquerItem(true);
                                item10.ID = itemid10;
                                item10.Color = MTA.Game.Enums.Color.White;
                                item10.Bound = true;
                                item10.Plus = 3;
                                //item10.SocketOne = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.SocketTwo = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.Bless = 1;
                                item10.Durability = item10.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid10].Durability;
                                client.Inventory.Add(item10, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item = new ConquerItem(true);
                                item.ID = itemid;
                                item.Color = MTA.Game.Enums.Color.White;
                                item.Bound = true;
                                item.Plus = 3;
                                item.Durability = item.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid].Durability;
                                if (client.Inventory.Add(item, MTA.Game.Enums.ItemUse.CreateAndAdd))
                                {
                                    client.Entity.Teleport(1002, 303, 278);
                                    npc.OptionID = 255;
                                    client.Send(npc.ToArray());
                                }
                                else
                                {
                                    dialog.Text("You need at least one free slot in your inventory.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }

                        }
                        else
                        {
                            dialog.Text("You need to make at least 10 free slots in your inventory.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region BoundItems Archer
                case 45:
                    {
                        uint itemid = 202009;//Tower
                        uint itemid2 = 201009;//Fan
                        uint itemid3 = 160019;//Boot
                        uint itemid4 = 120029;//Necklace
                        uint itemid5 = 150019;//Ring
                        uint itemid6 = 117009;//Earrings
                        uint itemid7 = 133009;//Armor
                        uint itemid8 = 500009;//Bow
                                              //uint itemid9 = 900009;//Shield
                        uint itemid9 = 204009;//TempestWing
                        if (client.Inventory.Count <= 30)
                        {
                            //    if (client.Inventory.Contains(720356, 1))
                            {
                                ConquerItem item2 = new ConquerItem(true);
                                item2.ID = itemid2;
                                item2.Color = MTA.Game.Enums.Color.White;
                                item2.Bound = true;
                                item2.Plus = 3;
                                item2.Durability = item2.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid2].Durability;
                                client.Inventory.Add(item2, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item3 = new ConquerItem(true);
                                item3.ID = itemid3;
                                item3.Color = MTA.Game.Enums.Color.White;
                                item3.Bound = true;
                                item3.Plus = 3;
                                item3.Durability = item3.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid3].Durability;
                                client.Inventory.Add(item3, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item4 = new ConquerItem(true);
                                item4.ID = itemid4;
                                item4.Color = MTA.Game.Enums.Color.White;
                                item4.Bound = true;
                                item4.Plus = 3;
                                item4.Durability = item4.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid4].Durability;
                                client.Inventory.Add(item4, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item5 = new ConquerItem(true);
                                item5.ID = itemid5;
                                item5.Color = MTA.Game.Enums.Color.White;
                                item5.Bound = true;
                                item5.Plus = 3;
                                item5.Durability = item5.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid5].Durability;
                                client.Inventory.Add(item5, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item6 = new ConquerItem(true);
                                item6.ID = itemid6;
                                item6.Color = MTA.Game.Enums.Color.White;
                                item6.Bound = true;
                                item6.Plus = 3;
                                item6.Durability = item6.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid6].Durability;
                                client.Inventory.Add(item6, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item7 = new ConquerItem(true);
                                item7.ID = itemid7;
                                item7.Color = MTA.Game.Enums.Color.White;
                                item7.Bound = true;
                                item7.Plus = 3;
                                item7.Durability = item7.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid7].Durability;
                                client.Inventory.Add(item7, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item8 = new ConquerItem(true);
                                item8.ID = itemid8;
                                item8.Color = MTA.Game.Enums.Color.White;
                                item8.Bound = true;
                                item8.Plus = 3;
                                item8.Durability = item8.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid8].Durability;
                                client.Inventory.Add(item8, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item9 = new ConquerItem(true);
                                item9.ID = itemid9;
                                item9.Color = MTA.Game.Enums.Color.White;
                                item9.Bound = true;
                                item9.Plus = 3;
                                // item9.SocketOne = MTA.Game.Enums.Gem.EmptySocket;
                                // item9.SocketTwo = MTA.Game.Enums.Gem.EmptySocket;
                                //  item9.Bless = 1;
                                item9.Durability = item9.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid9].Durability;
                                client.Inventory.Add(item9, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item = new ConquerItem(true);
                                item.ID = itemid;
                                item.Color = MTA.Game.Enums.Color.White;
                                item.Bound = true;
                                item.Plus = 3;
                                item.Durability = item.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid].Durability;
                                if (client.Inventory.Add(item, MTA.Game.Enums.ItemUse.CreateAndAdd))
                                {
                                    client.Entity.Teleport(1002, 303, 278);
                                    npc.OptionID = 255;
                                    client.Send(npc.ToArray());
                                }
                                else
                                {
                                    dialog.Text("You need at least one free slot in your inventory.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }

                        }
                        else
                        {
                            dialog.Text("You need to make at least 10 free slots in your inventory.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region BoundItems Ninja
                case 55:
                    {
                        uint itemid = 202009;//Tower
                        uint itemid2 = 201009;//Fan
                        uint itemid3 = 160019;//Boot
                        uint itemid4 = 120029;//Necklace
                        uint itemid5 = 150019;//Ring
                        uint itemid6 = 117009;//Earrings
                        uint itemid7 = 135009;//Armor
                        uint itemid8 = 601019;//Katana
                        uint itemid9 = 601019;//Katana
                        uint itemid10 = 204009;//TempestWing
                        if (client.Inventory.Count <= 30)
                        {
                            //   if (client.Inventory.Contains(720356, 1))
                            {
                                ConquerItem item2 = new ConquerItem(true);
                                item2.ID = itemid2;
                                item2.Color = MTA.Game.Enums.Color.White;
                                item2.Bound = true;
                                item2.Plus = 3;
                                item2.Durability = item2.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid2].Durability;
                                client.Inventory.Add(item2, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item3 = new ConquerItem(true);
                                item3.ID = itemid3;
                                item3.Color = MTA.Game.Enums.Color.White;
                                item3.Bound = true;
                                item3.Plus = 3;
                                item3.Durability = item3.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid3].Durability;
                                client.Inventory.Add(item3, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item4 = new ConquerItem(true);
                                item4.ID = itemid4;
                                item4.Color = MTA.Game.Enums.Color.White;
                                item4.Bound = true;
                                item4.Plus = 3;
                                item4.Durability = item4.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid4].Durability;
                                client.Inventory.Add(item4, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item5 = new ConquerItem(true);
                                item5.ID = itemid5;
                                item5.Color = MTA.Game.Enums.Color.White;
                                item5.Bound = true;
                                item5.Plus = 3;
                                item5.Durability = item5.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid5].Durability;
                                client.Inventory.Add(item5, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item6 = new ConquerItem(true);
                                item6.ID = itemid6;
                                item6.Color = MTA.Game.Enums.Color.White;
                                item6.Bound = true;
                                item6.Plus = 3;
                                item6.Durability = item6.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid6].Durability;
                                client.Inventory.Add(item6, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item7 = new ConquerItem(true);
                                item7.ID = itemid7;
                                item7.Color = MTA.Game.Enums.Color.White;
                                item7.Bound = true;
                                item7.Plus = 3;
                                item7.Durability = item7.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid7].Durability;
                                client.Inventory.Add(item7, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item8 = new ConquerItem(true);
                                item8.ID = itemid8;
                                item8.Color = MTA.Game.Enums.Color.White;
                                item8.Bound = true;
                                item8.Plus = 3;
                                item8.Durability = item8.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid8].Durability;
                                client.Inventory.Add(item8, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item9 = new ConquerItem(true);
                                item9.ID = itemid9;
                                item9.Color = MTA.Game.Enums.Color.White;
                                item9.Bound = true;
                                item9.Plus = 3;
                                item9.Durability = item9.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid9].Durability;
                                client.Inventory.Add(item9, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item10 = new ConquerItem(true);
                                item10.ID = itemid10;
                                item10.Color = MTA.Game.Enums.Color.White;
                                item10.Bound = true;
                                item10.Plus = 3;
                                //item10.SocketOne = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.SocketTwo = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.Bless = 1;
                                item10.Durability = item10.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid10].Durability;
                                client.Inventory.Add(item10, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item = new ConquerItem(true);
                                item.ID = itemid;
                                item.Color = MTA.Game.Enums.Color.White;
                                item.Bound = true;
                                item.Plus = 3;
                                item.Durability = item.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid].Durability;
                                if (client.Inventory.Add(item, MTA.Game.Enums.ItemUse.CreateAndAdd))
                                {
                                    client.Entity.Teleport(1002, 303, 278);
                                    npc.OptionID = 255;
                                    client.Send(npc.ToArray());
                                }
                                else
                                {
                                    dialog.Text("You need at least one free slot in your inventory.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }

                        }
                        else
                        {
                            dialog.Text("You need to make at least 10 free slots in your inventory.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region BoundItems Monk
                case 65:
                    {
                        uint itemid = 202009;//Tower
                        uint itemid2 = 201009;//Fan
                        uint itemid3 = 160019;//Boot
                        uint itemid4 = 120029;//Necklace
                        uint itemid5 = 150019;//Ring
                        uint itemid6 = 117009;//Earrings
                        uint itemid7 = 136009;//Armor
                        uint itemid8 = 610019;//Katana
                        uint itemid9 = 610019;//Katana
                        uint itemid10 = 204009;//TempestWing
                        if (client.Inventory.Count <= 30)
                        {
                            //     if (client.Inventory.Contains(720356, 1))
                            {
                                ConquerItem item2 = new ConquerItem(true);
                                item2.ID = itemid2;
                                item2.Color = MTA.Game.Enums.Color.White;
                                item2.Bound = true;
                                item2.Plus = 3;
                                item2.Durability = item2.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid2].Durability;
                                client.Inventory.Add(item2, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item3 = new ConquerItem(true);
                                item3.ID = itemid3;
                                item3.Color = MTA.Game.Enums.Color.White;
                                item3.Bound = true;
                                item3.Plus = 3;
                                item3.Durability = item3.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid3].Durability;
                                client.Inventory.Add(item3, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item4 = new ConquerItem(true);
                                item4.ID = itemid4;
                                item4.Color = MTA.Game.Enums.Color.White;
                                item4.Bound = true;
                                item4.Plus = 3;
                                item4.Durability = item4.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid4].Durability;
                                client.Inventory.Add(item4, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item5 = new ConquerItem(true);
                                item5.ID = itemid5;
                                item5.Color = MTA.Game.Enums.Color.White;
                                item5.Bound = true;
                                item5.Plus = 3;
                                item5.Durability = item5.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid5].Durability;
                                client.Inventory.Add(item5, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item6 = new ConquerItem(true);
                                item6.ID = itemid6;
                                item6.Color = MTA.Game.Enums.Color.White;
                                item6.Bound = true;
                                item6.Plus = 3;
                                item6.Durability = item6.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid6].Durability;
                                client.Inventory.Add(item6, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item7 = new ConquerItem(true);
                                item7.ID = itemid7;
                                item7.Color = MTA.Game.Enums.Color.White;
                                item7.Bound = true;
                                item7.Plus = 3;
                                item7.Durability = item7.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid7].Durability;
                                client.Inventory.Add(item7, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item8 = new ConquerItem(true);
                                item8.ID = itemid8;
                                item8.Color = MTA.Game.Enums.Color.White;
                                item8.Bound = true;
                                item8.Plus = 3;
                                item8.Durability = item8.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid8].Durability;
                                client.Inventory.Add(item8, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item9 = new ConquerItem(true);
                                item9.ID = itemid9;
                                item9.Color = MTA.Game.Enums.Color.White;
                                item9.Bound = true;
                                item9.Plus = 3;
                                item9.Durability = item9.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid9].Durability;
                                client.Inventory.Add(item9, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item10 = new ConquerItem(true);
                                item10.ID = itemid10;
                                item10.Color = MTA.Game.Enums.Color.White;
                                item10.Bound = true;
                                item10.Plus = 3;
                                //item10.SocketOne = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.SocketTwo = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.Bless = 1;
                                item10.Durability = item10.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid10].Durability;
                                client.Inventory.Add(item10, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item = new ConquerItem(true);
                                item.ID = itemid;
                                item.Color = MTA.Game.Enums.Color.White;
                                item.Bound = true;
                                item.Plus = 3;
                                item.Durability = item.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid].Durability;
                                if (client.Inventory.Add(item, MTA.Game.Enums.ItemUse.CreateAndAdd))
                                {
                                    client.Entity.Teleport(1002, 303, 278);
                                    npc.OptionID = 255;
                                    client.Send(npc.ToArray());
                                }
                                else
                                {
                                    dialog.Text("You need at least one free slot in your inventory.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }

                        }
                        else
                        {
                            dialog.Text("You need to make at least 10 free slots in your inventory.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region BoundItems WaterTaoist
                case 135:
                    {
                        uint itemid = 202009;//Tower
                        uint itemid2 = 201009;//Fan
                        uint itemid3 = 160019;//Boot
                        uint itemid4 = 120029;//Necklace
                        uint itemid5 = 150019;//Ring
                        uint itemid6 = 114009;//Cap
                        uint itemid7 = 134009;//Armor
                        uint itemid8 = 560019;//Spear
                        uint itemid9 = 421009;//BackSword
                        uint itemid10 = 204009;//TempestWing
                        if (client.Inventory.Count <= 30)
                        {
                            //   if (client.Inventory.Contains(720356, 1))
                            {
                                ConquerItem item2 = new ConquerItem(true);
                                item2.ID = itemid2;
                                item2.Color = MTA.Game.Enums.Color.White;
                                item2.Bound = true;
                                item2.Plus = 3;
                                item2.Durability = item2.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid2].Durability;
                                client.Inventory.Add(item2, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item3 = new ConquerItem(true);
                                item3.ID = itemid3;
                                item3.Color = MTA.Game.Enums.Color.White;
                                item3.Bound = true;
                                item3.Plus = 3;
                                item3.Durability = item3.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid3].Durability;
                                client.Inventory.Add(item3, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item4 = new ConquerItem(true);
                                item4.ID = itemid4;
                                item4.Color = MTA.Game.Enums.Color.White;
                                item4.Bound = true;
                                item4.Plus = 3;
                                item4.Durability = item4.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid4].Durability;
                                client.Inventory.Add(item4, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item5 = new ConquerItem(true);
                                item5.ID = itemid5;
                                item5.Color = MTA.Game.Enums.Color.White;
                                item5.Bound = true;
                                item5.Plus = 3;
                                item5.Durability = item5.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid5].Durability;
                                client.Inventory.Add(item5, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item6 = new ConquerItem(true);
                                item6.ID = itemid6;
                                item6.Color = MTA.Game.Enums.Color.White;
                                item6.Bound = true;
                                item6.Plus = 3;
                                item6.Durability = item6.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid6].Durability;
                                client.Inventory.Add(item6, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item7 = new ConquerItem(true);
                                item7.ID = itemid7;
                                item7.Color = MTA.Game.Enums.Color.White;
                                item7.Bound = true;
                                item7.Plus = 3;
                                item7.Durability = item7.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid7].Durability;
                                client.Inventory.Add(item7, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item8 = new ConquerItem(true);
                                item8.ID = itemid8;
                                item8.Color = MTA.Game.Enums.Color.White;
                                item8.Bound = true;
                                item8.Plus = 3;
                                item8.Durability = item8.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid8].Durability;
                                client.Inventory.Add(item8, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item9 = new ConquerItem(true);
                                item9.ID = itemid9;
                                item9.Color = MTA.Game.Enums.Color.White;
                                item9.Bound = true;
                                item9.Plus = 3;
                                item9.Durability = item9.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid9].Durability;
                                client.Inventory.Add(item9, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item10 = new ConquerItem(true);
                                item10.ID = itemid10;
                                item10.Color = MTA.Game.Enums.Color.White;
                                item10.Bound = true;
                                item10.Plus = 3;
                                //item10.SocketOne = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.SocketTwo = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.Bless = 1;
                                item10.Durability = item10.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid10].Durability;
                                client.Inventory.Add(item10, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item = new ConquerItem(true);
                                item.ID = itemid;
                                item.Color = MTA.Game.Enums.Color.White;
                                item.Bound = true;
                                item.Plus = 3;
                                item.Durability = item.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid].Durability;
                                if (client.Inventory.Add(item, MTA.Game.Enums.ItemUse.CreateAndAdd))
                                {
                                    client.Entity.Teleport(1002, 303, 278);
                                    npc.OptionID = 255;
                                    client.Send(npc.ToArray());
                                }
                                else
                                {
                                    dialog.Text("You need at least one free slot in your inventory.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }

                        }
                        else
                        {
                            dialog.Text("You need to make at least 10 free slots in your inventory.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region BoundItems FireTaoist
                case 145:
                    {
                        uint itemid = 202009;//Tower
                        uint itemid2 = 201009;//Fan
                        uint itemid3 = 160019;//Boot
                        uint itemid4 = 121029;//Bag
                        uint itemid5 = 152019;//Bracelet
                        uint itemid6 = 114009;//Cap
                        uint itemid7 = 134009;//Armor
                        uint itemid8 = 560019;//Spear
                        uint itemid9 = 421009;//BackSword
                        uint itemid10 = 204009;//TempestWing
                        if (client.Inventory.Count <= 30)
                        {
                            //  if (client.Inventory.Contains(720356, 1))
                            {
                                ConquerItem item2 = new ConquerItem(true);
                                item2.ID = itemid2;
                                item2.Color = MTA.Game.Enums.Color.White;
                                item2.Bound = true;
                                item2.Plus = 3;
                                item2.Durability = item2.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid2].Durability;
                                client.Inventory.Add(item2, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item3 = new ConquerItem(true);
                                item3.ID = itemid3;
                                item3.Color = MTA.Game.Enums.Color.White;
                                item3.Bound = true;
                                item3.Plus = 3;
                                item3.Durability = item3.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid3].Durability;
                                client.Inventory.Add(item3, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item4 = new ConquerItem(true);
                                item4.ID = itemid4;
                                item4.Color = MTA.Game.Enums.Color.White;
                                item4.Bound = true;
                                item4.Plus = 3;
                                item4.Durability = item4.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid4].Durability;
                                client.Inventory.Add(item4, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item5 = new ConquerItem(true);
                                item5.ID = itemid5;
                                item5.Color = MTA.Game.Enums.Color.White;
                                item5.Bound = true;
                                item5.Plus = 3;
                                item5.Durability = item5.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid5].Durability;
                                client.Inventory.Add(item5, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item6 = new ConquerItem(true);
                                item6.ID = itemid6;
                                item6.Color = MTA.Game.Enums.Color.White;
                                item6.Bound = true;
                                item6.Plus = 3;
                                item6.Durability = item6.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid6].Durability;
                                client.Inventory.Add(item6, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item7 = new ConquerItem(true);
                                item7.ID = itemid7;
                                item7.Color = MTA.Game.Enums.Color.White;
                                item7.Bound = true;
                                item7.Plus = 3;
                                item7.Durability = item7.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid7].Durability;
                                client.Inventory.Add(item7, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item8 = new ConquerItem(true);
                                item8.ID = itemid8;
                                item8.Color = MTA.Game.Enums.Color.White;
                                item8.Bound = true;
                                item8.Plus = 3;
                                item8.Durability = item8.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid8].Durability;
                                client.Inventory.Add(item8, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item9 = new ConquerItem(true);
                                item9.ID = itemid9;
                                item9.Color = MTA.Game.Enums.Color.White;
                                item9.Bound = true;
                                item9.Plus = 3;
                                item9.Durability = item9.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid9].Durability;
                                client.Inventory.Add(item9, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item10 = new ConquerItem(true);
                                item10.ID = itemid10;
                                item10.Color = MTA.Game.Enums.Color.White;
                                item10.Bound = true;
                                item10.Plus = 3;
                                //item10.SocketOne = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.SocketTwo = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.Bless = 1;
                                item10.Durability = item10.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid10].Durability;
                                client.Inventory.Add(item10, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item = new ConquerItem(true);
                                item.ID = itemid;
                                item.Color = MTA.Game.Enums.Color.White;
                                item.Bound = true;
                                item.Plus = 3;
                                item.Durability = item.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid].Durability;
                                if (client.Inventory.Add(item, MTA.Game.Enums.ItemUse.CreateAndAdd))
                                {
                                    client.Entity.Teleport(1002, 303, 278);
                                    npc.OptionID = 255;
                                    client.Send(npc.ToArray());
                                }
                                else
                                {
                                    dialog.Text("You need at least one free slot in your inventory.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }

                        }
                        else
                        {
                            dialog.Text("You need to make at least 10 free slots in your inventory.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region BoundItems Pirate
                case 75:
                    {
                        uint itemid = 202009;//Tower
                        uint itemid2 = 201009;//Fan
                        uint itemid3 = 160019;//Boot
                        uint itemid4 = 120029;//Necklace
                        uint itemid5 = 150019;//Ring
                        uint itemid6 = 117009;//Earrings
                        uint itemid7 = 139009;//Armor
                        uint itemid8 = 611019;//Rapier
                        uint itemid9 = 612019;//Pistol
                        uint itemid10 = 204009;//TempestWing
                        if (client.Inventory.Count <= 30)
                        {
                            //   if (client.Inventory.Contains(720356, 1))
                            {
                                ConquerItem item2 = new ConquerItem(true);
                                item2.ID = itemid2;
                                item2.Color = MTA.Game.Enums.Color.White;
                                item2.Bound = true;
                                item2.Plus = 3;
                                item2.Durability = item2.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid2].Durability;
                                client.Inventory.Add(item2, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item3 = new ConquerItem(true);
                                item3.ID = itemid3;
                                item3.Color = MTA.Game.Enums.Color.White;
                                item3.Bound = true;
                                item3.Plus = 3;
                                item3.Durability = item3.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid3].Durability;
                                client.Inventory.Add(item3, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item4 = new ConquerItem(true);
                                item4.ID = itemid4;
                                item4.Color = MTA.Game.Enums.Color.White;
                                item4.Bound = true;
                                item4.Plus = 3;
                                item4.Durability = item4.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid4].Durability;
                                client.Inventory.Add(item4, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item5 = new ConquerItem(true);
                                item5.ID = itemid5;
                                item5.Color = MTA.Game.Enums.Color.White;
                                item5.Bound = true;
                                item5.Plus = 3;
                                item5.Durability = item5.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid5].Durability;
                                client.Inventory.Add(item5, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item6 = new ConquerItem(true);
                                item6.ID = itemid6;
                                item6.Color = MTA.Game.Enums.Color.White;
                                item6.Bound = true;
                                item6.Plus = 3;
                                item6.Durability = item6.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid6].Durability;
                                client.Inventory.Add(item6, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item7 = new ConquerItem(true);
                                item7.ID = itemid7;
                                item7.Color = MTA.Game.Enums.Color.White;
                                item7.Bound = true;
                                item7.Plus = 3;
                                item7.Durability = item7.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid7].Durability;
                                client.Inventory.Add(item7, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item8 = new ConquerItem(true);
                                item8.ID = itemid8;
                                item8.Color = MTA.Game.Enums.Color.White;
                                item8.Bound = true;
                                item8.Plus = 3;
                                item8.Durability = item8.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid8].Durability;
                                client.Inventory.Add(item8, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item9 = new ConquerItem(true);
                                item9.ID = itemid9;
                                item9.Color = MTA.Game.Enums.Color.White;
                                item9.Bound = true;
                                item9.Plus = 3;
                                item9.Durability = item9.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid9].Durability;
                                client.Inventory.Add(item9, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item10 = new ConquerItem(true);
                                item10.ID = itemid10;
                                item10.Color = MTA.Game.Enums.Color.White;
                                item10.Bound = true;
                                item10.Plus = 3;
                                //item10.SocketOne = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.SocketTwo = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.Bless = 1;
                                item10.Durability = item10.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid10].Durability;
                                client.Inventory.Add(item10, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item = new ConquerItem(true);
                                item.ID = itemid;
                                item.Color = MTA.Game.Enums.Color.White;
                                item.Bound = true;
                                item.Plus = 3;
                                item.Durability = item.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid].Durability;
                                if (client.Inventory.Add(item, MTA.Game.Enums.ItemUse.CreateAndAdd))
                                {
                                    client.Entity.Teleport(1002, 303, 278);
                                    npc.OptionID = 255;
                                    client.Send(npc.ToArray());
                                }
                                else
                                {
                                    dialog.Text("You need at least one free slot in your inventory.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }
                        }
                        else
                        {
                            dialog.Text("You need to make at least 10 free slots in your inventory.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region BoundItems Dragon-Warrior
                case 85:
                    {
                        uint itemid = 202009;//Tower
                        uint itemid2 = 201009;//Fan
                        uint itemid3 = 160019;//Boot
                        uint itemid4 = 120029;//Necklace
                        uint itemid5 = 150019;//Ring
                        uint itemid6 = 148009;//Head
                        uint itemid7 = 138009;//Armor
                        uint itemid8 = 617009;//Nunchaku
                        uint itemid9 = 617009;//Nunchaku
                        uint itemid10 = 204009;//TempestWing
                        if (client.Inventory.Count <= 30)
                        {
                            //   if (client.Inventory.Contains(720356, 1))
                            {
                                ConquerItem item2 = new ConquerItem(true);
                                item2.ID = itemid2;
                                item2.Color = MTA.Game.Enums.Color.White;
                                item2.Bound = true;
                                item2.Plus = 3;
                                item2.Durability = item2.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid2].Durability;
                                client.Inventory.Add(item2, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item3 = new ConquerItem(true);
                                item3.ID = itemid3;
                                item3.Color = MTA.Game.Enums.Color.White;
                                item3.Bound = true;
                                item3.Plus = 3;
                                item3.Durability = item3.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid3].Durability;
                                client.Inventory.Add(item3, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item4 = new ConquerItem(true);
                                item4.ID = itemid4;
                                item4.Color = MTA.Game.Enums.Color.White;
                                item4.Bound = true;
                                item4.Plus = 3;
                                item4.Durability = item4.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid4].Durability;
                                client.Inventory.Add(item4, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item5 = new ConquerItem(true);
                                item5.ID = itemid5;
                                item5.Color = MTA.Game.Enums.Color.White;
                                item5.Bound = true;
                                item5.Plus = 3;
                                item5.Durability = item5.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid5].Durability;
                                client.Inventory.Add(item5, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item6 = new ConquerItem(true);
                                item6.ID = itemid6;
                                item6.Color = MTA.Game.Enums.Color.White;
                                item6.Bound = true;
                                item6.Plus = 3;
                                item6.Durability = item6.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid6].Durability;
                                client.Inventory.Add(item6, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item7 = new ConquerItem(true);
                                item7.ID = itemid7;
                                item7.Color = MTA.Game.Enums.Color.White;
                                item7.Bound = true;
                                item7.Plus = 3;
                                item7.Durability = item7.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid7].Durability;
                                client.Inventory.Add(item7, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item8 = new ConquerItem(true);
                                item8.ID = itemid8;
                                item8.Color = MTA.Game.Enums.Color.White;
                                item8.Bound = true;
                                item8.Plus = 3;
                                item8.Durability = item8.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid8].Durability;
                                client.Inventory.Add(item8, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item9 = new ConquerItem(true);
                                item9.ID = itemid9;
                                item9.Color = MTA.Game.Enums.Color.White;
                                item9.Bound = true;
                                item9.Plus = 3;
                                item9.Durability = item9.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid9].Durability;
                                client.Inventory.Add(item9, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item10 = new ConquerItem(true);
                                item10.ID = itemid10;
                                item10.Color = MTA.Game.Enums.Color.White;
                                item10.Bound = true;
                                item10.Plus = 3;
                                //item10.SocketOne = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.SocketTwo = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.Bless = 1;
                                item10.Durability = item10.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid10].Durability;
                                client.Inventory.Add(item10, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item = new ConquerItem(true);
                                item.ID = itemid;
                                item.Color = MTA.Game.Enums.Color.White;
                                item.Bound = true;
                                item.Plus = 3;
                                item.Durability = item.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid].Durability;
                                if (client.Inventory.Add(item, MTA.Game.Enums.ItemUse.CreateAndAdd))
                                {
                                    client.Entity.Teleport(1002, 303, 278);
                                    npc.OptionID = 255;
                                    client.Send(npc.ToArray());
                                }
                                else
                                {
                                    dialog.Text("You need at least one free slot in your inventory.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }
                        }
                        else
                        {
                            dialog.Text("You need to make at least 10 free slots in your inventory.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region BoundItems WindWalker
                case 165:
                    {
                        uint itemid = 202009;//Tower
                        uint itemid2 = 201009;//Fan
                        uint itemid3 = 160039;//Boot
                        uint itemid4 = 120029;//Necklace
                        uint itemid5 = 150039;//Ring
                        uint itemid6 = 170009;//Hood
                        uint itemid7 = 101008;//Armor
                        uint itemid8 = 626029;//DragonFan
                        uint itemid9 = 626029;//DragonFan
                        uint itemid10 = 204009;//Wing
                                               //uint itemid11 = 203009;//Crop
                                               //uint itemid15 = 300000;//Steed
                        if (client.Inventory.Count <= 30)
                        {
                            //   if (client.Inventory.Contains(720356, 1))
                            {
                                ConquerItem item2 = new ConquerItem(true);
                                item2.ID = itemid2;
                                item2.Color = MTA.Game.Enums.Color.White;
                                item2.Bound = true;
                                item2.Plus = 3;
                                item2.Durability = item2.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid2].Durability;
                                client.Inventory.Add(item2, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item3 = new ConquerItem(true);
                                item3.ID = itemid3;
                                item3.Color = MTA.Game.Enums.Color.White;
                                item3.Bound = true;
                                item3.Plus = 3;
                                item3.Durability = item3.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid3].Durability;
                                client.Inventory.Add(item3, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item4 = new ConquerItem(true);
                                item4.ID = itemid4;
                                item4.Color = MTA.Game.Enums.Color.White;
                                item4.Bound = true;
                                item4.Plus = 3;
                                item4.Durability = item4.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid4].Durability;
                                client.Inventory.Add(item4, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item5 = new ConquerItem(true);
                                item5.ID = itemid5;
                                item5.Color = MTA.Game.Enums.Color.White;
                                item5.Bound = true;
                                item5.Plus = 3;
                                item5.Durability = item5.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid5].Durability;
                                client.Inventory.Add(item5, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item6 = new ConquerItem(true);
                                item6.ID = itemid6;
                                item6.Color = MTA.Game.Enums.Color.White;
                                item6.Bound = true;
                                item6.Plus = 3;
                                item6.Durability = item6.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid6].Durability;
                                client.Inventory.Add(item6, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item7 = new ConquerItem(true);
                                item7.ID = itemid7;
                                item7.Color = MTA.Game.Enums.Color.White;
                                item7.Bound = true;
                                item7.Plus = 3;
                                item7.Durability = item7.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid7].Durability;
                                client.Inventory.Add(item7, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item8 = new ConquerItem(true);
                                item8.ID = itemid8;
                                item8.Color = MTA.Game.Enums.Color.White;
                                item8.Bound = true;
                                item8.Plus = 3;
                                item8.Durability = item8.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid8].Durability;
                                client.Inventory.Add(item8, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item9 = new ConquerItem(true);
                                item9.ID = itemid9;
                                item9.Color = MTA.Game.Enums.Color.White;
                                item9.Bound = true;
                                item9.Plus = 3;
                                item9.Durability = item9.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid9].Durability;
                                client.Inventory.Add(item9, MTA.Game.Enums.ItemUse.CreateAndAdd);

                                ConquerItem item10 = new ConquerItem(true);
                                item10.ID = itemid10;
                                item10.Color = MTA.Game.Enums.Color.White;
                                item10.Bound = true;
                                item10.Plus = 3;
                                //item10.SocketOne = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.SocketTwo = MTA.Game.Enums.Gem.EmptySocket;
                                //item10.Bless = 1;
                                item10.Durability = item10.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid10].Durability;
                                client.Inventory.Add(item10, MTA.Game.Enums.ItemUse.CreateAndAdd);


                                ConquerItem item = new ConquerItem(true);
                                item.ID = itemid;
                                item.Color = MTA.Game.Enums.Color.White;
                                item.Bound = true;
                                item.Plus = 3;
                                item.Durability = item.MaximDurability = MTA.Database.ConquerItemInformation.BaseInformations[itemid].Durability;
                                if (client.Inventory.Add(item, MTA.Game.Enums.ItemUse.CreateAndAdd))
                                {
                                    client.Entity.Teleport(1002, 303, 278);
                                    npc.OptionID = 255;
                                    client.Send(npc.ToArray());
                                }
                                else
                                {
                                    dialog.Text("You need at least one free slot in your inventory.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }
                        }
                        else
                        {
                            dialog.Text("You need to make at least 10 free slots in your inventory.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                    #endregion
            }
        }
    }
}
