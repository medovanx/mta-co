using MTA.Client;
using MTA.Database;
using MTA.Game.ConquerStructures.Society;
using MTA.Network.GamePackets;
using static MTA.Game.Enums;

namespace MTA.Game.Npcs.Handlers.TwinCity {
    /// <summary>
    /// Guild Director - Handles guild creation, management, and administration
    /// </summary>
    [NpcHandler(10003)]
    public static class NpcGuildDirector {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            const uint guildCost = 1000000;
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        $"Hello there. Do you want to create a new guild? You need to be level 90, have {guildCost:N0} silver, and not belong to any guild. If you are a guild leader, you can manage your guild here.");
                    if (client.Guild == null) {
                        dialog.Option("Create a Guild", 1);
                    }

                    if (client is { Guild: not null, AsMember.Rank: GuildMemberRank.GuildLeader }) {
                        dialog.Option("Disband my Guild", 3);
                        dialog.Option("Change my Guild Name", 5);
                    }

                    dialog.Option("Nevermind", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    dialog.Text("Name your guild. The name must be between 1 and 15 characters.");
                    dialog.Input("Guild name:", 2, 16);
                    dialog.Option("Cancel", 255);
                    dialog.Send();
                    break;
                }
                case 2: {
                    if (client.Guild == null && client.Entity is { Level: >= 90, Money: >= guildCost }) {
                        if (npcRequest.Input != "" && npcRequest.Input.Length is >= 1 and <= 16) {
                            if (!Guild.CheckNameExist(npcRequest.Input)) {
                                client.Entity.Money -= guildCost;
                                var guild = new Guild(client.Entity.Name) {
                                    ID = Guild.GuildCounter.Next,
                                    SilverFund = guildCost
                                };
                                client.AsMember = new Guild.Member(guild.ID) {
                                    SilverDonation = 500000,
                                    ID = client.Entity.UID,
                                    Level = client.Entity.Level,
                                    Name = client.Entity.Name,
                                    Rank = GuildMemberRank.GuildLeader,
                                };
                                if (client.NobilityInformation != null) {
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
                                GuildArsenalTable.Insert(guild.ID);
                                client.Screen.FullWipe();
                                client.Screen.Reload();
                                Kernel.SendWorldMessage(
                                    new Message(
                                        $"A new guild [{npcRequest.Input}] has been created by {client.Entity.Name}!",
                                        System.Drawing.Color.Red, Message.Center),
                                    Program.Values);
                            }
                            else {
                                dialog.Text(
                                    "Sorry, there is already a guild with this name. Please choose a different name.");
                                dialog.Option("Choose another name", 1);
                                dialog.Option("Cancel", 255);
                                dialog.Send();
                            }
                        }
                    }
                    else {
                        dialog.Text(
                            $"You don't meet the requirements. You need to be level 90, have {guildCost:N0} silver, and not be in any guild.");
                        dialog.Option("I understand", 255);
                        dialog.Send();
                    }

                    break;
                }
                case 3: {
                    dialog.Text("Are you sure you want to disband your guild? This action cannot be undone!");
                    dialog.Option("Yes, disband my guild", 4);
                    dialog.Option("No, cancel", 255);
                    dialog.Send();
                    break;
                }
                case 4: {
                    if (client is { Guild: not null, AsMember.Rank: GuildMemberRank.GuildLeader }) {
                        var guildName = client.Guild.Name;
                        client.Guild.Disband();
                        Kernel.SendWorldMessage(
                            new Message(
                                $"The guild [{guildName}] has been disbanded by {client.Entity.Name}.",
                                System.Drawing.Color.Red, Message.Center),
                            Program.Values);
                    }

                    break;
                }
                case 5: {
                    dialog.Text($"Name your guild. The name must be less than 16 characters.\nThis will cost 215 CPs.");
                    dialog.Input("Enter new guild name:", 6, 16);
                    dialog.Option("Cancel", 255);
                    dialog.Send();
                    break;
                }
                case 6: {
                    if (client is {
                            Guild: not null, AsMember.Rank: GuildMemberRank.GuildLeader, Entity.ConquerPoints: >= 215
                        }) {
                        if (npcRequest.Input != "" && npcRequest.Input.Length is >= 1 and <= 16) {
                            if (!Guild.CheckNameExist(npcRequest.Input)) {
                                var oldGuildName = client.Guild.Name;
                                client.Entity.ConquerPoints -= 215;
                                GuildTable.ChangeName(client, npcRequest.Input);
                                client.Guild.Name = npcRequest.Input;
                                client.Guild.SendGuild(client);
                                client.Guild.SendName(client);
                                client.Screen.FullWipe();
                                client.Screen.Reload();
                                Kernel.SendWorldMessage(
                                    new Message(
                                        $"The guild [{oldGuildName}] has been renamed to [{npcRequest.Input}] by {client.Entity.Name}.",
                                        System.Drawing.Color.Red, Message.Center),
                                    Program.Values);
                            }
                            else {
                                dialog.Text("Sorry, there is already a guild with this name.");
                                dialog.Option("Choose Another Name", 5);
                                dialog.Option("Cancel", 255);
                                dialog.Send();
                            }
                        }
                        else {
                            dialog.Text("Invalid guild name. The name must be between 1 and 15 characters.");
                            dialog.Option("Try Again", 5);
                            dialog.Option("Cancel", 255);
                            dialog.Send();
                        }
                    }
                    else {
                        dialog.Text(
                            "You must be in a guild, be the Guild Leader, and have at least 215 Conquer Points.");
                        dialog.Option("Try Again", 5);
                        dialog.Option("Cancel", 255);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}