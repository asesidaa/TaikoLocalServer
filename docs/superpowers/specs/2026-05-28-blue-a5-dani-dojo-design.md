# Blue A5 Dani Dojo Spec

**Date:** 2026-05-28
**Status:** approved design; ready for implementation plan after written-spec
review
**Scope:** Add Blue-owned Dani Dojo catalog responses, Dan progression
persistence, Dan score readback, save-data Dan flags, and limited
AdminApi/WebUI Dani readback. This stage does not implement broader Blue WebUI
parity, item-shop purchase state, battle mode, Tokkun, or Banacoin behavior.

## Purpose

Stage A5 makes Blue Dani Dojo work at the same product level currently
expected for Green, while keeping Blue runtime state separate from Green.

The target flow is:

1. A Blue cabinet asks `taikojuku.php` for playable Dan packs.
2. A registered Blue card plays Dani Dojo.
3. `playresult.php` saves normal A4 play rows and also persists Blue Dan
   summary/stage rows.
4. BAID and userdata readback expose safe Blue Dan flags and display state.
5. AdminApi and `DaniDojo.razor` can show Blue Dan progress and catalog data
   through the existing era route.

Blue and Green are intentionally close for Dani. The protocol messages,
catalog entry shape, BAID Dan flags, and userdata `disp_taikojuku_dan` field
match the same AC15-era pattern. A5 should mirror Green's behavior where the
shape is identical, but Blue must own its tables, helpers, constants, mappers,
and tests.

## Evidence Inputs

- Roadmap: `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`
- A2 catalog/data layout:
  `docs/superpowers/specs/2026-05-28-blue-a2-catalog-data-layout-design.md`
- A3 identity/profile/userdata:
  `docs/superpowers/specs/2026-05-28-blue-a3-identity-profile-userdata-design.md`
- A4 playresult/score/crown/reward:
  `docs/superpowers/specs/2026-05-28-blue-a4-enso-play-result-score-crowns-rewards-design.md`
- Green Dani design:
  `docs/superpowers/specs/2026-05-15-green-dani-flow-design.md`
- Green Dani implementation plan:
  `docs/superpowers/plans/2026-05-15-green-dani-flow/`
- Blue wire shape: `proto/blue/taiko.proto`
- Green wire shape: `proto/green/green.proto`
- Blue catalog types:
  - `Application/Catalog/Blue/BlueTaikojukuEntry.cs`
  - `Infrastructure/GameDataCatalog/Blue/BlueTaikojukuLoader.cs`
- Green implementation references:
  - `Application/Handlers/UpdatePlayResultCommand.Green.cs`
  - `Application/Handlers/GetDanScoreQuery.Green.cs`
  - `Application/Handlers/GetTaikojukuQuery.Green.cs`
  - `Application/Common/GreenDanHelpers.cs`
  - `Adapters.GameProtocol.Green/Mappers/TaikojukuMappers.cs`
  - `Adapters.AdminApi/Controllers/DanBestDataController.cs`
  - `Adapters.AdminApi/Controllers/GameDataController.cs`
  - `TaikoWebUI/Pages/DaniDojo.razor`

## Decisions

| Topic | Decision |
|---|---|
| Delivery shape | Mirror Green's Dani behavior, but implement Blue-owned state and helpers. |
| Persistence | Add Blue-owned `DanScoreDatumBlue` and `DanStageScoreDatumBlue`; do not reuse Green Dan tables. |
| Clear grade | Add `BlueDanClearGrade` with the same practical values as Green: not clear, normal clear, gold clear. |
| Dan id | Persist playable `ChallengeLevel` as `DanId`, not catalog `UniqueId`. |
| Normal and extra Dans | Support normal Dan ids `1..25` and extra Dan ids `101..128` initially, matching the proven Green id split. |
| Taikojuku response | Replace the Blue `taikojuku.php` success stub with Blue catalog-backed pack responses. |
| Playresult | A4 normal stage saves remain; A5 adds Blue Dan upsert when the playresult is a valid Dani play. |
| Selfbest | Keep Blue normal selfbest behavior unchanged. Dani songs already save to normal A4 play/best rows unless later evidence proves Blue differs. |
| BAID/userdata | Return Blue Dan flags and safe display Dan from `UserSaveDataBlue`. |
| AdminApi/WebUI | Add only the Dani-specific readback needed by `DanBestDataController`, `GameDataController`, and `DaniDojo.razor`. Broader Blue WebUI parity remains A7. |
| Battle/Tokkun/Banacoin | Keep out of scope. Blue battle and payment-looking fields do not affect Dani in A5. |

