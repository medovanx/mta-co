using System;
using System.Drawing;
using MTA.Client;
using MTA.Network.GamePackets;
using Message = MTA.Network.GamePackets.Message;
using static MTA.Game.Constants.Items.GemPacks;
using static MTA.Game.Constants.Items.Gems;
using static MTA.Game.Constants.Items.QuestAndOther;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles GemPack items that grant random gems when used.
    /// </summary>
    [ItemHandler(RefinedGemBPack, SuperGemBPack, SuperGemPack)]
    public static class GemPackHandler {
        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count > 38) {
                return;
            }

            client.Inventory.Remove(item, Enums.ItemUse.Remove);

            var r = new Random();
            var nr = r.Next(1, 11);

            if (item.ID == RefinedGemBPack) {
                var gemId = nr switch {
                    1 => RefinedPhoenixGem,
                    2 => RefinedDragonGem,
                    3 => RefinedFuryGem,
                    4 => RefinedRainbowGem,
                    5 => RefinedKylinGem,
                    6 => RefinedVioletGem,
                    7 => RefinedMoonGem,
                    8 => RefinedTortoiseGem,
                    9 => RefinedThunderGem,
                    10 => RefinedGloryGem,
                    _ => 0u
                };

                if (gemId != 0) {
                    client.Inventory.Add(gemId, 0, 1);
                }

                client.Send(new Message("Congratultions you have got a Refined gem!", Color.Red, Message.TopLeft));
            }
            else if (item.ID == SuperGemBPack) {
                var gemId = nr switch {
                    1 => SuperPhoenixGem,
                    2 => SuperDragonGem,
                    3 => SuperFuryGem,
                    4 => SuperRainbowGem,
                    5 => SuperKylinGem,
                    6 => SuperVioletGem,
                    7 => SuperMoonGem,
                    8 => SuperTortoiseGem,
                    9 => SuperThunderGem,
                    10 => SuperGloryGem,
                    _ => 0u
                };

                if (gemId != 0) {
                    client.Inventory.Add(gemId, 0, 1);
                }

                client.Send(new Message("Congratultions you have got a Super gem!", Color.Red, Message.TopLeft));
            }
            else if (item.ID == SuperGemPack) {
                if (client.Inventory.Count > 38) {
                    return;
                }

                if (!client.Inventory.Contains(SuperGemPack, 1)) {
                    return;
                }

                client.Inventory.Remove(SuperGemPack, 1);

                var nr2 = r.Next(1, 11);

                var gemId = nr2 switch {
                    1 => SuperPhoenixGem,
                    2 => SuperDragonGem,
                    3 => SuperFuryGem,
                    4 => SuperRainbowGem,
                    5 => SuperKylinGem,
                    6 => SuperVioletGem,
                    7 => SuperMoonGem,
                    8 => SuperTortoiseGem,
                    9 => SuperThunderGem,
                    10 => SuperGloryGem,
                    _ => 0u
                };

                if (gemId == 0) return;
                // SuperTortoiseGem gives 2, others give 1
                var amount = gemId == SuperTortoiseGem ? 2u : 1u;
                client.Inventory.Add(gemId, 0, (byte)amount);
            }
        }
    }
}
