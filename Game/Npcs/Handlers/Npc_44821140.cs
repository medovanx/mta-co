using System;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers
{
    /// <summary>
    /// Super Guild War Prize Claimer NPC
    /// </summary>
    public static class Npc_44821140
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Hello there! Would you like to claim your Super Guild War prize? You can only claim it once if you won the Super Guild War.");
                        dialog.Option("Claim Top Guild Leader Prize", 1);
                        dialog.Option("Claim Top Deputy Leader Prize", 3);
                        dialog.Option("Claim Top Member Leader Prize", 6);
                        dialog.Option("Exchange Token to Cup", 9);
                        dialog.Option("Just Passing By", 255);
                        dialog.Send();
                        break;
                    }
                case 8:
                    {
                        if (client.Guild != null)
                        {
                            if (client.AsMember.Rank == GuildMemberRank.GuildLeader)
                            {
                                dialog.Text("Are you sure you want to exchange for your prize?");
                                dialog.Option("Yes", 9);
                                dialog.Option("Never mind", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("Sorry, only the Guild Leader of the winning guild can claim the prize after the Super Guild War ends.");
                                dialog.Option("I understand", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("Sorry, you are not a member of any guild yet.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 9:
                    {
                        dialog.Text("Which prize would you like to exchange for?");
                        dialog.Option("Bronze Prize [1 Lord Token]", 14);
                        dialog.Option("Silver Prize [2 Lord Tokens]", 13);
                        dialog.Option("Gold Trophy [4 Lord Tokens]", 12);
                        dialog.Option("Just passing by", 255);
                        dialog.Avatar(85);
                        dialog.Send();
                        break;
                    }
                case 12:
                    {
                        if (client.Inventory.Contains(723467, 4))
                        {
                            client.Inventory.Remove(723467, 4);
                            client.Inventory.Add(2100085, 0, 1);
                        }
                        else
                        {
                            dialog.Text("You don't have enough Lord Tokens! You need 4 Lord Tokens for the Gold Trophy.");
                            dialog.Option("I see", 255);
                            dialog.Avatar(85);
                            dialog.Send();
                        }
                        break;
                    }
                case 13:
                    {
                        if (client.Inventory.Contains(723467, 2))
                        {
                            client.Inventory.Remove(723467, 2);
                            client.Inventory.Add(2100065, 0, 1);
                        }
                        else
                        {
                            dialog.Text("You don't have enough Lord Tokens! You need 2 Lord Tokens for the Silver Prize.");
                            dialog.Option("I see", 255);
                            dialog.Avatar(85);
                            dialog.Send();
                        }
                        break;
                    }
                case 14:
                    {
                        if (client.Inventory.Contains(723467, 1))
                        {
                            client.Inventory.Remove(723467, 1);
                            client.Inventory.Add(2100055, 0, 1);
                        }
                        else
                        {
                            dialog.Text("You don't have enough Lord Tokens! You need 1 Lord Token for the Bronze Prize.");
                            dialog.Option("I see", 255);
                            dialog.Avatar(85);
                            dialog.Send();
                        }
                        break;
                    }
                case 1:
                    {
                        if (client.Guild != null)
                        {
                            if (client.Guild.SuperPoleKeeper && client.Guild != null && client.AsMember.Rank == GuildMemberRank.GuildLeader)
                            {
                                dialog.Text("Are you sure you want to claim your prize?");
                                dialog.Option("Yes", 2);
                                dialog.Option("Never mind", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("Sorry, only the Super Guild Leader of the winning guild can claim the prize after the Super Guild War ends.");
                                dialog.Option("I understand", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("Sorry, you are not a member of any guild yet.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 2:
                    {
                        if (!SuperGuildWar.IsWar && client.Guild.SuperPoleKeeper && SuperGuildWar.Claim && client.Guild != null && client.AsMember.Rank == GuildMemberRank.GuildLeader)
                        {
                            Program.AddWarLog("SuperGuildWar", ".", client.Entity.Name);
                            SuperGuildWar.Claim = false;
                            SuperGuildWar.KeeperID = 0;
                            client.Inventory.Add(723467, 0, 1);
                            client.Entity.AddTopStatus(Network.GamePackets.Update.Flags3.ConuqerSuperYellow, 3, DateTime.Now.AddDays(7));
                            Kernel.SendWorldMessage(new Message("Congratulations! " + client.Entity.Name + ", Leader of " + client.Guild.SuperPoleKeeper + ", the winning guild, has claimed the Super Guild War Prize Lord Token!", System.Drawing.Color.White, Message.TopLeft), Program.Values);
                        }
                        else
                        {
                            dialog.Text("Sorry, you don't have any prize to claim. Only the Guild Leader of the winning guild can claim the prize after the Super Guild War.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 3:
                    {
                        if (client.Guild != null)
                        {
                            if (client.Guild.SuperPoleKeeper && client.Guild != null && client.AsMember.Rank == GuildMemberRank.DeputyLeader)
                            {
                                dialog.Text("Are you sure you want to claim your prize?");
                                dialog.Option("Yes", 4);
                                dialog.Option("Never mind", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("Sorry, only the Deputy Leader of the winning guild can claim the prize after the Super Guild War ends.");
                                dialog.Option("I understand", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("Sorry, you are not a member of any guild yet.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 4:
                    {
                        if (!SuperGuildWar.IsWar && client.Guild.SuperPoleKeeper && client.Guild != null && client.AsMember.Rank == GuildMemberRank.DeputyLeader)
                        {
                            client.Entity.AddTopStatus((ulong)MTA.Network.GamePackets.Update.Flags3.ConuqerSuperBlue, 3, DateTime.Now.AddDays(7));
                            Kernel.SendWorldMessage(new Message("Congratulations! " + client.Entity.Name + " from " + client.Guild.SuperPoleKeeper + ", the winning guild, has claimed the Top Deputy Leader Halo!", System.Drawing.Color.White, Message.TopLeft), Program.Values);
                        }
                        else
                        {
                            dialog.Text("Sorry, you don't have any prize to claim. Only the Deputy Leader of the winning guild can claim the halo after the Super Guild War ends.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 6:
                    {
                        if (client.Guild != null)
                        {
                            if (client.Guild.SuperPoleKeeper && client.Guild != null && client.AsMember.Rank != GuildMemberRank.GuildLeader && client.AsMember.Rank != GuildMemberRank.DeputyLeader)
                            {
                                dialog.Text("Are you sure you want to claim your prize?");
                                dialog.Option("Yes", 7);
                                dialog.Option("Never mind", 255);
                                dialog.Send();
                            }
                            else
                            {
                                dialog.Text("Sorry, only a Member Leader of the winning guild can claim the prize after the Super Guild War ends.");
                                dialog.Option("I understand", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("Sorry, you are not a member of any guild yet.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 7:
                    {
                        if (!SuperGuildWar.IsWar && client.Guild.SuperPoleKeeper && client.Guild != null && client.AsMember.Rank != GuildMemberRank.GuildLeader && client.AsMember.Rank != GuildMemberRank.DeputyLeader)
                        {
                            client.Entity.AddTopStatus(Network.GamePackets.Update.Flags3.ConuqerSuperUnderBlue, 3, DateTime.Now.AddDays(7));
                            Kernel.SendWorldMessage(new Message("Congratulations! " + client.Entity.Name + " from " + client.Guild.SuperPoleKeeper + ", the winning guild, has claimed the Top Member Leader Title!", System.Drawing.Color.White, Message.TopLeft), Program.Values);
                        }
                        else
                        {
                            dialog.Text("Sorry, you don't have any prize to claim. Only members of the winning guild can claim the halo after the Super Guild War ends.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
            }
        }
    }
}
