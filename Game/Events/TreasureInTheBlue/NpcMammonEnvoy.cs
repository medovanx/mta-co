using MTA.Client;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;

namespace MTA.Game.Events.TreasureInTheBlue;

/// <summary>
/// Entry point for the Prize Center of the Treasure in the Blue event
/// </summary>
/// <event>Treasure in the Blue</event>
/// <npc>Mammon Envoy</npc>
/// <description>Exchanges event coins for rewards</description>
[NpcHandler(115522011)]
public static class NpcMammonEnvoy {
    public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
        const ushort tradeMapId = MapConstants.TreasureInTheBlue_PrizeCenter;
        switch (npcRequest.OptionID) {
            case 0: {
                dialog.Text("Welcome to the Prize Center! Here you can exchange your ancient coins for rewards.\n\n" +
                            "Remember: coins expire after 60 minutes, so exchange them quickly!");
                dialog.Option("Enter the Prize Center", 1);
                dialog.Option("Not now.", 255);
                dialog.Send();
                break;
            }

            case 1: {
                client.Entity.Teleport(MapConstants.JOB_CENTER, tradeMapId, 52, 55);
                client.Entity.Update(_String.Effect, "accession4", true);
                client.Send(
                    "Welcome to the Prize Center! Exchange your ancient coins with the Mammon Envoy for rewards!");

                break;
            }
        }
    }
}