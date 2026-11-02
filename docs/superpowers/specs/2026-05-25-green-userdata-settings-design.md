# Green Userdata Settings Design

Date: 2026-05-25

## Goal

Add focused Green userdata support for three user-visible behaviors:

- `is_tojiru`: control whether Green shows the folder-close button.
- `disp_level_chassis`: control the local machine ranking display difficulty.
- `disp_taikojuku_dan`: advance correctly after skipped Dan clears.

The feature should follow the existing Green settings patterns in the Admin API
and Web UI, and should avoid inventing new semantics for fields that are still
client-owned.

## Current State

`UserSaveDataGreen` already stores the relevant protocol fields:

- `IsTojiru`
- `DispLevelTotal`
- `DispLevelChassis`
- `DispLevelSelf`
- `DifficultyPlayedCourse`
- `DifficultyPlayedStar`
- `DispTaikojukuDan`

The server currently creates default Green saves with `IsTojiru = false`, but
official behavior expects the folder-close button to be enabled by default.
There is no Web UI option for this setting.

The server persists `DifficultyPlayedCourse` and `DifficultyPlayedStar` from
Green playresult uploads when the request marks those fields present. It also
returns the saved values through userdata. These fields should remain
client-owned in this feature.

The current Green Dan display advancement can recompute the first uncleared Dan
from slot `1`. That breaks skipped-Dan play: if a player clears 5th Dan while
1st through 4th Dan are still uncleared, the display can move to 1st Dan rather
than 6th Dan.

## Evidence

IDA evidence for Green userdata response handling shows distinct behavior for
the ranking-related fields:

- `disp_level_chassis` is handled as field `11`.
- When `disp_level_chassis` is present and nonzero, the client stores
  `value - 1` clamped to `0..3`.
- When `disp_level_chassis` is present as `0`, the client stores a separate
  status value, which appears to mean no fixed course override.
- `disp_level_self` has a separate handler and destination offsets, so it
  should not be treated as interchangeable with `disp_level_chassis`.
- `disp_level_total` was decoded by the userdata parser but no direct response
  handler use was found in the current pass.

Log evidence confirms `difficulty_played_course` and `difficulty_played_star`
are uploaded from Green playresult requests:

- `Host/Logs/log-20260515.txt` contains a Dani playresult with
  `DifficultyPlayedCourse: 2` and `DifficultyPlayedStar: 3`, while all three
  played stages are `Level: 1` and `StarLevel: 1`.
- The same request maps both presence flags as true in `CommonPlayResultData`.
- `Host/Logs/message.txt` contains `DifficultyPlayedCourse: 0` and
  `DifficultyPlayedStar: 0` with both presence flags false.

This supports treating `difficulty_played_course` and
`difficulty_played_star` as Green saved filter-folder state rather than local
ranking display difficulty or played chart difficulty.

## Scope

Implement now:

- Green default save data should use `IsTojiru = true`.
- Existing Green saves should be migrated or backfilled to `IsTojiru = true`
  because the previous false value was an unsupported server default.
- The Green settings API should expose `IsTojiru`.
- The Green Web UI should let users change `IsTojiru`.
- The Green settings API should expose `DispLevelChassis`.
- The Green Web UI should let users change `DispLevelChassis`.
- Green userdata responses should emit the saved `IsTojiru` and
  `DispLevelChassis` values.
- Green Dan advancement after a positive normal Dan clear should use the
  cleared Dan slot plus one.

Do not implement now:

- Do not expose `disp_level_self` in the Web UI.
- Do not expose `disp_level_total` in the Web UI.
- Do not expose `difficulty_played_course` or `difficulty_played_star` in the
  Web UI.
- Do not reinterpret `difficulty_played_course` or `difficulty_played_star` as
  local ranking display fields.
- Do not change Green extra Dan display behavior; `disp_taikojuku_dan` remains
  a normal Dan slot in `1..25`.

## Data Contract

Add Green-specific settings fields to the Admin API settings model:

- `GreenIsTojiru`: boolean.
- `GreenDispLevelChassis`: unsigned integer.

`GreenDispLevelChassis` uses the client field values:

- `0`: no fixed local ranking course override.
- `1`: Easy.
- `2`: Normal.
- `3`: Hard.
- `4`: Oni.

The Web UI should show `0` as the no-fixed-course state and should show the
four fixed course choices for the user-facing ranking difficulty control. This
keeps existing saves stable while still letting users choose a fixed local
ranking difficulty. The API should validate incoming values to `0..4`.

