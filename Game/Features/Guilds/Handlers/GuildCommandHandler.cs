using MTA.Client;
using MTA.Network.GamePackets;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Guilds.Handlers;

[PacketHandler(1107)]
public static class GuildCommandHandler {
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        // Skip if in specific map (original logic from PacketHandler)
        if (client.Map.BaseID == 700) {
            client.Send(packet);
            return true;
        }

        var command = new GuildCommand(false);
        command.Deserialize(packet);

        switch (command.Type) {
            case GuildCommand.PromoteInfo:
                GuildPromotionHandler.HandlePromoteInfo(command, packet, client);
                break;
            case GuildCommand.RequestPromote:
                GuildPromotionHandler.HandleRequestPromote(command, client);
                break;
            case GuildCommand.Info:
                GuildSettingsHandler.HandleInfo(command, client);
                break;
            case GuildCommand.ChangeGuildRequirements:
                GuildSettingsHandler.HandleChangeRequirements(command, client);
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
                GuildSettingsHandler.HandleBulletin(command, packet, client);
                break;
            case GuildCommand.DonateSilvers:
                GuildDonationHandler.HandleDonateSilvers(command, client);
                break;
            case GuildCommand.DonateConquerPoints:
                GuildDonationHandler.HandleDonateConquerPoints(command, client);
                break;
            case GuildCommand.Refresh:
                GuildSettingsHandler.HandleRefresh(client);
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