## Architecture And Boundaries

A5 adds Blue Dani beside the existing A3/A4 Blue save and playresult work:

```text
Domain/
  Enums/
    BlueDanClearGrade.cs
  Entities/
    DanScoreDatumBlue.cs
    DanStageScoreDatumBlue.cs

Application/
  Abstractions/
    ITaikoDbContext.Blue.cs
  Common/
    BlueDanHelpers.cs
  Handlers/
    UpdatePlayResultCommand.Blue.cs
    GetDanScoreQuery.Blue.cs
    GetTaikojukuQuery.Blue.cs
    UserDataQuery.Blue.cs
    BaidQuery.Blue.cs

Infrastructure/
  Persistence/
    TaikoDbContext.Blue.cs
    Migrations/<EF-generated AddBlueDaniSupport migration>

Adapters.GameProtocol.Blue/
  Controllers/
    TaikojukuController.cs
  Mappers/
    TaikojukuMappers.cs

Adapters.AdminApi/
  Controllers/
    DanBestDataController.cs
    GameDataController.cs
```

The implementation may extract neutral AC15 helpers only when the helper takes
explicit byte counts, neutral DTO values, or catalog-neutral projections. Blue
A5 must not depend on:

- Green generated wire types;
- `GreenProtocolBytes`;
- `GreenDanHelpers`;
- `DanScoreDatumGreen` or `DanStageScoreDatumGreen`;
- `UserSaveDataGreen`;
- Green ghost, AI battle, or item-shop state.

If a helper is easier to reason about as a Blue copy first, copy it. A later
refactor can consolidate proven AC15 behavior after Blue is working.

## Data Model

Add `BlueDanClearGrade`:

- `0 = NotClear`
- `1 = NormalClear`
- `2 = GoldClear`

Add `DanScoreDatumBlue`:

- `Baid`
- `DanId`: Blue playable slot/id from `ChallengeLevel`
- `IsExtra`: true for extra Dan ids
- `MedleyUniqueId`: optional trace back to catalog `UniqueId`
- `ClearGrade`
- `ArrivalSongCount`
- `SoulGaugeTotal`
- `ComboCountTotal`
- child collection of Blue Dan stage score rows

Use `(Baid, DanId, IsExtra)` as the key.

Add `DanStageScoreDatumBlue`:

- `Baid`
- `DanId`
- `IsExtra`
- `StageIndex`
- `SongNumber`
- `HighScore`
- `PlayScore`
- `GoodCount`
- `OkCount`
- `BadCount`
- `DrumrollCount`
- `TotalHitCount`
- `ComboCount`

Use `(Baid, DanId, IsExtra, StageIndex)` as the key. Stage index is part of the
key so duplicate songs in one medley remain separate rows.

`UserSaveDataBlue` already has the summary fields A5 needs:

- `DispDanType`
- `GotDanMax`
- `GotDanFlg`
- `GotDanExtraFlg`
- `DispTaikojukuDan`

A5 should populate those fields from Blue Dan rows and fixed-width Blue
constants.

## Blue Dan Helpers

`BlueDanHelpers` should mirror Green's proven helper responsibilities using
Blue constants:

- classify normal Dan ids as `1..25`;
- classify extra Dan ids as `101..128`;
- reject unknown ids outside those ranges;
- clamp clear grades above gold to gold on output only;
- pack and read 2-bit clear grades;
- compute `GotDanMax` from cleared normal Dan rows;
- compute the next uncleared normal Dan;
- normalize `DispTaikojukuDan` to a safe value in `1..25`;
- advance display Dan after a cleared normal Dan;
- never use an extra Dan id as the userdata display Dan.

The packed Dan flag behavior should match Green unless new Blue cabinet
evidence contradicts it:

- normal Dan slot `1` maps to packed index `0`;
- normal Dan slot `25` maps to packed index `24`;
- extra Dan slot `101` maps to packed index `0`;
- extra Dan slot `128` maps to packed index `27`;
- normal clear and gold clear serialize as distinct nonzero packed states;
- out-of-range persisted values are sanitized on output.

## Protocol Behavior

### `taikojuku.php`

The Blue controller should stop returning only `TaikojukuResponse { Result =
1 }`.

The controller should:

1. Log the Blue request.
2. Send a Blue-aware taikojuku query using requested `get_dan` values.
3. Map Blue catalog packs to Blue generated wire response types.

The query should select packs by `ChallengeLevel`, not by `UniqueId`. If all
requested ids are invalid or missing, return a deterministic fallback set of
normal Dan packs, capped the same way Green is capped today. The mapper should
drop unsafe pack ids, invalid song ids, invalid levels, and excess songs.

Blue may use the same common response DTO as Green, but the dispatch must be
era-aware. Acceptable designs are:

- extend `GetTaikojukuQuery` with `GameEra Era` and add Green/Blue partials; or
- add a Blue-specific query if that is less disruptive.

The implementation plan should choose the smaller change after checking call
sites.

### `playresult.php`

A4 already maps Blue direct `PlayResultRequest` bodies and saves normal stage
rows. A5 should extend `UpdatePlayResultCommand.Blue.cs` after normal stage
processing to save Blue Dan data when:

- `PlayMode == DanMode`; and
- exactly one distinct nonzero `PlayDan` value exists across stage rows; and
- the Dan id exists in the Blue taikojuku catalog by `ChallengeLevel`; and
- the Dan id is in the known normal or extra Blue ranges; and
- `DanResult <= 2`.

For a valid Dani play, A5 should upsert the parent Blue Dan score and stage
rows:

- `ClearGrade = max(existing.ClearGrade, DanResult)`;
- `ArrivalSongCount = max(existing.ArrivalSongCount, stage count)`;
- `ComboCountTotal = max(existing.ComboCountTotal, ComboCntTotal)`;
- `SoulGaugeTotal = max(existing.SoulGaugeTotal, final stage soul gauge)`;
- per stage, preserve best positive counters with max semantics;
- per stage, preserve `BadCount` with min semantics after initializing the row
  from the first observed attempt.

Stage/catalog song mismatches should be logged, not used to reject the Dan
save. Existing Green evidence accepts saving known Dan ids even when song
matching is imperfect, and A5 has no stronger Blue evidence yet.

A5 should not stop A4 from saving normal Blue play/best rows. Unlike Green,
there is no current Blue-specific evidence requiring Dan-mode stages to be
excluded from normal Blue selfbest. If cabinet evidence later proves a
difference, handle that in a focused follow-up.

### BAID And Userdata

`BaidQuery.Blue.cs` should return Dan fields from Blue save state:

- fixed-length `GotDanFlg`;
- fixed-length `GotDanExtraFlg`;
- clamped `GotDanMax`;
- normalized `DispDanType` if needed.

`UserDataQuery.Blue.cs` should compute a safe `DispTaikojukuDan` from Blue Dan
rows, not just return sentinel `1`.

The display rule should mirror Green:

- if saved display Dan is unset, invalid, or already cleared, use the first
  uncleared normal Dan;
- if all normal Dans are cleared, use `25`;
- after clearing the currently displayed normal Dan, advance to the next normal
  Dan;
- after failing a Dan, keep the current display Dan;
- after clearing a non-displayed Dan, leave the current display Dan unless it is
  invalid, unset, or already cleared;
- never emit `0` or an extra Dan id on the wire.

### Dan Score Readback

Extend `GetDanScoreQuery` dispatch to support `GameEra.Blue`. The Blue handler
should:

1. Filter requested ids against Blue taikojuku `ChallengeLevel` values.
2. Read `DanScoreDataBlue` rows for the BAID and valid requested Dan ids.
3. Include child stage rows ordered by `StageIndex`.
4. Return common Dan score rows with `DanId`, arrival count, soul gauge total,
   combo count total, and stage counters.

If no requested ids are valid or no rows exist, return `Result = 1` with an
empty score list.

## AdminApi And WebUI

A5 should expose only the Dani surfaces needed to inspect Blue Dan progress:

- `DanBestDataController` should support `GameEra.Blue` by reading
  `DanScoreDataBlue`.
- `GameDataController` should support Blue `DanData` by projecting
  `IBlueCatalog.TaikojukuFileOrder`.
- `DaniDojo.razor` should work for Blue through the existing era route and
  shared contracts.

The WebUI work should not add general Blue profile/settings editing, Blue
score-history pages, Blue item-shop management, Blue customization editing, or
general era-selector polish. Those belong to A7 unless a tiny route fix is
required to make the Dani page load.

## Error Handling

A5 should keep the cabinet response permissive:

