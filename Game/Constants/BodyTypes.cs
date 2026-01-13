namespace MTA.Game.Constants;

/// <summary>
///     Centralized body type constants and helper methods
/// </summary>
public static class BodyTypes {
    // Boy body IDs
    public const ushort BoySmall = 1003;
    public const ushort BoyBig = 1004;

    // Girl body IDs
    public const ushort GirlSmall = 2001;
    public const ushort GirlBig = 2002;

    /// <summary>
    ///     Determines if a body ID represents a boy
    /// </summary>
    public static bool IsBoy(ushort body) {
        return body is BoySmall or BoyBig;
    }

    /// <summary>
    ///     Determines if a body ID represents a girl
    /// </summary>
    public static bool IsGirl(ushort body) {
        return body is GirlSmall or GirlBig;
    }
}
