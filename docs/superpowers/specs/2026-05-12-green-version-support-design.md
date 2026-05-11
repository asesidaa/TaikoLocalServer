# Green version support — design spec

**Date:** 2026-05-12
**Status:** approved, ready for implementation plan
**Scope:** stub-first server-side support for AC15 "Green" cabinets alongside the existing 39.06-era (Nijiiro) `WwR08` + `CnR00` adapters

## Goal

Add a third game-era adapter so that a Green cabinet (AC15 era, `/v11r01/chassis/...` routes) can complete its full network lifecycle against `TaikoLocalServer`, without disturbing existing Nijiiro behavior. Each era owns its own score and save-state tables; identity (Card / Credential / minimal UserDatum) is shared. This iteration delivers full project structure, EF migration, DI wiring, and stub handlers/controllers; real Green game logic and admin UI for Green are explicit follow-ups.

## Decisions summary

| Topic | Choice |
|---|---|
| DB partitioning | Separate tables in one `taiko.db3`. Shared `Card` / `Credential` / `UserDatum` (slim). Per-era `*_Nijiiro` / `*_Green` tables for score and save state. |
| `UserDatum` split | Slim shared core (`Baid`, `IsAdmin`, `MyDonName`, `MyDonNameLanguage`). Save state moves to per-era `UserSaveData_Nijiiro` and `UserSaveData_Green`. |
| Handler / DTO shape | One `Common*` DTO per concept with `GameEra Era` discriminator on the Mediator request. Each handler's central `Handle` dispatches by era to per-era partial helpers. |
| Data assets on disk | Per-era subfolders under `wwwroot/data/`: `data/nijiiro/`, `data/green/`, `data/shared/`. |
| Catalog | Per-era `IEraGameDataCatalog` implementations behind an `IGameDataCatalog.For(GameEra)` multiplex. Per-era entry types; shared interface holds only the genuinely common surface (music / telop / event folders). |
| Migration | Single in-place EF migration (`AddGreenEraSupport`). Renames Nijiiro tables, slims `UserDatum`, creates Green tables. Auto-applied at startup. |
| AllNet / Mucha | Unchanged. Green cabinets share the existing `PowerOn` / Mucha responses. |
| WebUI / Admin API | Out of scope for this iteration. Follow-up. |
| Era enabling | All eras opt-in via `ServerSettings.json:Eras`. Hard-fail at startup if zero enabled, or if an enabled era's required data files are missing. |
| File organization | C# `partial class` / `partial interface` per era for any type that grows by era (`TaikoDbContext`, `ITaikoDbContext`, `Common*` DTOs, per-concept handlers). |
| Iteration posture | **Stub-first.** All Green handlers, mappers, and catalog loaders ship as stubs in this iteration. |

## Architecture

### 1. New project layout

```
TaikoLocalServer.slnx
├── Domain                                  (unchanged + GameEra.cs added)
├── Contracts.AdminApi                      (unchanged)
├── Application                             (modified — see §3)
├── Infrastructure                          (modified — see §4, §5, §7)
├── Adapters.AdminApi                       (unchanged)
├── Adapters.AllnetMucha                    (unchanged)
├── Adapters.GameProtocol.Shared            (unchanged)
├── Adapters.GameProtocol.WwR08             (handlers refactored to per-era partials; routes unchanged)
├── Adapters.GameProtocol.CnR00             (same as WwR08)
├── Adapters.GameProtocol.Green             (NEW — see §6)
├── Host                                    (Program.cs + ServerSettings.json grow; see §8)
├── TaikoWebUI                              (unchanged)
└── LocalSaveModScoreMigrator               (unchanged)
```

Dependency direction is unchanged: `Domain ← Contracts.AdminApi ← Application ← Infrastructure ← Host`; adapters slot in at the right layer. `Adapters.GameProtocol.Green → Application` + `Adapters.GameProtocol.Shared` only; no cross-adapter references.

### 2. Domain — `GameEra` enum

```csharp
// Domain/Enums/GameEra.cs
namespace TaikoLocalServer.Domain.Enums;

public enum GameEra
{
    Nijiiro = 0,
    Green   = 1
}
```

Additive forever; numeric encodings are stable. Used as the discriminator on Mediator requests, the multiplex key on `IGameDataCatalog.For(GameEra)`, and as the in-memory tag distinguishing eras for any code that needs to branch.

