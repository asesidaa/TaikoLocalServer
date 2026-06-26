---
phase: 42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil
plan: "06"
subsystem: adapter
tags: [momoiro, ac15, playresult, mapperly, controller]

requires:
  - phase: 42-05-momoiro-application-playresult-mutation
    provides: Momoiro Application-layer playresult mutation handler for normal, reward, favorite, recent, challenge, and bounded Dan state
provides:
  - Momoiro PlayResult Mapperly projection from direct protobuf wire request to AC15 common DTOs
  - Mediator-backed Momoiro `playresult.php` controller dispatching `UpdateAc15PlayResultCommand(GameEra.Momoiro)`
  - Public controller test assertions aligned with two-stage request persistence and favorite ordering
affects: [phase-42, momoiro-playresult, momoiro-adapter, mapperly-generated-source]

tech-stack:
  added: []
  patterns:
    - Source-generator-driven Mapperly request mapper with helper conversions for nullable protocol fields
    - Thin protocol controller that logs, maps, sends Mediator command, and maps the response

key-files:
  created:
    - Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs
    - .planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-06-SUMMARY.md
  modified:
    - Adapters.GameProtocol.Momoiro/Controllers/PlayResultController.cs
    - Tests/Momoiro/MomoiroPlayResultControllerTests.cs

key-decisions:
  - "Momoiro playresult request mapping remains Mapperly source-generator driven; nullable protocol fields use explicit helper conversions and emitted generated source was inspected."
  - "Momoiro `playresult.php` now dispatches the existing 42-05 Application mutation path with `GameEra.Momoiro`; no adapter-local persistence or business rules were added."
  - "Controller test expectations now match the two-stage public request: both stages persist play-history rows and both favorite songs append by DisplayOrder."
  - "MORUN requirement checkboxes remain pending because 42-07 owns final Phase 42 verification and closeout."

patterns-established:
  - "Future Momoiro protocol controller work should stay thin: direct protobuf bind, Mapperly map, Mediator send, response map."
  - "Generated Mapperly source is the evidence for Momoiro playresult field preservation, not handwritten mapper-body assertions."

requirements-addressed: [MORUN-01, MORUN-02, MORUN-03, MORUN-04, MORUN-05]
requirements-completed: []
requirements-note: "Controller and mapper wiring is implemented, but MORUN requirements are intentionally not marked complete; 42-07 owns final verification and requirement closeout."

duration: 17min
completed: 2026-06-26
status: complete
---

# Phase 42 Plan 06: Momoiro Playresult Controller and Mapper Summary

**Momoiro direct-protobuf playresult requests now map through Mapperly into the AC15 Application mutation pipeline**

## Performance

- **Duration:** 17 min
- **Started:** 2026-06-26T19:37:00Z
- **Completed:** 2026-06-26T19:54:26Z
- **Tasks:** 3/3
- **Files modified:** 3 implementation/test files plus this summary

## Accomplishments

- Added `PlayResultMappers` for Momoiro wire `PlayResultRequest` to `Ac15PlayResultEnvelope`.
- Preserved release-song, Don Point, reward pattern/progress, Dan result, `play_dan`, and all three challenge-shaped arrays in the generated Mapperly projection.
- Replaced the Momoiro `playresult.php` scaffold with a thin async Mediator-backed controller.
- Verified the public controller path with the existing Momoiro handler and route-surface tests.

## Task Commits

1. **Task 1: Add Momoiro PlayResult Mapperly mapper** - `288827d6` (`feat`)
2. **Task 2: Replace playresult scaffold with Mediator controller** - `05674f7f` (`feat`)
3. **Task 3: Green controller tests and adapter source gates** - `7c900c5d` (`test`)

## Files Created/Modified

- `Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs` - New Mapperly mapper from Momoiro playresult wire DTOs into common AC15 playresult DTOs.
- `Adapters.GameProtocol.Momoiro/Controllers/PlayResultController.cs` - Replaces static scaffold response with log, map, `UpdateAc15PlayResultCommand(GameEra.Momoiro)`, and response map.
- `Tests/Momoiro/MomoiroPlayResultControllerTests.cs` - Aligns the public-route test with both stages in its request persisting and appending favorites.

## Verification

| Command | Result |
|---------|--------|
| `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore` | PASS: 0 warnings, 0 errors. |
| `rg -n "ReleaseSongNoes|GetDonpoint|RewardPtn|RewardProgress|DanResult|PlayDan|ChallengeIds|UserCompeIds|BngCompeIds" Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/PlayResultMappers.g.cs Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs` | PASS: emitted mapper assigns required release, reward, Dan, and challenge facts. |
| `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj --no-restore` | PASS: 0 warnings, 0 errors. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroPlayResultControllerTests|FullyQualifiedName~MomoiroPlayResultHandlerTests|FullyQualifiedName~MomoiroRouteSurfaceTests" --no-restore -- RunConfiguration.DisableParallelization=true` | PASS: 9 passed, 0 failed, 0 skipped. |
| `git status --porcelain -- proto\momoiro` | PASS: no proto changes. |
| `git diff --cached --name-only -- Host/.gitignore` | PASS: no staged `Host/.gitignore`. |
| `git diff --cached --name-only -- .scratch` | PASS: no staged `.scratch/`. |

