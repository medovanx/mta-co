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
    /// Trojan Master - Provides skills learning for Trojan class
    /// </summary>
    [NpcHandler(10022)]
    public static class Npc_TrojanMaster
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            dialog.Text("I am the master of the one hand weapons. As I see, you started your way on conquering this world. I will try to help you teaching you warrior skills and promoting you.");
                            dialog.Option("Promote me.", 1);
                            dialog.Option("Learn skills.", 2);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("Trojans do not share their secrets of battle with others. I shall not teach you.");
                            dialog.Option("I see.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region Promotion
                case 1:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            if (client.Entity.Class == 15)
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
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 3:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            if (client.Entity.Class == 15)
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
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Skill
                case 2:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            dialog.Text("Let me know what you want to learn.");
                            dialog.Option("XPSkills (Lvl 40).", 5);
                            dialog.Option("Hercules (Lvl 40).", 6);
                            dialog.Option("Golem. (Lvl 40).", 7);
                            dialog.Option("Spritual Healing. (Lvl 40).", 8);
                            dialog.Option("New: Super Cyclone (Lv.40)", 16);
                            dialog.Option("New: Fatal Cross. (Lv.40)", 17);
                            dialog.Option("New: Mortal Strike. (Lv.40)", 18);
                            dialog.Option("New: Breath Focus (Lv.90)", 19);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 5:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            if (client.Entity.Level >= 40)
                            {
                                dialog.Text("You have learned the XP Skills.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(1110));
                                client.AddSpell(LearnableSpell(1015));
                                if (!client.AddSpell(LearnableSpell(1110)) || !client.AddSpell(LearnableSpell(1015)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
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
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 6:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(1115)))
                            {
                                dialog.Text("You have learned the Hercules.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(1115));
                                if (!client.AddSpell(LearnableSpell(1115)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
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
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 7:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(1270)))
                            {
                                dialog.Text("You have learned the Golem.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(1270));
                                if (!client.AddSpell(LearnableSpell(1270)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
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
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 8:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(1190)))
                            {
                                dialog.Text("You have learned the Spritual Healing.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(1190));
                                if (!client.AddSpell(LearnableSpell(1190)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
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
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 16:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(11970)))
                            {
                                dialog.Text("You have learned the Super Cyclone.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(11970));
                                if (!client.AddSpell(LearnableSpell(11970)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
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
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 17:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(11980)))
                            {
                                dialog.Text("You have learned the Fatal Cross.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(11980));
                                if (!client.AddSpell(LearnableSpell(11980)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
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
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 18:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(11990)))
                            {
                                dialog.Text("You have learned the Mortal Strike.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(11990));
                                if (!client.AddSpell(LearnableSpell(11990)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
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
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 19:
                    {
                        if (client.Entity.Class >= 10 && client.Entity.Class <= 15)
                        {
                            if (client.Entity.Level >= 90 && client.AddSpell(LearnableSpell(11960)))
                            {
                                dialog.Text("You have learned the Breath Focus.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(11960));
                                if (!client.AddSpell(LearnableSpell(11960)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("You need to be level 90 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the trojan secrets to another class, so, good bye.");
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
