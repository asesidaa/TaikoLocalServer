# Green Auto Costume Design

Date: 2026-05-26

## Goal

Expose Green's existing `is_auto_costume_on` userdata option and use it to
control whether Green playresult uploads can replace a player's currently
selected costume from `ary_current_costume`.

The option should default to enabled so Green keeps the official behavior where
the game can equip newly awarded Dan-specific costumes. Users can disable it in
the Web UI when they want to keep their chosen costume stable.

## Current State

Green already has the protocol and storage shape for this behavior:

- `BAIDResponse.is_auto_costume_on` is generated as `IsAutoCostumeOn`.
- `UserSaveDataGreen.IsAutoCostumeOn` persists the value.
- `BaidQuery.Green` already returns the saved value through
  `CommonBaidResponse.IsAutoCostumeOn`.
- `BaidResponseMapper` already serializes it back to Green.

What is missing:

- New Green save data currently defaults `IsAutoCostumeOn` to `false`.
- Existing Green saves contain the old false default unless manually edited.
- The Admin API settings contract does not expose the option.
- The Web UI profile/settings page does not expose the option.
- `UpdatePlayResultCommand.Green` currently applies `AryCurrentCostume` whenever
  the field is present, regardless of the saved option.

Local log evidence shows `AryCurrentCostume` appears in normal, AI Battle, and
Dani playresults. Therefore this feature treats `IsAutoCostumeOn` as a broad
Green playresult costume replacement gate, not as a Dan-only setting.

## Scope

Implement now:

- Default new Green saves to `IsAutoCostumeOn = true`.
- Backfill existing Green save rows to `IsAutoCostumeOn = true`.
- Add a Green-specific settings field to the Admin API settings model.
- Read and write the field in `UserSettingsController.Green`.
- Add a Green-only switch to the Web UI profile settings page.
- Gate `AryCurrentCostume` application in `UpdatePlayResultCommand.Green` on
  `saveData.IsAutoCostumeOn`.
- Keep explicit costume reward unlock arrays working regardless of the option.

Do not implement now:

- Do not add a second server-only auto-costume setting.
- Do not make the behavior Dan-only.
- Do not change Nijiiro playresult costume persistence.
- Do not change Green favorite costume slots or add favorite costume editing.

## Data Contract

Add a Green settings field to `UserSetting`:

- `GreenIsAutoCostumeOn`: boolean.

The field maps directly to `UserSaveDataGreen.IsAutoCostumeOn` and to Green's
existing `is_auto_costume_on` wire value. It should be exposed only through
Green-era settings routes and Green Web UI controls.

The BAID response path keeps using the existing `CommonBaidResponse` and
`BaidResponseMapper` fields. No new protocol field is needed.

## Save Defaults And Migration

`CreateDefaultGreenSaveData` should initialize:

```csharp
IsAutoCostumeOn = true
```

Add an EF Core migration that sets `IsAutoCostumeOn = true` for existing
`UserSaveDataGreen` rows. The previous false value was an unexposed server
default, not a user-visible choice. After migration, users who dislike automatic
costume replacement can disable it from the Web UI.

## Admin API

`BuildGreenUserSetting` should populate `GreenIsAutoCostumeOn` from
`saveData.IsAutoCostumeOn`.

`SaveGreenUserSetting` should persist `userSetting.GreenIsAutoCostumeOn` to
`saveData.IsAutoCostumeOn`.

There is no special validation beyond normal boolean binding.

## Web UI

Add a Green-only switch in the existing profile settings section. The label is:

- `Apply Costume Changes from Play Results`

The switch should appear near the other Green profile behavior switches, not in
the costume picker list. It controls automatic server acceptance of playresult
current-costume changes; manual costume selection remains in the Costume tab.

The Web UI should round-trip the loaded setting with the rest of the profile
settings so saving unrelated profile changes does not unintentionally disable
the option.

## Playresult Flow

Green playresult handling should behave as follows:

- If `HasAryCurrentCostume == false`, leave the saved current costume unchanged.
- If `HasAryCurrentCostume == true` and `saveData.IsAutoCostumeOn == true`,
  apply `AryCurrentCostume` to `Costume1..5`.
- When applying `AryCurrentCostume`, continue marking the equipped ids as
  unlocked in the per-slot costume bitsets. This preserves the current behavior
  that accepts client-sent Dan-specific ids even when the local catalog does not
  know them yet.
- If `HasAryCurrentCostume == true` and `saveData.IsAutoCostumeOn == false`,
  leave `Costume1..5` unchanged and do not unlock ids solely from
  `AryCurrentCostume`.
- Always apply explicit reward arrays `GetCostumeNo1s..5s` through the existing
  unlock flow, regardless of `IsAutoCostumeOn`.

This preserves official automatic replacement by default while giving users a
way to opt out of overwriting their selected costume.

## Error Handling

- Missing `AryCurrentCostume` remains non-destructive.
- Unknown costume ids remain accepted when auto costume is enabled, because Dan
  reward costumes may not be present in the local extracted catalog.
- Unknown ids from `AryCurrentCostume` are ignored when auto costume is
  disabled unless they also appear in explicit reward arrays.
- Existing malformed or out-of-range bitsets should continue using the current
  fixed-width bitset normalization helpers.

## Tests

Add focused tests for:

- New Green save data defaults `IsAutoCostumeOn` to true.
- Existing Green save rows are backfilled to true.
- Green settings GET exposes `GreenIsAutoCostumeOn`.
- Green settings POST persists `GreenIsAutoCostumeOn`.
- Green Web UI renders a Green-only switch for the option.
- Green BAID response emits the saved `is_auto_costume_on` value.
- Green playresult with `HasAryCurrentCostume = true` updates saved costume
  when auto costume is enabled.
- Green playresult with `HasAryCurrentCostume = true` preserves saved costume
  and does not unlock ids solely from `AryCurrentCostume` when auto costume is
  disabled.
- Green playresult still applies explicit `GetCostumeNo*` unlock arrays when
  auto costume is disabled.
- Green playresult with missing `AryCurrentCostume` still preserves saved
  costume.

Verification should include focused Green tests, relevant Web UI tests, and
`dotnet build`. Cabinet testing is useful for confirming the visual result, but
the server-side contract can be verified through settings, BAID, and
playresult tests.
