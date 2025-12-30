using MTA.Game.Constants;

namespace MTA.Game.Events.GuildWar;

/// <summary>
///     Constants for Guild War event
/// </summary>
public static class GuildWarConstants {
    /// <summary>
    ///     Pole NPC ID
    /// </summary>
    public const uint PoleNpcId = 810;

    /// <summary>
    ///     Left Gate NPC ID
    /// </summary>
    public const uint LeftGateNpcId = 516074;

    /// <summary>
    ///     Right Gate NPC ID
    /// </summary>
    public const uint RightGateNpcId = 516075;

    /// <summary>
    ///     Event duration in minutes
    /// </summary>
    public const int EventDurationMinutes = 60;

    /// <summary>
    ///     Score broadcast interval in seconds
    /// </summary>
    public const int ScoreBroadcastIntervalSeconds = 3;

    /// <summary>
    ///     Pole repair interval in seconds
    /// </summary>
    public const int PoleRepairIntervalSeconds = 10;

    /// <summary>
    ///     HP restored per repair interval
    /// </summary>
    public const uint PoleRepairHpPerInterval = 10000;

    /// <summary>
    ///     Silver cost per HP (10 HP = 1 Silver)
    /// </summary>
    public const uint PoleRepairSilverPerHp = 10;

    /// <summary>
    ///     West gate closed mesh
    /// </summary>
    public const ushort WestGateClosedMesh = 241;

    /// <summary>
    ///     West gate open mesh
    /// </summary>
    public const ushort WestGateOpenMesh = 251;

    /// <summary>
    ///     West gate broken mesh (same as open)
    /// </summary>
    public const ushort WestGateBrokenMesh = 251;

    /// <summary>
    ///     East gate closed mesh
    /// </summary>
    public const ushort EastGateClosedMesh = 271;

    /// <summary>
    ///     East gate open mesh
    /// </summary>
    public const ushort EastGateOpenMesh = 281;

    /// <summary>
    ///     East gate broken mesh (same as open)
    /// </summary>
    public const ushort EastGateBrokenMesh = 281;

    /// <summary>
    ///     West gate bomb location X coordinate
    /// </summary>
    public const ushort WestGateBombX = 165;

    /// <summary>
    ///     West gate bomb location Y coordinate
    /// </summary>
    public const ushort WestGateBombY = 213;

    /// <summary>
    ///     East gate bomb location X coordinate
    /// </summary>
    public const ushort EastGateBombX = 225;

    /// <summary>
    ///     East gate bomb location Y coordinate
    /// </summary>
    public const ushort EastGateBombY = 178;

    /// <summary>
    ///     Bomb location tolerance in paces (for both X and Y)
    /// </summary>
    public const ushort BombLocationTolerance = 5;

    /// <summary>
    ///     Number of bombs required to destroy a gate
    /// </summary>
    public const int BombsRequiredToDestroyGate = 4;
}