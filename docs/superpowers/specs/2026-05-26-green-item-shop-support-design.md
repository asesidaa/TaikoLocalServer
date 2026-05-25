# Green Item Shop Support - Design

**Date:** 2026-05-26
**Branch:** `feat/green-version-support`
**Scope:** Add configurable Green item shop support with active-season selection,
season-scoped Don medal state, purchase/reward unlock flow, and an offline path
for curating official shop data.

## Problem

Green exposes the reward/item shop protocol, but the server does not yet provide
real shop seasons. `GreenItemShopLoader` returns an empty catalog,
`initialdatacheck.php` only advertises the shop when that catalog is non-empty,
and `getitemshopinfo.php` bypasses the handler/mapper path with a direct
success-only response. `itempurchase.php` already validates item_no/type/id/price
against the server catalog, but it spends from the global
`UserSaveData_Green.TotalUseDonmedal` balance. `rewardexecution.php` still has
placeholder logic that only accepts rewards that are already unlocked.

That is not enough for Green's shop model. Only one Green shop season is exposed
at a time, but configuration/default data should be able to contain all official
seasons and select one by `season_id`. Don medal balances and purchases must be
scoped by season, similar to Nijiiro season token behavior, not one global Green
balance.

Official shop announcement pages are image-first and list display names/prices,
not protocol ids:

- Spring: <https://taiko-ch.net/blog/?page_id=3177>
- Summer: <https://taiko-ch.net/blog/?page_id=3354>
- Fall: <https://taiko-ch.net/blog/?page_id=3645>
- Winter: <https://taiko-ch.net/blog/?page_id=3824>

Runtime configuration therefore must use resolved numeric protocol ids. Name to
id extraction is an offline curation concern.

## Goals

- Add a server setting that enables/disables the Green shop.
- Add a server setting that selects exactly one active Green shop season.
- Load Green shop seasons from a dedicated JSON data file.
- Keep item rows minimal: `item_type`, `item_id`, and `item_price` only.
- Infer protocol `item_no` from 1-based row order within the active season.
- Send active season protocol fields through `getitemshopinfo.php`.
- Advertise active shop version data through `initialdatacheck.php`.
- Lock configured shop items until the player unlocks them through the shop.
- Store Don medal earnings/spending and item purchases by shop season.
- Seed a player's first active-season medal state from existing global Green
  Don medal totals for migration compatibility.
- Preserve `ItemshopTutorialFlg` as a global one-off save field.
- Provide an offline subagent-friendly curation workflow for official default
  data candidates.

## Non-goals

- No server-side enforcement of `start_datetime` / `end_datetime` against the
  current wall clock. These are client-facing protocol/display fields.
- No provenance or source metadata in the runtime shop JSON.
- No name-only runtime rows. Names from official images must be resolved to
  numeric ids before the data is used by the server.
- No title shop support unless future client evidence proves an item shop
  `item_type` for titles.
- No reliance on unattended OCR as authoritative official data. OCR/subagents may
  produce candidate data, but a human validates before committing defaults.

## Client Evidence

`GetitemshopinfoResponse` contains:

- `result`
- `verup_no`
- `season_id`
- `telop`
- `start_datetime`
- `end_datetime`
- `afterstart_days`
- `beforeclose_days`
- `ary_itemshop_data`

Each `ItemshopData` row contains:

- `item_no`
- `item_type`
- `item_id`
- `item_price`

The client copies at most 64 shop rows. The datetime parser slices fixed
positions from a 14-character string: `yyyyMMddHHmmss`.

IDA evidence maps Green shop `item_type` values to unlock bitsets:

| item_type | Meaning | Unlock destination |
|---:|---|---|
| 1 | Song | `release_song_flg` / `release_song_no` |
| 2 | Tone | `tone_flg` / `get_tone_no` |
| 3 | Kigurumi | `CostumeFlg1` / `get_costume_no_1` |
| 5 | Head | `CostumeFlg2` / `get_costume_no_2` |
| 4 | Body | `CostumeFlg3` / `get_costume_no_3` |
| 6 | Face | `CostumeFlg4` / `get_costume_no_4` |
| 7 | Puchi | `CostumeFlg5` / `get_costume_no_5` |

The non-obvious ordering is important: `item_type=4` is body and
`item_type=5` is head. This is protocol evidence, not display ordering.

Shop display/name/icon evidence is strongest for item types 1..5. Types 6 and 7
are proven for acquired-state logic but still need more display-path evidence
before relying on them in official visible defaults.

## Settings

Extend `ServerSettings:Eras:Green`:

```json
{
  "Enabled": true,
  "AutoExtractCatalog": true,
  "GameDataPath": "wwwroot/data/green/data",
  "CustomizationNameDataPath": "",
  "EnableShop": true,
  "ActiveShopSeasonId": 1
}
```

Behavior:

