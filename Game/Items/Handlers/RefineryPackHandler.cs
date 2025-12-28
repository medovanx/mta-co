using System.Collections.Generic;
using MTA.Client;
using MTA.Network;
using MTA.Network.GamePackets;
using static MTA.Game.ItemConstants;

namespace MTA.Game.Items.Handlers {
    /// <summary>
    /// Handles Refinery Pack items that transform into refined items when used.
    /// </summary>
    [ItemHandler(
        // Penetration
        PenetrationPrimaryPackBracelet, PenetrationMediumPackBracelet, PenetrationSuperiorPackBracelet,
        PenetrationPrimaryPackHeadgear, PenetrationMediumPackHeadgear, PenetrationSuperiorPackHeadgear,
        PenetrationPrimaryPackBag, PenetrationMediumPackBag, PenetrationSuperiorPackBag,
        // Skill Critical Strike
        SkillCriticalStrikePrimaryPackBacksword, SkillCriticalStrikeMediumPackBacksword, SkillCriticalStrikeSuperiorPackBacksword,
        SkillCriticalStrikePrimaryPackBracelet, SkillCriticalStrikeMediumPackBracelet, SkillCriticalStrikeSuperiorPackBracelet,
        // Block
        BlockPrimaryPackShield, BlockMediumPackShield, BlockSuperiorPackShield,
        BlockPrimaryPackHeadgear, BlockMediumPackHeadgear, BlockSuperiorPackHeadgear,
        // CriticalStrike
        CriticalStrikePrimaryPackBow, CriticalStrikeMediumPackBow, CriticalStrikeSuperiorPackBow,
        CriticalStrikePrimaryPack1Handed, CriticalStrikeMediumPack1Handed, CriticalStrikeSuperiorPack1Handed,
        CriticalStrikePrimaryPack2Handed, CriticalStrikeMediumPack2Handed, CriticalStrikeSuperiorPack2Handed,
        CriticalStrikePrimaryPackRing, CriticalStrikePrimaryPackRing2, CriticalStrikeMediumPackRing2,
        // Detoxication
        DetoxicationPrimaryPackNecklace, DetoxicationMediumPackNecklace, DetoxicationSuperiorPackNecklace,
        DetoxicationPrimaryPackHeadgear, DetoxicationMediumPackHeadgear, DetoxicationSuperiorPackHeadgear,
        DetoxicationPrimaryPackBag, DetoxicationMediumPackBag, DetoxicationSuperiorPackBag,
        DetoxicationPrimaryPackArmor, DetoxicationMediumPackArmor, DetoxicationSuperiorPackArmor,
        DetoxicationPrimaryPackBoots, DetoxicationMediumPackBoots, DetoxicationSuperiorPackBoots,
        // Breakthrough
        BreakthroughPrimaryPack1Handed, BreakthroughMediumPack1Handed, BreakthroughSuperiorPack1Handed,
        BreakthroughPrimaryPack2Handed, BreakthroughMediumPack2Handed, BreakthroughSuperiorPack2Handed,
        BreakthroughPrimaryPackBracelet, BreakthroughMediumPackBracelet, BreakthroughSuperiorPackBracelet,
        BreakthroughPrimaryPackBow, BreakthroughMediumPackBow, BreakthroughSuperiorPackBow,
        BreakthroughPrimaryPackRing, BreakthroughMediumPackRing, BreakthroughSuperiorPackRing,
        // Counteraction
        CounteractionPrimaryPackArmor, CounteractionMediumPackArmor, CounteractionSuperiorPackArmor,
        CounteractionPrimaryPackBag, CounteractionMediumPackBag, CounteractionSuperiorPackBag,
        CounteractionPrimaryPackNecklace, CounteractionMediumPackNecklace, CounteractionSuperiorPackNecklace,
        // Immunity
        ImmunityPrimaryPackBoots, ImmunityMediumPackBoots, ImmunitySuperiorPackBoots,
        ImmunityPrimaryPackArmor, ImmunityMediumPackArmor, ImmunitySuperiorPackArmor,
        // Intensification
        IntensificationPrimaryPackHeadgear, IntensificationMediumPackHeadgear, IntensificationSuperiorPackHeadgear,
        // M-Defense
        MDefensePrimaryPackNecklace, MDefenseMediumPackNecklace, MDefenseSuperiorPackNecklace,
        MDefensePrimaryPackBag, MDefenseMediumPackBag, MDefenseSuperiorPackBag,
        MDefensePrimaryPackBracelet, MDefenseMediumPackBracelet, MDefenseSuperiorPackBracelet,
        MDefensePrimaryPackRing, MDefenseMediumPackRing, MDefenseSuperiorPackRing,
        // GainRefineryItem cases
        RefineryPack724130, RefineryPack724131, RefineryPack724132, RefineryPack724133, RefineryPack724134, RefineryPack724135,
        RefineryPack724151, RefineryPack724152, RefineryPack724153, RefineryPack724154, RefineryPack724155, RefineryPack724156,
        RefineryPack724157, RefineryPack724158, RefineryPack724159, RefineryPack724160, RefineryPack724161, RefineryPack724162,
        RefineryPack724163, RefineryPack724164, RefineryPack724165, RefineryPack724166, RefineryPack724167, RefineryPack724168,
        RefineryPack724169, RefineryPack724170, RefineryPack724171, RefineryPack724172, RefineryPack724173, RefineryPack724174,
        RefineryPack724175, RefineryPack724176, RefineryPack724177, RefineryPack724178, RefineryPack724179, RefineryPack724180,
        RefineryPack724181, RefineryPack724182, RefineryPack724183, RefineryPack724184, RefineryPack724185, RefineryPack724186,
        RefineryPack724190, RefineryPack724191, RefineryPack724192,
        RefineryPack725055, RefineryPack725056, RefineryPack725057, RefineryPack725058
    )]
    public static class RefineryPackHandler {
        private static readonly Dictionary<uint, int> ItemOffsets = new Dictionary<uint, int> {
            // Penetration - Bracelet
            { PenetrationPrimaryPackBracelet, 1260 }, { PenetrationMediumPackBracelet, 1260 }, { PenetrationSuperiorPackBracelet, 1260 },
            // Penetration - Headgear
            { PenetrationPrimaryPackHeadgear, 696 }, { PenetrationMediumPackHeadgear, 696 }, { PenetrationSuperiorPackHeadgear, 696 },
            // Penetration - Bag
            { PenetrationPrimaryPackBag, 237 }, { PenetrationMediumPackBag, 237 }, { PenetrationSuperiorPackBag, 237 },
            // Skill Critical Strike - Backsword
            { SkillCriticalStrikePrimaryPackBacksword, 1287 }, { SkillCriticalStrikeMediumPackBacksword, 1287 }, { SkillCriticalStrikeSuperiorPackBacksword, 1287 },
            // Skill Critical Strike - Bracelet
            { SkillCriticalStrikePrimaryPackBracelet, 230 }, { SkillCriticalStrikeMediumPackBracelet, 230 }, { SkillCriticalStrikeSuperiorPackBracelet, 230 },
            // Block - Shield
            { BlockPrimaryPackShield, 689 }, { BlockMediumPackShield, 689 }, { BlockSuperiorPackShield, 689 },
            // Block - Headgear
            { BlockPrimaryPackHeadgear, 208 }, { BlockMediumPackHeadgear, 208 }, { BlockSuperiorPackHeadgear, 208 },
            // CriticalStrike - Bow
            { CriticalStrikePrimaryPackBow, 749 }, { CriticalStrikeMediumPackBow, 749 }, { CriticalStrikeSuperiorPackBow, 749 },
            // CriticalStrike - 1 Handed weapons
            { CriticalStrikePrimaryPack1Handed, 751 }, { CriticalStrikeMediumPack1Handed, 751 }, { CriticalStrikeSuperiorPack1Handed, 751 },
            // CriticalStrike - 2 Handed weapons
            { CriticalStrikePrimaryPack2Handed, 753 }, { CriticalStrikeMediumPack2Handed, 753 }, { CriticalStrikeSuperiorPack2Handed, 753 },
            // CriticalStrike - 2 Ring
            { CriticalStrikePrimaryPackRing, 200 },
            { CriticalStrikePrimaryPackRing2, 223 }, { CriticalStrikeMediumPackRing2, 223 },
            // Detoxication - Necklace
            { DetoxicationPrimaryPackNecklace, 667 }, { DetoxicationMediumPackNecklace, 667 }, { DetoxicationSuperiorPackNecklace, 667 },
            // Detoxication - Headgear
            { DetoxicationPrimaryPackHeadgear, 811 }, { DetoxicationMediumPackHeadgear, 811 }, { DetoxicationSuperiorPackHeadgear, 811 },
            // Detoxication - Bag
            { DetoxicationPrimaryPackBag, 683 }, { DetoxicationMediumPackBag, 683 }, { DetoxicationSuperiorPackBag, 683 },
            // Detoxication - Armor
            { DetoxicationPrimaryPackArmor, 813 }, { DetoxicationMediumPackArmor, 813 }, { DetoxicationSuperiorPackArmor, 813 },
            // Detoxication - Boots
            { DetoxicationPrimaryPackBoots, 815 }, { DetoxicationMediumPackBoots, 815 }, { DetoxicationSuperiorPackBoots, 815 },
            // Breakthrough - 1 Handed weapons
            { BreakthroughPrimaryPack1Handed, 789 }, { BreakthroughMediumPack1Handed, 789 }, { BreakthroughSuperiorPack1Handed, 789 },
            // Breakthrough - 2 Handed weapons
            { BreakthroughPrimaryPack2Handed, 791 }, { BreakthroughMediumPack2Handed, 791 }, { BreakthroughSuperiorPack2Handed, 791 },
            // Breakthrough - Bracelet
            { BreakthroughPrimaryPackBracelet, 162 }, { BreakthroughMediumPackBracelet, 162 }, { BreakthroughSuperiorPackBracelet, 162 },
            // Breakthrough - Bow
            { BreakthroughPrimaryPackBow, 793 }, { BreakthroughMediumPackBow, 793 }, { BreakthroughSuperiorPackBow, 793 },
            // Breakthrough - Ring
            { BreakthroughPrimaryPackRing, 266 }, { BreakthroughMediumPackRing, 266 }, { BreakthroughSuperiorPackRing, 266 },
            // Counteraction - Armor
            { CounteractionPrimaryPackArmor, 331 }, { CounteractionMediumPackArmor, 331 }, { CounteractionSuperiorPackArmor, 331 },
            // Counteraction - Bag
            { CounteractionPrimaryPackBag, 161 }, { CounteractionMediumPackBag, 161 }, { CounteractionSuperiorPackBag, 161 },
            // Counteraction - Necklace
            { CounteractionPrimaryPackNecklace, 274 }, { CounteractionMediumPackNecklace, 274 }, { CounteractionSuperiorPackNecklace, 274 },
            // Immunity - Boots
            { ImmunityPrimaryPackBoots, 225 }, { ImmunityMediumPackBoots, 225 }, { ImmunitySuperiorPackBoots, 225 },
            // Immunity - Armor
            { ImmunityPrimaryPackArmor, 294 }, { ImmunityMediumPackArmor, 294 }, { ImmunitySuperiorPackArmor, 294 },
            // Intensification - Headgear
            { IntensificationPrimaryPackHeadgear, 301 }, { IntensificationMediumPackHeadgear, 301 }, { IntensificationSuperiorPackHeadgear, 301 },
            // M-Defense - Necklace
            { MDefensePrimaryPackNecklace, 32 }, { MDefenseMediumPackNecklace, 32 }, { MDefenseSuperiorPackNecklace, 32 },
            // M-Defense - Bag
            { MDefensePrimaryPackBag, 34 }, { MDefenseMediumPackBag, 34 }, { MDefenseSuperiorPackBag, 34 },
            // M-Defense - Bracelet
            { MDefensePrimaryPackBracelet, 36 }, { MDefenseMediumPackBracelet, 36 }, { MDefenseSuperiorPackBracelet, 36 },
            // M-Defense - Ring
            { MDefensePrimaryPackRing, 38 }, { MDefenseMediumPackRing, 38 }, { MDefenseSuperiorPackRing, 38 }
        };

