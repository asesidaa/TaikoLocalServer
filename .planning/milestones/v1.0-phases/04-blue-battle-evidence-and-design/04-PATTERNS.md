# Phase 04: Blue Battle Evidence And Design - Pattern Map

**Mapped:** 2026-05-30
**Files analyzed:** 9 candidate Phase 4 artifacts, plus read-only Blue protocol references
**Analogs found:** 9 / 9

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `.planning/phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md` | evidence doc | batch, request-response evidence | `docs/superpowers/specs/2026-05-27-blue-a0-evidence-bootstrap-design.md` | exact-doc-shape |
| `.planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md` | evidence doc | file-I/O audit, transform | `.planning/phases/04-blue-battle-evidence-and-design/04-RESEARCH.md` data inventory | exact-existing-input |
| `.planning/phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md` | design/gate doc | batch, decision gate | `.planning/phases/04-blue-battle-evidence-and-design/04-RESEARCH.md` Phase 5 gate inputs | exact-existing-input |
| `Tests/Blue/BlueBattleEvidenceTests.cs` (optional) | test | batch/static-analysis | `Tests/Blue/BlueDocsTests.cs` and `Tests/Blue/BlueRouteSkeletonTests.cs` | role-match |
| `Tests/Blue/BlueBattleSourceGuardTests.cs` (optional) | test | batch/static-analysis | `Tests/Blue/BlueA6SourceGuardTests.cs` | exact-shape |
| `Tests/Blue/BlueInitialDataTests.cs` (optional extension) | test | request-response, transform | existing `Tests/Blue/BlueInitialDataTests.cs` | existing-to-extend |
| `Tests/Blue/BluePlayResultMapperTests.cs` (optional extension) | test | transform | existing `Tests/Blue/BluePlayResultMapperTests.cs` | existing-to-extend |
| `.planning/phases/04-blue-battle-evidence-and-design/04-VALIDATION.md` (optional status update only) | validation doc | batch | current Phase 4 validation table | exact-existing-input |
| `.planning/phases/04-blue-battle-evidence-and-design/04-03-SUMMARY.md` (if closeout writes one) | summary/gate doc | batch | `.planning/phases/01-blue-a6-item-shop-and-unlocking/01-06-SUMMARY.md` | role-match |

## Pattern Assignments

### `.planning/phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md` (evidence doc, request-response evidence)

**Analog:** `docs/superpowers/specs/2026-05-27-blue-a0-evidence-bootstrap-design.md`

**Evidence input pattern** (lines 8-16):
```markdown
## Evidence Inputs

- Blue IDB: `.tools/blue/EBOOT.ELF.i64`
- Local protos: `proto/blue/taiko.proto`, `proto/blue/vsinterface.proto`
- Existing shared startup route:
  `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`
- Existing missing-content-type workaround: `Host/Program.cs`
- Local Blue data symlink: `Host/wwwroot/data/blue/data`
```

**Route evidence pattern** (lines 17-55):
```markdown
## Confirmed Routes

The Blue binary builds two base URLs in `sub_2D5A28`:

- Game endpoints use `https://%s:%s@%s:%d/v10r03`.
- Startup/version endpoints use `https://%s:%s@%s:%d/v01r00`.
...
- `/v10r03/chassis/battleuserdata.php`
```

**Apply to Phase 4:** Copy this structure, but narrow it to battle menu entry and attempted battle flow. The evidence pack should include route sequence rows for `initialdatacheck.php`, `battleuserdata.php`, and `playresult.php`, with the concrete source for each claim: proto, generated wire, IDA/client note, static client string, local XML candidate, wiki gameplay note, or explicit unknown.

**Cautions:**
- Context lines 18-23 say Phase 4 uses static client/proto evidence, IDA/client behavior wins for wire mechanics, wiki is gameplay-only, and unproven defaults need user approval.
- Do not require RPCS3/cabinet logs in Phase 4 unless the user revises D-01.

---

### `.planning/phases/04-blue-battle-evidence-and-design/04-02-BATTLE-DATA-INVENTORY.md` (evidence doc, file-I/O audit)

**Analog:** `.planning/phases/04-blue-battle-evidence-and-design/04-RESEARCH.md`

**Inventory table pattern** (lines 145-157):
```markdown
## Data File Inventory