- `EnableShop=false`: preserve current default unlock behavior, do not advertise
  the shop, do not apply shop locks, and reject purchases because there is no
  active shop catalog.
- `EnableShop=true`: `ActiveShopSeasonId` is required and must identify exactly
  one season in the shop data file.
- Missing or unknown active season is a startup/catalog-load failure.

The setting owns enablement and active season selection. The data file remains
pure shop data.

## Shop Data File

Add a dedicated Green shop data file under the Green data directory, for example
`Host/wwwroot/data/green/green_item_shop_data.json` in source checkouts and
`wwwroot/data/green/green_item_shop_data.json` in release layouts.

Shape:

```json
{
  "seasons": [
    {
      "season_id": 1,
      "verup_no": 1,
      "telop": "Spring reward shop",
      "start_datetime": "20190314000000",
      "end_datetime": "20190626075959",
      "afterstart_days": 7,
      "beforeclose_days": 7,
      "items": [
        { "item_type": 4, "item_id": 117, "item_price": 500 },
        { "item_type": 1, "item_id": 865, "item_price": 1300 }
      ]
    }
  ]
}
```

Rules:

- Season fields are protocol data, not metadata.
- Item rows contain only `item_type`, `item_id`, and `item_price`.
- `item_no` is not stored. It is inferred as the 1-based item index in the
  season's `items` array.
- Runtime JSON has no source URLs, image paths, confidence scores, or comments.
  Those belong in offline tooling output or documentation.

## Catalog Loading

`GreenItemShopLoader` parses the dedicated JSON file into a catalog model that
preserves both:

- all configured seasons by `season_id`
- the active season selected by settings

The active season exposes:

- `season_id`
- `verup_no`
- `telop`
- `start_datetime`
- `end_datetime`
- `afterstart_days`
- `beforeclose_days`
- ordered item rows with inferred `item_no`
- item lookup by inferred `item_no`

Validation:

- missing file is allowed only when `EnableShop=false`
- missing file is a configuration failure when `EnableShop=true`
- active season id is required when `EnableShop=true`
- active season id must exist
- `items.Count <= 64`
- `start_datetime` and `end_datetime` must match 14 ASCII digits
- item_type must be 1..7
- `item_id > 0`
- `item_price > 0`
- duplicate `(item_type, item_id)` within a season is invalid
- duplicate season ids are invalid

The loader does not enforce the current date range.

## Protocol Responses

`initialdatacheck.php`:

- advertise `is_itemshop=true` only when `EnableShop=true` and the active season
  has at least one row
- add one `ary_itemshop_data` entry for the active season using
  `InfoId = season_id` and `VerupNo = verup_no`

`getitemshopinfo.php`:

- route through `GetItemShopInfoQueryHandler` and `ItemShopMappers`
- when shop is disabled, return success with no rows
- when shop is enabled, return active season fields and ordered rows
- include inferred `item_no` in each row

The mapper must populate `AryItemshopDatas`; the current mapper only sets the
season envelope.

BAID and item purchase responses:

- when shop is enabled, report active-season `TotalGetDonmedal` and
  `TotalUseDonmedal` through the existing protocol fields
- `ItemshopTutorialFlg` remains read from `UserSaveData_Green`

## Persistence

Add season-scoped shop state.

`GreenShopSeasonState`:

- key: `(Baid, SeasonId)`
- fields: `TotalGetDonmedal`, `TotalUseDonmedal`, `CreatedAt`, `UpdatedAt`

`GreenShopItemState`:

- key: `(Baid, SeasonId, ItemType, ItemId)`
- fields: `ItemNo`, `ItemPrice`, `Status`, `PurchasedAt`, `UnlockedAt`

`Status` values:

- `PendingReward`
- `Unlocked`

Creation behavior:

- first touch of an active season creates `GreenShopSeasonState`
- if the player has no `GreenShopSeasonState` rows at all, seed the first row
  from
  `UserSaveData_Green.TotalGetDonmedal` and
  `UserSaveData_Green.TotalUseDonmedal`
- later seasons start from zero and do not share balances

This preserves existing player balances when the shop is first enabled while
still providing season isolation after that.

## Medal Updates

When `EnableShop=false`, `UpdatePlayResultCommand.Green` continues updating
`UserSaveData_Green.TotalGetDonmedal` as it does today.

When `EnableShop=true`:

- play-result `GetDonmedal` increments the active season's
  `GreenShopSeasonState.TotalGetDonmedal`
- purchase spending increments active season
  `GreenShopSeasonState.TotalUseDonmedal`
- global `UserSaveData_Green.TotalGetDonmedal` / `TotalUseDonmedal` are not the
  source of truth for shop balances after the season row exists
- `TotalGetKatsumedal` / `TotalUseKatsumedal` remain global because the Green
  item shop design here only spends Don medals

