# Phase 40: MOMOIRO Protocol Limits, Root Catalog, and Route Behavior - Pattern Map

**Mapped:** 2026-06-26
**Files analyzed:** 28
**Analogs found:** 26 / 28

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `Application/Abstractions/IMomoiroCatalog.cs` | model/interface | catalog read | `Application/Abstractions/IKimidoriCatalog.cs` | exact structure, reduce surface |
| `Application/Common/CatalogExtensions.cs` | utility | catalog read | `Application/Common/CatalogExtensions.cs` Kimidori extension | exact extension |
| `Infrastructure/GameDataCatalog/Momoiro/MomoiroGameDataPaths.cs` | config/utility | file-I/O | `Infrastructure/GameDataCatalog/Kimidori/KimidoriGameDataPaths.cs` | exact root-layout |
| `Infrastructure/GameDataCatalog/Momoiro/MomoiroRequiredDataFiles.cs` | utility | file-I/O validation | `Infrastructure/GameDataCatalog/Kimidori/KimidoriRequiredDataFiles.cs` | exact root-layout |
| `Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs` | service | file-I/O catalog load | `Infrastructure/GameDataCatalog/Kimidori/KimidoriEraGameDataCatalog.cs` | exact root-layout, reduce optional features |
| `Infrastructure/DependencyInjection.cs` | config | dependency injection | Kimidori catalog registration in same file | exact extension |
| `Application/Ac15/Ac15EraProfiles.cs` | config/model | transform/profile lookup | Kimidori profile in same file | role-match, values require Momoiro evidence |
| `Application/Ac15/Ac15ProtocolLimits.cs` | model | byte-limit config | existing record | role-match, extend only if evidence needs new fields |
| `Application/Ac15/Ac15FeatureSet.cs` | model | feature gating | existing record | role-match, extend only if current flags cannot express Momoiro absence |
| `Application/Ac15/Ac15CatalogSnapshotFactory.cs` | utility | catalog transform | `FromKimidori` | exact, reduce unsupported rows |
| `Application/Handlers/GetInitialDataQuery.cs` | handler dispatcher | request-response | Kimidori dispatch | role-match for split default-song/telopcheck metadata |
| `Application/Handlers/GetInitialDataQuery.Momoiro.cs` | handler | request-response | `GetInitialDataQuery.Kimidori.cs` | exact for default-song/telopcheck backing |
| `Application/Handlers/GetRecommendQuery.cs` | handler dispatcher | request-response | Kimidori dispatch | exact extension |
| `Application/Handlers/GetRecommendQuery.Momoiro.cs` | handler | request-response | `GetRecommendQuery.Kimidori.cs` | exact |
| `Application/Handlers/GetTelopQuery.cs` | handler dispatcher | request-response | Kimidori dispatch | exact extension |
| `Application/Handlers/GetTelopQuery.Momoiro.cs` | handler | request-response | `GetTelopQuery.Kimidori.cs` | exact |
| `Adapters.GameProtocol.Momoiro/GlobalUsings.cs` | config/imports | build/imports | `Adapters.GameProtocol.Kimidori/GlobalUsings.cs` | role-match |
| `Adapters.GameProtocol.Momoiro/Mappers/RecommendMappers.cs` | mapper | transform | `Adapters.GameProtocol.Kimidori/Mappers/RecommendMappers.cs` | exact |
| `Adapters.GameProtocol.Momoiro/Mappers/GetTelopMappers.cs` | mapper | transform | `Adapters.GameProtocol.Kimidori/Mappers/GetTelopMappers.cs` | exact shape, Momoiro wire names differ |
| `Adapters.GameProtocol.Momoiro/Controllers/RecommendController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/RecommendController.cs` | exact replacement |
| `Adapters.GameProtocol.Momoiro/Controllers/DefaultSongController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/DefaultSongController.cs` | exact root/songhash behavior |
| `Adapters.GameProtocol.Momoiro/Controllers/SongHashController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/SongHashController.cs` | exact |
| `Adapters.GameProtocol.Momoiro/Controllers/TelopCheckController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/TelopCheckController.cs` | exact |
| `Adapters.GameProtocol.Momoiro/Controllers/GetTelopController.cs` | controller | request-response | `Adapters.GameProtocol.Kimidori/Controllers/GetTelopController.cs` | exact |
| `Adapters.GameProtocol.Momoiro/Controllers/HeartbeatController.cs` | controller | static request-response | existing Momoiro scaffold / Kimidori heartbeat | exact static stub |
| `Adapters.GameProtocol.Momoiro/Controllers/BookkeepingController.cs` | controller | static request-response | existing Momoiro scaffold / Kimidori bookkeeping | exact static stub |
| `Tests/Momoiro/MomoiroCatalogLoaderTests.cs` | test | file-I/O catalog load | `Tests/Kimidori/KimidoriCatalogLoaderTests.cs` | exact root-layout |
| `Tests/Momoiro/MomoiroMetadataRouteTests.cs` | test | request-response transform | `Tests/Yellow/YellowMetadataRouteTests.cs`, `Tests/Green/GreenTelopTests.cs` | role-match |
| `Tests/Ac15/Ac15RecommendQueryHandlerTests.cs` | test | request-response handler | existing multi-era matrix | exact extension |
| `Tests/Ac15/Ac15CatalogSnapshotFactoryCompositionTests.cs` | test | transform | existing snapshot factory test | exact extension |
| `Tests/Ac15/Ac15ProfileCapabilitiesTests.cs` or `Tests/Momoiro/MomoiroProtocolLimitsTests.cs` | test | profile config | existing AC15 profile capability tests | role-match |
| `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` | test | route discovery | existing Momoiro route-surface test | exact guard |

