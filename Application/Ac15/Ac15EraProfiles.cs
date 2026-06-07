namespace TaikoLocalServer.Application.Ac15;

public static class Ac15EraProfiles
{
    private static readonly Ac15FeatureSet BlueGreenFeatures = new(
        NormalPlay: true,
        UserData: true,
        SelfBest: true,
        Crowns: true,
        InitialData: true,
        Folders: true,
        Telops: true,
        Recommendations: true,
        Taikojuku: true,
        Dani: true,
        ItemShop: true);

    public static Ac15EraProfile Blue { get; } = new(
        GameEra.Blue,
        BlueGreenFeatures,
        CreateCommonLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: true,
            HasInitialDataLegalTermsRows: true,
            HasTokkunTutorialFlagInUserData: true),
        DefaultAc15EraHooks.Instance);

    public static Ac15EraProfile Green { get; } = new(
        GameEra.Green,
        BlueGreenFeatures,
        CreateCommonLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: true,
            HasInitialDataLegalTermsRows: false,
            HasTokkunTutorialFlagInUserData: false),
        DefaultAc15EraHooks.Instance);

    public static Ac15EraProfile Yellow { get; } = new(
        GameEra.Yellow,
        BlueGreenFeatures,
        CreateCommonLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: true,
            HasInitialDataLegalTermsRows: true,
            HasTokkunTutorialFlagInUserData: false),
        DefaultAc15EraHooks.Instance);

    private static Ac15ProtocolLimits CreateCommonLimits() => new(
        SongFlagBytes: BlueProtocolBytes.SongFlagBytes,
        ToneFlagBytes: BlueProtocolBytes.ToneFlagBytes,
        TitleFlagBytes: BlueProtocolBytes.TitleFlagBytes,
        CostumeFlagBytes: BlueProtocolBytes.CostumeFlagBytes,
        DanFlagBytes: BlueProtocolBytes.DanFlagBytes,
        DanExtraFlagBytes: BlueProtocolBytes.DanExtraFlagBytes,
        ContentInfoBytes: BlueProtocolBytes.ContentInfoBytes,
        CrownPackedBytes: BlueProtocolBytes.CrownInflatedBytes,
        CrownSongCount: 1024,
        MaxFavoriteSongs: 5,
        MaxRecentSongs: 10,
        MaxSongsPerTaikojukuPack: 10,
        MaxRequestedTaikojukuSlots: 11,
        MinCourseLevel: 1,
        MaxCourseLevel: 5,
        MinNormalDanId: 1,
        MaxNormalDanId: 25,
        MinExtraDanId: 101,
        MaxKnownExtraDanId: 128,
        SafeDisplayDanFallback: 1);
}
