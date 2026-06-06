# Phase 10: Evidence-Backed Tokkun State Persistence and Readback - Research

**Researched:** 2026-06-06
**Domain:** Blue Tokkun protocol-backed persistence, EF Core SQLite, userdata readback
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Treat the user-reported live Tokkun session log showing `play_mode = 3` as proven runtime evidence. Phase 10 should add or plan `PlayMode.Tokkun = 3`; this is no longer a guessed value.
- **D-02:** `play_mode = 3` is the primary Tokkun classifier for Phase 10. `ary_tokkunstage_info` remains payload detail and summary evidence, but the old "numeric Tokkun mode unknown" guidance is superseded for this phase.
- **D-03:** The user also reported additional fields in the Tokkun session log. Planners must inspect the concrete log/proto field list before adding any extra persisted fields beyond named protocol-backed Tokkun facts. Do not store unnamed fields from memory.
- **D-04:** Store Tokkun tutorial/readback state on `UserSaveData_Blue`, alongside existing Blue tutorial/profile flags. Store Tokkun summary/progress as separate Blue-owned history rows.
- **D-05:** Tokkun summary/history is append-only per classified upload. Do not dedupe by `banacoin_datetime` and do not keep latest-only state unless later evidence proves idempotency or replacement behavior.
- **D-06:** Persist `tookun_songno` with raw fidelity: preserve client order and duplicates if the client sends them. The exact representation is implementation discretion, but it must not normalize into unique songs.
- **D-07:** Store client-reported protocol timestamp data only, such as `play_datetime` and `banacoin_datetime` where available. Do not add a separate server `UploadedAtUtc` field in Phase 10.
- **D-08:** Interpret the roadmap "upload time" requirement as client-reported protocol time, not server-observed write time.
- **D-09:** Before a user has a persisted Tokkun tutorial value, Blue userdata should continue to omit optional `tokkun_tutorial_flg`. Do not invent default `0` or `1` readback values.
- **D-10:** Update persisted `tokkun_tutorial_flg` only from Tokkun-classified uploads where the optional tutorial field is present. Tutorial-flag-only non-Tokkun uploads must not update this state.
- **D-11:** Store and return `tokkun_tutorial_flg` as a raw nullable `uint`. Do not normalize to bool and do not clamp to `0` or `1`.
- **D-12:** In Phase 10, protocol readback is limited to `UserDataResponse.tokkun_tutorial_flg`. Tokkun summary/history readback means durable server-side persistence and tests unless Phase 11 proves another client-facing response surface.
- **D-13:** Phase 10 verification should include a focused automated suite: classifier/mapper tests, handler persistence tests, userdata readback tests, schema/migration reload tests, no-cross-write tests, and a Host build.
- **D-14:** The existing Tokkun session evidence is enough to unlock `PlayMode.Tokkun = 3` and result-shape planning. Phase 11 still owns final cabinet/RPCS3 smoke after Phase 10 implementation.
- **D-15:** Do not reintroduce Tokkun word-scan source guards. Safety proof should come from runtime behavior tests that execute Tokkun handling and assert only the allowed Tokkun state changes.

### the agent's Discretion
- Choose the exact history-row representation for `tookun_songno` as long as raw order and duplicates are preserved.
- Choose exact code shape, such as a `UpdatePlayResultCommand.BlueTokkun.cs` partial and/or `BlueTokkunStateExtensions`, as long as Blue-owned layering and existing partial-file patterns are preserved.
- Choose exact test file organization and migration name, provided the required proof in D-13 is covered.

### Deferred Ideas (OUT OF SCOPE)
None. Additional logged Tokkun fields are not a deferred feature; they are an evidence-inspection prerequisite before extending storage beyond named protocol-backed fields.
</user_constraints>

## Summary

Phase 10 is a storage and readback slice built on Phase 9's mapper/handler foundation. The current code already exposes `CommonPlayResultData.TokkunTutorialFlg` and `TokkunStageData`, and `UpdatePlayResultCommand.Blue.cs` already accepts Tokkun before battle/normal writes. The Phase 10 change is to replace that no-op branch with narrow Blue-owned persistence while preserving all forbidden no-write guarantees. [VERIFIED: `.planning/phases/09-tokkun-mapper-and-safe-playresult-acceptance/09-01-SUMMARY.md`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`]

The storage shape should be one nullable raw `uint` on `UserSaveDataBlue` for tutorial readback, plus one append-only Blue Tokkun history table for summary/progress facts. `tookun_songno` should be stored in a structured raw-order representation; a JSON string column serialized with `System.Text.Json` is the lowest-friction option because EF Core SQLite maps strings naturally and JSON preserves order and duplicates without ad hoc delimiter parsing. [VERIFIED: `Domain/Entities/UserSaveDataBlue.cs`, `Infrastructure/Persistence/TaikoDbContext.Blue.cs`, `proto/blue/taiko.proto`]

