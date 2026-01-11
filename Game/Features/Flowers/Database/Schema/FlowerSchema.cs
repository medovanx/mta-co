// ReSharper disable InconsistentNaming

namespace MTA.Game.Features.Flowers.Database.Schema;

/// <summary>
///     Centralized schema definition for flowers database table and columns.
///     All database column name references should use these constants.
/// </summary>
public static class FlowerSchema {
    /// <summary>
    ///     Table name constants
    /// </summary>
    public static class Tables {
        public const string FlowersTable = "flowers";
    }

    /// <summary>
    ///     Column names for the `flowers` table
    /// </summary>
    public static class Flowers {
        public const string EntityId = "entity_id";
        public const string RedRoses = "redroses";
        public const string RedRosesToday = "redroses_today";
        public const string Lilies = "lilies";
        public const string LiliesToday = "lilies_today";
        public const string Orchids = "orchids";
        public const string OrchidsToday = "orchids_today";
        public const string Tulips = "tulips";
        public const string TulipsToday = "tulips_today";
        public const string LastFlowerSent = "last_flower_sent";
        public const string SendDay = "send_day";
        public const string AFlower = "a_flower";
    }
}