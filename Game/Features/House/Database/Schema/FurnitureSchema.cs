namespace MTA.Game.Features.House.Database.Schema;

/// <summary>
///     Centralized schema definition for furniture database table and columns.
///     All database column name references should use these constants.
/// </summary>
public static class FurnitureSchema {
    /// <summary>
    ///     Table name constants
    /// </summary>
    public static class Tables {
        public const string FurnitureTable = "furniture";
    }

    /// <summary>
    ///     Column names for the `furniture` table
    /// </summary>
    public static class Furniture {
        public const string NpcId = "npc_id";
        public const string Type = "type";
        public const string Mesh = "mesh";
        public const string Map = "map";
        public const string X = "x";
        public const string Y = "y";
        public const string ItemId = "item_id";
        public const string Price = "price";
    }
}