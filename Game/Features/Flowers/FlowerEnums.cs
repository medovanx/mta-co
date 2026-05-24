namespace MTA.Game.Features.Flowers;

/// <summary>
///     Logical type IDs for the four girl-side flower kinds. Boy-side gifts use
///     <see cref="Kisses.KissType" /> in the Kisses feature folder.
///     The byte values double as the screen-index for the flower-fairy ranking screen
///     (used by <see cref="Flowers.SendScreenValue" />). Boy-side gifts occupy the upper
///     half of the same SendFlower-packet FType field via <see cref="Kisses.KissesT" />.
///     <see cref="Unknown" /> is the sentinel for unmatched inputs.
/// </summary>
public enum FlowerType : byte {
    RedRoses = 0,
    Lilies = 1,
    Orchids = 2,
    Tulips = 3,
    Unknown = 255
}

/// <summary>
///     Visual effect IDs (1-4) for girl-side flowers in the SendFlower packet.
///     Boy-side gifts use <see cref="Kisses.KissEffect" /> with the same byte values
///     intentionally — the wire format reuses the byte for either gender.
/// </summary>
public enum FlowerEffect : byte {
    None = 0,
    Rose = 1,
    Lilies = 2,
    Orchids = 3,
    Tulips = 4
}