## Pattern Assignments

### `Application/Abstractions/IMomoiroCatalog.cs` (model/interface, catalog read)

**Analog:** `Application/Abstractions/IKimidoriCatalog.cs`

**Catalog interface shape** (lines 5-25):
```csharp
public interface IKimidoriCatalog : IEraGameDataCatalog
{
    IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder { get; }

    uint SongHashVersion { get; }

    IReadOnlyList<ushort> SongHashTable { get; }

    IReadOnlyDictionary<uint, Ac15MusicInfoEntry> KimidoriMusicInfos { get; }

    IReadOnlyList<Ac15TaikojukuEntry> DaniFileOrder { get; }

    IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

    IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; }

    IReadOnlyList<MovieData> Movies { get; }
```

**Apply to Momoiro:** create `IMomoiroCatalog : IEraGameDataCatalog` with the minimum Phase 40 surface: `MusicInfoFileOrder`, `SongHashVersion`, `SongHashTable`, `MomoiroMusicInfos`, and `Telops`. Only add Dani/reward/customization/event-folder/movie/present/special-BAID properties if Momoiro-specific proto plus route/data evidence proves Phase 40 needs them.

### `Infrastructure/GameDataCatalog/Momoiro/MomoiroGameDataPaths.cs` (config/utility, file-I/O)

**Analog:** `Infrastructure/GameDataCatalog/Kimidori/KimidoriGameDataPaths.cs`

**Root-level data path pattern** (lines 5-22):
```csharp
public static class KimidoriGameDataPaths
{
    public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Kimidori), "data");

    public static string MusicInfoXml => Path.Combine(GameDataRoot, "musicinfo.xml");

    public static string MusicMedleyInfoXml => Path.Combine(GameDataRoot, "musicmedleyinfo.xml");

    public static string DefMusicBin => Path.Combine(GameDataRoot, "defmusic.bin");

    public static string PresentXml => Path.Combine(GameDataRoot, "present.xml");

    public static string SpecialBaidXml => Path.Combine(GameDataRoot, "spacialbaid.xml");

    public static string TuningBin => Path.Combine(GameDataRoot, "fumen", "tuning.bin");
```

**Apply to Momoiro:** use `PathHelper.GetDataPath(GameEra.Momoiro)` plus `"data"` and root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`. Do not use `config/STxxxx-*` paths.

### `Infrastructure/GameDataCatalog/Momoiro/MomoiroRequiredDataFiles.cs` (utility, file-I/O validation)

**Analog:** `Infrastructure/GameDataCatalog/Kimidori/KimidoriRequiredDataFiles.cs`

**Required file guard** (lines 3-29):
```csharp
public static class KimidoriRequiredDataFiles
{
    public static IReadOnlyList<string> GetRequiredPaths()
    {
        return
        [
            KimidoriGameDataPaths.MusicInfoXml,
            KimidoriGameDataPaths.MusicMedleyInfoXml,
            KimidoriGameDataPaths.DefMusicBin,
            KimidoriGameDataPaths.TuningBin
        ];
    }

