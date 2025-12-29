// ReSharper disable InconsistentNaming

namespace MTA.Game.Constants {
    /// <summary>
    /// Entity class ID constants for the game.
    /// Each class has 5 phases: base, +1, +2, +3, and +5 (Final).
    /// </summary>
    public static class EntityClass {
        // Trojan classes (base: 10)
        public const byte Trojan_1 = 10;
        public const byte Trojan_Veteran_2 = 11;
        public const byte Trojan_Tiger_3 = 12;
        public const byte Trojan_Dragon_4 = 13;
        public const byte Trojan_Master_5 = 15;

        // Warrior classes (base: 20)
        public const byte Warrior_1 = 20;
        public const byte Warrior_Brass_2 = 21;
        public const byte Warrior_Silver_3 = 22;
        public const byte Warrior_Gold_4 = 23;
        public const byte Warrior_King_5 = 25;

        // Archer classes (base: 40)
        public const byte Archer_1 = 40;
        public const byte Archer_Eagle_2 = 41;
        public const byte Archer_Tiger_3 = 42;
        public const byte Archer_Dragon_4 = 43;
        public const byte Archer_Master_5 = 45;

        // Ninja classes (base: 50)
        public const byte Ninja_1 = 50;
        public const byte Ninja_Middle_2 = 51;
        public const byte Ninja_Dark_3 = 52;
        public const byte Ninja_Mystic_4 = 53;
        public const byte Ninja_Master_5 = 55;

        // Monk classes (base: 60)
        public const byte Monk_1 = 60;
        public const byte Monk_Dhyana_2 = 61;
        public const byte Monk_Dharma_3 = 62;
        public const byte Monk_Prajna_4 = 63;
        public const byte Monk_Nirvana_5 = 65;

        // Pirate classes (base: 70)
        public const byte Pirate_1 = 70;
        public const byte Pirate_Gunner_2 = 71;
        public const byte Pirate_Quartermaster_3 = 72;
        public const byte Pirate_Captain_4 = 73;
        public const byte Pirate_Lord_5 = 75;

        // Dragon Warrior classes (base: 80)
        public const byte DragonWarrior_1 = 80;
        public const byte DragonWarrior_Expert_2 = 81;
        public const byte DragonWarrior_Elite_3 = 82;
        public const byte DragonWarrior_Master_4 = 83;
        public const byte DragonWarrior_King_5 = 85;

        // Water Taoist classes (base: 130)
        public const byte Water_1 = 130;
        public const byte Water_Taoist_2 = 131;
        public const byte Water_Wizard_3 = 132;
        public const byte Water_Master_4 = 133;
        public const byte Water_Saint_5 = 135; // Water Saint

        // Fire Taoist classes (base: 140)
        public const byte Fire_1 = 140;
        public const byte Fire_Taoist_2 = 141;
        public const byte Fire_Wizard_3 = 142;
        public const byte Fire_Master_4 = 143;
        public const byte Fire_Saint_5 = 145; // Fire Master

        // Windwalker classes (base: 160)
        public const byte Windwalker_Guard_1 = 160;
        public const byte Windwalker_Officer_2 = 161;
        public const byte Windwalker_Supervisor_3 = 162;
        public const byte Windwalker_Manager_4 = 163;
        public const byte Windwalker_Lord_5 = 165;

        /// <summary>
        /// Checks if the class ID belongs to a Trojan class (10-15).
        /// </summary>
        public static bool IsTrojan(byte classId) => classId is >= Trojan_1 and <= Trojan_Master_5;

        /// <summary>
        /// Checks if the class ID belongs to a Warrior class (20-25).
        /// </summary>
        public static bool IsWarrior(byte classId) => classId is >= Warrior_1 and <= Warrior_King_5;

        /// <summary>
        /// Checks if the class ID belongs to an Archer class (40-45).
        /// </summary>
        public static bool IsArcher(byte classId) => classId is >= Archer_1 and <= Archer_Master_5;

        /// <summary>
        /// Checks if the class ID belongs to a Ninja class (50-55).
        /// </summary>
        public static bool IsNinja(byte classId) => classId is >= Ninja_1 and <= Ninja_Master_5;

        /// <summary>
        /// Checks if the class ID belongs to a Monk class (60-65).
        /// </summary>
        public static bool IsMonk(byte classId) => classId is >= Monk_1 and <= Monk_Nirvana_5;

        /// <summary>
        /// Checks if the class ID belongs to a Pirate class (70-75).
        /// </summary>
        public static bool IsPirate(byte classId) => classId is >= Pirate_1 and <= Pirate_Lord_5;

        /// <summary>
        /// Checks if the class ID belongs to a Dragon Warrior class (80-85).
        /// </summary>
        public static bool IsDragonWarrior(byte classId) => classId is >= DragonWarrior_1 and <= DragonWarrior_King_5;

        /// <summary>
        /// Checks if the class ID belongs to a Water Taoist class (130-135).
        /// </summary>
        public static bool IsWaterTaoist(byte classId) => classId is >= Water_1 and <= Water_Saint_5;

        /// <summary>
        /// Checks if the class ID belongs to a Fire Taoist class (140-145).
        /// </summary>
        public static bool IsFireTaoist(byte classId) => classId is >= Fire_1 and <= Fire_Saint_5;

        /// <summary>
        /// Checks if the class ID belongs to a Windwalker class (160-165).
        /// </summary>
        public static bool IsWindwalker(byte classId) => classId is >= Windwalker_Guard_1 and <= Windwalker_Lord_5;

        /// <summary>
        /// Checks if the class ID belongs to any Taoist class (Water or Fire: 130-145).
        /// </summary>
        public static bool IsTaoist(byte classId) => IsWaterTaoist(classId) || IsFireTaoist(classId);

        /// <summary>
        /// Checks if the class ID is a Master class (ends in 5).
        /// </summary>
        public static bool IsMaster(byte classId) => classId % 10 == 5;
    }
}