**Primary recommendation:** Add `PlayMode.Tokkun = 3`, switch the mapper classifier to that enum value, add `BlueTokkunStageResult` history persistence plus nullable `UserSaveDataBlue.TokkunTutorialFlg`, then project only that nullable tutorial value through Blue userdata.

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Tokkun classification | Adapter mapper | Domain enum | `PlayResultMappers.Map` converts Blue wire requests to `CommonPlayResultData`; `PlayMode.Tokkun = 3` makes the proven mode value named. |
| Tutorial persistence | Application handler / EF persistence | Domain entity | `HandleBlue` owns playresult side effects; `UserSaveData_Blue` is the approved readback state location. |
| Summary/history persistence | Application handler / EF persistence | Domain entity | Each classified upload appends a Blue-owned row and does not affect normal, battle, shop, or Banacoin storage. |
| Userdata readback | Application query / adapter mapper | Wire response | `UserDataQuery.Blue.cs` projects persisted state to `CommonUserDataResponse`; `UserDataMappers.Map` controls optional protobuf serialization. |
| Verification | Tests project | Host build | Existing Blue tests use xUnit, real mapper execution, and SQLite-backed handler/context assertions. |

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET / C# | 10 / C# 13 | Runtime and implementation language | Existing solution target and language defaults. [VERIFIED: `.planning/codebase/STACK.md`] |
| EF Core SQLite | 10.0.7 | Blue-owned persistent state and migrations | Existing `TaikoDbContext` and migrations use EF Core SQLite. [VERIFIED: `.planning/codebase/STACK.md`] |
| protobuf-net wire models | 3.2.x | Blue direct-protobuf request/response fields | Existing Blue endpoints and generated `Wire/Game.cs` expose `play_mode`, `tokkun_tutorial_flg`, and `ary_tokkunstage_info`. [VERIFIED: `Adapters.GameProtocol.Blue/Wire/Game.cs`] |
| xUnit | 2.9.3 | Mapper, handler, persistence, and userdata tests | Existing `Tests/Blue` coverage is xUnit-based. [VERIFIED: `.planning/codebase/TESTING.md`] |

### Supporting

| Library | Purpose | When to Use |
|---------|---------|-------------|
| `System.Text.Json` | Serialize `TookunSongnoes` as JSON in a string column | Use in a small helper for the history entity or handler so raw order and duplicates survive without delimiter parsing. |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| JSON string for `TookunSongnoes` | Comma-separated string | CSV is smaller but invites ad hoc parsing and escaping concerns. |
| JSON string for `TookunSongnoes` | Separate child rows with ordinal | More relational and queryable, but more files and migrations for no proven Phase 10 client readback surface. |
| Store `play_datetime` as `DateTime` | Store raw string | Raw string better matches D-07/D-08 client-protocol-time-only wording and avoids server-side reinterpretation. |

**Installation:** No package install is needed.

## Package Legitimacy Audit

No external packages are added. Existing framework/package dependencies remain unchanged.

## Architecture Patterns

### System Architecture Diagram

```text
-----------------------------+
| Blue PlayResultRequest      |
| play_mode=3, optional facts |
+--------------+--------------+
               |
               v
+-----------------------------+
| PlayResultMappers.Map        |
| names PlayMode.Tokkun        |
| preserves raw Tokkun facts   |
+--------------+--------------+
               |
               v
+-----------------------------+
| UpdatePlayResultCommand.Blue |
| guest/unknown success first  |
| Tokkun branch before writes  |
+------+----------------------+
       |
       +--> UserSaveData_Blue.TokkunTutorialFlg (nullable raw uint)
       |
       +--> BlueTokkunStageResults append-only raw summary row
       |
       +--> no normal/battle/Dani/shop/Banacoin writes

+-----------------------------+
| UserDataQuery.Blue           |
| projects nullable tutorial   |
+--------------+--------------+
               |
               v
+-----------------------------+
| UserDataMappers.Map          |
| serializes tokkun_tutorial   |
| only when value is present   |
+-----------------------------+
```

### Recommended Project Structure

```text
Domain/Entities/
  BlueTokkunStageResult.cs        # append-only Blue Tokkun summary/history row
  UserSaveDataBlue.cs             # nullable tutorial readback state
Domain/Enums/
  PlayMode.cs                     # Tokkun = 3
Application/Dtos/
  CommonUserDataResponse.Blue.cs  # nullable tutorial response field
Application/Handlers/
  UpdatePlayResultCommand.Blue.cs or .BlueTokkun.cs
  UserDataQuery.Blue.cs
Application/Abstractions/
  ITaikoDbContext.Blue.cs
Infrastructure/Persistence/
  TaikoDbContext.Blue.cs
  Migrations/*_AddBlueTokkunState.cs
Tests/Blue/
  BluePlayResultMapperTests.cs
  BluePlayResultHandlerTests.cs
  BlueUserDataTests.cs
  BlueMapperTests.cs
  BlueTokkunPersistenceTests.cs
  BlueTokkunPersistenceShapeTests.cs
```

