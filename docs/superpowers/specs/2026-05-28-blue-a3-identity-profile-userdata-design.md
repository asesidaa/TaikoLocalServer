# Blue A3 Identity, Profile, Initial Data, And User Data Spec

**Date:** 2026-05-28
**Status:** approved design; ready for implementation plan after written-spec review
**Scope:** Add Blue-owned normal profile persistence and real behavior for
`baidcheck.php`, `mydonentry.php`, `initialdatacheck.php`, and `userdata.php`.
This stage assumes A2 catalog work is complete by implementation time.

## Purpose

Stage A3 makes Blue card registration, known-card login, initial data, and
profile readback work against real server state. It replaces the A1 success
stubs for the identity/profile endpoints with mediator-backed handlers while
keeping unclear or later-stage behavior out of scope.

The target is a Blue normal cabinet flow:

1. An unknown access code is reported as a new Blue user.
2. `mydonentry.php` creates the shared identity rows and Blue save row.
3. The same card logs in again and receives stable profile data.
4. `initialdatacheck.php` and `userdata.php` return Blue catalog-backed song,
   customization, and information bitsets without inventing battle, Tokkun, or
   unclear XML behavior.

## Evidence Inputs

- Roadmap: `docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md`
- A0 evidence: `docs/superpowers/specs/2026-05-27-blue-a0-evidence-bootstrap-design.md`
- A1 adapter skeleton:
  `docs/superpowers/specs/2026-05-27-blue-a1-era-foundation-adapter-skeleton-design.md`
- A2 catalog design:
  `docs/superpowers/specs/2026-05-28-blue-a2-catalog-data-layout-design.md`
- Blue wire shape: `proto/blue/taiko.proto`
- Current Blue stubs: `Adapters.GameProtocol.Blue/Controllers/`
- Green implementation references:
  - `Application/Handlers/BaidQuery.Green.cs`
  - `Application/Handlers/AddMyDonEntryCommand.Green.cs`
  - `Application/Handlers/GetInitialDataQuery.Green.cs`
  - `Application/Handlers/UserDataQuery.Green.cs`
  - `Application/Common/UserSaveDataGreenExtensions.cs`
  - `Domain/Entities/UserSaveDataGreen.cs`
  - `Adapters.GameProtocol.Green/Mappers/BaidResponseMapper.cs`
  - `Adapters.GameProtocol.Green/Mappers/InitialDataMappers.cs`
  - `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`

## Decisions

| Topic | Decision |
|---|---|
| Persistence | Add `UserSaveDataBlue` as a Blue-owned save row instead of reusing Green save rows. |
| Shared identity | Continue using shared `Card`, `UserDatum`, and `Credential` for access code, BAID, and MyDon name. |
| Handler shape | Add Blue partial handlers for existing mediator requests: `BaidQuery`, `AddMyDonEntryCommand`, `GetInitialDataQuery`, and `UserDataQuery`. |
| Adapter mapping | Add Blue-owned mappers under `Adapters.GameProtocol.Blue/Mappers`. Do not reference Green generated wire types. |
| Catalog dependency | Use `IBlueCatalog` or `IGameDataCatalog.For(GameEra.Blue)` from A2 for song hash, songs, telop, event folder, taikojuku, item shop, recommend, title, tone, and costume data. |
| AC15 sharing | Extract small helpers only where evidence-backed. Do not build a broad generic AC15 era framework in A3. |
| Bitset widths | Use Blue-owned byte-width constants. Shared bitset helpers may accept `byteCount`, but Blue must not import `GreenProtocolBytes`. |
| Legal terms | Return an empty `ary_legalterms_data`; current evidence suggests this is a legacy/unused-looking field with no matching getter route. |
| Tokkun | Do not implement Tokkun behavior. Omit `tokkun_tutorial_flg` unless later client evidence proves omission is unsafe. |
| Battle | Do not implement battle profile fields. Battle initial-data fields stay absent or explicitly false only where mapper tests prove safe serialization. |
| Unclear XML files | Do not parse `present.xml`, `spacialbaid.xml`, or `waiwaiconfig.xml` in A3. Their uses remain deferred. |
| Favorites/recents | Return valid empty arrays in `userdata.php`. Add Blue favorite/recent persistence in A4 when play-result writes exist. |

## Architecture And Boundaries

A3 adds Blue normal profile behavior without creating a generic AC15 framework.
The implementation should add:

