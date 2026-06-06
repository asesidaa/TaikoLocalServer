# Domain

Domain is the pure core of the solution. It owns entities, enums, and constants, and has no project or package references.

## Role

Domain names the durable state used by Application and Infrastructure without depending on HTTP, EF configuration, JSON contracts, or filesystem code.

Era-scoped state uses era suffixes. Blue state is separate from Green and Nijiiro state except where the data is shared identity state such as cards and users.

## Key Folders

- `Entities/` - persistent entity types such as users, score rows, save data, item-shop rows, Blue Tokkun rows, and Blue battle rows.
- `Enums/` - shared domain enums such as `GameEra`, `Difficulty`, and play/result classifications.
- `DomainConstants.cs` - repo-wide constants.

## Blue Notes

- Blue normal state uses Blue entities such as `UserSaveDataBlue`, `SongPlayDatumBlue`, `SongBestDatumBlue`, `DanScoreDatumBlue`, favorites, recent songs, and shop state.
- Blue Tokkun state uses nullable `UserSaveDataBlue.TokkunTutorialFlg` plus `BlueTokkunStageResult` append-only raw history rows.
- Blue battle state uses `BlueBattleUserState`, `BlueBattleNpcState`, `BlueBattleTokenState`, and `BlueBattleStageResult`.
- Do not collapse Blue Tokkun or Blue battle state into Green AI Battle state, Green shop state, or normal Blue score state.

## When To Add Code Here

- Add a new entity or enum.
- Add a domain constant used across layers.

Do not add DTOs, port interfaces, settings, EF configuration, migrations, or protocol wire models here.
