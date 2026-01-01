using System;
using MTA.Client;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Packets;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.Guilds.Handlers;

public static class GuildJoinHandler {
    public static void HandleJoinRequest(GuildCommand command, GameState client) {
        if (!Kernel.GamePool.TryGetValue(command.DwParam, out var target)) return;
        client.GuildJoinTarget = target.Entity.UID;
        if (client.GuildJoinTarget == target.Entity.UID &&
            target.GuildJoinTarget == client.Entity.UID) {
            client.GuildJoinTarget = 0;
            target.GuildJoinTarget = 0;

            if (target.Guild!.BlackList.Contains(client.Entity.UID)) {
                command.Type = 47;
                client.Send(command);
                return;
            }

            if (!Kernel.Guilds.TryGetValue(target.Entity.GuildID, out var g)) return;
            if (target.AsMember!.Rank == MemberRank.Member) return;
            if (client.Entity.GuildID == 0)
                g.AddMember(client);
        }
        else {
            if (!Kernel.Guilds.TryGetValue(target.Entity.GuildID, out var tG)) return;
            if (target.AsMember!.Rank == MemberRank.Member) return;
            if (target.Guild!.BlackList.Contains(client.Entity.UID)) {
                command.Type = 47;
                client.Send(command);
                return;
            }

            if (!GuildHelpers.PassJoinRequirements(client, tG)) return;
            client.Entity.GuildRequest = Time32.Now;
            command.DwParam = client.Entity.UID;

            var inf = new PopupLevelandBP {
                Level = client.Entity.Level,
                BattlePower = (uint)client.Entity.BattlePower,
                Receiver = target.Entity.UID,
                Requester = client.Entity.UID
            };

            target.Send(inf.ToArray());
            target.Send(command);
        }
    }

    public static void HandleInviteRequest(GuildCommand command, GameState client) {
        if (!Kernel.GamePool.TryGetValue(command.DwParam, out var target)) return;
        client.GuildJoinTarget = target.Entity.UID;
        if (client.GuildJoinTarget == target.Entity.UID &&
            target.GuildJoinTarget == client.Entity.UID) {
            client.GuildJoinTarget = 0;
            target.GuildJoinTarget = 0;

            if (client.Guild!.BlackList.Contains(target.Entity.UID)) {
                command.Type = 49;
                client.Send(command);
                return;
            }

            if (!Kernel.Guilds.TryGetValue(client.Entity.GuildID, out var g)) return;
            if (client.AsMember!.Rank != MemberRank.Member)
                g.AddMember(target);
        }
        else {
            if (client.AsMember!.Rank == MemberRank.Member) return;
            if (client.Guild!.BlackList.Contains(target.Entity.UID)) {
                command.Type = 49;
                client.Send(command);
                return;
            }

            client.Entity.GuildRequest = Time32.Now;
            command.DwParam = client.Entity.UID;
            var inf = new PopupLevelandBP {
                Level = client.Entity.Level,
                BattlePower = (uint)client.Entity.BattlePower,
                Receiver = target.Entity.UID,
                Requester = client.Entity.UID
            };

            target.Send(inf.ToArray());
            target.Send(command);
        }
    }

    public static void HandleQuit(GameState client) {
        if (client is { Guild: not null, AsMember.Rank: not MemberRank.GuildLeader })
            client.Guild.ExpelMember(client.Entity.Name, true);
    }

    public static void HandleExpelMemberViaNpc(string memberName, GameState client) {
        if (client.Guild == null) return;
        if (!client.Guild.Members.TryGetValue(client.Entity.UID, out var clientMember)) return;

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (clientMember.Rank) {
            case MemberRank.GuildLeader:
                client.Guild.ExpelMember(memberName, false);
                break;
            case MemberRank.DeputyLeader: {
                var member = client.Guild.GetMemberByName(memberName);
                if (member != null) {
                    if (member.Rank is MemberRank.DeputyLeader
                        or MemberRank.GuildLeader)
                        return;
                    client.Guild.ExpelMember(memberName, false);
                }

                break;
            }
        }
    }
}