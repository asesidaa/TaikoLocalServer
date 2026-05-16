# Task 5: Recent-List Ordering & Favorites Cap

**Goal:** Make `GreenRecentSongs` server-tracked (upsert on every played stage, trim to 10 newest) and enforce the Green-era 5-favorite cap on the playresult write path.

**Files:**
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`

**Acceptance Criteria:**
- [ ] `UpsertFavoriteAndRecentAsync` takes a `DateTime playTime` parameter, called from `SaveStageAsync`.
- [ ] Every played stage upserts a `GreenRecentSongs` row with `LastPlayed = playTime`, regardless of `stage.IsRecent`.
- [ ] After upsert, rows beyond the 10 most-recent (by `LastPlayed`) are removed for that `Baid`.
- [ ] Favorites cap: if `stage.IsFavorite=true` and the existing favorite count is already ≥ 5, the new insert is silently skipped.
- [ ] Favorites removal still works at any count (no cap check on the remove branch).

**Verify:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests"` → all pass.

---

- [ ] **Step 1: Write the failing tests**

Append these tests to `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
[Fact]
public async Task UpdatePlayResult_Green_RecentSongsUpsertsForEveryStage_OrderedByPlayTime()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    // First play: SongNo 101.
    await handler.Handle(new UpdatePlayResultCommand(1, GameEra.Green, new CommonPlayResultData
    {
        Baid = 1,
        PlayDatetime = "2026-05-15 09:00:00",
        AryStageInfoes = [PlainStage(songNo: 101)]
    }), CancellationToken.None);

    // Second play: SongNo 102, later.
    await handler.Handle(new UpdatePlayResultCommand(1, GameEra.Green, new CommonPlayResultData
    {
        Baid = 1,
        PlayDatetime = "2026-05-15 12:00:00",
        AryStageInfoes = [PlainStage(songNo: 102)]
    }), CancellationToken.None);

    var recents = await fixture.Context.GreenRecentSongs
        .Where(s => s.Baid == 1)
        .OrderByDescending(s => s.LastPlayed)
        .ToListAsync();
    Assert.Equal(2, recents.Count);
    Assert.Equal(102u, recents[0].SongNo);
    Assert.Equal(101u, recents[1].SongNo);
}

[Fact]
public async Task UpdatePlayResult_Green_RecentSongsTrimToTen()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    // Play 11 distinct songs across 11 plays, increasing timestamps.
    for (var i = 0; i < 11; i++)
    {
        await handler.Handle(new UpdatePlayResultCommand(1, GameEra.Green, new CommonPlayResultData
        {
            Baid = 1,
            PlayDatetime = new DateTime(2026, 5, 15, 9, 0, 0).AddMinutes(i).ToString("yyyy-MM-dd HH:mm:ss"),
            AryStageInfoes = [PlainStage(songNo: (uint)(101 + i))]
        }), CancellationToken.None);
    }

    var recents = await fixture.Context.GreenRecentSongs
        .Where(s => s.Baid == 1)
        .OrderByDescending(s => s.LastPlayed)
        .ToListAsync();
    Assert.Equal(10, recents.Count);
    Assert.Equal(111u, recents[0].SongNo);   // newest
    Assert.Equal(102u, recents[9].SongNo);   // 10th newest; 101 trimmed
    Assert.DoesNotContain(recents, r => r.SongNo == 101u);
}

[Fact]
public async Task UpdatePlayResult_Green_FavoritesCapAtFive()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    // Six distinct favorite-marked songs.
    for (var i = 0; i < 6; i++)
    {
        await handler.Handle(new UpdatePlayResultCommand(1, GameEra.Green, new CommonPlayResultData
        {
            Baid = 1,
            PlayDatetime = new DateTime(2026, 5, 15, 9, 0, 0).AddMinutes(i).ToString("yyyy-MM-dd HH:mm:ss"),
            AryStageInfoes = [PlainStage(songNo: (uint)(101 + i), isFavorite: true)]
        }), CancellationToken.None);
    }

    var favorites = await fixture.Context.GreenFavoriteSongs
        .Where(s => s.Baid == 1)
        .ToListAsync();
    Assert.Equal(5, favorites.Count);
    // 6th (SongNo 106) must have been silently rejected.
    Assert.DoesNotContain(favorites, f => f.SongNo == 106u);
}

[Fact]
public async Task UpdatePlayResult_Green_FavoritesRemovalWorksWhenAtCap()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    // Seed 5 favorites directly.
    for (var i = 0; i < 5; i++)
    {
        fixture.Context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = 1, SongNo = (uint)(101 + i) });
    }
    await fixture.Context.SaveChangesAsync();

    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    // Play SongNo 101 with IsFavorite=false → must remove favorite, even at cap.
    await handler.Handle(new UpdatePlayResultCommand(1, GameEra.Green, new CommonPlayResultData
    {
        Baid = 1,
        PlayDatetime = "2026-05-15 12:00:00",
        AryStageInfoes = [PlainStage(songNo: 101, isFavorite: false)]
    }), CancellationToken.None);

    var favorites = await fixture.Context.GreenFavoriteSongs.Where(s => s.Baid == 1).ToListAsync();
    Assert.Equal(4, favorites.Count);
    Assert.DoesNotContain(favorites, f => f.SongNo == 101u);
}

private static CommonPlayResultData.StageData PlainStage(uint songNo, bool isFavorite = false)
    => new()
    {
        SongNo = songNo,
        Level = 1,
        PlayResult = 1,
        PlayScore = 100000,
        GoodCnt = 1, OkCnt = 0, NgCnt = 0, PoundCnt = 0, ComboCnt = 1, HitCnt = 1,
        OptionFlg = [0],
        ToneFlg = [0],
        MusicCateg = 0,
        IsFavorite = isFavorite,
        IsRecent = false
    };
```

