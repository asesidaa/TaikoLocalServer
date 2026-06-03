# Phase 07: Tokkun Evidence Contract and Guardrail Reset - Pattern Map

**Mapped:** 2026-06-03
**Files analyzed:** 2
**Analogs found:** 2 / 2 target files

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` | documentation | transform | `.planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md` | exact |
| `Tests/Blue/BlueA4SourceGuardTests.cs` | test | batch/source-scan reset | `Tests/Blue/BlueA4SourceGuardTests.cs` | exact-self |

## Pattern Assignments

### `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` (documentation, transform)

**Primary analog:** `.planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md`

**Supporting analogs:**
- `.planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md`
- `.planning/milestones/v1.0-phases/05-blue-battle-runtime-support/05-RESOLUTION.md`

**Header and scope pattern** (`04-01-BATTLE-EVIDENCE.md` lines 1-7):

```markdown
# Phase 04 Plan 04-01: Blue Battle Route And Wire Evidence

## Scope And Guardrails

This evidence pack covers BTEV-01 route/equivalent-client evidence and BTEV-02 proto-to-generated-wire ownership for the first Phase 4 battle slice. It is a planning artifact only. It does not approve battle runtime behavior, persistence, loaders, migrations, generated battle JSON, or replacement of the current `BattleUserDataController` stub.

Phase 4 follows the locked decisions from `04-CONTEXT.md`: D-01 accepts static client/proto evidence instead of requiring RPCS3 or cabinet logs in this phase; D-02 makes IDA/client behavior authoritative for wire/runtime mechanics when it disagrees with proto, generated wire, or XML; D-03 keeps wiki evidence to gameplay semantics only; D-04 requires traceable field-by-field evidence with citations; D-05 requires unresolved battle fields/defaults to be routed to case-by-case user approval before Phase 5 relies on them.
```

Copy this structure for Tokkun, replacing battle-specific references with Phase 7 decisions. The Tokkun contract must explicitly say it is docs-only and does not approve runtime DTO, mapper, handler, route, persistence, migration, wire, or cabinet/RPCS3 changes.

**Evidence priority pattern** (`04-01-BATTLE-EVIDENCE.md` lines 9-17):

```markdown
## Evidence Priority

| Priority | Evidence Type | Use In This Artifact | Source |
|----------|---------------|----------------------|--------|
| 1 | IDA/client behavior | Confirms route strings, transport/framing, and runtime mechanics when available. The A0 evidence bootstrap cites `.tools/blue/EBOOT.ELF.i64` and the `sub_2D5A28`, `sub_2DE860`, and `sub_2DCB64` findings. | `docs/superpowers/specs/2026-05-27-blue-a0-evidence-bootstrap-design.md:8`, `docs/superpowers/specs/2026-05-27-blue-a0-evidence-bootstrap-design.md:17`, `docs/superpowers/specs/2026-05-27-blue-a0-evidence-bootstrap-design.md:86` |
| 2 | Proto and generated Blue wire | Maps candidate request/response shape and presence helpers. Proto/wire is not proof of safe defaults per D-02 and D-11. | `proto/blue/taiko.proto:91`, `proto/blue/taiko.proto:699`, `Adapters.GameProtocol.Blue/Wire/Game.cs:329`, `Adapters.GameProtocol.Blue/Wire/Game.cs:3328` |
| 3 | Current server source and tests | Records present ownership and read-only stub/reference state. | `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs:4`, `Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs:4`, `Adapters.GameProtocol.Blue/Controllers/PlayResultController.cs:4`, `Tests/Blue/BlueRouteSkeletonTests.cs:5` |
| 4 | Gameplay references | Useful later for gameplay semantics, not wire/default authority in this document. | `.planning/phases/04-blue-battle-evidence-and-design/04-CONTEXT.md` D-03 |
| Gate | Unknowns | Any unproven route requirement, byte width, default, or row-count claim is marked `UNKNOWN - requires case-by-case user approval before Phase 5 relies on it`. | `.planning/phases/04-blue-battle-evidence-and-design/04-CONTEXT.md` D-05 |
```

Adapt to the four required Tokkun statuses: `proven`, `observed`, `deliberately ignored`, and `unknown/blocked`. Keep local proto/wire/current-code/IDA evidence above wiki or gameplay context.

**Row matrix pattern** (`04-01-BATTLE-EVIDENCE.md` lines 21-25):

```markdown
| Endpoint | Current Owner File | Request Wire Type | Response Wire Type | Evidence Source | Proven Required For Battle Menu Entry? | May Phase 5 Implement Against It? |
|----------|--------------------|-------------------|--------------------|-----------------|----------------------------------------|-----------------------------------|
| `/v10r03/chassis/initialdatacheck.php` | `Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs:4` | `InitialdatacheckRequest` | `InitialdatacheckResponse` | A0 IDB route list confirms the path under `/v10r03`; proto defines battle advertisement fields; generated wire has `ShouldSerialize*` presence helpers; current controller maps through `GetInitialDataQuery(GameEra.Blue)`. Sources: `docs/superpowers/specs/2026-05-27-blue-a0-evidence-bootstrap-design.md:30`, `proto/blue/taiko.proto:91`, `Adapters.GameProtocol.Blue/Wire/Game.cs:423`, `Adapters.GameProtocol.Blue/Controllers/InitialDataCheckController.cs:12`. | `UNKNOWN - requires case-by-case user approval before Phase 5 relies on it`. The path and fields are proven; exact menu-entry requirement and safe values are not proven here. | Only as a candidate Phase 5 surface after 04-03 approves battle advertisement behavior and any field defaults/omissions. |
```

Use a Tokkun version with columns like `Row`, `Subject`, `Status`, `Evidence`, `Phase 7 Contract`, `Blocked Assumptions`, and `Follow-on Owner`. Put protocol fields, route questions, classifier decisions, guard reset decisions, and deferred persistence/readback rows in the same matrix.

**Unknown handoff pattern** (`04-01-BATTLE-EVIDENCE.md` lines 127-139):

```markdown
## Unknowns For 04-03 Gate

