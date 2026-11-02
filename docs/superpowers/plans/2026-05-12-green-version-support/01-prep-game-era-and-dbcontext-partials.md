# 01 — Preparation: `GameEra` enum and `TaikoDbContext` partial split

**Surface:** Pure refactor + new `GameEra` enum. No schema changes, no functional behavior changes. Each task compiles cleanly and ships as its own commit.

**Why this comes first:** All downstream files reference `GameEra`. The DbContext partial split is a no-op refactor that makes Task 02 cleaner (small additive edits per era partial instead of one large central diff).

**Verification cadence:** `dotnet build` after each task — must succeed with zero new warnings. No EF migrations generated yet (schema unchanged).

---

## Task 01.1: Add `GameEra` enum

**Goal:** Single source of truth for the era discriminator at the Domain layer.

**Files:**
- Create: `Domain/Enums/GameEra.cs`

**Acceptance Criteria:**
- [ ] `Domain/Enums/GameEra.cs` exists with two values (`Nijiiro = 0`, `Green = 1`).
- [ ] `dotnet build Domain/Domain.csproj` succeeds.
- [ ] Solution-wide `dotnet build` succeeds (no consumers yet — additive only).

**Verify:**
```bash
dotnet build
```
Expected: build succeeds; no errors, no warnings caused by this change.

**Steps:**

- [ ] **Step 1: Create the enum file**

```csharp
// Domain/Enums/GameEra.cs
namespace TaikoLocalServer.Domain.Enums;

public enum GameEra
{
    Nijiiro = 0,
    Green   = 1
}
```

- [ ] **Step 2: Build**

Run: `dotnet build`
Expected: PASS, zero new warnings.

- [ ] **Step 3: Commit**

```bash
git add Domain/Enums/GameEra.cs
git commit -m "feat(domain): add GameEra enum"
```

---

## Task 01.2: Split `ITaikoDbContext` into partial interface files

**Goal:** Refactor `ITaikoDbContext` from one file into three partial files (`Shared`, `Nijiiro`). No DbSet additions, no removals — just relocate existing members.

**Files:**
- Delete: `Application/Abstractions/ITaikoDbContext.cs` (content moves into the three partials below)
- Create: `Application/Abstractions/ITaikoDbContext.cs` (slim — base partial with shared method signatures)
- Create: `Application/Abstractions/ITaikoDbContext.Shared.cs`
- Create: `Application/Abstractions/ITaikoDbContext.Nijiiro.cs`

**Acceptance Criteria:**
- [ ] `ITaikoDbContext` is declared `partial interface` in all three files.
- [ ] `Application/Abstractions/ITaikoDbContext.cs` contains only the `SaveChangesAsync` member.
- [ ] `Application/Abstractions/ITaikoDbContext.Shared.cs` contains `Cards`, `Credentials`, `UserData`, `Tokens` DbSets.
- [ ] `Application/Abstractions/ITaikoDbContext.Nijiiro.cs` contains `SongBestData`, `SongPlayData`, `DanScoreData`, `DanStageScoreData`, `AiScoreData`, `AiSectionScoreData` DbSets (names unchanged for now — renames come in Task 02).
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
```
Expected: PASS, zero new warnings. Existing consumers (`Adapters.AdminApi`, `Application.Handlers`, etc.) keep compiling because property names and types are unchanged.

**Steps:**

- [ ] **Step 1: Rewrite `Application/Abstractions/ITaikoDbContext.cs` as the base partial**

```csharp
// Application/Abstractions/ITaikoDbContext.cs
namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Create `Application/Abstractions/ITaikoDbContext.Shared.cs`**

```csharp
// Application/Abstractions/ITaikoDbContext.Shared.cs
namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<UserDatum>   UserData     { get; }
    DbSet<Card>        Cards        { get; }
    DbSet<Credential>  Credentials  { get; }
    DbSet<Token>       Tokens       { get; }
}
```

- [ ] **Step 3: Create `Application/Abstractions/ITaikoDbContext.Nijiiro.cs`**