The local Blue data tree is a symbolic link from `Host/wwwroot/data/blue/data` ...

| File | Present | Size | SHA-256 | Verified Counts | Key Fields / Candidate Role | Menu Entry Classification |
|------|---------|------|---------|-----------------|-----------------------------|---------------------------|
| `battleadjsetting.xml` | yes ... | ... | ... | One `adjustedsetting` node ... | ... candidate role | Unknown until IDA/client proves whether menu entry needs it. |
...
**Important data-planning constraint:** Phase 4 can document row counts, hashes, key fields, and candidate relationships only; committed loaders, committed runtime JSON, and migrations belong to Phase 5 or later after the gate passes.
```

**Field matrix style analog:** `docs/green-client-evidence/03-score-crown-counter-fields.md` lines 7-18:
```markdown
| Field | IDA evidence | Constraint | 0 valid? | Max/list length | Server behavior at audit baseline/current tree | Recommendation |
|---|---|---|---|---|---|---|
| `crownsdata.hash_crown_flg` | IDA-proven field string ... | Source-inferred zlib body inflates to `1280` bytes ... | All-zero table valid empty state. | Fixed 1280-byte inflated body. | ... | Add catalog membership filtering ... |
```

**Apply to Phase 4:** Use two tables:
- File inventory: all five XML files, hash, size, row counts, key fields, candidate role, menu-entry classification.
- Default/width matrix: battle field, wire property, optional/required, candidate XML source, proven width/default, row-count proof, unresolved status, Phase 5 task impact.

**Cautions:**
- Context lines 25-29 require all five files and forbid committed runtime JSON/loaders in Phase 4.
- XML row counts are evidence of local file shape only, not proof of required response rows.

---

### `.planning/phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md` (design/gate doc, batch decision)

**Analog:** `.planning/phases/04-blue-battle-evidence-and-design/04-RESEARCH.md`

**Phase 5 gate input pattern** (lines 183-192):
```markdown
| Gate Input | Must Contain | Blocks Phase 5 If Missing |
|------------|--------------|---------------------------|
| Battle route evidence pack | Endpoint sequence ... | yes, unless user revises BTEV-01. |
| Proto/wire field matrix | Every battle proto field, generated wire property, presence helper status, current mapper/handler owner, and Phase 5 owner. | yes for BTEV-02. |
| Local XML inventory appendix | All five file hashes, row counts, key fields, and candidate relationships ... | yes for BTEV-03. |
| Default/width proof matrix | Width/default proof for every battle byte array, repeated row, stage assignment, boss/last-stage field, NPC field, and token field. | yes for BTEV-04 unless user approves an exception. |
| Implementation gate review | A final "approved / blocked / approved with exceptions" status with user-approved unknowns listed one by one. | yes for BTEV-06. |
```

**Validation gate pattern** from `04-VALIDATION.md` lines 48-53:
```markdown
| T-04: Unsafe byte/default guesses enter Phase 5 plans | BTEV-04 | Gate fails on any required field left `UNKNOWN` without explicit user-approved exception. |
| T-06: Battle implementation starts before evidence gate closes | BTEV-06 | Final gate artifact records `APPROVED`, `BLOCKED`, or `APPROVED_WITH_USER_EXCEPTIONS` before Phase 5. |
```

**Apply to Phase 4:** The design gate should end with one status: `APPROVED`, `BLOCKED`, or `APPROVED_WITH_USER_EXCEPTIONS`. If exceptions exist, list each field/default/row-count separately with the user approval source. Do not allow "all unknown defaults are zero" as a gate outcome.

**Cautions:**
- Context lines 43-47 say Phase 5 runtime requires approved Phase 4 evidence/design and that Phase 5 implementation must be Blue-owned only.
- Green AI Battle can be a negative source-guard contrast only.

---

### Optional `Tests/Blue/BlueBattleEvidenceTests.cs` (test, batch/static-analysis)

**Analogs:** `Tests/Blue/BlueDocsTests.cs`, `Tests/Blue/BlueRouteSkeletonTests.cs`

**Doc guard pattern** from `Tests/Blue/BlueDocsTests.cs` lines 15-26:
```csharp
[Fact]
public void HostReadme_DocumentsBlueSymlinkAndBattleDeferral()
{
    var source = File.ReadAllText(FindRepoFile("Host", "README.md"));

    Assert.Contains("Blue AC15 Test Support", source, StringComparison.Ordinal);
    Assert.Contains("S10100-1", source, StringComparison.Ordinal);
    Assert.Contains("wwwroot/data/blue/data", source, StringComparison.Ordinal);
    Assert.Contains("config/S10100-1/battle", source, StringComparison.Ordinal);
    Assert.Contains("Track B", source, StringComparison.Ordinal);
}
```

**Route ownership guard pattern** from `Tests/Blue/BlueRouteSkeletonTests.cs` lines 61-91:
```csharp
[Fact]
public void BlueControllers_DoNotCallMediatorOutsideImplementedEndpoints()
{
    var root = FindRepoRoot();
    var controllersRoot = Path.Combine(root, "Adapters.GameProtocol.Blue", "Controllers");
    var mediatorBackedControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "BaidController.cs",
        "MyDonEntryController.cs",
        "InitialDataCheckController.cs",
        "UserDataController.cs",
        "PlayResultController.cs"
    };

    foreach (var file in Directory.EnumerateFiles(controllersRoot, "*.cs", SearchOption.AllDirectories))
    {
        if (mediatorBackedControllers.Contains(Path.GetFileName(file)))
        {
            continue;
        }

        var source = File.ReadAllText(file);
        Assert.DoesNotContain("Mediator.Send", source, StringComparison.Ordinal);
    }
}
```

**Apply to Phase 4:** If the planner wants automated doc completeness, create a doc/source guard that checks Phase 4 artifacts contain:
- `InitialdatacheckResponse`, `BattleUserDataResponse`, `BattleStageData`, `ReleaseBattleData`.
- all five XML filenames.
- `APPROVED`, `BLOCKED`, or `APPROVED_WITH_USER_EXCEPTIONS`.
- `battleuserdata.php`, `initialdatacheck.php`, and `playresult.php`.

**Caution:** This test should validate documentation/evidence completeness only. It must not imply battle runtime behavior is implemented.

---

### Optional `Tests/Blue/BlueBattleSourceGuardTests.cs` (test, batch/static-analysis)

**Analog:** `Tests/Blue/BlueA6SourceGuardTests.cs`

**Forbidden-reference list pattern** (lines 5-14):
```csharp
private static readonly string[] ForbiddenGreenShopReferences =
[
    "GreenShop",
    "GreenShopItemStatus",
    "GreenProtocolBytes",
    "Adapters.GameProtocol.Green",
    "UserSaveDataGreen",
    "GreenShopSeasonStates",
    "GreenShopItemStates"
];
```

**File scan pattern** (lines 101-110):
```csharp
private static void AssertFilesDoNotContainForbiddenReferences(IEnumerable<string> files)
{
    foreach (var file in files)
    {
        var source = File.ReadAllText(file);
        foreach (var forbidden in ForbiddenGreenShopReferences)
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
        }
    }
}
```

**Apply to Phase 4:** For a battle source guard, scan only Phase 4 docs or future Phase 5 Blue battle files when they exist. The forbidden list should target Green AI Battle truth leakage, for example `GreenAiBattle`, `GreenStageModeInterpreter`, `GetAiDataQuery.Green`, `GetAiScoreQuery.Green`, `GhostStageData`, and `Application/Handlers/UpdatePlayResultCommand.Green.cs`.

**Cautions:**
- Do not scan generated wire files for generic strings unless the expected false positives are understood.
- Do not use Green AI Battle tests or handlers as a behavioral template for Blue battle.

---

### Optional `Tests/Blue/BlueInitialDataTests.cs` Extension (test, request-response transform)

**Analogs:** existing `Tests/Blue/BlueInitialDataTests.cs`, `Application/Dtos/CommonInitialDataCheckResponse.Blue.cs`, `Adapters.GameProtocol.Blue/Mappers/InitialDataMappers.cs`

**Existing omission guard** from `Tests/Blue/BlueInitialDataTests.cs` lines 28-33:
```csharp
Assert.Empty(response.AryBlueLegaltermsDatas);
Assert.Empty(wire.AryLegaltermsDatas);
Assert.False(wire.ShouldSerializeIsBattleplay());
Assert.False(wire.ShouldSerializeReleaseBattleStageFlg());
Assert.False(wire.ShouldSerializeReleaseBattleSpecialFlg());
Assert.False(wire.ShouldSerializeBattleBondsLvCap());
```

**Nullable common DTO pattern** from `CommonInitialDataCheckResponse.Blue.cs` lines 5-11:
```csharp
public bool? IsBattleplay { get; set; }
public byte[]? ReleaseBattleStageFlg { get; set; }
public byte[]? ReleaseBattleSpecialFlg { get; set; }
public uint? BattleBondsLvCap { get; set; }
```

**Mapper presence pattern** from `InitialDataMappers.cs` lines 29-47:
```csharp
if (common.IsBattleplay is { } isBattleplay)
{
    response.IsBattleplay = isBattleplay;
}

