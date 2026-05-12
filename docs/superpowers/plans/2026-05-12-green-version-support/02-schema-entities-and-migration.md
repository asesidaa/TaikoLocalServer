# 02 — Schema, entities, and the `AddGreenEraSupport` EF migration

**Surface:** Define all new entity types (Nijiiro renames + Green new), wire them through the DbContext partials, slim `UserDatum`, generate the single EF migration.

**Why this comes second:** The DbContext partial scaffolding from 01 is now in place. Schema changes drop cleanly into the per-era partial files without touching the central `TaikoDbContext.cs`.

**Verification cadence:** `dotnet build` after each task. After the migration is generated (Task 02.7), inspect the generated SQL with `dotnet ef migrations script` and confirm the operations match what the spec §9 expects.

---

## Task 02.1: Add Green entity classes (new types only — not yet wired into DbContext)

**Goal:** Create the C# entity types that will back the new Green tables. No DbContext changes yet — these files compile but are unused until Task 02.3.

**Files (all new):**
- Create: `Domain/Entities/SongBestDatumGreen.cs`
- Create: `Domain/Entities/SongPlayDatumGreen.cs`
- Create: `Domain/Entities/UserSaveDataGreen.cs`
- Create: `Domain/Entities/GhostStageSectionDatumGreen.cs`
- Create: `Domain/Entities/GreenGhostWinnings.cs`
- Create: `Domain/Entities/GreenGhostTokens.cs`
- Create: `Domain/Entities/GreenFriends.cs`
- Create: `Domain/Entities/GreenFavoriteSongs.cs`
- Create: `Domain/Entities/GreenRecentSongs.cs`