```csharp
// Application/Abstractions/ITaikoDbContext.Nijiiro.cs
namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<SongBestDatum>          SongBestData          { get; }
    DbSet<SongPlayDatum>          SongPlayData          { get; }
    DbSet<DanScoreDatum>          DanScoreData          { get; }
    DbSet<DanStageScoreDatum>     DanStageScoreData     { get; }
    DbSet<AiScoreDatum>           AiScoreData           { get; }
    DbSet<AiSectionScoreDatum>    AiSectionScoreData    { get; }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build`
Expected: PASS — existing consumers still bind to the same DbSet property names through the partial-interface composition.

- [ ] **Step 5: Commit**

```bash
git add Application/Abstractions/ITaikoDbContext.cs Application/Abstractions/ITaikoDbContext.Shared.cs Application/Abstractions/ITaikoDbContext.Nijiiro.cs
git commit -m "refactor(app): split ITaikoDbContext into partial interface files (Shared, Nijiiro)"
```

---

## Task 01.3: Split `TaikoDbContext` implementation into partial class files

**Goal:** Refactor `TaikoDbContext` from one file into a central file + per-area partial files. `OnModelCreating` becomes a dispatcher to per-area `partial void` hooks. No DbSet changes, no `ModelBuilder` configuration changes.

**Files:**
- Modify: `Infrastructure/Persistence/TaikoDbContext.cs` (keep ctor, OnConfiguring; rewrite OnModelCreating to call partial hooks; remove DbSet declarations and OnModelCreating bodies — they move to the per-area partials)
- Create: `Infrastructure/Persistence/TaikoDbContext.Shared.cs`
- Create: `Infrastructure/Persistence/TaikoDbContext.Nijiiro.cs`
- Keep as-is: `Infrastructure/Persistence/TaikoDbContextPartial.cs` (existing — `OnModelCreatingPartial` hook stays wired)

**Acceptance Criteria:**
- [ ] `TaikoDbContext` is declared `partial class` in all three files.
- [ ] Central `TaikoDbContext.cs` `OnModelCreating` calls in order: `OnModelCreatingShared(b)`, `OnModelCreatingNijiiro(b)`, `OnModelCreatingPartial(b)`.
- [ ] Central file declares the partial hooks: `partial void OnModelCreatingShared(ModelBuilder b)` and `partial void OnModelCreatingNijiiro(ModelBuilder b)`.
- [ ] `TaikoDbContext.Shared.cs` holds `Cards`, `Credentials`, `UserData`, `Tokens` DbSets + their `OnModelCreatingShared` configuration block (the existing `Card`, `Credential`, `UserDatum` entity mappings from the original `OnModelCreating`).
- [ ] `TaikoDbContext.Nijiiro.cs` holds `SongBestData`, `SongPlayData`, `DanScoreData`, `DanStageScoreData`, `AiScoreData`, `AiSectionScoreData` DbSets + their `OnModelCreatingNijiiro` configuration block.
- [ ] `dotnet build` succeeds.
- [ ] `dotnet ef migrations add NoOpAfterPartialSplit --project Infrastructure --startup-project Host --dry-run` reports **no model changes** (validates the refactor produced the same model as before).

**Verify:**
```bash
dotnet build
dotnet ef migrations script --project Infrastructure --startup-project Host --idempotent --output /tmp/before.sql 2>/dev/null || true
# After applying:
dotnet ef migrations add NoOpAfterPartialSplit --project Infrastructure --startup-project Host --dry-run
```
Expected: build PASSES; dry-run reports **no model changes detected**. If model changes are reported, the refactor accidentally altered the EF model — investigate before continuing.

**Steps:**

- [ ] **Step 1: Rewrite `Infrastructure/Persistence/TaikoDbContext.cs` as the central partial**

```csharp
// Infrastructure/Persistence/TaikoDbContext.cs
using EntityFramework.Exceptions.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext : DbContext
{
    private string? dbFilePath;

    public TaikoDbContext() { }
    public TaikoDbContext(DbContextOptions<TaikoDbContext> options) : base(options) { }
    public TaikoDbContext(string dbFilePath) { this.dbFilePath = dbFilePath; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        var path = Path.Combine(PathHelper.GetRootPath(), "taiko.db3");
        if (dbFilePath is not null) path = dbFilePath;
        optionsBuilder.UseSqlite($"Data Source={path}");
        optionsBuilder.UseExceptionProcessor().EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingShared(modelBuilder);
        OnModelCreatingNijiiro(modelBuilder);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingShared(ModelBuilder modelBuilder);
    partial void OnModelCreatingNijiiro(ModelBuilder modelBuilder);
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
```

