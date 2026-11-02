# Task 8: End-to-End Verification

**Goal:** Build, run the full test suite, run the EF migration on a clean DB, and spot-check the userdata response shape so the feature actually works in a server boot.

**Files:**
- None modified.

**Acceptance Criteria:**
- [ ] `dotnet build` succeeds across the whole solution.
- [ ] `dotnet test Tests/Tests.csproj` passes all tests (Green suite plus any others present).
- [ ] EF migration applies cleanly from an empty DB.
- [ ] After applying the migration, columns `SongPlayDatum_Green.IsPushed` and `GreenRecentSongs.LastPlayed` exist.

**Verify:** See steps below.

---

- [ ] **Step 1: Build the whole solution**

```bash
dotnet build
```

Expected: build succeeds with no errors.

- [ ] **Step 2: Run the full test suite**

```bash
dotnet test Tests/Tests.csproj
```

Expected: all tests pass. Note the totals; if there is any failure, return to the relevant task and fix.

- [ ] **Step 3: Validate the EF migration on an empty database**

From the repo root:

```bash
rm -f Host/wwwroot/taiko_migration_check.db3
dotnet ef database update --project Infrastructure --startup-project Host --connection "Data Source=Host/wwwroot/taiko_migration_check.db3"
```

If the `--connection` flag is unavailable for the project's EF tooling version, instead temporarily edit `Host/Configurations/Database.json` to use `taiko_migration_check.db3`, run `dotnet ef database update --project Infrastructure --startup-project Host`, then revert the file. Do NOT commit the temporary database name change.

Expected: migration runs to completion. Stdout shows `AddGreenSongCategoryFields` among the applied migrations.

- [ ] **Step 4: Verify the schema reflects the new columns**

Use the SQLite CLI (or any SQLite browser):

```bash
sqlite3 Host/wwwroot/taiko_migration_check.db3 "PRAGMA table_info('SongPlayDatum_Green');" | grep -i IsPushed
sqlite3 Host/wwwroot/taiko_migration_check.db3 "PRAGMA table_info('GreenRecentSongs');" | grep -i LastPlayed
```

Expected: each grep prints exactly one row showing the new column.

Clean up:

```bash
rm Host/wwwroot/taiko_migration_check.db3
```

- [ ] **Step 5: Confirm the recommend-songs JSON ships to publish output (smoke build)**

```bash
dotnet publish Host/Host.csproj -c Release
```

Expected: the published directory `Host/bin/Release/net10.0/<rid>/publish/` (or the configured publish dir) contains `wwwroot/data/green/recommend_songs.json`. If single-file publish is in effect and the file is bundled into the exe, instead verify via `dotnet build` and check `Host/bin/Debug/net10.0/wwwroot/data/green/recommend_songs.json` exists.

```bash
ls Host/bin/Debug/net10.0/wwwroot/data/green/recommend_songs.json
```

Expected: the file exists.

- [ ] **Step 6: Quick code-level smoke (no runtime needed)**

Open `Application/Handlers/UserDataQuery.Green.cs`. Confirm the response now references all of:
- `saveData.SongPushedCnt`
- `saveData.SongFavoriteCnt`
- `saveData.SongRecentCnt`
- `green.Recommend.RecommendSong`
- `green.Recommend.RecommendBestSongs`
- `OrderByDescending(song => song.LastPlayed)` on the recent query.

Open `Application/Handlers/UpdatePlayResultCommand.Green.cs`. Confirm:
- `IsValidGreenStage` includes `stage.MusicCateg <= 7`.
- The `HandleGreen` per-stage loop calls `GreenProfileCounters.ApplyStage(saveData, stage);`.
- `SaveStageAsync`'s `new SongPlayDatumGreen { ... }` initializer includes `IsPushed = stage.IsPushed`.
- `UpsertFavoriteAndRecentAsync` takes a `playTime` parameter and runs the 5-cap on favorites + trim-to-10 on recents.

If any of those is missing, return to the corresponding task and fix.

- [ ] **Step 7: Commit a wrap-up note (optional, only if other files moved)**

If `dotnet ef migrations add` regenerated `TaikoDbContextModelSnapshot.cs` differently than committed in Task 2 (rare, but possible if any unrelated entity changed), commit the resulting snapshot delta:

```bash
git status
# If TaikoDbContextModelSnapshot.cs shows changes beyond Task 2's diff:
git add Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
git commit -m "Sync EF snapshot after Green song category migration"
```

Otherwise, no commit needed for this task — verification only.

- [ ] **Step 8: Confirm clean working tree (besides the two pre-existing unrelated edits noted in the spec)**

```bash
git status
```

Expected: working tree clean except possibly for the two files the spec mentioned as unrelated (`Application/Common/GreenDanHelpers.cs`, `Tests/Green/GreenPlayResultHandlerTests.cs` — the latter is expected to be modified by Tasks 4/5/6/7 and *committed*, so it should not show as dirty by the end).

If `Tests/Green/GreenPlayResultHandlerTests.cs` still shows as dirty, you missed a commit — check which task is responsible and commit before considering the plan complete.
