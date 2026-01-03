using MTA.Client;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Guilds.Handlers;

[PacketHandler(2102)]
public static class GuildMembersHandler {
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        var page = BitConverter.ToUInt16(packet, 8);
        client.Guild!.SendMembers(client, page);
        return true;
    }
}