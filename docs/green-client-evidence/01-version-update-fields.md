# Version and Update Field Evidence

## Scope

Fields: `song_hash_ver`, `verup_no`, `season_id`, `start_datetime`, `end_datetime`.

Endpoints: `initialdatacheck.php`, `taikojuku.php`, `getitemshopinfo.php`, `gettelop.php`, `getfolder.php`, `tournamentcheck.php`.

## Evidence Table

| Endpoint | Field | IDA evidence | Constraint | Server behavior at audit baseline/current tree | Recommendation |
|---|---|---|---|---|---|
| `initialdatacheck.php` | `song_hash_ver` | IDA-proven schema string: endpoint `0xEFEC60`, field `0xF006CB`. Value semantics not found. | Source-inferred: use a nonzero Green catalog version; `0` acceptance is not IDA-proven. | Current tree sets `green.SongHashVersion` in `Application/Handlers/GetInitialDataQuery.Green.cs:16`; `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs:32` parses missing/invalid XML as `0`. | Add fail-fast nonzero validation in `GreenMusicInfoLoader.cs` or `GreenEraGameDataCatalog.cs` before response mapping. |
| `taikojuku.php` | `verup_no` | IDA-proven schema string: endpoint `0xEFECE8`, field `0xF005D3`. Prior audit inferred cache/update comparison. | Source-inferred: `0` is safe as no-update only while pack data is otherwise valid. Nonzero domain not found. | Current tree copies `GreenTaikojukuEntry.VerupNo` in `Application/Handlers/GetTaikojukuQuery.Green.cs:107`; mapper serializes at `Adapters.GameProtocol.Green/Mappers/TaikojukuMappers.cs:37`. | Keep slot/song guards. If real cache updates are implemented, validate `VerupNo` beside `GetTaikojukuQuery.Green.cs:107`. |
| `getitemshopinfo.php` | `season_id` and dates | IDA-proven schema strings: endpoint `0xEFED00`, `season_id` `0xF00C8D`, `start_datetime` `0xF00CB1`, `end_datetime` `0xF00CC9`. Date format not found. | Source-inferred: omit all season/date optionals unless a real shop season and valid date format are known. | Current tree returns only `Result = 1` in `Adapters.GameProtocol.Green/Controllers/GetItemShopInfoController.cs:12`, so dates are now omitted. `ItemShopMappers.cs:13-17` would still serialize defaults if used. | Keep the direct controller omission or make `ItemShopMappers.cs` presence-aware before routing through the handler/mapper. |
| `gettelop.php` | `verup_no` and dates | IDA-proven schema strings: endpoint `0xEFECB8`, `verup_no` `0xF00300`, `start_datetime` `0xF00314`, `end_datetime` `0xF0032C`. Value/date semantics not found. | Source-inferred: omit while no telop catalog rows are advertised. | Current controller returns only `Result = 1`, so fields are omitted. | Add guards in `GetTelopController.cs` or a future handler/mapper only when telops are catalog-backed and advertised. |
| `getfolder.php` | `verup_no` | IDA-proven schema strings: endpoint `0xEFECD0`, field `0xF004E2`. Value semantics not found. | Source-inferred: omit while no event folder rows are advertised. | Current controller/mapper return only `Result = 1`. | Add folder ID/song/version validation in `Application/Handlers/GetFolderQuery.Green.cs` and `Adapters.GameProtocol.Green/Mappers/FolderDataMappers.cs` before populating rows. |
| `tournamentcheck.php` | `song_hash_ver` | IDA-proven schema strings: endpoint `0xEFEC80`, field `0xF00974`. Value semantics not found. | Source-inferred: omit until tournament/gacha payload is real; if populated, use nonzero catalog version. | Current controller returns only `Result = 1`; `TournamentMappers.cs:14` would serialize default `0` if used. | Keep omitted on the direct controller path; guard `TournamentCheckQuery.Green.cs` and `TournamentMappers.cs` before common mapper use. |

## Artifact Files

- `.tools/green-field-evidence/version_update_strings.json`
- `.tools/green-field-evidence/version_update_xrefs.json`