### 3. Application — Common DTOs and handlers

#### Common DTO partial split

Every per-concept `Common*` DTO that has era-specific deltas splits into three partial files, by convention:

```
Application/Dtos/
  CommonPlayResultData.cs           // partial class — shared core fields
  CommonPlayResultData.Nijiiro.cs   // partial class — Nijiiro-only fields
                                    //   (UraReleaseSongNoes, AryPlayCostume, AryCurrentCostume,
                                    //    DifficultyPlayedSort, IsRandomUsePlay,
                                    //    InputMedian/InputVariance — Nijiiro's AI-battle bits)
  CommonPlayResultData.Green.cs     // partial class — Green-only fields
                                    //   (GetDonmedal, GetKatsumedal, GhostReleaseData,
                                    //    GhostUpdatePerfData, GhostUpdateRankData,
                                    //    AryCollaboInfo, BonusDaily/Weekly/MonthlyFlg,
                                    //    PlayerAge, GenderType, AgeScore, EstimationCount,
                                    //    ItemshopTutorialFlg, IsDevil, IsExplain)
```

Same three-file split applies to `CommonUserDataResponse`, `CommonBaidResponse`, `CommonPlayResultData.StageData` (nested type — `IsPapamama` is Nijiiro; `WaiwaiResult` / `WaiwaiGauge` / `SupportLevel` / `StarLevel` are Green), and any other DTOs that diverge by era.

#### Mediator requests carry `Era`

Every Mediator request that reaches a per-era persistence path gains a `GameEra Era` field. Records are `readonly record struct`, so positional adds are free:

```csharp
public readonly record struct UpdatePlayResultCommand(
    uint Baid,
    GameEra Era,
    CommonPlayResultData PlayResultData
) : IRequest<uint>;

public readonly record struct BaidQuery(GameEra Era, /*...*/) : IRequest<CommonBaidResponse>;
public readonly record struct UserDataQuery(uint Baid, GameEra Era) : IRequest<CommonUserDataResponse>;
public readonly record struct GetSelfBestQuery(uint Baid, GameEra Era, /*...*/) : IRequest<CommonSelfBestResponse>;
public readonly record struct GetCrownsQuery(uint Baid, GameEra Era) : IRequest<CommonCrownsResponse>;
public readonly record struct AddMyDonEntryCommand(GameEra Era, /*...*/) : IRequest<CommonBaidResponse>;
public readonly record struct GetFolderQuery(GameEra Era, IReadOnlyList<uint> FolderIds)
                                   : IRequest<CommonGetFolderResponse>;
public readonly record struct GetInitialDataQuery(GameEra Era, /*...*/)
                                   : IRequest<CommonInitialDataCheckResponse>;
// ...etc for every existing handler that touches per-era state.

// Green-only requests (no Nijiiro counterpart):
public readonly record struct GetTaikojukuQuery(IReadOnlyList<uint> RequestedDans)
                                   : IRequest<CommonTaikojukuResponse>;
public readonly record struct GetItemShopInfoQuery(/*...*/) : IRequest<CommonItemShopInfoResponse>;
public readonly record struct ItemPurchaseCommand(uint Baid, /*...*/) : IRequest<CommonItemPurchaseResponse>;
public readonly record struct GetGhostDataQuery(uint Baid) : IRequest<CommonGhostDataResponse>;
public readonly record struct GetGhostScoreQuery(uint Baid, uint SongNo, uint Level)
                                   : IRequest<CommonGhostScoreResponse>;
public readonly record struct GetRecommendQuery(uint GenderType, uint PlayerAge)
                                   : IRequest<CommonRecommendResponse>;
public readonly record struct RewardCardCheckQuery(/*...*/) : IRequest<CommonRewardCardCheckResponse>;
public readonly record struct RewardExecutionCommand(uint Baid, /*...*/) : IRequest<CommonRewardExecutionResponse>;
public readonly record struct GetChallengeCompeQuery(uint Baid) : IRequest<CommonChallengeCompeResponse>;
public readonly record struct TournamentCheckQuery(GameEra Era, uint KitId)
                                   : IRequest<CommonTournamentCheckResponse>;
```

#### Handler partials per era