        public static void Handle(GameState client, ConquerItem item) {
            // Check if this item uses GainRefineryItem
            if (item.ID >= RefineryPack724130 && item.ID <= RefineryPack725058 &&
                (item.ID <= RefineryPack724135 || (item.ID >= RefineryPack724151 && item.ID <= RefineryPack724186) ||
                 (item.ID >= RefineryPack724190 && item.ID <= RefineryPack724192) ||
                 (item.ID >= RefineryPack725055 && item.ID <= RefineryPack725058))) {
                PacketHandler.GainRefineryItem(item, client);
                return;
            }

            // Handle items with offsets
            if (ItemOffsets.TryGetValue(item.ID, out var offset)) {
                client.Inventory.Remove(item, Enums.ItemUse.RemoveFromStack);
                var extra = PacketHandler.GetNextRefineryItem();
                var newItemID = (uint)(item.ID + extra + offset);

                // Special cases
                if (item.ID >= DetoxicationPrimaryPackNecklace && item.ID <= DetoxicationSuperiorPackNecklace) {
                    // Detoxication - Necklace
                    if (newItemID is 724348 or 724349)
                        newItemID += 150;
                }
                else if (item.ID >= BreakthroughPrimaryPack1Handed && item.ID <= BreakthroughSuperiorPack1Handed) {
                    // Breakthrough - 1 Handed weapons
                    if (newItemID == 724449)
                        newItemID = 724445;
                }
                else if (item.ID >= BreakthroughPrimaryPackRing && item.ID <= BreakthroughSuperiorPackRing) {
                    // Breakthrough - Ring
                    if (newItemID >= 724466)
                        newItemID += 4;
                }

                if (item.Bound)
                    client.Inventory.AddBound(newItemID, 0, 1);
                else
                    client.Inventory.Add(newItemID, 0, 1);
            }
        }
    }
}

