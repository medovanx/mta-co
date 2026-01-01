using System.Linq;
using System.Text;
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

    public static void HandlePromote(GuildCommand command, byte[] packet, GameState client) {
        if (client is not { Guild: not null, AsMember: not null }) return;
        var getMemberName = ReadString(packet, 26, packet[25]);
        var getMemberRank = BitConverter.ToUInt16(packet, 8);

        if (client.Guild.GetMember(getMemberName, out var memberPromote)) {
            if (client.AsMember.Rank < memberPromote!.Rank) {
                client.Entity.SendSysMesage(
                    "Sorry, you have small rank for change he position!");
                return;
            }

            if (client.AsMember.Rank == MemberRank.DeputyLeader)
                switch (getMemberRank) {
                    case (ushort)MemberRank.Steward when client.Guild.RanksCounts[getMemberRank] >= 3:
                        client.Entity.SendSysMesage(
                            "Sorry all Steward`s ranks its ocupated!");
                        return;
                    case (ushort)MemberRank.Steward: {
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]--;
                        memberPromote.Rank = (MemberRank)getMemberRank;
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]++;
                        if (Kernel.TryGetPlayer(memberPromote.Id, out var promoteClient)) {
                            client.Guild.SendGuild(promoteClient);
                            promoteClient.Entity.GuildBattlePower =
                                client.Guild.GetSharedBattlePower(memberPromote.Rank);
                            promoteClient.Entity.GuildRank = (ushort)memberPromote.Rank;
                            promoteClient.Screen.FullWipe();
                            promoteClient.Screen.Reload();
                        }

                        break;
                    }
                    case (ushort)MemberRank.Follower when client.Guild.RanksCounts[getMemberRank] >= 10:
                        client.Entity.SendSysMesage(
                            "Sorry all Follower`s ranks its ocupated!");
                        return;
                    case (ushort)MemberRank.Follower: {
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]--;
                        memberPromote.Rank = (MemberRank)getMemberRank;
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]++;
                        if (Kernel.TryGetPlayer(memberPromote.Id, out var promoteClient)) {
                            client.Guild.SendGuild(promoteClient);
                            promoteClient.Entity.GuildBattlePower =
                                client.Guild.GetSharedBattlePower(memberPromote.Rank);
                            promoteClient.Entity.GuildRank = (ushort)memberPromote.Rank;
                            promoteClient.Screen.FullWipe();
                            promoteClient.Screen.Reload();
                        }

                        break;
                    }
                    case (ushort)MemberRank.Aide when client.Guild.RanksCounts[getMemberRank] >= 6:
                        client.Entity.SendSysMesage("Sorry all Aide`s ranks its ocupated!");
                        return;
                    case (ushort)MemberRank.Aide: {
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]--;
                        memberPromote.Rank = (MemberRank)getMemberRank;
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]++;
                        if (Kernel.TryGetPlayer(memberPromote.Id, out var promoteClient)) {
                            client.Guild.SendGuild(promoteClient);
                            promoteClient.Entity.GuildBattlePower =
                                client.Guild.GetSharedBattlePower(memberPromote.Rank);
                            promoteClient.Entity.GuildRank = (ushort)memberPromote.Rank;
                            promoteClient.Screen.FullWipe();
                            promoteClient.Screen.Reload();
                        }

                        break;
                    }
                }

            if (client.AsMember.Rank is MemberRank.Manager
                or MemberRank.HonoraryManager)
                if (getMemberRank == (ushort)MemberRank.Aide) {
                    if (client.Guild.RanksCounts[getMemberRank] >= 6) {
                        client.Entity.SendSysMesage("Sorry all Aide`s ranks its ocupated!");
                        return;
                    }

                    client.Guild.RanksCounts[(ushort)memberPromote.Rank]--;
                    memberPromote.Rank = (MemberRank)getMemberRank;
                    client.Guild.RanksCounts[(ushort)memberPromote.Rank]++;
                    if (Kernel.TryGetPlayer(memberPromote.Id, out var promoteClient)) {
                        client.Guild.SendGuild(promoteClient);
                        promoteClient.Entity.GuildBattlePower =
                            client.Guild.GetSharedBattlePower(memberPromote.Rank);
                        promoteClient.Entity.GuildRank = (ushort)memberPromote.Rank;
                        promoteClient.Screen.FullWipe();
                        promoteClient.Screen.Reload();
                    }
                }

            if (client.AsMember.Rank is MemberRank.GuildLeader
                or MemberRank.LeaderSpouse)
                switch (getMemberRank) {
                    case (ushort)MemberRank.GuildLeader
                        when client.AsMember.Rank == MemberRank.LeaderSpouse:
                        return;
                    case (ushort)MemberRank.GuildLeader: {
                        memberPromote.Rank = MemberRank.GuildLeader;

                        client.Guild.LeaderId = memberPromote.Id;
                        client.Guild.Leader = memberPromote;
                        client.Guild.LeaderName = memberPromote.Name;
                        if (Kernel.TryGetPlayer(memberPromote.Id, out var promoteClient)) {
                            client.Guild.SendGuild(promoteClient);
                            promoteClient.Entity.GuildBattlePower =
                                client.Guild.GetSharedBattlePower(memberPromote.Rank);
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
                        break;
                    }
                    case (ushort)MemberRank.DeputyLeader when client.Guild.RanksCounts[getMemberRank] >= 6:
                        client.Entity.SendSysMesage(
                            "Sorry all DeputyLeader`s ranks its ocupated!");
                        return;
                    case (ushort)MemberRank.DeputyLeader: {
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]--;
                        memberPromote.Rank = (MemberRank)getMemberRank;
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]++;
                        if (Kernel.TryGetPlayer(memberPromote.Id, out var promoteClient)) {
                            client.Guild.SendGuild(promoteClient);
                            promoteClient.Entity.GuildBattlePower =
                                client.Guild.GetSharedBattlePower(memberPromote.Rank);
                            promoteClient.Entity.GuildRank = (ushort)memberPromote.Rank;
                            promoteClient.Screen.FullWipe();
                            promoteClient.Screen.Reload();
                        }

                        break;
                    }
                    case (ushort)MemberRank.Aide
                        when client.AsMember.Rank == MemberRank.LeaderSpouse:
                        return;
                    case (ushort)MemberRank.Aide when client.Guild.RanksCounts[getMemberRank] >= 6:
                        client.Entity.SendSysMesage("Sorry all Aide`s ranks its ocupated!");
                        return;
                    case (ushort)MemberRank.Aide: {
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]--;
                        memberPromote.Rank = (MemberRank)getMemberRank;
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]++;
                        if (Kernel.TryGetPlayer(memberPromote.Id, out var promoteClient)) {
                            client.Guild.SendGuild(promoteClient);
                            promoteClient.Entity.GuildBattlePower =
                                client.Guild.GetSharedBattlePower(memberPromote.Rank);
                            promoteClient.Entity.GuildRank = (ushort)memberPromote.Rank;
                            promoteClient.Screen.FullWipe();
                            promoteClient.Screen.Reload();
                        }

                        break;
                    }
                    case (ushort)MemberRank.Steward when client.Guild.RanksCounts[getMemberRank] >= 3:
                        client.Entity.SendSysMesage(
                            "Sorry all Steward`s ranks its ocupated!");
                        return;
                    case (ushort)MemberRank.Steward: {
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]--;
                        memberPromote.Rank = (MemberRank)getMemberRank;
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]++;
                        if (Kernel.TryGetPlayer(memberPromote.Id, out var promoteClient)) {
                            client.Guild.SendGuild(promoteClient);
                            promoteClient.Entity.GuildBattlePower =
                                client.Guild.GetSharedBattlePower(memberPromote.Rank);
                            promoteClient.Entity.GuildRank = (ushort)memberPromote.Rank;
                            promoteClient.Screen.FullWipe();
                            promoteClient.Screen.Reload();
                        }

                        break;
                    }
                    case (ushort)MemberRank.Follower when client.Guild.RanksCounts[getMemberRank] >= 10:
                        client.Entity.SendSysMesage(
                            "Sorry all Follower`s ranks its ocupated!");
                        return;
                    case (ushort)MemberRank.Follower:
                    case (ushort)MemberRank.Member: {
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]--;
                        memberPromote.Rank = (MemberRank)getMemberRank;
                        client.Guild.RanksCounts[(ushort)memberPromote.Rank]++;
                        if (Kernel.TryGetPlayer(memberPromote.Id, out var promoteClient)) {
                            client.Guild.SendGuild(promoteClient);
                            promoteClient.Entity.GuildBattlePower =
                                client.Guild.GetSharedBattlePower(memberPromote.Rank);
                            promoteClient.Entity.GuildRank = (ushort)memberPromote.Rank;
                            promoteClient.Screen.FullWipe();
                            promoteClient.Screen.Reload();
                        }

                        break;
                    }
                }

            client.Entity.GuildBattlePower =
                client.Guild.GetSharedBattlePower(client.Entity.GuildRank);
        }
        else {
            client.Entity.SendSysMesage("Sorry Can't Find " + getMemberName);
        }
    }

    private static string ReadString(byte[] data, ushort position, ushort count) {
        return Program.Encoding.GetString(data, position, count);
    }
}