| Unknown | Why It Matters | 04-03 Gate Requirement |
|---------|----------------|------------------------|
| Whether `initialdatacheck.php` battle fields are required for battle menu entry, and whether omission, explicit false, or explicit true is safest. | Incorrect advertisement could hide battle mode or expose a broken flow. | `UNKNOWN - requires case-by-case user approval before Phase 5 relies on it`. Gate must approve exact `is_battleplay`, `release_battle_stage_flg`, `release_battle_special_flg`, and `battle_bonds_lv_cap` behavior. |

## No Runtime Write Targets

This plan intentionally does not modify runtime source, tests, loaders, migrations, generated wire, committed battle JSON, or local Blue game data. Phase 4 writes only planning evidence. Runtime write targets such as `Infrastructure/GameDataCatalog/Blue/*Battle*Loader.cs`, `Host/wwwroot/data/blue/*battle*.json`, `Infrastructure/Persistence/Migrations/*Battle*.cs`, `Domain/Entities/*Battle*.cs`, `Application/Handlers/*Battle*.cs`, and `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs` remain out of scope until the approved Phase 4 gate.
```

Copy the explicit "No Runtime Write Targets" style. For Phase 7, list forbidden targets: `CommonPlayResultData.BlueTokkun.cs`, `UpdatePlayResultCommand.BlueTokkun.cs`, Tokkun entities/DbSets/migrations, `GetbanacoininfoController.cs`, manual wire edits, generated `Wire/` cleanup, and cabinet/RPCS3 proof work.

**Gate checklist pattern** (`04-03-BLUE-BATTLE-DESIGN-GATE.md` lines 125-140):

```markdown
## Phase 5 Gate Checklist

The gate fails closed. A missing prerequisite or unresolved `UNKNOWN` without a named user approval blocks Phase 5 runtime planning.

| Check | Required Evidence | Current Result | Consequence |
|---|---|---|---|
| 04-01 artifact exists | `.planning/phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md` | PASS | Route/proto evidence may be cited, but route-default unknowns remain blocked. |
| BTEV-04 defaults/widths/row counts | Every battle default/width/row count proven or approved | FAIL_CLOSED | Phase 5 is blocked unless each `REQUIRED_UNKNOWN` row below receives proof or user approval. |
```

Use this for a Tokkun follow-on checklist. Rows should hand off numeric `play_mode` to Phase 11 evidence, `getbanacoininfo.php` to Phase 8, mapper/classifier runtime work to Phase 9, and persistence/readback to Phase 10.

**Resolution matrix pattern** (`05-RESOLUTION.md` lines 11-16):

```markdown
## Resolution Matrix

