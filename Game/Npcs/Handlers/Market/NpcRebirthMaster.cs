using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.Market {
    /// <summary>
    /// Rebirth Master - Provides Rebirth related services
    /// </summary>
    [NpcHandler(59558)]
    public static class NpcRebirthMaster {
        private const byte MasterClassType = 5; // Classes ending in 5 are master classes
        private const byte WaterSaintClassId = 135;
        private const byte WaterSaintRequiredLevel = 110;
        private const byte OtherClassesRequiredLevel = 120;

        // Item IDs
        private const uint CelestialStoneId = 721259;
        private const uint OblivionDewId = 711083;
        private const uint OblivionDewPrice = 1500;
        private const uint DragonBallId = 1088000;
        private const uint ExemptionTokenId = 723701;
        private const uint SuperGemBaseId = 700000;

        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        "I have spent my whole life studying the changes of the universe, and I have finally understood the mystery of rebirth. As long as you reach level 90, you can embark on rebirth.");
                    dialog.Option("1st Rebirth.", 1);
                    dialog.Option("2nd Rebirth.", 2);
                    dialog.Option("Reincarnation.", 3);
                    dialog.Option("Reset my Attribute Points.", 4);
                    dialog.Option("Just passing by.", 255);
                    dialog.Avatar(51);
                    dialog.Send();
                    break;
                }

                #region 1st Rebirth

                case 1: {
                    if (client.Entity.Reborn == 0) {
                        if (client.Entity.Class % 10 == MasterClassType &&
                            client.Entity.Level >= (client.Entity.Class == WaterSaintClassId
                                ? WaterSaintRequiredLevel
                                : OtherClassesRequiredLevel)) {
                            dialog.Text(
                                "There are two kinds of rebirths. One is the normal one and the second one is blessed. The normal rebirth will give you the chance to get a Super Gem, and the blessed rebirth will set a -1 into one piece of equipment that you wear during the rebirth. What do you choose?");
                            dialog.Option("Normal rebirth.", 15);
                            dialog.Option("Blessed rebirth.", 13);
                            dialog.Option("Nothing, thank you.", 255);
                            dialog.Send();
                        }
                        else {
                            dialog.Text(
                                "You cannot be reborn unless you are a master in your class and your level is 110+ for Water Saints or 120+ for other classes.");
                            dialog.Option("I understand.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("You already got the first rebirth.");
                        dialog.Option("Thank you.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 15: {
                    dialog.Text("Select the super gem you desire.");
                    dialog.Option("Phoenix Gem [Super].", 203);
                    dialog.Option("Dragon Gem [Super].", 213);
                    dialog.Option("Fury Gem [Super].", 223);
                    dialog.Option("Rainbow Gem [Super].", 233);
                    dialog.Option("Violet Gem [Super].", 253);
                    dialog.Option("Moon Gem [Super].", 254);
                    dialog.Option("Kylin Gem [Super].", 243);
                    dialog.Option("Nothing, thank you.", 255);
                    dialog.Send();
                    break;
                }
                case 13: {
                    dialog.Text("Select the class you want to be reborn as.");
                    dialog.Option("Trojan.", (byte)(10 + npcRequest.OptionID));
                    dialog.Option("Warrior.", (byte)(20 + npcRequest.OptionID));
                    dialog.Option("Archer.", (byte)(40 + npcRequest.OptionID));
                    dialog.Option("Water Taoist.", (byte)(132 + npcRequest.OptionID));
                    dialog.Option("Fire Taoist.", (byte)(142 + npcRequest.OptionID));
                    dialog.Option("Ninja.", (byte)(50 + npcRequest.OptionID));
                    dialog.Option("Monk.", (byte)(60 + npcRequest.OptionID));
                    dialog.Option("Pirate.", (byte)(70 + npcRequest.OptionID));
                    dialog.Option("Dragon Warrior.", (byte)(80 + npcRequest.OptionID));
                    dialog.Option("Windwalker.", (byte)(160 + npcRequest.OptionID));
                    dialog.Send();
                    break;
                }

                // Class selection for first rebirth (normal and blessed)
                case 14: // Trojan (normal rebirth gem selection)
                case 24: // Warrior (normal rebirth gem selection)
                case 44: // Archer (normal rebirth gem selection)
                case 54: // Ninja (normal rebirth gem selection)
                case 64: // Monk (normal rebirth gem selection)
                case 74: // Pirate (normal rebirth gem selection)
                case 84: // Dragon-Warrior (normal rebirth gem selection)
                case 136: // Water Taoist (normal rebirth gem selection)
                case 146: // Fire Taoist (normal rebirth gem selection)
                case 164: // Windwalker (normal rebirth gem selection)
                case 23: // Trojan (blessed rebirth)
                case 33: // Warrior (blessed rebirth)
                case 43: // Archer (blessed rebirth)
                case 53: // Ninja (blessed rebirth)
                case 63: // Monk (blessed rebirth)
                case 73: // Pirate (blessed rebirth)
                case 93: // Dragon Warrior (blessed rebirth)
                case 145: // Water Taoist (blessed rebirth)
                case 155: // Fire Taoist (blessed rebirth)
                case 173: // Windwalker (blessed rebirth)
                {
                    if (client.Entity.Reborn == 0) {
                        if (client.Entity.Class % 10 == MasterClassType &&
                            client.Entity.Level >= (client.Entity.Class == WaterSaintClassId
                                ? WaterSaintRequiredLevel
                                : OtherClassesRequiredLevel)) {
                            if (client.Inventory.Contains(CelestialStoneId, 1)) {
                                byte _class = (byte)(npcRequest.OptionID - npcRequest.OptionID % 10);
                                if (_class > 100)
                                    _class += 2;
                                byte type = (byte)(npcRequest.OptionID - _class);
                                if (_class < 100)
                                    _class++;
                                if (type != 4) {
                                    _class -= 10;
                                }

                                if (client.Reborn(_class)) {
                                    client.Inventory.Remove(CelestialStoneId, 1);
                                    if (type == 4) {
                                        if (client.SelectedGem != 0) {
                                            uint gemId = (uint)(client.SelectedGem + SuperGemBaseId);
                                            client.Inventory.Add(gemId, 0, 1);
                                        }
                                    }
                                    else {
                                        int availableshots = 0;
                                        for (byte count = 0; count < 12; count++)
                                            if (!client.Equipment.Free(count))
                                                if (client.Equipment.TryGetItem(count).Bless == 0)
                                                    availableshots++;
                                        if (availableshots != 0) {
                                            byte ex = (byte)Kernel.Random.Next(12);
                                            if (!client.Equipment.Free(ex))
                                                if (client.Equipment.TryGetItem(ex).Bless == 0) {
                                                    var item = client.Equipment.TryGetItem(ex);
                                                    item.Bless = 1;
                                                    item.Mode = Enums.ItemMode.Update;
                                                    item.Send(client);
                                                    ConquerItemTable.UpdateBless(item);
                                                }
                                        }
                                    }
                                }
                                else {
                                    dialog.Text("Sorry, but you need at least 2 free slots in your inventory.");
                                    dialog.Option("I understand.", 255);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text("You need a Celestial Stone to perform rebirth.");
                                dialog.Option("I understand.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "You cannot be reborn if your level is not 110+ for water saints and 120+ for other masters.");
                            dialog.Option("I understand.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            "You cannot be reborn again here. Alex, an elder who lives in Ape Canyon, will tell you about the third life.");
                        dialog.Option("Thank you.", 255);
                        dialog.Send();
                    }

                    break;
                }

                // Gem selection (200-254) - shows class selection dialog
                case 203: // Phoenix Gem [Super]
                case 213: // Dragon Gem [Super]
                case 223: // Fury Gem [Super]
                case 233: // Rainbow Gem [Super]
                case 243: // Kylin Gem [Super]
                case 253: // Violet Gem [Super]
                case 254: // Moon Gem [Super]
                {
                    // Only reachable from case 15, which is only reachable from case 1 (already validated)
                    client.SelectedGem = (byte)(npcRequest.OptionID % 100);
                    if (client.SelectedGem == 54) {
                        client.SelectedGem = 63;
                    }

                    dialog.Text("Select the class you want to be reborn as.");
                    dialog.Option("Trojan.", 14);
                    dialog.Option("Warrior.", 24);
                    dialog.Option("Archer.", 44);
                    dialog.Option("Water Taoist.", 136);
                    dialog.Option("Fire Taoist.", 146);
                    dialog.Option("Ninja.", 54);
                    dialog.Option("Monk.", 64);
                    dialog.Option("Pirate.", 74);
                    dialog.Option("Dragon Warrior.", 84);
                    dialog.Option("Windwalker.", 164);
                    dialog.Send();
                    break;
                }

                #endregion

                #region 2nd Rebirth

                case 2: {
                    if (client.Entity.Reborn == 1) {
                        if (client.Entity.Class % 10 == MasterClassType && client.Entity.Level >=
                            (client.Entity.Class == WaterSaintClassId
                                ? WaterSaintRequiredLevel
                                : OtherClassesRequiredLevel)) {
                            if (client.Inventory.Contains(ExemptionTokenId, 1)) {
                                dialog.Text("Select the class you want to be reborn as.");
                                dialog.Option("Trojan.", 11);
                                dialog.Option("Warrior.", 21);
                                dialog.Option("Archer.", 41);
                                dialog.Option("Water Taoist.", 132);
                                dialog.Option("Fire Taoist.", 142);
                                dialog.Option("Ninja.", 51);
                                dialog.Option("Monk.", 61);
                                dialog.Option("Pirate.", 71);
                                dialog.Option("Dragon Warrior.", 81);
                                dialog.Option("Windwalker.", 161);
                                dialog.Send();
                            }
                            else {
                                dialog.Text("You need an Exemption Token to perform second rebirth.");
                                dialog.Option("I understand.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "You need to be a master in your class and your level is 110+ for Water Saints or 120+ for other classes.");
                            dialog.Option("I'll just leave.", 255);
                            dialog.Send();
                            break;
                        }
                    }
                    else {
                        dialog.Text("You need to get the first rebirth to be able to get the second rebirth");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 11:
                case 21:
                case 41:
                case 51:
                case 61:
                case 71:
                case 81:
                case 132:
                case 142:
                case 161: {
                    if (npcRequest.OptionID == 255)
                        return;
                    if (client.Entity.Reborn == 1) {
                        if (client.Entity.Class % 10 == MasterClassType &&
                            client.Entity.Level >= (client.Entity.Class == WaterSaintClassId
                                ? WaterSaintRequiredLevel
                                : OtherClassesRequiredLevel)) {
                            if (client.Inventory.Contains(ExemptionTokenId, 1)) {
                                // Calculate the actual class ID from the option ID (same logic as first rebirth)
                                byte _class = (byte)(npcRequest.OptionID - npcRequest.OptionID % 10);
                                if (_class > 100)
                                    _class += 2;
                                if (_class < 100)
                                    _class++;

                                if (client.Reborn(_class)) {
                                    client.Inventory.Remove(ExemptionTokenId, 1);
                                }
                                else {
                                    dialog.Text("You need two free slots in your inventory.");
                                    dialog.Option("I'll just leave.", 255);
                                    dialog.Send();
                                    break;
                                }
                            }
                            else {
                                dialog.Text("You need an Exemption Token to perform second rebirth.");
                                dialog.Option("I understand.", 255);
                                dialog.Send();
                                break;
                            }
                        }
                        else {
                            dialog.Text("If you are a water saint, you need level 110+. Otherwise, you need 120+.");
                            dialog.Option("I'll just leave.", 255);
                            dialog.Send();
                            break;
                        }
                    }
                    else {
                        dialog.Text("You need to be in the second life to be able to get the third life.");
                        dialog.Option("I'll just leave.", 255);
                        dialog.Send();
                        break;
                    }

                    break;
                }

                #endregion

                #region Reincarnation

                case 3: {
                    if (client.Entity.Reborn == 2 && client.Entity.Level >= 120) {
                        dialog.Text(
                            "I can help you change your class through, but first you need to have an Oblivion Dew in your inventory.");
                        dialog.Option("Here is the Oblivion Dew.", 5);
                        dialog.Option("I want to buy an Oblivion Dew.", 6);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("Sorry, you need to be second reborn and level 120+.");
                        dialog.Option("All right.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 5: {
                    if (client.Inventory.Contains(OblivionDewId, 1)) {
                        client.Send(new Data(true) {
                            UID = client.Entity.UID,
                            ID = Data.OpenWindow,
                            dwParam = Data.WindowCommands.Reincarnation,
                            wParam1 = client.Entity.X,
                            wParam2 = client.Entity.Y
                        });
                        client.Inventory.Remove(OblivionDewId, 1);
                    }
                    else {
                        dialog.Text("Sorry, you don't have an Oblivion Dew in your inventory.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 6: {
                    if (client.Entity.ConquerPoints >= OblivionDewPrice) {
                        dialog.Text($"Do you really want to buy Oblivion Dew? It costs {OblivionDewPrice} CPs.");
                        dialog.Option("Yes.", 7);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text($"Sorry, you don't have {OblivionDewPrice} CPs.");
                        dialog.Option("All right.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 7: {
                    if (client.Entity.ConquerPoints >= OblivionDewPrice) {
                        client.Entity.ConquerPoints -= OblivionDewPrice;
                        client.Inventory.Add(OblivionDewId, 0, 1);
                        dialog.Text("Here is your Oblivion Dew. You can now change your class.");
                        dialog.Option("Thank you!", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text($"Sorry, you don't have {OblivionDewPrice} CPs in your bag.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion

                #region Attribute Reset

                case 4: {
                    dialog.Text(
                        "If you have got the first reborn and you misplaced your attribute points, or you want to set them another way, I'll reset your attribute points for one Dragon Ball. Do you accept?");
                    dialog.Option("Here is the Dragon Ball.", 8);
                    dialog.Option("I'll just leave.", 255);
                    dialog.Send();
                    break;
                }
                case 8: {
                    if (client.Entity.Reborn > 0 && client.Entity.Level >= 70) {
                        if (client.Inventory.Contains(DragonBallId, 1)) {
                            client.Inventory.Remove(DragonBallId, 1);
                            client.Entity.Agility = 0;
                            client.Entity.Strength = 0;
                            client.Entity.Vitality = 1;
                            client.Entity.Spirit = 0;
                            if (client.Entity.Reborn == 1) {
                                client.Entity.Atributes = (ushort)
                                    (client.ExtraAtributePoints(client.Entity.FirstRebornLevel,
                                         client.Entity.FirstRebornLevel)
                                     + 52 + 3 * (client.Entity.Level - 15));
                            }
                            else {
                                client.Entity.Atributes =
                                    (ushort)
                                    (client.ExtraAtributePoints(client.Entity.FirstRebornLevel,
                                         client.Entity.FirstRebornClass) +
                                     client.ExtraAtributePoints(client.Entity.SecondRebornLevel,
                                         client.Entity.SecondRebornClass) + 52 +
                                     3 * (client.Entity.Level - 15));
                            }
                        }
                        else {
                            dialog.Text("You don't have a Dragon Ball in your inventory.");
                            dialog.Option("Sorry, I'll just leave.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("You must get the first reborn and be level 70+ to reset your attribute points.");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion
            }
        }
    }
}