    public static void ThrowIfMissing()
        => ThrowIfMissing(GetRequiredPaths());
```

**Apply to Momoiro:** same four required paths, with Momoiro-specific exception text. Do not require optional sidecar JSON or later-era folders for Phase 40 startup.

### `Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs` (service, file-I/O catalog load)

**Analog:** `Infrastructure/GameDataCatalog/Kimidori/KimidoriEraGameDataCatalog.cs`

**Catalog state and interface properties** (lines 24-55):
```csharp
private uint songHashVersion;
private IReadOnlyList<ushort> songHashTable = [];
private IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder = [];
private IReadOnlyDictionary<uint, Ac15MusicInfoEntry> musicInfos = new Dictionary<uint, Ac15MusicInfoEntry>();
private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();

public GameEra Era => GameEra.Kimidori;

public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

public uint SongHashVersion => songHashVersion;

public IReadOnlyList<ushort> SongHashTable => songHashTable;

public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;
```

**Root-load core** (lines 69-88, 117-124):
```csharp
public async Task InitializeAsync(CancellationToken cancellationToken)
{
    KimidoriRequiredDataFiles.ThrowIfMissing();

    var musicInfo = await Ac15MusicInfoLoader.LoadFromFileAsync(KimidoriGameDataPaths.MusicInfoXml, cancellationToken);
    var stars = await Ac15TuningLoader.LoadFromFileAsync(
        KimidoriGameDataPaths.TuningBin,
        nameof(GameEra.Kimidori),
        cancellationToken);
    var loadedDaniFileOrder = await Ac15TaikojukuLoader.LoadFromFileAsync(
        KimidoriGameDataPaths.MusicMedleyInfoXml,
        cancellationToken);

    songHashVersion = musicInfo.SongHashVersion;
    songHashTable = Ac15SongHashCodec.BuildTable(enrichedEntries.Select(entry => entry.SongNo));
    musicInfoFileOrder = enrichedEntries;
    musicInfos = enrichedEntries.ToDictionary(entry => entry.SongNo);
    sharedMusicInfos = musicInfos.ToDictionary(
        pair => pair.Key,
        pair => (IMusicInfoEntry)pair.Value);
```

**Optional sidecar loading pattern** (lines 131-145):
```csharp
telops = await Ac15TelopLoader.LoadFromFileAsync(
    Path.Combine(PathHelper.GetDataPath(GameEra.Kimidori), TelopFileName),
    cancellationToken);
presents = File.Exists(KimidoriGameDataPaths.PresentXml)
    ? await Ac15PresentLoader.LoadFromFileAsync(KimidoriGameDataPaths.PresentXml, cancellationToken)
    : [];
specialBaids = File.Exists(KimidoriGameDataPaths.SpecialBaidXml)
    ? await Ac15SpecialBaidLoader.LoadFromFileAsync(KimidoriGameDataPaths.SpecialBaidXml, cancellationToken)
    : [];
```

**Apply to Momoiro:** copy the root-load core and song-hash table construction. Load telops from a Momoiro sidecar only if the server-authored `momoiro_telop_data.json` exists or is deliberately added as an empty/real committed artifact. Do not load KIMIDORI event folders, movies, presents, special BAID, Taikojuku, or customization unless Phase 40 evidence says Momoiro has that route/data role.

### `Infrastructure/DependencyInjection.cs` (config, dependency injection)

**Analog:** Kimidori catalog registration in `Infrastructure/DependencyInjection.cs`

**Enabled-era registration pattern** (lines 117-125):
```csharp
if (enabledEras.Contains(GameEra.Kimidori))
{
    services.AddSingleton<KimidoriEraGameDataCatalog>();
    services.AddSingleton<IKimidoriCatalog>(sp => sp.GetRequiredService<KimidoriEraGameDataCatalog>());
    services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<KimidoriEraGameDataCatalog>());
}

services.AddSingleton<IGameDataCatalog>(sp => new FileGameDataCatalog(
    sp.GetServices<IEraGameDataCatalog>()));
```

**Apply to Momoiro:** add the same three singleton registrations for `MomoiroEraGameDataCatalog`, `IMomoiroCatalog`, and `IEraGameDataCatalog` only when `GameEra.Momoiro` is enabled.

### `Application/Common/CatalogExtensions.cs` (utility, catalog read)

**Analog:** `Application/Common/CatalogExtensions.cs`

**Era-specific catalog accessor** (lines 26-30):
```csharp
public static IMurasakiCatalog Murasaki(this IGameDataCatalog catalog)
    => (IMurasakiCatalog)catalog.For(GameEra.Murasaki);

public static IKimidoriCatalog Kimidori(this IGameDataCatalog catalog)
    => (IKimidoriCatalog)catalog.For(GameEra.Kimidori);
```

**Apply to Momoiro:** add `Momoiro(this IGameDataCatalog catalog)` that casts `catalog.For(GameEra.Momoiro)` to `IMomoiroCatalog`.

### `Application/Ac15/Ac15EraProfiles.cs` and `Application/Ac15/Ac15ProtocolLimits.cs` (config/model, profile limits)

**Analog:** Kimidori profile and shared limit shape.

**Feature inheritance pattern** from `Ac15EraProfiles.cs` (lines 20-33):
```csharp
private static readonly Ac15FeatureSet RedFeatures = BlueGreenFeatures with
{
    ItemShop = false
};

private static readonly Ac15FeatureSet MurasakiFeatures = RedFeatures;

private static readonly Ac15FeatureSet KimidoriFeatures = MurasakiFeatures with
{
    InitialData = false,
    Taikojuku = false
};
```

**Profile registration pattern** from `Ac15EraProfiles.cs` (lines 101-110):
```csharp
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
```

**Lookup extension pattern** from `Ac15EraProfiles.cs` (lines 115-127):
```csharp
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
```

**Limit record shape** from `Ac15ProtocolLimits.cs` (lines 3-23):
```csharp
public sealed record Ac15ProtocolLimits(
    int SongFlagBytes,
    int ToneFlagBytes,
    int TitleFlagBytes,
    int CostumeFlagBytes,
    int DanFlagBytes,
    int DanExtraFlagBytes,
    int ContentInfoBytes,
    int CrownPackedBytes,
    int CrownSongCount,
    int MaxFavoriteSongs,
    int MaxRecentSongs,
    int MaxSongsPerTaikojukuPack,
    int MaxRequestedTaikojukuSlots,
```

**Crown placement shape** from `Ac15WirePlacement.cs` (lines 3-14):
```csharp
public enum Ac15CrownWirePlacement
{
    Absent = 0,
    DedicatedEndpoint = 1,
    UserData = 2
}

public sealed record Ac15WirePlacement(
    Ac15CrownWirePlacement CrownPlacement,
    bool HasInitialDataItemShopRows,
    bool HasInitialDataLegalTermsRows,
    bool HasTokkunTutorialFlagInUserData);
```

**Apply to Momoiro:** add `MomoiroFeatures`, `Momoiro`, `CreateMomoiroLimits()`, and a `TryGet` switch arm. Set absent features explicitly. Phase 40 context says no standalone `crownsdata.php`; use `CrownPlacement: Ac15CrownWirePlacement.UserData` if crown readback is modeled now, or leave crown packing deferred but do not add a dedicated endpoint. Do not copy KIMIDORI numeric byte counts blindly; if evidence is missing for song/crown/release widths, record the gap in the plan and keep implementation behind tests.

**Caution:** `Ac15ProtocolLimits` currently has no Don Point cap field. If MOMOIRO's 30000 point cap is implemented as a behavior guard, add an explicit field and tests instead of using a handler-local magic number. Mutation still belongs to Phase 42.

### `Application/Ac15/Ac15CatalogSnapshotFactory.cs` (utility, catalog transform)

**Analog:** `FromKimidori`

**Snapshot projection** (lines 71-78):
```csharp
public static Ac15CatalogSnapshot FromKimidori(IKimidoriCatalog kimidori)
    => FromSource(new Ac15CatalogProjectionSource(
        kimidori.SongHashVersion,
        kimidori.MusicInfoFileOrder.Select(song => song.SongNo).ToArray(),
        kimidori.EventFolders,
        kimidori.Telops,
        Ac15ItemShopCatalog.Disabled,
        []));
```

**Apply to Momoiro:** add `FromMomoiro(IMomoiroCatalog momoiro)` with Momoiro song order, telops, disabled item shop, and empty Taikojuku packs unless Momoiro evidence proves those surfaces. If `IMomoiroCatalog` omits event folders, pass an empty dictionary.

### `Application/Handlers/GetInitialDataQuery*.cs` (handler, request-response metadata)

**Analog:** Kimidori dispatch plus partial handler.

**Dispatcher pattern** from `GetInitialDataQuery.cs` (lines 17-27):
```csharp
public ValueTask<CommonInitialDataCheckResponse> Handle(GetInitialDataQuery request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    GameEra.Yellow => HandleYellow(request, cancellationToken),
    GameEra.Red => HandleRed(request, cancellationToken),
    GameEra.White => HandleWhite(request, cancellationToken),
    GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
    GameEra.Kimidori => HandleKimidori(request, cancellationToken),
```

**Kimidori metadata backing handler** from `GetInitialDataQuery.Kimidori.cs` (lines 7-13):
```csharp
private partial ValueTask<CommonInitialDataCheckResponse> HandleKimidori(
    GetInitialDataQuery request,
    CancellationToken cancellationToken)
{
    var snapshot = Ac15CatalogSnapshotFactory.FromKimidori(gameDataService.Kimidori());
    var response = Ac15InitialDataService.BuildCommonInitialData(snapshot, Ac15EraProfiles.Kimidori);
    return ValueTask.FromResult(response);
}
```

**Apply to Momoiro:** add `HandleMomoiro` and use it to back `defaultsong.php` and `telopcheck.php`. This does not imply a Momoiro `initialdatacheck.php` route; Phase 39 route inventory excludes it.

### `Application/Handlers/GetRecommendQuery*.cs` (handler, request-response)

**Analog:** Kimidori recommend handler.

**Dispatch and partial declaration** from `GetRecommendQuery.cs` (lines 10-18, 22-28):
```csharp
public ValueTask<CommonRecommendResponse> Handle(GetRecommendQuery request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Blue => HandleBlue(request, cancellationToken),
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Yellow => HandleYellow(request, cancellationToken),
    GameEra.Red => HandleRed(request, cancellationToken),
    GameEra.White => HandleWhite(request, cancellationToken),
    GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
    GameEra.Kimidori => HandleKimidori(request, cancellationToken),
```

**Catalog-backed implementation** from `GetRecommendQuery.Kimidori.cs` (lines 7-13):
```csharp
private partial ValueTask<CommonRecommendResponse> HandleKimidori(
    GetRecommendQuery request,
    CancellationToken cancellationToken)
{
    logger.LogDebug("Kimidori recommend requested for gender {GenderType}, age {PlayerAge}", request.GenderType, request.PlayerAge);
    var snapshot = Ac15CatalogSnapshotFactory.FromKimidori(gameDataService.Kimidori());
    return ValueTask.FromResult(Ac15CatalogReadbackService.BuildRecommendResponse(snapshot));
}
```

**Apply to Momoiro:** add a Momoiro switch arm and `HandleMomoiro` using `Ac15CatalogSnapshotFactory.FromMomoiro(gameDataService.Momoiro())`.

### `Application/Handlers/GetTelopQuery*.cs` (handler, request-response)

**Analog:** Kimidori telop handler.

**Dispatch pattern** from `GetTelopQuery.cs` (lines 8-17):
```csharp
public ValueTask<CommonGetTelopResponse> Handle(GetTelopQuery request, CancellationToken cancellationToken) => request.Era switch
{
    GameEra.Green => HandleGreen(request, cancellationToken),
    GameEra.Blue => HandleBlue(request, cancellationToken),
    GameEra.Yellow => HandleYellow(request, cancellationToken),
    GameEra.Red => HandleRed(request, cancellationToken),
    GameEra.White => HandleWhite(request, cancellationToken),
    GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
    GameEra.Kimidori => HandleKimidori(request, cancellationToken),
    _ => throw new InvalidOperationException($"GetTelopQuery is not implemented for era: {request.Era}")
```

**Catalog lookup implementation** from `GetTelopQuery.Kimidori.cs` (lines 7-12):
```csharp
private partial ValueTask<CommonGetTelopResponse> HandleKimidori(
    GetTelopQuery request,
    CancellationToken cancellationToken)
{
    var snapshot = Ac15CatalogSnapshotFactory.FromKimidori(gameDataService.Kimidori());
    return ValueTask.FromResult(Ac15CatalogReadbackService.BuildTelopResponse(snapshot, request.TelopId));
}
```

**Apply to Momoiro:** add a Momoiro switch arm and partial method. Missing telop ID should return success with omitted optional fields via `Ac15CatalogReadbackService`.

### `Adapters.GameProtocol.Momoiro/GlobalUsings.cs` and mappers (mapper/imports, transform)

**Analog:** `Adapters.GameProtocol.Kimidori/GlobalUsings.cs` and Kimidori mappers.

**Mapper global using pattern** from `Kimidori/GlobalUsings.cs` (lines 5-14):
```csharp
global using TaikoLocalServer.Adapters.GameProtocol.Kimidori;
global using TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;
global using TaikoLocalServer.Adapters.GameProtocol.Kimidori.Wire;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Application.Ac15;
global using TaikoLocalServer.Application.Common;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.Dtos.Ac15;
global using TaikoLocalServer.Application.Handlers;
```

**Current Momoiro global usings** (lines 5-12):
```csharp
global using TaikoLocalServer.Adapters.GameProtocol.Momoiro;
global using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
global using TaikoLocalServer.Application.Abstractions;
global using TaikoLocalServer.Application.Ac15;
global using TaikoLocalServer.Application.Common;
global using TaikoLocalServer.Application.Dtos;
global using TaikoLocalServer.Application.Dtos.Ac15;
global using TaikoLocalServer.Application.Handlers;
```

**Apply to Momoiro:** add `global using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Mappers;` if new mapper classes are used from controllers. Keep `Wire` imported explicitly or add a global using consistently.

**Recommend mapper pattern** from `Kimidori/Mappers/RecommendMappers.cs` (lines 5-9):
```csharp
[Mapper]
public static partial class RecommendMappers
{
    [MapProperty(nameof(CommonRecommendResponse.RecommendBestSong), nameof(RecommendResponse.RecommendBestSongs))]
    public static partial RecommendResponse Map(CommonRecommendResponse common);
}
```

**GetTelop mapper pattern** from `Kimidori/Mappers/GetTelopMappers.cs` (lines 5-14):
```csharp
[Mapper]
public static partial class GetTelopMappers
{
    [MapperIgnoreSource(nameof(CommonGetTelopResponse.VerupNo))]
    [MapProperty(nameof(CommonGetTelopResponse.StartDatetime), nameof(GettelopResponse.StartDatetime), Use = nameof(MapPresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.EndDatetime), nameof(GettelopResponse.EndDatetime), Use = nameof(MapPresentString))]
    [MapProperty(nameof(CommonGetTelopResponse.Telop), nameof(GettelopResponse.Telop), Use = nameof(MapPresentString))]
    public static partial GettelopResponse Map(CommonGetTelopResponse common);

    private static string MapPresentString(string? value) => string.IsNullOrEmpty(value) ? null! : value;
}
```

**Apply to Momoiro:** copy mapper shape but use Momoiro wire class names. Momoiro generated classes are `GetTelopRequest` / `GetTelopResponse` and `TelopCheckRequest` / `TelopCheckResponse`, not Kimidori's `Gettelop*` / `Telopcheck*` casing.

### `Adapters.GameProtocol.Momoiro/Controllers/RecommendController.cs` (controller, request-response)

**Analog:** `Adapters.GameProtocol.Kimidori/Controllers/RecommendController.cs`

**Controller body pattern** (lines 6-14):
```csharp
[HttpPost(KimidoriRoutePrefixes.Game + "/recommend.php")]
[Produces("application/protobuf")]
public async Task<IActionResult> Recommend([FromBody] RecommendRequest request)
{
    Logger.LogInformation("Kimidori Recommend request: {@Request}", request);
    var common = await Mediator.Send(
        new GetRecommendQuery(GameEra.Kimidori, request.GenderType, request.PlayerAge),
        HttpContext.RequestAborted);
    return Ok(RecommendMappers.Map(common));
}
```

**Current Momoiro scaffold to replace** (lines 8-18):
```csharp
[HttpPost(MomoiroRoutePrefixes.Game + "/recommend.php")]
[Produces("application/protobuf")]
public IActionResult Recommend([FromBody] RecommendRequest request)
{
    Logger.LogInformation(
        "Momoiro recommend.php scaffold request: ChassisId={ChassisId}, GenderType={GenderType}, PlayerAge={PlayerAge}",
        request.ChassisId,
        request.GenderType,
        request.PlayerAge);

    return Ok(new RecommendResponse { Result = 1 });
}
```

**Apply to Momoiro:** keep route/signature, make it async, send `GetRecommendQuery(GameEra.Momoiro, request.GenderType, request.PlayerAge)`, return `RecommendMappers.Map(common)`.

### `Adapters.GameProtocol.Momoiro/Controllers/DefaultSongController.cs` (controller, request-response)

**Analog:** `Adapters.GameProtocol.Kimidori/Controllers/DefaultSongController.cs`

**Catalog-backed default-song pattern** (lines 4-21):
```csharp
public sealed class DefaultSongController(IGameDataCatalog gameDataService)
    : BaseProtocolController<DefaultSongController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/defaultsong.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> DefaultSong([FromBody] DefaultsongRequest request)
    {
        Logger.LogInformation("Kimidori DefaultSong request: {@Request}", request);
        var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Kimidori), HttpContext.RequestAborted);
        var kimidori = gameDataService.Kimidori();
        return Ok(new DefaultsongResponse
        {
            Result = common.Result,
            SongHashVer = common.SongHashVer,
            HashDefaultSongFlg = Ac15SongHashCodec.CompactBitset(
                common.DefaultSongFlg,
                kimidori.SongHashTable)
        });
    }
}
```

**Apply to Momoiro:** inject `IGameDataCatalog`, send `GetInitialDataQuery(GameEra.Momoiro)`, and compact `common.DefaultSongFlg` by `gameDataService.Momoiro().SongHashTable`. This is the key KIMIDORI root-era behavior; do not copy Murasaki's un-compacted default-song body unless binary evidence proves Momoiro expects inflated flags.

### `Adapters.GameProtocol.Momoiro/Controllers/SongHashController.cs` (controller, request-response)

**Analog:** `Adapters.GameProtocol.Kimidori/Controllers/SongHashController.cs`

**Song hash response pattern** (lines 4-18):
```csharp
public sealed class SongHashController(IGameDataCatalog gameDataService)
    : BaseProtocolController<SongHashController>
{
    [HttpPost(KimidoriRoutePrefixes.Game + "/songhash.php")]
    [Produces("application/protobuf")]
    public IActionResult SongHash([FromBody] SonghashRequest request)
    {
        Logger.LogInformation("Kimidori SongHash request: {@Request}", request);
        var kimidori = gameDataService.Kimidori();
        return Ok(new SonghashResponse
        {
            Result = 1,
            SongHashVer = kimidori.SongHashVersion,
            SongHashTbl = Ac15SongHashCodec.EncodeTable(kimidori.SongHashTable)
        });
    }
}
```

**Apply to Momoiro:** same structure with `gameDataService.Momoiro()`. The route is Phase 39-proven, so KIMIDORI is the correct analog. Murasaki has no live `SongHashController`; do not treat its absence as a reason to omit Momoiro `songhash.php`.

### `Adapters.GameProtocol.Momoiro/Controllers/TelopCheckController.cs` (controller, request-response)

**Analog:** `Adapters.GameProtocol.Kimidori/Controllers/TelopCheckController.cs`

**Telop ID advertisement pattern** (lines 6-16):
```csharp
[HttpPost(KimidoriRoutePrefixes.Game + "/telopcheck.php")]
[Produces("application/protobuf")]
public async Task<IActionResult> TelopCheck([FromBody] TelopcheckRequest request)
{
    Logger.LogInformation("Kimidori TelopCheck request: {@Request}", request);
    var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Kimidori), HttpContext.RequestAborted);
    return Ok(new TelopcheckResponse
    {
        Result = common.Result,
        TelopIds = common.AryTelopDatas.Select(row => row.InfoId).ToArray()
    });
}
```

**Apply to Momoiro:** use `TelopCheckRequest` / `TelopCheckResponse` and `GameEra.Momoiro`. Keep response limited to catalog telop IDs.

### `Adapters.GameProtocol.Momoiro/Controllers/GetTelopController.cs` (controller, request-response)

**Analog:** `Adapters.GameProtocol.Kimidori/Controllers/GetTelopController.cs`

**Get telop mediator pattern** (lines 6-14):
```csharp
[HttpPost(KimidoriRoutePrefixes.Game + "/gettelop.php")]
[Produces("application/protobuf")]
public async Task<IActionResult> GetTelop([FromBody] GettelopRequest request)
{
    Logger.LogInformation("Kimidori GetTelop request: {@Request}", request);
    var common = await Mediator.Send(
        new GetTelopQuery(GameEra.Kimidori, request.TelopId),
        HttpContext.RequestAborted);
    return Ok(GetTelopMappers.Map(common));
}
```

**Apply to Momoiro:** use `GetTelopRequest`, `GetTelopQuery(GameEra.Momoiro, request.TelopId)`, and the Momoiro `GetTelopMappers.Map`.

### `Adapters.GameProtocol.Momoiro/Controllers/HeartbeatController.cs` and `BookkeepingController.cs` (controller, static request-response)

**Analog:** Kimidori static stubs and existing Momoiro scaffolds.

**Heartbeat static success** from Kimidori (lines 6-16):
```csharp
[HttpPost(KimidoriRoutePrefixes.Game + "/heartbeat.php")]
[Produces("application/protobuf")]
public IActionResult Heartbeat([FromBody] HeartBeatRequest request)
{
    Logger.LogInformation("Kimidori heartbeat.php request: {@Request}", request);
    return Ok(new HeartBeatResponse
    {
        Result = 1,
        ComSvrStat = 1,
        GameSvrStat = 1
    });
}
```

**Bookkeeping static success** from current Momoiro scaffold (lines 8-17):
```csharp
[HttpPost(MomoiroRoutePrefixes.Game + "/bookkeeping.php")]
[Produces("application/protobuf")]
public IActionResult Bookkeeping([FromBody] BookKeepingRequest request)
{
    Logger.LogInformation(
        "Momoiro bookkeeping.php scaffold request: ChassisId={ChassisId}, ShopId={ShopId}",
        request.ChassisId,
        request.ShopId);

    return Ok(new BookKeepingResponse { Result = 1 });
}
```

**Apply to Momoiro:** leave static success unless new binary/client evidence proves stateful heartbeat or accounting behavior. It is reasonable to remove the word `scaffold` from logs when Phase 40 records these as intentional operational stubs, but do not add persistence.

## Shared Patterns

### Root-Level Catalog Loading

**Sources:** `KimidoriGameDataPaths.cs`, `KimidoriRequiredDataFiles.cs`, `KimidoriEraGameDataCatalog.cs`

**Apply to:** Momoiro catalog path, required data guard, catalog initialization.

Use root-level `Host/wwwroot/data/momoiro/data` paths through `PathHelper.GetDataPath(GameEra.Momoiro)`. Required files are exactly `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin` for Phase 40.

### Catalog Snapshot Readback

**Sources:** `Ac15CatalogSnapshotFactory.cs`, `Ac15InitialDataService.cs`, `Ac15CatalogReadbackService.cs`

**Apply to:** `recommend.php`, `defaultsong.php`, `telopcheck.php`, `gettelop.php`.

**Default flags and telop rows** from `Ac15InitialDataService.cs` (lines 21-37, 55-63):
```csharp
return new CommonInitialDataCheckResponse
{
    Result = 1,
    DefaultSongFlg = Ac15ProtocolBytes.CreateFixedBitset(defaultSongNoes, profile.Limits.SongFlagBytes),
    AchievementSongBit = new byte[profile.Limits.SongFlagBytes],
    UraReleaseBit = new byte[profile.Limits.SongFlagBytes],
    SongHashVer = snapshot.SongHashVersion,
    IsDanplay = profile.Features.Dani,
    IsClose = false,
    IsItemshop = profile.Features.ItemShop && activeShopWithRows is not null,
    AryTelopDatas = BuildTelopInfoRows(snapshot),
};

