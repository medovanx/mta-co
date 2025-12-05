using System;
using System.Linq;
using static MTA.Game.Enums;
using MTA.Network.GamePackets;
using MTA.Database;
using MTA.Game.ConquerStructures.Society;

namespace MTA.Game.Npcs.Handlers.TwinCity
{
    /// <summary>
    /// Guild Director NPC - Handles guild creation, management, and administration
    /// </summary>
    [NpcHandler(10003)]
    public static class Npc_GuildDirector
    {
        public static void Handle(Client.GameState client, NpcRequest npcRequest, MTA.Npcs dialog)
        {
            switch (npcRequest.OptionID)
            {
                case 0:
                    {
                        dialog.Text("Hello there. Do you want to create a new guild? You need to be level 90, have 500,000 silver, and not belong to any guild. If you are a guild leader, you can manage your guild here.");
                        dialog.Option("Create Guild", 1);
                        dialog.Option("Disband Guild", 9);
                        dialog.Option("Change Guild Name", 17);
                        dialog.Option("Nevermind", 255);
                        dialog.Send();
                        break;
                    }
                default:
                    {
                        var member = client.Guild.Members.Values.Where(x => x.Name.StartsWith("~")).OrderBy(z => z.ID).ToArray()[npcRequest.OptionID - 100];
                        if (member.Rank != GuildMemberRank.Member)
                        {
                            dialog.Text("You cannot promote this member anymore.");
                            dialog.Option("Ah, nevermind.", 255);
                            dialog.Send();
                            return;
                        }
                        else
                        {
                            member.Rank = GuildMemberRank.DeputyLeader;
                            if (member.IsOnline)
                            {
                                client.Guild.SendGuild(member.Client);
                                member.Client.Entity.GuildRank = (ushort)member.Rank;
                                member.Client.Screen.FullWipe();
                                member.Client.Screen.Reload(null);
                            }
                            dialog.Text("You have promoted " + member.Name + " to be a Deputy Leader.");
                            dialog.Option("Great!", 255);
                            dialog.Send();
                            client.Guild.RanksCounts[(ushort)GuildMemberRank.DeputyLeader]++;
                            EntityTable.UpdateGuildRank(member.ID, member.Rank);
                        }
                        break;
                    }
                case 17:
                    {
                        if (client.Guild != null && client.AsMember.Rank == GuildMemberRank.GuildLeader && client.Entity.ConquerPoints >= 215)
                        {
                            dialog.Text("Name your guild. The name must be less than 16 characters.");
                            dialog.Text("This will cost 215 Conquer Points.");
                            dialog.Input("Enter new guild name:", 18, 16);
                            dialog.Option("Cancel", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You don't meet the requirements. You must be a Guild Leader and have at least 215 Conquer Points.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 18:
                    {
                        if (client.Guild != null && client.AsMember.Rank == GuildMemberRank.GuildLeader && client.Entity.ConquerPoints >= 215)
                        {
                            if (npcRequest.Input != "" && npcRequest.Input.Length < 16)
                            {
                                if (!Guild.CheckNameExist(npcRequest.Input))
                                {
                                    GuildTable.ChangeName(client, npcRequest.Input);
                                    client.Guild.Name = npcRequest.Input;
                                    client.Guild.SendGuild(client);
                                    client.Guild.SendName(client);
                                    client.Screen.FullWipe();
                                    client.Screen.Reload(null);
                                }
                                else
                                {
                                    dialog.Text("Sorry, there is already a guild with this name.");
                                    dialog.Option("Choose Another Name", 1);
                                    dialog.Option("Cancel", 255);
                                    dialog.Send();
                                }
                            }
                            else
                            {
                                dialog.Text("Invalid guild name. The name must be between 1 and 15 characters.");
                                dialog.Option("Try Again", 1);
                                dialog.Option("Cancel", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("Error: You must be in a guild, be the Guild Leader, and have at least 215 Conquer Points.");
                            dialog.Option("Try Again", 1);
                            dialog.Option("Cancel", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 9:
                    {
                        if (client.Guild != null && client.AsMember.Rank == GuildMemberRank.GuildLeader)
                        {
                            dialog.Text("Are you sure you want to disband your guild? This action cannot be undone!");
                            dialog.Option("Yes, disband my guild", 10);
                            dialog.Option("No, cancel", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You don't meet the requirements. You must be a Guild Leader.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 10:
                    {
                        if (client.Guild != null && client.AsMember.Rank == GuildMemberRank.GuildLeader)
                        {
                            client.Guild.Disband();
                        }
                        break;
                    }
                case 7:
                    {
                        if (client.Guild != null && client.AsMember.Rank == GuildMemberRank.GuildLeader)
                        {
                            if (npcRequest.Input != "")
                            {
                                string lookingFor = npcRequest.Input.Replace(" ", "").Replace("~", "");
                                var member = client.Guild.Members.Values.FirstOrDefault((p) => p.Name.Replace(" ", "").Replace("~", "") == lookingFor);

                                if (member == null)
                                {
                                    dialog.Text("There is no such member in your guild.");
                                    dialog.Option("Ah, nevermind.", 255);
                                    dialog.Send();
                                    return;
                                }
                                else
                                {
                                    if (member.Rank == GuildMemberRank.GuildLeader)
                                    {
                                        dialog.Text("You cannot promote this member anymore.");
                                        dialog.Option("Ah, nevermind.", 255);
                                        dialog.Send();
                                        return;
                                    }
                                    else
                                    {
                                        client.Entity.GuildBattlePower = 0;
                                        client.AsMember.Rank = member.Rank;
                                        EntityTable.UpdateGuildRank(client.Entity.UID, member.Rank);
                                        member.Rank = GuildMemberRank.GuildLeader;
                                        EntityTable.UpdateGuildRank(member.ID, member.Rank);
                                        if (member.IsOnline)
                                        {
                                            var memberClient = member.Client;
                                            member.Client.Entity.GuildBattlePower = 0;
                                            memberClient.Entity.GuildRank = (ushort)member.Rank;
                                            memberClient.Screen.FullWipe();
                                            memberClient.Screen.Reload(null);
                                            memberClient.Guild.SendGuild(memberClient);
                                        }
                                        client.Entity.GuildRank = (ushort)client.AsMember.Rank;
                                        client.Screen.FullWipe();
                                        client.Screen.Reload(null);
                                        client.Guild.SendGuild(client);
                                        client.Guild.GetMaxSharedBattlepower(true);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case 4:
                    {
                        if (client.Guild != null && client.AsMember.Rank == GuildMemberRank.GuildLeader)
                        {
                            if (npcRequest.Input != "")
                            {
                                string lookingFor = npcRequest.Input.Replace(" ", "~");
                                var member = client.Guild.Members.Values.FirstOrDefault((p) => p.Name == lookingFor);

                                if (member == null)
                                {
                                    dialog.Text("There is no such member in your guild.");
                                    dialog.Option("Ah, nevermind.", 255);
                                    dialog.Send();
                                    return;
                                }
                                else
                                {
                                    if (member.Rank != GuildMemberRank.Member)
                                    {
                                        dialog.Text("You cannot promote this member anymore.");
                                        dialog.Option("Ah, nevermind.", 255);
                                        dialog.Send();
                                        return;
                                    }
                                    else
                                    {
                                        member.Rank = GuildMemberRank.DeputyLeader;
                                        if (member.IsOnline)
                                        {
                                            client.Guild.SendGuild(member.Client);
                                            member.Client.Entity.GuildRank = (ushort)member.Rank;
                                            member.Client.Screen.FullWipe();
                                            member.Client.Screen.Reload(null);
                                            member.Client.Entity.GuildBattlePower = member.Guild.GetSharedBattlepower(member.Rank);
                                        }
                                        client.Guild.RanksCounts[(ushort)GuildMemberRank.DeputyLeader]++;
                                    }
                                }
                            }
                            else
                            {
                                dialog.Text("Please enter the name of the member you want to promote to Deputy Leader.");
                                dialog.Option("I understand", 255);
                                dialog.Send();
                            }
                        }
                        else
                        {
                            dialog.Text("You are not the Guild Leader of your current guild.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 1:
                    {
                        if (client.Guild == null && client.Entity.Level >= 90 && client.Entity.Money >= 500000)
                        {
                            dialog.Text("Name your guild. The name must be between 1 and 15 characters.");
                            dialog.Input("Guild name:", 2, 16);
                            dialog.Option("Cancel", 255);
                            dialog.Send();
                        }
                        else
                        {
                            dialog.Text("You don't meet the requirements. You need to be level 90, have 500,000 silver, and not be in any guild.");
                            dialog.Option("I understand", 255);
                            dialog.Send();
                        }
                        break;
                    }
                case 2:
                    {
                        if (client.Guild == null && client.Entity.Level >= 90 && client.Entity.Money >= 500000)
                        {
                            if (npcRequest.Input != "" && npcRequest.Input.Length >= 1 && npcRequest.Input.Length < 16)
                            {
                                if (!Guild.CheckNameExist(npcRequest.Input))
                                {
                                    client.Entity.Money -= 500000;
                                    Guild guild = new Guild(client.Entity.Name);
                                    guild.ID = Guild.GuildCounter.Next;
                                    guild.SilverFund = 1000000;
                                    client.AsMember = new MTA.Game.ConquerStructures.Society.Guild.Member(guild.ID)
                                    {
                                        SilverDonation = 500000,
                                        ID = client.Entity.UID,
                                        Level = client.Entity.Level,
                                        Name = client.Entity.Name,
                                        Rank = GuildMemberRank.GuildLeader,
                                    };
                                    if (client.NobilityInformation != null)
                                    {
                                        client.AsMember.Gender = client.NobilityInformation.Gender;
                                        client.AsMember.NobilityRank = client.NobilityInformation.Rank;
                                    }

                                    client.Entity.GuildID = (ushort)guild.ID;
                                    client.Entity.GuildRank = (ushort)GuildMemberRank.GuildLeader;
                                    guild.Leader = client.AsMember;
                                    client.Guild = guild;
                                    guild.Create(npcRequest.Input);
                                    EntityTable.UpdateGuildID(client);
                                    EntityTable.UpdateGuildRank(client);
                                    guild.Name = npcRequest.Input;
                                    guild.MemberCount++;
                                    guild.SendGuild(client);
                                    guild.SendName(client);
                                    Database.GuildArsenalTable.Insert(guild.ID);
                                    client.Screen.FullWipe();
                                    client.Screen.Reload(null);
                                }
                                else
                                {
                                    dialog.Text("Sorry, there is already a guild with this name. Please choose a different name.");
                                    dialog.Option("Choose Another Name", 1);
                                    dialog.Option("Cancel", 255);
                                    dialog.Send();
                                }
                            }
                        }
                        break;
                    }
            }
        }
    }
}

