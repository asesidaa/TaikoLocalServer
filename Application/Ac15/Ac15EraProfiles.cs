using System.Diagnostics.CodeAnalysis;

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

    private static readonly Ac15FeatureSet RedFeatures = BlueGreenFeatures with
    {
        ItemShop = false
    };

    private static readonly Ac15FeatureSet WhiteFeatures = RedFeatures;

    private static readonly Ac15FeatureSet MurasakiFeatures = RedFeatures;

    private static readonly Ac15FeatureSet KimidoriFeatures = MurasakiFeatures with
    {
        InitialData = false,
        Taikojuku = false
    };

    public static Ac15EraProfile Blue { get; } = new(
        GameEra.Blue,
        BlueGreenFeatures,
        CreateCommonLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: true,
            HasInitialDataLegalTermsRows: true,
            HasTokkunTutorialFlagInUserData: true),
        Ac15ProfileCapabilities.CurrentFull);

    public static Ac15EraProfile Green { get; } = new(
        GameEra.Green,
        BlueGreenFeatures,
        CreateCommonLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: true,
            HasInitialDataLegalTermsRows: false,
            HasTokkunTutorialFlagInUserData: false),
        Ac15ProfileCapabilities.CurrentFull);

    public static Ac15EraProfile Yellow { get; } = new(
        GameEra.Yellow,
        BlueGreenFeatures,
        CreateCommonLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: true,
            HasInitialDataLegalTermsRows: true,
            HasTokkunTutorialFlagInUserData: true),
        Ac15ProfileCapabilities.CurrentFull);

    public static Ac15EraProfile Red { get; } = new(
        GameEra.Red,
        RedFeatures,
        CreateCommonLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: false,
            HasInitialDataLegalTermsRows: true,
            HasTokkunTutorialFlagInUserData: true),
        Ac15ProfileCapabilities.CurrentFull);

    public static Ac15EraProfile White { get; } = new(
        GameEra.White,
        WhiteFeatures,
        CreateWhiteLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: false,
            HasInitialDataLegalTermsRows: false,
            HasTokkunTutorialFlagInUserData: false),
        Ac15ProfileCapabilities.CurrentWithoutTitlePlate);

    public static Ac15EraProfile Murasaki { get; } = new(
        GameEra.Murasaki,
        MurasakiFeatures,
        CreateMurasakiLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: false,
            HasInitialDataLegalTermsRows: false,
            HasTokkunTutorialFlagInUserData: false),
        Ac15ProfileCapabilities.CurrentWithoutTitlePlate);

    public static Ac15EraProfile Kimidori { get; } = new(
        GameEra.Kimidori,
        KimidoriFeatures,
        CreateKimidoriLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: false,
            HasInitialDataLegalTermsRows: false,
            HasTokkunTutorialFlagInUserData: false),
        Ac15ProfileCapabilities.CurrentWithoutTitlePlateOrTaikojuku);

    public static int? GetMaxFavoriteSongs(GameEra era)
        => TryGet(era, out var profile) ? profile.Limits.MaxFavoriteSongs : null;

    public static bool TryGet(GameEra era, [NotNullWhen(true)] out Ac15EraProfile? profile)
    {
        profile = era switch
        {
            GameEra.Blue => Blue,
            GameEra.Green => Green,
            GameEra.Yellow => Yellow,
            GameEra.Red => Red,
            GameEra.White => White,
            GameEra.Murasaki => Murasaki,
            GameEra.Kimidori => Kimidori,
            _ => null
        };

        return profile is not null;
    }

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
        MaxFavoriteSongs: 10,
        MaxRecentSongs: 5,
        MaxSongsPerTaikojukuPack: 10,
        MaxRequestedTaikojukuSlots: 11,
        MinCourseLevel: 1,
        MaxCourseLevel: 5,
        MinNormalDanId: 1,
        MaxNormalDanId: 25,
        MinExtraDanId: 101,
        MaxKnownExtraDanId: 128,
        SafeDisplayDanFallback: 1);

    private static Ac15ProtocolLimits CreateWhiteLimits() => CreateCommonLimits();

    private static Ac15ProtocolLimits CreateMurasakiLimits() => CreateCommonLimits();

    private static Ac15ProtocolLimits CreateKimidoriLimits() => CreateCommonLimits();
}
