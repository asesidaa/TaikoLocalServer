# 03 — `Common*` DTO split, `GameEra` plumbing, handler per-era partials

**Surface:** Split every era-divergent `Common*` DTO into Shared / Nijiiro / Green partial files. Add `GameEra Era` field to every Mediator request that reaches per-era persistence. Split every per-concept handler into central-dispatch + per-era partial helpers. Green handler partials are stubs returning success defaults.

**Why this comes after 02:** Handlers now reference `*Nijiiro` DbSets (renamed in 02). The handler dispatch needs to know the era to pick the right DbSet, so adding `GameEra Era` to requests becomes meaningful here.

**Verification cadence:** `dotnet build` after each task. The Nijiiro path stays byte-identical in behavior (existing logic merely lifted into per-era partial files); Green path returns success defaults from stubs.

---

## Task 03.1: Split `Common*` DTOs into partial files (no Green-only fields yet)

**Goal:** Mechanical refactor — move every existing `Common*` DTO from a single file into a `*.cs` (base partial with shared fields) + `*.Nijiiro.cs` (the era-specific fields currently in the type, which are all Nijiiro-era today). No fields removed, no fields added. Just relocation.

**Files (for each `Common*` DTO that has era-specific fields — `CommonPlayResultData`, `CommonUserDataResponse`, `CommonBaidResponse`, plus the nested `CommonPlayResultData.StageData`):**
- Modify: existing `Application/Dtos/Common<X>.cs` → keep ONLY the genuinely shared fields; declare class as `partial`
- Create: `Application/Dtos/Common<X>.Nijiiro.cs` → the Nijiiro-only fields (and nested types' Nijiiro-only sub-fields if applicable)

**Acceptance Criteria:**
- [ ] At least `CommonPlayResultData`, `CommonUserDataResponse`, `CommonBaidResponse` are split into `.cs` + `.Nijiiro.cs`.
- [ ] All existing fields appear in exactly one of the two files; no loss.
- [ ] All consumer code keeps compiling (`.PropertyName` access works regardless of which partial file declared it).
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
```

**Steps:**

- [ ] **Step 1: For each Common DTO, identify the shared vs Nijiiro-only field set**

Looking at `CommonPlayResultData`:
- **Shared** (every era has these): `Baid`, `ChassisId`, `ShopId`, `PlayDatetime`, `IsRight`, `CardType`, `IsTwoPlayers`, `AryStageInfoes`, `ReleaseSongNoes`, `GetToneNoes`, `GetCostumeNo1s` through `GetCostumeNo5s`, `GetTitleNoes`, `AryPlayCostume`, `AryCurrentCostume`, `Title`, `TitleplateId`, `PlayMode`, `CollaborationId`, `DanId`, `DanResult`, `SoulGaugeTotal`, `ComboCntTotal`, `IsNotRecordedDan`, `AreaCode`, `Reserved`, `Accesstoken`, `ContentInfo`
- **Nijiiro-only** (the rest of CLAUDE.md's intent — Nijiiro's specific bits): `UraReleaseSongNoes`, `GetGenericInfoNoes`, `TournamentMode`, `DifficultyPlayedCourse`, `DifficultyPlayedStar`, `DifficultyPlayedSort`, `IsRandomUsePlay`, `InputMedian`, `InputVariance`

(Verify against the actual current `Application/Dtos/CommonPlayResultData.cs` — these are best-guess from the existing 60-line file; adjust to match reality.)

- [ ] **Step 2: Rewrite the base file as a partial with only shared fields**

```csharp
// Application/Dtos/CommonPlayResultData.cs
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonPlayResultData
{
    public uint            Baid                   { get; set; }
    public string          ChassisId              { get; set; } = string.Empty;
    public string          ShopId                 { get; set; } = string.Empty;
    public string          PlayDatetime           { get; set; } = string.Empty;
    public bool            IsRight                { get; set; }
    public uint            CardType               { get; set; }
    public bool            IsTwoPlayers           { get; set; }
    public List<StageData> AryStageInfoes         { get; set; } = [];
    public List<uint>      ReleaseSongNoes        { get; set; } = [];
    public List<uint>      GetToneNoes            { get; set; } = [];
    public List<uint>      GetCostumeNo1s         { get; set; } = [];
    public List<uint>      GetCostumeNo2s         { get; set; } = [];
    public List<uint>      GetCostumeNo3s         { get; set; } = [];
    public List<uint>      GetCostumeNo4s         { get; set; } = [];
    public List<uint>      GetCostumeNo5s         { get; set; } = [];
    public List<uint>      GetTitleNoes           { get; set; } = [];
    public CostumeData     AryPlayCostume         { get; set; } = new();
    public CostumeData     AryCurrentCostume      { get; set; } = new();
    public string          Title                  { get; set; } = string.Empty;
    public uint            TitleplateId           { get; set; }
    public uint            PlayMode               { get; set; }
    public uint            CollaborationId        { get; set; }
    public uint            DanId                  { get; set; }
    public uint            DanResult              { get; set; }
    public uint            SoulGaugeTotal         { get; set; }
    public uint            ComboCntTotal          { get; set; }
    public bool            IsNotRecordedDan       { get; set; }
    public uint            AreaCode               { get; set; }
    public byte[]          Reserved               { get; set; } = [];
    public string          Accesstoken            { get; set; } = string.Empty;
    public byte[]          ContentInfo            { get; set; } = [];

    public partial class StageData
    {
        // shared StageData fields ...
    }

    public partial class ResultcompeData { ... }
    public partial class AiStageSectionData { ... }
    public partial class CostumeData { ... }
}
```

- [ ] **Step 3: Create the `.Nijiiro.cs` partial with Nijiiro-only fields**

```csharp
// Application/Dtos/CommonPlayResultData.Nijiiro.cs
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonPlayResultData
{
    public List<uint> UraReleaseSongNoes     { get; set; } = [];
    public List<uint> GetGenericInfoNoes     { get; set; } = [];
    public uint       TournamentMode         { get; set; }
    public uint       DifficultyPlayedCourse { get; set; }
    public uint       DifficultyPlayedStar   { get; set; }
    public uint       DifficultyPlayedSort   { get; set; }
    public uint       IsRandomUsePlay        { get; set; }
    public string     InputMedian            { get; set; } = string.Empty;
    public string     InputVariance          { get; set; } = string.Empty;

    public partial class StageData
    {
        public bool IsPapamama   { get; set; }
        public uint StageMode    { get; set; }
        public uint NotesPosition{ get; set; }
        public bool IsVoiceOn    { get; set; }
        public bool IsSkipOn     { get; set; }
        public bool IsSkipUse    { get; set; }
        public uint? IsRandomUseStage { get; set; }
    }
}
```

- [ ] **Step 4: Same split for `CommonUserDataResponse` and `CommonBaidResponse`**

Apply the same shared/Nijiiro split pattern to these two DTOs.

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add Application/Dtos
git commit -m "refactor(app): split Common* DTOs into Shared + Nijiiro partial files"
```

---

## Task 03.2: Add Green-only fields to `Common*` DTOs via `.Green.cs` partials

**Goal:** Introduce the per-DTO `*.Green.cs` partial file that adds the Green-only union fields. No handler logic yet — these fields are populated only by Green mappers (which arrive in Task 05) and consumed only by Green handler partials (Task 03.4).

**Files:**
- Create: `Application/Dtos/CommonPlayResultData.Green.cs`
- Create: `Application/Dtos/CommonUserDataResponse.Green.cs`
- Create: `Application/Dtos/CommonBaidResponse.Green.cs`

**Acceptance Criteria:**
- [ ] Each file exists, declares `partial class Common<X>` and adds Green-only fields.
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
```

**Steps:**

- [ ] **Step 1: `CommonPlayResultData.Green.cs`**

```csharp
// Application/Dtos/CommonPlayResultData.Green.cs
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonPlayResultData
{
    public uint GetDonmedal              { get; set; }
    public uint GetKatsumedal            { get; set; }
    public bool BonusDailyFlg            { get; set; }
    public bool BonusWeeklyFlg           { get; set; }
    public bool BonusMonthlyFlg          { get; set; }
    public uint GenderType               { get; set; }
    public uint PlayerAge                { get; set; }
    public uint? LowerlimitAge           { get; set; }
    public uint? UpperlimitAge           { get; set; }
    public uint? AgeScore                { get; set; }
    public uint? EstimationCount         { get; set; }
    public uint? ItemshopTutorialFlg     { get; set; }
    public bool? IsDevil                 { get; set; }
    public bool? IsExplain               { get; set; }
    public uint? WaiwaiTutorialFlg       { get; set; }
    public List<CollaboData>     AryCollaboInfo    { get; set; } = [];
    public UpdateGhostInfoData?  GhostReleaseData  { get; set; }
    public UpdateGhostPerfData?  GhostUpdatePerfData { get; set; }
    public UpdateGhostRankData?  GhostUpdateRankData { get; set; }

    public partial class StageData
    {
        public uint? StarLevel       { get; set; }
        public uint? SupportLevel    { get; set; }
        public uint? WaiwaiResult    { get; set; }
        public uint? WaiwaiGauge     { get; set; }
        public uint? SoulGauge       { get; set; }
        public uint? HitCount        { get; set; }
        public uint? PlayDan         { get; set; }
        public GhostStageData? GhostStageData { get; set; }
    }

    public class CollaboData
    {
        public uint  CollaboSelect { get; set; }
        public uint? CollaboId     { get; set; }
        public uint? CollaboResult { get; set; }
    }

    public class GhostStageData
    {
        public bool IsWin                 { get; set; }
        public uint SdCertifiedLevelId    { get; set; }
        public List<GhostStageSectionData> ArySectionData { get; set; } = [];
    }

    public class GhostStageSectionData
    {
        public bool IsWin     { get; set; }
        public uint GoodCnt   { get; set; }
        public uint OkCnt     { get; set; }
        public uint NgCnt     { get; set; }
        public uint PoundCnt  { get; set; }
    }

    public class UpdateGhostInfoData
    {
        public List<uint> ReleaseInfoId { get; set; } = [];
        public List<GhostTokenData> AryTokendata { get; set; } = [];
    }

    public class GhostTokenData
    {
        public uint TokenId    { get; set; }
        public uint TokenValue { get; set; }
    }

    public class UpdateGhostPerfData
    {
        public int  InputMedian   { get; set; }
        public uint InputVariance { get; set; }
    }

    public class UpdateGhostRankData
    {
        public uint RankId            { get; set; }
        public uint WinPoint          { get; set; }
        public uint CertifiedLevelId  { get; set; }
        public List<GhostWinningsData> AryWinningsData { get; set; } = [];
    }

    public class GhostWinningsData
    {
        public uint LevelId  { get; set; }
        public uint Winnings { get; set; }
    }
}
```

- [ ] **Step 2: `CommonUserDataResponse.Green.cs`**

```csharp
// Application/Dtos/CommonUserDataResponse.Green.cs
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonUserDataResponse
{
    public List<FriendInfo>  AryFriendInfo         { get; set; } = [];
    public uint? CategJpopCnt                      { get; set; }
    public uint? CategAnimeCnt                     { get; set; }
    public uint? CategDoyoCnt                      { get; set; }
    public uint? CategVarietyCnt                   { get; set; }
    public uint? CategClassicCnt                   { get; set; }
    public uint? CategGameCnt                      { get; set; }
    public uint? CategNamcoCnt                     { get; set; }
    public uint? CategVocaloidCnt                  { get; set; }
    public uint? SongPushedCnt                     { get; set; }
    public uint? SongFavoriteCnt                   { get; set; }
    public uint? SongRecentCnt                     { get; set; }
    public uint? TotalCreditCnt                    { get; set; }
    public uint? PrevAreaCode                      { get; set; }
    public uint? ConsecAreaCnt                     { get; set; }
    public uint? RecommendSong                     { get; set; }
    public List<uint> RecommendBestSong            { get; set; } = [];
    public uint? DispLevelTotal                    { get; set; }
    public uint? DispLevelChassis                  { get; set; }
    public uint? DispLevelSelf                     { get; set; }
    public byte[]? DefaultOptionSetting            { get; set; }
    public bool? DefaultShinSetting                { get; set; }
    public uint? DispTaikojukuDan                  { get; set; }
    public uint? DifficultyPlayedCourse            { get; set; }
    public uint? DifficultyPlayedStar              { get; set; }
    public bool? IsChallengeCompe                  { get; set; }
    public bool? IsTojiru                          { get; set; }
    public bool? IsDevilGreen                      { get; set; }
    public uint? DispScoreType                     { get; set; }

    public class FriendInfo
    {
        public uint   Baid       { get; set; }
        public string FriendName { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 3: `CommonBaidResponse.Green.cs`**

```csharp
// Application/Dtos/CommonBaidResponse.Green.cs
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonBaidResponse
{
    // Costume 1..5 unlock flag bitfields
    public byte[]? CostumeFlg1 { get; set; }
    public byte[]? CostumeFlg2 { get; set; }
    public byte[]? CostumeFlg3 { get; set; }
    public byte[]? CostumeFlg4 { get; set; }
    public byte[]? CostumeFlg5 { get; set; }

    public uint?   TotalGetDonmedal     { get; set; }
    public uint?   TotalUseDonmedal     { get; set; }
    public uint?   TotalGetKatsumedal   { get; set; }
    public uint?   TotalUseKatsumedal   { get; set; }
    public uint?   ItemshopTutorialFlg  { get; set; }
    public bool?   IsAutoCostumeOn      { get; set; }
    public uint?   DispDanType          { get; set; }
    public uint?   GotDanMax            { get; set; }
    public byte[]? GotDanFlg            { get; set; }
    public byte[]? GotDanExtraFlg       { get; set; }
    public uint?   DefaultToneSetting   { get; set; }
    public string? PersonId             { get; set; }
    public uint?   WaiwaiTutorialFlg    { get; set; }
    public List<GreenCostumeSlots> AryFavoriteCostumeData { get; set; } = [];

    public class GreenCostumeSlots
    {
        public uint Costume1 { get; set; }
        public uint Costume2 { get; set; }
        public uint Costume3 { get; set; }
        public uint Costume4 { get; set; }
        public uint Costume5 { get; set; }
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Dtos
git commit -m "feat(app): add Green-only fields to Common* DTOs via .Green.cs partials"
```

---

## Task 03.3: Add `GameEra Era` to every Mediator request that touches per-era state

**Goal:** Append `GameEra Era` to each Mediator request record. Existing Nijiiro callers (`Adapters.GameProtocol.WwR08/Controllers/`, `CnR00`) pass `GameEra.Nijiiro` at the call site. Handlers compile but don't yet branch on `Era` (that's Task 03.4).

**Files:**
- Modify: every file under `Application/Handlers/` declaring a `readonly record struct *Command` or `*Query` that touches per-era state
- Modify: every controller in `Adapters.GameProtocol.WwR08/Controllers/` and `Adapters.GameProtocol.CnR00/Controllers/` whose `Mediator.Send(...)` call uses one of those requests

**The full list of request types to extend:**
- `UpdatePlayResultCommand` — `(uint Baid, CommonPlayResultData PlayResultData)` → `(uint Baid, GameEra Era, CommonPlayResultData PlayResultData)`
- `AddMyDonEntryCommand`
- `AddTokenCountCommand`
- `BaidQuery`
- `GetAiDataQuery`
- `GetAiScoreQuery`
- `GetDanScoreQuery`
- `GetFolderQuery`
- `GetSelfBestQuery`
- `GetShopFolderQuery`
- `GetSongIntroductionQuery`
- `GetTokenCountQuery`
- `PurchaseSongCommand`
- `UserDataQuery`
- `GetDanOdaiQuery`
- `GetInitialDataQuery`

(Adjust to the actual current set of request types. Use `grep -rn "IRequest<" Application/Handlers/` if uncertain.)

**Acceptance Criteria:**
- [ ] Every Mediator request record above has a `GameEra Era` parameter (position depends on each record's existing signature; canonical convention: `Baid` first if present, then `Era`, then payload).
- [ ] Every call site in `Adapters.GameProtocol.WwR08` and `Adapters.GameProtocol.CnR00` passes `GameEra.Nijiiro`.
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
grep -rn "new UpdatePlayResultCommand(" --include="*.cs" Adapters.GameProtocol.WwR08 Adapters.GameProtocol.CnR00
```
Expected: build PASSES; every match in the grep includes `GameEra.Nijiiro`.

**Steps:**

- [ ] **Step 1: Update `Application/Handlers/UpdatePlayResultCommand.cs` record signature**

```csharp
public readonly record struct UpdatePlayResultCommand(
    uint Baid,
    GameEra Era,
    CommonPlayResultData PlayResultData
) : IRequest<uint>;
```

(The handler class body stays — it'll be split in Task 03.4.)

- [ ] **Step 2: Update every other request record similarly**

For each `readonly record struct *Command` / `*Query` in `Application/Handlers/`, add a `GameEra Era` parameter at the canonical position (second, after `Baid` if present; otherwise first).

- [ ] **Step 3: Update every call site in WwR08 and CnR00**

For each `Mediator.Send(new <Request>(...))` call, insert `GameEra.Nijiiro` at the matching position. Example:

```csharp
// before
await Mediator.Send(new UpdatePlayResultCommand(request.BaidConf, commonRequest), HttpContext.RequestAborted);
// after
await Mediator.Send(new UpdatePlayResultCommand(request.BaidConf, GameEra.Nijiiro, commonRequest), HttpContext.RequestAborted);
```

Don't forget the `GameEra` `using` — add `using TaikoLocalServer.Domain.Enums;` if not globally imported. (Nijiiro+CnR00 GlobalUsings may or may not already include it; check `GlobalUsings.cs`.)

- [ ] **Step 4: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Application Adapters.GameProtocol.WwR08 Adapters.GameProtocol.CnR00
git commit -m "feat(app): add GameEra Era to every Mediator request; Nijiiro callers thread it"
```

---

## Task 03.4: Split each handler into central-dispatch + per-era partials. Green partials are stubs.

**Goal:** For each handler class, move its existing `Handle` body into a `HandleNijiiro` partial method (in a new `<HandlerName>.Nijiiro.cs` file). Replace the central `Handle` with an `Era` switch that dispatches to the appropriate `Handle<Era>` partial helper. Add a `<HandlerName>.Green.cs` partial with a `HandleGreen` stub.

**Files (per handler):**
- Modify: `Application/Handlers/<HandlerName>.cs` (record struct stays; `Handle` method body replaced with switch dispatch; declare `partial class <HandlerName>Handler`)
- Create: `Application/Handlers/<HandlerName>.Nijiiro.cs` (partial class; `HandleNijiiro` private async method with existing logic)
- Create: `Application/Handlers/<HandlerName>.Green.cs` (partial class; `HandleGreen` private async method returning success-shaped defaults)

**Pattern for each handler:**

```csharp
// <HandlerName>.cs (central)
public partial class <HandlerName>Handler(/* existing constructor deps */)
    : IRequestHandler<<HandlerName>Command, <ReturnType>>
{
    public ValueTask<<ReturnType>> Handle(<HandlerName>Command request, CancellationToken ct) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, ct),
        GameEra.Green   => HandleGreen(request, ct),
        _               => throw new InvalidOperationException($"Unsupported era: {request.Era}"),
    };

    private partial ValueTask<<ReturnType>> HandleNijiiro(<HandlerName>Command request, CancellationToken ct);
    private partial ValueTask<<ReturnType>> HandleGreen(<HandlerName>Command request, CancellationToken ct);
}

// <HandlerName>.Nijiiro.cs
public partial class <HandlerName>Handler
{
    private partial async ValueTask<<ReturnType>> HandleNijiiro(<HandlerName>Command request, CancellationToken ct)
    {
        /* the existing handler body — verbatim from before */
    }
}

// <HandlerName>.Green.cs
public partial class <HandlerName>Handler
{
    private partial async ValueTask<<ReturnType>> HandleGreen(<HandlerName>Command request, CancellationToken ct)
    {
        // TODO iter 2: implement Green handler logic
        return default!;   // or whatever success-shaped default the Common* response type wants
    }
}
```

**Acceptance Criteria:**
- [ ] Every handler under `Application/Handlers/` follows the central-dispatch + per-era partials pattern.
- [ ] Nijiiro path is byte-identical to before — existing tests / smoke checks would behave the same.
- [ ] Green stubs return success-shaped defaults that respect the return type's required initial state (e.g. `new CommonPlayResultData()` is the wrong default for `UpdatePlayResultCommand` which returns `uint` — return `1u` to signal success; for query handlers returning a `CommonXResponse`, return `new CommonXResponse { /* required defaults */ }`).
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
```

**Steps (illustrative for one handler — repeat for every handler in `Application/Handlers/`):**

- [ ] **Step 1: Move `UpdatePlayResultCommand` to the partial pattern**

`UpdatePlayResultCommand.cs`:
```csharp
namespace TaikoLocalServer.Application.Handlers;

public readonly record struct UpdatePlayResultCommand(
    uint Baid,
    GameEra Era,
    CommonPlayResultData PlayResultData
) : IRequest<uint>;

public partial class UpdatePlayResultCommandHandler(
    ITaikoDbContext context,
    ILogger<UpdatePlayResultCommandHandler> logger
) : IRequestHandler<UpdatePlayResultCommand, uint>
{
    public ValueTask<uint> Handle(UpdatePlayResultCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green   => HandleGreen(request, cancellationToken),
        _               => throw new InvalidOperationException($"Unsupported era: {request.Era}"),
    };

    private partial ValueTask<uint> HandleNijiiro(UpdatePlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleGreen(UpdatePlayResultCommand request, CancellationToken cancellationToken);
}
```

`UpdatePlayResultCommand.Nijiiro.cs`:
```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private partial async ValueTask<uint> HandleNijiiro(UpdatePlayResultCommand request, CancellationToken cancellationToken)
    {
        // Existing Handle body — copied verbatim. Make sure all DbSet references
        // now use the *Nijiiro suffix (renamed in Task 02.4) and that any reads
        // of UserDatum save-state columns now go through context.UserSaveDataNijiiro.
    }
}
```

`UpdatePlayResultCommand.Green.cs`:
```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private partial async ValueTask<uint> HandleGreen(UpdatePlayResultCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green PlayResult stub — Baid {Baid}, returning success", request.Baid);
        // TODO iter 2: write to SongBestDataGreen, SongPlayDataGreen, UserSaveDataGreen, etc.
        await Task.CompletedTask;
        return 1u;
    }
}
```

(Note: `logger` is the primary-constructor parameter — accessible inside partials since the handler is one class spread across files.)

- [ ] **Step 2: Apply the same pattern to every other handler**

Walk `Application/Handlers/`:
- `AddMyDonEntryCommand` → split
- `AddTokenCountCommand` → split (Green stub no-op; Nijiiro keeps existing)
- `BaidQuery` → split (Green stub returns `new CommonBaidResponse { Result = 1u, Baid = request.Baid }`-style defaults)
- `GetAiDataQuery` → split (Green stub returns empty; Green has no AI anyway, but the dispatch must exist if Green sends this request — actually Green doesn't have AI endpoints, so Green stub can throw `InvalidOperationException("AI not supported on Green")`. Defer to safest path: empty response, no throw.)
- `GetAiScoreQuery` → same as above
- `GetDanScoreQuery` → split (Green stub returns empty — Green doesn't have graded dan)
- `GetFolderQuery` → split (Green stub returns empty list)
- `GetSelfBestQuery` → split (Green stub returns empty)
- `GetShopFolderQuery` → split (Green stub returns empty)
- `GetSongIntroductionQuery` → split (Green stub returns empty — Green doesn't have this endpoint anyway)
- `GetTokenCountQuery` → split (Green stub returns empty)
- `PurchaseSongCommand` → split (Green stub no-op success)
- `UserDataQuery` → split (Green stub returns `new CommonUserDataResponse()` with defaults)
- `GetDanOdaiQuery` → split (Green stub returns empty)
- `GetInitialDataQuery` → split (Green stub returns minimal success response)

For each handler that takes constructor dependencies (`context`, `logger`, `clock`, `catalog`, etc.), declare the partial-class signature once in the central file with the primary constructor; each partial file can reference those parameters by name as they're in scope across the whole class.

- [ ] **Step 3: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 4: Verify Nijiiro behavior unchanged via existing handler test (if any)**

This codebase has no automated tests. The Nijiiro behavior check happens at smoke time (Task 08).

- [ ] **Step 5: Commit**

```bash
git add Application/Handlers
git commit -m "refactor(app): split handlers into central-dispatch + per-era partials; Green stubs return defaults"
```

---

## Task 03.5: Add Green-only Mediator request types (no Nijiiro counterpart)

**Goal:** Green has endpoints with no Nijiiro analog (taikojuku, getitemshopinfo, itempurchase, getghostdata, getghostscore, rewardcardcheck, rewardexecution, recommend, challengecompe). These need their own request types and handlers. All handlers stubbed.

**Files:**
- Create: `Application/Handlers/GetTaikojukuQuery.cs` + `GetTaikojukuQuery.Green.cs`
- Create: `Application/Handlers/GetItemShopInfoQuery.cs` + `GetItemShopInfoQuery.Green.cs`
- Create: `Application/Handlers/ItemPurchaseCommand.cs` + `ItemPurchaseCommand.Green.cs`
- Create: `Application/Handlers/GetGhostDataQuery.cs` + `GetGhostDataQuery.Green.cs`
- Create: `Application/Handlers/GetGhostScoreQuery.cs` + `GetGhostScoreQuery.Green.cs`
- Create: `Application/Handlers/RewardCardCheckQuery.cs` + `RewardCardCheckQuery.Green.cs`
- Create: `Application/Handlers/RewardExecutionCommand.cs` + `RewardExecutionCommand.Green.cs`
- Create: `Application/Handlers/GetRecommendQuery.cs` + `GetRecommendQuery.Green.cs`
- Create: `Application/Handlers/GetChallengeCompeQuery.cs` + `GetChallengeCompeQuery.Green.cs`
- Create: `Application/Handlers/TournamentCheckQuery.cs` + `TournamentCheckQuery.Green.cs` + `TournamentCheckQuery.Nijiiro.cs` (Nijiiro tournamentcheck exists today as an inline controller response; lift it here as a Mediator handler so the dispatch pattern is uniform)

These Green-only handlers do **not** carry `GameEra Era` — they're inherently Green-scoped — so they don't need the central-dispatch pattern. Just one handler class, one `Handle` method, stub body.

For `TournamentCheckQuery` which BOTH Nijiiro and Green respond to (differently), use the era-dispatch pattern from 03.4.

**Acceptance Criteria:**
- [ ] All listed handler files exist.
- [ ] Each request type defines a `Common*Response` return type — define those alongside in `Application/Dtos/` if they don't exist.
- [ ] Each Green stub returns a success-shaped empty response.
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
```

**Steps:**

- [ ] **Step 1: Define each request type's response DTO if missing**

For each Green-only endpoint, create a `CommonXResponse` in `Application/Dtos/` if it doesn't exist:
- `CommonTaikojukuResponse` — per-dan-level song lists (mirrors green.proto's TaikojukuResponse.JukupackData)
- `CommonItemShopInfoResponse` — item shop entries
- `CommonItemPurchaseResponse` — `{ Result, TotalGetDonmedal, TotalUseDonmedal }`
- `CommonGhostDataResponse` — ghost battle state for a player
- `CommonGhostScoreResponse` — best ghost-section data for a song
- `CommonRewardCardCheckResponse` — `{ Result, Baid }`
- `CommonRewardExecutionResponse` — `{ Result }`
- `CommonRecommendResponse` — `{ Result, RecommendSong, RecommendBestSong[] }`
- `CommonChallengeCompeResponse` — per-compe stat lists
- `CommonTournamentCheckResponse` — gacha + tournament fields (already shared across eras)

Each DTO follows the pattern from existing `CommonBaidResponse.cs`:
```csharp
namespace TaikoLocalServer.Application.Dtos;

public class CommonTaikojukuResponse
{
    public uint Result { get; set; } = 1;
    public List<JukupackData> AryJukupackData { get; set; } = [];

    public class JukupackData
    {
        public uint GetDan        { get; set; }
        public uint VerupNo       { get; set; }
        public List<JukusongData> AryJukusongData { get; set; } = [];
    }

    public class JukusongData
    {
        public uint SongNo { get; set; }
        public uint Level  { get; set; }
    }
}
```

- [ ] **Step 2: Create each Green-only handler**

```csharp
// Application/Handlers/GetTaikojukuQuery.cs
namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetTaikojukuQuery(IReadOnlyList<uint> RequestedDans) : IRequest<CommonTaikojukuResponse>;

public partial class GetTaikojukuQueryHandler(ILogger<GetTaikojukuQueryHandler> logger)
    : IRequestHandler<GetTaikojukuQuery, CommonTaikojukuResponse>
{
    public partial ValueTask<CommonTaikojukuResponse> Handle(GetTaikojukuQuery request, CancellationToken cancellationToken);
}
```

```csharp
// Application/Handlers/GetTaikojukuQuery.Green.cs
namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    public partial ValueTask<CommonTaikojukuResponse> Handle(GetTaikojukuQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green Taikojuku stub — requested dans: {Count}, returning empty", request.RequestedDans.Count);
        // TODO iter 2: read catalog.For(GameEra.Green).Taikojuku for the requested dan IDs
        return ValueTask.FromResult(new CommonTaikojukuResponse { Result = 1 });
    }
}
```

Apply the same skeleton to each of the nine Green-only handlers.

- [ ] **Step 3: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add Application/Handlers Application/Dtos
git commit -m "feat(app): add Green-only Mediator request types and stub handlers"
```
