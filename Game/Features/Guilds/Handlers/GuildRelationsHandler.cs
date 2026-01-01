using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Packets;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.Guilds.Handlers;

public static class GuildRelationsHandler {
    public static void HandleAllied(GuildCommand command, byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is { Guild: not null, AsMember.Rank: MemberRank.GuildLeader } &&
            client.Guild.Ally.Count < client.Guild.GetMaxAllies())
            GuildHelpers.AllyGuilds(name, client);
    }

    public static void HandleEnemied(GuildCommand command, byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is { Guild: not null, AsMember.Rank: MemberRank.GuildLeader } &&
            client.Guild.Enemy.Count < client.Guild.GetMaxEnemies())
            client.Guild.AddEnemy(name);
    }

    public static void HandleNeutral(GuildCommand command, byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is not { Guild: not null, AsMember.Rank: MemberRank.GuildLeader }) return;
        client.Guild.RemoveAlly(name);
        foreach (var guild in Kernel.Guilds.Values.Where(guild => guild.Name == name && client.Guild.Name != name))
            guild.RemoveAlly(client.Guild.Name);

        // Remove enemy relationship if it exists (any guild can remove their own enemies)
        var targetGuild = Kernel.Guilds.Values.FirstOrDefault(g => g.Name == name);
        if (targetGuild != null && client.Guild.Enemy.ContainsKey(targetGuild.Id))
            client.Guild.RemoveEnemy(name);
    }
}