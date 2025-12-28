using System;
using MTA.Client;
using MTA.Npcs;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles SmallCPBox item that grants random rewards (money, CPs, study points, bound CPs).
    /// </summary>
    [ItemHandler(SmallCPBox)]
    public static class SmallCPBoxHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count <= 38) {
                client.Inventory.Remove(item, Enums.ItemUse.Remove);
                var r = new Random();
                var nr = r.Next(1, 10);
                if (nr == 1) client.Entity.Money += 100000000;
                if (nr == 2) client.Entity.ConquerPoints += 5000000;
                if (nr == 4) client.Entity.SubClasses.StudyPoints += 20000;
                if (nr == 5) client.Entity.BoundCps += 5000000;
            }

            var dialog = new MTA.Npcs(client);
            dialog.Text("Hello. " + client.Entity.Name +
                        " You Can Open [ SmallCPBox ] [ Maded By Franko ] Get Prize >>> 500.000.000 Silvers - OR - 5.000.000 CPs - OR - 20.000 Study Points - OR - 5.000.000 CPSBound <<< Good Luck Good.");
            dialog.Option("Thansk you:*.", 255);
            dialog.Send();
            client.ActiveNpc = item.ID;
        }
    }
}

