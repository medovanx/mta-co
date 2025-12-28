using System;
using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.Constants.Items.DragonSoulPacks;
using static MTA.Game.Constants.Items.DragonSouls.P4;
using static MTA.Game.Constants.Items.DragonSouls.P5;
using static MTA.Game.Constants.Items.DragonSouls.P6;
using static MTA.Game.Constants.Items.DragonSouls.P7;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles DragonSoulPack items that grant random dragon souls when used.
    /// </summary>
    [ItemHandler(P4DragonSoulBag, P6DragonSoulBag, P7WeaponSoulPack, P4DragonSoulPack, P5DragonSoulPack,
        P7WeaponSoulPack2, P6WeaponSoulPack, P6DragonSoulPack)]
    public static class DragonSoulPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count > 38) {
                return;
            }

            client.Inventory.Remove(item, Enums.ItemUse.Remove);

            var r = new Random();
            var soulId = 0u;

            if (item.ID == P4DragonSoulBag) {
                var nr = r.Next(1, 8);
                soulId = nr switch {
                    1 => GloomPistol,
                    2 => TitanRapier,
                    3 => VioletRing,
                    4 => HolyHeavyRing,
                    5 => MoonBracelet,
                    6 => HolyBeadsOfMagnanimity,
                    7 => AnnihilationScythe,
                    _ => 0u
                };

                if (soulId != 0) {
                    client.Inventory.Add(soulId, 0, 1);
                }

                client.Send(new Message("Congratultions you have got P4DragonSoul.", Color.Red, Message.TopLeft));
            }
            else if (item.ID == P6DragonSoulBag) {
                var nr = r.Next(1, 8);
                soulId = nr switch {
                    1 => TombBlade,
                    2 => StealthKatana,
                    3 => DragonChant,
                    4 => DestinyRapier,
                    5 => HolyBeadsOfConsciousness,
                    6 => SaintBag,
                    7 => SaintNecklace,
                    _ => 0u
                };

                if (soulId != 0) {
                    client.Inventory.Add(soulId, 0, 1);
                }

                client.Send(new Message("Congratultions you have got P6DragonSoul.", Color.Red, Message.TopLeft));
            }
            else if (item.ID == P7WeaponSoulPack) {
                var nr = r.Next(1, 9);
                soulId = nr switch {
                    1 => SkyHammer,
                    2 => ShadowKatana,
                    3 => TimeBacksword,
                    4 => SunBow,
                    5 => SpiritShield,
                    6 => BuddaBeads,
                    7 => DeathPistol,
                    8 => RepentRapier,
                    _ => 0u
                };

                if (soulId != 0) {
                    client.Inventory.Add(soulId, 0, 1);
                }

                client.Send(new Message("Congratultions you have got P7WeaponSoul.", Color.Red, Message.TopLeft));
            }
            else if (item.ID == P4DragonSoulPack) {
                var nr = r.Next(1, 17);
                soulId = nr switch {
                    1 => RadiantSword,
                    2 => MoonHammer,
                    3 => OnimaKatana,
                    4 => SnakeHalbert,
                    5 => AnnihilationScythe,
                    6 => LotusBacksword,
                    7 => GraceBow,
                    8 => HolyBeadsOfMagnanimity,
                    9 => GloomPistol,
                    10 => TitanRapier,
                    11 => StygianKnifeSoul,
                    12 => VioletRing,
                    13 => HolyHeavyRing,
                    14 => MoonBracelet,
                    15 => BlazingScale,
                    16 => UniversalHossu,
                    _ => 0u
                };

                if (soulId != 0) {
                    client.Inventory.Add(soulId, 0, 1);
                }
            }
            else if (item.ID == P5DragonSoulPack) {
                var nr = r.Next(1, 6);
                soulId = nr switch {
                    1 => ThunderShield,
                    2 => AzureHat,
                    3 => SolarHat,
                    4 => SpiritNecklace,
                    5 => EbonyBag,
                    _ => 0u
                };

                if (soulId != 0) {
                    client.Inventory.Add(soulId, 0, 1);
                }
            }
            else if (item.ID == P7WeaponSoulPack2) {
                var nr = r.Next(1, 17);
                soulId = nr switch {
                    1 => MonsterSaber,
                    2 => SkyHammer,
                    3 => ShadowKatana,
                    4 => SkyHalberd,
                    5 => DemonScythe,
                    6 => SpiritShield,
                    7 => TimeBacksword,
                    8 => SunBow,
                    9 => BuddaBeads,
                    10 => DeathPistol,
                    11 => RepentRapier,
                    12 => StygianKnifeSoul2,
                    13 => WarCraze,
                    14 => WonderHossu,
                    15 => FistofDemon,
                    16 => FistofDeity,
                    _ => 0u
                };

                if (soulId != 0) {
                    client.Inventory.Add(soulId, 0, 1);
                }
            }
            else if (item.ID == P6WeaponSoulPack) {
                var nr = r.Next(1, 18);
                soulId = nr switch {
                    1 => TombBlade,
                    2 => StealthKatana,
                    3 => GrimHammer,
                    4 => SufferingScythe,
                    5 => ArchonWand,
                    6 => FlameShield,
                    7 => LotusBacksword,
                    8 => WingedBow,
                    9 => HolyBeadsOfConsciousness,
                    10 => TimePistol,
                    11 => DestinyRapier,
                    12 => DominantKnifeSoul,
                    13 => DragonChant,
                    14 => HeavenHossu,
                    15 => FistofSky,
                    16 => FistofEarth,
                    17 => SolarFanSoul,
                    _ => 0u
                };

                if (soulId != 0) {
                    client.Inventory.Add(soulId, 0, 1);
                }

                // P6WeaponSoulPack also gives P6DragonSoulPack and CPs
                client.Inventory.Add(P6DragonSoulPack, 0, 1);
                client.Entity.ConquerPoints += 2000000;
            }
            else if (item.ID == P6DragonSoulPack) {
                var nr = r.Next(1, 11);
                soulId = nr switch {
                    1 => WhirlpoolArmor,
                    2 => WaterflowArmor,
                    3 => SaintRing,
                    4 => SaintBracelet,
                    5 => SaintHeavyRing,
                    6 => SaintBoots,
                    7 => SaintNecklace,
                    8 => SaintBag,
                    9 => SaintHeadgear,
                    10 => HolyHeadgear,
                    _ => 0u
                };

                if (soulId != 0) {
                    client.Inventory.Add(soulId, 0, 1);
                }
            }
        }
    }
}

