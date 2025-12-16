using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Npcs;

namespace MTA.Game.Npcs.Handlers.TwinCity.JobCenter {
    /// <summary>
    /// Dragon Warrior Master - Provides skills learning for Dragon-Warrior class
    /// </summary>
    [NpcHandler(17126)]
    public static class NpcDragonWarriorMaster {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        dialog.Text(
                            "Dragon-Warrior is a kung fu star who fights fast, accurately and ruthlessly, as well as a group training martial arts on the basis");
                        dialog.Text(
                            "~of practicality. The Dragon Warrior masters martial arts of various schools, and also excels in wielding many kinds of weapons, such as nunchaku.");
                        dialog.Option("I want to get promoted.", 1);
                        dialog.Option("Class skills.", 2);
                        dialog.Option("Pure skill.", 95);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("Sorry, you're not Dragon-Warrior. Go find your origin.");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #region Promotion

                case 1: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Class == 85) {
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
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 3: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Class == 85) {
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
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion

                #region Skills

                case 2: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        dialog.Text(
                            "Dragon-Warrior enjoys unique and brilliant combo skills. Typically, the striking [Dragon Kicks], destructive [Dragon Swipe]");
                        dialog.Text(
                            "~and overwhelming [Dragon Strides] enable the caster to get close to enemies at a lightning speed");
                        dialog.Text("~and kill the target in a second");
                        dialog.Option("Dragon Punch", 5);
                        dialog.Option("Dragon Cyclone (lv.40).", 6);
                        dialog.Option("Dragon Strides(lv.15).", 7);
                        dialog.Option("Dragon Flow(lv.15).", 8);
                        dialog.Option("Speed Kick(lv.40).", 9);
                        dialog.Option("Cracking Swipe(lv.70).", 10);
                        dialog.Option("Next.", 11);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 11: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        dialog.Text("Let me know what you want to learn.");
                        dialog.Option("Splitting Swipe(lv.100).", 12);
                        dialog.Option("Dragon Slash(lv.100).", 13);
                        dialog.Option("Dragon Roar(lv.70).", 14);
                        dialog.Option("Dragon Swing(lv.70).", 15);
                        dialog.Option("Previous.", 2);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 5: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Level >= 40) {
                            if (client.AddSpell(LearnableSpell(12240)) && client.AddSpell(LearnableSpell(12220)) &&
                                client.AddSpell(LearnableSpell(12210))) {
                                dialog.Text("You have learned the Dragon Punch.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12240));
                                client.AddSpell(LearnableSpell(12220));
                                client.AddSpell(LearnableSpell(12210));
                                if (!client.AddSpell(LearnableSpell(12240)) ||
                                    !client.AddSpell(LearnableSpell(12220)) ||
                                    !client.AddSpell(LearnableSpell(12210))) {
                                    dialog.Text(
                                        "This passive skill nicely reveals the essence of Dragon-Warrior kung fu which takes the fist strength");
                                    dialog.Text("~and deals considerable range damage on enemies in front.");
                                    dialog.Option("It's amazing!", 255);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "This passive skill nicely reveals the essence of Dragon-Warrior kung fu which takes the fist strength");
                                dialog.Text("~and deals considerable range damage on enemies in front.");
                                dialog.Option("It's amazing!", 255);
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
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 6: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(12290))) {
                            dialog.Text("You have learned the Dragon Cyclone.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(12290));
                            if (!client.AddSpell(LearnableSpell(12290))) {
                                dialog.Text("Stirs up the potency in body for an instant, and subdues the enemies");
                                dialog.Text("~without giving them time to response.");
                                dialog.Option("Great!", 255);
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
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 7: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Level >= 15) {
                            if (client.AddSpell(LearnableSpell(12320)) && client.AddSpell(LearnableSpell(12330)) &&
                                client.AddSpell(LearnableSpell(12340))) {
                                dialog.Text("You have learned the Dragon Strides.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12320));
                                client.AddSpell(LearnableSpell(12330));
                                client.AddSpell(LearnableSpell(12340));
                                if (!client.AddSpell(LearnableSpell(12320)) ||
                                    !client.AddSpell(LearnableSpell(12330)) ||
                                    !client.AddSpell(LearnableSpell(12340))) {
                                    dialog.Text(
                                        "The three continuous strikes, [Air Kick], [Air Sweep] and [Air Raid], form the ultimate [Dragon Strides].");
                                    dialog.Text("~Where they sweep, all living things quiver in terror.");
                                    dialog.Option("Great!", 255);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "The three continuous strikes, [Air Kick], [Air Sweep] and [Air Raid], form the ultimate [Dragon Strides].");
                                dialog.Text("~Where they sweep, all living things quiver in terror.");
                                dialog.Option("Great!", 255);
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
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 8: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Level >= 15 && client.AddSpell(LearnableSpell(12270))) {
                            dialog.Text("You have learned the Dragon Flow.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(12270));
                            if (!client.AddSpell(LearnableSpell(12270))) {
                                dialog.Text(
                                    "Descendents of the Dragon will be blessed with Stamina when it is most needed.");
                                dialog.Option("Good.", 255);
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
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 9: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Level >= 40) {
                            if (client.AddSpell(LearnableSpell(12120)) && client.AddSpell(LearnableSpell(12130)) &&
                                client.AddSpell(LearnableSpell(12140))) {
                                dialog.Text("You have learned the Dragon Kicks.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12120));
                                client.AddSpell(LearnableSpell(12130));
                                client.AddSpell(LearnableSpell(12140));
                                if (!client.AddSpell(LearnableSpell(12120)) ||
                                    !client.AddSpell(LearnableSpell(12130)) ||
                                    !client.AddSpell(LearnableSpell(12140))) {
                                    dialog.Text(
                                        "[Dragon Kicks] consist of 3 successive kicks, first hit - Speed Kick, second hit - Violent Kick, and third hit - Storm Kick.");
                                    dialog.Text(
                                        "~Each kick deals considerable damage to opponents within range in a flash.");
                                    dialog.Option("Excellent!", 255);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "[Dragon Kicks] consist of 3 successive kicks, first hit - Speed Kick, second hit - Violent Kick, and third hit - Storm Kick.");
                                dialog.Text(
                                    "~Each kick deals considerable damage to opponents within range in a flash.");
                                dialog.Option("Excellent!", 255);
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
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 10: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(12160))) {
                            dialog.Text("You have learned the Cracking Swipe.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(12160));
                            if (!client.AddSpell(LearnableSpell(12160))) {
                                dialog.Text(
                                    "[Cracking Swipe], the first hit of [Dragon Swipe], disarms all enemies before the eyes,");
                                dialog.Text(
                                    "~while the second hit, [Splitting Swipe], completely arouses the power at critical moments.");
                                dialog.Option("Great!", 255);
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
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 12: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Level >= 100 && client.AddSpell(LearnableSpell(12170))) {
                            dialog.Text("You have learned the Splitting Swipe.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(12170));
                            if (!client.AddSpell(LearnableSpell(12170))) {
                                dialog.Text(
                                    "[Cracking Swipe], the first hit of [Dragon Swipe], disarms all enemies before the eyes,");
                                dialog.Text(
                                    "~while the second hit, [Splitting Swipe], completely arouses the power at critical moments.");
                                dialog.Option("Great!", 255);
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
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 13: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Level >= 100 && client.AddSpell(LearnableSpell(12350))) {
                            dialog.Text("You have learned the Dragon Slash.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(12350));
                            if (!client.AddSpell(LearnableSpell(12350))) {
                                dialog.Text(
                                    "[Dragon Slash] is a range attack skill which suddenly tears off the opponent's guard by illusive kicking tails.");
                                dialog.Option("Sounds good.", 255);
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
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 14: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(12280))) {
                            dialog.Text("You have learned the Dragon Roar.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(12280));
                            if (!client.AddSpell(LearnableSpell(12280))) {
                                dialog.Text(
                                    "The ones who fight at your side for justice will also be blessed by the Dragon, recovering Stamina at critical moments and you should reborn Dragon.");
                                dialog.Option("Great!", 255);
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
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 15: {
                    if (client.Entity.Class >= 80 && client.Entity.Class <= 85) {
                        if (client.Entity.FirstRebornClass >= 85) {
                            if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(12200))) {
                                dialog.Text("You have learned the Dragon Swing.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12200));
                                if (!client.AddSpell(LearnableSpell(12200))) {
                                    dialog.Text(
                                        "The ones who fight at your side for justice will also be blessed by the Dragon, recovering Stamina at critical moments.");
                                    dialog.Option("Great!", 255);
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
                            dialog.Text(
                                "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            "I will not tell any of the Dragon-Warrior secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion

                #region Pure Skill

                case 95: {
                    if (client.Entity.Class >= 85 && client.Entity.FirstRebornClass >= 85 &&
                        client.Entity.SecondRebornClass >= 85) {
                        dialog.Text(
                            "The pure skill, [Dragon Fury], is exclusive for Dragon-Warrior who were also Dragon-Warrior in previous two lives.");
                        dialog.Text(
                            "~If you've been 2nd-reborn but not a pure Dragon-Warrior, go find the Rebirth Master for reincarnation to get a new class combination.");
                        dialog.Option("Learn [Dragon Fury].", 17);
                        dialog.Option("I see.", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text(
                            "You're not a pure Dragon-Warrior, so you can't learn [Dragon Fury]. Go find the Rebirth Master for reincarnation.");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 17: {
                    if (client.Entity.Class >= 85 && client.Entity.FirstRebornClass >= 85 &&
                        client.Entity.SecondRebornClass >= 85) {
                        if (client.AddSpell(LearnableSpell(12300))) {
                            dialog.Text("You have learned Dragon Fury.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(12300));
                            if (!client.AddSpell(LearnableSpell(12300))) {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you.", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "You are not allowed, I think you're not promoted yet or you're not Revenge Walter.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            "You are not allowed, I think you're not promoted yet or you're not Revenge Walter.");
                        dialog.Option("Thank you.", 255);
                        dialog.Send();
                    }

                    break;
                }

                #endregion
            }
        }
    }
}