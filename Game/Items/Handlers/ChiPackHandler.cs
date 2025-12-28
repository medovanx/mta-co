using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.Constants.Items.ChiItems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles ChiPack items that grant Chi Points when used.
    /// </summary>
    [ItemHandler(NormalChiPack, MediumChiPack, SeniorChiPack, ExtremeChiPack, ChiPack_100, ChiPack_120, ChiPack_160,
        ChiPack_300, ChiPack_800, ChiPack_1000)]
    public static class ChiPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.Remove);

            var chiPoints = item.ID switch {
                NormalChiPack => 500u,
                MediumChiPack => 1000u,
                SeniorChiPack => 2000u,
                ExtremeChiPack => 3000u,
                ChiPack_100 => 100u,
                ChiPack_120 => 120u,
                ChiPack_160 => 160u,
                ChiPack_300 => 300u,
                ChiPack_800 => 800u,
                ChiPack_1000 => 1000u,
                _ => 0u // Default case (should not occur)
            };

            client.ChiPoints += chiPoints;
            client.Send(new Message($"Congratultions you have got {chiPoints} Chi points.", Color.Red,
                Message.TopLeft));
        }
    }
}

