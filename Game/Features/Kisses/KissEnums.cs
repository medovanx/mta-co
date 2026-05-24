namespace MTA.Game.Features.Kisses;

/// <summary>
///     Logical type IDs for the four boy-side gift kinds. Girl-side flowers use
///     <see cref="Flowers.FlowerType" /> in the Flowers feature folder.
///     The byte values double as the screen-index for the kiss-fairy ranking screen
///     (used by <see cref="Kisses.SendScreenValue" />), parallel to
///     <see cref="Flowers.FlowerType" /> for girls.
///     <see cref="Unknown" /> is the sentinel for unmatched inputs.
/// </summary>
public enum KissType : byte {
    Kisses = 0,
    Letters = 1,
    Wine = 2,
    Jades = 3,
    Unknown = 255
}

/// <summary>
///     Visual effect IDs (1-4) for boy-side gifts in the SendFlower packet.
///     Girl-side flowers use <see cref="Flowers.FlowerEffect" /> with the same byte
///     values intentionally — the wire format reuses the byte for either gender.
/// </summary>
public enum KissEffect : byte {
    None = 0,
    Kiss = 1,
    Love = 2,
    Wine = 3,
    Jade = 4
}

/// <summary>
///     Boy-side FType offsets (4-7) for the cross-gender SendFlower packet.
///     Sit in the upper half of the same field as <see cref="Flowers.FlowerType" />
///     (girl 0-3) so the client can distinguish sender gender from a single byte.
/// </summary>
public enum KissesT : byte {
    Kiss = 4,
    Love = 5,
    Wine = 6,
    Jade = 7
}
