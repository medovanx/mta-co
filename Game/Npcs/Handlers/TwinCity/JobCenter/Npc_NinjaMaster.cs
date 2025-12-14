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
    /// Ninja Master - Provides skills learning for Ninja class
    /// </summary>
    [NpcHandler(4720)]
    public static class Npc_NinjaMaster
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            dialog.Text("I am the master of the no sound attack and the master of the katanas. As I see, you started your way on conquering this world. I will try to help you teaching you warrior skills and promoting you.");
                            dialog.Option("Promote me.", 1);
                            dialog.Option("Learn skills.", 2);
                            dialog.Option("Learn Epic Shadow Skills.", 76);
                            dialog.Option("Learn skills Scythe.", 59);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("Fancy the skills of Ninja, huh? But the secrets of Ninja are not for trade. Find your own trainer, please.");
                            dialog.Option("What a shame!", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #region Promotion
                case 1:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Class == 55)
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
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 3:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Class == 55)
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
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Skills
                case 2:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            dialog.Text("Let me know what you want to learn. '[...]' and '{...}' means ninja as second life and ninja as third life.");
                            dialog.Option("FatalStrike (Lvl 15).", 11);
                            dialog.Option("TwofoldBlades (Lvl 40).", 5);
                            dialog.Option("SuperTwofoldBlade (Lvl 40).", 82);
                            dialog.Option("ToxicFog (Lvl 70).", 6);
                            dialog.Option("ShurikenVortex (Lvl 70).", 10);
                            dialog.Option("PoisonStar [Lvl 70].", 7);
                            dialog.Option("ArcherBane (Lvl 110).", 9);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 11:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 15 && client.AddSpell(LearnableSpell(6011)))
                            {
                                dialog.Text("You have learned the FatalStrike.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(6011));
                                if (!client.AddSpell(LearnableSpell(6011)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("You need to be level 15 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 5:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(6000)))
                            {
                                dialog.Text("You have learned the TwofoldBlades.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(6000));
                                if (!client.AddSpell(LearnableSpell(6000)))
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
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 82:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(12080)))
                            {
                                dialog.Text("You have learned the SuperTwofoldBlade.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12080));
                                if (!client.AddSpell(LearnableSpell(12080)))
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
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 6:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(6001)))
                            {
                                dialog.Text("You have learned the ToxicFog.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(6001));
                                if (!client.AddSpell(LearnableSpell(6001)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
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
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 10:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 70 && client.AddSpell(LearnableSpell(6010)))
                            {
                                dialog.Text("You have learned the ShurikenVortex.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(6010));
                                if (!client.AddSpell(LearnableSpell(6010)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
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
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 7:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 70)
                            {
                                if ((client.Entity.Reborn == 1 && client.Entity.Class == 55) || (client.Entity.Reborn == 2 && client.Entity.SecondRebornClass == 55))
                                {
                                    if (client.AddSpell(LearnableSpell(6002)))
                                    {
                                        dialog.Text("You have learned the PoisonStar.");
                                        dialog.Option("Thank you master.", 255);
                                        dialog.Send();
                                        client.AddSpell(LearnableSpell(6002));
                                        if (!client.AddSpell(LearnableSpell(6002)))
                                        {
                                            dialog.Text("You already know this skill.");
                                            dialog.Option("Thank you master.", 255);
                                            dialog.Send();
                                        }
                                    }
                                    else
                                    {
                                        dialog.Text("You already know this skill.");
                                        dialog.Option("Thank you master.", 255);
                                        dialog.Send();
                                    }
                                }
                                else
                                {
                                    dialog.Text("You need to be ninja in the second life.");
                                    dialog.Option("Alright.", 255);
                                    dialog.Send();
                                }
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
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 9:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 110 && client.AddSpell(LearnableSpell(6004)))
                            {
                                dialog.Text("You have learned the ArcherBane.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(6004));
                                if (!client.AddSpell(LearnableSpell(6004)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("You need to be level 110 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Epic Shadow Skills
                case 76:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            dialog.Text("Let me know what you want to learn.");
                            dialog.Option("TwilightDance.", 80);
                            dialog.Option("ShadowClone.", 81);
                            dialog.Option("FatalSpin.", 83);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 80:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 15 && client.AddSpell(LearnableSpell(12070)))
                            {
                                dialog.Text("You have learned the TwilightDance.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12070));
                                if (!client.AddSpell(LearnableSpell(12070)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("You need to be level 15 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 81:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 40 && client.AddSpell(LearnableSpell(12090)))
                            {
                                dialog.Text("You have learned the ShadowClone.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12090));
                                if (!client.AddSpell(LearnableSpell(12090)))
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
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 83:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 15 && client.AddSpell(LearnableSpell(12110)))
                            {
                                dialog.Text("You have learned the FatalSpin.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(12110));
                                if (!client.AddSpell(LearnableSpell(12110)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("You need to be level 15 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                #endregion
                #region Scythe Skills
                case 59:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            dialog.Text("Let me know what you want to learn.");
                            dialog.Option("Learn Bloody Scythe.", 60);
                            dialog.Option("Learn Mortal Drag.", 61);
                            dialog.Option("Wait a minute.", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 60:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 20 && client.AddSpell(LearnableSpell(11170)))
                            {
                                dialog.Text("You have learned the Bloody Scythe.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(11170));
                                if (!client.AddSpell(LearnableSpell(11170)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("You need to be level 20 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
                            dialog.Option("Alright.", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 61:
                    {
                        if (client.Entity.Class >= 50 && client.Entity.Class <= 55)
                        {
                            if (client.Entity.Level >= 20 && client.AddSpell(LearnableSpell(11180)))
                            {
                                dialog.Text("You have learned the Mortal Drag.");
                                dialog.Option("Thank you master.", 255);
                                dialog.Send();
                                client.AddSpell(LearnableSpell(11180));
                                if (!client.AddSpell(LearnableSpell(11180)))
                                {
                                    dialog.Text("You already know this skill.");
                                    dialog.Option("Thank you master.", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("You need to be level 20 or more.");
                                dialog.Option("Alright.", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("I will not tell any of the ninja secrets to another class, so, good bye.");
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