| # | Phase 4 Missing-Evidence Row | Phase 5 Status | Evidence Source | Runtime Use | Next Gate |
|---:|---|---|---|---|---|
| 1 | Battle menu entry sequence and required `initialdatacheck.php` fields | APPROVED_BY_USER_DATA_DERIVED_INITIALDATA | IDA proves the route (`sub_2E3A60`), response handling (`sub_143720`), and battle availability candidate (`sub_250F04`). Latest user decision on 2026-05-31 approved Blue battle initialdata advertisement from the required local battle XML set when all five files exist and parse. | allowed: set `is_battleplay=true` only when the required Blue battle XML set exists and parses; otherwise emit explicit false/default battle initialdata fields | Cabinet/RPCS3 smoke still required for full validation; do not hardcode constants or derive values outside the parsed Blue battle catalog. |
```

Do not use battle status names literally. Tokkun status vocabulary is locked to `proven`, `observed`, `deliberately ignored`, and `unknown/blocked`.

**Runtime rule pattern** (`05-RESOLUTION.md` lines 42-50):

```markdown
## Runtime Rule

Rows with `STILL_MISSING_*`, `NEEDS_USER_APPROVAL`, `DEFER_RUNTIME_USE`, or `DEFER_*` must not be used to emit battle protocol values, advertise battle availability, derive rewards, mutate progression, or mirror normal Blue unlocks.

