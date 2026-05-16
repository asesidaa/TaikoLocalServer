# Task 2: Schema, Entities, and EF Migration

**Goal:** Add `IsPushed` to `SongPlayDatumGreen` and `LastPlayed` to `GreenRecentSongs`, then generate the EF Core migration.

**Files:**
- Modify: `Domain/Entities/SongPlayDatumGreen.cs`
- Modify: `Domain/Entities/GreenRecentSongs.cs`
- Modify: `Infrastructure/Persistence/TaikoDbContext.Green.cs`
- Create: `Infrastructure/Persistence/Migrations/<timestamp>_AddGreenSongCategoryFields.cs` (generated)
- Create: `Infrastructure/Persistence/Migrations/<timestamp>_AddGreenSongCategoryFields.Designer.cs` (generated)
- Modify: `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` (generated update)
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`

**Acceptance Criteria:**
- [ ] `SongPlayDatumGreen` has a `bool IsPushed { get; set; }` property.
- [ ] `GreenRecentSongs` has a `DateTime LastPlayed { get; set; }` property.
- [ ] EF migration adds both columns with appropriate defaults (IsPushed → 0; LastPlayed → '1970-01-01 00:00:00').
- [ ] New schema smoke test inserts and reads back a SongPlayDatumGreen row with `IsPushed=true` and a GreenRecentSongs row with `LastPlayed=2026-05-15T12:00:00`.

**Verify:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayLog_StoresIsPushed Or FullyQualifiedName~GreenRecentSongs_StoresLastPlayed"` → both pass.

---

- [ ] **Step 1: Write the failing schema smoke tests**

Append both tests to `Tests/Green/GreenPlayResultHandlerTests.cs`, near the existing schema tests:

```csharp
[Fact]
public async Task GreenPlayLog_StoresIsPushed()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    await fixture.Context.SaveChangesAsync();

    fixture.Context.SongPlayDataGreen.Add(new SongPlayDatumGreen
    {
        Baid = 1,
        SongId = 101,
        Difficulty = Difficulty.Easy,
        Crown = CrownType.Clear,
        Score = 100000,
        ScoreRate = 80,
        GoodCount = 10,
        OkCount = 2,
        MissCount = 1,
        ComboCount = 12,
        HitCount = 13,
        PoundCount = 0,
        StarLevel = 3,
        SupportLevel = 0,
        OptionFlg = [0],
        ToneFlg = [0],
        PlayMode = 0,
        StageMode = 0,
        IsShin = false,
        MusicCategory = 0,
        SelectedFolderId = 0,
        IsFavorite = false,
        IsRecent = false,
        IsPapamama = false,
        IsPushed = true,
        SoulGauge = 100,
        PlayDan = 0,
        WaiwaiResult = 0,
        WaiwaiGauge = 0,
        PlayTime = new DateTime(2026, 5, 15, 12, 0, 0)
    });
    await fixture.Context.SaveChangesAsync();

    var saved = await fixture.Context.SongPlayDataGreen.SingleAsync(row => row.Baid == 1);
    Assert.True(saved.IsPushed);
}

[Fact]
public async Task GreenRecentSongs_StoresLastPlayed()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    await fixture.Context.SaveChangesAsync();

    var when = new DateTime(2026, 5, 15, 12, 0, 0);
    fixture.Context.GreenRecentSongs.Add(new GreenRecentSongs
    {
        Baid = 1,
        SongNo = 101,
        LastPlayed = when
    });
    await fixture.Context.SaveChangesAsync();

    var saved = await fixture.Context.GreenRecentSongs.SingleAsync(row => row.Baid == 1);
    Assert.Equal(when, saved.LastPlayed);
}
```

- [ ] **Step 2: Run the tests and verify they fail**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayLog_StoresIsPushed Or FullyQualifiedName~GreenRecentSongs_StoresLastPlayed"
```

Expected: COMPILE ERROR — `SongPlayDatumGreen.IsPushed` and `GreenRecentSongs.LastPlayed` do not exist.

- [ ] **Step 3: Add IsPushed to the play-log entity**

Open `Domain/Entities/SongPlayDatumGreen.cs`. Add `IsPushed` right after the existing `IsPapamama` line. The relevant section becomes:

```csharp
public bool IsFavorite { get; set; }
public bool IsRecent { get; set; }
public bool IsPapamama { get; set; }
public bool IsPushed { get; set; }
public uint SoulGauge { get; set; }
```

- [ ] **Step 4: Add LastPlayed to the recent-songs entity**

Open `Domain/Entities/GreenRecentSongs.cs`. The full updated file:

```csharp
namespace TaikoLocalServer.Domain.Entities;

public partial class GreenRecentSongs
{
    public uint Baid { get; set; }
    public uint SongNo { get; set; }
    public DateTime LastPlayed { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

- [ ] **Step 5: Map LastPlayed as a SQLite `datetime` column**

Open `Infrastructure/Persistence/TaikoDbContext.Green.cs`. Find the `modelBuilder.Entity<GreenRecentSongs>(...)` block (currently the last entity in `OnModelCreatingGreen`). Add a `Property` call inside it so the body reads:

```csharp
modelBuilder.Entity<GreenRecentSongs>(entity =>
{
    entity.ToTable("GreenRecentSongs");
    entity.HasKey(e => new { e.Baid, e.SongNo });
    entity.Property(e => e.LastPlayed).HasColumnType("datetime");
    entity.HasOne(d => d.Ba)
        .WithMany()
        .HasPrincipalKey(p => p.Baid)
        .HasForeignKey(d => d.Baid)
        .OnDelete(DeleteBehavior.Cascade);
});
```

No EF mapping changes are needed for `SongPlayDatumGreen.IsPushed` — `bool` columns are auto-mapped to `INTEGER`.

- [ ] **Step 6: Generate the EF migration**

From the repo root, run:

```bash
dotnet ef migrations add AddGreenSongCategoryFields --project Infrastructure --startup-project Host
```

Expected: EF writes three files into `Infrastructure/Persistence/Migrations/`:
- `<timestamp>_AddGreenSongCategoryFields.cs`
- `<timestamp>_AddGreenSongCategoryFields.Designer.cs`
- (updates) `TaikoDbContextModelSnapshot.cs`

- [ ] **Step 7: Verify the generated Up()**

Open the new `<timestamp>_AddGreenSongCategoryFields.cs`. The `Up()` body should contain (in any order):

```csharp
migrationBuilder.AddColumn<bool>(
    name: "IsPushed",
    table: "SongPlayDatum_Green",
    type: "INTEGER",
    nullable: false,
    defaultValue: false);

migrationBuilder.AddColumn<DateTime>(
    name: "LastPlayed",
    table: "GreenRecentSongs",
    type: "datetime",
    nullable: false,
    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
```

If EF chose a different default for `LastPlayed` (e.g. `DateTime.MinValue`), that's acceptable — old rows just sort to the bottom of the recent-folder list. Do NOT manually rewrite the generated migration unless it's missing one of the two columns.

If a column is missing, delete the generated migration files and re-run Step 6 after double-checking the entity edits.

- [ ] **Step 8: Run the schema smoke tests and verify they pass**

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenPlayLog_StoresIsPushed Or FullyQualifiedName~GreenRecentSongs_StoresLastPlayed"
```

Expected: both PASS.

- [ ] **Step 9: Commit Task 2**

```bash
git add Domain/Entities/SongPlayDatumGreen.cs Domain/Entities/GreenRecentSongs.cs Infrastructure/Persistence/TaikoDbContext.Green.cs Infrastructure/Persistence/Migrations/ Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "Add Green IsPushed and LastPlayed schema"
```
