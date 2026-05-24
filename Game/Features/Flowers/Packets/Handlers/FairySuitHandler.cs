using MTA.Client;
using MTA.Game.Features.Flowers.Packets.Writers;
using MTA.Network.PacketHandlers;

namespace MTA.Game.Features.Flowers.Packets.Handlers;

/// <summary>
///     Handles packet 2070 (MsgSuitStatus) for the flower-fairy suit equip / unequip flow.
///     Switching to a different fairy is allowed without an explicit unequip first.
/// </summary>
[PacketHandler(Constants.Packets.MsgSuitStatus)]
public static class FairySuitHandler {
    public static bool Handle(ushort packetId, byte[] packet, GameState client) {
        var fairySpawn = new FairySpawn(false, packet);
        switch (fairySpawn.SType) {
            case 1:
                client.IsFairy = true;
                client.FairyType = fairySpawn.FairyType;
                client.SType = fairySpawn.SType;
                fairySpawn.Uid = client.Entity.UID;
                client.SendScreen(fairySpawn);
                break;
            case 2:
                if (!client.IsFairy) return true;
                client.IsFairy = false;
                client.FairyType = 0;
                client.SType = 0;
                fairySpawn.Uid = client.Entity.UID;
                client.SendScreen(fairySpawn);
                break;
        }

        return true;
    }
}