### Pattern 1: Blue-Owned EF State

Use the existing Blue partial DbContext pattern: expose a `DbSet` in `ITaikoDbContext.Blue.cs`, mirror it in `TaikoDbContext.Blue.cs`, configure table name, key, indexes, and `UserData.Baid` cascade relation in `OnModelCreatingBlue`. [VERIFIED: `ITaikoDbContext.Blue.cs`, `TaikoDbContext.Blue.cs`, `BlueBattlePersistenceShapeTests.cs`]

### Pattern 2: Nullable Optional Protocol Fields

Keep absence distinct from zero. Existing generated wire types expose `ShouldSerializeTokkunTutorialFlg()` and response-side optional fields can be omitted by leaving the generated nullable backing state unset. [VERIFIED: `Adapters.GameProtocol.Blue/Wire/Game.cs`, `Tests/Blue/BlueMapperTests.cs`]

### Pattern 3: Behavior Tests Over Source Word Guards

Use real mapper/handler/query execution and SQLite assertions. Phase 10 should add no Tokkun source-word guard; it should assert allowed writes and forbidden writes directly. [VERIFIED: `Tests/Blue/BluePlayResultHandlerTests.cs`, `Tests/Blue/BlueBattlePersistenceTests.cs`]

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Song list persistence | Manual comma splitting/parsing | `System.Text.Json` serialize/deserialize to a string column | Preserves order/duplicates with structured parsing. |
| Protocol optional presence | Bool sentinels or default zero | Nullable `uint?` plus generated `ShouldSerialize...` methods | D-09/D-11 require absence to stay distinct from raw zero. |
| Migration state | Manual SQL-only schema edits | EF Core migration generated through `dotnet ef migrations add` | Keeps snapshot and DbContext in sync. |
| Tokkun safety | Source-text word scans | Behavior tests through mapper/handler/query paths | D-15 requires runtime behavior proof. |

## Common Pitfalls

### Pitfall 1: Continuing To Classify From `ary_tokkunstage_info`
**What goes wrong:** A payload with `play_mode = 3` but no stage summary fails the Tokkun path, or a stage-shaped non-Tokkun payload updates tutorial state.
**How to avoid:** Add `PlayMode.Tokkun = 3`, update mapper tests, and make `play_mode = 3` the classifier while preserving stage data as payload detail.

### Pitfall 2: Defaulting `tokkun_tutorial_flg`
**What goes wrong:** New users receive `tokkun_tutorial_flg = 0` even though absence is the only proven pre-upload state.
**How to avoid:** Store `uint?`, project `uint?`, and map the response only when the value is present.

### Pitfall 3: Treating History As Latest State
**What goes wrong:** Repeated uploads with the same client timestamp replace or dedupe rows without evidence.
**How to avoid:** Append one history row per classified upload with summary data; no uniqueness key on `banacoin_datetime`.

### Pitfall 4: Adding Server Upload Time
**What goes wrong:** Phase 10 creates `UploadedAtUtc` or `CreatedAt`, contradicting D-07/D-08.
**How to avoid:** Store `PlayDatetime` and `BanacoinDatetime` as client protocol strings only.

## Code Examples

No external code examples are needed. The plan should reference existing local analogs:

- `BlueBattleStageResult` for Blue-owned append-only rows.
- `BlueBattlePersistenceTests.SqliteSchema_PersistsAndReloadsRepresentativeBlueBattleState` for schema reload proof.
- `BluePlayResultHandlerTests.UpdatePlayResult_Blue_TokkunExistingUserReturnsSuccessWithoutStateWrites` for no-cross-write assertions.
- `BlueMapperTests.UserDataMapper_Blue_OmitsTokkunTutorialFlag` for optional omission behavior.

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | JSON string storage for `TookunSongnoes` is acceptable for Phase 10 history rows. | Standard Stack / Don't Hand-Roll | If future client readback requires relational queries by song, a later migration may split the list into child rows. |

## Open Questions (RESOLVED)

1. **Should Phase 10 add `PlayMode.Tokkun = 3`?** RESOLVED: Yes, D-01/D-02 lock this from user-reported runtime evidence.
2. **Should summary/history read back through a protocol response?** RESOLVED: No, D-12 limits protocol readback to `UserDataResponse.tokkun_tutorial_flg`.
3. **Should server upload time be stored?** RESOLVED: No, D-07/D-08 require client protocol time only.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | build/test/migration execution | Yes | pinned by `global.json` | none |
| EF Core tooling | migration generation | Expected from existing repo workflow | not probed in research | If unavailable, executor should report and stop before manual migration edits. |
| SQLite in-memory | persistence tests | Yes via test project packages | EF Core SQLite 10.0.7 | none |