public static List<CommonInitialDataCheckResponse.InformationData> BuildTelopInfoRows(Ac15CatalogSnapshot snapshot)
    => snapshot.Telops.Values
        .OrderBy(entry => entry.TelopId)
```

### Recommendation Selection

**Source:** `Application/Ac15/Ac15RecommendationService.cs`

**Apply to:** Momoiro `recommend.php` and later userdata recommendations.

**Reserved medley filtering** (lines 8-34):
```csharp
private const uint FirstReservedMedleySongNo = 10_000;
private const uint LastReservedMedleySongNo = 49_999;

public static CommonRecommendResponse BuildRecommendResponse(IReadOnlyList<uint> songNoes)
    => new()
    {
        Result = 1,
        RecommendSong = PickRecommendSong(songNoes)
    };

private static bool IsRecommendSeedCandidate(uint songNo)
    => songNo > 0
        && (songNo < FirstReservedMedleySongNo || songNo > LastReservedMedleySongNo);
```

### Song Hash Encoding

**Source:** `Application/Ac15/Ac15SongHashCodec.cs`

**Apply to:** Momoiro `songhash.php`, compacted default/release/crown flags.

**Encoding and compaction** (lines 5-25):
```csharp
public static ushort[] BuildTable(IEnumerable<uint> songNoes)
    => songNoes.Select(ToHashIndex).ToArray();

