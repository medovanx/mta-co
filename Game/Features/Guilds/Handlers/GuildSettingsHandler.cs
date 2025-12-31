using System;
using System.Text;
using MTA.Client;
using MTA.Game.Features.Guilds.Database;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;
using Writer = MTA.Network.Writer;

namespace MTA.Game.Features.Guilds.Handlers;

[PacketHandler(1058)]
public static class GuildSettingsHandler {
    public static void HandleInfo(GuildCommand command, GameState client) {
        if (Kernel.Guilds.TryGetValue(command.dwParam, out var guild)) guild.SendName(client);
    }

    public static void HandleChangeRequirements(GuildCommand command, GameState client) {
        if (client.AsMember!.Rank != Enums.GuildMemberRank.GuildLeader) return;
        client.Guild!.LevelRequirement = Math.Min(command.dwParam2, 140);
        client.Guild.RebornRequirement = Math.Min(command.dwParam3, 2);
        client.Guild.ClassRequirement = Math.Min(command.dwParam4, 127);
        foreach (var member in client.Guild.Members.Values)
            if (Kernel.TryGetPlayer(member.Id, out var memberClient))
                client.Guild.SendGuild(memberClient);
        GuildTable.SaveRequirements(client.Guild);
    }

    public static void HandleBulletin(GuildCommand command, byte[] packet, GameState client) {
        var message = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is not { Guild: not null, AsMember.Rank: Enums.GuildMemberRank.GuildLeader }) return;
        client.Guild.Bulletin = message;
        client.Guild.CreateBulletinTime();
        client.Guild.SendGuild(client);
        GuildTable.UpdateBulletin(client.Guild, client.Guild.Bulletin);
    }

    public static void HandleRefresh(GameState client) {
        if (client is { AsMember: not null, Guild: not null }) client.Guild.SendGuild(client);
    }

    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        if (client is not { Guild: not null, AsMember: not null }) return true;
        if (client.AsMember != null) {
            Writer.WriteUInt64(client.AsMember.SilverDonation, 8, packet);
            Writer.WriteUInt32((uint)client.AsMember.ConquerPointDonation, 12, packet);
            Writer.WriteUInt32(client.AsMember.ArsenalDonation, 24, packet);
            Writer.WriteUInt32(client.AsMember.PkDonation, 20, packet);
            Writer.WriteUInt32(client.AsMember.Roses, 28, packet);
            Writer.WriteUInt32(client.AsMember.Tulips, 32, packet);
            Writer.WriteUInt32(client.AsMember.Lilies, 36, packet);
            Writer.WriteUInt32(client.AsMember.Orchids, 40, packet);
            Writer.WriteUInt32(client.AsMember.Orchids
                               + client.AsMember.Roses
                               + client.AsMember.Tulips
                               + client.AsMember.Lilies, 44, packet);
        }

        Writer.WriteUInt32(0, 16, packet); //history donation
        client.Guild?.SendGuild(client);
        client.Send(packet);

        return true;
    }
}