If a `PlainStage` helper already exists in the file with a different signature, rename this new one to `MakeStage` and use the new name everywhere in the test bodies above. The test class has only this scope of helpers; no clash is expected.

- [ ] **Step 2: Run the tests and verify they fail**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UpdatePlayResult_Green_RecentSongsUpsertsForEveryStage_OrderedByPlayTime Or FullyQualifiedName~UpdatePlayResult_Green_RecentSongsTrimToTen Or FullyQualifiedName~UpdatePlayResult_Green_FavoritesCapAtFive Or FullyQualifiedName~UpdatePlayResult_Green_FavoritesRemovalWorksWhenAtCap"
```

Expected:
- `RecentSongsUpsertsForEveryStage_OrderedByPlayTime` fails: only 0–1 rows present (existing code only inserts when `stage.IsRecent=true`).
- `RecentSongsTrimToTen` fails: same reason — and no trim logic exists.
- `FavoritesCapAtFive` fails: 6 favorites persist (no cap).
- `FavoritesRemovalWorksWhenAtCap` should pass already (remove path is unchanged); confirm it.

- [ ] **Step 3: Rewrite `UpsertFavoriteAndRecentAsync` to enforce cap + server-tracked recents**

Open `Application/Handlers/UpdatePlayResultCommand.Green.cs`. Replace the existing `UpsertFavoriteAndRecentAsync` method with:

```csharp
private async Task UpsertFavoriteAndRecentAsync(
    uint baid,
    CommonPlayResultData.StageData stage,
    DateTime playTime,
    CancellationToken cancellationToken)
{
    // Favorites: cap at 5 inserts; remove path unchanged.
    var favorite = await context.GreenFavoriteSongs.FindAsync([baid, stage.SongNo], cancellationToken);
    if (stage.IsFavorite && favorite is null)
    {
        var count = await context.GreenFavoriteSongs.CountAsync(s => s.Baid == baid, cancellationToken);
        if (count < 5)
        {
            context.GreenFavoriteSongs.Add(new GreenFavoriteSongs { Baid = baid, SongNo = stage.SongNo });
        }
    }
    else if (!stage.IsFavorite && favorite is not null)
    {
        context.GreenFavoriteSongs.Remove(favorite);
    }

    // Recents: server-tracked. Upsert every played stage. Ignore stage.IsRecent for population.
    var recent = await context.GreenRecentSongs.FindAsync([baid, stage.SongNo], cancellationToken);
    if (recent is null)
    {
        context.GreenRecentSongs.Add(new GreenRecentSongs
        {
            Baid = baid,
            SongNo = stage.SongNo,
            LastPlayed = playTime
        });
    }
    else
    {
        recent.LastPlayed = playTime;
    }

    // Trim to 10 newest. SaveChanges is called once at the end of HandleGreen; queries here
    // see the in-memory tracked entities via the change tracker once SaveChanges flushes.
    // To compute "overage", flush pending recents first so the count is accurate.
    await context.SaveChangesAsync(cancellationToken);

    var overage = await context.GreenRecentSongs
        .Where(s => s.Baid == baid)
        .OrderByDescending(s => s.LastPlayed)
        .Skip(10)
        .ToListAsync(cancellationToken);
    if (overage.Count > 0)
    {
        context.GreenRecentSongs.RemoveRange(overage);
    }
}
```

Note: the intermediate `SaveChangesAsync` call inside `UpsertFavoriteAndRecentAsync` is intentional — it makes the new row visible to the subsequent `Skip(10)` query so the trim logic operates on the post-insert state. The outer `HandleGreen` still calls `SaveChangesAsync` at the end to flush the deletions and the remaining writes (best, save-data updates).

- [ ] **Step 4: Update the call site to pass `playTime`**

Still in `UpdatePlayResultCommand.Green.cs`. Find the call to `UpsertFavoriteAndRecentAsync` inside `SaveStageAsync` (currently `await UpsertFavoriteAndRecentAsync(baid, stage, cancellationToken);`). Change it to:

```csharp
await UpsertFavoriteAndRecentAsync(baid, stage, playTime, cancellationToken);
```

The signature change ripples nowhere else — only `SaveStageAsync` calls this method.

- [ ] **Step 5: Run the tests and verify they pass**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayResultHandlerTests"
```

Expected: all Green play-result handler tests pass, including the four new ones.

- [ ] **Step 6: Commit Task 5**

```bash
git add Application/Handlers/UpdatePlayResultCommand.Green.cs Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "Server-track Green recents and cap favorites at five"
```
