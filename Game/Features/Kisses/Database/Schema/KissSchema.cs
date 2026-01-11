// ReSharper disable InconsistentNaming

namespace MTA.Game.Features.Kisses.Database.Schema;

/// <summary>
///     Centralized schema definition for kisses database table and columns.
///     All database column name references should use these constants.
/// </summary>
public static class KissSchema {
    /// <summary>
    ///     Table name constants
    /// </summary>
    public static class Tables {
        public const string KissesTable = "kisses";
    }

    /// <summary>
    ///     Column names for the `kisses` table
    /// </summary>
    public static class Kisses {
        public const string EntityId = "entity_id";
        public const string KissesCount = "kisses";
        public const string KissesToday = "kisses_today";
        public const string Letters = "letters";
        public const string LettersToday = "letters_today";
        public const string Wine = "wine";
        public const string WineToday = "wine_today";
        public const string Jades = "jades";
        public const string JadesToday = "jades_today";
        public const string LastKissesSent = "last_kiss_sent";
    }
}