if (common.ReleaseBattleStageFlg is not null)
{
    response.ReleaseBattleStageFlg = common.ReleaseBattleStageFlg;
}

if (common.BattleBondsLvCap is { } battleBondsLvCap)
{
    response.BattleBondsLvCap = battleBondsLvCap;
}
```

**Generated presence helpers** from `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 423-461:
```csharp
[ProtoMember(14, Name = @"is_battleplay")]
public bool IsBattleplay { get; set; }
public bool ShouldSerializeIsBattleplay() => __pbn__IsBattleplay != null;

[ProtoMember(15, Name = @"release_battle_stage_flg")]
public byte[] ReleaseBattleStageFlg { get; set; }
public bool ShouldSerializeReleaseBattleStageFlg() => __pbn__ReleaseBattleStageFlg != null;

[ProtoMember(17, Name = @"battle_bonds_lv_cap")]
public uint BattleBondsLvCap { get; set; }
public bool ShouldSerializeBattleBondsLvCap() => __pbn__BattleBondsLvCap != null;
```

**Apply to Phase 4:** Existing default omission test is already the correct baseline. Only add positive tests in Phase 4 if the evidence/design artifact proves a specific field should be set; otherwise record those tests as Phase 5 requirements.

---

### Optional `Tests/Blue/BluePlayResultMapperTests.cs` Extension (test, transform)

