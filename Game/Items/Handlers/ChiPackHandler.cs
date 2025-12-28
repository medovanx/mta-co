using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles ChiPack items that grant Chi Points when used.
    /// </summary>
    [ItemHandler(NormalChiPack, MediumChiPack, SeniorChiPack, ExtremeChiPack, ChiPack100, ChiPack120, ChiPack160,
        ChiPack300, ChiPack800, ChiPack1000)]
    public static class ChiPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.Remove);

            var chiPoints = item.ID switch {
                NormalChiPack => 500u,
                MediumChiPack => 1000u,
                SeniorChiPack => 2000u,
                ExtremeChiPack => 3000u,
                ChiPack100 => 100u,
                ChiPack120 => 120u,
                ChiPack160 => 160u,
                ChiPack300 => 300u,
                ChiPack800 => 800u,
                ChiPack1000 => 1000u,
                _ => 0u // Default case (should not occur)
            };

            client.ChiPoints += chiPoints;
            client.Send(new Message($"Congratultions you have got {chiPoints} Chi points.", Color.Red,
                Message.TopLeft));
        }
    }
}

