using System.Collections.Generic;
using MTA.Client;
using MTA.Database;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Elite PK Prize items that grant various rewards based on ranking and level.
    /// </summary>
    [ItemHandler(ElitePKChampion130, ElitePKSecond130, ElitePKThird130, ElitePKFourth130,
        ElitePKChampion120, ElitePKSecond120, ElitePKThird120, ElitePKFourth120,
        ElitePKChampion110, ElitePKSecond110, ElitePKThird110, ElitePKFourth110,
        ElitePKChampion1, ElitePKSecond1, ElitePKThird1, ElitePKFourth1)]
    public static class ElitePKPrizeHandler {
        private const byte RequiredInventorySlots = 17; // Need at least 23 free slots (40 - 23 = 17)

        private struct PrizeReward {
            public uint? SteedPack;
            public uint? Item1;
            public int Item1Count;
            public bool Item1Bound;
            public uint? Item2;
            public int Item2Count;
            public uint? Item3;
            public int Item3Count;
            public uint? Item4;
            public int Item4Count;
            public uint? Item5;
            public int Item5Count;
            public uint? Item6;
            public int Item6Count;
            public uint? Item7;
            public int Item7Count;
            public int? ExperienceMultiplier; // null = no exp, 1 = full, 2 = half, etc.
        }

        private static readonly Dictionary<uint, PrizeReward> PrizeRewards = new Dictionary<uint, PrizeReward> {
            // Level 130+
            { ElitePKChampion130, new PrizeReward { SteedPack = RandomSteedPack, Item1 = ElitePKItem720730, Item1Count = 3, Item1Bound = true, Item2 = RandomItemPack350k, Item2Count = 5, Item3 = RandomSuperItemPack, Item3Count = 5, Item4 = DragonBall, Item4Count = 3, Item5 = ElitePKItem720598, Item5Count = 3, Item6 = ModestyBook, Item6Count = 3 } },
            { ElitePKSecond130, new PrizeReward { Item1 = ElitePKItem720730, Item1Count = 1, Item1Bound = true, Item2 = RandomItemPack350k, Item2Count = 3, Item3 = RandomSuperItemPack, Item3Count = 3, Item4 = DragonBall, Item4Count = 1, Item5 = ElitePKItem720598, Item5Count = 2, Item6 = ModestyBook, Item6Count = 1 } },
            { ElitePKThird130, new PrizeReward { Item1 = ExpBall_B, Item1Count = 5, Item1Bound = true, Item2 = RandomItemPack350k, Item2Count = 2, Item3 = RandomSuperItemPack, Item3Count = 2, Item5 = ElitePKItem720598, Item5Count = 1, Item6 = ModestyBook, Item6Count = 1 } },
            { ElitePKFourth130, new PrizeReward { Item1 = ExpBall_B, Item1Count = 3, Item1Bound = true, Item2 = RandomItemPack350k, Item2Count = 1, Item3 = RandomSuperItemPack, Item3Count = 1, Item5 = ElitePKItem720598, Item5Count = 1, Item7 = EnduranceBook, Item7Count = 1 } },
            // Level 120+
            { ElitePKChampion120, new PrizeReward { ExperienceMultiplier = 1, Item4 = DragonBall, Item4Count = 1, Item7 = EnduranceBook, Item7Count = 5, Item3 = RandomSuperItemPack, Item3Count = 1, Item2 = RandomItemPack350k, Item2Count = 3 } },
            { ElitePKSecond120, new PrizeReward { ExperienceMultiplier = 2, Item7 = EnduranceBook, Item7Count = 3, Item3 = RandomSuperItemPack, Item3Count = 1, Item2 = RandomItemPack350k, Item2Count = 2 } },
            { ElitePKThird120, new PrizeReward { ExperienceMultiplier = 10, Item7 = EnduranceBook, Item7Count = 2, Item3 = RandomSuperItemPack, Item3Count = 1, Item2 = RandomItemPack350k, Item2Count = 1 } },
            { ElitePKFourth120, new PrizeReward { ExperienceMultiplier = 100, Item7 = EnduranceBook, Item7Count = 1, Item3 = RandomSuperItemPack, Item3Count = 1 } },
            // Level 110+
            { ElitePKChampion110, new PrizeReward { ExperienceMultiplier = 1, Item4 = DragonBall, Item4Count = 1, Item7 = EnduranceBook, Item7Count = 5, Item3 = RandomSuperItemPack, Item3Count = 1, Item2 = RandomItemPack350k, Item2Count = 3 } },
            { ElitePKSecond110, new PrizeReward { ExperienceMultiplier = 2, Item7 = EnduranceBook, Item7Count = 3, Item3 = RandomSuperItemPack, Item3Count = 1, Item2 = RandomItemPack350k, Item2Count = 2 } },
            { ElitePKThird110, new PrizeReward { ExperienceMultiplier = 10, Item7 = EnduranceBook, Item7Count = 2, Item3 = RandomSuperItemPack, Item3Count = 1, Item2 = RandomItemPack350k, Item2Count = 1 } },
            { ElitePKFourth110, new PrizeReward { ExperienceMultiplier = 100, Item7 = EnduranceBook, Item7Count = 1, Item3 = RandomSuperItemPack, Item3Count = 1 } },
            // Level 1+
            { ElitePKChampion1, new PrizeReward { ExperienceMultiplier = 1, Item4 = DragonBall, Item4Count = 1, Item7 = EnduranceBook, Item7Count = 5, Item3 = RandomSuperItemPack, Item3Count = 1, Item2 = RandomItemPack350k, Item2Count = 3 } },
            { ElitePKSecond1, new PrizeReward { ExperienceMultiplier = 2, Item7 = EnduranceBook, Item7Count = 3, Item3 = RandomSuperItemPack, Item3Count = 1, Item2 = RandomItemPack350k, Item2Count = 2 } },
            { ElitePKThird1, new PrizeReward { ExperienceMultiplier = 10, Item7 = EnduranceBook, Item7Count = 2, Item3 = RandomSuperItemPack, Item3Count = 1, Item2 = RandomItemPack350k, Item2Count = 1 } },
            { ElitePKFourth1, new PrizeReward { ExperienceMultiplier = 100, Item7 = EnduranceBook, Item7Count = 1, Item3 = RandomSuperItemPack, Item3Count = 1 } }
        };

        public static void Handle(GameState client, ConquerItem item) {
            if (client.Inventory.Count > RequiredInventorySlots) {
                client.Send("You need atleast 23 open spots!");
                return;
            }

            if (!PrizeRewards.TryGetValue(item.ID, out var reward)) {
                return;
            }

            client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);

            // Grant experience if applicable
            if (reward.ExperienceMultiplier.HasValue) {
                var multiplier = reward.ExperienceMultiplier.Value;
                int exp = multiplier switch {
                    1 => DataHolder.LevelExperience(client.Entity.Level),
                    2 => DataHolder.LevelExperience(client.Entity.Level) / 2,
                    10 => DataHolder.LevelExperience(client.Entity.Level) / 10 * 3,
                    100 => DataHolder.LevelExperience(client.Entity.Level) / 100 * 15,
                    _ => 0
                };
                if (exp > 0) {
                    client.IncreaseExperience(exp, false);
                }
            }

            // Grant items
            if (reward.SteedPack.HasValue) {
                client.Inventory.Add(reward.SteedPack.Value, 0, 1);
            }
            if (reward.Item1.HasValue) {
                if (reward.Item1Bound) {
                    client.Inventory.Add(reward.Item1.Value, 0, reward.Item1Count, true);
                } else {
                    client.Inventory.Add(reward.Item1.Value, 0, reward.Item1Count);
                }
            }
            if (reward.Item2.HasValue) {
                client.Inventory.Add(reward.Item2.Value, 0, reward.Item2Count);
            }
            if (reward.Item3.HasValue) {
                client.Inventory.Add(reward.Item3.Value, 0, reward.Item3Count);
            }
            if (reward.Item4.HasValue) {
                client.Inventory.Add(reward.Item4.Value, 0, reward.Item4Count);
            }
            if (reward.Item5.HasValue) {
                client.Inventory.Add(reward.Item5.Value, 0, reward.Item5Count);
            }
            if (reward.Item6.HasValue) {
                client.Inventory.Add(reward.Item6.Value, 0, reward.Item6Count);
            }
            if (reward.Item7.HasValue) {
                client.Inventory.Add(reward.Item7.Value, 0, reward.Item7Count);
            }
        }
    }
}

