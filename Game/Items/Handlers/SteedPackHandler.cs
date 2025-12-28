using MTA.Client;
using MTA.Database;
using MTA.Game.Items;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SteedPack items that grant steeds when used.
    /// </summary>
    [ItemHandler(Plus1SteedPack, Plus3SteedPack, Plus6SteedPack, Plus1MaroonSteedPack, Plus1WhiteSteedPack,
        Plus1BlackSteedPack, Plus3MaroonSteedPack, Plus3WhiteSteedPack, Plus3BlackSteedPack,
        Plus6MaroonSteedPack, Plus6WhiteSteedPack, Plus6BlackSteedPack, MaroonSteedPack,
        WhiteSteedPack, BlackSteedPack)]
    public static class SteedPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            // Dialog-based steed packs (728525-728527) - don't remove item, handled in dialog response
            if (item.ID is Plus1SteedPack or Plus3SteedPack or Plus6SteedPack) {
                var dialog = new Npcs(client);
                dialog.Text("Hello. You can chose a horse: black, brown or white.");
                dialog.Option("Black horse", 1);
                dialog.Option("Brown horse", 2);
                dialog.Option("White horse", 3);
                dialog.Option("Nevermind.", 255);
                dialog.Send();
                client.ActiveNpc = item.ID;
                return;
            }

            // Direct steed packs - create steed item directly
            byte plus = item.ID switch {
                Plus1MaroonSteedPack or Plus1WhiteSteedPack or Plus1BlackSteedPack => 1,
                Plus3MaroonSteedPack or Plus3WhiteSteedPack or Plus3BlackSteedPack => 3,
                Plus6MaroonSteedPack or Plus6WhiteSteedPack or Plus6BlackSteedPack => 6,
                _ => 0 // MaroonSteedPack, WhiteSteedPack, BlackSteedPack
            };

            uint socketProgress = item.ID switch {
                Plus1MaroonSteedPack or Plus3MaroonSteedPack or Plus6MaroonSteedPack or MaroonSteedPack => 150 << 8 | 255 << 16,
                Plus1WhiteSteedPack or Plus3WhiteSteedPack or Plus6WhiteSteedPack or WhiteSteedPack => 150 | 255 << 8,
                Plus1BlackSteedPack or Plus3BlackSteedPack or Plus6BlackSteedPack or BlackSteedPack => 255 | 150 << 16,
                _ => 0u
            };

            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
            var steed = new ConquerItem(true) {
                ID = Steed
            };
            var itemInfo = new ConquerItemInformation(steed.ID, 0);
            steed.Durability = steed.MaximDurability = itemInfo.BaseInformation.Durability;
            steed.Plus = plus;
            steed.Effect = Enums.ItemEffect.Horse;
            steed.SocketProgress = socketProgress;
            client.Inventory.Add(steed, Enums.ItemUse.CreateAndAdd);
        }
    }
}

