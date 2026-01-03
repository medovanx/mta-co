using System;
using System.Text;
using MTA.Client;
using MTA.Game.Features.Guilds.Constants;
using MTA.Game.Features.Guilds.Database;
using MTA.Game.Features.Guilds.Packets.Writers;

namespace MTA.Game.Features.Guilds.Packets.Handlers.Command;

/// <summary>
///     Handles guild settings commands that are routed from GuildCommandHandler. These commands
///     manage guild information display, requirements, bulletin, and data refresh operations.
/// </summary>
public static class GuildSettingsHandler {
    /// <summary>
    ///     Sends the guild name to the client when they request guild information.
    ///     Used when a player wants to view details about a specific guild.
    /// </summary>
    /// <param name="command">The guild command containing the guild ID to look up</param>
    /// <param name="client">The client requesting the guild information</param>
    public static void HandleInfo(GuildCommand command, GameState client) {
        if (Kernel.Guilds.TryGetValue(command.DwParam, out var guild)) guild.SendName(client);
    }

    /// <summary>
    ///     Updates the guild's join requirements (level, reborn, class restrictions) when the
    ///     Guild Leader changes them. Only Guild Leaders can modify these requirements, and the
    ///     values are capped to prevent invalid settings. All guild members are notified of the change.
    /// </summary>
    /// <param name="command">The guild command containing the new requirement values</param>
    /// <param name="client">The client (must be Guild Leader) updating the requirements</param>
    public static void HandleChangeRequirements(GuildCommand command, GameState client) {
        if (client.AsMember!.Rank != MemberRank.GuildLeader) return;
        client.Guild!.LevelRequirement = Math.Min(command.DwParam2, 140);
        client.Guild.RebornRequirement = Math.Min(command.DwParam3, 2);
        client.Guild.ClassRequirement = Math.Min(command.DwParam4, 127);
        foreach (var member in client.Guild.Members.Values) {
            if (Kernel.TryGetPlayer(member.Id, out var memberClient))
                client.Guild.SendGuild(memberClient);
        }

        GuildTable.SaveRequirements(client.Guild);
    }

    /// <summary>
    ///     Updates the guild bulletin message when the Guild Leader sets a new announcement.
    ///     The bulletin is displayed to all guild members and stored in the database. Only
    ///     Guild Leaders can modify the bulletin.
    /// </summary>
    /// <param name="command">The guild command</param>
    /// <param name="packet">The packet containing the bulletin message text</param>
    /// <param name="client">The client (must be Guild Leader) setting the bulletin</param>
    public static void HandleBulletin(GuildCommand command, byte[] packet, GameState client) {
        var message = Encoding.Default.GetString(packet, 26, packet[25]);
        if (client is not { Guild: not null, AsMember.Rank: MemberRank.GuildLeader }) return;
        client.Guild.Bulletin = message;
        client.Guild.CreateBulletinTime();
        client.Guild.SendGuild(client);
        GuildTable.UpdateBulletin(client.Guild, client.Guild.Bulletin);
    }

    /// <summary>
    ///     Refreshes the guild data display for the client. This sends the current guild
    ///     information to the client, updating their view with the latest guild state.
    /// </summary>
    /// <param name="client">The client requesting the refresh</param>
    public static void HandleRefresh(GameState client) {
        if (client is { AsMember: not null, Guild: not null }) client.Guild.SendGuild(client);
    }
}