public static byte[] EncodeTable(IReadOnlyList<ushort> table)
{
    var result = new byte[checked(table.Count * 2)];
    for (var i = 0; i < table.Count; i++)
    {
        var value = table[i];
        result[i * 2] = (byte)(value >> 8);
        result[i * 2 + 1] = (byte)value;
    }

    return result;
}

public static byte[] CompactBitset(byte[] inflated, IReadOnlyList<ushort> table)
    => CompactValues(inflated, table, bitsPerValue: 1);
```

**Caution:** `ToHashIndex` error text says "KIMIDORI" (lines 49-57). If this becomes visible in Momoiro tests, rename the message to shared AC15 wording in a separate small edit.

### Testing Patterns

**Catalog loader analog:** `Tests/Kimidori/KimidoriCatalogLoaderTests.cs`

**Root data copy pattern** (lines 79-106):
```csharp
private static void CopyKimidoriCatalogFilesToProcessRoot()
{
    var repoRoot = FindRepoRoot();
    var targetRoot = Path.Combine(
        Path.GetDirectoryName(Environment.ProcessPath)
            ?? throw new ApplicationException("Cannot resolve process directory."),
        "wwwroot",
        "data",
        "kimidori");

    Copy(
        Path.Combine(repoRoot, "Host", "wwwroot", "data", "kimidori", "data", "musicinfo.xml"),
        Path.Combine(targetRoot, "data", "musicinfo.xml"));
```

**Song hash catalog assertion** from Kimidori tests (lines 27-40):
```csharp
[Fact]
public async Task CatalogInitialize_BuildsSongHashTableFromMusicInfoFileOrder()
{
    CopyKimidoriCatalogFilesToProcessRoot();
    var logger = new RecordingLogger<KimidoriEraGameDataCatalog>();
    var catalog = new KimidoriEraGameDataCatalog(logger);

    await catalog.InitializeAsync(CancellationToken.None);

    Assert.Equal(427, catalog.SongHashTable.Count);
    Assert.Equal([236, 234, 128, 199, 5], catalog.SongHashTable.Take(5).Select(value => (int)value).ToArray());
    Assert.Equal(20015, catalog.SongHashTable[^1]);
    Assert.Equal([0x00, 0xec, 0x00, 0xea], Ac15SongHashCodec.EncodeTable(catalog.SongHashTable)[..4]);
}
```

**Readback service tests** from `Tests/Ac15/Ac15CatalogReadbackServiceTests.cs` (lines 20-28, 40-58):
```csharp
[Fact]
public void BuildTelopResponse_ReturnsEmptySuccessWhenMissing()
{
    var response = Ac15CatalogReadbackService.BuildTelopResponse(Snapshot(), 99);

    Assert.Equal(1u, response.Result);
    Assert.Null(response.VerupNo);
    Assert.Null(response.Telop);
}

[Fact]
public void BuildRecommendResponse_DoesNotUseReservedMedleyRowsAsSeed()
{
    var response = Ac15CatalogReadbackService.BuildRecommendResponse(Snapshot(songNoes: [20001]));

    Assert.Equal(1u, response.Result);
    Assert.Equal(0u, response.RecommendSong);
    Assert.Empty(response.RecommendBestSong);
}
```

**Song hash codec tests** from `Tests/Ac15/Ac15SongHashCodecTests.cs` (lines 7-41):
```csharp
[Fact]
public void EncodeTable_UsesBigEndianSongIndexes()
{
    var table = Ac15SongHashCodec.BuildTable([236u, 5u]);

    var bytes = Ac15SongHashCodec.EncodeTable(table);

    Assert.Equal([0x00, 0xec, 0x00, 0x05], bytes);
}

[Fact]
public void CompactBitset_ReindexesSongNumberBitsByHashTableOrdinal()
{
    var inflated = Ac15ProtocolBytes.CreateFixedBitset([236u, 5u], byteCount: 128);
    var table = Ac15SongHashCodec.BuildTable([236u, 234u, 128u, 199u, 5u]);

    var compact = Ac15SongHashCodec.CompactBitset(inflated, table);

    Assert.Equal([0b0001_0001], compact);
}
```

**Multi-era recommendation handler matrix** from `Tests/Ac15/Ac15RecommendQueryHandlerTests.cs` (lines 13-41, 44-73):
```csharp
[Theory]
[InlineData(GameEra.Blue, 201u)]
[InlineData(GameEra.Green, 202u)]
[InlineData(GameEra.Yellow, 203u)]
[InlineData(GameEra.Red, 204u)]
[InlineData(GameEra.White, 205u)]
[InlineData(GameEra.Murasaki, 206u)]
[InlineData(GameEra.Kimidori, 207u)]
public async Task Handle_ReturnsRandomCatalogSongAndLeavesBestSongUnset(GameEra era, uint expectedSongNo)
```

**Telop behavior tests** from `Tests/Green/GreenTelopTests.cs` (lines 94-152):
```csharp
[Fact]
public async Task GetTelop_ReturnsCatalogEntryWithFourteenCharDatetimes()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync(new GreenHandlerFixture.TestGreenCatalog
    {
        Telops = new Dictionary<uint, Ac15TelopEntry>
        {
            [7] = new()
            {
                TelopId = 7,
                VerupNo = 4,
                StartDatetime = "20240101000000",
                EndDatetime = "20991231235959",
                Message = "Hello Green"
            }
        }
    });
```

**Momoiro route absence guard** from `Tests/Momoiro/MomoiroRouteSurfaceTests.cs` (lines 14-36, 69-83):
```csharp
private static readonly string[] ExpectedGameRoutes =
[
    "/v04r00/chassis/baidcheck.php",
    "/v04r00/chassis/bookkeeping.php",
    "/v04r00/chassis/defaultsong.php",
    "/v04r00/chassis/gettelop.php",
    "/v04r00/chassis/heartbeat.php",
    "/v04r00/chassis/mydonentry.php",
    "/v04r00/chassis/playresult.php",
    "/v04r00/chassis/recommend.php",
    "/v04r00/chassis/selfbest.php",
    "/v04r00/chassis/songhash.php",
    "/v04r00/chassis/telopcheck.php",
    "/v04r00/chassis/userdata.php"
];

private static readonly string[] ProtoOnlyRouteFragments =
[
    "shoppingresult.php",
    "bestscore.php",
    "communicationlog.php",
    "mainichisong.php"
];
```

## Caution And Avoid Patterns

| Pattern | Source | Why to avoid or constrain |
|---------|--------|---------------------------|
| KIMIDORI `BestScoreController`, `CommunicationLogController`, `MainichiSongController`, `ShoppingResultController` | `Adapters.GameProtocol.Kimidori/Controllers/*` | Phase 39 Momoiro route evidence explicitly excludes these proto-only route families. Keep them absent. |
| KIMIDORI `FolderCheckController` / `GetFolderController` and `EventFolders` behavior | KIMIDORI controllers and `IKimidoriCatalog.EventFolders` | Momoiro Phase 40 scope excludes event-folder routes. Do not expose folder readback without Momoiro route evidence. |
| KIMIDORI `CrownsDataController` | `Adapters.GameProtocol.Kimidori/Controllers/CrownsDataController.cs` | Momoiro crowns are scoped as userdata-owned `hash_crown_flg`; no standalone `crownsdata.php`. |
| Murasaki dual final/compatibility route attributes | `Adapters.GameProtocol.Murasaki/Controllers/DefaultSongController.cs` lines 6-7 | Momoiro has only `/v04r00/chassis` game routes plus shared `/v01r00` startup/version. Do not add a compatibility prefix. |
| Murasaki un-compacted default-song flags | `Adapters.GameProtocol.Murasaki/Controllers/DefaultSongController.cs` lines 13-18 | Momoiro has `songhash.php` and KIMIDORI-style song-hash fields. Use KIMIDORI compaction unless binary/client evidence proves inflated flags. |
| Don Challenge, ChallengeCompe, Tokkun, Banacoin, battle, gacha, tournament, item-shop authority | Red/Yellow/Blue implementations | Explicitly out of Phase 40 and mostly out of Momoiro 0.11 scope without Momoiro-specific proto plus route evidence. |

## No Analog Found

| File/Concern | Role | Data Flow | Reason |
|--------------|------|-----------|--------|
| Momoiro binary-backed byte counts and crown placement evidence | evidence/config | binary research | No source file can prove Momoiro-specific song flag byte count, crown byte count/song count, release flag shape, or Don Point cap. Use `.tools/momoiro/EBOOT.ELF.i64`, generated Momoiro wire, and cabinet/client evidence. |
| `Ac15ProtocolLimits` Don Point cap if needed | model/config | mutation guard | Existing limit record has byte widths and list limits, but no Don Point maximum. Add only if evidence-backed and test it directly. |

## Metadata

**Analog search scope:** `Application/Ac15/`, `Application/Handlers/`, `Application/Abstractions/`, `Application/Common/`, `Infrastructure/GameDataCatalog/`, `Infrastructure/DependencyInjection.cs`, `Adapters.GameProtocol.Kimidori/`, `Adapters.GameProtocol.Murasaki/`, `Adapters.GameProtocol.Yellow/`, `Adapters.GameProtocol.Momoiro/`, `Tests/Ac15/`, `Tests/Kimidori/`, `Tests/Green/`, `Tests/Yellow/`, `Tests/Momoiro/`, Phase 39 artifacts.

**Files scanned:** 90+ source/test/planning files through `rg` and targeted line-numbered reads.

**Strong analogs used:** KIMIDORI root-level catalog loader and metadata controllers, shared AC15 catalog snapshot/readback services, Kimidori recommendation/telop handler partials, Green telop tests, Yellow/Red mapper tests, Momoiro route-surface absence tests.

**Pattern extraction date:** 2026-06-26
