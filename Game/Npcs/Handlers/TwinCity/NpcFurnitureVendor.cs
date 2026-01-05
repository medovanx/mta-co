using MTA.Client;
using MTA.Network.GamePackets;
using static MTA.Database.ConquerItemInformation;
using static MTA.Game.Features.House.Furniture;

namespace MTA.Game.Npcs.Handlers.TwinCity {
    /// <summary>
    /// Furniture Vendor NPCs - Handles buying furniture items from furniture vendors
    /// </summary>
    public static class NpcFurnitureVendor {
        public static void Handle(GameState client, NpcRequest npcRequest, MTA.Npcs dialog) {
            if (!FurnitureVendors.TryGetValue(client.ActiveNpc, out var info)) {
                return;
            }

            var price = info.Price;
            switch (npcRequest.OptionID) {
                case 0: {
                    if (!BaseInformations.TryGetValue(info.ItemId, out var value))
                        return;
                    if (value.Name is "" or " ")
                        return;
                    dialog.Text($"Hello, I can offer you a {value.Name} for just {price:N0} gold.");
                    dialog.Option("I would like to buy this!", 1);
                    dialog.Option("No, thank you.", 255);
                    dialog.Send();
                    break;
                }
                case 1: {
                    if (client.Entity.Money >= price) {
                        client.Entity.Money -= price;
                        client.Inventory.Add(info.ItemId, 0, 1);
                    }
                    else {
                        dialog.Text($"You don't have {price} gold ");
                        dialog.Option("I see.", 255);
                        dialog.Avatar(116);
                        dialog.Send();
                    }

                    break;
                }
            }
        }
    }
}