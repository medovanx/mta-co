using System;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// Stuff Seller NPC - Sells full equipment sets and special items
    /// </summary>
    [NpcHandler(225584)]
    public static class Npc_StuffSeller
    {
        private const int MAX_INVENTORY_SLOTS = 40;

        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Hello there, " + client.Entity.Name + "! Would you like to purchase a full equipment set?");
                        dialog.Option("Bruce Lee P7 Set", 1);
                        dialog.Option("Ninja P7 Set", 3);
                        dialog.Option("Monk P7 Set", 4);
                        dialog.Option("Taoist P7 Set", 5);
                        dialog.Option("Trojan P7 Set", 6);
                        dialog.Option("Pirate P7 Set", 7);
                        dialog.Option("Warrior P7 Set", 8);
                        dialog.Option("Archer P7 Set", 9);
                        dialog.Option("Wind Walker P7 Set", 11);
                        dialog.Option("Fan, Tower, Wing, Crop, and Steed Set", 10);
                        dialog.Option("Epic Monk Equipment", 12);
                        dialog.Option("Epic Taoist Equipment", 13);
                        dialog.Option("Epic Warrior Equipment", 14);
                        dialog.Option("Vigorous Heavy Ring", 15);
                        dialog.Send();
                        break;
                    }

                #region Epic Monk Equipment
                case 12:
                    {
                        uint itemId = 622439; // Epic Monk
                        int requiredSlots = 1;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            ConquerItem item = new ConquerItem(true);
                            item.ID = itemId;
                            item.Color = Game.Enums.Color.White;
                            item.Plus = 12;
                            item.Bless = 7;
                            item.Enchant = 240;
                            item.SocketOne = Game.Enums.Gem.SuperDragonGem;
                            item.SocketTwo = Game.Enums.Gem.SuperDragonGem;
                            item.Durability = item.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId].Durability;

                            if (client.Inventory.Add(item, Game.Enums.ItemUse.CreateAndAdd))
                            {
                                Network.GamePackets.NpcReply npc = new Network.GamePackets.NpcReply(6, "You have received the Epic Monk equipment!");
                                npc.OptionID = 255;
                                client.Send(npc.ToArray());
                            }
                            else
                            {
                                dialog.Text("You need at least one free slot in your inventory.");
                                dialog.Option("Understood.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Epic Taoist Equipment
                case 13:
                    {
                        uint itemId1 = 619439; // Universe Hossu
                        uint itemId2 = 620439; // Imperial Backsword
                        int requiredSlots = 2;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            ConquerItem item1 = new ConquerItem(true);
                            item1.ID = itemId1;
                            item1.Color = Game.Enums.Color.White;
                            item1.Plus = 12;
                            item1.Bless = 7;
                            item1.Enchant = 240;
                            item1.SocketOne = Game.Enums.Gem.SuperPhoenixGem;
                            item1.SocketTwo = Game.Enums.Gem.SuperPhoenixGem;
                            item1.Durability = item1.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId1].Durability;

                            ConquerItem item2 = new ConquerItem(true);
                            item2.ID = itemId2;
                            item2.Color = Game.Enums.Color.White;
                            item2.Plus = 12;
                            item2.Bless = 7;
                            item2.Enchant = 240;
                            item2.SocketOne = Game.Enums.Gem.SuperPhoenixGem;
                            item2.SocketTwo = Game.Enums.Gem.SuperPhoenixGem;
                            item2.Durability = item2.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId2].Durability;

                            bool success1 = client.Inventory.Add(item1, Game.Enums.ItemUse.CreateAndAdd);
                            bool success2 = client.Inventory.Add(item2, Game.Enums.ItemUse.CreateAndAdd);

                            if (success1 && success2)
                            {
                                Network.GamePackets.NpcReply npc = new Network.GamePackets.NpcReply(6, "You have received the Epic Taoist equipment!");
                                npc.OptionID = 255;
                                client.Send(npc.ToArray());
                            }
                            else
                            {
                                dialog.Text("You need at least " + requiredSlots + " free slots in your inventory.");
                                dialog.Option("Understood.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Epic Warrior Equipment
                case 14:
                    {
                        uint itemId = 624439; // Epic Warrior
                        int requiredSlots = 1;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            ConquerItem item = new ConquerItem(true);
                            item.ID = itemId;
                            item.Color = Game.Enums.Color.White;
                            item.Plus = 12;
                            item.Bless = 7;
                            item.Enchant = 240;
                            item.SocketOne = Game.Enums.Gem.SuperDragonGem;
                            item.SocketTwo = Game.Enums.Gem.SuperDragonGem;
                            item.Durability = item.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId].Durability;

                            if (client.Inventory.Add(item, Game.Enums.ItemUse.CreateAndAdd))
                            {
                                Network.GamePackets.NpcReply npc = new Network.GamePackets.NpcReply(6, "You have received the Epic Warrior equipment!");
                                npc.OptionID = 255;
                                client.Send(npc.ToArray());
                            }
                            else
                            {
                                dialog.Text("You need at least one free slot in your inventory.");
                                dialog.Option("Understood.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Vigorous Heavy Ring
                case 15:
                    {
                        uint itemId = 151269; // Vigorous Heavy Ring
                        int requiredSlots = 1;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            ConquerItem item = new ConquerItem(true);
                            item.ID = itemId;
                            item.Color = Game.Enums.Color.White;
                            item.Plus = 12;
                            item.Bless = 7;
                            item.Enchant = 240;
                            item.SocketOne = Game.Enums.Gem.SuperDragonGem;
                            item.SocketTwo = Game.Enums.Gem.SuperDragonGem;
                            item.Durability = item.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId].Durability;

                            if (client.Inventory.Add(item, Game.Enums.ItemUse.CreateAndAdd))
                            {
                                Network.GamePackets.NpcReply npc = new Network.GamePackets.NpcReply(6, "You have received the Vigorous Heavy Ring!");
                                npc.OptionID = 255;
                                client.Send(npc.ToArray());
                            }
                            else
                            {
                                dialog.Text("You need at least one free slot in your inventory.");
                                dialog.Option("Understood.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Fan, Tower, Wing, Crop, and Steed Set
                case 10:
                    {
                        uint itemId1 = 300000; // Steed
                        uint itemId2 = 202009; // Tower
                        uint itemId3 = 201009; // Fan
                        uint itemId4 = 204009; // Wing
                        uint itemId5 = 203009; // Crop
                        int requiredSlots = 5;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            ConquerItem item1 = new ConquerItem(true);
                            item1.ID = itemId1;
                            item1.Color = Game.Enums.Color.White;
                            item1.Plus = 12;
                            item1.Durability = item1.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId1].Durability;
                            client.Inventory.Add(item1, Game.Enums.ItemUse.CreateAndAdd);

                            ConquerItem item2 = new ConquerItem(true);
                            item2.ID = itemId2;
                            item2.Color = Game.Enums.Color.White;
                            item2.Plus = 12;
                            item2.Bless = 1;
                            item2.SocketOne = Game.Enums.Gem.SuperGloryGem;
                            item2.SocketTwo = Game.Enums.Gem.SuperGloryGem;
                            item2.Durability = item2.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId2].Durability;
                            client.Inventory.Add(item2, Game.Enums.ItemUse.CreateAndAdd);

                            ConquerItem item3 = new ConquerItem(true);
                            item3.ID = itemId3;
                            item3.Color = Game.Enums.Color.White;
                            item3.Plus = 12;
                            item3.Bless = 1;
                            item3.SocketOne = Game.Enums.Gem.SuperThunderGem;
                            item3.SocketTwo = Game.Enums.Gem.SuperThunderGem;
                            item3.Durability = item3.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId3].Durability;
                            client.Inventory.Add(item3, Game.Enums.ItemUse.CreateAndAdd);

                            ConquerItem item4 = new ConquerItem(true);
                            item4.ID = itemId4;
                            item4.Color = Game.Enums.Color.White;
                            item4.Plus = 12;
                            item4.Bless = 0;
                            item4.SocketOne = Game.Enums.Gem.SuperThunderGem;
                            item4.SocketTwo = Game.Enums.Gem.SuperGloryGem;
                            item4.Durability = item4.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId4].Durability;
                            client.Inventory.Add(item4, Game.Enums.ItemUse.CreateAndAdd);

                            ConquerItem item5 = new ConquerItem(true);
                            item5.ID = itemId5;
                            item5.Color = Game.Enums.Color.White;
                            item5.Plus = 12;
                            item5.Bless = 1;
                            item5.Durability = item5.MaximDurability = Database.ConquerItemInformation.BaseInformations[itemId5].Durability;
                            client.Inventory.Add(item5, Game.Enums.ItemUse.CreateAndAdd);

                            dialog.Text("You have received the Fan, Tower, Wing, Crop, and Steed set!");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Wind Walker P7 Set
                case 11:
                    {
                        int requiredSlots = 7;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            client.Inventory.AddSoul(626439, 801308, 6, 12, 12, 1, true, false); // Dragon Fan
                            client.Inventory.AddSoul(626439, 801308, 6, 12, 12, 1, true, false); // Dragon Fan
                            client.Inventory.AddSoul(101309, 822071, 6, 12, 12, 1, true, false); // Armor
                            client.Inventory.AddSoul(170309, 820073, 6, 12, 12, 1, true, false); // Hat
                            client.Inventory.AddSoul(120269, 821033, 6, 12, 12, 1, true, false); // Necklace
                            client.Inventory.AddSoul(150269, 823058, 6, 12, 12, 1, true, false); // Ring
                            client.Inventory.AddSoul(160249, 824018, 6, 12, 12, 1, true, false); // Boot

                            dialog.Text("Thank you for your purchase! You have received the Wind Walker P7 set with soul-stabilized items.");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Bruce Lee P7 Set
                case 1:
                    {
                        int requiredSlots = 7;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            client.Inventory.AddSoul(617439, 801004, 6, 12, 12, 1, true, false); // Sky Nunchaku
                            client.Inventory.AddSoul(617439, 801004, 6, 12, 12, 1, true, false); // Sky Nunchaku
                            client.Inventory.AddSoul(138309, 822071, 6, 12, 12, 1, true, false); // Bruce Lee Armor
                            client.Inventory.AddSoul(148309, 820073, 6, 12, 12, 1, true, false); // Bruce Lee Hood
                            client.Inventory.AddSoul(120269, 821033, 6, 12, 12, 1, true, false); // Necklace
                            client.Inventory.AddSoul(150269, 823058, 6, 12, 12, 1, true, false); // Ring
                            client.Inventory.AddSoul(160249, 824018, 6, 12, 12, 1, true, false); // Boot

                            dialog.Text("Thank you for your purchase! You have received the Bruce Lee P7 set with soul-stabilized items.");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Ninja P7 Set
                case 3:
                    {
                        int requiredSlots = 13;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            client.Inventory.AddSoul(511439, 800255, 6, 12, 12, 1, true, false); // Scythe
                            client.Inventory.AddSoul(511439, 800255, 6, 12, 12, 1, true, false); // Scythe
                            client.Inventory.AddSoul(616439, 800111, 6, 12, 12, 1, true, false); // Katana
                            client.Inventory.AddSoul(616439, 800111, 6, 12, 12, 1, true, false); // Katana
                            client.Inventory.AddSoul(601439, 800111, 6, 12, 12, 1, true, false); // Katana
                            client.Inventory.AddSoul(601439, 800111, 6, 12, 12, 1, true, false); // Katana
                            client.Inventory.AddSoul(601439, 800142, 6, 12, 12, 1, true, false); // Katana
                            client.Inventory.AddSoul(601439, 800142, 6, 12, 12, 1, true, false); // Katana
                            client.Inventory.AddSoul(135309, 822071, 6, 12, 12, 1, true, false); // Ninja Armor
                            client.Inventory.AddSoul(123309, 820073, 6, 12, 12, 1, true, false); // Ninja Hood
                            client.Inventory.AddSoul(120269, 821033, 6, 12, 12, 1, true, false); // Necklace
                            client.Inventory.AddSoul(150269, 823058, 6, 12, 12, 1, true, false); // Ring
                            client.Inventory.AddSoul(160249, 824018, 6, 12, 12, 1, true, false); // Boot

                            dialog.Text("Thank you for your purchase! You have received the Ninja P7 set with soul-stabilized items.");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Monk P7 Set
                case 4:
                    {
                        int requiredSlots = 9;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            client.Inventory.AddSoul(622439, 800725, 6, 12, 12, 1, true, false); // Beads
                            client.Inventory.AddSoul(622439, 800725, 6, 12, 12, 1, true, false); // Beads
                            client.Inventory.AddSoul(610439, 800725, 6, 12, 12, 1, true, false); // Beads
                            client.Inventory.AddSoul(610439, 800725, 6, 12, 12, 1, true, false); // Beads
                            client.Inventory.AddSoul(136309, 822071, 6, 12, 12, 1, true, false); // Monk Armor
                            client.Inventory.AddSoul(143309, 820073, 6, 12, 12, 1, true, false); // Monk Cap
                            client.Inventory.AddSoul(120269, 821033, 6, 12, 12, 1, true, false); // Necklace
                            client.Inventory.AddSoul(150269, 823058, 6, 12, 12, 1, true, false); // Ring
                            client.Inventory.AddSoul(160249, 824018, 6, 12, 12, 1, true, false); // Boot

                            dialog.Text("Thank you for your purchase! You have received the Monk P7 set with soul-stabilized items.");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Taoist P7 Set
                case 5:
                    {
                        int requiredSlots = 8;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            client.Inventory.AddSoul(421439, 800522, 6, 12, 12, 1, true, false); // Back Sword
                            client.Inventory.AddSoul(620439, 800522, 6, 12, 12, 1, true, false); // Imperial Backsword
                            client.Inventory.AddSoul(619439, 801104, 6, 12, 12, 1, true, false); // Universe Hossu
                            client.Inventory.AddSoul(134309, 822071, 6, 12, 12, 1, true, false); // Fire Armor
                            client.Inventory.AddSoul(114309, 820076, 6, 12, 12, 1, true, false); // Fire Cap
                            client.Inventory.AddSoul(152279, 823060, 6, 12, 12, 1, true, false); // Bracelet
                            client.Inventory.AddSoul(121269, 821034, 6, 12, 12, 1, true, false); // Bag
                            client.Inventory.AddSoul(160249, 824018, 6, 12, 12, 1, true, false); // Boot

                            dialog.Text("Thank you for your purchase! You have received the Taoist P7 set with soul-stabilized items.");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Trojan P7 Set
                case 6:
                    {
                        int requiredSlots = 10;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            client.Inventory.AddSoul(480439, 800111, 6, 12, 12, 1, true, false); // Club
                            client.Inventory.AddSoul(614429, 800111, 6, 12, 12, 1, true, false); // Shadow Cross Saber
                            client.Inventory.AddSoul(614429, 800111, 6, 12, 12, 1, true, false); // Shadow Cross Saber
                            client.Inventory.AddSoul(410439, 800111, 6, 12, 12, 1, true, false); // Blade
                            client.Inventory.AddSoul(420439, 800111, 6, 12, 12, 1, true, false); // Sword
                            client.Inventory.AddSoul(130309, 822071, 6, 12, 12, 1, true, false); // Trojan Armor
                            client.Inventory.AddSoul(118309, 820073, 6, 12, 12, 1, true, false); // Trojan Cap
                            client.Inventory.AddSoul(120269, 821033, 6, 12, 12, 1, true, false); // Necklace
                            client.Inventory.AddSoul(150269, 823058, 6, 12, 12, 1, true, false); // Ring
                            client.Inventory.AddSoul(160249, 824018, 6, 12, 12, 1, true, false); // Boot

                            dialog.Text("Thank you for your purchase! You have received the Trojan P7 set with soul-stabilized items.");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Pirate P7 Set
                case 7:
                    {
                        int requiredSlots = 7;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            client.Inventory.AddSoul(611439, 800811, 6, 12, 12, 1, true, false); // Rapier
                            client.Inventory.AddSoul(612439, 800810, 6, 12, 12, 1, true, false); // Pistol
                            client.Inventory.AddSoul(139309, 822071, 6, 12, 12, 1, true, false); // Pirate Armor
                            client.Inventory.AddSoul(144309, 820073, 6, 12, 12, 1, true, false); // Pirate Cap
                            client.Inventory.AddSoul(120269, 821033, 6, 12, 12, 1, true, false); // Necklace
                            client.Inventory.AddSoul(150269, 823058, 6, 12, 12, 1, true, false); // Ring
                            client.Inventory.AddSoul(160249, 824018, 6, 12, 12, 1, true, false); // Boot

                            dialog.Text("Thank you for your purchase! You have received the Pirate P7 set with soul-stabilized items.");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Warrior P7 Set
                case 8:
                    {
                        int requiredSlots = 7;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            client.Inventory.AddSoul(560439, 800215, 6, 12, 12, 1, true, false); // Spear
                            client.Inventory.AddSoul(900309, 800422, 6, 12, 12, 1, true, false); // Shield
                            client.Inventory.AddSoul(131309, 822071, 6, 12, 12, 1, true, false); // Warrior Armor
                            client.Inventory.AddSoul(111309, 820073, 6, 12, 12, 1, true, false); // Warrior Cap
                            client.Inventory.AddSoul(120269, 821033, 6, 12, 12, 1, true, false); // Necklace
                            client.Inventory.AddSoul(150269, 823058, 6, 12, 12, 1, true, false); // Ring
                            client.Inventory.AddSoul(160249, 824018, 6, 12, 12, 1, true, false); // Boot

                            dialog.Text("Thank you for your purchase! You have received the Warrior P7 set with soul-stabilized items.");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion

                #region Archer P7 Set
                case 9:
                    {
                        int requiredSlots = 8;

                        if (client.Inventory.Count <= MAX_INVENTORY_SLOTS - requiredSlots)
                        {
                            client.Inventory.AddSoul(613429, 800917, 6, 12, 12, 1, true, false); // Knife
                            client.Inventory.AddSoul(613429, 800917, 6, 12, 12, 1, true, false); // Knife
                            client.Inventory.AddSoul(500429, 800618, 6, 12, 12, 1, true, false); // Bow
                            client.Inventory.AddSoul(133309, 822071, 6, 12, 12, 1, true, false); // Archer Armor
                            client.Inventory.AddSoul(113309, 820073, 6, 12, 12, 1, true, false); // Archer Cap
                            client.Inventory.AddSoul(120269, 821033, 6, 12, 12, 1, true, false); // Necklace
                            client.Inventory.AddSoul(150269, 823058, 6, 12, 12, 1, true, false); // Ring
                            client.Inventory.AddSoul(160249, 824018, 6, 12, 12, 1, true, false); // Boot

                            dialog.Text("Thank you for your purchase! You have received the Archer P7 set with soul-stabilized items.");
                            dialog.Option("Thank you!", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need to free up at least " + requiredSlots + " slot(s) in your inventory.");
                            dialog.Option("Understood.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                    #endregion
            }
        }
    }
}