- BAID `0` still returns success without writes.
- Unknown shared user still returns success with no writes.
- Invalid `DanResult > 2` logs and skips the Dan-specific update.
- Conflicting nonzero `PlayDan` values log and skip the Dan-specific update.
- Unknown Dan ids log and skip the Dan-specific update.
- Stage/catalog song mismatch logs but does not block a known Dan save.
- Out-of-range packed flag data is sanitized on BAID/userdata output.

When the Dan-specific update is skipped, normal A4 playresult behavior should
continue wherever it can safely continue.

## Testing And Verification

The implementation plan should use focused, test-first checks:

- Blue taikojuku tests proving:
  - requested `ChallengeLevel` packs are returned;
  - catalog `UniqueId` values are not treated as Dan slots;
  - invalid request sets fall back deterministically;
  - invalid pack slots, invalid songs, invalid levels, and excess songs are
    dropped.
- Blue Dan helper tests proving:
  - normal and extra Dan id classification;
  - 2-bit flag packing for normal and extra Dans;
  - `GotDanMax` calculation;
  - safe display-Dan normalization and advancement.
- Blue playresult handler tests proving:
  - valid Dani play saves Blue Dan parent and stage rows;
  - normal A4 stage rows still save;
  - normal clear and gold clear update flags and `GotDanMax`;
  - failed Dan saves stats but does not mark clear;
  - extra Dans save and do not affect `GotDanMax`;
  - duplicate songs keep separate `StageIndex` rows;
  - invalid `DanResult`, conflicting `PlayDan`, and unknown Dan id skip only
    the Dan update.
- Blue BAID/userdata tests proving:
  - fixed-width Blue Dan flags are returned;
  - `disp_taikojuku_dan` never emits `0` or an extra id;
  - next-uncleared display behavior mirrors Green.
- Blue Dan score tests proving saved rows are returned by requested
  `ChallengeLevel`.
- AdminApi/WebUI tests proving:
  - `api/Blue/DanBestData/{baid}` reads Blue rows;
  - `api/Blue/GameData/DanData` returns Blue catalog data;
  - the WebUI Dani page can request Blue data without falling back to Green.
- Source guard tests proving Blue A5 code does not reference:
  - `GreenDanHelpers`;
  - `GreenProtocolBytes`;
  - `DanScoreDatumGreen`;
  - `DanStageScoreDatumGreen`;
  - `UserSaveDataGreen`;
  - Green generated wire types.

Verification commands:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenTaikojukuTests
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenPlayResultHandlerTests
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenAdminApiControllerTests
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~WebUi
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a5"
```

If normal Host output is locked, use the temp output build path above.

## Acceptance Criteria

- Blue `taikojuku.php` returns catalog-backed Blue Dan packs.
- Blue Dan progression persists in Blue-owned parent and stage tables.
- Blue playresult continues saving normal A4 stage/best rows during Dani play.
- Blue BAID readback returns Blue Dan flags, max, and display type from
  `UserSaveDataBlue`.
- Blue userdata emits a safe `disp_taikojuku_dan` in `1..25`.
- Blue Dan score readback returns saved Blue Dan rows by requested
  `ChallengeLevel`.
- Blue AdminApi Dani data and catalog endpoints work for `GameEra.Blue`.
- `DaniDojo.razor` can display Blue Dan progress without reading Green state.
- Invalid Dani payload details skip only the Dan-specific update.
- Blue A5 code does not depend on Green Dan entities, Green Dan helpers, Green
  protocol constants, or Green generated wire types.

## Out Of Scope

- Full Blue AdminApi/WebUI parity outside Dani readback.
- Blue profile/settings editing in WebUI.
- Blue item-shop purchase state, reward execution, and active-season medal
  state.
- Blue battle mode, `BattleStageData`, `BattleUserData`, release-battle flags,
  battle tokens, NPC state, and battle progression.
- Tokkun behavior.
- Banacoin balance, payment, info, or error behavior.
- Changing Green Dani semantics except for call-site updates needed by a shared
  era-aware query.
- Runtime scraping of external pages or wiki data.

## Handoff To Later Stages

A6 should own Blue item-shop purchase behavior, reward execution if cabinet
evidence shows it is needed, active-season medal state, and shop item locking.
A7 should expose broader Blue score, favorite, recent, customization, shop,
profile, and settings state through AdminApi and WebUI. Track B should design
Blue battle mode separately after Track A normal support has cabinet-level
evidence.
