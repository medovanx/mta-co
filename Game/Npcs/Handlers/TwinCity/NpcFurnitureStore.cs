using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Database.ConquerItemInformation;
using static MTA.Game.ConquerStructures.House.Furniture;

namespace MTA.Game.Npcs.Handlers.TwinCity {
    /// <summary>
    /// Furniture Store NPC - Provides furniture store services
    /// </summary>
    [NpcHandler(30161)]
    public static class NpcFurnitureStore {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            // Furniture store entrance logic
            switch (npcRequest.OptionID) {
                case 0: {
                    dialog.Text(
                        "Greetings! Welcome to the Twin City Furniture Store. Our selection is currently limited, but new furniture will be arriving soon.");
                    dialog.Option("I wanna have a look.", 1);
                    dialog.Option("I am not interested.", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    client.Entity.Teleport(1511, 52, 70);
                    dialog.Send();
                    break;
                }
            }

            // Check if this NPC is a furniture vendor (for buying)
            if (!Furnitures.TryGetValue(client.ActiveNpc, out var info)) return;
            const int price = 50000;
            switch (npcRequest.OptionID) {
                case 0: {
                    if (!BaseInformations.TryGetValue(info.itemid, out var value))
                        return;
                    if (value.Name is "" or " ")
                        return;
                    dialog.Text(
                        $"Greetings, {client.Entity.Name}! I have a {value.Name} available for {price} gold. Would you like to purchase it?");
                    dialog.Option("Buy Item", 1);
                    dialog.Option("I'm not interested", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    if (client.Entity.Money >= price) {
                        client.Entity.Money -= price;
                        client.Inventory.Add(info.itemid, 0, 1);
                    }
                    else {
                        dialog.Text($"You don't have {price} gold ");
                        dialog.Option("Alright", 255);
                        dialog.Avatar(116);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}