```
Application/Handlers/
  UpdatePlayResultCommand.cs            // partial class — record struct, Handle() dispatching on Era
  UpdatePlayResultCommand.Nijiiro.cs    // partial class — async ValueTask<uint> HandleNijiiro(...)
                                        //   writes SongBestDataNijiiro, SongPlayDataNijiiro,
                                        //   DanScoreDataNijiiro, AiScoreDataNijiiro
                                        //   (existing logic lifted verbatim from current handler)
  UpdatePlayResultCommand.Green.cs      // partial class — async ValueTask<uint> HandleGreen(...)
                                        //   STUB: returns 1 (success) without writing.
                                        //   Real implementation in iter 2.
```

Central dispatch:

```csharp
public ValueTask<uint> Handle(UpdatePlayResultCommand cmd, CancellationToken ct) => cmd.Era switch
{
    GameEra.Nijiiro => HandleNijiiro(cmd, ct),
    GameEra.Green   => HandleGreen(cmd, ct),
    _               => throw new InvalidOperationException($"Unsupported era: {cmd.Era}"),
};
```

All Green handler partials in this iteration are stubs that return success-shaped empty / default responses, log a TODO at debug level, and don't touch the DB.

### 4. Application — Catalog abstractions

```csharp
// Application/Abstractions/IGameDataCatalog.cs
public interface IGameDataCatalog
{
    IEraGameDataCatalog For(GameEra era);
    Task InitializeAsync(CancellationToken ct = default);
}

// Application/Abstractions/IEraGameDataCatalog.cs
public interface IEraGameDataCatalog
{
    GameEra Era { get; }
    IReadOnlyDictionary<uint, IMusicInfoEntry>   MusicInfos       { get; }
    IReadOnlyCollection<IEventFolderEntry>       EventFolders     { get; }
    IReadOnlyDictionary<uint, ITelopEntry>       Telops           { get; }
    Task InitializeAsync(CancellationToken ct);
}

// Application/Abstractions/IMusicInfoEntry.cs
public interface IMusicInfoEntry
{
    uint SongNo { get; }
    string Title { get; }
    // only what every era genuinely has and that handlers might cross-era lookup
}
```

Era-specific catalog surfaces (`Nijiiro.DanData`, `Green.TaikojukuEntries`, `Green.ItemShopEntries`, etc.) live on the concrete `NijiiroEraGameDataCatalog` / `GreenEraGameDataCatalog` types. Handlers in per-era partial files cast `For(GameEra.Green)` to `GreenEraGameDataCatalog` to reach era-specific entries.

### 5. Infrastructure — Catalog implementations

```
Infrastructure/GameDataCatalog/
  PathHelper.cs                       // existing surface kept; new helpers:
                                      //   GetDataPath(GameEra)   -> wwwroot/data/<era>/
                                      //   GetSharedDataPath()    -> wwwroot/data/shared/
                                      //   GetDataTablePath(GameEra) -> wwwroot/data/<era>/datatable/

  FileGameDataCatalog.cs              // aggregator. Holds:
                                      //   ImmutableDictionary<GameEra, IEraGameDataCatalog>
                                      // ctor takes the set of enabled eras' catalogs.
                                      // For(era) => map[era] (throws if not enabled — caller bug).
                                      // InitializeAsync awaits each enabled child's InitializeAsync.

  Nijiiro/
    NijiiroEraGameDataCatalog.cs      // existing Nijiiro logic, re-wrapped behind IEraGameDataCatalog.
    NijiiroMusicInfoLoader.cs         // parses wwwroot/data/nijiiro/datatable/musicinfo.bin
    NijiiroMusicInfoEntry.cs
    NijiiroDanLoader.cs               // wwwroot/data/nijiiro/dan_data.json
    NijiiroDanEntry.cs                // 3906 odai-style dan
    NijiiroEventFolderLoader.cs
    NijiiroEventFolderEntry.cs
    NijiiroShopLoader.cs              // wwwroot/data/nijiiro/shop_folder_data.json
    NijiiroShopEntry.cs
    NijiiroIntroLoader.cs             // wwwroot/data/nijiiro/intro_data.json
    NijiiroIntroEntry.cs
    NijiiroTelopLoader.cs
    NijiiroTelopEntry.cs

  Green/
    GreenEraGameDataCatalog.cs        // IEraGameDataCatalog impl
    GreenMusicInfoLoader.cs           // parses wwwroot/data/green/datatable/musicinfo.bin.
                                      //   Binary format diverges from Nijiiro; separate parser.
                                      //   STUB in this iteration: returns empty.
    GreenMusicInfoEntry.cs            // Green's music info entry shape (8 category counters,
                                      //   no ai event fields, etc.)
    GreenTaikojukuLoader.cs           // wwwroot/data/green/taikojuku_data.json.
                                      //   Produces map<dan-level, list<(song_no, level)>>.
                                      //   Taikojuku is a CATALOG endpoint — given a dan-level
                                      //   filter, returns the songs the cabinet should let the
                                      //   player choose. Not a graded dan system; no per-user
                                      //   score storage. STUB initially.
    GreenTaikojukuEntry.cs            // value object { DanLevel, IReadOnlyList<TaikojukuSong> }
    GreenEventFolderLoader.cs         // wwwroot/data/green/eventfolder_data.json
    GreenEventFolderEntry.cs          // Green's EventfolderData has fewer fields than Nijiiro's
    GreenItemShopLoader.cs            // wwwroot/data/green/itemshop_data.json
    GreenItemShopEntry.cs             // (item_no, item_type, item_id, item_price)
    GreenTelopLoader.cs               // Green telop catalog
    GreenTelopEntry.cs
    GreenGachaLoader.cs               // tournament-check gacha tables
                                      //   (ary_gacha_{song,tone,costume_1..5,title}_data)
    GreenGachaEntry.cs
    GreenTournamentLoader.cs          // TournamentcheckResponse top-level fields
                                      //   (rare_rate, song_hash_ver)
    GreenTournamentEntry.cs
```

