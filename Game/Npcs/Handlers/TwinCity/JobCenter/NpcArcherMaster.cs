using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Npcs;

namespace MTA.Game.Npcs.Handlers.TwinCity.JobCenter {
    /// <summary>
    /// Archer Master - Provides skills learning for Archer class
    /// </summary>
    [NpcHandler(400)]
    public static class NpcArcherMaster {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        dialog.Text(
                            "I am the bow master. As I see, you started your way on conquering this world. I will try to help you teaching you warrior skills and promoting you.");
                        dialog.Option("Promote me.", 1);
                        dialog.Option("Learn skills.", 2);
                        dialog.Option("Assassin skills.", 49);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("Sorry, you are not the Archer.");
                        dialog.Option("I see.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #region Promotion

                case 1: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.Entity.Class == 45) {
                            dialog.Text("You cannot be promoted anymore. You have mastered your class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                        else {
                            dialog.Text("To promote now you need" + client.PromoteItemNameNeed + " level " +
                                        client.PromoteLevelNeed + ".");
                            dialog.Option("Promote me sir.", 3);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 3: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.Entity.Class == 45) {
                            dialog.Text("You cannot be promoted anymore. You have mastered your class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                        else {
                            if (client.PromoteItemNeed == 721020) {
                                if (client.Inventory.Remove("moonbox")) {
                                    client.Inventory.Add(client.PromoteItemGain, 0, 1);
                                    client.Entity.Class++;
                                    client.Entity.Update(10, "end_task", true);
                                    dialog.Text("Congratulations! You have been promoted.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                                else {
                                    dialog.Text("You don't meet the requirements.");
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
                                dialog.Text("Congratulations! You have been promoted.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else {
                                dialog.Text("You don't meet the requirements.");
                                dialog.Option("Ahh.", 255);
                                dialog.Send();
                            }
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion

                #region Skills

                case 2: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        dialog.Text("Let me know what you want to learn.");
                        dialog.Option("XPFly (Lvl 1).", 5);
                        dialog.Option("Scatter (Lvl 23).", 6);
                        dialog.Option("RapidFire (Lvl 40).", 7);
                        dialog.Option("Fly (Lvl 70).", 8);
                        dialog.Option("Intensify (Lvl 70).", 9);
                        dialog.Option("Franko rain (Lvl 70).", 10);
                        dialog.Option("Advanced Fly (Lvl 100).", 11);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 5: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.Entity.Level >= 1 && client.AddSpell(LearnableSpell(8002))) {
                            dialog.Text("You have learned the XP Skill of this class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(8002));
                            if (!client.AddSpell(LearnableSpell(8002))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 1 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 6: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.Entity.Level >= 23 && client.AddSpell(LearnableSpell(8001))) {
                            dialog.Text("You have learned the scatter.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(8001));
                            if (!client.AddSpell(LearnableSpell(8001))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 23 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 7: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(8000))) {
                            dialog.Text("You have learned the rapid fire.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(8000));
                            if (!client.AddSpell(LearnableSpell(8000))) {
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
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 8: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(8003))) {
                            dialog.Text("You have learned the fly.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(8003));
                            if (!client.AddSpell(LearnableSpell(8003))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 70 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 9: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(9000))) {
                            dialog.Text("You have learned the intensify.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(9000));
                            if (!client.AddSpell(LearnableSpell(9000))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 70 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 10: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(8030))) {
                            dialog.Text("You have learned the Franko rain.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(8030));
                            if (!client.AddSpell(LearnableSpell(8030))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 70 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 11: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.Entity.Level >= 100 && client.AddSpell(LearnableSpell(8003, 1))) {
                            dialog.Text("You have learned the advanced fly.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(8003, 1));
                            if (!client.AddSpell(LearnableSpell(8003, 1))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 100 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion

                #region Assassin skills

                case 49: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        dialog.Text("Let me know what you want to learn.");
                        dialog.Option("KineticSpark.", 12);
                        dialog.Option("DaggerStorm.", 13);
                        dialog.Option("PathOfShadow.", 14);
                        dialog.Option("BladeFlurry.", 15);
                        dialog.Option("BlisteringWave.", 16);
                        dialog.Option("MortalWound.", 17);
                        dialog.Option("SpiritFocus.", 18);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 12: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.AddSpell(LearnableSpell(11590))) {
                            dialog.Text("You have learned the KineticSpark Skill of this class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11590));
                            if (!client.AddSpell(LearnableSpell(11590))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You already know this skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 13: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.AddSpell(LearnableSpell(11600))) {
                            dialog.Text("You have learned the DaggerStorm Skill of this class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11600));
                            if (!client.AddSpell(LearnableSpell(11600))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You already know this skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 14: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.AddSpell(LearnableSpell(11620))) {
                            dialog.Text("You have learned the PathOfShadow Skill of this class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11620));
                            if (!client.AddSpell(LearnableSpell(11620))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You already know this skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 15: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.AddSpell(LearnableSpell(11610))) {
                            dialog.Text("You have learned the BladeFlurry Skill of this class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11610));
                            if (!client.AddSpell(LearnableSpell(11610))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You already know this skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 16: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.AddSpell(LearnableSpell(11650))) {
                            dialog.Text("You have learned the BlisteringWave Skill of this class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11650));
                            if (!client.AddSpell(LearnableSpell(11650))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You already know this skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 17: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.AddSpell(LearnableSpell(11660))) {
                            dialog.Text("You have learned the MortalWound Skill of this class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11660));
                            if (!client.AddSpell(LearnableSpell(11660))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You already know this skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 18: {
                    if (client.Entity.Class >= 40 && client.Entity.Class <= 45) {
                        if (client.AddSpell(LearnableSpell(11670))) {
                            dialog.Text("You have learned the SpiritFocus Skill of this class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11670));
                            if (!client.AddSpell(LearnableSpell(11670))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You already know this skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the archer secrets to another class, so, good bye.");
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