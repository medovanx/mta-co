using MTA.Client;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Guilds.Packets.Handlers;

/// <summary>
///     Handles packet 2102 for guild member list requests, displaying all guild members with their information.
/// </summary>
[PacketHandler(Game.Constants.Packets.MsgSynMemberList)]
public static class GuildMembersHandler {
    /// <summary>
    ///     Processes member list page request and sends the member list to the client.
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        var page = BitConverter.ToUInt16(packet, 8);
        client.Guild!.SendMembers(client, page);
        return true;
    }
}