**Analogs:** existing `Tests/Blue/BluePlayResultMapperTests.cs`, `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`

**Current no-inference guard** from `BluePlayResultMapperTests.cs` lines 60-84:
```csharp
[Fact]
public void Map_BluePlayResult_DoesNotInferRuntimeSemanticsFromUnimplementedOptionalSections()
{
    var request = CreateRequest();
    request.AryReleaseBattledata = new PlayResultRequest.ReleaseBattleData
    {
        AssignNextStageId = 2
    };
    request.AryStageInfoes.Add(CreateStage(101, 1, 0, includeBattle: true));

    var common = PlayResultMappers.Map(request);

    Assert.Equal(1u, common.Baid);
    Assert.Single(common.AryStageInfoes);
    Assert.Equal(101u, common.AryStageInfoes[0].SongNo);
}
```

**Mapper shape** from `PlayResultMappers.cs` lines 8-56 and 64-94:
```csharp
public static CommonPlayResultData Map(PlayResultRequest request)
{
    return new CommonPlayResultData
    {
        Baid = request.Baid,
        AryStageInfoes = request.AryStageInfoes.Select(MapStage).ToList(),
        ReleaseSongNoes = (request.ReleaseSongNoes ?? []).ToList(),
        GetToneNoes = (request.GetToneNoes ?? []).ToList(),
        WaiwaiTutorialFlg = request.ShouldSerializeWaiwaiTutorialFlg() ? request.WaiwaiTutorialFlg : null
    };
}

private static CommonPlayResultData.StageData MapStage(PlayResultRequest.StageData stage)
{
    return new CommonPlayResultData.StageData
    {
        SongNo = stage.SongNo,
        Level = stage.Level,
        StageMode = stage.StageMode,
        SelectedFolderId = stage.SelectedFolderId
    };
}
```

