using System.Collections.Generic;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using static MTA.Game.Constants.EntityClass;
using static MTA.Game.Constants.Items.BasicItems;
using static MTA.Game.Constants.Items.Gems;

namespace MTA.Game.Npcs.Handlers.Market {
    /// <summary>
    /// Rebirth Master - Provides Rebirth related services
    /// </summary>
    [NpcHandler(59558)]
    public static class NpcRebirthMaster {
        private const byte WaterSaintRequiredLevel = 110;
        private const byte OtherClassesRequiredLevel = 120;
        private const uint OblivionDewPrice = 1500;

        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        "I have spent my whole life studying the changes of the universe, and I have finally understood the mystery of rebirth. As long as you reach level 90, you can embark on rebirth.");
                    switch (client.Entity.Reborn) {
                        case 0:
                            dialog.Option("1st Rebirth.", 1);
                            break;
                        case 1:
                            dialog.Option("2nd Rebirth.", 2);
                            break;
                        case 2:
                            dialog.Option("Reincarnation.", 3);
                            break;
                    }

                    dialog.Option("Reset my Attribute Points.", 4);
                    dialog.Option("Just passing by.", 255);
                    dialog.Send();
                    break;
                }

                case 1: {
                    if (!IsMaster(client.Entity.Class) || client.Entity.Level <
                        (client.Entity.Class == Water_Saint_5
                            ? WaterSaintRequiredLevel
                            : OtherClassesRequiredLevel)) {
                        dialog.Text(
                            "You cannot be reborn unless you are a master in your class and your level is 110+ for Water Saints or 120+ for other classes.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                        break;
                    }

                    dialog.Text(
                        "There are two kinds of rebirths. One is the normal one and the second one is blessed. The normal rebirth will give you the chance to get a Super Gem, and the blessed rebirth will set a -1 into one piece of equipment that you wear during the rebirth. What do you choose?");
                    dialog.Option("Normal rebirth.", 15);
                    dialog.Option("Blessed rebirth.", 13);
                    dialog.Option("Nothing, thank you.", 255);
                    dialog.Send();
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
                    dialog.Option("Trojan.", 23);
                    dialog.Option("Warrior.", 33);
                    dialog.Option("Archer.", 43);
                    dialog.Option("Water Taoist.", 145);
                    dialog.Option("Fire Taoist.", 155);
                    dialog.Option("Ninja.", 53);
                    dialog.Option("Monk.", 63);
                    dialog.Option("Pirate.", 73);
                    dialog.Option("Dragon Warrior.", 93);
                    dialog.Option("Windwalker.", 173);
                    dialog.Send();
                    break;
                }

                case 203:
                case 213:
                case 223:
                case 233:
                case 243:
                case 253:
                case 254: {
                    client.SelectedGem = npcRequest.OptionID switch {
                        203 => 3,
                        213 => 13,
                        223 => 23,
                        233 => 33,
                        243 => 43,
                        253 => 53,
                        254 => 63,
                        _ => 0
                    };
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

                case 14:
                case 24:
                case 44:
                case 54:
                case 64:
                case 74:
                case 84:
                case 136:
                case 146:
                case 164:
                case 23:
                case 33:
                case 43:
                case 53:
                case 63:
                case 73:
                case 93:
                case 145:
                case 155:
                case 173: {
                    if (!client.Inventory.Contains(CelestialStone, 1)) {
                        dialog.Text("You need a Celestial Stone to perform rebirth.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                        break;
                    }

                    byte classId = npcRequest.OptionID switch {
                        14 => Trojan_1,
                        24 => Warrior_1,
                        44 => Archer_1,
                        54 => Ninja_1,
                        64 => Monk_1,
                        74 => Pirate_1,
                        84 => DragonWarrior_1,
                        136 => Water_1,
                        146 => Fire_1,
                        164 => Windwalker_Guard_1,

                        23 => Trojan_1,
                        33 => Warrior_1,
                        43 => Archer_1,
                        53 => Ninja_1,
                        63 => Monk_1,
                        73 => Pirate_1,
                        93 => DragonWarrior_1,
                        145 => Water_1,
                        155 => Fire_1,
                        173 => Windwalker_Guard_1,
                        _ => 0
                    };

                    var isNormalRebirth =
                        npcRequest.OptionID is 14 or 24 or 44 or 54 or 64 or 74 or 84 or 136 or 146 or 164;

                    if (!client.Reborn(classId)) {
                        dialog.Text("Sorry, but you need at least 2 free slots in your inventory.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                        break;
                    }

                    client.Inventory.Remove(CelestialStone, 1);

                    if (isNormalRebirth) {
                        uint gemId = client.SelectedGem switch {
                            3 => SuperPhoenixGem,
                            13 => SuperDragonGem,
                            23 => SuperFuryGem,
                            33 => SuperRainbowGem,
                            43 => SuperKylinGem,
                            53 => SuperVioletGem,
                            63 => SuperMoonGem,
                            _ => 0
                        };
                        if (gemId != 0) {
                            client.Inventory.Add(gemId, 0, 1);
                        }
                    }
                    else {
                        var blessableSlots = new List<byte>();
                        for (byte slot = 0; slot < 12; slot++) {
                            if (!client.Equipment.Free(slot) && client.Equipment.TryGetItem(slot).Bless == 0) {
                                blessableSlots.Add(slot);
                            }
                        }

                        if (blessableSlots.Count > 0) {
                            var selectedSlot = blessableSlots[Kernel.Random.Next(blessableSlots.Count)];
                            var item = client.Equipment.TryGetItem(selectedSlot);
                            item.Bless = 1;
                            item.Mode = Enums.ItemMode.Update;
                            item.Send(client);
                            ConquerItemTable.UpdateBless(item);
                        }
                    }

                    break;
                }

                case 2: {
                    if (!IsMaster(client.Entity.Class) || client.Entity.Level <
                        (client.Entity.Class == Water_Saint_5
                            ? WaterSaintRequiredLevel
                            : OtherClassesRequiredLevel)) {
                        dialog.Text(
                            "You need to be a master in your class and your level is 110+ for Water Saints or 120+ for other classes.");
                        dialog.Option("I'll just leave.", 255);
                        dialog.Send();
                        break;
                    }

                    if (!client.Inventory.Contains(ExemptionToken, 1)) {
                        dialog.Text("You need an Exemption Token to perform second rebirth.");
                        dialog.Option("I understand.", 255);
                        dialog.Send();
                        break;
                    }

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
                    byte classOption = npcRequest.OptionID switch {
                        11 => Trojan_Master_5,
                        21 => Warrior_King_5,
                        41 => Archer_Master_5,
                        51 => Ninja_Master_5,
                        61 => Monk_Nirvana_5,
                        71 => Pirate_Lord_5,
                        81 => DragonWarrior_King_5,
                        132 => Water_Saint_5,
                        142 => Fire_Saint_5,
                        161 => Windwalker_Lord_5,
                        _ => 0
                    };
                    if (!client.Reborn(classOption)) {
                        dialog.Text("You need two free slots in your inventory.");
                        dialog.Option("I'll just leave.", 255);
                        dialog.Send();
                        break;
                    }

                    client.Inventory.Remove(ExemptionToken, 1);
                    break;
                }

                case 3: {
                    if (client.Entity.Level < 110) {
                        dialog.Text("Sorry, you need to be level 110+.");
                        dialog.Option("All right.", 255);
                        dialog.Send();
                        break;
                    }

                    dialog.Text(
                        "I can help you change your class through, but first you need to have an Oblivion Dew in your inventory.");
                    dialog.Option("Okay, go ahead.", 5);
                    dialog.Option("I want to buy an Oblivion Dew.", 6);
                    dialog.Option("Wait a minute.", 255);
                    dialog.Send();
                    break;
                }

                case 5: {
                    client.Send(new Data(true) {
                        UID = client.Entity.UID,
                        ID = Data.OpenWindow,
                        dwParam = Data.WindowCommands.Reincarnation,
                        wParam1 = client.Entity.X,
                        wParam2 = client.Entity.Y
                    });

                    break;
                }

                case 6: {
                    if (client.Entity.ConquerPoints >= OblivionDewPrice) {
                        dialog.Text($"Do you really want to buy Oblivion Dew? It costs {OblivionDewPrice} CPs.");
                        dialog.Option("Yes.", 7);
                        dialog.Option("Wait a minute.", 255);
                    }
                    else {
                        dialog.Text($"Sorry, you don't have {OblivionDewPrice} CPs.");
                        dialog.Option("All right.", 255);
                    }

                    dialog.Send();
                    break;
                }

                case 7: {
                    if (client.Entity.ConquerPoints >= OblivionDewPrice) {
                        client.Entity.ConquerPoints -= OblivionDewPrice;
                        client.Inventory.Add(OblivionDew, 0, 1);
                        dialog.Text("Here is your Oblivion Dew. You can now change your class.");
                        dialog.Option("Thank you!", 255);
                    }
                    else {
                        dialog.Text($"Sorry, you don't have {OblivionDewPrice} CPs in your bag.");
                        dialog.Option("I understand.", 255);
                    }

                    dialog.Send();
                    break;
                }

                case 4: {
                    dialog.Text(
                        "If you have got the first reborn and you misplaced your attribute points, or you want to set them another way, I'll reset your attribute points for one Dragon Ball. Do you accept?");
                    dialog.Option("Here is the Dragon Ball.", 8);
                    dialog.Option("I'll just leave.", 255);
                    dialog.Send();
                    break;
                }

                case 8: {
                    if (client.Entity is not { Reborn: > 0, Level: >= 70 }) {
                        dialog.Text("You must get the first reborn and be level 70+ to reset your attribute points.");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                        break;
                    }

                    if (!client.Inventory.Contains(DragonBall, 1)) {
                        dialog.Text("You don't have a Dragon Ball in your inventory.");
                        dialog.Option("Sorry, I'll just leave.", 255);
                        dialog.Send();
                        break;
                    }

                    client.Inventory.Remove(DragonBall, 1);
                    client.Entity.Agility = 0;
                    client.Entity.Strength = 0;
                    client.Entity.Vitality = 1;
                    client.Entity.Spirit = 0;
                    if (client.Entity.Reborn == 1) {
                        client.Entity.Atributes =
                            (ushort)(client.ExtraAtributePoints(client.Entity.FirstRebornLevel,
                                client.Entity.FirstRebornLevel) + 52 + 3 * (client.Entity.Level - 15));
                    }
                    else {
                        client.Entity.Atributes =
                            (ushort)(client.ExtraAtributePoints(client.Entity.FirstRebornLevel,
                                         client.Entity.FirstRebornClass) +
                                     client.ExtraAtributePoints(client.Entity.SecondRebornLevel,
                                         client.Entity.SecondRebornClass) + 52 + 3 * (client.Entity.Level - 15));
                    }

                    break;
                }
            }
        }
    }
}