## Generated-Source Observations

- `PlayResultMappers.g.cs` assigns `ReleaseSongNoes = MapUIntList(request.ReleaseSongNoes)`.
- `PlayResultMappers.g.cs` assigns `GetDonpoint`, `RewardPtn`, and `RewardProgress` from Momoiro request fields.
- `PlayResultMappers.g.cs` maps nullable `DanResult` through `MapNullableUInt(request.DanResult)`.
- `PlayResultMappers.g.cs` assigns `PlayDan`, `ChallengeIds`, `UserCompeIds`, and `BngCompeIds` for each mapped stage.
- `Tokkun`, `BlueBattle`, and `GreenGhost` remain mapped to `null` in the envelope.

## Decisions Made

- Used explicit nullable conversion helpers for Momoiro nullable wire fields instead of relying on Mapperly default mismatch behavior.
- Kept challenge arrays as mapped facts only; the adapter adds no persistence or route behavior for challenge features.
- Left `proto/` and generated `Wire/` files untouched.
- Skipped MORUN requirement completion because the user explicitly assigned final closeout to 42-07.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Mapper Warning] Corrected nullable Momoiro wire field mappings**
- **Found during:** Task 1
- **Issue:** Initial Mapperly build emitted RMG072 warnings for nullable `GetDonpoint`, `DanResult`, and stage `HitCnt` source values using a non-nullable helper.
- **Fix:** Switched those three mappings to the existing nullable-to-default helper.
- **Files modified:** `Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs`
- **Verification:** Adapter generated-source build passed with 0 warnings and 0 errors.
- **Committed in:** `288827d6`

**2. [Rule 1 - Test Contract] Aligned public controller test with its two-stage request**
- **Found during:** Task 3
- **Issue:** The controller test request contains two valid favorite stages, but expected one Momoiro play row and only one appended favorite.
- **Fix:** Updated assertions to expect two play rows and favorite order `[300, 101, 250, 102]`.
- **Files modified:** `Tests/Momoiro/MomoiroPlayResultControllerTests.cs`
- **Verification:** Focused Momoiro controller/handler/route test slice passed.
- **Committed in:** `7c900c5d`

### Workflow-Scope Adjustments

**1. Skipped MORUN requirement completion**
- **Found during:** Plan closeout
- **Issue:** The plan frontmatter lists MORUN-01 through MORUN-05, but the user explicitly instructed not to mark MORUN requirements complete because 42-07 owns final verification.
- **Adjustment:** Summary records requirements as addressed, not complete; `.planning/REQUIREMENTS.md` was left unchanged.
- **Files modified:** None.
- **Verification:** No `.planning/REQUIREMENTS.md` change was staged.

**Total deviations:** 2 auto-fixed issues; 1 workflow-scope adjustment.
**Impact on plan:** Adapter behavior is complete without expanding supported routes/features or overclaiming Phase 42 requirement closeout.

## Issues Encountered

- `ctx7` was not installed, so Mapperly documentation was checked through the official Mapperly website instead.
- The first Task 3 test run exposed stale RED assertions after controller wiring reached the real 42-05 handler path; assertions were updated to match the two-stage public request rather than weakening behavior.

## Authentication Gates

None.

## User Setup Required

None - no external service configuration required.

## Known Stubs

None. The scoped placeholder scan over the files touched in this plan found no placeholder/TODO/FIXME text. Empty arrays in mapper helpers and test payloads are intentional DTO/test defaults, not runtime stubs.

## Threat Flags

None. This plan implemented the planned cabinet-to-adapter and adapter-to-Mediator trust-boundary changes from the threat model and introduced no unplanned endpoint, auth path, filesystem access, schema, proto, generated wire, or unsupported route family.

## Next Phase Readiness

Ready for `42-07`: final Phase 42 verification can now close over Application mutation plus Momoiro adapter mapping/controller behavior. Phase 42 remains in progress and MORUN requirements remain pending until 42-07.

## Self-Check: PASSED

- Summary file exists: `.planning/phases/42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil/42-06-SUMMARY.md`.
- Modified files exist on disk: `Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs`, `Adapters.GameProtocol.Momoiro/Controllers/PlayResultController.cs`, and `Tests/Momoiro/MomoiroPlayResultControllerTests.cs`.
- Task commits found: `288827d6`, `05674f7f`, and `7c900c5d`.
- `git status --porcelain -- proto\momoiro` returned no output.
- `git diff --cached --name-only -- Host/.gitignore` returned no output.
- `.scratch/` remains unstaged/untracked.

---
*Phase: 42-momoiro-normal-playresult-unlocks-rewards-and-dan-compatibil*
*Completed: 2026-06-26*