#### File layout on disk

```
wwwroot/data/
├── nijiiro/
│   ├── dan_data.json
│   ├── event_folder_data.json
│   ├── gaiden_data.json
│   ├── intro_data.json
│   ├── locked_*_data.json
│   ├── movie_data.json
│   ├── shop_folder_data.json
│   ├── special_songs_data.json
│   └── datatable/
│       ├── musicinfo.bin
│       ├── music_order.bin
│       ├── wordlist.bin
│       ├── don_cos_reward.bin
│       ├── shougou.bin
│       └── neiro.bin
├── green/
│   ├── taikojuku_data.json
│   ├── eventfolder_data.json
│   ├── itemshop_data.json
│   ├── telop_data.json
│   ├── tournament_data.json
│   └── datatable/
│       ├── musicinfo.bin
│       └── (any other binary tables — exact list determined by iter 2
│            cabinet-binary reverse-engineering)
└── shared/
    ├── token_data.json
    ├── qrcode_data.json
    └── any other genuinely cross-era files
```

Existing operators on the `dev` branch already have files under `wwwroot/data/`; the migration plan (§9) includes moving those into `wwwroot/data/nijiiro/`.

#### Missing-file behavior

If an enabled era's loader can't find a required file:
- The loader **throws** with a clear message naming the missing path.
- `FileGameDataCatalog.InitializeAsync` propagates the exception.
- `Program.cs` lets it propagate out of the startup try-block — Serilog logs it as `Fatal` and the process exits.

Disabled eras' catalogs are not constructed in the first place, so their files don't have to exist.

### 6. New project — `Adapters.GameProtocol.Green`

Sibling of `Adapters.GameProtocol.WwR08` / `CnR00`. Owns Green-specific wire types, Mapperly mappers, and controllers.

