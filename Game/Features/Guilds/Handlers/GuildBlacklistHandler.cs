using MTA.Client;
using MTA.Game.Features.Guilds.Packets;

namespace MTA.Game.Features.Guilds.Handlers;

public static class GuildBlacklistHandler {
    public static void HandleBlacklistAdd(GuildCommand command, GameState client) {
        var uid = command.DwParam;
        if (!Kernel.GamePool.TryGetValue(uid, out var c)) return;
        if (!client.Guild!.BlackList.Contains(uid))
            client.Guild.BlackList.Add(uid);
        c.Send(command);
    }

    public static void HandleBlacklistRemove(GuildCommand command, GameState client) {
        var uid = command.DwParam;
        client.Guild!.BlackList.Remove(uid);
        client.Send(command);
    }
}