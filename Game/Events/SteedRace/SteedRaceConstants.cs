namespace MTA.Game.Events.SteedRace;

/// <summary>
///     Shared constants for the Steed Race event
/// </summary>
public static class SteedRaceConstants {
    /// <summary>
    ///     Race settings for all available race maps
    ///     Each array contains: [MapID, StartX, StartY, EndX, EndY, EndRadius, GateX, GateY, GateMesh, PotionArea1X,
    ///     PotionArea1Y, PotionArea1Radius, PotionArea2X, PotionArea2Y, PotionArea2Radius, PotionArea3X, PotionArea3Y,
    ///     PotionArea3Radius]
    /// </summary>
    public static readonly uint[][] RaceSettings = [
        [
            (uint)Enums.Maps.MarketRace, 88, 149,
            420, 431, 4,
            65, 174, 621,
            123, 243, 60,
            214, 334, 70,
            346, 459, 100
        ],
        [
            (uint)Enums.Maps.IceRace, 175, 250,
            200, 153, 6,
            154, 267, 621,
            146, 392, 70,
            283, 351, 100,
            295, 079, 100
        ],
        [
            (uint)Enums.Maps.IslandRace, 60, 400,
            899, 816, 10,
            96, 392, 621,
            220, 234, 200,
            472, 160, 200,
            777, 464, 300
        ],
        [
            (uint)Enums.Maps.DungeonRace, 450, 520,
            682, 484, 10,
            435, 559, 621,
            471, 759, 200,
            714, 598, 250,
            489, 679, 20
        ],
        [
            (uint)Enums.Maps.LavaRace, 150, 350,
            330, 170, 6,
            101, 397, 623,
            327, 553, 100,
            526, 477, 200,
            283, 275, 100
        ]
    ];
}