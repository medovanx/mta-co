using System.Drawing;
using MTA.Client;
using MTA.Database;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles DemonBox items that spawn demons when used (cannot be used in Market map 1036).
    /// </summary>
    [ItemHandler(DemonBox, AncientDemonBox, FloodDemonBox, HeavenDemonBox, ChaosDemonBox, SacredDemonBox)]
    public static class DemonBoxHandler {
        private const ushort MarketMapID = 1036;

        public static void Handle(GameState client, ConquerItem item) {
            if (client.Entity.MapID == MarketMapID) {
                client.Send(new Message("Sorry You can't open it In Market", Color.Red, Message.TopLeft));
                return;
            }

            uint monsterId = item.ID switch {
                DemonBox => 2420u,
                AncientDemonBox => 2421u,
                FloodDemonBox => 2422u,
                HeavenDemonBox => 2423u,
                ChaosDemonBox => 2424u,
                SacredDemonBox => 2425u,
                _ => 0u // Default case (should not occur)
            };

            if (MonsterInformation.MonsterInformations.TryGetValue(monsterId, out var mob)) {
                client.Map.SpawnMonsterNearToHero(mob, client);
                client.Inventory.Remove(item, Enums.ItemUse.Remove);
            }
        }
    }
}

