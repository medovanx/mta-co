using MTA.Client;
using MTA.Franko;
using MTA.Network.GamePackets;
using _String = MTA.Network.GamePackets._String;
using QuestID = MTA.Network.GamePackets.QuestID;
using static MTA.Game.Constants.Items.QuestAndOther;
using static MTA.Kernel;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles MagnoliaSeed items that complete magnolia quests when used at specific locations.
    /// </summary>
    [ItemHandler(NormalMagnoliaSeed, RefinedMagnoliaSeed, UniqueMagnoliaSeed, EliteMagnoliaSeed, SuperMagnoliaSeed)]
    public static class MagnoliaSeedHandler {
        private const ushort LocationX1 = 99;
        private const ushort LocationY1 = 113;
        private const ushort LocationX2 = 105;
        private const ushort LocationY2 = 99;
        private const byte MaxDistance = 5;

        public static void Handle(GameState client, ConquerItem item) {
            if (GetDistance(client.Entity.X, client.Entity.Y, LocationX1, LocationY1) > MaxDistance &&
                GetDistance(client.Entity.X, client.Entity.Y, LocationX2, LocationY2) > MaxDistance) {
                client.MessageBox("You are too far away!");
                return;
            }

            var seedType = item.ID switch {
                NormalMagnoliaSeed => "Normal",
                RefinedMagnoliaSeed => "Refined",
                UniqueMagnoliaSeed => "Unique",
                EliteMagnoliaSeed => "Elite",
                SuperMagnoliaSeed => "Super",
                _ => "Normal"
            };

            client.ProgressBar = new ProgressBar(client, seedType, p => {
                p.Inventory.Remove(item, Enums.ItemUse.Remove);

                var expDivisor = item.ID switch {
                    NormalMagnoliaSeed => 5u,
                    RefinedMagnoliaSeed => 4u,
                    UniqueMagnoliaSeed => 3u,
                    EliteMagnoliaSeed => 2u,
                    SuperMagnoliaSeed => 1u,
                    _ => 5u
                };

                p.IncreaseExperience(p.ExpBall / expDivisor, true);

                switch (item.ID) {
                    case UniqueMagnoliaSeed:
                        p.Entity.ConquerPoints += 300;
                        break;
                    case EliteMagnoliaSeed:
                        p.Entity.ConquerPoints += 400;
                        break;
                    case SuperMagnoliaSeed:
                        p.Entity.ConquerPoints += 1000;
                        break;
                }

                var str = new _String(true) {
                    Type = 10,
                    UID = p.Entity.UID
                };
                str.Texts.Add("accession3");
                str.Texts.Add("end_task");
                p.Inventory.Add(ChiToken, 0, 1);
                p.SendScreen(str.ToArray());
                p.Quests.FinishQuest(QuestID.Magnolias);
            });
        }
    }
}