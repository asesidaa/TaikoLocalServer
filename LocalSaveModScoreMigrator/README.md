# LocalSaveModScoreMigrator

Standalone CLI utility (System.CommandLine) for importing local-save-mod
JSON dumps into `taiko.db3`. Not part of the runtime — operators run it
once after exporting from a local-save-mod install.

## Role

Reads a local-save-mod JSON file and writes the relevant rows
(`SongPlayData`, `SongBestData`, `UserData` updates, ...) into the
SQLite database via the same `TaikoDbContext` the server uses. Reuses
`Infrastructure` so EF Core configurations and entity types stay in
sync with the runtime.

## Dependencies

- Inbound: none (it's a standalone exe; not referenced by anything).
- Outbound: `Domain`, `Infrastructure`.
- Notable packages: `System.CommandLine`,
  `Microsoft.EntityFrameworkCore.Sqlite`, `SharpZipLib`,
  `JorgeSerrano.Json.JsonSnakeCaseNamingPolicy`.

## Key files

- `Program.cs` — CLI entry point and argument parsing.
- `PlayRecordJson.cs`, `MusicInfo.cs`, `MusicInfoEntry.cs` — JSON shapes
  matching the local-save-mod export format.
- `DateTimeConverter.cs`, `ScoreRankConverter.cs` — `JsonConverter`
  types for the import file format.

## Usage

From the repo root:

```bash
dotnet run --project LocalSaveModScoreMigrator -- --save-file-path <path> --baid <id>
```

## When to add code here

- A new local-save-mod export field that needs to be imported.
- Support for a new local-save-mod file format / version.
- A new CLI flag.

Do **not** add: server-side runtime logic (this exe is run once,
offline), HTTP code (it has no HTTP surface), or domain entities (→
`Domain`).

See the root `CLAUDE.md` for the full hexagonal layout.
