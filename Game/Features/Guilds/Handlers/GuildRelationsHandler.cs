using System;
using System.Drawing;
using System.Linq;
using System.Text;
using MTA.Client;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Packets;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;

namespace MTA.Game.Features.Guilds.Handlers;

public static class GuildRelationsHandler {
    public static void HandleAllied(GuildCommand command, byte[] packet, GameState client) {
        var name = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is { Guild: not null, AsMember.Rank: MemberRank.GuildLeader } &&
            client.Guild.Ally.Count < client.Guild.GetMaxAllies())
            AllyGuilds(name, client);
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

    private static void AllyGuilds(string name, GameState client) {
        foreach (var guild in Kernel.Guilds.Values.Where(guild => guild.Name == name && client.Guild!.Name != name))
            if (guild.Leader != null && Kernel.TryGetPlayer(guild.Leader.Id, out var guildLeaderClient)) {
                guildLeaderClient.OnMessageBoxEventParams = [
                    guild,
                    client.Guild
                ];
                client.OnMessageBoxEventParams = [
                    guild,
                    client.Guild
                ];
                var leader = guildLeaderClient;
                leader.MessageOK = delegate {
                    if (leader.OnMessageBoxEventParams[0] is not Guild guild1 ||
                        leader.OnMessageBoxEventParams[1] is not Guild guild2)
                        return;

                    if (guild1.Ally.Count == 6 || guild2.Ally.Count == 6)
                        return;

                    guild1.AddAlly(guild2.Name);
                    guild2.AddAlly(guild1.Name);

                    if (guild1.Leader == null ||
                        !Kernel.TryGetPlayer(guild1.Leader.Id, out var guild1Leader) ||
                        !guild1Leader.Socket.Alive) return;

                    if (guild2.Leader != null &&
                        Kernel.TryGetPlayer(guild2.Leader.Id, out var guild2Leader) &&
                        guild2Leader.Socket.Alive)
                        guild2Leader.Send(new Message(
                            $"{guild1.Leader.Name} has accepted your alliance request.", Color.Blue,
                            Message.TopLeft));
                };
                guildLeaderClient.MessageCancel = delegate {
                    try {
                        if (!guildLeaderClient.Socket.Alive) return;
                        if (guildLeaderClient.OnMessageBoxEventParams[0] is not Guild guild1)
                            return;

                        if (guildLeaderClient.OnMessageBoxEventParams[1] is Guild { Leader: not null } guild2 &&
                            Kernel.TryGetPlayer(guild2.Leader.Id, out var guild2LeaderClient))
                            guild2LeaderClient.Send(new Message(
                                $"{guild1.Leader!.Name} has declined your alliance request.",
                                Color.Blue, Message.TopLeft));
                    }
                    catch (Exception e) {
                        Program.SaveException(e);
                    }
                };
                guildLeaderClient.Send(new NpcReply(
                    NpcReply.MessageBox,
                    $"{client.Entity.Name}, the Guild Leader of {client.Guild.Name}, wants to form an alliance with your guild."
                ));
            }
    }
}