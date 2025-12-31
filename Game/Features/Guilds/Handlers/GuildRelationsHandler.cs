using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Game.Features.Guilds.Database;
using MTA.Network.GamePackets;

namespace MTA.Game.Features.Guilds.Handlers;

public static class GuildRelationsHandler {
    public static void HandleAllied(GuildCommand command, byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is {
                Guild.Ally.Count: < 15, AsMember.Rank: Enums.GuildMemberRank.GuildLeader
            })
            GuildHelpers.AllyGuilds(name, client);
    }

    public static void HandleEnemied(GuildCommand command, byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is {
                Guild.Enemy.Count: < 15, AsMember.Rank: Enums.GuildMemberRank.GuildLeader
            })
            client.Guild!.AddEnemy(name);
    }

    public static void HandleNeutral(GuildCommand command, byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is not { Guild: not null, AsMember.Rank: Enums.GuildMemberRank.GuildLeader }) return;
        client.Guild.RemoveAlly(name);
        foreach (var guild in Kernel.Guilds.Values.Where(guild => guild.Name == name && client.Guild.Name != name))
            guild.RemoveAlly(client.Guild.Name);

        // Check if this is an enemy removal and verify authorization
        var targetGuild = Kernel.Guilds.Values.FirstOrDefault(g => g.Name == name);
        if (targetGuild != null && client.Guild.Enemy.ContainsKey(targetGuild.Id))
            // This is an enemy removal - check if this guild is the initiator
            if (!GuildTable.IsEnemyInitiator(client.Guild, targetGuild.Id)) {
                client.MessageBox(
                    "You cannot remove this enemy relationship. Only the guild that initiated it can remove it.");
                return;
            }

        client.Guild.RemoveEnemy(name);
    }
}