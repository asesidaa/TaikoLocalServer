---
phase: 41-momoiro-identity-userdata-self-best-and-crown-readback
plan: "04"
subsystem: adapters.game-protocol
tags: [momoiro, ac15, controllers, mapperly, userdata, selfbest, crown-readback]

requires:
  - phase: 41-momoiro-identity-userdata-self-best-and-crown-readback
    provides: Momoiro Application BAID, MyDon, userdata, self-best, release, and crown readback responses from plan 41-03
provides:
  - Mediator-backed Momoiro BAID and MyDon protocol routes
  - Mediator-backed Momoiro userdata route with release hash compaction and userdata-owned crown bytes
  - Mediator-backed Momoiro selfbest route over Momoiro-owned normal, ura, and shin rows
  - Mapperly-generated Momoiro BAID, userdata, and selfbest response projections
affects: [phase-41, 41-05, momoiro-readback, mapperly-generated-source]

tech-stack:
  added: []
  patterns:
    - Thin Momoiro protocol controllers that deserialize, call Mediator/catalog abstractions, and map responses
    - Mapperly section mappers with explicit source ignores for unsupported fields
    - Generated-source inspection for Momoiro adapter response mappers

key-files:
  created:
    - Adapters.GameProtocol.Momoiro/Mappers/BaidResponseMapper.cs
    - Adapters.GameProtocol.Momoiro/Mappers/UserDataMappers.cs
    - Adapters.GameProtocol.Momoiro/Mappers/SelfBestMappers.cs
  modified:
    - Adapters.GameProtocol.Momoiro/Controllers/BaidController.cs
    - Adapters.GameProtocol.Momoiro/Controllers/MyDonEntryController.cs
    - Adapters.GameProtocol.Momoiro/Controllers/UserDataController.cs
    - Adapters.GameProtocol.Momoiro/Controllers/SelfBestController.cs

key-decisions:
  - "Momoiro protocol controllers now route BAID, MyDon, userdata, and selfbest through existing Application handlers instead of scaffold responses."
  - "Momoiro userdata maps `Ac15UserDataResponse.HashCrownFlg` to wire `UserDataResponse.HashCrownFlg` only when Application supplies the bytes."
  - "Momoiro userdata release flags are compacted through `gameDataService.Momoiro().SongHashTable` in the adapter controller."
  - "Unsupported Momoiro challenge, friend, auto-title, and proto-only route fields remain unassigned."

patterns-established:
  - "Mapperly mappers own mechanical common-to-wire projection; controllers only assemble the response and apply catalog compaction."
  - "Generated Mapperly files under `obj/.../generated/Riok.Mapperly/...` are inspection evidence, not committed source."

requirements-completed: [MORDB-01, MORDB-02, MORDB-03, MORDB-04, MORDB-05]

duration: 18min
completed: 2026-06-26
status: complete
---

# Phase 41 Plan 04: Momoiro Readback Controllers and Mapperly Mappers Summary

**Stateful Momoiro BAID, MyDon, userdata, and selfbest protocol routes backed by Application handlers and Mapperly-generated response projection**

## Performance

- **Duration:** 18 min
- **Started:** 2026-06-26T09:02:34Z
- **Completed:** 2026-06-26T09:19:57Z
- **Tasks:** 3/3
- **Files modified:** 7 implementation files plus this summary

## Accomplishments

