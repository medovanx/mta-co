using MTA.Client;
using MTA.Game.Features.Guilds.Packets.Writers;

namespace MTA.Game.Features.Guilds.Packets.Handlers.Command;

/// <summary>
///     Handles adding/removing players from guild blacklist, preventing blacklisted players from joining the guild.
/// </summary>
public static class GuildBlacklistHandler {
    /// <summary>
    ///     Adds player to blacklist, preventing them from sending join requests to the guild.
    /// </summary>
    public static void HandleBlacklistAdd(GuildCommand command, GameState client) {
        var uid = command.DwParam;
        if (!Kernel.GamePool.TryGetValue(uid, out var c)) return;
        if (!client.Guild!.BlackList.Contains(uid))
            client.Guild.BlackList.Add(uid);
        c.Send(command);
    }

    /// <summary>
    ///     Removes player from blacklist, allowing them to send join requests again.
    /// </summary>
    public static void HandleBlacklistRemove(GuildCommand command, GameState client) {
        var uid = command.DwParam;
        client.Guild!.BlackList.Remove(uid);
        client.Send(command);
    }
}