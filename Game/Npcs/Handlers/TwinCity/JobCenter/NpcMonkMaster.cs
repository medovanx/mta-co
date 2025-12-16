using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Npcs;

namespace MTA.Game.Npcs.Handlers.TwinCity.JobCenter {
    /// <summary>
    /// Monk Master - Provides skills learning for Monk class
    /// </summary>
    [NpcHandler(4271)]
    public static class NpcMonkMaster {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        dialog.Text(
                            "I am the coach monk, master of balance and harmony destruction What do you want, young monk.?");
                        dialog.Option("Promote me, master.", 1);
                        dialog.Option("Learn skills.", 2);
                        dialog.Option("Epic skills.", 33);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text(
                            "Sorry, but I can only teach the monks of these lessons, we can not share our traditional knowledge to those who do not share the faith..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #region Promotion

                case 1: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Class == 65) {
                            dialog.Text("It can not be promoted more You have dominated this class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                        else {
                            dialog.Text("Promoting now you need" + client.PromoteItemNameNeed + "level" +
                                        client.PromoteLevelNeed + ".");
                            dialog.Option("promote me sir.", 3);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 3: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Class == 65) {
                            dialog.Text("It can not be promoted more You have dominated his class..");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                        else {
                            if (client.PromoteItemNeed == 721020) {
                                if (client.Inventory.Remove("moonbox")) {
                                    client.Inventory.Add(client.PromoteItemGain, 0, 1);
                                    client.Entity.Class++;
                                    client.Entity.Update(10, "end_task", true);
                                    dialog.Text("Congratulations You've been promoted!.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }

                                else {
                                    dialog.Text("You do not meet the requirements.");
                                    dialog.Option("Ahh.", 255);
                                    dialog.Send();
                                }

                                return;
                            }

                            if (client.Inventory.Contains(client.PromoteItemNeed, client.PromoteItemCountNeed) &&
                                client.Entity.Level >= client.PromoteLevelNeed) {
                                client.Inventory.Remove(client.PromoteItemNeed, client.PromoteItemCountNeed);
                                client.Inventory.Add(client.PromoteItemGain, 0, 1);
                                client.Entity.Class++;
                                client.Entity.Update(10, "end_task", true);
                                dialog.Text("Congratulations You've been promoted!.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else {
                                dialog.Text("You do not meet the requirements.");
                                dialog.Option("Ahh.", 255);
                                dialog.Send();
                            }
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion

                #region Skill

                case 2: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        dialog.Text("Let me know what you want to learn.");
                        dialog.Option("Triple Attack (Lvl 5).", 6);
                        dialog.Option("Oblivion (XP) (Lvl 15)", 7);
                        dialog.Option("Whirlwind Kick (Lvl 15)", 8);
                        dialog.Option("Radiant Palm (Lvl 40)", 9);
                        dialog.Option("Serenity (Lvl 40)", 10);
                        dialog.Option("Tranquility (Lvl 70)", 11);
                        dialog.Option("Compassion (Lvl 100)", 12);
                        dialog.Option("Auras (Lvl 20->100)", 5);
                        dialog.Send();
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 5: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        dialog.Text("What aura want to learn?");
                        dialog.Option("Tyrant (Lvl 20).", 13);
                        dialog.Option("Fend (Lvl 20)", 14);
                        dialog.Option("Metal (Lvl 100)", 15);
                        dialog.Option("Wood (Lvl 100)", 16);
                        dialog.Option("Water (Lvl 100)", 17);
                        dialog.Option("Fire (Lvl 100)", 18);
                        dialog.Option("Earth (Lvl 100)", 19);
                        dialog.Option("Regresar a Skill", 2);
                        dialog.Send();
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 6: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 5 && client.AddSpell(LearnableSpell(10490))) {
                            dialog.Text("You have learned Triple Attack.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10490));
                            if (!client.AddSpell(LearnableSpell(10490))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 5 or more.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 7: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 15 && client.AddSpell(LearnableSpell(10390))) {
                            dialog.Text("You have learned Oblivion XP skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10390));
                            if (!client.AddSpell(LearnableSpell(10390))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 15 or higher.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 8: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 15 && client.AddSpell(LearnableSpell(10415))) {
                            dialog.Text("You have learned Whirlwind Kick.");
                            dialog.Option("Thank you master.", 255);
                            client.AddSpell(LearnableSpell(10415));
                            dialog.Send();
                            if (!client.AddSpell(LearnableSpell(10415))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 15 or higher.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 9: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(10381))) {
                            dialog.Text("You have learned Radiant Palm.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10381));
                            if (!client.AddSpell(LearnableSpell(10381))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 40 or higher.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 10: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(10400))) {
                            dialog.Text("You have learned Serenity.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10400));
                            if (!client.AddSpell(LearnableSpell(10400))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 40 or higher.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 11: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(10425))) {
                            dialog.Text("You have learned Tranquility.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10425));
                            if (!client.AddSpell(LearnableSpell(10425))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 70 or higher.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 12: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 100 && client.AddSpell(LearnableSpell(10430))) {
                            dialog.Text("You have learned Compassion.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10430));
                            if (!client.AddSpell(LearnableSpell(10430))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 100 or more.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 13: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 20 && client.AddSpell(LearnableSpell(10395))) {
                            dialog.Text("You have learned Tyrant Aura.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10395));
                            if (!client.AddSpell(LearnableSpell(10395))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 20 or more.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 14: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 20 && client.AddSpell(LearnableSpell(10410))) {
                            dialog.Text("You have learned Fend Aura.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10410));
                            if (!client.AddSpell(LearnableSpell(10410))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 20 or more.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 15: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 100 && client.AddSpell(LearnableSpell(10420))) {
                            dialog.Text("You have learned Metal Elemental Aura.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10420));
                            if (!client.AddSpell(LearnableSpell(10420))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 100 or more.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 16: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 100 && client.AddSpell(LearnableSpell(10421))) {
                            dialog.Text("You have learned Wood Elemental Aura.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10421));
                            if (!client.AddSpell(LearnableSpell(10421))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 100 or more.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 17: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 100 && client.AddSpell(LearnableSpell(10422))) {
                            dialog.Text("You have learned Water Elemental Aura.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10422));
                            if (!client.AddSpell(LearnableSpell(10422))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 100 or more.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 18: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 100 && client.AddSpell(LearnableSpell(10423))) {
                            dialog.Text("You have learned Fire Elemental Aura.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10423));
                            if (!client.AddSpell(LearnableSpell(10423))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 100 or more.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 19: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 100 && client.AddSpell(LearnableSpell(10424))) {
                            dialog.Text("You have learned Earth Elemental Aura.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(10424));
                            if (!client.AddSpell(LearnableSpell(10424))) {
                                dialog.Text("You know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 100 or more.");
                            dialog.Option("Okay.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            ". The ancient secrets of the monk is not to trade If you want to learn the secrets of the monk back in another life goodbye..");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion

                #region Epic

                case 33: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        dialog.Text("Let me know what you want to Epic.");
                        dialog.Option("InfernalEcho.", 34);
                        dialog.Option("GraceofHeaven", 35);
                        dialog.Option("WrathofiheEmperor", 36);
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text(
                            "The ancient secrets of the monk is not for trade.\nIf you wish to learn the secrets of the monk come back in another life. Good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 34: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 40) {
                            dialog.Text("You have learned InfernalEcho.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            if (!client.AddSpell(LearnableSpell(12550))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 40 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            "The ancient secrets of the monk is not for trade.\nIf you wish to learn the secrets of the monk come back in another life. Good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 35: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 50) {
                            dialog.Text("You have learned GraceofHeaven.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            if (!client.AddSpell(LearnableSpell(12560))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 50 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            "The ancient secrets of the monk is not for trade.\nIf you wish to learn the secrets of the monk come back in another life. Good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 36: {
                    if (client.Entity.Class >= 60 && client.Entity.Class <= 65) {
                        if (client.Entity.Level >= 40) {
                            dialog.Text("You have learned WrathofiheEmperor.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            if (!client.AddSpell(LearnableSpell(12570))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 40 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            "The ancient secrets of the monk is not for trade.\nIf you wish to learn the secrets of the monk come back in another life. Good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion }
            }
        }
    }
}