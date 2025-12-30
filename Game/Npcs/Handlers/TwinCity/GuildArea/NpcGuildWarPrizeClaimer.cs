using System;
using System.Collections.Generic;
using MTA.Client;
using MTA.Database;
using MTA.Game.Events;
using MTA.Game.Events.GuildWar;
using MTA.Network.GamePackets;
using static MTA.Game.Enums;

namespace MTA.Game.Npcs.Handlers.TwinCity.GuildArea {
    /// <summary>
    /// Guild War Prize Claimer - Handles prize claiming for guild war winners
    /// </summary>
    [NpcHandler(19)]
    public static class NpcGuildWarPrizeClaimer {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text("Hello, how may I help you?");

                    // Only show claim options to eligible players and when guild war is not active
                    var isWarActive = EventScheduler.GetEvent("GUILD_WAR") is GuildWarEvent { IsActive: true };
                    if (!isWarActive) {
                        var latest = GuildWarHistoryTable.GetLatest();
                        if (latest != null && client.Guild != null && latest.GuildId == client.Guild.ID) {
                            var member = client.Guild.Members.GetValueOrDefault(client.Entity.UID);
                            if (member != null) {
                                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                                switch (member.Rank) {
                                    case GuildMemberRank.GuildLeader when
                                        latest.GuildLeaderEntityId == client.Entity.UID &&
                                        !latest.GuildLeaderClaimed:
                                        dialog.Option("Claim Top Guild Leader.", 1);
                                        break;
                                    case GuildMemberRank.DeputyLeader when
                                        !GuildWarHistoryTable.HasDeputyClaimed(latest.Id, client.Entity.UID) &&
                                        GuildWarHistoryTable.CanDeputyClaim(latest.Id):
                                        dialog.Option("Claim Top Deputy Leader.", 2);
                                        break;
                                }

                                // Show Top Member Leader option for deputy leaders of winner guild
                                if (member.Rank == GuildMemberRank.DeputyLeader) {
                                    dialog.Option("Claim Top Member Leader.", 7);
                                }
                            }
                        }
                    }

                    dialog.Option("View Last 3 Guild War Wins.", 5);
                    dialog.Send();
                    break;
                }
                case 1: {
                    const int prizeAmount = 3000;
                    // Check if already claimed
                    var latest = GuildWarHistoryTable.GetLatest()!;
                    if (latest.GuildLeaderClaimed) {
                        dialog.Text("You have already claimed the top guild leader prize.");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                        break;
                    }

                    // Process the claim
                    GuildWarHistoryTable.SetGuildLeaderClaimed(latest.Id);
                    client.Entity.ConquerPoints += prizeAmount;
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopGuildLeader, 1,
                        DateTime.Now.AddDays(7));
                    Kernel.SendWorldMessage(
                        new Message(
                            $"Well done! {client.Entity.Name}, leader of {client.Guild!.Name}, has claimed the Guild War top prize of {prizeAmount} CPs!",
                            System.Drawing.Color.White, Message.TopLeft), Program.Values);

                    break;
                }
                case 2: {
                    // Check if already claimed
                    var latest = GuildWarHistoryTable.GetLatest()!;
                    if (GuildWarHistoryTable.HasDeputyClaimed(latest.Id, client.Entity.UID)) {
                        dialog.Text("You have already claimed the deputy leader prize.");
                        dialog.Option("Okay.", 255);
                        dialog.Send();
                        break;
                    }

                    // Process the claim
                    GuildWarHistoryTable.AddDeputyClaim(latest.Id, client.Entity.UID);
                    client.Entity.AddTopStatus(Network.GamePackets.Update.Flags.TopDeputyLeader, 1,
                        DateTime.Now.AddDays(7));
                    Kernel.SendWorldMessage(
                        new Message(
                            $"Well done! {client.Entity.Name} from {client.Guild!.Name} has successfully claimed the Top Deputy Leader halo!",
                            System.Drawing.Color.White, Message.TopLeft), Program.Values);
                    break;
                }
                case 5: {
                    var lastWins = GuildWarHistoryTable.GetLastNWins(3);
                    if (lastWins.Count == 0) {
                        dialog.Text("No guild war history found.");
                    }
                    else {
                        var text = "Last 3 Guild War Winners:\n\n";
                        for (var i = 0; i < lastWins.Count; i++) {
                            var win = lastWins[i];
                            var guild = Kernel.Guilds.GetValueOrDefault(win.GuildId);
                            var guildName = guild != null ? guild.Name : "[DISBANDED GUILD]";
                            var leaderName = !string.IsNullOrEmpty(win.GuildLeaderName)
                                ? win.GuildLeaderName
                                : "Unknown Player";
                            var dateStr = win.WarEndTime.ToString("yyyy-MM-dd HH:mm");

                            text += $"{i + 1}. {guildName}\n";
                            text += $"   Leader: {leaderName}\n";
                            text += $"   Date: {dateStr}\n\n";
                        }

                        dialog.Text(text);
                    }

                    dialog.Option("Okay.", 255);

                    dialog.Send();
                    break;
                }
                case 7: {
                    if (
                        client.Guild != null &&
                        client.Guild.Members.GetValueOrDefault(client.Entity.UID)?.Rank ==
                        GuildMemberRank.DeputyLeader &&
                        client.Guild.PoleKeeper) {
                        client.Entity.AddTopStatus((int)TitlePacket.Titles.membmerguild, 0,
                            DateTime.Now.AddDays(7));
                        Kernel.SendWorldMessage(
                            new Message(
                                "Congratulations! " + client.Entity.Name + " From " +
                                client.Guild.Name + " Has Claimed TopMemberLeader Title!",
                                System.Drawing.Color.White, Message.TopLeft), Program.Values);

                        dialog.Text("Congratulations! You have successfully claimed the Top Member Leader title!");
                        dialog.Option("Thank you!", 255);
                        dialog.Send();
                    }
                    else {
                        dialog.Text(
                            "Sorry you don't have Any Prize to claim only Member of the Winner Guild Can claim the halo After GW end.");
                        dialog.Option("Ahh.", 255);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}