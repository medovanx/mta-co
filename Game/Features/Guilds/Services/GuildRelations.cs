using System.Drawing;
using System.Linq;
using MTA.Client;
using MTA.Game.Features.Guilds.Database;
using MTA.Game.Features.Guilds.Models;
using MTA.Game.Features.Guilds.Packets.Writers;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;

namespace MTA.Game.Features.Guilds.Services;

/// <summary>
///     Manages guild relationships (allies and enemies), handling addition, removal, and notifications.
/// </summary>
public static class GuildRelations {
    /// <summary>
    ///     Adds alliance relationship, removing any existing enemy relationship and notifying all members.
    /// </summary>
    /// <param name="guild">The guild initiating the alliance</param>
    /// <param name="targetGuildName">The name of the guild to ally with</param>
    private static void AddAlly(Guild guild, string targetGuildName) {
        foreach (var targetGuild in Kernel.Guilds.Values.Where(g => g.Name == targetGuildName)) {
            // Remove enemy relationship from initiator's side (if exists)
            if (guild.Enemy.ContainsKey(targetGuild.Id))
                RemoveEnemy(guild, targetGuild.Name);

            // Remove enemy relationship from target guild's side (if they had marked us as enemy)
            if (targetGuild.Enemy.ContainsKey(guild.Id))
                RemoveEnemy(targetGuild, guild.Name);

            guild.Ally.Add(targetGuild.Id, targetGuild);
            var message = new _String(true) {
                UID = targetGuild.Id,
                Type = 0x15
            };
            message.Texts.Add(string.Concat(new object[]
                { targetGuild.Name, " ", targetGuild.LeaderName, " 0 ", targetGuild.MemberCount }));
            guild.SendGuildMessage(message);
            guild.SendGuildMessage(message);
            GuildTable.AddAlly(guild, targetGuild.Id);
            return;
        }
    }

    /// <summary>
    ///     Removes alliance relationship, notifying all members and updating database.
    /// </summary>
    /// <param name="guild">The guild removing the alliance</param>
    /// <param name="targetGuildName">The name of the guild to remove from allies</param>
    public static void RemoveAlly(Guild guild, string targetGuildName) {
        foreach (var targetGuild in guild.Ally.Values) {
            if (targetGuild.Name != targetGuildName) continue;
            var cmd = new GuildCommand(true) {
                Type = GuildCommand.Unally,
                DwParam = targetGuild.Id
            };
            guild.SendGuildMessage(cmd);
            GuildTable.RemoveAlly(guild, targetGuild.Id);
            guild.Ally.Remove(targetGuild.Id);
            return;
        }
    }

    /// <summary>
    ///     Adds enemy relationship, removing any existing alliance and notifying all members.
    /// </summary>
    /// <param name="guild">The guild initiating the enemy relationship</param>
    /// <param name="targetGuildName">The name of the guild to mark as enemy</param>
    public static void AddEnemy(Guild guild, string targetGuildName) {
        foreach (var targetGuild in Kernel.Guilds.Values.Where(g => g.Name == targetGuildName)) {
            if (guild.Ally.ContainsKey(targetGuild.Id)) {
                RemoveAlly(guild, targetGuild.Name);
                RemoveAlly(targetGuild, guild.Name);
            }

            guild.Enemy.Add(targetGuild.Id, targetGuild);
            var stringPacket = new _String(true) {
                UID = targetGuild.Id,
                Type = _String.GuildEnemies
            };
            stringPacket.Texts.Add(targetGuild.Name + " " + targetGuild.LeaderName + " " + targetGuild.Level + " " +
                                   targetGuild.MemberCount);
            guild.SendGuildMessage(stringPacket);
            guild.SendGuildMessage(stringPacket);
            GuildTable.AddEnemy(guild, targetGuild.Id);

            return;
        }
    }

    /// <summary>
    ///     Removes enemy relationship, notifying all members and updating database.
    /// </summary>
    /// <param name="guild">The guild removing the enemy relationship</param>
    /// <param name="targetGuildName">The name of the guild to remove from enemies</param>
    public static void RemoveEnemy(Guild guild, string targetGuildName) {
        foreach (var targetGuild in guild.Enemy.Values) {
            if (targetGuild.Name != targetGuildName) continue;
            var cmd = new GuildCommand(true) {
                Type = GuildCommand.Peace,
                DwParam = targetGuild.Id
            };
            guild.SendGuildMessage(cmd);
            guild.SendGuildMessage(cmd);
            GuildTable.RemoveEnemy(guild, targetGuild.Id);
            guild.Enemy.Remove(targetGuild.Id);

            return;
        }
    }

    /// <summary>
    ///     Sends an alliance confirmation popup to the target guild leader, requiring mutual approval.
    ///     Establishes the alliance when both guild leaders agree.
    /// </summary>
    /// <param name="targetGuildName">The name of the guild to request an alliance with</param>
    /// <param name="initiatorPlayer">The client initiating the alliance request (must be a guild leader)</param>
    public static void AllianceConfirmationPopup(string targetGuildName, GameState initiatorPlayer) {
        var initiatorGuild = initiatorPlayer.Guild!;
        var targetGuild = Kernel.Guilds.Values.First(g => g.Name == targetGuildName);

        if (!Kernel.TryGetPlayer(targetGuild.Leader!.Id, out var targetLeader))
            return;

        var message =
            $"{initiatorPlayer.Entity.Name}, the Guild Leader of {initiatorGuild.Name}, wants to form an alliance with your guild.";

        targetLeader.MessageBox(
            message,
            msg_ok: _ => {
                AddAlly(targetGuild, initiatorGuild.Name);
                AddAlly(initiatorGuild, targetGuild.Name);
                initiatorPlayer.Send(new Message(
                    $"{targetGuild.Leader!.Name} has accepted your alliance request.",
                    Color.Red, Message.TopLeft));
            },
            msg_cancel: _ => {
                initiatorPlayer.Send(new Message(
                    $"{targetGuild.Leader!.Name} has declined your alliance request.",
                    Color.Red, Message.TopLeft));
            }
        );
    }

    /// <summary>
    ///     Gets the maximum number of relations (allies or enemies) allowed based on guild level.
    /// </summary>
    /// <param name="guildLevel">The guild level</param>
    /// <returns>The maximum number of relations allowed</returns>
    public static byte GetMaxRelations(byte guildLevel) {
        return guildLevel switch {
            1 => 5,
            2 => 7,
            3 => 9,
            4 => 12,
            >= 5 => 15,
        };
    }

    /// <summary>
    ///     Sends alliance/enemy list to client, displaying all diplomatic relationships.
    /// </summary>
    public static void SendGuildRelations(GameState client) {
        foreach (var guild in client.Guild!.Enemy.Values) {
            var stringPacket = new _String(true) {
                UID = guild.Id,
                Type = _String.GuildEnemies
            };
            stringPacket.Texts.Add(guild.Name + " " + guild.LeaderName + " 0 " + guild.MemberCount);
            client.Send(stringPacket);
            client.Send(stringPacket);
        }

        foreach (var guild in client.Guild!.Ally.Values) {
            var stringPacket = new _String(true) {
                UID = guild.Id,
                Type = _String.GuildAllies
            };
            stringPacket.Texts.Add(guild.Name + " " + guild.LeaderName + " 0 " + guild.MemberCount);
            client.Send(stringPacket);
            client.Send(stringPacket);
        }
    }
}