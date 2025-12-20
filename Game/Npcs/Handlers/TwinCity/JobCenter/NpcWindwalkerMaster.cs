using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Npcs;

namespace MTA.Game.Npcs.Handlers.TwinCity.JobCenter {
    /// <summary>
    /// Windwalker Master - Provides skills learning for Windwalker class
    /// </summary>
    [NpcHandler(19634)]
    public static class NpcWindwalkerMaster {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        dialog.Text(
                            "Our Windwalker school was founded by Saint Wan on the principle of KINDNESS and JUSTICE. Once you decide to");
                        dialog.Text(
                            "~join us as a Windwalker, you take the responsibility for the security of the country against alien invaders and devils.");
                        dialog.Option("I want to get promoted.", 1);
                        dialog.Option("Learn skills.", 2);
                        dialog.Option("Switch to another branch.", 3);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("Sorry, you're not Wind-Walker. Go find your origin.");
                        dialog.Option("Okay.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }

                #region Promotion

                case 1: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Class == 165) {
                            dialog.Text("You cannot be promoted anymore. You have mastered your class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                        else {
                            dialog.Text("To promote now you need" + client.PromoteItemNameNeed + " level " +
                                        client.PromoteLevelNeed + ".");
                            dialog.Option("Promote me sir.", 27);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 27: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Class == 165) {
                            dialog.Text("You cannot be promoted anymore. You have mastered your class.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                        else {
                            if (client.PromoteItemNeed == 721020) {
                                if (client.Inventory.Remove("moonbox")) {
                                    client.Entity.Update(10, "end_task", true);
                                    client.Inventory.Add(client.PromoteItemGain, 0, 1);
                                    client.Entity.Class++;
                                    dialog.Text("Congratulations! You have been promoted.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                                else {
                                    dialog.Text("You don't meet the requirements.");
                                    dialog.Option("Ahh.", 255);
                                    dialog.Avatar(333);
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
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                            else {
                                dialog.Text("You don't meet the requirements.");
                                dialog.Option("Ahh.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }

                #endregion

                #region Skill

                case 2: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        dialog.Text(
                            "Our Windwalker school is divided into two branches: Chaser and Stomper. Chaser focuses on");
                        dialog.Text(
                            "~ranged combat, while Stomper excels in melee combat. I'm here representing the Chief to");
                        dialog.Text("~impart distinctive skills to the Windwalkers.");
                        dialog.Option("Learn skills of Stomper.", 4);
                        dialog.Option("Learn skills of Chaser.", 5);
                        dialog.Option("Learn rebirth skill.", 6);
                        dialog.Option("Learn universal skills.", 7);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 3: {
                    dialog.Text("										*Branches of Windwalker School*\n");
                    dialog.Text("ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ\n");
                    dialog.Text(
                        "Stomper: Excels in melee combat with excellent attack range and remarkable recovery ability. Stomper's exclusive skill\n");
                    dialog.Text(
                        "		   can freeze enemies' blood to extend their skill cooldown time and response time.\n");
                    dialog.Text(
                        "Chaser: Excels in ranged combat. With a unique focusing skill, Chaser is able to control the power of wind to attack\n");
                    dialog.Text("		  enemies far away.\n");
                    dialog.Text("ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ،ھ\n");
                    dialog.Text(
                        "(You're currently on the [MeleeStomper] branch. Each Windwalker can switch his/her branch once every 30 days, free of charge.)\n");
                    dialog.Option("Switch to [RangedChaser] branch.", 8);
                    dialog.Option("Wait a minute.", 255);
                    dialog.Avatar(333);
                    dialog.Send();
                    break;
                }
                case 4: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        dialog.Text(
                            "The Stomper branch has developed overwhelming melee skills which enable the caster to disable an enemy with swift and flexible fighting moves.");
                        dialog.Text("~FellowBrother, which skill would you like to learn?");
                        if (client.Spells.ContainsKey(12980)) {
                            dialog.Option("AngerofStomper [Lv.1].", 9);
                        }
                        else {
                            dialog.Option("AngerofStomper [Lv.1]. (Learned)", 9);
                        }

                        if (client.Spells.ContainsKey(12940)) {
                            dialog.Option("BurntFrost [Lv.15].", 10);
                        }
                        else {
                            dialog.Option("BurntFrost [Lv.15]. (Learned)", 10);
                        }

                        if (client.Spells.ContainsKey(12950)) {
                            dialog.Option("HealingSnow [Lv.15].", 11);
                        }
                        else {
                            dialog.Option("HealingSnow [Lv.15]. (Learned)", 11);
                        }

                        if (client.Spells.ContainsKey(12930)) {
                            dialog.Option("RageofWar [Lv.40].", 12);
                        }
                        else {
                            dialog.Option("RageofWar [Lv.40]. (Learned)", 12);
                        }

                        if (client.Spells.ContainsKey(12990)) {
                            dialog.Option("HorrorofStomper [Lv.40].", 13);
                        }
                        else {
                            dialog.Option("HorrorofStomper [Lv.40]. (Learned)", 13);
                        }

                        if (client.Spells.ContainsKey(12960)) {
                            dialog.Option("ChillingSnow [Lv.70] [Buff]", 14);
                        }
                        else {
                            dialog.Option("ChillingSnow [Lv.70] (Learned)", 14);
                        }

                        if (client.Spells.ContainsKey(13000)) {
                            dialog.Option("PeaceofStomper [Lv.70] [Passive]", 15);
                        }
                        else {
                            dialog.Option("PeaceofStomper [Lv.70] (Learned)", 15);
                        }

                        dialog.Option("Next.", 16);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 5: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        dialog.Text(
                            "The Chaser branch is famous for its ability to control wind and outstanding ranged attacks.");
                        dialog.Text("~FellowBrother, which skill would you like to learn?");
                        if (client.Spells.ContainsKey(12890)) {
                            dialog.Option("SwirlingStorm [Lv.15].", 17);
                        }
                        else {
                            dialog.Option("SwirlingStorm [Lv.15]. (Learned)", 17);
                        }

                        if (client.Spells.ContainsKey(13090)) {
                            dialog.Option("ShadowofChaser [Lv.15].", 18);
                        }
                        else {
                            dialog.Option("ShadowofChaser [Lv.15]. (Learned)", 18);
                        }

                        if (client.Spells.ContainsKey(12850)) {
                            dialog.Option("TripleBlasts [Lv.40].", 19);
                        }
                        else {
                            dialog.Option("TripleBlasts [Lv.40]. (Learned)", 19);
                        }

                        if (client.Spells.ContainsKey(12840)) {
                            dialog.Option("Thundercloud [Lv.70] [Summon]", 20);
                        }
                        else {
                            dialog.Option("Thundercloud [Lv.70] (Learned)", 20);
                        }

                        if (client.Spells.ContainsKey(12970)) {
                            dialog.Option("Thunderbolt [Lv.100] [EnhancedThundercloud]", 21);
                        }
                        else {
                            dialog.Option("Thunderbolt [Lv.100] (Learned)", 21);
                        }

                        dialog.Option("I want to learn other skills.", 2);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 6: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        dialog.Text(
                            "Frost Gaze is an exclusive rebirth skill for Windwalker. The heroes who got reborn to be Windwalker can learn Frost Gaze I. If both of your precious and");
                        dialog.Text(
                            "~current Class are Windwalker, you can learn Frost Gaze II. While for Windwalkers who were also a Windwalker in previous two lives, they can learn Frost Gaze III.");
                        dialog.Option("Frost Gaze I.", 22);
                        dialog.Option("I want to learn other skills.", 2);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 7: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        dialog.Text(
                            "Omnipotence and Justice Chant are basic skills for Windwalker, which are available to both Chaser and Stomper learners. I'll teach you if you want.");
                        if (client.Spells.ContainsKey(12860)) {
                            dialog.Option("Omnipotence [Lv.3].", 23);
                        }
                        else {
                            dialog.Option("Omnipotence [Lv.3]. (Learned)", 23);
                        }

                        if (client.Spells.ContainsKey(12870)) {
                            dialog.Option("JusticeChant [Lv.3].", 24);
                        }
                        else {
                            dialog.Option("JusticeChant [Lv.3]. (Learned)", 24);
                        }

                        dialog.Option("I want to learn other skills.", 2);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 8: {
                    dialog.Text("Okay, I've changed your branch to [RangedChaser].");
                    dialog.Option("Thanks!", 255);
                    dialog.Avatar(333);
                    dialog.Send();
                    break;
                }
                case 9: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.AddSpell(LearnableSpell(12980))) {
                            dialog.Text("FellowBrother has learned AngerofStomper. Go practice and feel its power.");
                            dialog.Option("Okay.", 255);
                            dialog.Avatar(333);
                            dialog.Send();
                            client.AddSpell(LearnableSpell(12980));
                            client.Entity.Update(10, "end_task", true);
                            if (!client.AddSpell(LearnableSpell(12980))) {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "FellowBrother has already acquired this skill. Go practice and feel its power.");
                            dialog.Option("Okay.", 255);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 10: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 15) {
                            if (client.AddSpell(LearnableSpell(12940))) {
                                dialog.Text("FellowBrother has learned BurntFrost. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12940));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12940))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [BurntFrost] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 15.");
                            dialog.Option("I'll see you at Lv.15!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 11: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 15) {
                            if (client.AddSpell(LearnableSpell(12950))) {
                                dialog.Text("FellowBrother has learned HealingSnow. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12950));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12950))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [HealingSnow] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 15.");
                            dialog.Option("I'll see you at Lv.15!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 12: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 40) {
                            if (client.AddSpell(LearnableSpell(12930))) {
                                dialog.Text("FellowBrother has learned RageofWar. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12930));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12930))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [RageofWar] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 40.");
                            dialog.Option("I'll see you at Lv.40!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 13: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 40) {
                            if (client.AddSpell(LearnableSpell(12990))) {
                                dialog.Text(
                                    "FellowBrother has learned HorrorofStomper. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12990));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12990))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [HorrorofStomper] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 40.");
                            dialog.Option("I'll see you at Lv.40!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 14: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 70) {
                            if (client.AddSpell(LearnableSpell(12960))) {
                                dialog.Text("FellowBrother has learned ChillingSnow. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12960));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12960))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [ChillingSnow] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 70.");
                            dialog.Option("I'll see you at Lv.70!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 15: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 70) {
                            if (client.AddSpell(LearnableSpell(13000))) {
                                dialog.Text(
                                    "FellowBrother has learned PeaceofStomper. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(13000));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(13000))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [PeaceofStomper] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 70.");
                            dialog.Option("I'll see you at Lv.70!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 16: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        dialog.Text(
                            "The Stomper branch has developed overwhelming melee skills which enable the caster to disable an enemy with swift and flexible fighting moves.");
                        dialog.Text("~FellowBrother, which skill would you like to learn?");
                        dialog.Option("RevengeTail [Lv.100] [Counterstrike]", 25);
                        dialog.Option("FreezingPelter [Lv.100] [Buff]", 26);
                        dialog.Option("I want to learn other skills.", 2);
                        dialog.Option("Previous.", 4);
                        dialog.Option("Wait a minute.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 17: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 15) {
                            if (client.AddSpell(LearnableSpell(12890))) {
                                dialog.Text("FellowBrother has learned SwirlingStorm. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12890));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12890))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [SwirlingStorm] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 15.");
                            dialog.Option("I'll see you at Lv.15!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 18: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 15) {
                            if (client.AddSpell(LearnableSpell(13090))) {
                                dialog.Text(
                                    "FellowBrother has learned ShadowofChaser. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(13090));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(13090))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [ShadowofChaser] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 15.");
                            dialog.Option("I'll see you at Lv.15!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 19: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 40) {
                            if (client.AddSpell(LearnableSpell(12850))) {
                                dialog.Text("FellowBrother has learned TripleBlasts. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12850));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12850))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [TripleBlasts] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 40.");
                            dialog.Option("I'll see you at Lv.40!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 20: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 70) {
                            if (client.AddSpell(LearnableSpell(12840))) {
                                dialog.Text("FellowBrother has learned Thundercloud. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12840));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12840))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [Thundercloud] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 70.");
                            dialog.Option("I'll see you at Lv.70!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 21: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 100) {
                            if (client.AddSpell(LearnableSpell(12970))) {
                                dialog.Text("FellowBrother has learned Thunderbolt. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12970));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12970))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [Thunderbolt] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 100.");
                            dialog.Option("I'll see you at Lv.100!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 22: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        client.MessageBox("You're not qualified to learn this skill.");
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 23: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 3) {
                            if (client.AddSpell(LearnableSpell(12860))) {
                                dialog.Text("FellowBrother has learned Omnipotence. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12860));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12860))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [Omnipotence] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 3.");
                            dialog.Option("I'll see you at Lv.3!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 24: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 3) {
                            if (client.AddSpell(LearnableSpell(12870))) {
                                dialog.Text("FellowBrother has learned JusticeChant. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12870));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(12870))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [JusticeChant] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 3.");
                            dialog.Option("I'll see you at Lv.3!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 25: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 100) {
                            if (client.AddSpell(LearnableSpell(13030))) {
                                dialog.Text("FellowBrother has learned RevengeTail. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(13030));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(13030))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [RevengeTail] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 100.");
                            dialog.Option("I'll see you at Lv.100!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }
                case 26: {
                    if (client.Entity.Class is >= 160 and <= 165) {
                        if (client.Entity.Level >= 100) {
                            if (client.AddSpell(LearnableSpell(13020))) {
                                dialog.Text(
                                    "FellowBrother has learned FreezingPelter. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(13020));
                                client.Entity.Update(10, "end_task", true);
                                if (!client.AddSpell(LearnableSpell(13020))) {
                                    dialog.Text(
                                        "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                    dialog.Option("Okay.", 255);
                                    dialog.Avatar(333);
                                    dialog.Send();
                                }
                            }
                            else {
                                dialog.Text(
                                    "FellowBrother has already acquired this skill. Go practice and feel its power.");
                                dialog.Option("Okay.", 255);
                                dialog.Avatar(333);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text(
                                "The study of [FreezingPelter] requires FellowBrother to reach a certain level. FellowBrother, you can find me when you reach Level 100.");
                            dialog.Option("I'll see you at Lv.100!", 2);
                            dialog.Avatar(333);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text("I will not tell any of the windwalker secrets to another class, so, good bye.");
                        dialog.Option("Alright.", 255);
                        dialog.Avatar(333);
                        dialog.Send();
                    }

                    break;
                }

                #endregion
            }
        }
    }
}