using System.Linq;
using System.Text;
using System.Drawing;
using MTA.Client;
using MTA.Database;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Database;
using MTA.Game.Features.Guilds.Packets;
using MTA.Network.GamePackets;
using Writer = MTA.Network.Writer;

namespace MTA.Game.Features.Guilds.Handlers;

public static class GuildPromotionHandler {
    public static void HandlePromoteInfo(GuildCommand command, byte[] packet, GameState client) {
        if (client.AsMember!.Rank == MemberRank.GuildLeader) {
            var array2 = client.Guild!.Members.Values.Where(p => p.Rank == (MemberRank)command.DwParam)
                .ToDictionary(p => p.Id);

            var array = array2.Values.ToArray();
            {
                var buffer = new byte[8 + 48 + array.Length * 32];
                Writer.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                Writer.WriteUInt16(2102, 2, buffer);
                Writer.Uint(1, 4, buffer);
                Writer.Uint((uint)array.Length, 12, buffer);
                var offset = 16;
                foreach (var t in array) {
                    Writer.Uint(t.Level, offset, buffer); //level
                    offset += 4;
                    Writer.Uint((uint)(Kernel.TryGetPlayer(t.Id, out _) ? 1 : 0), offset, buffer); //online
                    offset += 4;
                    if (Kernel.TryGetPlayer(t.Id, out var tClient))
                        Writer.Uint((uint)tClient.Entity.BattlePower, offset, buffer); //bp
                    offset += 4;
                    offset += 4;
                    Writer.String(t.Name, offset, buffer);
                    offset += 16;
                }

                client.Send(buffer);
            }
        }

        client.Send(packet);
    }

    public static void HandleRequestPromote(GuildCommand command, GameState client) {
        if (client.Guild == null) return;
        if (client.AsMember == null) return;

        command.SendPromote(client, (ushort)GuildCommand.RequestPromote);
    }

    public static void HandleDischarge(GuildCommand command, byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is not { Guild: not null, AsMember.Rank: MemberRank.GuildLeader }) return;
        var member = client.Guild.GetMemberByName(name);
        if (member?.Id == client.Entity.UID) return;
        if (member?.Rank != MemberRank.DeputyLeader) return;
        client.Guild.RanksCounts[(ushort)MemberRank.DeputyLeader]--;
        member.Rank = MemberRank.Member;
        if (Kernel.TryGetPlayer(member.Id, out var memberClient)) {
            client.Guild.SendGuild(memberClient);
            memberClient.Entity.GuildRank = (ushort)member.Rank;
            memberClient.Screen.FullWipe();
            memberClient.Screen.Reload();
            memberClient.Entity.GuildBattlePower =
                member.Guild.GetSharedBattlePower(member.Rank);
        }

