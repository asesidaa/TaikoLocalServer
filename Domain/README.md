# Domain

Pure domain core — entities, value objects, enums, constants. The hexagonal
inner ring; no project or package references at all.

## Role

Holds the language of the game (`Card`, `UserData`, `DanType`, `CrownType`,
`Difficulty`, `DomainConstants`, ...) free of any infrastructure concern.
Migrations live in `Infrastructure/Persistence/Migrations/`; serialization
shapes live in `Contracts.AdminApi`/`Application`.

## Dependencies

- Inbound (transitively): every other project in the solution.
- Outbound: none.

## Key folders

- `Entities/` — EF Core entity types (`UserData`, `SongBestData`,
  `DanScoreData`, ...) annotated with data attributes and EF conventions.
- `Enums/` — domain enums (`Difficulty`, `CrownType`, `ScoreRank`, `DanType`,
  ...).
- `DomainConstants.cs` — repo-wide constants (`MusicIdMax`,
  `MusicIdMaxExpanded`, `DateTimeFormat`, ...).

## When to add code here

- New entity or aggregate.
- Domain enum, value object, or constant.

Do **not** add: DTOs (→ `Contracts.AdminApi/Application`), port interfaces
(→ `Application/Abstractions/`), settings (→ `Application/Settings/`), or
EF Core configuration / migrations (→ `Infrastructure/Persistence/`).

See the root `CLAUDE.md` for the full hexagonal layout.
