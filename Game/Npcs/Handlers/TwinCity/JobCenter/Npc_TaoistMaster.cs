using System;
using System.Collections.Generic;
using System.Linq;
using static MTA.Game.Enums;
using MTA.Network;
using MTA.Network.GamePackets;
using static MTA.Npcs;

namespace MTA.Game.Npcs.Handlers.TwinCity.JobCenter
{
    /// <summary>
    /// Taoist Master - Provides skills learning for Taoist class
    /// </summary>
    [NpcHandler(10000)]
    public static class Npc_TaoistMaster
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            byte mClass = 142;
            byte MClass = 145;
            byte mClasss = 132;
            byte MClasss = 135;
            string Class = "Fire-Taoist";
            string Classs = "Water-Taoist";
            dialog.Avatar(6);
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        if (client.Entity.Class >= 100 && client.Entity.Class <= 101)
                        {
                            dialog.Text("I am the master of the toist skills. As I see, you started your way on conquering this world. I will try to help you teaching you warrir skills and promoting you.");
                            dialog.Option("Promote me.", 100);
                            dialog.Option("Learn Pure skills.", 70);
                            dialog.Option("Just passing by.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                            {
                                dialog.Text("Every Taoist wants holy spirit and few gain. You`re gifted in controlling inner power. Just pay more attention to details, you may be the next achiever.");
                                dialog.Option("I want to get promoted.", 1);
                                dialog.Option("Learn class skills.", 2);
                                dialog.Option("Learn Pure Fire Taoist.", 70);
                                dialog.Option("Okay. I see.", 255);
                                dialog.Send();
                            }
                            else if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                            {
                                dialog.Text("Every Taoist wants holy spirit and few gain. You`re gifted in controlling inner power. Just pay more attention to details, you may be the next achiever.");
                                dialog.Option("I want to get promoted.", 1);
                                dialog.Option("Learn class skills.", 101);
                                dialog.Option("Learn Pure WaterTaoist.", 80);
                                dialog.Option("Okay. I see.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("The taoists` secrets are not to be trained to outsiders. Train elsewhere, master conqueror.");
                                dialog.Option("I~see.", 255);
                                dialog.Send();
                            }
                        }
                        break;
                    }
                #region Prometed
                case 100:
                    {
                        if (client.Entity.Class >= 100 && client.Entity.Class <= 101)
                        {
                            if (client.Entity.Class == 101)
                            {
                                dialog.Text("I Want To promote ?");
                                dialog.Option("Water Taoist.", 253);
                                dialog.Option("Fire Taoist.", 254);
                                dialog.Option("No thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("To promote now you need" + client.PromoteItemNameNeed + " level " + client.PromoteLevelNeed + ".");
                                dialog.Option("Promote me sir.", 254);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region Promote Taoist
                case 253:
                    {
                        if (client.Entity.Class == 100)
                        {
                            client.Entity.Class++;
                            client.Entity.Update(10, "end_task", true);
                            dialog.Text("Congratulations! You have been promoted.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                        else if (client.Entity.Class == 101)
                        {
                            client.Entity.Class = 132;
                            client.Entity.Update(10, "end_task", true);
                            dialog.Text("Congratulations! You have been promoted.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 254:
                    {
                        if (client.Entity.Class == 100)
                        {
                            client.Entity.Class++;
                            client.Entity.Update(10, "end_task", true);
                            dialog.Text("Congratulations! You have been promoted.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                        else if (client.Entity.Class == 101)
                        {
                            client.Entity.Class = 142;
                            client.Entity.Update(10, "end_task", true);
                            dialog.Text("Congratulations! You have been promoted.");
                            dialog.Option("Thank you master.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                case 1:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass || client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Class == MClass || client.Entity.Class == MClasss)
                            {
                                dialog.Text("You cannot be promoted anymore. You have mastered your class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("To promote now you need" + client.PromoteItemNameNeed + " level " + client.PromoteLevelNeed + ".");
                                dialog.Option("Promote me sir.", 3);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the taoist secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 3:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass || client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Class == MClass || client.Entity.Class == MClasss)
                            {
                                dialog.Text("You cannot be promoted anymore. You have mastered your class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                if (client.PromoteItemNeed == 721020)
                                {
                                    if (client.Inventory.Remove("moonbox"))
                                    {
                                        client.Entity.Update(10, "end_task", true);
                                        client.Inventory.Add(client.PromoteItemGain, 0, 1);
                                        client.Entity.Class++;
                                        dialog.Text("Congratulations! You have been promoted.");
                                        dialog.Option("Thank you master.", 255);
                                        dialog.Send();
                                    }

                                    else
                                    {
                                        dialog.Text("You don't meet the requierments.");
                                        dialog.Option("Ahh.", 255);
                                        dialog.Send();
                                    }
                                    return;
                                }
                                if (client.Inventory.Contains(client.PromoteItemNeed, client.PromoteItemCountNeed) && client.Entity.Level >= client.PromoteLevelNeed)
                                {
                                    client.Entity.Update(10, "end_task", true);
                                    client.Inventory.Remove(client.PromoteItemNeed, client.PromoteItemCountNeed);
                                    client.Inventory.Add(client.PromoteItemGain, 0, 1);
                                    client.Entity.Class++;
                                    dialog.Text("Congratulations! You have been promoted.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                                else
                                {
                                    dialog.Text("You don't meet the requierments.");
                                    dialog.Option("Ahh.", 255);
                                    dialog.Send();
                                }
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the raoist secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Water
                #region Learn Pure WaterTaoist
                case 80:
                    {
                        if (client.Entity.Class >= 132 && client.Entity.FirstRebornClass >= 132 && client.Entity.SecondRebornClass >= 132)
                        {
                            if (!client.AddSpell(LearnableSpell(30000)))
                            {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                break;
                            }
                            dialog.Text("You have learned AzureShield.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You are not allowed, I think your not promoted yet or your not Revenge Walter.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Learn class skills
                case 101:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            dialog.Text("i know many types of skills and spells for a true water taoist, like you! which of them do you want to learn, first ?.");
                            dialog.Option("Learn water Spell.", 15);
                            dialog.Option("Learn XP Skill.", 21);
                            dialog.Option("Universal Skills.", 102);
                            dialog.Option("Supirior skill.", 4);
                            dialog.Option("[New]Skill Epic Water.", 106);
                            dialog.Option("Nothing.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region Learn water Spell
                case 15:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            dialog.Text("There are many Unique Skills and Spells available to those Who study the Path of water What whould you ask of me ?");
                            dialog.Option("Healing Rain.", 118);
                            dialog.Option("Invisibility [Level 60+].", 112);
                            dialog.Option("Star of Accuracy .", 119);
                            dialog.Option("Magic Shield [Level 50+].", 110);
                            dialog.Option("Stigma.", 111);
                            dialog.Option("Pary [Level 70] .", 113);
                            dialog.Option("Advanced Cure [Level 81+].", 217);
                            dialog.Option("Next.", 20);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region Healing Rain
                case 118:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                if (!client.AddSpell(LearnableSpell(1055)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the fire healing rain.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Invisibility
                case 112:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 70)
                            {
                                if (!client.AddSpell(LearnableSpell(1075)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the invisibility skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 70 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Star of Accuacy
                case 119:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                if (!client.AddSpell(LearnableSpell(1085)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the star of acurracy skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Magic Shield
                case 110:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                if (!client.AddSpell(LearnableSpell(1090)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the magic shield skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region stigma
                case 111:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                if (!client.AddSpell(LearnableSpell(1095)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the stigma skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region pary
                case 113:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 70)
                            {
                                if (!client.AddSpell(LearnableSpell(1100)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the pray skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 70 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Advanced Cure
                case 217:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 80)
                            {
                                if (!client.AddSpell(LearnableSpell(1175)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the advanced cure skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 80 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                case 20:
                    {
                        if (client.Entity.Class >= 131 && client.Entity.Class <= 135)
                        {
                            dialog.Text("Let me know what you want to learn.");
                            dialog.Option("Nectar [Level 94+].", 116);
                            dialog.Option("Summon Guard.", 88);
                            dialog.Option("Summon Bat Boss [Level 40+].", 81);

                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region Nectar
                case 116:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 70)
                            {
                                if (!client.AddSpell(LearnableSpell(1170)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the nectar skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 70 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Summon Guard
                case 88:
                    {
                        if (client.Entity.Reborn > 0)
                        {
                            if (client.Inventory.Contains(1072031, 1))
                            {
                                client.Inventory.Remove(1072031, 1);
                                if (!client.AddSpell(LearnableSpell(4000)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                        }
                        else
                        {
                            dialog.Text("You cannot learn those skills until you reborn atleast once.");
                            dialog.Option("Alright", 255);
                            dialog.Send();
                            break;
                        }
                        break;
                    }
                #endregion
                #region Summon Bat Boss
                case 81:
                    {
                        if (client.Entity.Reborn > 0)
                        {
                            if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                            {
                                dialog.Text("You cannot learn skills like this one. Ninjas don't need such thing. They are much more stronger than every other class.");
                                dialog.Option("Alright", 255);
                                dialog.Send();
                                break;
                            }
                            if (client.Inventory.Contains(1072054, 1))
                            {
                                client.Inventory.Remove(1072054, 1);
                                if (client.Entity.Class <= 15)
                                    client.AddSpell(LearnableSpell(4050));
                                else if (client.Entity.Class <= 25)
                                    client.AddSpell(LearnableSpell(4060));
                                else if (client.Entity.Class <= 45)
                                    client.AddSpell(LearnableSpell(4070));
                                else if (client.Entity.Class <= 135)
                                    client.AddSpell(LearnableSpell(4010));
                                else if (client.Entity.Class <= 145)
                                    client.AddSpell(LearnableSpell(4020));
                            }
                        }
                        else
                        {
                            dialog.Text("You cannot learn those skills until you reborn atleast once.");
                            dialog.Option("Alright", 255);
                            dialog.Send();
                            break;
                        }
                        break;
                    }
                #endregion
                #endregion
                #region Learn XP Skill
                case 21:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            dialog.Text("You can Learn lightning, Volcano, Water Elf, Revive, and Chain Bolt From me. ");
                            dialog.Option("Volcano [level 40].", 218);
                            dialog.Option("Lightning .", 117);
                            dialog.Option("XP Revive.", 115);
                            dialog.Option("Chain bolt.", 61);
                            dialog.Send();
                        }
                        break;
                    }
                #region Volcano
                case 218:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                if (!client.AddSpell(LearnableSpell(1125)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the volcano xp skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Lightning
                case 117:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                if (!client.AddSpell(LearnableSpell(1010)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the lightning xp skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region XP Revive
                case 115:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                if (!client.AddSpell(LearnableSpell(1050)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned xp revive skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Chain bolt
                case 210:
                    {
                        if (client.Entity.Class >= mClasss && client.Entity.Class <= MClasss)
                        {
                            if (client.Entity.Level >= 80)
                            {
                                if (!client.AddSpell(LearnableSpell(10309)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the chain bolt.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 80 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #endregion
                #region Universal Skills
                case 102:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            dialog.Text("Thunder and Cure are Two powerful arts that you are ready to practice. Should i teach you now ?");
                            dialog.Option("Cure (Lvl 1).", 206);
                            dialog.Option("Meditation. (Lvl 40).", 207);
                            dialog.Option("Thunder (Lvl 1).", 205);
                            dialog.Option("Nothing.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 205:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            if (!client.AddSpell(LearnableSpell(1000)))
                            {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                break;
                            }
                            dialog.Text("You have learned thunder.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 206:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            if (!client.AddSpell(LearnableSpell(1005)))
                            {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                break;
                            }
                            dialog.Text("You have learned cure.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 207:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                if (!client.AddSpell(LearnableSpell(1195)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned meditation.");
                                dialog.Option("Thank you.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }

                case 208:
                    {
                        if (client.Entity.Level >= 40)
                        {
                            if (!client.AddSpell(LearnableSpell(10309)))
                            {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                break;
                            }
                            dialog.Text("You have learned chain bolt.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need atleast level 40.");
                            dialog.Option("Ahh.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Supirior skil
                case 4:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            dialog.Text("Only the most Powerful of taoists can master that are. [Fire Taoist, Level 80+] .");
                            dialog.Option("I understand.", 255);
                            dialog.Send();
                        }

                        break;
                    }
                #endregion
                #region Skill Epic
                case 106:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            dialog.Text("There are many Unique Skills and Spells available to those Who study the Path of water What whould you ask of me ?");
                            dialog.Option("FlameLotus (Lvl 117).", 178);
                            dialog.Option("BreakingTouch (Lvl 94).", 183);
                            dialog.Option("Nothing.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region AuroraLotus
                case 178:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            if (client.Entity.Level >= 117)
                            {
                                if (!client.AddSpell(LearnableSpell(12370)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the AuroraLotus skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 112 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }

                #endregion
                #region BlessingTouch
                case 183:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            if (client.Entity.Level >= 94)
                            {
                                if (!client.AddSpell(LearnableSpell(12390)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the BlessingTouch skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 94 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }

                #endregion
                #endregion
                #endregion
                #endregion
                #region Fire
                #region Pure skill
                case 70:
                    {
                        if (client.Entity.Class >= 142 && client.Entity.FirstRebornClass >= 142 && client.Entity.SecondRebornClass >= 142)
                        {
                            if (!client.AddSpell(LearnableSpell(10310)))
                            {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                break;
                            }
                            dialog.Text("You have learned HeavenBlade.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You are not allowed, I think your not promoted yet or your not Revenge Fire.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Learn class skills
                case 2:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            dialog.Text("i know many types of skills and spells for a true water taoist, like you! which of them do you want to learn, first ?.");
                            dialog.Option("Learn Magic.", 71);
                            dialog.Option("Learn XP Skill.", 22);
                            dialog.Option("Universal Skills.", 102);
                            dialog.Option("Supirior skill.", 69);
                            dialog.Option("[New]Skill Epic Fire.", 199);
                            dialog.Option("Nothing.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Classs + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region Learn Magic
                case 71:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            dialog.Text("There are many Unique Skills and Spells available to those Who study the Path of water What whould you ask of me ?");
                            dialog.Option("Fire Ring.", 33);
                            dialog.Option("Fire Meteor.", 34);
                            dialog.Option("Fire Circle .", 35);
                            dialog.Option("Bomb.", 36);
                            dialog.Option("Fire of Hell.", 37);
                            dialog.Option("Tornado.", 38);
                            dialog.Option("Nothing.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region Fire Ring
                case 33:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            if (client.Entity.Level >= 52)
                            {
                                if (!client.AddSpell(LearnableSpell(1150)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the fire ring skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 52 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }

                #endregion
                #region Fire Meteor
                case 34:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            if (client.Entity.Level >= 55)
                            {
                                if (!client.AddSpell(LearnableSpell(1180)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the fire meteor skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 55 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Fire Circle
                case 35:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            if (client.Entity.Level >= 48)
                            {
                                if (!client.AddSpell(LearnableSpell(1120)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the fire circle skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 48 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Bomb
                case 36:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            if (client.Entity.Level >= 43)
                            {
                                if (!client.AddSpell(LearnableSpell(1160)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the bomb skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 43 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }

                #endregion
                #region Fire of Hell
                case 37:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            if (client.Entity.Level >= 48)
                            {
                                if (!client.AddSpell(LearnableSpell(1165)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the fire of Hell skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 48 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Tornado
                case 38:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            if (client.Entity.Level >= 81)
                            {
                                if (client.Spells.ContainsKey(1001) && client.Spells[1001] != null && client.Spells[1001].Level == 3)
                                {
                                    if (!client.AddSpell(LearnableSpell(1002)))
                                    {
                                        dialog.Text("You already know this skill.");
                                        dialog.Option("Thank you master.", 255);
                                        dialog.Send();
                                        break;
                                    }
                                    dialog.Text("You have learned the tornado.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                                else
                                {
                                    dialog.Text("You need to know thunder very well to be able to learn the fire skill.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("You need to be level 81 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #endregion
                #region Learn XP Skill
                case 22:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            dialog.Text("You can Learn lightning, Volcano, Water Elf, Revive, and Chain Bolt From me. ");
                            dialog.Option("Chain Bolt.", 61);
                            dialog.Option("Lightning .", 62);
                            dialog.Option("Volcano.", 63);
                            dialog.Option("SpeedLightning.", 64);
                            dialog.Send();
                        }
                        break;
                    }
                case 63:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            if (client.Entity.Level >= 1)
                            {
                                if (!client.AddSpell(LearnableSpell(1125)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the volcano xp skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 1 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 62:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            if (client.Entity.Level >= 1)
                            {
                                if (!client.AddSpell(LearnableSpell(1010)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the lightning xp skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 1 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 64:
                    {
                        if (client.Entity.Class >= mClass && client.Entity.Class <= MClass)
                        {
                            if (client.Entity.Level >= 1)
                            {
                                if (!client.AddSpell(LearnableSpell(5001)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the Speedlightning xp skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 1 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 61:
                    {
                        if (client.Entity.Level >= 40)
                        {
                            if (!client.AddSpell(LearnableSpell(10309)))
                            {
                                dialog.Text("You already know this skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                break;
                            }
                            dialog.Text("You have learned chain bolt.");
                            dialog.Option("Thank you.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You need atleast level 40.");
                            dialog.Option("Ahh.", 255);
                            dialog.Send();
                        }
                        break;
                    }

                #endregion
                #region Supirior skil
                case 69:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            dialog.Text("Before learning the Tornado spell, you must first parctice you fire spell. [Fire Spell 3rd Level+] .");
                            dialog.Option("I understand.", 255);
                            dialog.Send();

                        }
                        break;
                    }
                #endregion
                #region Skill Epic
                case 199:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            dialog.Text("There are many Unique Skills and Spells available to those Who study the Path of water What whould you ask of me ?");
                            dialog.Option("FlameLotus (Lvl 117).", 198);
                            dialog.Option("BreakingTouch (Lvl 86).", 211);
                            dialog.Option("Nothing.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region BreakingTouch
                case 211:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            if (client.Entity.Level >= 117)
                            {
                                if (!client.AddSpell(LearnableSpell(12400)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the FlameLotus skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 117 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }

                #endregion

                #region FlameLotus
                case 198:
                    {
                        if (client.Entity.Class >= 100)
                        {
                            if (client.Entity.Level >= 117)
                            {
                                if (!client.AddSpell(LearnableSpell(12380)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                    break;
                                }
                                dialog.Text("You have learned the FlameLotus skill.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("You need to be level 117 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the " + Class + " secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }

                    #endregion

                    #endregion
                    #endregion
                    #endregion
            }
        }
    }
}