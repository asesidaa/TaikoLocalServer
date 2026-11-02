# Task 6: Migration and Full Verification

**Files:**
- Create: `Infrastructure/Persistence/Migrations/*_AddGreenDanScoreData.cs`
- Modify: `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs`

- [ ] **Step 1: Generate EF migration**

Run from repo root:

```powershell
dotnet ef migrations add AddGreenDanScoreData --project Infrastructure --startup-project Host
```

Expected: EF creates a migration adding `DanScoreDatum_Green` and `DanStageScoreDatum_Green`, and updates the model snapshot.

- [ ] **Step 2: Inspect the migration**

Open the generated migration and verify it contains these structural elements:

```csharp
migrationBuilder.CreateTable(
    name: "DanScoreDatum_Green",
    columns: table => new
    {
        Baid = table.Column<uint>(type: "INTEGER", nullable: false),
        DanId = table.Column<uint>(type: "INTEGER", nullable: false),
        IsExtra = table.Column<bool>(type: "INTEGER", nullable: false),
        MedleyUniqueId = table.Column<uint>(type: "INTEGER", nullable: false),
        ArrivalSongCount = table.Column<uint>(type: "INTEGER", nullable: false),
        SoulGaugeTotal = table.Column<uint>(type: "INTEGER", nullable: false),
        ComboCountTotal = table.Column<uint>(type: "INTEGER", nullable: false),
        ClearGrade = table.Column<uint>(type: "INTEGER", nullable: false, defaultValue: 0u)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_DanScoreDatum_Green", x => new { x.Baid, x.DanId, x.IsExtra });
    });
```

Also verify `DanStageScoreDatum_Green` has primary key `{ Baid, DanId, IsExtra, StageIndex }` and a cascade foreign key to `DanScoreDatum_Green`.

- [ ] **Step 3: Run focused Green tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenDan|FullyQualifiedName~Dani|FullyQualifiedName~GetDanScore_Green|FullyQualifiedName~UserData_Green_NewSaveSendsSentinelOneForDispTaikojukuDan"
```

Expected: PASS.

- [ ] **Step 4: Run all tests**

Run:

```powershell
dotnet test Tests/Tests.csproj
```

Expected: PASS.

- [ ] **Step 5: Build the solution**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: build succeeds with no new warnings caused by this work.

- [ ] **Step 6: Review git diff**

Run:

```powershell
git diff --stat
git diff --check
```

Expected: diff contains only Green Dani schema/handler/test/migration changes, and `git diff --check` reports no whitespace errors.

- [ ] **Step 7: Commit Task 6**

```powershell
git add Infrastructure/Persistence/Migrations Infrastructure/Persistence/TaikoDbContextModelSnapshot.cs
git commit -m "Add Green Dani score migration"
```

- [ ] **Step 8: Final status**

Run:

```powershell
git status --short
```

Expected: clean working tree.
