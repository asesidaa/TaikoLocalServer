# Task 2: Schema And Migration

**Files:**
- Modify: `Domain/Entities/SongBestDatumGreen.cs`
- Modify: `Domain/Entities/SongPlayDatumGreen.cs`
- Modify: `Infrastructure/Persistence/TaikoDbContext.Green.cs`
- Create: `Infrastructure/Persistence/Migrations/*_AddGreenShinScoreSplit.cs`
- Create: `Infrastructure/Persistence/Migrations/*_AddGreenShinScoreSplit.Designer.cs`
- Modify: `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs`

## Steps

- [ ] **Step 1: Add `IsShin` to Green entities**

In `Domain/Entities/SongBestDatumGreen.cs`, add the property after `Difficulty`:

```csharp
    public bool IsShin { get; set; }
```

In `Domain/Entities/SongPlayDatumGreen.cs`, add the property after `StageMode`:

```csharp
    public bool IsShin { get; set; }
```

- [ ] **Step 2: Update Green best primary key**

In `Infrastructure/Persistence/TaikoDbContext.Green.cs`, change the `SongBestDatumGreen` key to:

```csharp
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty, e.IsShin });
```

No explicit conversion is needed for `IsShin`; EF maps it as a SQLite integer boolean.

- [ ] **Step 3: Generate the EF migration**

Run:

```powershell
dotnet ef migrations add AddGreenShinScoreSplit --project Infrastructure --startup-project Host
```

Expected: creates a new migration pair under `Infrastructure/Persistence/Migrations/` and updates `TaikoDbContextModelSnapshot.cs`.

- [ ] **Step 4: Verify migration operations**

Open the generated migration and make sure `Up()` contains these operations in this order:

```csharp
migrationBuilder.AddColumn<bool>(
    name: "IsShin",
    table: "SongPlayDatum_Green",
    type: "INTEGER",
    nullable: false,
    defaultValue: false);

migrationBuilder.AddColumn<bool>(
    name: "IsShin",
    table: "SongBestDatum_Green",
    type: "INTEGER",
    nullable: false,
    defaultValue: false);

migrationBuilder.DropPrimaryKey(
    name: "PK_SongBestDatum_Green",
    table: "SongBestDatum_Green");

migrationBuilder.AddPrimaryKey(
    name: "PK_SongBestDatum_Green",
    table: "SongBestDatum_Green",
    columns: new[] { "Baid", "SongId", "Difficulty", "IsShin" });
```

Make sure `Down()` reverses those operations:

```csharp
migrationBuilder.DropPrimaryKey(
    name: "PK_SongBestDatum_Green",
    table: "SongBestDatum_Green");

migrationBuilder.DropColumn(
    name: "IsShin",
    table: "SongPlayDatum_Green");

migrationBuilder.DropColumn(
    name: "IsShin",
    table: "SongBestDatum_Green");

migrationBuilder.AddPrimaryKey(
    name: "PK_SongBestDatum_Green",
    table: "SongBestDatum_Green",
    columns: new[] { "Baid", "SongId", "Difficulty" });
```

- [ ] **Step 5: Verify schema build**

Run:

```powershell
dotnet build
```

Expected: build exits `0`.

- [ ] **Step 6: Commit schema and migration**

Run:

```powershell
git add -- Domain/Entities/SongBestDatumGreen.cs Domain/Entities/SongPlayDatumGreen.cs Infrastructure/Persistence/TaikoDbContext.Green.cs Infrastructure/Persistence/Migrations
git commit -m "Add Green shin score storage"
```