```text
Domain/
  Entities/
    UserSaveDataBlue.cs

Application/
  Abstractions/
    ITaikoDbContext.Blue.cs
  Common/
    BlueProtocolBytes.cs
    UserSaveDataBlueExtensions.cs
    focused AC15 helper files where duplication is already proven
  Dtos/
    CommonBaidResponse.Blue.cs
    CommonInitialDataCheckResponse.Blue.cs
    CommonUserDataResponse.Blue.cs
  Handlers/
    BaidQuery.Blue.cs
    AddMyDonEntryCommand.Blue.cs
    GetInitialDataQuery.Blue.cs
    UserDataQuery.Blue.cs

Infrastructure/
  Persistence/
    TaikoDbContext.Blue.cs
    Migrations/<EF-generated AddBlueIdentityProfileSupport migration>

Adapters.GameProtocol.Blue/
  Mappers/
    BaidResponseMapper.cs
    InitialDataMappers.cs
    UserDataMappers.cs
```

Existing dispatch records should add `GameEra.Blue` cases. Blue controllers for
`baidcheck.php`, `mydonentry.php`, `initialdatacheck.php`, and `userdata.php`
should become asynchronous mediator-backed actions, mirroring the Green
controller pattern but passing `GameEra.Blue`.

Shared identity remains shared:

- `Card` maps access code to BAID.
- `UserDatum` stores MyDon name and shared user identity.
- `Credential` is created for compatibility with existing account assumptions.

Era-owned save state remains separate:

- Nijiiro data stays in `UserSaveData_Nijiiro`.
- Green data stays in `UserSaveData_Green`.
- Blue data lives in new `UserSaveData_Blue`.

A known shared identity without `UserSaveDataBlue` is treated as a new Blue
registration. `mydonentry.php` completes the era registration by adding the Blue
save row without duplicating shared identity rows.

## Targeted AC15 Sharing

The repo already has multiple AC15-era patterns, and AC15 as a family includes
many versions. A3 should avoid needless copy/paste, but it should not introduce
a broad framework before more versions prove the stable abstraction.

Allowed shared helpers:

- card lookup, next-BAID calculation, and shared identity registration mechanics;
- fixed-width bitset encoding/normalization helpers that accept explicit byte
  counts;
- title text matching and titleplate fallback/resolution against an era catalog;
- simple `InformationData` projection helpers where both source and target are
  already structurally identical.

Do not share:

- generated wire mappers across adapters;
- Green ghost, shop-locking, or AI battle logic;
- Green Dani display normalization;
- Blue battle defaults;
- hardcoded byte widths.

Each era owns its constants. If Blue and Green both use 128 song-flag bytes
today, that is an implementation detail expressed as two era constants, not a
reason for Blue to depend on Green.

## Blue Save Data

`UserSaveDataBlue` should cover only normal profile/readback state needed by
BAID and userdata:

- selected `Title` and `TitleplateId`;
- Don colors: `ColorFace`, `ColorBody`, `ColorLimb`;
- current costume slots: `Costume1` through `Costume5`;
- unlock flags: `CostumeFlg1` through `CostumeFlg5`, `ToneFlg`, `TitleFlg`;
- options: `OptionFlg`, `DefaultOptionSetting`, `DefaultShinSetting`,
  `DefaultToneSetting`;
- Dan readback storage: `DispDanType`, `GotDanMax`, `GotDanFlg`,
  `GotDanExtraFlg`, `DispTaikojukuDan`;
- medal and item-shop readback storage: `TotalGetDonmedal`,
  `TotalUseDonmedal`, `TotalGetKatsumedal`, `TotalUseKatsumedal`,
  `ItemshopTutorialFlg`;
- counters: category counts, pushed/favorite/recent counts, credit count,
  previous area, consecutive area;
- display fields: `DispLevelTotal`, `DispLevelChassis`, `DispLevelSelf`,
  `DispScoreType`, `DifficultyPlayedCourse`, `DifficultyPlayedStar`;
- booleans: `IsAutoCostumeOn`, `IsDevil`, `IsChallengeCompe`, `IsTojiru`,
  `IsExplain`;
- `WaiwaiTutorialFlg`;
- `LastPlayDatetime`.

Do not add storage in A3 for:

- battle release flags, battle special flags, or battle bonds cap;
- Tokkun progression;
- favorite or recent song rows;
- score, crown, play history, reward, item purchase, Dan score, or battle rows.

Those belong to A4, A5, A6, or Track B.

## Default Save State

`UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(baid)` should use safe
normal-profile defaults that match Green where the Blue proto shape is
identical:

- empty title;
- titleplate `0`;
- colors `ColorFace = 0`, `ColorBody = 1`, `ColorLimb = 3`;
- costume slots all `0`;
- starter unlock bit `0` set for each costume slot and tone bitset;
- empty title flags;
- empty option flags;
- `DefaultOptionSetting` as two zero bytes;
- `DefaultShinSetting = false`;
- `DefaultToneSetting = 0`;
- `DispDanType = 1`;
- `GotDanMax = 0`;
- empty fixed-width Dan flags;
- `DispTaikojukuDan = 0` in storage;
- zero medal and counter fields;
- `IsAutoCostumeOn = true`;
- `IsDevil = false`;
- `IsExplain = false`;
- `IsChallengeCompe = false`;
- `IsTojiru = true`;
- `LastPlayDatetime = DateTime.UnixEpoch`.

