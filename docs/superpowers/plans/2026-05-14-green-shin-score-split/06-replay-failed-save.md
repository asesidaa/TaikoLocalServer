# Task 6: Replay Failed Save

**Files:**
- Runtime data operation only: `Host/bin/Debug/net10.0/wwwroot/taiko.db3`

This task intentionally uses direct SQLite operations. It does not use the local save migrator.

## Captured Save Data

The rejected payload in `Host/Logs/log-20260514.txt` at `2026-05-14 03:24:44` has:

- `Baid = 1`
- `PlayDatetime = 20260514032442`
- `AreaCode = 1`
- `GetDonmedal = 550`
- `GetKatsumedal = 100`
- `DifficultyPlayedCourse = 2`
- `DifficultyPlayedStar = 3`
- stage 1: song `729`, level `1`, normal mode `stage_mode = 0`, score `229170`
- stage 2: song `729`, level `2`, Shin mode `stage_mode = 1`, score `897650`
- reward IDs:
  - tone `4`
  - costume slot 1 IDs `1`, `43`, `3`, `44`
  - title IDs `106`, `132`, `144`, `151`, `158`, `181`

## Steps

- [ ] **Step 1: Confirm SQLite CLI and database path**

Run:

```powershell
Get-Command sqlite3
Test-Path "Host\bin\Debug\net10.0\wwwroot\taiko.db3"
```

Expected: `sqlite3` resolves to an executable and the database path returns `True`.

- [ ] **Step 2: Back up the database**

Run:

```powershell
Copy-Item -LiteralPath "Host\bin\Debug\net10.0\wwwroot\taiko.db3" -Destination "Host\bin\Debug\net10.0\wwwroot\taiko.db3.before-green-shin-replay.bak"
```

Expected: no output and the backup file exists.

- [ ] **Step 3: Confirm migration has added Shin columns**

Run:

```powershell
sqlite3 "Host\bin\Debug\net10.0\wwwroot\taiko.db3" "PRAGMA table_info('SongBestDatum_Green');" "PRAGMA table_info('SongPlayDatum_Green');"
```

Expected: both table definitions include `IsShin`.

- [ ] **Step 4: Confirm BAID 1 save row exists**

Run:

```powershell
sqlite3 "Host\bin\Debug\net10.0\wwwroot\taiko.db3" ".mode quote" "SELECT Baid, MyDonName FROM UserData WHERE Baid = 1;" "SELECT Baid, hex(ToneFlg), hex(CostumeFlg1), hex(TitleFlg), TotalGetDonmedal, TotalGetKatsumedal FROM UserSaveData_Green WHERE Baid = 1;"
```

Expected:

```text
1,'あ'
1,'01000000000000000000000000000000','0100000000000000000000000000000000000000000000000000000000000000','0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000',0,0
```

If the values differ, stop before Step 5 and preserve the backup from Step 2. Do not run the replay SQL against a database with additional unsummarized Green saves because the fixed bitset updates in Step 5 are calibrated to the verified values above.

- [ ] **Step 5: Replay the rejected save directly into SQLite**

Run:

```powershell
@"
PRAGMA foreign_keys = ON;
BEGIN;

UPDATE UserSaveData_Green
SET
    ToneFlg = X'11000000000000000000000000000000',
    CostumeFlg1 = X'0B00000000180000000000000000000000000000000000000000000000000000',
    TitleFlg = X'0000000000000000000000000004000010008140000020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000',
    TotalGetDonmedal = TotalGetDonmedal + 550,
    TotalGetKatsumedal = TotalGetKatsumedal + 100,
    ItemshopTutorialFlg = 0,
    IsDevil = 0,
    IsExplain = 0,
    DifficultyPlayedCourse = 2,
    DifficultyPlayedStar = 3,
    WaiwaiTutorialFlg = 0,
    PrevAreaCode = 1,
    LastPlayDatetime = '2026-05-14 03:24:42'
WHERE Baid = 1;

DELETE FROM SongPlayDatum_Green
WHERE Baid = 1
  AND SongId = 729
  AND PlayTime = '2026-05-14 03:24:42';

INSERT INTO SongPlayDatum_Green (
    Baid, SongId, Difficulty, Crown, Score, ScoreRate,
    GoodCount, OkCount, MissCount, ComboCount, HitCount, PoundCount,
    StarLevel, SupportLevel, OptionFlg, ToneFlg, PlayMode, StageMode, IsShin,
    MusicCategory, SelectedFolderId, IsFavorite, IsRecent, IsPapamama,
    SoulGauge, PlayDan, WaiwaiResult, WaiwaiGauge, PlayTime
) VALUES
(
    1, 729, 2, 1, 229170, 0,
    49, 12, 1, 54, 127, 66,
    2, 0, X'0000', X'01000000000000000000000000000000', 0, 0, 0,
    1, 0, 0, 0, 0,
    100, 0, 0, 0, '2026-05-14 03:24:42'
),
(
    1, 729, 3, 1, 897650, 0,
    71, 16, 1, 78, 160, 73,
    3, 0, X'0000', X'01000000000000000000000000000000', 0, 1, 1,
    1, 0, 0, 0, 0,
    100, 0, 0, 0, '2026-05-14 03:24:42'
);

INSERT INTO SongBestDatum_Green (
    Baid, SongId, Difficulty, IsShin, BestScore, BestRate, BestCrown
) VALUES
    (1, 729, 2, 0, 229170, 0, 1),
    (1, 729, 3, 1, 897650, 0, 1)
ON CONFLICT(Baid, SongId, Difficulty, IsShin) DO UPDATE SET
    BestScore = CASE
        WHEN excluded.BestScore > SongBestDatum_Green.BestScore THEN excluded.BestScore
        ELSE SongBestDatum_Green.BestScore
    END,
    BestRate = CASE
        WHEN excluded.BestScore > SongBestDatum_Green.BestScore THEN excluded.BestRate
        ELSE SongBestDatum_Green.BestRate
    END,
    BestCrown = CASE
        WHEN excluded.BestCrown > SongBestDatum_Green.BestCrown THEN excluded.BestCrown
        ELSE SongBestDatum_Green.BestCrown
    END;

COMMIT;
"@ | sqlite3 "Host\bin\Debug\net10.0\wwwroot\taiko.db3"
```

Expected: no output.

- [ ] **Step 6: Verify replayed rows**

Run:

```powershell
sqlite3 "Host\bin\Debug\net10.0\wwwroot\taiko.db3" ".mode column" ".headers on" "SELECT Baid, TotalGetDonmedal, TotalGetKatsumedal, DifficultyPlayedCourse, DifficultyPlayedStar, hex(ToneFlg) AS ToneFlg, hex(CostumeFlg1) AS CostumeFlg1 FROM UserSaveData_Green WHERE Baid = 1;" "SELECT Baid, SongId, Difficulty, StageMode, IsShin, Score, GoodCount, OkCount, MissCount, ComboCount, HitCount, PoundCount FROM SongPlayDatum_Green WHERE Baid = 1 AND SongId = 729 ORDER BY IsShin, Difficulty;" "SELECT Baid, SongId, Difficulty, IsShin, BestScore, BestCrown FROM SongBestDatum_Green WHERE Baid = 1 AND SongId = 729 ORDER BY IsShin, Difficulty;"
```

Expected:

```text
SongPlayDatum_Green contains two rows:
- Difficulty 2, StageMode 0, IsShin 0, Score 229170
- Difficulty 3, StageMode 1, IsShin 1, Score 897650

SongBestDatum_Green contains two rows:
- Difficulty 2, IsShin 0, BestScore 229170
- Difficulty 3, IsShin 1, BestScore 897650
```

Do not commit the database backup. This task changes runtime data, not source.
