# Task 1: Schema and DbContext

**Files:**
- Create: `Domain/Enums/GreenDanClearGrade.cs`
- Create: `Domain/Entities/DanScoreDatumGreen.cs`
- Create: `Domain/Entities/DanStageScoreDatumGreen.cs`
- Modify: `Application/Abstractions/ITaikoDbContext.Green.cs`
- Modify: `Infrastructure/Persistence/TaikoDbContext.Green.cs`
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`

- [ ] **Step 1: Write the failing schema smoke test**

Add this test to `Tests/Green/GreenPlayResultHandlerTests.cs` near the other Green persistence tests:

```csharp
[Fact]
public async Task GreenDanSchema_CanInsertParentAndStageRows()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    await fixture.Context.SaveChangesAsync();

    fixture.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
    {
        Baid = 1,
        DanId = 1,
        IsExtra = false,
        MedleyUniqueId = 20001,
        ClearGrade = GreenDanClearGrade.GoldClear,
        ArrivalSongCount = 1,
        SoulGaugeTotal = 100,
        ComboCountTotal = 138,
        DanStageScoreData =
        [
            new DanStageScoreDatumGreen
            {
                Baid = 1,
                DanId = 1,
                IsExtra = false,
                StageIndex = 0,
                SongNumber = 790,
                PlayScore = 326090,
                HighScore = 326090,
                GoodCount = 124,
                OkCount = 14,
                BadCount = 0,
                DrumrollCount = 139,
                TotalHitCount = 277,
                ComboCount = 138
            }
        ]
    });

    await fixture.Context.SaveChangesAsync();

    var saved = await fixture.Context.DanScoreDataGreen
        .Include(row => row.DanStageScoreData)
        .SingleAsync(row => row.Baid == 1 && row.DanId == 1 && !row.IsExtra);

    Assert.Equal(GreenDanClearGrade.GoldClear, saved.ClearGrade);
    var stage = Assert.Single(saved.DanStageScoreData);
    Assert.Equal(0u, stage.StageIndex);
    Assert.Equal(790u, stage.SongNumber);
}
```

- [ ] **Step 2: Run the test and verify it fails**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenDanSchema_CanInsertParentAndStageRows"
```

Expected: compile failure because `DanScoreDatumGreen`, `DanStageScoreDatumGreen`, `GreenDanClearGrade`, and `DanScoreDataGreen` do not exist.

- [ ] **Step 3: Add the Green clear grade enum**

Create `Domain/Enums/GreenDanClearGrade.cs`:

```csharp
namespace TaikoLocalServer.Domain.Enums;

public enum GreenDanClearGrade : uint
{
    NotClear = 0,
    NormalClear = 1,
    GoldClear = 2
}
```

- [ ] **Step 4: Add the Green Dan entities**

Create `Domain/Entities/DanScoreDatumGreen.cs`:

```csharp
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public class DanScoreDatumGreen
{
    public uint Baid { get; set; }
    public uint DanId { get; set; }
    public bool IsExtra { get; set; }
    public uint MedleyUniqueId { get; set; }
    public uint ArrivalSongCount { get; set; }
    public uint SoulGaugeTotal { get; set; }
    public uint ComboCountTotal { get; set; }
    public GreenDanClearGrade ClearGrade { get; set; }
    public List<DanStageScoreDatumGreen> DanStageScoreData { get; set; } = [];

    public virtual UserDatum? Ba { get; set; }
}
```

Create `Domain/Entities/DanStageScoreDatumGreen.cs`:

```csharp
namespace TaikoLocalServer.Domain.Entities;

public class DanStageScoreDatumGreen
{
    public uint Baid { get; set; }
    public uint DanId { get; set; }
    public bool IsExtra { get; set; }
    public uint StageIndex { get; set; }
    public uint SongNumber { get; set; }
    public uint PlayScore { get; set; }
    public uint GoodCount { get; set; }
    public uint OkCount { get; set; }
    public uint BadCount { get; set; }
    public uint DrumrollCount { get; set; }
    public uint TotalHitCount { get; set; }
    public uint ComboCount { get; set; }
    public uint HighScore { get; set; }

    public DanScoreDatumGreen Parent { get; set; } = null!;
}
```

- [ ] **Step 5: Expose DbSets through the app abstraction and DbContext**

Add these properties to `Application/Abstractions/ITaikoDbContext.Green.cs`:

```csharp
DbSet<DanScoreDatumGreen> DanScoreDataGreen { get; }
DbSet<DanStageScoreDatumGreen> DanStageScoreDataGreen { get; }
```

Add these properties to `Infrastructure/Persistence/TaikoDbContext.Green.cs` with the other Green DbSets:

```csharp
public virtual DbSet<DanScoreDatumGreen> DanScoreDataGreen { get; set; } = null!;
public virtual DbSet<DanStageScoreDatumGreen> DanStageScoreDataGreen { get; set; } = null!;
```

- [ ] **Step 6: Configure EF mappings**

In `Infrastructure/Persistence/TaikoDbContext.Green.cs`, add `using TaikoLocalServer.Domain.Enums;` and add these mappings inside `OnModelCreatingGreen` after `UserSaveDataGreen`:

```csharp
modelBuilder.Entity<DanScoreDatumGreen>(entity =>
{
    entity.ToTable("DanScoreDatum_Green");
    entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra });

    entity.HasOne(d => d.Ba)
        .WithMany()
        .HasPrincipalKey(p => p.Baid)
        .HasForeignKey(d => d.Baid)
        .OnDelete(DeleteBehavior.Cascade);

    entity.Property(e => e.ClearGrade)
        .HasConversion<uint>()
        .HasDefaultValue(GreenDanClearGrade.NotClear);
});

modelBuilder.Entity<DanStageScoreDatumGreen>(entity =>
{
    entity.ToTable("DanStageScoreDatum_Green");
    entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra, e.StageIndex });

    entity.HasOne(d => d.Parent)
        .WithMany(p => p.DanStageScoreData)
        .HasForeignKey(d => new { d.Baid, d.DanId, d.IsExtra })
        .OnDelete(DeleteBehavior.Cascade);
});
```

- [ ] **Step 7: Run the schema smoke test and verify it passes**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenDanSchema_CanInsertParentAndStageRows"
```

Expected: PASS.

- [ ] **Step 8: Commit Task 1**

```powershell
git add Domain/Enums/GreenDanClearGrade.cs Domain/Entities/DanScoreDatumGreen.cs Domain/Entities/DanStageScoreDatumGreen.cs Application/Abstractions/ITaikoDbContext.Green.cs Infrastructure/Persistence/TaikoDbContext.Green.cs Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "Add Green Dan score schema"
```