**Apply to Phase 4:** Preserve this guard while writing the design. Do not extend `CommonPlayResultData` or `PlayResultMappers` for battle persistence in Phase 4 unless the user explicitly changes the scope.

---

### Read-Only Blue Battle Protocol References

These are not Phase 4 write targets, but the planner should cite them in evidence/design artifacts.

**Route stubs and implemented routes:**
```csharp
// BattleUserDataController.cs lines 3-12
[Route("/v10r03/chassis/battleuserdata.php")]
public class BattleUserDataController : BaseProtocolController<BattleUserDataController>
{
    [HttpPost]
    public IActionResult BattleUserData([FromBody] BattleUserDataRequest request)
    {
        Logger.LogInformation("Blue BattleUserData request: {Request}", request.Stringify());
        return Ok(new BattleUserDataResponse { Result = 1 });
    }
}
```

```csharp
// InitialDataCheckController.cs lines 9-13
Logger.LogInformation("Blue InitialDataCheck request: {Request}", request.Stringify());
var common = await Mediator.Send(new GetInitialDataQuery(GameEra.Blue), HttpContext.RequestAborted);
return Ok(InitialDataMappers.Map(common));
```

```csharp
// PlayResultController.cs lines 11-18
Logger.LogInformation("Blue PlayResult request: {Request}", request.Stringify());
var common = PlayResultMappers.Map(request);
var result = await Mediator.Send(
    new UpdatePlayResultCommand(request.Baid, GameEra.Blue, common),
    HttpContext.RequestAborted);
return Ok(PlayResultMappers.Map(result));
```

**Proto battle surfaces** from `proto/blue/taiko.proto`:
```protobuf
// lines 111-114
optional bool is_battleplay = 14;
optional bytes release_battle_stage_flg = 15;
optional bytes release_battle_special_flg = 16;
optional uint32 battle_bonds_lv_cap = 17;

// lines 365-388
optional BattleStageData ary_battlestagedata = 27;
message BattleStageData {
    required uint32 support_lv = 1;
    required uint32 battle_stage_id = 2;
    required BattleNpcData npc_data = 3;
    required uint32 kill_cnt = 4;
    required uint32 boss_life = 5;
}

// lines 456-471
optional ReleaseBattleData ary_release_battledata = 47;
message ReleaseBattleData {
    repeated uint32 release_info_id = 1;
    repeated uint32 release_battle_stage_id = 2;
    repeated uint32 release_npc_id = 3;
    repeated BattleTokenData ary_battletokendata = 6;
    required uint32 assign_next_stage_id = 7;
}
```