```
Adapters.GameProtocol.Green/
  GlobalUsings.cs                       // mirrors WwR08's globals
  Adapters.GameProtocol.Green.csproj    // references Application, Adapters.GameProtocol.Shared
                                        //   + martinothamar.Mediator, protobuf-net, Riok.Mapperly
  GreenAdapterMarker.cs                 // empty marker class used by the ApplicationPart filter
                                        //   in Host/Program.cs to identify this assembly.
  DependencyInjection.cs                // public const GameEra Era = GameEra.Green;
                                        // AddGameProtocolGreen(this IServiceCollection s):
                                        //   s.AddSingleton<Mappers...>();
                                        //   return s;
                                        // Controller discovery is via the default
                                        // ApplicationPartManager (Host removes this assembly's
                                        // part if Green isn't enabled — see §8).

  Wire/
    Green.cs                            // protobuf-net types from proto/green/green.proto
                                        //   (all the messages enumerated in proto/green/green.proto)
    VsInterface.cs                      // duplicated copy of vsinterface.proto types
                                        //   (matches the existing WwR08 duplication pattern;
                                        //    could be promoted to Shared in a future cleanup)

  Controllers/                          // every route under /v11r01/chassis/<endpoint>.php.
                                        // All controllers in this iteration are STUBS returning
                                        // success-shaped empty defaults.
    HeartbeatController.cs              // chassis/heartbeat.php  — local response, no Mediator
    StartupAuthController.cs            // chassis/startupauth.php (vsinterface)
    VerupAuthController.cs              // chassis/verupauth.php (vsinterface)
    VerupCompleteController.cs          // chassis/verupcomplete.php (vsinterface)
    BookkeepingController.cs            // chassis/bookkeeping.php
    InitialDataCheckController.cs       // chassis/initialdatacheck.php
    TournamentCheckController.cs        // chassis/tournamentcheck.php
    GetTelopController.cs               // chassis/gettelop.php
    GetFolderController.cs              // chassis/getfolder.php
    BaidController.cs                   // chassis/baidcheck.php
    MyDonEntryController.cs             // chassis/mydonentry.php
    UserDataController.cs               // chassis/userdata.php
    PlayResultController.cs             // chassis/playresult.php
    SelfBestController.cs               // chassis/selfbest.php
    CrownsDataController.cs             // chassis/crownsdata.php
    ChallengeCompeController.cs         // chassis/challengecompe.php
    RecommendController.cs              // chassis/recommend.php
    ItemPurchaseController.cs           // chassis/itempurchase.php
    GetItemShopInfoController.cs        // chassis/getitemshopinfo.php
    GetGhostDataController.cs           // chassis/getghostdata.php
    GetGhostScoreController.cs          // chassis/getghostscore.php
    RewardCardCheckController.cs        // chassis/rewardcardcheck.php
    RewardExecutionController.cs        // chassis/rewardexecution.php
    TaikojukuController.cs              // chassis/taikojuku.php — catalog-read only
    HeadClerk2Controller.cs             // chassis/headclerk2.php — emits CSV log line

  Mappers/                              // Mapperly source-generated mappers. Stubs in this
                                        // iteration — declarations exist, partial method bodies
                                        // have TODOs where the wire ↔ Common mapping isn't yet
                                        // exercised.
    BaidResponseMapper.cs
    UserDataMappers.cs
    PlayResultMappers.cs
    SelfBestMappers.cs
    InitialDataMappers.cs
    FolderDataMappers.cs
    TaikojukuMappers.cs
    ItemShopMappers.cs
    ChallengeCompeMappers.cs
    GhostMappers.cs
    TournamentMappers.cs
    RecommendMappers.cs
```

Controllers inherit `BaseProtocolController` (from `Adapters.GameProtocol.Shared`). Each controller lazily resolves `IMediator` and `Logger` from `HttpContext.RequestServices`, matching the existing pattern.

Stub-controller pattern (example):

```csharp
[ApiController]
[Route("/v11r01/chassis/playresult.php")]
public class PlayResultController : BaseProtocolController<PlayResultController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> PlayResult([FromBody] PlayResultRequest request)
    {
        Logger.LogInformation("Green PlayResult request: {Request}", request.Stringify());
        // TODO iter 2: decode playresult_data → CommonPlayResultData → UpdatePlayResultCommand(Era.Green)
        return Ok(new PlayResultResponse { Result = 1 });
    }
}
```

### 7. Infrastructure — DbContext and `ITaikoDbContext` partial split

Following the per-era partial-file rule:

