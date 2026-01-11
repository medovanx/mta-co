using MTA.Client;
using MTA.Game.Features.Flowers.Services;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Flowers.Packets.Handlers;

/// <summary>
///     Handles packet 1150 for boy/girl flower sending (type 0/1)
/// </summary>
[PacketHandler(Constants.Packets.MsgFlower)]
public static class FlowerSendingHandler {
    /// <summary>
    ///     Handles boy/girl flower sending logic
    /// </summary>
    public static bool Handle(ushort packetId, byte[] packet, GameState caller) {
        // Check if this is a boy/girl flower sending packet (type 0 or 1)
        var packetType = System.BitConverter.ToUInt32(packet, 4);
        if (packetType != 0 && packetType != 1) return false; // Not a boy/girl sending packet

        FlowerSendingService.SendFlower(caller, packet);
        return true;
    }
}