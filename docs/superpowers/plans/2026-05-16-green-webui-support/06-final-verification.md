# Task 6: Final Verification

**Goal:** Verify the full Green WebUI slice builds, tests pass, and the branch contains only intended changes.

**Files:**
- Review: `docs/superpowers/specs/2026-05-16-green-webui-support-design.md`
- Review: all files changed by Tasks 1-5

- [ ] **Step 1: Run backend tests**

Run:

```powershell
dotnet test Tests/Tests.csproj
```

Expected: all tests pass.

- [ ] **Step 2: Build WebUI**

Run:

```powershell
dotnet build TaikoWebUI/TaikoWebUI.csproj
```

Expected: build succeeds with no new errors.

- [ ] **Step 3: Build solution**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: build succeeds.

- [ ] **Step 4: Inspect Green route references**

Run:

```powershell
Select-String -Path TaikoWebUI\Pages\*.razor,TaikoWebUI\Pages\*.razor.cs,TaikoWebUI\Components\*.razor -Pattern '/Users/\{baid:int\}/\{era\}|WebUiEra.Api|WebUiEra.UserRoute|TODO Green WebUI'
```

Expected: route pages contain era routes, API calls use `WebUiEra.Api`, links use `WebUiEra.UserRoute`, and hidden Green controls have explicit TODO comments.

- [ ] **Step 5: Inspect AdminApi route references**

Run:

```powershell
Select-String -Path Adapters.AdminApi\Controllers\*.cs -Pattern '/api/\{era\}|EraRoute|GameEra.Green|GreenFavoriteSongs|SongBestDataGreen|DanScoreDataGreen'
```

Expected: era-aware routes and Green dispatch branches are present in the intended controllers.

- [ ] **Step 6: Check git status**

Run:

```powershell
git status --short
```

Expected: only intentional files are modified. Pre-existing unrelated changes may still be present; do not revert them.

- [ ] **Step 7: Commit final verification notes if needed**

If verification required small fixes, commit them:

```powershell
git add -- Adapters.AdminApi/Controllers Contracts.AdminApi/ViewModels Tests/Green/GreenAdminApiControllerTests.cs TaikoWebUI/Utilities TaikoWebUI/Services TaikoWebUI/Components TaikoWebUI/Pages
git commit -m "Verify Green WebUI support"
```

If no fixes were required, do not create an empty commit.