```
Application/Abstractions/
  ITaikoDbContext.cs           // partial interface ITaikoDbContext {
                               //   Task<int> SaveChangesAsync(CancellationToken ct = default);
                               // }
  ITaikoDbContext.Shared.cs    // partial interface ITaikoDbContext {
                               //   DbSet<Card> Cards { get; }
                               //   DbSet<Credential> Credentials { get; }
                               //   DbSet<UserDatum> UserData { get; }       // slim
                               //   DbSet<Token> Tokens { get; }
                               // }
  ITaikoDbContext.Nijiiro.cs   // partial interface ITaikoDbContext {
                               //   DbSet<SongBestDatumNijiiro>        SongBestDataNijiiro       { get; }
                               //   DbSet<SongPlayDatumNijiiro>        SongPlayDataNijiiro       { get; }
                               //   DbSet<DanScoreDatumNijiiro>        DanScoreDataNijiiro       { get; }
                               //   DbSet<DanStageScoreDatumNijiiro>   DanStageScoreDataNijiiro  { get; }
                               //   DbSet<AiScoreDatumNijiiro>         AiScoreDataNijiiro        { get; }
                               //   DbSet<AiSectionScoreDatumNijiiro>  AiSectionScoreDataNijiiro { get; }
                               //   DbSet<UserSaveDataNijiiro>         UserSaveDataNijiiro       { get; }
                               // }
  ITaikoDbContext.Green.cs     // partial interface ITaikoDbContext {
                               //   DbSet<SongBestDatumGreen>          SongBestDataGreen         { get; }
                               //   DbSet<SongPlayDatumGreen>          SongPlayDataGreen         { get; }
                               //   DbSet<UserSaveDataGreen>           UserSaveDataGreen         { get; }
                               //   DbSet<GhostStageSectionDatumGreen> GhostStageSectionDataGreen { get; }
                               //   DbSet<GreenGhostWinnings>          GreenGhostWinnings        { get; }
                               //   DbSet<GreenGhostTokens>            GreenGhostTokens          { get; }
                               //   DbSet<GreenFriends>                GreenFriends              { get; }
                               //   DbSet<GreenFavoriteSongs>          GreenFavoriteSongs        { get; }
                               //   DbSet<GreenRecentSongs>            GreenRecentSongs          { get; }
                               // }
```

Green has **no** `DanScoreDataGreen` / `DanStageScoreDataGreen` — taikojuku is a catalog-read concept, not a graded dan system.

```
Infrastructure/Persistence/
  TaikoDbContext.cs            // partial class. Holds ctor, OnConfiguring, and OnModelCreating
                               // which calls OnModelCreatingShared(b), OnModelCreatingNijiiro(b),
                               // OnModelCreatingGreen(b), OnModelCreatingPartial(b).
                               // Declares partial void hooks.
  TaikoDbContext.Shared.cs     // partial class — Card/Credential/UserDatum/Token DbSets +
                               //   OnModelCreatingShared(ModelBuilder)
  TaikoDbContext.Nijiiro.cs    // partial class — Nijiiro DbSets + OnModelCreatingNijiiro
  TaikoDbContext.Green.cs      // partial class — Green DbSets + OnModelCreatingGreen
  TaikoDbContextPartial.cs     // existing — keep as-is
```

Adding a future era is: drop in `ITaikoDbContext.<Era>.cs` + `TaikoDbContext.<Era>.cs`, declare a new `partial void OnModelCreating<Era>(ModelBuilder);` hook in `TaikoDbContext.cs`, add one new line to its `OnModelCreating`. Existing per-era files are not touched.

### 8. Host — startup wiring and ServerSettings

```jsonc
// Host/Configurations/ServerSettings.json
{
  "EnableMoreSongs": false,
  "Eras": {
    "Nijiiro": { "Enabled": true },
    "Green":   { "Enabled": false }
  }
}
```

```csharp
// Application/Settings/ServerSettings.cs (extended)
public sealed class ServerSettings
{
    public bool EnableMoreSongs { get; set; }
    public Dictionary<string, EraSettings> Eras { get; set; } = new();
}
public sealed class EraSettings
{
    public bool Enabled { get; set; }
}
```

Program.cs (additions, abbreviated):

