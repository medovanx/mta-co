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
    /// Warrior Master - Provides skills learning for Warrior class
    /// </summary>
    [NpcHandler(10001)]
    public static class Npc_WarriorMaster
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            dialog.Text("Warriors destroy the enemies and shield the Compatriots. but remember violence is a means of attaining a goal. Never let yourself sink into killing.");
                            dialog.Option("Learn Skills for Fist.", 7);
                            dialog.Option("Promote me.", 1);
                            dialog.Option("Learn Shield skills.", 2);
                            dialog.Option("Learn Weapon skills.", 8);
                            dialog.Option("Learn Pure skills.", 17);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("Only a warrior can learn what I have to teach. Our secrets are not for trade.");
                            dialog.Option("I understand.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region Promotion
                case 1:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Class == 25)
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
                            dialog.Text("I will not tell any of the warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 3:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Class == 25)
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
                                        client.Inventory.Add(client.PromoteItemGain, 0, 1);
                                        client.Entity.Class++;
                                        client.Entity.Update(10, "end_task", true);
                                        dialog.Text("Congratulations! You have been promoted.");
                                        dialog.Option("Thank you master.", 255);
                                        dialog.Send();
                                    }
                                    else
                                    {
                                        dialog.Text("You don't meet the requirements.");
                                        dialog.Option("Ahh.", 255);
                                        dialog.Send();
                                    }
                                    return;
                                }
                                if (client.Inventory.Contains(client.PromoteItemNeed, client.PromoteItemCountNeed) && client.Entity.Level >= client.PromoteLevelNeed)
                                {
                                    client.Inventory.Remove(client.PromoteItemNeed, client.PromoteItemCountNeed);
                                    client.Inventory.Add(client.PromoteItemGain, 0, 1);
                                    client.Entity.Class++;
                                    client.Entity.Update(10, "end_task", true);
                                    dialog.Text("Congratulations! You have been promoted.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                                else
                                {
                                    dialog.Text("You don't meet the requirements.");
                                    dialog.Option("Ahh.", 255);
                                    dialog.Send();
                                }
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Skills
                case 2:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            dialog.Text("Let me know what you want to learn.");
                            dialog.Option("XP Skills (Lvl 40).", 5);
                            dialog.Option("Dash (Lvl 61).", 6);
                            dialog.Option("Shield Block (Lvl 40).", 9);
                            dialog.Option("Defensive Stance (Lvl 70).", 10);
                            dialog.Option("Magic Defender (Lvl 40).", 11);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 7:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            dialog.Text("If You're interested in the skills for fist. I'm glad to give some guidance.");
                            dialog.Option("Scare of Earth. (Lv.40).", 77);
                            dialog.Option("Wave or Blood (Lv.40).", 88);
                            dialog.Option("Maniac Dance.(Lv.40).", 99);
                            dialog.Option("Pounce.", 98);
                            dialog.Option("Twist of war.(Lv.70).", 55);
                            dialog.Option("Backfire (Lvl 40).", 66);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region Scare of Earth
                case 77:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(12670)))
                            {
                                dialog.Text("You have learned the Scare of Earth of this Fist.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12670));
                                if (!client.AddSpell(LearnableSpell(12670)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Sorry, you can't get Scare of Earth because you are not level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Wave or Blood
                case 88:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(12690)))
                            {
                                dialog.Text("You have learned the Wave or Blood of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12690));
                                if (!client.AddSpell(LearnableSpell(12690)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Sorry, you can't get Wave or Blood because you are not level 70 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Pounce
                case 98:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(12770)))
                            {
                                dialog.Text("You have learned the Pounce of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12770));
                                if (!client.AddSpell(LearnableSpell(12770)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Sorry, you can't get Pounce because you are not level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Maniac Dance
                case 99:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(12700)))
                            {
                                dialog.Text("You have learned the Maniac Dance of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12700));
                                if (!client.AddSpell(LearnableSpell(12700)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Sorry, you can't get Maniac Dance because you are not level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Twist of war
                case 55:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(12660)))
                            {
                                dialog.Text("You have learned the Twist of war of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12660));
                                if (!client.AddSpell(LearnableSpell(12660)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Sorry, you can't get Twist of war because you are not level 70 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Backfire
                case 66:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 61 && client.AddSpell(LearnableSpell(12680)))
                            {
                                dialog.Text("You have learned the Backfire Skill of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12680));
                                if (!client.AddSpell(LearnableSpell(12680)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("You need to be level 61 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                case 8:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                dialog.Text("You have learned the Fast/Second of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(1045));
                                client.AddSpell(LearnableSpell(1046));
                                if (!client.AddSpell(LearnableSpell(1045)) || !client.AddSpell(LearnableSpell(1046)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Sorry, you can't get Fast/Second because you are not level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 17:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(10311)))
                            {
                                dialog.Text("You have learned the Perseverance of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(10311));
                                if (!client.AddSpell(LearnableSpell(10311)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Sorry, you can't get Perseverance because you are not level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 11:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(11200)))
                            {
                                dialog.Text("You have learned the Magic Defender of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(11200));
                                if (!client.AddSpell(LearnableSpell(11200)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Sorry, you can't get Magic Defender because you are not level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 10:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(11160)))
                            {
                                dialog.Text("You have learned the Defensive Stance of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(11160));
                                if (!client.AddSpell(LearnableSpell(11160)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Sorry, you can't get Defensive Stance because you are not level 70 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 9:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(10470)))
                            {
                                dialog.Text("You have learned the Shield Block of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(10470));
                                if (!client.AddSpell(LearnableSpell(10470)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Sorry, you can't get Shield Block because you are not level 40 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the Warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 5:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                dialog.Text("You have learned the XP Skills of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(1025));
                                client.AddSpell(LearnableSpell(1020));
                                client.AddSpell(LearnableSpell(1015));
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
                            dialog.Text("I will not tell any of the warrior secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 6:
                    {
                        if (client.Entity.Class >= 20 && client.Entity.Class <= 25)
                        {
                            if (client.Entity.Level >= 61 && client.AddSpell(LearnableSpell(1051)))
                            {
                                dialog.Text("You have learned the Dash Skill of this class.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(1051));
                                if (!client.AddSpell(LearnableSpell(1051)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("You need to be level 61 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the warrior secrets to another class, so, good bye.");
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