`BlueProtocolBytes` should define Blue-owned constants for the initial known
widths:

- song flags;
- tone flags;
- title flags;
- costume flags;
- Dan flags;
- extra-Dan flags;
- content info.

These constants may initially equal Green's values, but tests should assert
Blue defaults against Blue constants.

## Endpoint Behavior

### `baidcheck.php`

The Blue controller should call:

```csharp
Mediator.Send(new BaidQuery(GameEra.Blue, request.AccessCode), ...)
```

Unknown access code returns:

- `Result = 1`;
- `PlayerType = 1`;
- next candidate BAID;
- no database writes.

Known access code with no `UserSaveDataBlue` returns the same new-user state
using the existing BAID. This supports registering Blue on a card already known
to Nijiiro or Green.

Known access code with a Blue save returns:

- `Result = 1`;
- `PlayerType = 0`;
- `IsPublish = true`;
- BAID and access code;
- MyDon name;
- title, titleplate, colors, selected costume slots;
- fixed-width costume flags, tone/title flags, Dan flags, and content-info
  bytes;
- medal counters and item-shop tutorial flag;
- `IsAutoCostumeOn`, Dan display fields, default tone, Waiwai tutorial flag,
  and last-play datetime.

If a known Blue save lacks the shared `UserDatum`, treat that as invalid
database state and throw with a Blue-specific message, matching Green's
existing error posture.

### `mydonentry.php`

The Blue controller should call:

```csharp
Mediator.Send(new AddMyDonEntryCommand(GameEra.Blue, request.AccessCode, request.MydonName, 0), ...)
```

The Blue handler should:

1. Reuse an existing `Card` BAID for the access code when present, otherwise
   allocate the next BAID.
2. Create or update `UserDatum` with the requested MyDon name and language.
3. Create `UserSaveDataBlue` if absent.
4. Create `Card` if absent.
5. Create `Credential` if absent.
6. Save once.

The response should include result, BAID, access code, MyDon name,
`IsPublish = true`, `ComSvrResult = 1`, and a fixed empty `ContentInfo` byte
array. A3 must not create score, Dan-score, shop, favorite, recent, reward, or
battle rows.

### `initialdatacheck.php`

The Blue controller should call:

```csharp
Mediator.Send(new GetInitialDataQuery(GameEra.Blue), ...)
```

The handler uses `IBlueCatalog` from A2:

- `SongHashVer` from the Blue catalog;
- `HashDefaultSongFlg` from Blue catalog song numbers, excluding active shop
  song locks only if the A2 catalog exposes a safe active-shop view;
- `HashMainichidojoAll` and `HashMainichidojoRare` as empty fixed-width bytes;
- telop, event-folder, taikojuku, and item-shop information arrays from the
  Blue catalog;
- `IsDanplay = true`;
- `IsClose = false`;
- `IsItemshop` from the active Blue shop catalog;
- empty `AryLegaltermsDatas`.

Battle initial-data fields are not implemented. Prefer omitting
`IsBattleplay`, release battle flags, and battle bonds cap. If cabinet tests
show the client expects explicit presence, serialize `IsBattleplay = false` and
empty fixed-width flags only after adding mapper tests that document why.

### `userdata.php`

The Blue controller should call:

```csharp
Mediator.Send(new UserDataQuery(request.Baid, GameEra.Blue), ...)
```

The handler should validate shared user existence, get or create
`UserSaveDataBlue`, and return:

- `Result = 1`;
- `SongHashVer` from Blue catalog;
- `HashReleaseSongFlg` for Blue catalog song numbers;
- fixed-width `ToneFlg`, `TitleFlg`, and `DefaultOptionSetting`;
- `OptionFlg`;
- empty `AryFavoriteSongNoes` and `AryRecentSongNoes`;
- category counters, pushed/favorite/recent counts, credit count, previous and
  consecutive area counters;
- recommend data from the Blue catalog;
- display-level fields, default shin, difficulty-played fields;
- safe `DispTaikojukuDan` value;
- challenge-compe, tojiru, and devil flags.

`tokkun_tutorial_flg` should not be serialized in A3. Tokkun was officially
disabled before Blue service, and any remaining client code should be checked
after normal identity/profile support is working.

## Mapper Rules

Blue mappers should use Blue generated response types and Blue constants. They
must not import `Adapters.GameProtocol.Green.Wire`, `GreenProtocolBytes`, or
Green mappers.