`difficulty_played_course` and `difficulty_played_star` keep their existing
playresult persistence and userdata response behavior. They should not be
included in the settings contract for this feature.

## Save Defaults And Migration

`CreateDefaultGreenSaveData` should initialize `IsTojiru = true`.

Add an EF Core migration for existing Green save rows:

- Set `IsTojiru = true` for existing rows created before this setting was
  exposed.
- Leave the existing ranking and filter fields unchanged.

Because there was no previous Web UI or Admin API support for `IsTojiru`,
stored `false` values are assumed to be the old bad default. If an operator had
manually edited the database to set `false`, they can set it again through the
new Web UI after the migration.

## Userdata Response Flow

`UserDataQuery.Green` should populate the common response from
`UserSaveDataGreen`:

- `IsTojiru` comes from the save row and defaults to true for new rows.
- `DispLevelChassis` comes from the save row.
- `DispLevelSelf` and `DispLevelTotal` remain stored and returned by existing
  behavior, but are not user-editable in this feature.
- `DifficultyPlayedCourse` and `DifficultyPlayedStar` remain the values most
  recently accepted from client playresult uploads.

The Green protocol mapper should serialize these values through the existing
generated optional protobuf fields. It should not force
`difficulty_played_course` or `difficulty_played_star` to new values during a
settings update.

## Web UI

Add Green-only controls to the existing profile/settings experience:

- A switch for `IsTojiru`.
- A compact select or segmented choice for local ranking difficulty backed by
  `GreenDispLevelChassis`.

The ranking control should include a no-fixed-course option plus plain course
labels that match the rest of the profile settings UI. It should not present
`disp_level_self`, `disp_level_total`, or filtered-folder settings because
those meanings are distinct and not part of this pass.

The Web UI should load the current Green settings, update only the edited
settings, and preserve unrelated Green customization fields.

## Dani Advancement

This spec supersedes the older Green Dani display-advancement rule anywhere it
conflicts with `docs/superpowers/specs/2026-05-15-green-dani-flow-design.md`.

When a Green playresult is accepted as a normal Dan play:

- `DanResult = 0`: do not advance `DispTaikojukuDan`.
- `DanResult = 1` or `2` and cleared Dan slot `N` is in `1..25`: set
  `DispTaikojukuDan` to `N + 1`, capped to `25`.
- Extra Dan clears do not set `DispTaikojukuDan`.
- The response path must still normalize serialized `disp_taikojuku_dan` to a
  safe normal Dan value in `1..25`.

The advancement rule must not scan from slot `1` looking for the first
uncleared Dan after a clear. Clearing 5th Dan should set the display Dan to 6th
Dan even if 1st through 4th Dan are still uncleared.

## Error Handling

- Reject Admin API updates with `GreenDispLevelChassis > 4`.
- Keep unknown or invalid stored `GreenDispLevelChassis` values from crashing
  the Web UI by displaying a safe fallback and requiring a valid value on save.
- Keep `disp_taikojuku_dan` output wire-safe even if storage contains `0`,
  extra Dan ids, or out-of-range values.
- Continue preserving `difficulty_played_course` and
  `difficulty_played_star` only when Green playresult presence flags are true.

## Tests

Add focused tests for:

- Default Green save data sets `IsTojiru = true`.
- Existing Green save rows are backfilled to `IsTojiru = true`.
- Green settings API reads and writes `IsTojiru`.
- Green settings API reads and writes `DispLevelChassis`.
- Invalid `DispLevelChassis` values are rejected.
- Green userdata response emits saved `IsTojiru`.
- Green userdata response emits saved `DispLevelChassis`.
- Green playresult persistence still updates `DifficultyPlayedCourse` and
  `DifficultyPlayedStar` only when presence flags are true.
- Clearing normal 5th Dan with positive `DanResult` sets
  `DispTaikojukuDan = 6` even when earlier Dans are uncleared.
- Failing a Dan does not advance `DispTaikojukuDan`.
- Clearing normal 25th Dan keeps `DispTaikojukuDan = 25`.
- Extra Dan clears do not set `DispTaikojukuDan` to an extra Dan id.
- Green Web UI renders controls for `IsTojiru` and local ranking difficulty.

Verification for implementation should include the focused Green tests,
relevant Web UI tests, and `dotnet build`. In-game verification is still useful
for confirming the visual behavior of the folder-close button and local ranking
tab, but the server-side contract can be verified through userdata and
playresult tests.