**Battle userdata generated shape** from `Adapters.GameProtocol.Blue/Wire/Game.cs` lines 3345-3460:
```csharp
public partial class BattleUserDataResponse : IExtensible
{
    [ProtoMember(2, Name = @"release_info_flg")]
    public byte[] ReleaseInfoFlg { get; set; }
    public bool ShouldSerializeReleaseInfoFlg() => __pbn__ReleaseInfoFlg != null;

    [ProtoMember(7, Name = @"npc_data")]
    public List<BattleUserNpcData> NpcDatas { get; } = new();

    [ProtoMember(8, Name = @"ary_token_data")]
    public List<BattleUserTokenData> AryTokenDatas { get; } = new();

    [ProtoMember(9, Name = @"assign_stage_id")]
    public uint AssignStageId { get; set; }
    public bool ShouldSerializeAssignStageId() => __pbn__AssignStageId != null;
}
```

## Shared Patterns

### Evidence Priority

**Source:** `04-CONTEXT.md` lines 18-23

Apply to all Phase 4 artifacts:
- IDA/client behavior is authoritative for wire/runtime mechanics.
- Proto/generated wire/XML are candidate maps until proven.
- Wiki is acceptable for gameplay semantics such as battle score/crown separation.
- Unknown defaults require case-by-case approval, not blanket stubbing.

### Blue-Owned Boundary

**Source:** `04-CONTEXT.md` lines 43-47

Apply to design and gate artifacts:
- Phase 5 implementation must be Blue-owned: Blue entities, handlers, mappers, controllers, migrations, catalog types, tests, and source guards.
- Green AI Battle is contrast material only.
- Phase 4 should not modify runtime Blue battle behavior.

### Normal Playresult Non-Corruption

**Source:** `04-RESEARCH.md` lines 139-143

Apply to the design:
- Current Blue playresult path accepts normal modes and skips unsupported stage modes.
- Current mapper does not map `BattleStageData.SupportLv` or `ReleaseBattleData`.
- Design must keep battle stages out of normal play history, recent/favorites, self-best, crowns, Dani, and normal profile counters unless an approved unlock mirror explicitly says otherwise.

### Validation Checklist

**Source:** `04-VALIDATION.md` lines 37-42 and 55-59

Apply to plans:
- BTEV-01: route/evidence review.
- BTEV-02: proto/wire ownership doc/source guard.
- BTEV-03: local XML audit.
- BTEV-04: default/width proof matrix.
- BTEV-05: normal record separation design review.
- BTEV-06: final implementation gate.

## No Phase 4 Write Targets

The following are explicitly not Phase 4 implementation targets. Planner should mention them only as Phase 5 or later work.

| File/Area | Role | Data Flow | Reason |
|-----------|------|-----------|--------|
| `Infrastructure/GameDataCatalog/Blue/*Battle*Loader.cs` | loader | file-I/O | Context D-08 and research line 157 defer loaders until after the design gate. |
| `Host/wwwroot/data/blue/*battle*.json` or generated battle JSON | config/runtime data | file-I/O | Phase 4 records inventory only; no committed runtime JSON. |
| `Infrastructure/Persistence/Migrations/*Battle*.cs` | migration | CRUD | Battle persistence starts in Phase 5 only. |
| `Domain/Entities/*Battle*.cs` | model | CRUD | Phase 5 Blue-owned persistence target, not Phase 4. |
| `Application/Handlers/*Battle*.cs` or battle edits to `UpdatePlayResultCommand.Blue.cs` | service | request-response, CRUD | Runtime battle behavior must wait for approved evidence/design. |
| `Adapters.GameProtocol.Blue/Controllers/BattleUserDataController.cs` runtime replacement | controller | request-response | Current success-shaped stub is a read-only reference for Phase 4. Replace only in Phase 5 after safe defaults are proven. |

## Metadata

**Analog search scope:** `.planning/phases`, `.planning`, `docs/superpowers/specs`, `docs/green-client-evidence`, `proto/blue`, `Adapters.GameProtocol.Blue`, `Application/Dtos`, `Application/Handlers`, `Tests/Blue`
**Files scanned:** phase docs, roadmap, requirements, Blue proto/wire/controller/mapper/handler/test references, prior evidence docs, prior source guard tests
**Pattern extraction date:** 2026-05-30
