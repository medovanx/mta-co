using MTA.Client;
using MTA.Game.Features.Guilds.Packets.Writers;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Guilds.Packets.Handlers;

/// <summary>
///     Handles packet 1058 which displays guild member donation information to the client.
///     Uses GuildDonationInfoPacket writer to construct the packet with current donation data.
/// </summary>
[PacketHandler(1058)]
public static class GuildDonationInfoHandler {
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        var donationPacket = new GuildDonationInfoPacket();
        donationPacket.Build(client);
        client.Guild?.SendGuild(client);
        client.Send(donationPacket.ToArray());
        return true;
    }
}