**Acceptance Criteria:**
- [ ] All nine files exist.
- [ ] All types are in the `TaikoLocalServer.Domain.Entities` namespace.
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
```
Expected: PASS, zero new warnings.

**Steps:**

- [ ] **Step 1: `SongBestDatumGreen.cs`** — mirrors `SongBestDatum.cs` minus `BestScoreRank` (Green has no score rank).

```csharp
// Domain/Entities/SongBestDatumGreen.cs
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public partial class SongBestDatumGreen
{
    public uint Baid { get; set; }
    public uint SongId { get; set; }
    public Difficulty Difficulty { get; set; }
    public uint BestScore { get; set; }
    public uint BestRate { get; set; }
    public CrownType BestCrown { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

- [ ] **Step 2: `SongPlayDatumGreen.cs`** — Green's per-play columns.

```csharp
// Domain/Entities/SongPlayDatumGreen.cs
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public partial class SongPlayDatumGreen
{
    public long       Id              { get; set; }
    public uint       Baid            { get; set; }
    public uint       SongId          { get; set; }
    public Difficulty Difficulty      { get; set; }
    public CrownType  Crown           { get; set; }
    public uint       Score           { get; set; }
    public uint       ScoreRate       { get; set; }
    public uint       GoodCount       { get; set; }
    public uint       OkCount         { get; set; }
    public uint       MissCount       { get; set; }
    public uint       ComboCount      { get; set; }
    public uint       HitCount        { get; set; }
    public uint       PoundCount      { get; set; }
    public uint       StarLevel       { get; set; }
    public uint       SupportLevel    { get; set; }
    public byte[]     OptionFlg       { get; set; } = [];
    public byte[]     ToneFlg         { get; set; } = [];
    public uint       PlayMode        { get; set; }
    public uint       StageMode       { get; set; }
    public uint       MusicCategory   { get; set; }
    public uint       SelectedFolderId{ get; set; }
    public bool       IsFavorite      { get; set; }
    public bool       IsRecent        { get; set; }
    public bool       IsPapamama      { get; set; }   // Green also tracks this per spec
    public uint       SoulGauge       { get; set; }
    public uint       PlayDan         { get; set; }
    public uint       WaiwaiResult    { get; set; }
    public uint       WaiwaiGauge     { get; set; }
    public DateTime   PlayTime        { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

- [ ] **Step 3: `UserSaveDataGreen.cs`** — Green's per-user save bag.

```csharp
// Domain/Entities/UserSaveDataGreen.cs
namespace TaikoLocalServer.Domain.Entities;

public partial class UserSaveDataGreen
{
    public uint   Baid                    { get; set; }
    public string Title                   { get; set; } = string.Empty;
    public uint   TitleplateId            { get; set; }
    public uint   ColorBody               { get; set; }
    public uint   ColorFace               { get; set; }
    public uint   ColorLimb               { get; set; }
    public uint   Costume1                { get; set; }
    public uint   Costume2                { get; set; }
    public uint   Costume3                { get; set; }
    public uint   Costume4                { get; set; }
    public uint   Costume5                { get; set; }
    public byte[] CostumeFlg1             { get; set; } = [];
    public byte[] CostumeFlg2             { get; set; } = [];
    public byte[] CostumeFlg3             { get; set; } = [];
    public byte[] CostumeFlg4             { get; set; } = [];
    public byte[] CostumeFlg5             { get; set; } = [];
    public byte[] ToneFlg                 { get; set; } = [];
    public byte[] TitleFlg                { get; set; } = [];
    public byte[] OptionFlg               { get; set; } = [];
    public byte[] DefaultOptionSetting    { get; set; } = [];
    public bool   DefaultShinSetting      { get; set; }
    public uint   DefaultToneSetting      { get; set; }
    public uint   DispDanType             { get; set; }
    public uint   GotDanMax               { get; set; }
    public byte[] GotDanFlg               { get; set; } = [];
    public byte[] GotDanExtraFlg          { get; set; } = [];
    public uint   DispTaikojukuDan        { get; set; }
    public uint   TotalGetDonmedal        { get; set; }
    public uint   TotalUseDonmedal        { get; set; }
    public uint   TotalGetKatsumedal      { get; set; }
    public uint   TotalUseKatsumedal      { get; set; }
    public uint   ItemshopTutorialFlg     { get; set; }
    public bool   IsAutoCostumeOn         { get; set; }
    public uint   CategJpopCnt            { get; set; }
    public uint   CategAnimeCnt           { get; set; }
    public uint   CategDoyoCnt            { get; set; }
    public uint   CategVarietyCnt         { get; set; }
    public uint   CategClassicCnt         { get; set; }
    public uint   CategGameCnt            { get; set; }
    public uint   CategNamcoCnt           { get; set; }
    public uint   CategVocaloidCnt        { get; set; }
    public uint   SongPushedCnt           { get; set; }
    public uint   SongFavoriteCnt         { get; set; }
    public uint   SongRecentCnt           { get; set; }
    public uint   TotalCreditCnt          { get; set; }
    public uint   PrevAreaCode            { get; set; }
    public uint   ConsecAreaCnt           { get; set; }
    public uint   DispLevelTotal          { get; set; }
    public uint   DispLevelChassis        { get; set; }
    public uint   DispLevelSelf           { get; set; }
    public bool   IsDevil                 { get; set; }
    public uint   DispScoreType           { get; set; }
    public uint   DifficultyPlayedCourse  { get; set; }
    public uint   DifficultyPlayedStar    { get; set; }
    public uint   WaiwaiTutorialFlg       { get; set; }
    public bool   IsChallengeCompe        { get; set; }
    public bool   IsTojiru                { get; set; }
    public bool   IsExplain               { get; set; }
    public int    GhostInputMedian        { get; set; }
    public uint   GhostInputVariance      { get; set; }
    public uint   GhostRankId             { get; set; }
    public uint   GhostWinPoint           { get; set; }
    public uint   GhostCertifiedLevelId   { get; set; }
    public uint   GhostTotalWinnings      { get; set; }
    public byte[] GhostReleaseInfoFlag    { get; set; } = [];
    public byte[] GhostPlayedSongFlag     { get; set; } = [];
    public DateTime LastPlayDatetime      { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

- [ ] **Step 4: `GhostStageSectionDatumGreen.cs`** — ghost battle per-section perf, linked to `SongPlayDatumGreen`.

```csharp
// Domain/Entities/GhostStageSectionDatumGreen.cs
namespace TaikoLocalServer.Domain.Entities;

public class GhostStageSectionDatumGreen
{
    public long PlayId      { get; set; }
    public uint SectionNo   { get; set; }
    public bool IsWin       { get; set; }
    public uint GoodCount   { get; set; }
    public uint OkCount     { get; set; }
    public uint NgCount     { get; set; }
    public uint PoundCount  { get; set; }
    public SongPlayDatumGreen Parent { get; set; } = null!;
}
```

- [ ] **Step 5: Green child tables** — `GreenGhostWinnings`, `GreenGhostTokens`, `GreenFriends`, `GreenFavoriteSongs`, `GreenRecentSongs`.

```csharp
// Domain/Entities/GreenGhostWinnings.cs
namespace TaikoLocalServer.Domain.Entities;

public class GreenGhostWinnings
{
    public uint Baid     { get; set; }
    public uint LevelId  { get; set; }
    public uint Winnings { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

```csharp
// Domain/Entities/GreenGhostTokens.cs
namespace TaikoLocalServer.Domain.Entities;

public class GreenGhostTokens
{
    public uint Baid       { get; set; }
    public uint TokenId    { get; set; }
    public uint TokenValue { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

```csharp
// Domain/Entities/GreenFriends.cs
namespace TaikoLocalServer.Domain.Entities;

public class GreenFriends
{
    public uint   Baid       { get; set; }
    public uint   FriendBaid { get; set; }
    public string FriendName { get; set; } = string.Empty;
    public virtual UserDatum? Ba { get; set; }
}
```

```csharp
// Domain/Entities/GreenFavoriteSongs.cs
namespace TaikoLocalServer.Domain.Entities;

public class GreenFavoriteSongs
{
    public uint Baid   { get; set; }
    public uint SongNo { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

```csharp
// Domain/Entities/GreenRecentSongs.cs
namespace TaikoLocalServer.Domain.Entities;

public class GreenRecentSongs
{
    public uint Baid   { get; set; }
    public uint SongNo { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
```

- [ ] **Step 6: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add Domain/Entities/SongBestDatumGreen.cs Domain/Entities/SongPlayDatumGreen.cs Domain/Entities/UserSaveDataGreen.cs Domain/Entities/GhostStageSectionDatumGreen.cs Domain/Entities/GreenGhostWinnings.cs Domain/Entities/GreenGhostTokens.cs Domain/Entities/GreenFriends.cs Domain/Entities/GreenFavoriteSongs.cs Domain/Entities/GreenRecentSongs.cs
git commit -m "feat(domain): add Green-era entity classes (no DbContext wiring yet)"
```

---

## Task 02.2: Add `UserSaveDataNijiiro` entity class

**Goal:** Mirror entity for the Nijiiro half of the soon-to-be-slimmed `UserDatum`. Holds every save-state column currently on `UserDatum`.

**Files:**
- Create: `Domain/Entities/UserSaveDataNijiiro.cs`

**Acceptance Criteria:**
- [ ] File exists in `TaikoLocalServer.Domain.Entities` namespace.
- [ ] Holds every save-state column currently on `UserDatum` except identity (`Baid`, `IsAdmin`, `MyDonName`, `MyDonNameLanguage`).
- [ ] `dotnet build` succeeds.

**Steps:**

- [ ] **Step 1: Create the file**

```csharp
// Domain/Entities/UserSaveDataNijiiro.cs
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public partial class UserSaveDataNijiiro
{
    public uint       Baid                          { get; set; }
    public string     Title                         { get; set; } = string.Empty;
    public uint       TitlePlateId                  { get; set; }
    public List<uint> FavoriteSongsArray            { get; set; } = [];
    public List<uint> ToneFlgArray                  { get; set; } = [0];
    public List<uint> TitleFlgArray                 { get; set; } = [];
    public List<uint> UnlockedKigurumi              { get; set; } = [0];
    public List<uint> UnlockedHead                  { get; set; } = [0];
    public List<uint> UnlockedBody                  { get; set; } = [0];
    public List<uint> UnlockedFace                  { get; set; } = [0];
    public List<uint> UnlockedPuchi                 { get; set; } = [0];
    public uint[]     GenericInfoFlgArray           { get; set; } = Array.Empty<uint>();
    public short      OptionSetting                 { get; set; }
    public int        NotesPosition                 { get; set; }
    public bool       IsVoiceOn                     { get; set; } = true;
    public bool       IsSkipOn                      { get; set; }
    public uint       DifficultyPlayedCourse        { get; set; }
    public uint       DifficultyPlayedStar          { get; set; }
    public uint       DifficultyPlayedSort          { get; set; }
    public uint       DifficultySettingCourse       { get; set; }
    public uint       DifficultySettingStar         { get; set; }
    public uint       DifficultySettingSort         { get; set; }
    public uint       SelectedToneId                { get; set; }
    public DateTime   LastPlayDatetime              { get; set; }
    public uint       LastPlayMode                  { get; set; }
    public uint       ColorBody                     { get; set; }
    public uint       ColorFace                     { get; set; }
    public uint       ColorLimb                     { get; set; }
    public uint       CurrentKigurumi               { get; set; }
    public uint       CurrentHead                   { get; set; }
    public uint       CurrentBody                   { get; set; }
    public uint       CurrentFace                   { get; set; }
    public uint       CurrentPuchi                  { get; set; }
    public bool       DisplayDan                    { get; set; }
    public bool       DisplayAchievement            { get; set; }
    public bool       DisplaySouUchi                { get; set; }
    public Difficulty AchievementDisplayDifficulty  { get; set; }
    public int        AiWinCount                    { get; set; }
    public List<uint> UnlockedSongIdList            { get; set; } = [];
    public List<uint> UnlockedUraSongIdList         { get; set; } = [];
    public virtual UserDatum? Ba { get; set; }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add Domain/Entities/UserSaveDataNijiiro.cs
git commit -m "feat(domain): add UserSaveDataNijiiro entity (Nijiiro save-state target)"
```

---

## Task 02.3: Add Green DbContext partial and ITaikoDbContext.Green partial

**Goal:** Register the new Green entity classes with EF Core. Defines `ITaikoDbContext.Green.cs` interface partial, `TaikoDbContext.Green.cs` implementation partial, and adds the `OnModelCreatingGreen` hook.

**Files:**
- Create: `Application/Abstractions/ITaikoDbContext.Green.cs`
- Create: `Infrastructure/Persistence/TaikoDbContext.Green.cs`
- Modify: `Infrastructure/Persistence/TaikoDbContext.cs` (declare the partial hook + invoke it from `OnModelCreating`)

**Acceptance Criteria:**
- [ ] `ITaikoDbContext.Green.cs` declares Green DbSets: `SongBestDataGreen`, `SongPlayDataGreen`, `UserSaveDataGreen`, `GhostStageSectionDataGreen`, `GreenGhostWinnings`, `GreenGhostTokens`, `GreenFriends`, `GreenFavoriteSongs`, `GreenRecentSongs`. **No `DanScoreDataGreen` / `DanStageScoreDataGreen`** (per spec — Green has no graded dan).
- [ ] `TaikoDbContext.Green.cs` declares the same DbSets and an `OnModelCreatingGreen` partial method body that maps every Green entity (PKs, enum conversions, FKs to `UserDatum.Baid` for cascade delete, FK from `GhostStageSectionDatumGreen` to `SongPlayDatumGreen`).
- [ ] `TaikoDbContext.cs` calls `OnModelCreatingGreen(modelBuilder)` after `OnModelCreatingNijiiro(modelBuilder)`.
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
```

**Steps:**

- [ ] **Step 1: Add the `Application/Abstractions/ITaikoDbContext.Green.cs` partial**

```csharp
// Application/Abstractions/ITaikoDbContext.Green.cs
namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<SongBestDatumGreen>          SongBestDataGreen          { get; }
    DbSet<SongPlayDatumGreen>          SongPlayDataGreen          { get; }
    DbSet<UserSaveDataGreen>           UserSaveDataGreen          { get; }
    DbSet<GhostStageSectionDatumGreen> GhostStageSectionDataGreen { get; }
    DbSet<GreenGhostWinnings>          GreenGhostWinnings         { get; }
    DbSet<GreenGhostTokens>            GreenGhostTokens           { get; }
    DbSet<GreenFriends>                GreenFriends               { get; }
    DbSet<GreenFavoriteSongs>          GreenFavoriteSongs         { get; }
    DbSet<GreenRecentSongs>            GreenRecentSongs           { get; }
}
```

- [ ] **Step 2: Update `Infrastructure/Persistence/TaikoDbContext.cs` to call `OnModelCreatingGreen`**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    OnModelCreatingShared(modelBuilder);
    OnModelCreatingNijiiro(modelBuilder);
    OnModelCreatingGreen(modelBuilder);          // ← NEW
    OnModelCreatingPartial(modelBuilder);
}

partial void OnModelCreatingShared(ModelBuilder modelBuilder);
partial void OnModelCreatingNijiiro(ModelBuilder modelBuilder);
partial void OnModelCreatingGreen(ModelBuilder modelBuilder);  // ← NEW
partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
```

- [ ] **Step 3: Create `Infrastructure/Persistence/TaikoDbContext.Green.cs`**

```csharp
// Infrastructure/Persistence/TaikoDbContext.Green.cs
using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<SongBestDatumGreen>          SongBestDataGreen          { get; set; } = null!;
    public virtual DbSet<SongPlayDatumGreen>          SongPlayDataGreen          { get; set; } = null!;
    public virtual DbSet<UserSaveDataGreen>           UserSaveDataGreen          { get; set; } = null!;
    public virtual DbSet<GhostStageSectionDatumGreen> GhostStageSectionDataGreen { get; set; } = null!;
    public virtual DbSet<GreenGhostWinnings>          GreenGhostWinnings         { get; set; } = null!;
    public virtual DbSet<GreenGhostTokens>            GreenGhostTokens           { get; set; } = null!;
    public virtual DbSet<GreenFriends>                GreenFriends               { get; set; } = null!;
    public virtual DbSet<GreenFavoriteSongs>          GreenFavoriteSongs         { get; set; } = null!;
    public virtual DbSet<GreenRecentSongs>            GreenRecentSongs           { get; set; } = null!;

    partial void OnModelCreatingGreen(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SongBestDatumGreen>(entity =>
        {
            entity.ToTable("SongBestDatum_Green");
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.BestCrown).HasConversion<uint>();
        });

        modelBuilder.Entity<SongPlayDatumGreen>(entity =>
        {
            entity.ToTable("SongPlayDatum_Green");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PlayTime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.Crown).HasConversion<uint>();
        });

        modelBuilder.Entity<UserSaveDataGreen>(entity =>
        {
            entity.ToTable("UserSaveData_Green");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GhostStageSectionDatumGreen>(entity =>
        {
            entity.ToTable("GhostStageSectionDatum_Green");
            entity.HasKey(e => new { e.PlayId, e.SectionNo });
            entity.HasOne(d => d.Parent)
                .WithMany()
                .HasForeignKey(d => d.PlayId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenGhostWinnings>(entity =>
        {
            entity.ToTable("GreenGhostWinnings");
            entity.HasKey(e => new { e.Baid, e.LevelId });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenGhostTokens>(entity =>
        {
            entity.ToTable("GreenGhostTokens");
            entity.HasKey(e => new { e.Baid, e.TokenId });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenFriends>(entity =>
        {
            entity.ToTable("GreenFriends");
            entity.HasKey(e => new { e.Baid, e.FriendBaid });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenFavoriteSongs>(entity =>
        {
            entity.ToTable("GreenFavoriteSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GreenRecentSongs>(entity =>
        {
            entity.ToTable("GreenRecentSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Abstractions/ITaikoDbContext.Green.cs Infrastructure/Persistence/TaikoDbContext.Green.cs Infrastructure/Persistence/TaikoDbContext.cs
git commit -m "feat(infra): register Green entities in TaikoDbContext.Green partial"
```

---

## Task 02.4: Rename Nijiiro DbSets and tables — entity types stay, `[Table]` names suffix `_Nijiiro`

**Goal:** Mark the existing Nijiiro DbSets as renamed-on-disk (`SongBestDatum_Nijiiro` table name) and rename their DbSet C# property names to `SongBestDataNijiiro`, etc. Existing entity classes (`SongBestDatum`, `SongPlayDatum`, etc.) are renamed too. This is a codebase-wide rename that compiles, with EF Core understanding the table rename via `ToTable("..._Nijiiro")`.

**Why one task:** The rename has to be atomic. Half-rename leaves dangling references; full-rename is one coherent commit.

**Files:**
- Modify: `Domain/Entities/SongBestDatum.cs` → rename class to `SongBestDatumNijiiro` (keep file path or rename file; class name change is the key)
- Modify: `Domain/Entities/SongBestDatumMethods.cs` → references the renamed type
- Modify: `Domain/Entities/SongPlayDatum.cs` → rename class to `SongPlayDatumNijiiro`
- Modify: `Domain/Entities/DanScoreDatum.cs` → rename class to `DanScoreDatumNijiiro`
- Modify: `Domain/Entities/DanStageScoreDatum.cs` → rename class to `DanStageScoreDatumNijiiro`
- Modify: `Domain/Entities/AiScoreDatum.cs` → rename class to `AiScoreDatumNijiiro`
- Modify: `Domain/Entities/AiSectionScoreDatum.cs` → rename class to `AiSectionScoreDatumNijiiro`
- Modify: `Application/Abstractions/ITaikoDbContext.Nijiiro.cs` → DbSet property renames
- Modify: `Infrastructure/Persistence/TaikoDbContext.Nijiiro.cs` → DbSet property renames + `ToTable("..._Nijiiro")` on every entity config block
- Modify: `Infrastructure/Persistence/TaikoDbContextPartial.cs` → existing entity configs reference the renamed types
- Modify: every file under `Application/Handlers/` that references the renamed types/properties — search and replace
- Modify: every file under `Adapters.AdminApi/Controllers/` that references the renamed types/properties — search and replace
- Modify: every file under `Adapters.GameProtocol.WwR08/` and `Adapters.GameProtocol.CnR00/` that references the renamed types
- Modify: `LocalSaveModScoreMigrator/Program.cs` if it references these types

**Acceptance Criteria:**
- [ ] Every `SongBestDatum` (type) usage in the codebase is `SongBestDatumNijiiro`, and `.SongBestData` (DbSet) usage is `.SongBestDataNijiiro`. Same for the other five renamed types.
- [ ] In `TaikoDbContext.Nijiiro.cs`, each `modelBuilder.Entity<...>` block calls `entity.ToTable("<Name>_Nijiiro")` (e.g. `entity.ToTable("SongBestDatum_Nijiiro")`).
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
# Confirm no stragglers — should return nothing:
grep -r "SongBestDatum\b" --include="*.cs" Domain Application Infrastructure Adapters.AdminApi Adapters.GameProtocol.WwR08 Adapters.GameProtocol.CnR00 LocalSaveModScoreMigrator 2>/dev/null | grep -v "SongBestDatumNijiiro\|SongBestDatumGreen\|SongBestDatumMethods" | grep -v "/obj/"
```
Expected: build PASSES; the grep returns no matches.

**Steps:**

- [ ] **Step 1: Rename the six entity types** (in `Domain/Entities/`)

For each of `SongBestDatum`, `SongPlayDatum`, `DanScoreDatum`, `DanStageScoreDatum`, `AiScoreDatum`, `AiSectionScoreDatum`: update the `public partial class <Name>` or `public class <Name>` line and all type references inside that file to use `<Name>Nijiiro`. `SongBestDatumMethods.cs` keeps its filename and updates the `public partial class SongBestDatumNijiiro` declaration. Update any internal nav-property references (e.g. `DanScoreDatum.Parent` field already lives inside the type — change `DanScoreDatum Parent` references to `DanScoreDatumNijiiro Parent`).

- [ ] **Step 2: Rename the DbSet property names in `ITaikoDbContext.Nijiiro.cs`**

```csharp
// Application/Abstractions/ITaikoDbContext.Nijiiro.cs
namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<SongBestDatumNijiiro>        SongBestDataNijiiro        { get; }
    DbSet<SongPlayDatumNijiiro>        SongPlayDataNijiiro        { get; }
    DbSet<DanScoreDatumNijiiro>        DanScoreDataNijiiro        { get; }
    DbSet<DanStageScoreDatumNijiiro>   DanStageScoreDataNijiiro   { get; }
    DbSet<AiScoreDatumNijiiro>         AiScoreDataNijiiro         { get; }
    DbSet<AiSectionScoreDatumNijiiro>  AiSectionScoreDataNijiiro  { get; }
    DbSet<UserSaveDataNijiiro>         UserSaveDataNijiiro        { get; }
}
```

- [ ] **Step 3: Update `TaikoDbContext.Nijiiro.cs`** — rename DbSets and add `ToTable("<Name>_Nijiiro")` on every entity config block. Also register `UserSaveDataNijiiro` (added in 02.2). The block becomes:

```csharp
public partial class TaikoDbContext
{
    public virtual DbSet<SongBestDatumNijiiro>        SongBestDataNijiiro        { get; set; } = null!;
    public virtual DbSet<SongPlayDatumNijiiro>        SongPlayDataNijiiro        { get; set; } = null!;
    public virtual DbSet<DanScoreDatumNijiiro>        DanScoreDataNijiiro        { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatumNijiiro>   DanStageScoreDataNijiiro   { get; set; } = null!;
    public virtual DbSet<AiScoreDatumNijiiro>         AiScoreDataNijiiro         { get; set; } = null!;
    public virtual DbSet<AiSectionScoreDatumNijiiro>  AiSectionScoreDataNijiiro  { get; set; } = null!;
    public virtual DbSet<UserSaveDataNijiiro>         UserSaveDataNijiiro        { get; set; } = null!;

    partial void OnModelCreatingNijiiro(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SongBestDatumNijiiro>(entity =>
        {
            entity.ToTable("SongBestDatum_Nijiiro");
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty });
            entity.HasOne(d => d.Ba).WithMany().HasPrincipalKey(p => p.Baid).HasForeignKey(d => d.Baid).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.BestCrown).HasConversion<uint>();
            entity.Property(e => e.BestScoreRank).HasConversion<uint>();
        });

        modelBuilder.Entity<SongPlayDatumNijiiro>(entity =>
        {
            entity.ToTable("SongPlayDatum_Nijiiro");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PlayTime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba).WithMany().HasPrincipalKey(p => p.Baid).HasForeignKey(d => d.Baid).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.ScoreRank).HasConversion<uint>();
            entity.Property(e => e.Crown).HasConversion<uint>();
        });

        modelBuilder.Entity<UserSaveDataNijiiro>(entity =>
        {
            entity.ToTable("UserSaveData_Nijiiro");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.Property(e => e.AchievementDisplayDifficulty).HasConversion<uint>();
            entity.HasOne(d => d.Ba).WithMany().HasPrincipalKey(p => p.Baid).HasForeignKey(d => d.Baid).OnDelete(DeleteBehavior.Cascade);
        });

        // DanScoreDatum, DanStageScoreDatum, AiScoreDatum, AiSectionScoreDatum
        // were originally configured in TaikoDbContextPartial. Move their config
        // blocks here, suffixing each table name with _Nijiiro:

        modelBuilder.Entity<DanScoreDatumNijiiro>(entity =>
        {
            entity.ToTable("DanScoreDatum_Nijiiro");
            // ... existing PK / FK config from TaikoDbContextPartial ...
        });
        modelBuilder.Entity<DanStageScoreDatumNijiiro>(entity =>
        {
            entity.ToTable("DanStageScoreDatum_Nijiiro");
            // ...
        });
        modelBuilder.Entity<AiScoreDatumNijiiro>(entity =>
        {
            entity.ToTable("AiScoreDatum_Nijiiro");
            // ...
        });
        modelBuilder.Entity<AiSectionScoreDatumNijiiro>(entity =>
        {
            entity.ToTable("AiSectionScoreDatum_Nijiiro");
            // ...
        });
    }
}
```

Important: when lifting the Dan/Ai config blocks from `TaikoDbContextPartial.cs`, **delete them** from `TaikoDbContextPartial.cs` so they aren't applied twice.

- [ ] **Step 4: Bulk-rename references in `Application/Handlers/`**

For every file in `Application/Handlers/`, replace:
- `context.SongBestData` → `context.SongBestDataNijiiro`
- `context.SongPlayData` → `context.SongPlayDataNijiiro`
- `context.DanScoreData` → `context.DanScoreDataNijiiro`
- `context.DanStageScoreData` → `context.DanStageScoreDataNijiiro`
- `context.AiScoreData` → `context.AiScoreDataNijiiro`
- `context.AiSectionScoreData` → `context.AiSectionScoreDataNijiiro`
- Type names: `SongBestDatum` (and its `<Type>` and ` Type` usages) → `SongBestDatumNijiiro`, and same for the other five types.

Don't replace `SongBestDatumGreen` or `SongBestDatumMethods` (substring match safeguard).

- [ ] **Step 5: Bulk-rename references in `Adapters.AdminApi/Controllers/`**

Same find-and-replace as Step 4, scoped to `Adapters.AdminApi/`.

- [ ] **Step 6: Bulk-rename references in `Adapters.GameProtocol.WwR08/` and `Adapters.GameProtocol.CnR00/`**

Only mapper / response builder files reference the renamed entity types directly (e.g. `BaidResponseMapper`, `SelfBestMappers`, mappers that return `IEnumerable<SongBestDatum>`). Update those.

- [ ] **Step 7: Bulk-rename references in `LocalSaveModScoreMigrator/`**

Check `LocalSaveModScoreMigrator/Program.cs` and any helpers for the same type / DbSet references.

- [ ] **Step 8: Build**

Run: `dotnet build`
Expected: PASS, zero errors.

- [ ] **Step 9: Sanity-grep for stragglers**

Run:
```bash
grep -rn "SongBestDatum\b\|SongPlayDatum\b\|DanScoreDatum\b\|DanStageScoreDatum\b\|AiScoreDatum\b\|AiSectionScoreDatum\b" --include="*.cs" Domain Application Infrastructure Adapters.AdminApi Adapters.GameProtocol.WwR08 Adapters.GameProtocol.CnR00 LocalSaveModScoreMigrator | grep -v "SongBestDatumMethods\|Nijiiro\|Green" | grep -v "/obj/"
```
Expected: no output.

- [ ] **Step 10: Commit**

```bash
git add Domain Application Infrastructure Adapters.AdminApi Adapters.GameProtocol.WwR08 Adapters.GameProtocol.CnR00 LocalSaveModScoreMigrator
git commit -m "refactor: rename Nijiiro entity types and DbSets to *Nijiiro / *_Nijiiro tables"
```

---

## Task 02.5: Slim `UserDatum` — move save-state columns out, keep only identity

**Goal:** `UserDatum` becomes a slim type holding only `Baid`, `IsAdmin`, `MyDonName`, `MyDonNameLanguage`. All other columns move to `UserSaveDataNijiiro` (the entity already exists from 02.2).

**Files:**
- Modify: `Domain/Entities/UserDatum.cs`
- Modify: `Application/Handlers/UserDataQuery.cs` (and any other handler that reads/writes the save-state columns now on `UserSaveDataNijiiro`)
- Modify: `Adapters.AdminApi/Controllers/UsersController.cs` (and `UserSettingsController.cs` — any admin endpoint that exposes the save-state columns)
- Modify: `Adapters.GameProtocol.WwR08/Mappers/UserDataMappers.cs` and `Adapters.GameProtocol.CnR00/Mappers/UserDataMappers.cs` if they map directly from `UserDatum`'s save-state fields
- Modify: `Infrastructure/Persistence/TaikoDbContext.Shared.cs` — `UserDatum` entity config no longer mentions the moved columns; `OnModelCreatingShared` is reduced.

**Acceptance Criteria:**
- [ ] `Domain/Entities/UserDatum.cs` declares exactly four columns: `Baid`, `IsAdmin`, `MyDonName`, `MyDonNameLanguage`. (Plus any nav-collections like `Tokens` if they currently live there — those stay if they're already keyed by `Baid` and not a "save" concept; or move them into a separate shared concept if they're per-version.)
- [ ] Handler code that previously wrote `user.Title = ...` (or any other save-state column) now writes via `context.UserSaveDataNijiiro` against the user's `Baid`.
- [ ] Admin API and game-protocol mappers that previously read from `UserDatum.<save-state>` now go through `context.UserSaveDataNijiiro` joined on `Baid`.
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
grep -n "Title\|TitlePlateId\|FavoriteSongsArray\|ToneFlgArray\|TitleFlgArray\|UnlockedKigurumi\|UnlockedHead\|UnlockedBody\|UnlockedFace\|UnlockedPuchi\|GenericInfoFlgArray\|NotesPosition\|IsVoiceOn\|IsSkipOn\|DifficultyPlayed\|DifficultySetting\|SelectedToneId\|LastPlayDatetime\|LastPlayMode\|ColorBody\|ColorFace\|ColorLimb\|CurrentKigurumi\|CurrentHead\|CurrentBody\|CurrentFace\|CurrentPuchi\|DisplayDan\|DisplayAchievement\|DisplaySouUchi\|AchievementDisplayDifficulty\|AiWinCount\|UnlockedSongIdList\|UnlockedUraSongIdList" Domain/Entities/UserDatum.cs
```
Expected: build PASSES; grep returns no matches in `UserDatum.cs`.

**Steps:**

- [ ] **Step 1: Slim `Domain/Entities/UserDatum.cs`**

```csharp
// Domain/Entities/UserDatum.cs
namespace TaikoLocalServer.Domain.Entities;

public partial class UserDatum
{
    public uint   Baid              { get; set; }
    public string MyDonName         { get; set; } = string.Empty;
    public uint   MyDonNameLanguage { get; set; }
    public bool   IsAdmin           { get; set; }
    // Tokens stays here if it's identity-scoped today; remove if it's a Nijiiro-save concept.
    public List<Token> Tokens       { get; set; } = new();
}
```

(If `Tokens` is configured as part of the slim identity in `OnModelCreatingShared` today, keep it; if it was an EF-mapped value list on `UserDatum` that should be Nijiiro-scoped, move to `UserSaveDataNijiiro` and adjust the entity config accordingly. Decide based on whether tokens are per-era or shared identity. Default: keep on `UserDatum` because the existing `Token` entity is already in `Application.Abstractions.ITaikoDbContext.Shared`.)

- [ ] **Step 2: Update `TaikoDbContext.Shared.cs` UserDatum config**

The simplified config no longer mentions `LastPlayDatetime` or `AchievementDisplayDifficulty`:

```csharp
modelBuilder.Entity<UserDatum>(entity =>
{
    entity.HasKey(e => e.Baid);
});
```

- [ ] **Step 3: Rewrite consumers**

For every handler / admin controller / mapper that previously read `user.Title`, `user.CurrentKigurumi`, etc.:
- Inject (or already-have) `ITaikoDbContext context`.
- Replace `var user = await context.UserData.FindAsync(baid); var x = user.Title;` with a `FirstOrDefaultAsync` against `context.UserSaveDataNijiiro` keyed on `Baid`, falling back to defaults if the row doesn't exist yet.
- For writes: same — write to `context.UserSaveDataNijiiro`, inserting if absent.

A repeated read pattern can become an extension method on `ITaikoDbContext` (e.g. `GetOrCreateNijiiroSave(uint baid, CancellationToken)`) — add it in `Application/Common/UserSaveDataNijiiroExtensions.cs` if you find the same pattern repeated more than twice. Otherwise inline.

- [ ] **Step 4: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Domain/Entities/UserDatum.cs Infrastructure/Persistence/TaikoDbContext.Shared.cs Application Adapters.AdminApi Adapters.GameProtocol.WwR08 Adapters.GameProtocol.CnR00
git commit -m "refactor: slim UserDatum to identity columns; relocate save-state to UserSaveDataNijiiro"
```

---

## Task 02.6: Generate the `AddGreenEraSupport` EF migration

**Goal:** EF Core compares its model (now containing renamed Nijiiro tables, new Green tables, slim `UserDatum`) against the existing snapshot in `Migrations/TaikoDbContextModelSnapshot.cs` and generates a single migration file capturing all the schema diff.

**The migration will, in one transaction:**
1. Rename Nijiiro tables (`SongBestDatum` → `SongBestDatum_Nijiiro`, etc.).
2. Create `UserSaveData_Nijiiro` table.
3. `INSERT INTO UserSaveData_Nijiiro (...) SELECT (...) FROM UserDatum`.
4. Drop the moved columns from `UserDatum` (SQLite table-rebuild pattern; EF 10 handles it via `DropColumn`).
5. Create empty Green tables.

EF auto-generates step 1 (RenameTable), step 2 (CreateTable), step 4 (DropColumn cascade through table-rebuild), step 5 (CreateTable). **Step 3 (INSERT) must be added manually** with `migrationBuilder.Sql(...)` injected between the CreateTable and DropColumn calls.

**Files:**
- Create: `Infrastructure/Persistence/Migrations/20260512XXXXXX_AddGreenEraSupport.cs` (generated by EF, then manually edited to add the data-copy `Sql(...)` block)
- Modify: `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs` (auto-updated by EF)

**Acceptance Criteria:**
- [ ] Migration file generated with timestamped name `20260512XXXXXX_AddGreenEraSupport`.
- [ ] `migrationBuilder.RenameTable(name: "SongBestDatum", newName: "SongBestDatum_Nijiiro");` (and the other five renames) appears in `Up`.
- [ ] `migrationBuilder.CreateTable(name: "UserSaveData_Nijiiro", ...)` appears in `Up`.
- [ ] `migrationBuilder.Sql(@"INSERT INTO UserSaveData_Nijiiro (...) SELECT (...) FROM UserDatum");` appears in `Up` **between** the `CreateTable("UserSaveData_Nijiiro", ...)` and the `DropColumn("...", "UserDatum")` calls.
- [ ] `migrationBuilder.CreateTable(name: "SongBestDatum_Green", ...)` (and the other eight Green tables) appear in `Up`.
- [ ] `Down` reverses everything (auto-generated; not exercised in dev).
- [ ] `dotnet build` succeeds.
- [ ] `dotnet ef migrations script 20260504132033_EntityNamespaceMove 20260512XXXXXX_AddGreenEraSupport --project Infrastructure --startup-project Host` produces SQL that, when read top-to-bottom, performs the rename + create + copy + drop + create flow.

**Verify:**
```bash
dotnet build
dotnet ef migrations script 20260504132033_EntityNamespaceMove 20260512XXXXXX_AddGreenEraSupport --project Infrastructure --startup-project Host
```
Expected: build PASSES; the generated SQL contains `ALTER TABLE "SongBestDatum" RENAME TO "SongBestDatum_Nijiiro";` and `INSERT INTO "UserSaveData_Nijiiro"` and `CREATE TABLE "SongBestDatum_Green"`.

**Steps:**

- [ ] **Step 1: Generate the migration**

Run:
```bash
dotnet ef migrations add AddGreenEraSupport --project Infrastructure --startup-project Host
```

Expected output: `Done. To undo this action, use 'ef migrations remove'.` A new file `Infrastructure/Persistence/Migrations/<timestamp>_AddGreenEraSupport.cs` appears, and `TaikoDbContextModelSnapshot.cs` is updated.

- [ ] **Step 2: Inspect the generated `Up` body**

Open the new migration file. Confirm the `Up` body contains:
- `RenameTable` calls for the six Nijiiro tables.
- `CreateTable("UserSaveData_Nijiiro", ...)` with every save-state column.
- `DropColumn` calls (or `Sql(@"ALTER TABLE UserDatum ...")` table-rebuild) removing the save-state columns from `UserDatum`.
- `CreateTable` calls for each of the nine Green tables.

If anything's missing or out of order, fix the entity classes / DbContext config to match the design and regenerate the migration (`dotnet ef migrations remove` then `add` again).

- [ ] **Step 3: Insert the data-copy `Sql(...)` block**

Locate the gap between `CreateTable("UserSaveData_Nijiiro", ...)` and the first `DropColumn` (or table-rebuild) on `UserDatum`. Insert:

```csharp
migrationBuilder.Sql(@"
    INSERT INTO ""UserSaveData_Nijiiro"" (
        ""Baid"",
        ""Title"",
        ""TitlePlateId"",
        ""FavoriteSongsArray"",
        ""ToneFlgArray"",
        ""TitleFlgArray"",
        ""UnlockedKigurumi"",
        ""UnlockedHead"",
        ""UnlockedBody"",
        ""UnlockedFace"",
        ""UnlockedPuchi"",
        ""GenericInfoFlgArray"",
        ""OptionSetting"",
        ""NotesPosition"",
        ""IsVoiceOn"",
        ""IsSkipOn"",
        ""DifficultyPlayedCourse"",
        ""DifficultyPlayedStar"",
        ""DifficultyPlayedSort"",
        ""DifficultySettingCourse"",
        ""DifficultySettingStar"",
        ""DifficultySettingSort"",
        ""SelectedToneId"",
        ""LastPlayDatetime"",
        ""LastPlayMode"",
        ""ColorBody"",
        ""ColorFace"",
        ""ColorLimb"",
        ""CurrentKigurumi"",
        ""CurrentHead"",
        ""CurrentBody"",
        ""CurrentFace"",
        ""CurrentPuchi"",
        ""DisplayDan"",
        ""DisplayAchievement"",
        ""DisplaySouUchi"",
        ""AchievementDisplayDifficulty"",
        ""AiWinCount"",
        ""UnlockedSongIdList"",
        ""UnlockedUraSongIdList""
    )
    SELECT
        ""Baid"",
        ""Title"",
        ""TitlePlateId"",
        ""FavoriteSongsArray"",
        ""ToneFlgArray"",
        ""TitleFlgArray"",
        ""UnlockedKigurumi"",
        ""UnlockedHead"",
        ""UnlockedBody"",
        ""UnlockedFace"",
        ""UnlockedPuchi"",
        ""GenericInfoFlgArray"",
        ""OptionSetting"",
        ""NotesPosition"",
        ""IsVoiceOn"",
        ""IsSkipOn"",
        ""DifficultyPlayedCourse"",
        ""DifficultyPlayedStar"",
        ""DifficultyPlayedSort"",
        ""DifficultySettingCourse"",
        ""DifficultySettingStar"",
        ""DifficultySettingSort"",
        ""SelectedToneId"",
        ""LastPlayDatetime"",
        ""LastPlayMode"",
        ""ColorBody"",
        ""ColorFace"",
        ""ColorLimb"",
        ""CurrentKigurumi"",
        ""CurrentHead"",
        ""CurrentBody"",
        ""CurrentFace"",
        ""CurrentPuchi"",
        ""DisplayDan"",
        ""DisplayAchievement"",
        ""DisplaySouUchi"",
        ""AchievementDisplayDifficulty"",
        ""AiWinCount"",
        ""UnlockedSongIdList"",
        ""UnlockedUraSongIdList""
    FROM ""UserDatum"";
");
```

(Adjust column names to match exactly what's in the generated migration's `CreateTable("UserSaveData_Nijiiro", ...)` and the existing `UserDatum` schema. Some columns are EF-serialized JSON arrays — same column name on both sides, plain copy.)

- [ ] **Step 4: Inspect via the SQL dump**

Run:
```bash
dotnet ef migrations script 20260504132033_EntityNamespaceMove 20260512XXXXXX_AddGreenEraSupport --project Infrastructure --startup-project Host
```
(Replace `20260512XXXXXX` with the actual generated timestamp.)

Expected: the dumped SQL flows top-to-bottom as: rename Nijiiro tables → create `UserSaveData_Nijiiro` → `INSERT INTO UserSaveData_Nijiiro ... SELECT ... FROM UserDatum` → drop save-state columns from `UserDatum` (or rebuild `UserDatum` via the SQLite pattern) → create the nine Green tables.

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add Infrastructure/Persistence/Migrations
git commit -m "feat(db): add AddGreenEraSupport migration (rename Nijiiro tables, slim UserDatum, create Green tables)"
```

> The **user** is the one who runs the server. Migration application against the real dev `taiko.db3` happens in `08-smoke-checklist.md` under user supervision — not by Claude.