The first-touch seed rule avoids resetting existing users to zero immediately
when the feature is enabled.

## Locking Behavior

When `EnableShop=false`, no shop locks are applied.

When `EnableShop=true`, only items configured in the active season are locked
until that player has an `Unlocked` `GreenShopItemState` row for the same
`SeasonId`, `ItemType`, and `ItemId`.

Lock application:

- `item_type=1`: remove configured song ids from `initialdatacheck` default song
  flags unconditionally because that request is not profile-scoped; remove them
  from `userdata` release song flags unless unlocked for the current player
- `item_type=2`: remove configured tone ids from `tone_flg` unless unlocked
- `item_type=3`: remove configured kigurumi ids from `CostumeFlg1` unless
  unlocked
- `item_type=5`: remove configured head ids from `CostumeFlg2` unless unlocked
- `item_type=4`: remove configured body ids from `CostumeFlg3` unless unlocked
- `item_type=6`: remove configured face ids from `CostumeFlg4` unless unlocked
- `item_type=7`: remove configured puchi ids from `CostumeFlg5` unless unlocked

Unconfigured songs, tones, and costumes follow existing behavior.

Title flags are not affected by this design because no title shop `item_type` is
proven.

## Purchase Flow

`itempurchase.php` validates against the active season catalog.

Steps:

1. Resolve active season from settings/catalog.
2. Resolve the requested `item_no` to the inferred active-season row.
3. Compare request `item_type`, `item_id`, and `item_price` to the server row.
4. Reject zero price, unknown rows, mismatches, insufficient active-season Don
   medals, already unlocked items, and already pending items.
5. Increment active-season `TotalUseDonmedal`.
6. Create `GreenShopItemState` as `PendingReward`.
7. Return success with active-season medal totals.

Duplicate purchase attempts must not double spend.

## Reward Execution Flow

`rewardexecution.php` stays separate from purchase because the client performs a
purchase request followed by reward execution.

Steps:

1. Resolve active season.
2. Read the reward arrays from the request.
3. Convert each requested id to the matching shop `item_type` domain:
   - `release_song_no` -> item_type 1
   - `get_tone_no` -> item_type 2
   - `get_costume_no_1` -> item_type 3
   - `get_costume_no_2` -> item_type 5
   - `get_costume_no_3` -> item_type 4
   - `get_costume_no_4` -> item_type 6
   - `get_costume_no_5` -> item_type 7
4. Accept a requested unlock only when there is a matching active-season item in
   `PendingReward` state, or it is already unlocked for idempotent retries.
5. Reject catalog-forged unlocks.
6. Apply the corresponding Green bitset.
7. Mark matching pending rows as `Unlocked`.

The current "already unlocked only" placeholder is replaced by
pending-purchase validation.

## Official Data Curation Workflow

The runtime server does not parse official announcement pages. It consumes only
numeric shop data.

Offline curation can be delegated to subagents because each season is mostly
independent:

- fetch the official page
- extract the announcement image URLs
- OCR/read item names and prices from the images
- cross-reference item names with local Green song/customization catalogs
- map rows to `item_type`, `item_id`, and `item_price`
- preserve row order so `item_no` can be inferred later
- produce a candidate season JSON fragment
- produce a review report with evidence, confidence, and unresolved rows

The main session reviews and reconciles candidate output before committing
default data. This keeps official-data extraction efficient without making OCR
authoritative.

## Testing

Focused tests should cover:

- shop disabled preserves old unlock behavior and does not advertise item shop
- `EnableShop=true` requires active season id
- active season id must exist in the shop data file
- loader infers 1-based `item_no`
- loader rejects more than 64 rows
- loader rejects invalid date formats and duplicate ids
- `initialdatacheck.php` advertises the active season id/version
- `getitemshopinfo.php` returns active season fields and ordered rows
- purchase validates item_no/type/id/price against the active season
- purchase uses active-season Don medal balance
- duplicate purchase does not double spend
- first-ever shop season state seeds medal state from existing global Green
  totals
- different seasons have isolated medal balances and item states
- reward execution unlocks songs, tones, and every supported costume slot
- regression for body/head mapping: `item_type=4` unlocks `CostumeFlg3`, and
  `item_type=5` unlocks `CostumeFlg2`
- shop locks remove only configured active-season items and restore them after
  unlock
- `ItemshopTutorialFlg` remains global and is not season-scoped

## Implementation Notes

- Keep edits scoped to Green shop settings, catalog loading, protocol handlers,
  persistence, and tests.
- Preserve existing global Green medal columns for compatibility and migration
  seeding.
- Avoid a generic `reward_kind` abstraction in the shop catalog. `item_type` is
  the protocol discriminator and is the canonical model.
- Do not make runtime official-page scraping part of server startup.
- Keep the direct `getitemshopinfo.php` controller response only until the
  handler/mapper path is ready; final implementation routes through the
  application handler.