**Missing dependencies with no fallback:** None known from local project configuration.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit via `Tests/Tests.csproj` |
| Config file | `Tests/Tests.csproj` |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~BlueMapperTests"` |
| Full suite command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Blue"` |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| TKST-01 | Persist raw nullable tutorial flag from classified Tokkun uploads and read it back through Blue userdata only when present. | mapper + handler + query | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueMapperTests|FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~BluePlayResultHandlerTests"` | Yes, extend existing files |
| TKST-02 | Append raw Tokkun summary/progress facts including `banacoin_datetime`, song count/list, counts, and client upload time. | SQLite handler + schema reload | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueTokkunPersistenceTests"` | Create `BlueTokkunPersistenceTests.cs` |
| TKST-03 | Keep Tokkun state Blue-owned and separate from Green, Nijiiro, normal score, Dani, battle, shop, Banacoin-like state. | persistence shape + no-cross-write | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueTokkunPersistenceShapeTests|FullyQualifiedName~BluePlayResultHandlerTests"` | Create `BlueTokkunPersistenceShapeTests.cs` |
| TKST-04 | Store protocol-backed raw facts only; no inferred ranking, reward, score, payment, practice-time, unlock, or server upload semantics. | source-shape + behavior | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BlueTokkunPersistenceShapeTests|FullyQualifiedName~BluePlayResultHandlerTests"` | Create/extend tests |

### Sampling Rate

- **Per task commit:** Run the focused xUnit class touched by that task.
- **Per wave merge:** Run `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResultMapperTests|FullyQualifiedName~BluePlayResultHandlerTests|FullyQualifiedName~BlueUserDataTests|FullyQualifiedName~BlueMapperTests|FullyQualifiedName~BlueTokkunPersistenceTests|FullyQualifiedName~BlueTokkunPersistenceShapeTests"`.
- **Phase gate:** Run the wave command plus `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase10"`.

### Wave 0 Gaps

- [ ] `Tests/Blue/BlueTokkunPersistenceTests.cs` - schema reload and history persistence.
- [ ] `Tests/Blue/BlueTokkunPersistenceShapeTests.cs` - Blue-owned storage shape and forbidden semantic references.

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | no | Existing cabinet identity flow; no new auth. |
| V3 Session Management | no | No session/cookie work. |
| V4 Access Control | yes | Blue state remains scoped by `Baid` and `UserData.Baid` FK. |
| V5 Input Validation | yes | Treat protobuf fields as untrusted raw facts; avoid semantic side effects. |
| V6 Cryptography | no | No crypto added. |

### Known Threat Patterns for This Phase

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Tokkun upload mutates normal/battle/shop/Banacoin state | Tampering | Tokkun branch before other write paths plus SQLite no-cross-write tests. |
| Client timestamp treated as server audit truth | Repudiation | Store only protocol timestamp strings and document no server audit semantics. |
| `tookun_songno` normalized | Tampering | JSON/raw ordered list tests with duplicates. |
| Optional tutorial absence collapsed to zero | Tampering | Nullable `uint?` storage and `ShouldSerializeTokkunTutorialFlg()` tests. |

## Sources

### Primary (HIGH confidence)
- `.planning/phases/10-evidence-backed-tokkun-state-persistence-and-readback/10-CONTEXT.md` - locked Phase 10 decisions.
- `.planning/REQUIREMENTS.md` - TKST-01 through TKST-04.
- `.planning/ROADMAP.md` - Phase 10 goal and Phase 11 smoke boundary.
- `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` - Tokkun protocol fields.
- `Application/Handlers/UpdatePlayResultCommand.Blue.cs` and `Tests/Blue/BluePlayResultHandlerTests.cs` - Phase 9 behavior and test precedent.
- `Infrastructure/Persistence/TaikoDbContext.Blue.cs` and Blue battle persistence tests - Blue-owned EF persistence precedent.

### Secondary (MEDIUM confidence)
- `.planning/codebase/STACK.md`, `.planning/codebase/ARCHITECTURE.md`, `.planning/codebase/CONVENTIONS.md`, `.planning/codebase/TESTING.md` - codebase map, current as of 2026-05-28.

### Tertiary (LOW confidence)
- None.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - existing repo docs and project files define it.
- Architecture: HIGH - Phase 10 context and Blue code paths directly identify ownership boundaries.
- Pitfalls: HIGH - based on locked decisions and Phase 9 no-write tests.

**Research date:** 2026-06-06
**Valid until:** Stable for this Phase 10 plan; refresh if Blue Tokkun logs add extra named fields before execution.
