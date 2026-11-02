# 03 - Season State Persistence

**Goal:** Add EF entities and helpers for season-scoped Don medal totals and item purchase state.

**Files:**

- Create: `Domain/Enums/GreenShopItemStatus.cs`
- Create: `Domain/Entities/GreenShopSeasonState.cs`
- Create: `Domain/Entities/GreenShopItemState.cs`
- Modify: `Application/Abstractions/ITaikoDbContext.Green.cs`
- Modify: `Infrastructure/Persistence/TaikoDbContext.Green.cs`
- Create: `Application/Common/GreenShopStateExtensions.cs`
- Create: `Tests/Green/GreenItemShopStateTests.cs`
- Generate: `Infrastructure/Persistence/Migrations/<timestamp>_AddGreenItemShopState.cs`
- Generate: `Infrastructure/Persistence/Migrations/<timestamp>_AddGreenItemShopState.Designer.cs`
- Modify: `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs`

## Acceptance Criteria

- [ ] EF model contains `GreenShopSeasonState` keyed by `(Baid, SeasonId)`.
- [ ] EF model contains `GreenShopItemState` keyed by `(Baid, SeasonId, ItemType, ItemId)`.
- [ ] First-ever shop season state seeds from existing global Green Don medal totals.
- [ ] Later seasons start from zero.
- [ ] Migration creates both tables with cascade delete to `UserDatum`.

## Steps

- [ ] **Step 1: Add failing state tests**

Create `Tests/Green/GreenItemShopStateTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class GreenItemShopStateTests
{
    [Fact]
    public async Task GetOrCreateGreenShopSeasonState_FirstSeasonSeedsFromGlobalMedals()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 100;
        save.TotalUseDonmedal = 40;
        fixture.Context.UserSaveDataGreen.Add(save);
        await fixture.Context.SaveChangesAsync();

        var state = await fixture.Context.GetOrCreateGreenShopSeasonStateAsync(save, 2, CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Equal(2u, state.SeasonId);
        Assert.Equal(100u, state.TotalGetDonmedal);
        Assert.Equal(40u, state.TotalUseDonmedal);
    }

    [Fact]
    public async Task GetOrCreateGreenShopSeasonState_LaterSeasonStartsAtZero()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.TotalGetDonmedal = 100;
        save.TotalUseDonmedal = 40;
        fixture.Context.UserSaveDataGreen.Add(save);
        fixture.Context.GreenShopSeasonStates.Add(new GreenShopSeasonState
        {
            Baid = 1,
            SeasonId = 1,
            TotalGetDonmedal = 100,
            TotalUseDonmedal = 40,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var state = await fixture.Context.GetOrCreateGreenShopSeasonStateAsync(save, 2, CancellationToken.None);
        await fixture.Context.SaveChangesAsync();

        Assert.Equal(0u, state.TotalGetDonmedal);
        Assert.Equal(0u, state.TotalUseDonmedal);
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopStateTests"
```

Expected: fails because entities, DbSets, and helper do not exist.

- [ ] **Step 3: Add status enum**

Create `Domain/Enums/GreenShopItemStatus.cs`:

```csharp
namespace TaikoLocalServer.Domain.Enums;

public enum GreenShopItemStatus : uint
{
    PendingReward = 1,
    Unlocked = 2
}
```

- [ ] **Step 4: Add season entity**

Create `Domain/Entities/GreenShopSeasonState.cs`:

```csharp
namespace TaikoLocalServer.Domain.Entities;

public sealed class GreenShopSeasonState
{
    public uint Baid { get; set; }

    public uint SeasonId { get; set; }

    public uint TotalGetDonmedal { get; set; }

    public uint TotalUseDonmedal { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public UserDatum? Ba { get; set; }
}
```

- [ ] **Step 5: Add item entity**

Create `Domain/Entities/GreenShopItemState.cs`:

```csharp
namespace TaikoLocalServer.Domain.Entities;

public sealed class GreenShopItemState
{
    public uint Baid { get; set; }

    public uint SeasonId { get; set; }

    public uint ItemType { get; set; }

    public uint ItemId { get; set; }

    public uint ItemNo { get; set; }

    public uint ItemPrice { get; set; }

    public GreenShopItemStatus Status { get; set; }

    public DateTime PurchasedAt { get; set; }

    public DateTime? UnlockedAt { get; set; }

    public UserDatum? Ba { get; set; }
}
```