Optional-field rule:

- serialize fields that are required by the proto or intentionally populated by
  A3;
- prefer absence over unsafe zero defaults for Blue-only or unproven fields;
- add mapper tests for fields whose presence matters, using generated
  `ShouldSerialize*` methods where possible;
- keep legal terms empty and Tokkun/battle fields absent unless later evidence
  changes the design.

The Blue `disp_taikojuku_dan` mapper can reuse Green's safe-sentinel rule if
the code path is structurally identical: serialize a valid value in `1..25`,
otherwise serialize `1`. The stored value may remain `0` for a new save. This
keeps A3 boot-safe while A5 owns real Blue Dan progression.

## Error Handling

Runtime behavior should stay close to the existing Green identity path:

- unknown access code in BAID is not an error;
- known shared identity without Blue save is a Blue new-registration case;
- missing shared `UserDatum` for a known Blue save is invalid database state and
  should throw a Blue-specific `InvalidOperationException`;
- unknown BAID in `userdata.php` should throw or follow the same server-error
  shape Green currently produces rather than silently creating shared identity;
- missing Blue catalog files are A2 startup validation failures, not A3 runtime
  fallbacks.

## Testing And Verification

The implementation plan should use focused, test-first checks:

- `BlueSaveDataTests` proving default Blue save data initializes every
  fixed-width byte array using Blue constants and safe normal-profile defaults.
- Blue identity handler tests proving:
  - unknown access code returns new-user state without writes;
  - `AddMyDonEntryCommand(GameEra.Blue, ...)` creates shared identity,
    credential, card, and `UserSaveDataBlue`;
  - known card plus Blue save returns stable profile data;
  - known shared identity without Blue save is a new Blue registration;
  - Blue registration does not create score, Dan, shop, favorite, recent,
    reward, or battle rows.
- Blue initial-data tests proving:
  - all Blue catalog songs are present in default song flags;
  - legal terms are empty;
  - telop, event-folder, taikojuku, and item-shop information arrays come from
    `IBlueCatalog`;
  - battle fields are absent or explicitly false only if mapper tests require
    that behavior.
- Blue userdata tests proving:
  - release-song flags include Blue catalog songs;
  - tone/title/default-option flags come from `UserSaveDataBlue`;
  - favorite and recent arrays are empty in A3;
  - recommend values come from `IBlueCatalog`;
  - Tokkun is not serialized.
- Blue mapper tests proving intended optional field presence and absence via
  `ShouldSerialize*`.
- Green regression tests for identity and userdata still pass.
- Source guard tests or simple text checks proving Blue A3 code does not
  reference `GreenProtocolBytes`, `IGreenCatalog`, `UserSaveDataGreen`,
  Green shop state, Green ghost helpers, or Green Dan helper internals.

Verification commands:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenIdentityHandlerTests
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~GreenUserDataMapperTests
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-blue-a3"
```

If normal Host output is locked, use the temp output build path above.

## Acceptance Criteria

- `UserSaveDataBlue` exists and is mapped to `UserSaveData_Blue`.
- Shared identity rows remain shared; Blue save state is era-owned.
- Blue card registration creates `UserSaveDataBlue` and can log in again.
- Blue BAID readback returns stable normal profile fields and fixed-width
  unlock bitsets.
- Blue initial data is catalog-backed and has empty legal terms.
- Blue userdata is catalog-backed and returns valid empty favorite/recent arrays.
- Blue mappers use Blue generated wire types and Blue constants.
- Blue code does not depend on Green protocol constants, Green generated wire
  types, Green save rows, Green shop state, Green ghost state, or Green Dan
  helper internals.
- Battle, Tokkun, unclear XML files, play-result persistence, self-best,
  crowns, rewards, real Dan progression, item purchase, AdminApi, and WebUI
  behavior remain out of scope.

## Out Of Scope

- Blue play-result persistence, score readback, self-best, crowns, rewards, and
  reward unlock arrays.
- Blue favorite and recent-song persistence.
- Blue Dani progression and Dan score rows.
- Blue item purchase state and season medal behavior.
- Blue AdminApi or WebUI surfaces.
- Blue battle mode.
- Tokkun behavior.
- Banacoin balance or payment behavior.
- Parsing `present.xml`, `spacialbaid.xml`, or `waiwaiconfig.xml`.
- Runtime scraping of external pages or wiki data.

## Handoff To Later Stages

A4 should add Blue play-result persistence, self-best/crown readback, rewards,
and any favorite/recent song persistence proven by play-result writes. A5 owns
real Blue Dani completion and display-Dan progression. A6 owns Blue item shop
purchases, reward execution, and season medal state. Track B owns every battle
field and `battleuserdata.php` semantics.
