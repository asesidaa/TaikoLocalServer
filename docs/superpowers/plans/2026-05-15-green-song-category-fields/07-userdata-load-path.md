# Task 7: Userdata Load Path

**Goal:** Update `UserDataQuery.Green` to surface cumulative counter columns, populate `SongPushedCnt`, project recommend from the catalog, and order the recent-song response by `LastPlayed`.

**Files:**
- Modify: `Application/Handlers/UserDataQuery.Green.cs`
- Test: `Tests/Green/GreenUserDataMapperTests.cs` OR `Tests/Green/GreenIdentityHandlerTests.cs` (whichever currently houses userdata response assertions — see Step 1)

**Acceptance Criteria:**
- [ ] Userdata response's `SongFavoriteCnt` equals `UserSaveDataGreen.SongFavoriteCnt`, not the `favorites` list length.
- [ ] Userdata response's `SongRecentCnt` equals `UserSaveDataGreen.SongRecentCnt`.
- [ ] Userdata response's `SongPushedCnt` is set from `UserSaveDataGreen.SongPushedCnt` (was never set before).
- [ ] Userdata response's `RecommendSong` / `RecommendBestSong` echo the values from `IGreenCatalog.Recommend`.
- [ ] `AryRecentSongNoes` are ordered by `GreenRecentSongs.LastPlayed DESC` (most recently played first), capped at 10.
- [ ] `AryFavoriteSongNoes` content unchanged from current behavior.

**Verify:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UserData_Green"` → all pass, including new fixtures.

---

- [ ] **Step 1: Locate the userdata test class**

```bash
grep -l "UserDataQueryHandler\|UserData_Green\|UserData_FallsBackToSentinelOneForInvalidDispTaikojukuDan" Tests/Green/
```

The mapper-level tests are in `Tests/Green/GreenUserDataMapperTests.cs`. End-to-end handler tests are in `Tests/Green/GreenIdentityHandlerTests.cs`. Use whichever currently constructs a `UserDataQueryHandler` and asserts on the returned `CommonUserDataResponse`. If neither file matches, search for `UserDataQuery` usages in tests and append to that file. If no test currently exercises the handler end-to-end, add tests to `GreenIdentityHandlerTests.cs` next to the existing `UserData_Green_NewSaveSendsSentinelOneForDispTaikojukuDan` test.

The remainder of this task assumes the new tests go into `GreenIdentityHandlerTests.cs`. Adjust the file path in Step 2 if you targeted a different file.

- [ ] **Step 2: Write the failing tests**

Append these tests to `Tests/Green/GreenIdentityHandlerTests.cs`:

```csharp
[Fact]
public async Task UserData_Green_SongCountersComeFromSaveDataColumns()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.SongPushedCnt = 7;
    save.SongFavoriteCnt = 42;
    save.SongRecentCnt = 99;
    save.CategJpopCnt = 12;
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var handler = new UserDataQueryHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UserDataQueryHandler>.Instance,
        Options.Create(new ServerSettings()));

    var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

    Assert.Equal(7u, response.SongPushedCnt);
    Assert.Equal(42u, response.SongFavoriteCnt);
    Assert.Equal(99u, response.SongRecentCnt);
    Assert.Equal(12u, response.CategJpopCnt);
}

[Fact]
public async Task UserData_Green_RecommendComesFromCatalog()
{
    var greenCatalog = new GreenHandlerFixture.TestGreenCatalog
    {
        Recommend = new GreenRecommendEntry
        {
            RecommendSong = 102,
            RecommendBestSongs = [101, 102, 103]
        }
    };
    await using var fixture = await GreenHandlerFixture.CreateAsync(greenCatalog);
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
    await fixture.Context.SaveChangesAsync();

    var handler = new UserDataQueryHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UserDataQueryHandler>.Instance,
        Options.Create(new ServerSettings()));

    var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

    Assert.Equal(102u, response.RecommendSong);
    Assert.Equal(new List<uint> { 101, 102, 103 }, response.RecommendBestSong);
}

[Fact]
public async Task UserData_Green_RecentsOrderedByLastPlayed()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));

    // Seed three recents with explicit timestamps.
    fixture.Context.GreenRecentSongs.AddRange(
        new GreenRecentSongs { Baid = 1, SongNo = 101, LastPlayed = new DateTime(2026, 5, 15,  9, 0, 0) },
        new GreenRecentSongs { Baid = 1, SongNo = 102, LastPlayed = new DateTime(2026, 5, 15, 12, 0, 0) },
        new GreenRecentSongs { Baid = 1, SongNo = 103, LastPlayed = new DateTime(2026, 5, 15, 11, 0, 0) });
    await fixture.Context.SaveChangesAsync();

    var handler = new UserDataQueryHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UserDataQueryHandler>.Instance,
        Options.Create(new ServerSettings()));

    var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

    Assert.Equal(new uint[] { 102, 103, 101 }, response.AryRecentSongNoes);
}
```

