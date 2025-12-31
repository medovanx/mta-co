using System;
using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;

namespace MTA.Game.Features.Guilds;

public static class GuildHelpers {
    public static void AllyGuilds(string name, GameState client) {
        foreach (var guild in Kernel.Guilds.Values.Where(guild => guild.Name == name && client.Guild.Name != name))
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
                    var guild1 =
                        leader.OnMessageBoxEventParams[0] as Guild;
                    var guild2 =
                        leader.OnMessageBoxEventParams[1] as Guild;
                    if (guild1.Ally.Count == 6 || guild2.Ally.Count == 6)
                        return;
                    guild1.AddAlly(guild2.Name);
                    guild2.AddAlly(guild1.Name);

                    if (!Kernel.TryGetPlayer(guild1.Leader.Id, out var guild1Leader) ||
                        !guild1Leader.Socket.Alive) return;
                    if (Kernel.TryGetPlayer(guild2.Leader.Id, out var guild2Leader) &&
                        guild2Leader.Socket.Alive)
                        guild2Leader.Send(new Message(
                            $"{guild1.Leader.Name} has accepted your ally request.", Color.Blue,
                            Message.TopLeft));
                };
                guildLeaderClient.MessageCancel = delegate {
                    try {
                        if (!guildLeaderClient.Socket.Alive) return;
                        var guild1 =
                            guildLeaderClient.OnMessageBoxEventParams[0] as Guild;

                        if (guildLeaderClient.OnMessageBoxEventParams[1] is Guild guild2 &&
                            Kernel.TryGetPlayer(guild2.Leader.Id, out var guild2LeaderClient))
                            guild2LeaderClient.Send(new Message(
                                $"{guild1.Leader.Name} has declined your ally request.",
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

    public static bool PassJoinRequirements(GameState client, Guild guild) {
        var cmd = new GuildCommand(true) {
            Type = GuildCommand.GuildRequirements,
            dwParam2 = guild.LevelRequirement,
            dwParam3 = guild.RebornRequirement,
            dwParam4 = guild.ClassRequirement
        };
        if ((client.Entity.Class is < 10 or > 15 || guild.AllowTrojans) &&
            (client.Entity.Class is < 20 or > 25 || guild.AllowWarriors) &&
            (client.Entity.Class is < 40 or > 45 || guild.AllowArchers) &&
            (client.Entity.Class is < 50 or > 55 || guild.AllowNinjas) &&
            (client.Entity.Class is < 60 or > 65 || guild.AllowMonks) &&
            (client.Entity.Class is < 70 or > 75 || guild.AllowPirates) &&
            (client.Entity.Class is < 100 or > 190 || guild.AllowTaoists) &&
            client.Entity.Reborn >= guild.RebornRequirement &&
            client.Entity.Level >= guild.LevelRequirement) return true;
        client.Send(cmd);
        return false;
    }
}