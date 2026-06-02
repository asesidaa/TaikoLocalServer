namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueBattleCatalog
{
    public static BlueBattleCatalog Unavailable { get; } = new()
    {
        IsRawDataAvailable = false,
        EnablesBattleAdvertisement = false,
        Files = []
    };

    public bool IsRawDataAvailable { get; init; }

    public bool EnablesBattleAdvertisement { get; init; }

    public IReadOnlyList<uint> BattleNpcIds { get; init; } = [];

    public IReadOnlyList<BlueBattleNpcCatalogEntry> BattleNpcs { get; init; } = [];

    public IReadOnlyList<uint> ReleaseBattleStageIds { get; init; } = [];

    public IReadOnlyList<uint> ReleaseBattleSpecialIds { get; init; } = [];

    public uint? BattleBondsLvCap { get; init; }

    public IReadOnlyList<BlueBattleCatalogFile> Files { get; init; } = [];
}

public sealed class BlueBattleNpcCatalogEntry
{
    public uint NpcId { get; init; }

    public uint StartExp { get; init; }

    public uint InitialDpn { get; init; }
}

public sealed class BlueBattleCatalogFile
{
    public required string FileName { get; init; }

    public bool IsPresent { get; init; }

    public bool IsXmlParsed { get; init; }

    public string? ParseError { get; init; }

    public int ElementCount { get; init; }

    public int RowCount { get; init; }
}