`PROVEN` rows are narrow. Rows 7-16 permit only the exact battleuserdata response parser/default behavior described above; they do not approve menu timing, initialdata mirroring, token rewards, stage graph behavior, stage `33`, boss-life completion, or normal unlock mirrors. Row 17 permits battle request classification and normal-state bypass, not progression or reward effects.
```

Use this exact narrow-proof style for Tokkun. A proven/observed Tokkun field does not approve score saving, reward grants, Banacoin state, normal unlock mirrors, or battle state writes.

### `Tests/Blue/BlueA4SourceGuardTests.cs` (test, batch/source-scan reset)

**Analog:** `Tests/Blue/BlueA4SourceGuardTests.cs`

**Namespace and test class pattern** (lines 1-4):

```csharp
namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueA4SourceGuardTests
{
```

No imports are needed in this file. Keep the existing xUnit implicit using style.

**Keep existing Green dependency guards** (lines 5-31):

```csharp
[Fact]
public void BlueA4ApplicationCode_DoesNotDependOnGreenProtocolState()
{
    var root = FindRepoRoot();
    var files = new[]
    {
        Path.Combine(root, "Application", "Handlers", "UpdatePlayResultCommand.Blue.cs"),
        Path.Combine(root, "Application", "Handlers", "GetSelfBestQuery.Blue.cs"),
        Path.Combine(root, "Application", "Handlers", "UserDataQuery.Blue.cs"),
        Path.Combine(root, "Application", "Common", "BlueProtocolBytes.cs"),
        Path.Combine(root, "Application", "Common", "BluePlayResultMapping.cs"),
        Path.Combine(root, "Application", "Common", "BlueProfileCounters.cs"),
        Path.Combine(root, "Application", "Common", "BlueCrownResponseBuilder.cs")
    };

    foreach (var file in files)
    {
        var source = File.ReadAllText(file);
        Assert.DoesNotContain("GreenProtocolBytes", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Adapters.GameProtocol.Green", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SongBestDatumGreen", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SongPlayDatumGreen", source, StringComparison.Ordinal);
        Assert.DoesNotContain("UserSaveDataGreen", source, StringComparison.Ordinal);
        Assert.DoesNotContain("GreenShop", source, StringComparison.Ordinal);
        Assert.DoesNotContain("GreenGhost", source, StringComparison.Ordinal);
        Assert.DoesNotContain("GreenDan", source, StringComparison.Ordinal);
    }
}
```

Phase 7 should not disturb these non-Tokkun Green-leakage guards.

**Delete stale Tokkun source guard** (lines 52-71):

```csharp
[Fact]
public void BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics()
{
    var root = FindRepoRoot();
    var files = Directory.GetFiles(Path.Combine(root, "Application", "Dtos"), "CommonPlayResultData*.cs")
        .Concat(
        [
            Path.Combine(root, "Application", "Handlers", "UpdatePlayResultCommand.Blue.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Controllers", "PlayResultController.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Mappers", "PlayResultMappers.cs")
        ]);

    foreach (var file in files)
    {
        var source = File.ReadAllText(file);
        Assert.DoesNotContain("Tokkun", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Tookun", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("deferred fields", source, StringComparison.OrdinalIgnoreCase);
    }
}
```

This is the Phase 7 removal target. Do not replace it with another Tokkun word scan or named allowlist.

**Keep repo root helper** (lines 73-86):

```csharp
private static string FindRepoRoot()
{
    for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
         directory is not null;
         directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
        {
            return directory.FullName;
        }
    }

    throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
}
```

The remaining Green guard tests still depend on this helper.

**Historical anti-pattern to avoid** (`Tests/Blue/BlueBattleSourceGuardTests.cs` lines 82-100):

```csharp
private static readonly string[] ForbiddenNormalBlueBattleEffects =
[
    "ApplyUnlockBits",
    "SaveBlueStageAsync",
    "UpsertBestAsync",
    "UpsertBlueFavoriteAndRecentAsync",
    "SaveBlueDanAsync",
    "BlueProfileCounters.ApplyStage",
    "ReleaseSongNoes",
    "GetToneNoes",
    "GetCostumeNo",
    "GetTitleNoes",
    "BlueShopItemStates",
    "SongPlayDataBlue",
    "SongBestDataBlue",
    "DanScoreDataBlue",
    "DanStageScoreDataBlue",
    "BlueFavoriteSongs"
];
```

Use only as historical context. Phase 7 locked decisions reject new Tokkun-specific source-scanning guardrail tests.

## Shared Patterns

### Tokkun Protocol Evidence

**Source:** `proto/blue/taiko.proto`
**Apply to:** `07-01-TOKKUN-EVIDENCE-CONTRACT.md`

Userdata tutorial/readback field (lines 300-319):

```protobuf
optional uint32 difficulty_played_course = 35;
optional uint32 difficulty_played_star = 36;
optional uint32 tokkun_tutorial_flg = 37;
optional bool is_challengecompe = 38;
optional bool is_tojiru = 39;
```

Playresult classifier and summary field candidates (lines 420-452):

```protobuf
required uint32 play_mode = 29;
required uint32 area_code = 30;
required bytes reserved = 31;
optional uint32 payment_method = 43;
optional uint32 tokkun_tutorial_flg = 44;

optional TokkunstageData ary_tokkunstage_info = 45;
message TokkunstageData {
    required string banacoin_datetime = 1;
    required uint32 tokkun_song_cnt = 2;
    repeated uint32 tookun_songno = 3;
    required uint32 tokkun_speedchange_cnt = 4;
    required uint32 tokkun_autoplay_cnt = 5;
    required uint32 tokkun_jump_cnt = 6;
}
```

Banacoin route shape exists in schema only; Phase 8 owns route availability (lines 632-650):

```protobuf
message GetbanacoininfoRequest {
    required uint32 device_type = 1;
    required string access_code = 2;
    required string chip_id = 3;
    required string chassis_id = 4;
    required string shop_id = 5;
    required string country_id = 6;
}

message GetbanacoininfoResponse {
    required uint32 result = 1;
```

### Generated Wire Evidence

**Source:** `Adapters.GameProtocol.Blue/Wire/Game.cs`
**Apply to:** `07-01-TOKKUN-EVIDENCE-CONTRACT.md`

Userdata optional presence helper (lines 1745-1753):

```csharp
[global::ProtoBuf.ProtoMember(37, Name = @"tokkun_tutorial_flg")]
public uint TokkunTutorialFlg
{
    get => __pbn__TokkunTutorialFlg.GetValueOrDefault();
    set => __pbn__TokkunTutorialFlg = value;
}
public bool ShouldSerializeTokkunTutorialFlg() => __pbn__TokkunTutorialFlg != null;
public void ResetTokkunTutorialFlg() => __pbn__TokkunTutorialFlg = null;
private uint? __pbn__TokkunTutorialFlg;
```

Playresult Tokkun fields (lines 2027-2038):

```csharp
[global::ProtoBuf.ProtoMember(44, Name = @"tokkun_tutorial_flg")]
public uint TokkunTutorialFlg
{
    get => __pbn__TokkunTutorialFlg.GetValueOrDefault();
    set => __pbn__TokkunTutorialFlg = value;
}
public bool ShouldSerializeTokkunTutorialFlg() => __pbn__TokkunTutorialFlg != null;
public void ResetTokkunTutorialFlg() => __pbn__TokkunTutorialFlg = null;
private uint? __pbn__TokkunTutorialFlg;

[global::ProtoBuf.ProtoMember(45, Name = @"ary_tokkunstage_info")]
public TokkunstageData AryTokkunstageInfo { get; set; }
```

Summary facts (lines 2375-2397):

```csharp
public partial class TokkunstageData : global::ProtoBuf.IExtensible
{
    [global::ProtoBuf.ProtoMember(1, Name = @"banacoin_datetime", IsRequired = true)]
    public string BanacoinDatetime { get; set; }

    [global::ProtoBuf.ProtoMember(2, Name = @"tokkun_song_cnt", IsRequired = true)]
    public uint TokkunSongCnt { get; set; }

    [global::ProtoBuf.ProtoMember(3, Name = @"tookun_songno")]
    public uint[] TookunSongnoes { get; set; }

    [global::ProtoBuf.ProtoMember(4, Name = @"tokkun_speedchange_cnt", IsRequired = true)]
    public uint TokkunSpeedchangeCnt { get; set; }
```

Do not edit generated wire in Phase 7.

### Current Runtime Boundary

**Source:** `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`
**Apply to:** `07-01-TOKKUN-EVIDENCE-CONTRACT.md`

Current mapper has a battle classifier, but no Tokkun classifier (lines 8-59):

```csharp
public static CommonPlayResultData Map(PlayResultRequest request)
{
    return new CommonPlayResultData
    {
        Baid = request.Baid,
        ChassisId = request.ChassisId,
        ShopId = request.ShopId,
        AryStageInfoes = request.AryStageInfoes.Select(MapStage).ToList(),
        PlayMode = request.PlayMode,
        IsBattlePlayResult = request.AryReleaseBattledata is not null
            || request.AryStageInfoes.Any(stage => stage.AryBattlestagedata is not null),
        BattleReleaseData = MapBattleReleaseData(request.AryReleaseBattledata),
        WaiwaiTutorialFlg = request.ShouldSerializeWaiwaiTutorialFlg() ? request.WaiwaiTutorialFlg : null
    };
}
```

Contract rows should hand off mapper/DTO changes to Phase 9. Phase 7 must not add them.

**Source:** `Application/Handlers/UpdatePlayResultCommand.Blue.cs`
**Apply to:** `07-01-TOKKUN-EVIDENCE-CONTRACT.md`

Current branch and normal side effects to protect (lines 30-103):

```csharp
var playResultData = request.PlayResultData;
if (playResultData.IsBattlePlayResult)
{
    return await HandleBlueBattle(request.Baid, playResultData, cancellationToken);
}

var saveData = await context.GetOrCreateBlueSaveDataAsync(request.Baid, cancellationToken);
...
ApplyUnlockBits(saveData, playResultData);

foreach (var stage in playResultData.AryStageInfoes)
{
    BlueProfileCounters.ApplyStage(saveData, stage);
    await SaveBlueStageAsync(request.Baid, stage, playResultData.PlayMode, playTime, cancellationToken);
}

await SaveBlueDanAsync(saveData, playResultData, blue, cancellationToken);
```

Contract rows should state that future Tokkun-classified uploads must bypass these normal writes, but Phase 7 does not implement that branch.

**Source:** `Domain/Enums/PlayMode.cs`
**Apply to:** `07-01-TOKKUN-EVIDENCE-CONTRACT.md`

Current known values (lines 3-8):

```csharp
public enum PlayMode
{
    Normal = 0,
    DanMode = 1,
    GaidenMode = 4,
    AiBattle = 6
}
```

Do not add `PlayMode.Tokkun` in Phase 7.

### Route Unknown Boundary

**Source:** `Tests/Blue/BlueRouteSkeletonTests.cs`
**Apply to:** `07-01-TOKKUN-EVIDENCE-CONTRACT.md`

Existing Blue route exclusions (lines 46-59):

```csharp
[Theory]
[InlineData("/v10r03/chassis/getreitai.php")]
[InlineData("/v10r03/chassis/getbanacoininfo.php")]
[InlineData("/v10r03/chassis/startupauth.php")]
[InlineData("/v10r03/chassis/verupauth.php")]
[InlineData("/v10r03/chassis/verupcomplete.php")]
public void BlueAdapter_DoesNotOwnExcludedOrSharedRoutes(string routeTemplate)
{
    var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Blue.DependencyInjection).Assembly)
        .Where(route => route.Template == routeTemplate)
        .ToList();

    Assert.Empty(routes);
}
```

The contract should list `getbanacoininfo.php` as Phase 8 unknown/blocked. Do not plan a route edit in Phase 7.

## No Analog Found

No target file lacks a useful analog.

| File | Role | Data Flow | Reason |
|------|------|-----------|--------|
| - | - | - | - |

## Metadata

**Analog search scope:** `.planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design`, `.planning/milestones/v1.0-phases/05-blue-battle-runtime-support`, `Tests/Blue`, `proto/blue`, `Adapters.GameProtocol.Blue`, `Application`, `Domain`
**Files scanned:** 515
**Project skill context:** `.codex/skills` listed; `gsd-plan-phase/SKILL.md` read for local workflow gating rules.
**Pattern extraction date:** 2026-06-03
