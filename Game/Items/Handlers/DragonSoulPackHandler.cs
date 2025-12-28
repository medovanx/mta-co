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
    [ItemHandler(P4DragonSoulBag, P6DragonSoulBag, P7WeaponSoulPack)]
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
        }
    }
}