```csharp
var serverSettings = builder.Configuration.GetSection("ServerSettings").Get<ServerSettings>()!;
var enabledEras = serverSettings.Eras
    .Where(kv => kv.Value.Enabled)
    .Select(kv => Enum.Parse<GameEra>(kv.Key, ignoreCase: true))
    .ToHashSet();
if (enabledEras.Count == 0)
{
    Log.Fatal("ServerSettings.Eras has no enabled era. Refusing to start.");
    throw new InvalidOperationException("No game eras enabled.");
}

builder.Services.AddControllers()
    .AddProtoBufNet()
    .ConfigureApplicationPartManager(apm =>
    {
        // Adapter assemblies are auto-discovered by default (matches the pre-Green behavior
        // for WwR08/CnR00/AdminApi/AllnetMucha). For era opt-in, we explicitly REMOVE
        // disabled-era adapter assemblies from the ApplicationPart list so their controllers
        // are never routed.
        if (!enabledEras.Contains(GameEra.Green))
        {
            var greenPart = apm.ApplicationParts.FirstOrDefault(p =>
                p is AssemblyPart a &&
                a.Assembly.GetName().Name == "Adapters.GameProtocol.Green");
            if (greenPart is not null) apm.ApplicationParts.Remove(greenPart);
        }
        if (!enabledEras.Contains(GameEra.Nijiiro))
        {
            foreach (var name in new[] { "Adapters.GameProtocol.WwR08", "Adapters.GameProtocol.CnR00" })
            {
                var part = apm.ApplicationParts.FirstOrDefault(p =>
                    p is AssemblyPart a && a.Assembly.GetName().Name == name);
                if (part is not null) apm.ApplicationParts.Remove(part);
            }
        }
    });

if (enabledEras.Contains(GameEra.Nijiiro))
{
    builder.Services.AddGameProtocolWwR08();
    builder.Services.AddGameProtocolCnR00();
}
if (enabledEras.Contains(GameEra.Green))
{
    builder.Services.AddGameProtocolGreen();
}
builder.Services.AddAdminApi(builder.Configuration);
builder.Services.AddAllnetMucha();

// AddInfrastructure now takes enabledEras and only constructs / registers the catalogs
// it'll actually need:
builder.Services.AddInfrastructure(builder.Configuration, enabledEras);
```

`AddInfrastructure(IConfiguration, ISet<GameEra>)` registers per-era catalogs only for enabled eras, and registers `IGameDataCatalog` as a singleton with only those eras in its multiplex map.

After `app.Build()`:

```csharp
var catalog = app.Services.GetRequiredService<IGameDataCatalog>();
await catalog.InitializeAsync(); // throws on missing data files for any enabled era
```

### 9. Migration — `AddGreenEraSupport`

Single EF migration; one forward step that:

1. **Rename Nijiiro tables**
   ```
   SongBestDatum         → SongBestDatum_Nijiiro
   SongPlayDatum         → SongPlayDatum_Nijiiro
   DanScoreDatum         → DanScoreDatum_Nijiiro
   DanStageScoreDatum    → DanStageScoreDatum_Nijiiro
   AiScoreDatum          → AiScoreDatum_Nijiiro
   AiSectionScoreDatum   → AiSectionScoreDatum_Nijiiro
   ```