- [ ] **Step 2: Create `Infrastructure/Persistence/TaikoDbContext.Shared.cs`**

Move the existing `Card`, `Credential`, `UserDatum` DbSets and their corresponding `modelBuilder.Entity<...>` blocks from the old `OnModelCreating` into the new partial.

```csharp
// Infrastructure/Persistence/TaikoDbContext.Shared.cs
using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<Card>        Cards        { get; set; } = null!;
    public virtual DbSet<Credential>  Credentials  { get; set; } = null!;
    public virtual DbSet<UserDatum>   UserData     { get; set; } = null!;
    // Tokens is registered via UserDatum's owned-type / list configuration today;
    // if Tokens has its own DbSet elsewhere, lift the declaration here.

    partial void OnModelCreatingShared(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.AccessCode);
            entity.ToTable("Card");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Credential>(entity =>
        {
            entity.HasKey(e => e.Baid);
            entity.ToTable("Credential");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserDatum>(entity =>
        {
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.Property(e => e.AchievementDisplayDifficulty).HasConversion<uint>();
        });
    }
}
```

> NOTE: If `TaikoDbContextPartial.cs` already declares `Tokens` and other entity configuration via the `OnModelCreatingPartial` hook, leave it untouched. The goal here is to **move** what's currently in the main `OnModelCreating` into `OnModelCreatingShared` and `OnModelCreatingNijiiro`, not to redistribute what's already in the partial.

- [ ] **Step 3: Create `Infrastructure/Persistence/TaikoDbContext.Nijiiro.cs`**

Move the `SongBestDatum`, `SongPlayDatum` DbSets + entity configuration. If `DanScoreData` / `DanStageScoreData` / `AiScoreData` / `AiSectionScoreData` are configured via `TaikoDbContextPartial.cs`, leave them alone for now — Task 02 will lift them here when it renames them.

```csharp
// Infrastructure/Persistence/TaikoDbContext.Nijiiro.cs
using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<SongBestDatum>  SongBestData  { get; set; } = null!;
    public virtual DbSet<SongPlayDatum>  SongPlayData  { get; set; } = null!;
    // DanScoreData / DanStageScoreData / AiScoreData / AiSectionScoreData
    // remain configured by TaikoDbContextPartial.cs (existing). They'll be
    // renamed to *_Nijiiro suffix in Task 02 — at which point their DbSet
    // declarations should be lifted here for consistency with the partial pattern.

    partial void OnModelCreatingNijiiro(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SongBestDatum>(entity =>
        {
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.BestCrown).HasConversion<uint>();
            entity.Property(e => e.BestScoreRank).HasConversion<uint>();
        });

        modelBuilder.Entity<SongPlayDatum>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PlayTime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.ScoreRank).HasConversion<uint>();
            entity.Property(e => e.Crown).HasConversion<uint>();
        });
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build`
Expected: PASS, zero new warnings.

- [ ] **Step 5: Verify the EF model is unchanged**

Run:
```bash
dotnet ef migrations add NoOpAfterPartialSplit --project Infrastructure --startup-project Host
```
Expected output includes: `No changes to the model were detected.` (No new migration file is created.) If a migration file IS generated, open it — anything inside means the refactor accidentally altered the model. Revert and investigate.

If a migration file was created accidentally, remove it:
```bash
dotnet ef migrations remove --project Infrastructure --startup-project Host
```

- [ ] **Step 6: Commit**

```bash
git add Infrastructure/Persistence/TaikoDbContext.cs Infrastructure/Persistence/TaikoDbContext.Shared.cs Infrastructure/Persistence/TaikoDbContext.Nijiiro.cs
git commit -m "refactor(infra): split TaikoDbContext into partial class files (Shared, Nijiiro)"
```
