using MTA.Client;
using MTA.Network.GamePackets;
using _String = MTA.Network.GamePackets._String;
using static MTA.Game.Constants.Items.QuestAndOther;
using static MTA.Game.Constants.Items.BasicItems;
using static MTA.Game.Constants.Items.StudyBooks;
using static MTA.Game.Constants.Items.ChiItems;
using static MTA.Game.Constants.Items.CPsBags;
using static MTA.Game.Constants.Items.DragonSouls.P4;
using static MTA.Game.Constants.Items.DragonSouls.P5;
using static MTA.Game.Constants.Items.DragonSouls.P6;
using static MTA.Game.Constants.Items.DragonSouls.P7;
using static MTA.Game.Constants.Items.Gems;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles AnniversaryJoyPack item that grants random items when used.
    /// </summary>
    [ItemHandler(AnniversaryJoyPack)]
    public static class AnniversaryJoyPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            client.Inventory.Remove(item, Enums.ItemUse.Remove);
            var type = (byte)Kernel.Random.Next(1, 50);
            var uid = type switch {
                1 => ArchonWand,
                2 => DragonBallPack3,
                3 => GrimHammer,
                4 => AzureHat,
                5 => OceanArmor,
                6 => DragonBallPack,
                7 => WhirlpoolArmor,
                8 => MysticCPPack,
                9 => StiffSword,
                10 => MoonHammer,
                11 => BillowHammer,
                12 => DeityCPPack,
                13 => StealthKatana,
                14 => GiantAxe,
                15 => OnimaKatana,
                16 => GhostCPPack,
                17 => LunarKatana,
                18 => DiligenceBook,
                19 => ScarKatana,
                20 => BloodyGlaive,
                21 => ResplendentWand,
                22 => ModestyBook,
                23 => SnakeHalbert,
                24 => CelestialSpear,
                25 => WaveShield,
                26 => DragonBall,
                27 => CrystalShield,
                28 => FlameShield,
                29 => LotusBacksword,
                30 => GrassBracelet,
                31 => ChiPill_200,
                32 => BurntBacksword,
                33 => DarkBacksword,
                34 => SoulCPPack,
                35 => PhoenixBow,
                36 => RuneBow,
                37 => HopeCPPack,
                38 => AncientBow,
                39 => HolyBeadsOfMagnanimity,
                40 => SuperGloryGem,
                41 => JadeAxe,
                42 => HolyBeadsOfPercept,
                43 => HolyBeadsOfDharma,
                44 => RainbowBlade,
                45 => FlameHat,
                46 => SuperThunderGem,
                47 => SurgeHeadgear,
                48 => LotusHeadgear,
                49 => FeatherHeadgear,
                50 => HolyBeadsOfConsciousness,
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