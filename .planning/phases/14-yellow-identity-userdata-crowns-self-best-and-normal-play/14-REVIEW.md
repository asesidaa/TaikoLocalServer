---
phase: 14-yellow-identity-userdata-crowns-self-best-and-normal-play
reviewed: 2026-06-08T08:32:23+08:00
depth: standard
files_reviewed: 41
scope_source: "14-01-SUMMARY.md, 14-02-SUMMARY.md, 14-03-SUMMARY.md, phase commit diff cross-check"
findings:
  critical: 0
  warning: 2
  info: 0
  total: 2
status: findings
reviewer: codex-inline-gsd-code-reviewer-fallback
---

# Phase 14: Code Review Report

## Scope

Reviewed the Phase 14 Yellow identity, userdata, self-best, crowns, normal-play persistence, EF mappings, migration, protocol mappers/controllers, and focused tests at standard depth.

Files reviewed:

- `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs`
- `Adapters.GameProtocol.Yellow/Mappers/BaidResponseMapper.cs`
- `Adapters.GameProtocol.Yellow/Mappers/CrownsDataMappers.cs`
- `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs`
- `Adapters.GameProtocol.Yellow/Mappers/SelfBestMappers.cs`
- `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs`
- `Application/Abstractions/ITaikoDbContext.Yellow.cs`
- `Application/Ac15/YellowAc15NormalPlayAdapter.cs`
- `Application/Ac15/YellowAc15UserDataAdapter.cs`
- `Application/Common/UserSaveDataYellowExtensions.cs`
- `Application/Dtos/CommonUserDataResponse.Yellow.cs`
- `Application/Handlers/AddMyDonEntryCommand.Yellow.cs`
- `Application/Handlers/AddMyDonEntryCommand.cs`
- `Application/Handlers/BaidQuery.Yellow.cs`
- `Application/Handlers/BaidQuery.cs`
- `Application/Handlers/GetSelfBestQuery.Yellow.cs`
- `Application/Handlers/GetSelfBestQuery.cs`
- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs`
- `Application/Handlers/UpdatePlayResultCommand.cs`
- `Application/Handlers/UserDataQuery.Yellow.cs`
- `Application/Handlers/UserDataQuery.cs`
- `Domain/Entities/SongBestDatumYellow.cs`
- `Domain/Entities/SongPlayDatumYellow.cs`
- `Domain/Entities/UserSaveDataYellow.cs`
- `Domain/Entities/YellowFavoriteSongs.cs`
- `Domain/Entities/YellowRecentSongs.cs`
- `Infrastructure/Persistence/Migrations/20260607222143_AddYellowPhase14State.Designer.cs`
- `Infrastructure/Persistence/Migrations/20260607222143_AddYellowPhase14State.cs`
- `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs`
- `Infrastructure/Persistence/TaikoDbContext.Yellow.cs`
- `Infrastructure/Persistence/TaikoDbContext.cs`
- `Tests/Green/GreenAuthConfigTests.cs`
- `Tests/Yellow/YellowCatalogBoundaryTests.cs`
- `Tests/Yellow/YellowCrownsDataTests.cs`
- `Tests/Yellow/YellowHandlerFixture.cs`
- `Tests/Yellow/YellowIdentityHandlerTests.cs`
- `Tests/Yellow/YellowPersistenceBoundaryTests.cs`
- `Tests/Yellow/YellowPlayResultHandlerTests.cs`
- `Tests/Yellow/YellowRouteSkeletonTests.cs`
- `Tests/Yellow/YellowSelfBestTests.cs`
- `Tests/Yellow/YellowUserDataProtocolTests.cs`

## Findings

### WR-01: Yellow userdata drops the persisted `is_explain` flag

**Severity:** Warning

**Location:** `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs:10`

`UpdatePlayResultCommand.Yellow` persists `playResultData.IsExplain` into `UserSaveDataYellow.IsExplain`, and Yellow proto exposes `UserDataResponse.is_explain` as field 2. The userdata mapper never assigns `UserDataResponse.IsExplain`, and the common userdata snapshot/response currently has no Yellow path for this value. As a result, a normal Yellow playresult can store the explanation/tutorial completion flag, but the next `userdata.php` response omits it and the client sees the protobuf default instead of saved state.

This is covered neither by `YellowUserDataProtocolTests.UserData_Yellow_ComposesSaveCatalogFavoritesRecentAndSupportedFlags` nor by `UserDataMapper_Yellow_MapsSupportedFieldsAndOmitsTokkunTutorial`; the mapper test sets `IsDevilYellow` and asserts `IsDevil`, but has no assertion for `IsExplain`.

Relevant evidence:

- `proto/yellow/yellow.proto:264-267` defines `UserDataResponse.is_explain`.
- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs:42-45` saves the uploaded flag.
- `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs:10-45` maps adjacent userdata fields but not `IsExplain`.

### WR-02: Invalid Yellow stages can still mutate profile counters before normal-play validation skips them

**Severity:** Warning

**Location:** `Application/Handlers/UpdatePlayResultCommand.Yellow.cs:40`

The Yellow handler mutates save-level state before delegating to `Ac15NormalPlayService.SaveAsync`: medal totals, tutorial flags, `LastPlayDatetime`, unlock bits, and per-stage profile counters are updated at lines 40-68. The shared normal-play service only validates stages later and skips invalid or unsupported stages at `Ac15NormalPlayService.cs:40-48` before adding play rows, best rows, favorites, or recent songs.

That creates an inconsistent accepted upload path: a payload whose stage has an out-of-range song number or invalid course level can still increment Yellow profile counters and update save metadata, while producing no Yellow play-history row, best row, favorite row, recent row, self-best result, or crown state for that stage. Blue and Green avoid this mismatch by validating or filtering stages before applying profile counters. Phase 14's normal-play readback tests only use valid `CreateStage(...)` data, so this inconsistency is not covered.

Relevant evidence:

- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs:40-68` applies save/profile mutations before shared validation.
- `Application/Ac15/Ac15NormalPlayService.cs:38-48` skips invalid/unsupported stages during persistence.
- `Tests/Yellow/YellowPlayResultHandlerTests.cs:553-575` only constructs valid stage helper data.

## Review Notes

No code fixes were applied in this review stage. `14-VERIFICATION.md` was intentionally left with `code_review: pending` because findings exist.

Lightweight checks performed:

- Phase 14 source scope cross-checked against `git diff --name-only b12a24aa^..HEAD -- . ':!.planning' ':!Host/.gitignore'`.
- `git diff --check -- Application Infrastructure Domain Adapters.GameProtocol.Yellow Tests` returned no whitespace errors.
- Focused source scans found no production Yellow references to Blue battle persistence, deferred Yellow shop/Tokkun storage, AdminApi/WebUI implementation, or TODO/FIXME/HACK markers in Phase 14-owned files.

No full verification or phase implementation commands were run.

---

_Reviewed: 2026-06-08T08:32:23+08:00_
_Reviewer: codex inline gsd-code-reviewer fallback_
_Depth: standard_
