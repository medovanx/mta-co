using MTA.Client;
using MTA.Game.Constants;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.Market {
    /// <summary>
    /// Lady Luck - Helps players to teleport into Lottery
    /// </summary>
    [NpcHandler(13)]
    public static class NpcLadyLuck {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text("Hey there! Listen, I can help you teleport into Lottery. Are you ready?");
                    dialog.Option("Yes, teleport me.", 1);
                    dialog.Option("Just passing by.", 255);

                    dialog.Send();

                    break;
                }
                case 1: {
                    client.Entity.Teleport(Maps.Lottery, 42, 47);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Lady Luck - Exit NPC for Lottery
    /// </summary>
    [NpcHandler(924)]
    public static class NpcLadyLuckExit {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text("You may come back later, if you still want to try your luck.\nDo you want to leave?");
                    dialog.Option("Yes please, let me out.", 1);
                    dialog.Option("No thank you.", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    client.Entity.Teleport(Maps.Market, 216, 189);
                    break;
                }
            }
        }
    }
}