2. **Slim `UserDatum` + extract `UserSaveData_Nijiiro`**
   - `CreateTable UserSaveData_Nijiiro` (PK `Baid`, all save-state columns currently on `UserDatum`).
   - `INSERT INTO UserSaveData_Nijiiro (...) SELECT (...) FROM UserDatum`.
   - Use SQLite table-rebuild pattern (or EF 10's improved `DropColumn`) to drop the moved columns from `UserDatum`. After rebuild, `UserDatum` columns are: `Baid`, `IsAdmin`, `MyDonName`, `MyDonNameLanguage`.

3. **Create empty Green tables**
   ```
   SongBestDatum_Green   (PK Baid, SongId, Difficulty)
                         (BestScore, BestRate, BestCrown — no BestScoreRank)
   SongPlayDatum_Green   (PK Id; per-play history with Green-specific cols
                         like StarLevel, SupportLevel, WaiwaiResult, WaiwaiGauge, SoulGauge)
   UserSaveData_Green    (PK Baid; Green's save-state union)
   GhostStageSectionDatum_Green  (PK PlayId, SectionNo; FK PlayId → SongPlayDatum_Green.Id)
   GreenGhostWinnings    (PK Baid, LevelId)
   GreenGhostTokens      (PK Baid, TokenId)
   GreenFriends          (PK Baid, FriendBaid)
   GreenFavoriteSongs    (PK Baid, SongNo)
   GreenRecentSongs      (PK Baid, SongNo)
   ```
   Note: **no** `DanScoreDatum_Green` / `DanStageScoreDatum_Green` (taikojuku is catalog-only).

4. **Re-create indexes and FKs** against the renamed Nijiiro tables and on new Green tables. FKs against `UserDatum.Baid` for new tables.

5. **Move existing on-disk data files** (operator-side, called out in iter 1 README change but not part of the EF migration):
   ```
   wwwroot/data/dan_data.json          → wwwroot/data/nijiiro/dan_data.json
   wwwroot/data/event_folder_data.json → wwwroot/data/nijiiro/event_folder_data.json
   wwwroot/data/movie_data.json        → wwwroot/data/nijiiro/movie_data.json
   wwwroot/data/intro_data.json        → wwwroot/data/nijiiro/intro_data.json
   wwwroot/data/locked_*_data.json     → wwwroot/data/nijiiro/locked_*_data.json
   wwwroot/data/qrcode_data.json       → wwwroot/data/shared/qrcode_data.json
   wwwroot/data/token_data.json        → wwwroot/data/shared/token_data.json
   wwwroot/data/special_songs_data.json → wwwroot/data/nijiiro/special_songs_data.json
   wwwroot/data/shop_folder_data.json  → wwwroot/data/nijiiro/shop_folder_data.json
   wwwroot/data/gaiden_data.json       → wwwroot/data/nijiiro/gaiden_data.json
   wwwroot/data/datatable/*.bin        → wwwroot/data/nijiiro/datatable/*.bin
   ```
   This file move is documented in `Host/README.md` as part of upgrade steps; it isn't done by the running server.

Migration filename: `Infrastructure/Persistence/Migrations/20260512xxxxxx_AddGreenEraSupport.cs`.

Down migration is generated (EF requires it) but treated as best-effort; not exercised in dev.

### 10. AllNet / Mucha

Unchanged. Green cabinets PowerOn against the same `Adapters.AllnetMucha` endpoints as Nijiiro. The PowerOn response routes the cabinet to whichever URL the operator has configured. If a Green cabinet is to be served, the operator simply points it at the same host, and the cabinet's internal config drives it to `/v11r01/chassis/...` routes (which the Green adapter owns).

### 11. WebUI / Admin API

Unchanged in this iteration. The admin API today reads from `SongBestData` etc.; after the migration those DbSet properties are renamed to `SongBestDataNijiiro` (etc.), so admin handlers need a one-line per-call update to point at the renamed DbSet. No API surface change visible to TaikoWebUI.

Green admin views and admin API endpoints are scoped to a follow-up iteration.

## Out-of-scope follow-ups

| Iter | Scope |
|---|---|
| 2 | Real Green handler bodies; real Mapperly bodies; real catalog loaders (including the Green `musicinfo.bin` format parser); operator documentation for Green data files. |
| 3 | Admin API + TaikoWebUI Green surfaces. Era selector, per-era player profile pages, donmedal/katsumedal balances, taikojuku progress view, ghost battle stats. |
| 4 (opt) | `LocalSaveModScoreMigrator` Green support. |

## Risks and open questions

- **Green `musicinfo.bin` format unknown.** The binary layout may differ from Nijiiro's. Iter 2 must reverse-engineer it (likely via the same `.rodata` / cabinet-binary technique used to extract the route table). Stub the loader and field set in iter 1 so the wiring is in place.
- **`vsinterface.proto` duplication.** Today's pattern is per-adapter duplication of the wire types. Green follows suit. A future cleanup could promote `VsInterface.cs` to `Adapters.GameProtocol.Shared`; explicitly out of scope here.
- **`play_dan` persistence.** Green's `PlayResultDataRequest` includes `play_dan` and `dan_result`. These reflect the active taikojuku filter at play time, not a graded dan performance. We don't persist them in iter 1; if iter 2 needs them, add a column to `UserSaveData_Green` (`LastPlayedTaikojukuDanLevel`) — easier additive change than designing around now.
- **Region differentiation under Nijiiro.** WwR08 and CnR00 both register as Nijiiro today. If we later need to enable only Cn or only Ww, the era settings can grow per-region children (`Eras.Nijiiro.Regions: ["Ww", "Cn"]`). Not needed for iter 1.
- **`SongBestDatum.Difficulty` for Green ura/shin.** Green's `SelfBestResponse` includes `ary_selfbest_score` and `ary_shin_selfbest_score`. The existing `Difficulty` enum needs to cover Ura and Shin (most likely already does for Nijiiro). Iter 2 confirms when wiring real best-score reads.
