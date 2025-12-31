using System;
using System.Collections.Generic;
using MTA.Client;
using MTA.Database;
using MTA.Game.Events;
using MTA.Game.Events.GuildWar;
using MTA.Game.Features.Guilds.Database;
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
                        if (latest != null && client.Guild != null && latest.GuildId == client.Guild.Id) {
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
                            }
                        }
                    }

                    dialog.Option("View last 3 Guild War winners.", 3);
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
                            $"Well done! [{client.Entity.Name}] from [{client.Guild!.Name}] has claimed the Guild War Leader prize and received {prizeAmount:N0} gold!",
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
                            $"Well done! [{client.Entity.Name}] from [{client.Guild!.Name}] has successfully claimed the Top Deputy Leader prize!",
                            System.Drawing.Color.White, Message.TopLeft), Program.Values);
                    break;
                }
                case 3: {
                    var lastWins = GuildWarHistoryTable.GetLastNWins(3);
                    if (lastWins.Count == 0) {
                        dialog.Text("No guild war history found.");
                    }
                    else {
                        var text = "The last 3 Guild War winners are:\n\n";
                        for (var i = 0; i < lastWins.Count; i++) {
                            var win = lastWins[i];
                            var guild = Kernel.Guilds.GetValueOrDefault(win.GuildId);
                            var guildName = guild != null ? guild.Name : "[DISBANDED GUILD]";
                            var leaderName = !string.IsNullOrEmpty(win.GuildLeaderName)
                                ? win.GuildLeaderName
                                : "Unknown Player";
                            var dateStr = win.WarEndTime.ToString("yyyy-MM-dd HH:mm");

                            text += $"{i + 1}. Guild: {guildName}\n";
                            text += $"   Leader: {leaderName}\n";
                            text += $"   Date: {dateStr}\n\n";
                        }

                        dialog.Text(text);
                    }

                    dialog.Option("Okay.", 255);

                    dialog.Send();
                    break;
                }
            }
        }
    }
}