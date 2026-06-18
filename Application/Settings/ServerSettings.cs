namespace TaikoLocalServer.Application.Settings;

public sealed class ServerSettings
{
    public bool EnableMoreSongs { get; set; }

    public int MoreSongsSize { get; set; } = TaikoLocalServer.Domain.DomainConstants.MusicIdMaxExpanded;

    public Dictionary<string, EraSettings> Eras { get; set; } = new();
}

public sealed class EraSettings
{
    public bool Enabled { get; set; }

    public bool AutoExtractCatalog { get; set; } = true;

    public string GameDataPath { get; set; } = string.Empty;

    public string? CustomizationNameDataPath { get; set; }

    public bool? EnableShop { get; set; }

    public uint? ActiveShopSeasonId { get; set; }

    public bool? EnableDonChallenge { get; set; }

    public string? ActiveDonChallengeBundleId { get; set; }

    public bool IsDonChallengeEnabled()
        => EnableDonChallenge.GetValueOrDefault();

    public string? GetActiveDonChallengeBundleId()
        => ActiveDonChallengeBundleId;
}