- Replaced Momoiro BAID, MyDon, userdata, and selfbest route scaffolds with async Mediator-backed protocol controllers.
- Added Momoiro Mapperly mappers for BAID sections, userdata sections plus crown bytes, and selfbest score arrays.
- Preserved direct protobuf transport and the existing controller shape.
- Kept unsupported `crownsdata.php`, `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, and `mainichisong.php` routes absent.
- Left `proto/` and the pre-existing dirty `Host/.gitignore` untouched.

## Task Commits

1. **Task 1: Wire BAID and MyDon controllers** - `5f15fe62` (`feat`)
2. **Task 2: Wire userdata controller and mapper** - `63da7530` (`feat`)
3. **Task 3: Wire self-best controller, mapper, and Mapperly build** - `d8f1d386` (`feat`)

## Files Created/Modified

- `Adapters.GameProtocol.Momoiro/Mappers/BaidResponseMapper.cs` - Mapperly section mapper for identity, profile, costume flags, Dan-default readback fields, and reward pattern.
- `Adapters.GameProtocol.Momoiro/Mappers/UserDataMappers.cs` - Mapperly userdata section mapper including release flags, favorite/recent arrays, counters, display settings, rewards, and `HashCrownFlg`.
- `Adapters.GameProtocol.Momoiro/Mappers/SelfBestMappers.cs` - Mapperly selfbest mapper for normal and shin score arrays, ignoring absent score-rate wire fields.
- `Adapters.GameProtocol.Momoiro/Controllers/BaidController.cs` - Mediator-backed `baidcheck.php`.
- `Adapters.GameProtocol.Momoiro/Controllers/MyDonEntryController.cs` - Mediator-backed `mydonentry.php`.
- `Adapters.GameProtocol.Momoiro/Controllers/UserDataController.cs` - Mediator-backed `userdata.php` with Momoiro catalog release compaction and crown byte mapping.
- `Adapters.GameProtocol.Momoiro/Controllers/SelfBestController.cs` - Mediator-backed `selfbest.php`.

## Verification

| Command | Result |
|---------|--------|
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroControllerReadbackTests|FullyQualifiedName~MomoiroReadbackHandlerTests" --no-restore -- RunConfiguration.DisableParallelization=true` after Task 1 | Expected partial GREEN: exit 1, 6 passed and 2 failed. Remaining failures were the later-plan userdata and selfbest controller assertions. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroControllerReadbackTests|FullyQualifiedName~MomoiroCrownReadbackTests|FullyQualifiedName~MomoiroRouteSurfaceTests" --no-restore -- RunConfiguration.DisableParallelization=true` after Task 2 | Expected partial GREEN: exit 1, 10 passed and 1 failed. Remaining failure was the Task 3 selfbest controller assertion. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UserDataController_ReturnsMomoiroListsSongHashReleaseAndCrownBytes|FullyQualifiedName~MomoiroCrownReadbackTests|FullyQualifiedName~MomoiroRouteSurfaceTests" --no-restore -- RunConfiguration.DisableParallelization=true` after Task 2 | PASS: 8 passed, 0 failed, 0 skipped. |
| Initial Task 3 `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore` | Failed with CS1503 because Momoiro `SelfBestRequest.Level` is nullable. Fixed in Task 3. |
| Final `dotnet build Adapters.GameProtocol.Momoiro/Adapters.GameProtocol.Momoiro.csproj /p:EmitCompilerGeneratedFiles=true --no-restore` | PASS: build succeeded, 0 warnings, 0 errors. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroControllerReadbackTests|FullyQualifiedName~MomoiroReadbackHandlerTests|FullyQualifiedName~MomoiroCrownReadbackTests" --no-restore -- RunConfiguration.DisableParallelization=true` | PASS: 10 passed, 0 failed, 0 skipped. |
| `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroRouteSurfaceTests" --no-restore -- RunConfiguration.DisableParallelization=true` | PASS: 5 passed, 0 failed, 0 skipped. |
| `git status --porcelain -- proto\momoiro` | PASS: no output; proto inputs unchanged. |
| `git diff --cached --name-only -- Host/.gitignore` | PASS: no output; `Host/.gitignore` was not staged. |

## Generated Mapperly Source Inspection

Generated files inspected after the clean emit build:

- `Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/BaidResponseMapper.g.cs`
- `Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/UserDataMappers.g.cs`
- `Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator/SelfBestMappers.g.cs`

Findings:

- `BaidResponseMapper.g.cs` assigns `MydonName`, title/color fields, `AryCostumedata`, `UpdateDatetime`, costume flags, `DispDanType`, `GotDanMax`, `GotDanFlg`, and `RewardPtn`.
- `UserDataMappers.g.cs` assigns `SongHashVer`, `HashReleaseSongFlg`, option/tone/title flags, favorite/recent arrays, recommendation fields, counters, display fields, mode flags, Don Point/reward fields, and `HashCrownFlg = MapRequiredBytes(source.HashCrownFlg)`.
- `UserDataMappers.g.cs` has no assignments for `AryChallengeStats`, `AryUserCompeStats`, `AryBngCompeStats`, `AryFriendInfoes`, or `IsAutoTitleOn`.
- `SelfBestMappers.g.cs` adds mapped rows to `ArySelfbestScores` and `AryShinSelfbestScores`, assigning only `SongNo`, `SelfBestScore`, and `UraBestScore`; `SelfBestScoreRate` and `UraBestScoreRate` are not assigned.

## Decisions Made

- Use existing Application readback responses as the only state source for the four Momoiro routes.
- Copy `HashCrownFlg` only from the top-level `Ac15UserDataResponse`, not from controller-side crown packing.
- Keep generated Mapperly output as ignored build evidence and record findings in this summary instead of committing `obj/` files.
- Normalize missing Momoiro selfbest level input to `0` at the controller boundary because the generated request field is nullable while `GetSelfBestQuery` requires a value.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Normalized nullable Momoiro selfbest level before Mediator dispatch**
- **Found during:** Task 3 (Wire self-best controller, mapper, and Mapperly build)
- **Issue:** The plan copied the adjacent `request.Level` pattern, but current Momoiro generated wire exposes `SelfBestRequest.Level` as `uint?`, causing CS1503 when constructing `GetSelfBestQuery`.
- **Fix:** Used `request.Level.GetValueOrDefault()` in `SelfBestController`.
- **Files modified:** `Adapters.GameProtocol.Momoiro/Controllers/SelfBestController.cs`
- **Verification:** Clean generated-source build passed with 0 warnings/0 errors; focused readback tests passed 10/10.
- **Committed in:** `d8f1d386`

**Total deviations:** 1 auto-fixed Rule 3 blocking issue.
**Impact on plan:** No scope expansion; the fix follows the actual generated Momoiro request shape.

## Issues Encountered

- A parallel final build/test run briefly produced a file-copy warning because both commands used the same output tree. The generated-source build was rerun by itself and passed cleanly with 0 warnings and 0 errors.
- `Host/.gitignore` was already dirty before this plan and remains unstaged and untouched.

## User Setup Required

None - no external service configuration required.

## Known Stubs

None in files created or modified by this plan. The scoped stub scan over touched controllers and mappers found no `TODO`, `FIXME`, `placeholder`, `coming soon`, `not available`, or hardcoded empty placeholder assignments.

## Threat Flags

None. The plan changed only the planned Momoiro protocol controller and Mapperly mapping boundary. It introduced no extra network endpoints beyond already-proven route files, no auth/session path, no schema change, and no filesystem access.

## TDD Gate Compliance

Wave 0 RED tests from plan 41-01 were used as the RED gate. This plan made those controller/readback tests green without adding new tests, consistent with the repo rule against blind test writing.

## Next Phase Readiness

Plan 41-05 can run the final Phase 41 verification and source audit against working Momoiro BAID, MyDon, userdata, selfbest, release, hash, and crown readback routes.

## Self-Check: PASSED

- Created/modified implementation files and this summary exist on disk.
- Task commits found: `5f15fe62`, `63da7530`, and `d8f1d386`.
- No tracked file deletions were introduced by task commits.
- `git status --porcelain -- proto\momoiro` returned no output.
- `git diff --cached --name-only -- Host/.gitignore` returned no output.
- The only unrelated working-tree change remains the pre-existing unstaged `Host/.gitignore`.

---
*Phase: 41-momoiro-identity-userdata-self-best-and-crown-readback*
*Completed: 2026-06-26*
