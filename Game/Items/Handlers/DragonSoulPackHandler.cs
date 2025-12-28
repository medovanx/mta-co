using System;
using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Constants;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles DragonSoulPack items that grant random dragon souls when used.
    /// </summary>
    [ItemHandler(P4DragonSoulBag, P6DragonSoulBag, P7WeaponSoulPack, P4DragonSoulPack, P5DragonSoulPack)]
    public static class DragonSoulPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count > 38) {
                return;
            }

            client.Inventory.Remove(item, Enums.ItemUse.Remove);

            var r = new Random();
            uint soulId = 0u;

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
        }
    }
}

