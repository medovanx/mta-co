namespace MTA.Game.Features.House.Database.Schema;

/// <summary>
///     Centralized schema definition for house database table and columns.
///     All database column name references should use these constants.
/// </summary>
public static class HouseSchema {
    /// <summary>
    ///     Table name constants
    /// </summary>
    public static class Tables {
        public const string HouseTable = "house";
    }

    /// <summary>
    ///     Column names for the `house` table
    /// </summary>
    public static class House {
        public const string Uid = "uid";
        public const string Name = "name";
        public const string Id = "id";
        public const string MapType = "map_type";
        public const string Level = "level";
        public const string Furniture = "furniture";
    }
}