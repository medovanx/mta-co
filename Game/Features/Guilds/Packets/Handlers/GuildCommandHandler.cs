using MTA.Client;
using MTA.Game.Features.Guilds.Packets.Handlers.Command;
using MTA.Game.Features.Guilds.Packets.Writers;
using MTA.Network.PacketHandlers;
using GuildSettingsCommandHandler = MTA.Game.Features.Guilds.Packets.Handlers.Command.GuildSettingsHandler;

namespace MTA.Game.Features.Guilds.Packets.Handlers;

/// <summary>
///     Central router for guild command packets (1107), handling all guild-related actions such as promotions, donations, relationships, and settings.
/// </summary>
[PacketHandler(1107)]
public static class GuildCommandHandler {
    /// <summary>
    ///     Routes commands to appropriate handlers based on command type, delegating to specialized handlers for each operation.
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        var command = new GuildCommand(false);
        command.Deserialize(packet);

        switch (command.Type) {
            case GuildCommand.PromoteInfo:
                GuildPromotionHandler.HandlePromoteInfo(command, packet, client);
                break;
            case GuildCommand.RequestPromote:
                GuildPromotionHandler.HandleRequestPromote(command, client);
                break;
            case GuildCommand.PromoteWithCP:
                GuildPromotionHandler.HandlePromote(command, packet, client);
                break;
            case GuildCommand.Info:
                GuildSettingsCommandHandler.HandleInfo(command, client);
                break;
            case GuildCommand.ChangeGuildRequirements:
                GuildSettingsCommandHandler.HandleChangeRequirements(command, client);
                break;
            case GuildCommand.Neutral1:
            case GuildCommand.Neutral2:
                GuildRelationsHandler.HandleNeutral(command, packet, client);
                break;
            case GuildCommand.Allied:
                GuildRelationsHandler.HandleAllied(command, packet, client);
                break;
            case GuildCommand.Enemied:
                GuildRelationsHandler.HandleEnemied(command, packet, client);
                break;
            case GuildCommand.AddToBlacklist:
                GuildBlacklistHandler.HandleBlacklistAdd(command, client);
                break;
            case GuildCommand.RemoveFromBlacklist:
                GuildBlacklistHandler.HandleBlacklistRemove(command, client);
                break;
            case GuildCommand.Bulletin:
                GuildSettingsCommandHandler.HandleBulletin(command, packet, client);
                break;
            case GuildCommand.DonateSilvers:
                GuildDonationHandler.HandleDonateSilvers(command, client);
                break;
            case GuildCommand.DonateConquerPoints:
                GuildDonationHandler.HandleDonateConquerPoints(command, client);
                break;
            case GuildCommand.Refresh:
                GuildSettingsCommandHandler.HandleRefresh(client);
                break;
            case GuildCommand.Discharge:
                GuildPromotionHandler.HandleDischarge(command, packet, client);
                break;
            case GuildCommand.Promote:
                GuildPromotionHandler.HandlePromote(command, packet, client);
                break;
            case GuildCommand.JoinRequest:
                GuildJoinHandler.HandleJoinRequest(command, client);
                break;
            case GuildCommand.InviteRequest:
                GuildJoinHandler.HandleInviteRequest(command, client);
                break;
            case GuildCommand.Quit:
                GuildJoinHandler.HandleQuit(client);
                break;
            default:
                client.Send(packet);
                break;
        }

        return true;
    }
}