If the existing `UserDataQuery` request type uses different property/constructor syntax (e.g. positional record), inspect `Application/Handlers/UserDataQuery.cs` and adapt the call site. Do NOT invent constructor shapes.

- [ ] **Step 3: Run the tests and verify they fail**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UserData_Green_SongCountersComeFromSaveDataColumns Or FullyQualifiedName~UserData_Green_RecommendComesFromCatalog Or FullyQualifiedName~UserData_Green_RecentsOrderedByLastPlayed"
```

Expected: all three fail. Counters come from list lengths (or 0 for pushed), recommend is empty, recents order by `SongNo`.

- [ ] **Step 4: Update `UserDataQuery.Green.cs`**

Open `Application/Handlers/UserDataQuery.Green.cs`. Apply three changes:

**Change A — recent query ordering.** Find the `recent = await context.GreenRecentSongs ...` block and replace `OrderByDescending(song => song.SongNo)` with `OrderByDescending(song => song.LastPlayed)`. The block becomes:

```csharp
var recent = await context.GreenRecentSongs
    .Where(song => song.Baid == request.Baid)
    .OrderByDescending(song => song.LastPlayed)
    .Select(song => song.SongNo)
    .Take(10)
    .ToArrayAsync(cancellationToken);
```

**Change B — swap counter sources.** Inside the `return new CommonUserDataResponse { ... }` initializer, locate:

```csharp
SongFavoriteCnt = (uint)favorites.Length,
SongRecentCnt = (uint)recent.Length,
```

Replace with:

```csharp
SongFavoriteCnt = saveData.SongFavoriteCnt,
SongRecentCnt = saveData.SongRecentCnt,
SongPushedCnt = saveData.SongPushedCnt,
```

(Insertion of `SongPushedCnt` is new — it was never set before.)

**Change C — wire recommend.** Still in the same response initializer, add:

```csharp
RecommendSong = green.Recommend.RecommendSong,
RecommendBestSong = green.Recommend.RecommendBestSongs.ToList(),
```

These can go anywhere in the initializer; place them next to `CategVocaloidCnt` for proximity. The relevant cluster of fields in the response becomes:

```csharp
CategJpopCnt = saveData.CategJpopCnt,
CategAnimeCnt = saveData.CategAnimeCnt,
CategDoyoCnt = saveData.CategDoyoCnt,
CategVarietyCnt = saveData.CategVarietyCnt,
CategClassicCnt = saveData.CategClassicCnt,
CategGameCnt = saveData.CategGameCnt,
CategNamcoCnt = saveData.CategNamcoCnt,
CategVocaloidCnt = saveData.CategVocaloidCnt,
SongPushedCnt = saveData.SongPushedCnt,
RecommendSong = green.Recommend.RecommendSong,
RecommendBestSong = green.Recommend.RecommendBestSongs.ToList(),
SongFavoriteCnt = saveData.SongFavoriteCnt,
SongRecentCnt = saveData.SongRecentCnt,
TotalCreditCnt = saveData.TotalCreditCnt,
```

Leave the `favorites` and `recent` local variables intact — they still populate `AryFavoriteSongNoes` and `AryRecentSongNoes`. Only the count fields move to save-data columns.

- [ ] **Step 5: Run the tests and verify they pass**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UserData_Green"
```

Expected: all three new tests PASS, plus any pre-existing `UserData_Green_*` tests still pass.

- [ ] **Step 6: Run the full Green test suite for regression**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Green"
```

Expected: every Green test passes. If a pre-existing test asserted that `SongFavoriteCnt` equalled `favorites.Length`, it must be updated to assert on the save-data column or recharacterized — that test's premise is what this task fixes. Fix the assertion before moving on.

- [ ] **Step 7: Commit Task 7**

```bash
git add Application/Handlers/UserDataQuery.Green.cs Tests/Green/
git commit -m "Read Green userdata counters from save data and project recommend"
```