- [ ] **Step 6: Add DbSets**

Modify `Application/Abstractions/ITaikoDbContext.Green.cs`:

```csharp
DbSet<GreenShopSeasonState> GreenShopSeasonStates { get; }
DbSet<GreenShopItemState> GreenShopItemStates { get; }
```

Modify `Infrastructure/Persistence/TaikoDbContext.Green.cs`:

```csharp
public virtual DbSet<GreenShopSeasonState> GreenShopSeasonStates { get; set; } = null!;
public virtual DbSet<GreenShopItemState> GreenShopItemStates { get; set; } = null!;
```

- [ ] **Step 7: Configure EF model**

Add these blocks inside `OnModelCreatingGreen`:

```csharp
modelBuilder.Entity<GreenShopSeasonState>(entity =>
{
    entity.ToTable("GreenShopSeasonStates");
    entity.HasKey(e => new { e.Baid, e.SeasonId });
    entity.Property(e => e.CreatedAt).HasColumnType("datetime");
    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
    entity.HasOne(d => d.Ba)
        .WithMany()
        .HasPrincipalKey(p => p.Baid)
        .HasForeignKey(d => d.Baid)
        .OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<GreenShopItemState>(entity =>
{
    entity.ToTable("GreenShopItemStates");
    entity.HasKey(e => new { e.Baid, e.SeasonId, e.ItemType, e.ItemId });
    entity.Property(e => e.Status).HasConversion<uint>();
    entity.Property(e => e.PurchasedAt).HasColumnType("datetime");
    entity.Property(e => e.UnlockedAt).HasColumnType("datetime");
    entity.HasOne(d => d.Ba)
        .WithMany()
        .HasPrincipalKey(p => p.Baid)
        .HasForeignKey(d => d.Baid)
        .OnDelete(DeleteBehavior.Cascade);
});
```

- [ ] **Step 8: Add season state helper**

Create `Application/Common/GreenShopStateExtensions.cs`:

```csharp
namespace TaikoLocalServer.Application.Common;

public static class GreenShopStateExtensions
{
    public static async ValueTask<GreenShopSeasonState> GetOrCreateGreenShopSeasonStateAsync(
        this ITaikoDbContext context,
        UserSaveDataGreen saveData,
        uint seasonId,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.GreenShopSeasonStates.FindAsync([saveData.Baid, seasonId], cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var hasAnyShopState = await context.GreenShopSeasonStates
            .AnyAsync(row => row.Baid == saveData.Baid, cancellationToken);
        var now = DateTime.UtcNow;
        var state = new GreenShopSeasonState
        {
            Baid = saveData.Baid,
            SeasonId = seasonId,
            TotalGetDonmedal = hasAnyShopState ? 0 : saveData.TotalGetDonmedal,
            TotalUseDonmedal = hasAnyShopState ? 0 : saveData.TotalUseDonmedal,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.GreenShopSeasonStates.Add(state);
        return state;
    }
}
```

- [ ] **Step 9: Generate migration**

Run:

```powershell
dotnet ef migrations add AddGreenItemShopState --project Infrastructure --startup-project Host
```

Expected: EF creates a migration pair under `Infrastructure/Persistence/Migrations/` and updates `TaikoDbContextModelSnapshot.cs`.

- [ ] **Step 10: Inspect migration**

Open the generated migration and verify `Up()` contains two `CreateTable` calls:

```csharp
migrationBuilder.CreateTable(
    name: "GreenShopItemStates",
```

```csharp
migrationBuilder.CreateTable(
    name: "GreenShopSeasonStates",
```

Verify `Down()` drops both tables.

- [ ] **Step 11: Run focused tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShopStateTests"
```

Expected: pass.

- [ ] **Step 12: Commit**

```powershell
git status --short
git add -- Domain/Enums/GreenShopItemStatus.cs Domain/Entities/GreenShopSeasonState.cs Domain/Entities/GreenShopItemState.cs Application/Abstractions/ITaikoDbContext.Green.cs Infrastructure/Persistence/TaikoDbContext.Green.cs Application/Common/GreenShopStateExtensions.cs Tests/Green/GreenItemShopStateTests.cs Infrastructure/Persistence/Migrations
git commit -m "Add Green item shop season state storage"
```

