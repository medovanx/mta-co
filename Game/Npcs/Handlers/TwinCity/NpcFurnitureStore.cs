using MTA.Client;
using MTA.Network.GamePackets;

namespace MTA.Game.Npcs.Handlers.TwinCity {
    /// <summary>
    /// Furniture Store NPC - Allows players to enter the furniture store
    /// </summary>
    [NpcHandler(30161)]
    public static class NpcFurnitureStore {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        "Greetings!\nWelcome to the Twin City Furniture Store.\nOur selection is currently limited, but new furniture will be arriving soon!");
                    dialog.Option("I wanna have a look!", 1);
                    dialog.Option("No, thank you.", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    client.Entity.Teleport(1511, 52, 70);
                    break;
                }
            }
        }
    }
}