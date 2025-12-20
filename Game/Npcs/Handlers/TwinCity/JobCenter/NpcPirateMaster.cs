using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Npcs;

namespace MTA.Game.Npcs.Handlers.TwinCity.JobCenter {
    /// <summary>
    /// Pirate Master - Provides skills learning for Pirate class
    /// </summary>
    [NpcHandler(4272)]
    public static class NpcPirateMaster {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        dialog.Text(
                            "I am the master of the Pirate skills. The Pirate skills, are not skills that would make wounds but heal them. As I see, you started your way on conquering this world. I will try to help you teaching you warrior skills and promoting you.");
                        dialog.Option("Promote me.", 1);
                        dialog.Option("Learn skills.", 2);
                        dialog.Option("Learn Pure Skill", 80);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("Sorry, but you are not a Pirate.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #region Promotion

                case 1: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Class == 75) {
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
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 3: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Class == 75) {
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
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion

                #region Skills

                case 2: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        dialog.Text("Let me know what you want to learn.");
                        dialog.Option("Windstorm. (Lvl 15).", 15);
                        dialog.Option("CannonBarrage (Lvl 15).", 7);
                        dialog.Option("Eagle Eye (Lvl 15).", 5);
                        dialog.Option("GaleBomb. (Lvl 25).", 9);
                        dialog.Option("Adrenaline Rush. (Lvl 40).", 8);
                        dialog.Option("Blackbeard'sRage. (Lvl 40).", 19);
                        dialog.Option("Next.", 20);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 20: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        dialog.Text("Let me know what you want to learn.");
                        dialog.Option("BlackSpot. (Lvl 40).", 11);
                        dialog.Option("BladeTempest. (Lvl 40).", 21);
                        dialog.Option("Kraken'sRevenge. (Lvl 70).", 10);
                        dialog.Option("Back.", 2);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 5: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Level >= 15 && client.AddSpell(LearnableSpell(11030))) {
                            dialog.Text("You have learned Eagle Eye skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11030));
                            if (!client.AddSpell(LearnableSpell(11030))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 15 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 7: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Level >= 15 && client.AddSpell(LearnableSpell(11050))) {
                            dialog.Text("You have learned the CannonBarrage skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11050));
                            if (!client.AddSpell(LearnableSpell(11050))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 15 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 15: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Level >= 15 && client.AddSpell(LearnableSpell(11140))) {
                            dialog.Text("You have learned the WindStorm skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11140));
                            if (!client.AddSpell(LearnableSpell(11140))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 15 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 9: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Level >= 25 && client.AddSpell(LearnableSpell(11070))) {
                            dialog.Text("You have learned the GaleBomb skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11070));
                            if (!client.AddSpell(LearnableSpell(11070))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("You need to be level 25 or more.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 8: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(11130))) {
                            dialog.Text("You have learned the AdrenalineRush skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11130));
                            if (!client.AddSpell(LearnableSpell(11130))) {
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
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 19: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(11060))) {
                            dialog.Text("You have learned Blackbeard'sRage skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11060));
                            if (!client.AddSpell(LearnableSpell(11060))) {
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
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 11: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(11120))) {
                            dialog.Text("You have learned the BlackSpot skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11120));
                            if (!client.AddSpell(LearnableSpell(11120))) {
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
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 21: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(11110))) {
                            dialog.Text("You have learned BladeTempest skill.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(11110));
                            if (!client.AddSpell(LearnableSpell(11110))) {
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
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 10: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.Level >= 70) {
                            if (client.Entity is { Reborn: >= 1, FirstRebornClass: 75 }) {
                                if (client.AddSpell(LearnableSpell(11100))) {
                                    dialog.Text("You have learned the Kraken'sRevenge skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    client.AddSpell(LearnableSpell(11100));
                                    if (!client.AddSpell(LearnableSpell(11100))) {
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
                                dialog.Text("You are not reborn or first reborn not Pirate.");
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
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion

                #region Pure Skill

                case 80: {
                    if (client.Entity.Class is >= 70 and <= 75) {
                        if (client.Entity.FirstRebornClass is >= 72 and <= 75 &&
                            client.Entity.SecondRebornClass is >= 72 and <= 75) {
                            if (client.AddSpell(LearnableSpell(11040))) {
                                dialog.Text("You have learned ScurvyBomb.");
                                dialog.Option("Thank you.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(11040));
                                if (!client.AddSpell(LearnableSpell(11040))) {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you.", 255);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "You are not allowed, I think you're not promoted yet or you're not pure Walter.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the pirate secrets to another class, so, good bye.");
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