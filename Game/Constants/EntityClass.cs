// ReSharper disable InconsistentNaming

namespace MTA.Game.Constants {
    /// <summary>
    /// Entity class ID constants for the game.
    /// Each class has 5 phases: base, +1, +2, +3, and +5 (Master).
    /// </summary>
    public static class EntityClass {
        // Trojan classes (base: 10)
        public const byte Trojan_1 = 10;
        public const byte Trojan_2 = 11;
        public const byte Trojan_3 = 12;
        public const byte Trojan_4 = 13;
        public const byte Trojan_Master = 15;

        // Warrior classes (base: 20)
        public const byte Warrior_1 = 20;
        public const byte Warrior_2 = 21;
        public const byte Warrior_3 = 22;
        public const byte Warrior_4 = 23;
        public const byte Warrior_Master = 25;

        // Archer classes (base: 40)
        public const byte Archer_1 = 40;
        public const byte Archer_2 = 41;
        public const byte Archer_3 = 42;
        public const byte Archer_4 = 43;
        public const byte Archer_Master = 45;

        // Ninja classes (base: 50)
        public const byte Ninja_1 = 50;
        public const byte Ninja_2 = 51;
        public const byte Ninja_3 = 52;
        public const byte Ninja_4 = 53;
        public const byte Ninja_Master = 55;

        // Monk classes (base: 60)
        public const byte Monk_1 = 60;
        public const byte Monk_2 = 61;
        public const byte Monk_3 = 62;
        public const byte Monk_4 = 63;
        public const byte Monk_Master = 65;

        // Pirate classes (base: 70)
        public const byte Pirate_1 = 70;
        public const byte Pirate_2 = 71;
        public const byte Pirate_3 = 72;
        public const byte Pirate_4 = 73;
        public const byte Pirate_Master = 75;

        // Dragon Warrior classes (base: 80)
        public const byte DragonWarrior_1 = 80;
        public const byte DragonWarrior_2 = 81;
        public const byte DragonWarrior_3 = 82;
        public const byte DragonWarrior_4 = 83;
        public const byte DragonWarrior_Master = 85;

        // Water Taoist classes (base: 130)
        public const byte WaterTaoist_1 = 130;
        public const byte WaterTaoist_2 = 131;
        public const byte WaterTaoist_3 = 132;
        public const byte WaterTaoist_4 = 133;
        public const byte WaterTaoist_Master = 135; // Water Saint

        // Fire Taoist classes (base: 140)
        public const byte FireTaoist_1 = 140;
        public const byte FireTaoist_2 = 141;
        public const byte FireTaoist_3 = 142;
        public const byte FireTaoist_4 = 143;
        public const byte FireTaoist_Master = 145; // Fire Master

        // Windwalker classes (base: 160)
        public const byte Windwalker_1 = 160;
        public const byte Windwalker_2 = 161;
        public const byte Windwalker_3 = 162;
        public const byte Windwalker_4 = 163;
        public const byte Windwalker_Master = 165;

        /// <summary>
        /// Checks if the class ID belongs to a Trojan class (10-15).
        /// </summary>
        public static bool IsTrojan(byte classId) => classId is >= Trojan_1 and <= Trojan_Master;

        /// <summary>
        /// Checks if the class ID belongs to a Warrior class (20-25).
        /// </summary>
        public static bool IsWarrior(byte classId) => classId is >= Warrior_1 and <= Warrior_Master;

        /// <summary>
        /// Checks if the class ID belongs to an Archer class (40-45).
        /// </summary>
        public static bool IsArcher(byte classId) => classId is >= Archer_1 and <= Archer_Master;

        /// <summary>
        /// Checks if the class ID belongs to a Ninja class (50-55).
        /// </summary>
        public static bool IsNinja(byte classId) => classId is >= Ninja_1 and <= Ninja_Master;

        /// <summary>
        /// Checks if the class ID belongs to a Monk class (60-65).
        /// </summary>
        public static bool IsMonk(byte classId) => classId is >= Monk_1 and <= Monk_Master;

        /// <summary>
        /// Checks if the class ID belongs to a Pirate class (70-75).
        /// </summary>
        public static bool IsPirate(byte classId) => classId is >= Pirate_1 and <= Pirate_Master;

        /// <summary>
        /// Checks if the class ID belongs to a Dragon Warrior class (80-85).
        /// </summary>
        public static bool IsDragonWarrior(byte classId) => classId is >= DragonWarrior_1 and <= DragonWarrior_Master;

        /// <summary>
        /// Checks if the class ID belongs to a Water Taoist class (130-135).
        /// </summary>
        public static bool IsWaterTaoist(byte classId) => classId is >= WaterTaoist_1 and <= WaterTaoist_Master;

        /// <summary>
        /// Checks if the class ID belongs to a Fire Taoist class (140-145).
        /// </summary>
        public static bool IsFireTaoist(byte classId) => classId is >= FireTaoist_1 and <= FireTaoist_Master;

        /// <summary>
        /// Checks if the class ID belongs to a Windwalker class (160-165).
        /// </summary>
        public static bool IsWindwalker(byte classId) => classId is >= Windwalker_1 and <= Windwalker_Master;

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

