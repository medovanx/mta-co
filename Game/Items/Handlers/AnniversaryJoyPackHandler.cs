using MTA.Client;
using MTA.Network.GamePackets;
using _String = MTA.Network.GamePackets._String;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles AnniversaryJoyPack item that grants random items when used.
    /// </summary>
    [ItemHandler(AnniversaryJoyPack)]
    public static class AnniversaryJoyPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.Remove);
            uint uid = 0;
            var type = (byte)Kernel.Random.Next(1, 50);
            uid = type switch {
                1 => 800320u,
                2 => 720891u,
                3 => 800110u,
                4 => 820056u,
                5 => 822056u,
                6 => 727100u,
                7 => 822053u,
                8 => CPBag6900,
                9 => 800050u,
                10 => 800015u,
                11 => 800090u,
                12 => CPBag500,
                13 => 800017u,
                14 => 800071u,
                15 => 800016u,
                16 => CPBag250,
                17 => 800130u,
                18 => DiligenceBook,
                19 => 800141u,
                20 => 800200u,
                21 => 800310u,
                22 => ModestyBook,
                23 => 800214u,
                24 => 800230u,
                25 => 800414u,
                26 => DragonBall,
                27 => 800420u,
                28 => 800401u,
                29 => 800512u,
                30 => 823043u,
                31 => ChiPoint200,
                32 => 800520u,
                33 => 800521u,
                34 => CPBag500_2,
                35 => 800614u,
                36 => 800615u,
                37 => CPBag5,
                38 => 800617u,
                39 => 800720u,
                40 => 700123u,
                41 => 800070u,
                42 => 800723u,
                43 => 800724u,
                44 => 800018u,
                45 => 820001u,
                46 => 700103u,
                47 => 820053u,
                48 => 820054u,
                49 => 820055u,
                50 => 800722u,
                _ => 0u
            };

            if (uid == 0) return;
            client.Inventory.Add(uid, 0, 1);
            var str = new _String(true) {
                UID = client.Entity.UID,
                Type = _String.Effect
            };
            str.Texts.Add("cortege");
            str.TextsCount = 1;
            client.Entity.SendScreen(str);
        }
    }
}