        EntityTable.UpdateData(member.Id, "GuildRank", (int)member.Rank);
    }

    private static void ApplyPromotion(Guild guild, Guild.Member member, MemberRank newRank,
        GameState promotingClient) {
        // Update rank counts
        if (member.Rank != newRank) {
            var oldRankIndex = (ushort)member.Rank;
            var newRankIndex = (ushort)newRank;

            // Bounds check
            if (oldRankIndex < guild.RanksCounts.Length) {
                guild.RanksCounts[oldRankIndex]--;
            }

            member.Rank = newRank;

            // Bounds check
            if (newRankIndex < guild.RanksCounts.Length) {
                guild.RanksCounts[newRankIndex]++;
            }
        }

        // Update online member
        if (Kernel.TryGetPlayer(member.Id, out var promoteClient)) {
            guild.SendGuild(promoteClient);
            promoteClient.Entity.GuildBattlePower = guild.GetSharedBattlePower(member.Rank);
            promoteClient.Entity.GuildRank = (ushort)member.Rank;
            promoteClient.Screen.FullWipe();
            promoteClient.Screen.Reload();
        }

        // Update database
        EntityTable.UpdateData(member.Id, "GuildRank", (int)member.Rank);

        // Refresh member list for the promoting client
        guild.SendMembers(promotingClient, 0);
    }

    public static void HandlePromote(GuildCommand command, byte[] packet, GameState client) {
        if (client is not { Guild: not null, AsMember: not null }) return;

        var getMemberName = ReadString(packet, 26, packet[25]);
        var getMemberRank = BitConverter.ToUInt16(packet, 8);
        var targetRank = (MemberRank)getMemberRank;

        if (!client.Guild.GetMember(getMemberName, out var memberPromote)) {
            client.Send(new Message("Sorry Can't Find " + getMemberName,
                Color.White, Message.System));
            return;
        }

        if (client.AsMember.Rank < memberPromote!.Rank) {
            client.Send(new Message(
                $"Sorry, you have small rank for change position! Your rank: {client.AsMember.Rank} (ID: {(ushort)client.AsMember.Rank}), Member rank: {memberPromote.Rank} (ID: {(ushort)memberPromote.Rank})",
                Color.White, Message.System));
            return;
        }

        // Check if member is already at the target rank
        if (memberPromote.Rank == targetRank) {
            client.Send(new Message($"{memberPromote.Name} is already at that rank!",
                Color.White, Message.System));
            return;
        }

        // Check if trying to promote to Manager or Supervisor (Guild Leader cannot promote to these)
        if (client.AsMember.Rank == MemberRank.GuildLeader &&
            (targetRank == MemberRank.Manager || targetRank == MemberRank.Supervisor)) {
            client.Send(new Message("Guild Leader cannot appoint Manager or Supervisor!",
                Color.White, Message.System));
            return;
        }

        var promotionApplied = false;

        #region Guild Leader Promotions

        if (client.AsMember.Rank == MemberRank.GuildLeader) {
            switch (targetRank) {
                case MemberRank.GuildLeader: {
                    // Transfer leadership
                    memberPromote.Rank = MemberRank.GuildLeader;
                    client.Guild.LeaderId = memberPromote.Id;
                    client.Guild.Leader = memberPromote;
                    client.Guild.LeaderName = memberPromote.Name;

                    if (Kernel.TryGetPlayer(memberPromote.Id, out var promoteClient)) {
                        client.Guild.SendGuild(promoteClient);
                        promoteClient.Entity.GuildBattlePower = client.Guild.GetSharedBattlePower(memberPromote.Rank);
                        promoteClient.Entity.GuildRank = (ushort)memberPromote.Rank;
                        promoteClient.Screen.FullWipe();
                        promoteClient.Screen.Reload();
                    }

                    client.AsMember.Rank = MemberRank.DeputyLeader;
                    client.Entity.GuildRank = (ushort)client.AsMember.Rank;
                    client.Guild.SendGuild(client);
                    client.Screen.FullWipe();
                    client.Screen.Reload();
                    GuildTable.SaveLeader(client.Guild);
                    client.Guild.SendMembers(client, 0);
                    return;
                }
                case MemberRank.DeputyLeader: {
                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxDeputyLeader(client.Guild.Level)) {
                        client.Send(new Message("Sorry all DeputyLeader ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.HDeputyLeader: {
                    // Check CP cost (650 CPs)
                    if (client.Entity.ConquerPoints < 650) {
                        client.Send(new Message("You need 650 Conquer Points to appoint Honorary Deputy Leader!",
                            Color.White, Message.System));
                        return;
                    }

                    var targetRankIndex = (ushort)targetRank;
                    if (targetRankIndex >= client.Guild.RanksCounts.Length) {
                        client.Send(new Message($"Error: Rank index {targetRankIndex} is out of bounds!",
                            Color.White, Message.System));
                        return;
                    }

                    if (client.Guild.RanksCounts[targetRankIndex] >=
                        GuildRankLimits.GetMaxHonoraryDeputyLeader(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Honorary Deputy Leader ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    client.Entity.ConquerPoints -= 650;
                    EntityTable.UpdateData(client.Entity.UID, "ConquerPoints", (int)client.Entity.ConquerPoints);
                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.HonoraryManager: {
                    // Check CP cost (320 CPs)
                    if (client.Entity.ConquerPoints < 320) {
                        client.Send(new Message("You need 320 Conquer Points to appoint Honorary Manager!",
                            Color.White, Message.System));
                        return;
                    }

                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxHonoraryManager(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Honorary Manager ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    client.Entity.ConquerPoints -= 320;
                    EntityTable.UpdateData(client.Entity.UID, "ConquerPoints", (int)client.Entity.ConquerPoints);
                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.HonorarySupervisor: {
                    // Check CP cost (270 CPs)
                    if (client.Entity.ConquerPoints < 270) {
                        client.Send(new Message("You need 270 Conquer Points to appoint Honorary Supervisor!",
                            Color.White, Message.System));
                        return;
                    }

                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxHonorarySupervisor(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Honorary Supervisor ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    client.Entity.ConquerPoints -= 270;
                    EntityTable.UpdateData(client.Entity.UID, "ConquerPoints", (int)client.Entity.ConquerPoints);
                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.HonorarySteward: {
                    // Check CP cost (100 CPs)
                    if (client.Entity.ConquerPoints < 100) {
                        client.Send(new Message("You need 100 Conquer Points to appoint Honorary Steward!",
                            Color.White, Message.System));
                        return;
                    }

                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxHonorarySteward(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Honorary Steward ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    client.Entity.ConquerPoints -= 100;
                    EntityTable.UpdateData(client.Entity.UID, "ConquerPoints", (int)client.Entity.ConquerPoints);
                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.LSpouseAide: {
                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxAide(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Leader Aide ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.Steward: {
                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxSteward(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Steward ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.Follower: {
                    // According to JSON, Followers have no number limitation, but we check against a high limit for safety
                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxFollower(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Follower ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.Member: {
                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                default: {
                    // Guild Leader can promote to all other officials except Manager and Supervisor
                    // Check if target rank is Manager or any Supervisor type
                    if (targetRank == MemberRank.Manager ||
                        targetRank == MemberRank.Supervisor ||
                        targetRank == MemberRank.TSupervisor ||
                        targetRank == MemberRank.OSupervisor ||
                        targetRank == MemberRank.CPSupervisor ||
                        targetRank == MemberRank.ASupervisor ||
                        targetRank == MemberRank.SSupervisor ||
                        targetRank == MemberRank.GSupervisor ||
                        targetRank == MemberRank.PKSupervisor ||
                        targetRank == MemberRank.RoseSupervisor ||
                        targetRank == MemberRank.LilySupervisor) {
                        // Cannot promote to Manager or Supervisor types
                        break;
                    }
                    
                    // Check if rank is below Steward (690) and above Member (200)
                    // This covers DeputySteward, Agents, Aides, Followers, SeniorMember, etc.
                    if ((ushort)targetRank < (ushort)MemberRank.Steward && (ushort)targetRank > (ushort)MemberRank.Member) {
                        // Check rank limits for specific ranks that have limits
                        var targetRankIndex = (ushort)targetRank;
                        if (targetRankIndex < client.Guild.RanksCounts.Length) {
                            // For ranks with no specific limit, allow promotion
                            // Some ranks like DeputySteward, Agent, SeniorMember have no limits per guide
                            ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                            promotionApplied = true;
                        }
                    }
                    break;
                }
            }
        }

        #endregion

        #region Deputy Leader, Honorary Deputy Leader, Leader Spouse Promotions

        if (client.AsMember.Rank == MemberRank.DeputyLeader ||
            client.AsMember.Rank == MemberRank.HDeputyLeader ||
            client.AsMember.Rank == MemberRank.LeaderSpouse) {
            switch (targetRank) {
                case MemberRank.Steward: {
                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxSteward(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Steward ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.HonorarySteward: {
                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxHonorarySteward(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Honorary Steward ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.DLeaderAide: {
                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxAide(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Deputy Leader Aide ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.Follower: {
                    // According to JSON, Followers have no number limitation, but we check against a high limit for safety
                    if (client.Guild.RanksCounts[(ushort)targetRank] >=
                        GuildRankLimits.GetMaxFollower(client.Guild.Level)) {
                        client.Send(new Message("Sorry all Follower ranks are occupied!",
                            Color.White, Message.System));
                        return;
                    }

                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                case MemberRank.Member: {
                    ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                    promotionApplied = true;
                    break;
                }
                // Can also promote to ranks below Steward
                default: {
                    if (targetRank < MemberRank.Steward && targetRank > MemberRank.Member) {
                        ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                        promotionApplied = true;
                    }

                    break;
                }
            }
        }

        #endregion

        #region Manager & Honorary Manager Promotions

        if (client.AsMember.Rank == MemberRank.Manager ||
            client.AsMember.Rank == MemberRank.HonoraryManager) {
            if (targetRank == MemberRank.ManagerAide) {
                if (client.Guild.RanksCounts[(ushort)targetRank] >= GuildRankLimits.GetMaxAide(client.Guild.Level)) {
                    client.Send(new Message("Sorry all Manager Aide ranks are occupied!",
                        Color.White, Message.System));
                    return;
                }

                ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                promotionApplied = true;
            }
        }

        #endregion

        #region Supervisor Promotions

        if (client.AsMember.Rank == MemberRank.Supervisor ||
            client.AsMember.Rank == MemberRank.HonorarySupervisor ||
            client.AsMember.Rank == MemberRank.TSupervisor ||
            client.AsMember.Rank == MemberRank.OSupervisor ||
            client.AsMember.Rank == MemberRank.CPSupervisor ||
            client.AsMember.Rank == MemberRank.ASupervisor ||
            client.AsMember.Rank == MemberRank.SSupervisor ||
            client.AsMember.Rank == MemberRank.GSupervisor ||
            client.AsMember.Rank == MemberRank.PKSupervisor ||
            client.AsMember.Rank == MemberRank.RoseSupervisor ||
            client.AsMember.Rank == MemberRank.LilySupervisor) {
            if (targetRank == MemberRank.SupervisorAide) {
                if (client.Guild.RanksCounts[(ushort)targetRank] >= GuildRankLimits.GetMaxAide(client.Guild.Level)) {
                    client.Send(new Message("Sorry all Supervisor Aide ranks are occupied!",
                        Color.White, Message.System));
                    return;
                }

                ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                promotionApplied = true;
            }
        }

        #endregion

        #region Agent Promotions

        if (client.AsMember.Rank == MemberRank.Agent) {
            if (targetRank == MemberRank.Aide) {
                if (client.Guild.RanksCounts[(ushort)targetRank] >= GuildRankLimits.GetMaxAide(client.Guild.Level)) {
                    client.Send(new Message("Sorry all Aide ranks are occupied!",
                        Color.White, Message.System));
                    return;
                }

                ApplyPromotion(client.Guild, memberPromote, targetRank, client);
                promotionApplied = true;
            }
        }

        #endregion

        if (!promotionApplied) {
            client.Send(new Message(
                $"You don't have permission to promote to {targetRank} (ID: {(ushort)targetRank})! Your rank: {client.AsMember.Rank}, Target member rank: {memberPromote.Rank}",
                Color.White, Message.System));
        }
        else {
            client.Entity.GuildBattlePower = client.Guild.GetSharedBattlePower(client.Entity.GuildRank);
        }
    }

    private static string ReadString(byte[] data, ushort position, ushort count) {
        return Program.Encoding.GetString(data, position, count);
    }
}