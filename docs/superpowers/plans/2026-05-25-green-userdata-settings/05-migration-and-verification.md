# Stage 5: Migration And Final Verification

## Goal

Backfill existing Green saves to official `IsTojiru = true`, verify the complete feature, and commit the migration.

## Files

- Create: `Infrastructure/Persistence/Migrations/<generated>_BackfillGreenIsTojiruDefault.cs`
- Create: `Infrastructure/Persistence/Migrations/<generated>_BackfillGreenIsTojiruDefault.Designer.cs`
- Inspect: `Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs`

## Steps

- [ ] **Step 1: Generate a data-only migration**

Run:

```powershell
dotnet ef migrations add BackfillGreenIsTojiruDefault --project Infrastructure --startup-project Host
```

Expected: EF creates a migration pair under `Infrastructure/Persistence/Migrations/`. Because there is no model change, the generated `Up` and `Down` methods can be empty before the manual edit.

- [ ] **Step 2: Edit the migration Up method**

In the generated `*_BackfillGreenIsTojiruDefault.cs`, replace the empty `Up` method body with:

```csharp
migrationBuilder.Sql(
    """
    UPDATE "UserSaveData_Green"
    SET "IsTojiru" = 1;
    """);
```

Use this `Down` method body:

```csharp
// Data backfill is intentionally irreversible.
```

The final migration should follow this shape:

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillGreenIsTojiruDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "UserSaveData_Green"
                SET "IsTojiru" = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data backfill is intentionally irreversible.
        }
    }
}
```

- [ ] **Step 3: Inspect the snapshot**

Run:

```powershell
git diff -- Infrastructure/Persistence/Migrations/TaikoDbContextModelSnapshot.cs
```

Expected: no semantic model changes. If EF only rewrites formatting, keep the generated snapshot. If EF adds schema changes, stop and inspect the earlier stages for an unintended model mutation before continuing.

- [ ] **Step 4: Run focused feature tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenSaveDataTests|FullyQualifiedName~GreenIdentityHandlerTests|FullyQualifiedName~GreenUserDataMapperTests|FullyQualifiedName~GreenAdminApiControllerTests.UserSettings_Green|FullyQualifiedName~GreenPlayResultHandlerTests.UpdatePlayResult_Green_Dani|FullyQualifiedName~GreenCustomizationWebUiTests.Profile_RendersGreenUserdataSettingsOnlyForGreen"
```

Expected: all selected tests pass.

- [ ] **Step 5: Run project build**

Run:

```powershell
dotnet build Host/Host.csproj
```

Expected: build succeeds. If the build fails with a file-copy lock such as `MSB3021`, `MSB3027`, or `CS2012`, check for a running Host process before treating it as a source regression.

- [ ] **Step 6: Inspect migration script**

Run:

```powershell
dotnet ef migrations script --project Infrastructure --startup-project Host
```

Expected: the final migration section contains:

```sql
UPDATE "UserSaveData_Green"
SET "IsTojiru" = 1;
```

No table or column creation should appear for this migration.

- [ ] **Step 7: Commit Stage 5**

Run:

```powershell
git status --short
git add -- Infrastructure/Persistence/Migrations
git diff --cached --name-status
git commit -m "Backfill Green tojiru default"
```

Expected staged files: the generated migration pair, plus `TaikoDbContextModelSnapshot.cs` only if EF changed it.

- [ ] **Step 8: Final status check**

Run:

```powershell
git status --short
```

Expected: no changes from this feature remain unstaged or uncommitted. Existing unrelated worktree changes from before the plan can remain.
