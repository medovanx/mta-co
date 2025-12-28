using MTA.Client;
using MTA.Game.Items;
using MTA.Game.Npcs;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles NobleSteedPack item that opens a dialog to choose a steed.
    /// </summary>
    [ItemHandler(NobleSteedPack)]
    public static class NobleSteedPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            var dialog = new Npcs(client);
            dialog.Text("Hello. You can chose a horse:");
            dialog.Option("Spitfire horse", 1);
            dialog.Option("Frostbite horse", 2);
            dialog.Option("Blazehoof horse", 3);
            dialog.Option("Spotted horse", 4);
            dialog.Option("Zebra", 5);
            dialog.Option("Nevermind.", 255);
            dialog.Send();
            client.ActiveNpc = item.ID;
        }
    }
}

