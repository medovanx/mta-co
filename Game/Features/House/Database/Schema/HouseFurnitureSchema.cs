// ReSharper disable InconsistentNaming

namespace MTA.Game.Features.House.Database.Schema;

/// <summary>
///     Centralized schema definition for house_furniture database table and columns.
///     All database column name references should use these constants.
/// </summary>
public static class HouseFurnitureSchema {
    /// <summary>
    ///     Table name constants
    /// </summary>
    public static class Tables {
        public const string HouseFurnitureTable = "house_furniture";
    }

    /// <summary>
    ///     Column names for the `house_furniture` table
    /// </summary>
    public static class HouseFurniture {
        public const string HouseUid = "house_uid";
        public const string FurnitureUid = "furniture_uid";
        public const string Mesh = "mesh";
        public const string X = "x";
        public const string Y = "y";
        public const string Type = "type";
    }
}