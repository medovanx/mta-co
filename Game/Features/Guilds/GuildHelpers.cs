using System;
using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;

namespace MTA.Game.Features.Guilds;

public static class GuildHelpers {
    public static void AllyGuilds(string name, GameState client) {
        if (client.Guild == null) return;
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

    public static bool PassJoinRequirements(GameState client, Guild guild) {
        var cmd = new GuildCommand(true) {
            Type = GuildCommand.GuildRequirements,
            dwParam2 = guild.LevelRequirement,
            dwParam3 = guild.RebornRequirement,
            dwParam4 = guild.ClassRequirement
        };
        if ((!EntityClass.IsTrojan(client.Entity.Class) || guild.AllowTrojans) &&
            (!EntityClass.IsWarrior(client.Entity.Class) || guild.AllowWarriors) &&
            (!EntityClass.IsArcher(client.Entity.Class) || guild.AllowArchers) &&
            (!EntityClass.IsNinja(client.Entity.Class) || guild.AllowNinjas) &&
            (!EntityClass.IsMonk(client.Entity.Class) || guild.AllowMonks) &&
            (!EntityClass.IsPirate(client.Entity.Class) || guild.AllowPirates) &&
            (!EntityClass.IsTaoist(client.Entity.Class) || guild.AllowTaoists) &&
            client.Entity.Reborn >= guild.RebornRequirement &&
            client.Entity.Level >= guild.LevelRequirement) return true;
        client.Send(cmd);
        return false;
    }
}