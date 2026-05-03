# .NET 10 Upgrade — PR3: TaikoWebUI MudBlazor 6 → 9

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate TaikoWebUI's MudBlazor stack from v6.20.0 to the latest v9.x. Absorbs three majors of breaking changes in one branch (v6 → v7 → v8 → v9). After this PR, the admin UI runs cleanly on .NET 10 with current MudBlazor.

**Architecture:** Bump MudBlazor and CodeBeam.MudBlazor.Extensions in CPM, then work through compile errors mechanically against the published migration guides. The known largest impact is the v8 typography rename (`Default` → `DefaultTypography`, `H1`–`H6` → `H1Typography`–`H6Typography`, etc.). Component-level fixes are small and localized. No automated tests — validation is "click every page, watch the browser console for errors."

**Tech Stack:** .NET 10 LTS, Blazor WebAssembly, MudBlazor 9.x, CodeBeam.MudBlazor.Extensions (matching 9.x), C# 13.

**Branch:** `dev/mudblazor-9` off the merged tip of `dev/net10-runtime` (i.e. `dev` after PR2 merges).

---

## Context — read these first

Both files are required reading before executing any task:

- `CLAUDE.md` (repo root) — solution layout, Blazor hosting model, file conventions.
- `docs/superpowers/specs/2026-05-03-net10-upgrade-design.md` — full spec (this plan covers only Section 5: PR3).

**Critical pre-condition:** PR2 (`docs/superpowers/plans/2026-05-03-net10-pr2-runtime-and-mediator.md`) must be merged into `dev` before starting this plan. PR2 puts the solution on .NET 10 / LangVersion 13 with all server-side packages refreshed.

**Reference docs you'll use during this PR:**

- [MudBlazor v8.0.0 Migration Guide (#9953)](https://github.com/MudBlazor/MudBlazor/issues/9953) — the largest single set of breaking changes (typography rename, MudCollapse, MudFormComponent disposal, etc.).
- [MudBlazor v7.0.0 release notes](https://github.com/MudBlazor/MudBlazor/releases) — v6 → v7 changes, mostly parameter and Color enum renames.
- [MudBlazor v9.0.0 release notes](https://github.com/MudBlazor/MudBlazor/releases) — drops .NET 8, smaller breaking change set than v8.

## File structure

**Files modified (CPM):**
- `Directory.Packages.props` — bump MudBlazor and adjacent UI packages.

**Files modified (TaikoWebUI):**
- `TaikoWebUI/Components/MainLayout.razor` — theme definition (typography rename, possibly `Palette` → `PaletteLight` property rename in v7).
- `TaikoWebUI/**/*.razor` — component-level breaking changes. Estimated impact areas: `MudTable` (v8 `EditButtonContext` namespace, v8 `CancelledEditingItem` typo fix); `MudTabs` (parameter renames); any `MudFormComponent` subclass (disposal pattern). Concrete file list determined by compile errors.
- `TaikoWebUI/**/*.razor.cs` — code-behind files for the same components if they reference renamed types.

**Files possibly replaced (CodeBeam dependency):**
- `TaikoWebUI/Pages/Dialogs/UserQrCodeDialog.razor` — uses `MudExtensions.MudBarcode`. If `CodeBeam.MudBlazor.Extensions` 9.x is unavailable, replace with a native or alternative QR component (see Task 4).

**Files modified (docs):**
- `README.md` — note new MudBlazor major version requirement for contributors customizing the UI.

**Files NOT touched:**
- `TaikoLocalServer/` source (unchanged from PR2).
- `GameDatabase/`, `SharedProject/`, `LocalSaveModScoreMigrator/`.
- `wwwroot/data/`, `Configurations/`, `Migrations/`.
- `Localization/*.resx` (translations are component-agnostic).

## Pre-flight

- [ ] **Step 0.1: Confirm PR2 is merged**

Run:
```bash
git checkout dev
git pull --ff-only
```
Verify `dev` is on .NET 10:
```bash
grep "TargetFramework" Directory.Build.props
```
Expected: `<TargetFramework>net10.0</TargetFramework>`. If still `net8.0`, PR2 isn't merged yet.

- [ ] **Step 0.2: Verify MudBlazor is still at 6.x**

Run:
```bash
grep "MudBlazor" Directory.Packages.props
```
Expected: `<PackageVersion Include="MudBlazor" Version="6.20.0" />` (PR2 deliberately left this at 6.x).

- [ ] **Step 0.3: Verify the WebUI currently builds (even if visually broken at runtime)**

Run:
```bash
dotnet build TaikoWebUI/TaikoWebUI.csproj
```
Expected: succeeds. MudBlazor 6.x targets older TFMs but is forward-compatible with .NET 10 at the build level.

- [ ] **Step 0.4: Capture pre-upgrade UI screenshots (optional but recommended)**

Run the server in one terminal:
```bash
dotnet run --project TaikoLocalServer
```

In a browser, visit each of these pages and screenshot them so you have a v6 visual baseline to compare against during PR3 validation:
- `/` (Dashboard)
- `/Login`
- `/Users` (admin must be enabled)
- `/Profile/<baid>`
- `/HighScores/<baid>`
- `/PlayHistory/<baid>`
- `/DaniDojo/<baid>`
- `/Song/<songId>`
- `/SongList`
- `/AccessCode/<baid>`

Save these somewhere outside the repo (e.g. `~/taiko-pr3-baseline/`).

Stop the server (Ctrl+C).

- [ ] **Step 0.5: Create the branch**

```bash
git checkout -b dev/mudblazor-9
```

---

## Task 1: Bump MudBlazor and adjacent UI packages in CPM

**Files:**
- Modify: `Directory.Packages.props`

- [ ] **Step 1.1: Check the latest stable versions**

Run:
```bash
dotnet list package --outdated --include-transitive false TaikoWebUI/TaikoWebUI.csproj
```
Note the latest stable for each UI package.

- [ ] **Step 1.2: Update the version entries**

In `D:\TaikoLocalServer\Directory.Packages.props`, update these entries to the latest stable values reported above. Best-known starting points as of 2026-05-03:

```xml
    <PackageVersion Include="MudBlazor" Version="9.4.0" />
    <PackageVersion Include="CodeBeam.MudBlazor.Extensions" Version="9.0.0" />
    <PackageVersion Include="Blazored.LocalStorage" Version="4.5.0" />
    <PackageVersion Include="Markdig" Version="0.38.0" />
    <PackageVersion Include="Autocomplete.Clients" Version="1.1.0" />
```

If `CodeBeam.MudBlazor.Extensions 9.x` does NOT exist on NuGet at PR3 implementation time:
1. Search NuGet for the latest CodeBeam version (likely 8.x or 7.x).
2. **Do not** mix-and-match versions: CodeBeam tracks the MudBlazor major. If only 8.x exists, you have two options:
   - Pin MudBlazor to 8.x in this PR (acceptable interim — the spec accepts this) and re-evaluate v9 in a follow-up.
   - Drop the CodeBeam dependency entirely (Task 4 has the fallback for the single QR-code usage).

- [ ] **Step 1.3: Restore (build will fail, that's expected)**

Run:
```bash
dotnet restore
```
Expected: succeeds — packages download. The next build will reveal compile errors throughout `TaikoWebUI`.

DO NOT commit yet — the tree is broken.

---

## Task 2: Migrate the theme definition in `MainLayout.razor`

**Files:**
- Modify: `TaikoWebUI/Components/MainLayout.razor`

The current theme in `MainLayout.razor` lines 98–113 uses MudBlazor v6's API:

```csharp
readonly MudTheme taikoWebUiTheme = new()
    {
        Palette = new PaletteLight()
        {
            Primary = Colors.Indigo.Default,
            PrimaryLighten = Colors.Indigo.Lighten2,
            PrimaryDarken = Colors.Indigo.Darken2,
            AppbarBackground = Colors.Indigo.Darken3,
        },
        PaletteDark = new PaletteDark()
        {
            Primary = Colors.Indigo.Accent1,
            PrimaryLighten = Colors.Indigo.Lighten4,
            PrimaryDarken = Colors.Indigo.Accent1,
        },
    };
```

In v7, the property `MudTheme.Palette` was renamed to `MudTheme.PaletteLight`. In v8, typography classes were renamed for STJ source-gen compat (no typography is set here, so v8's typography breaking changes don't directly affect this file — but they affect any `Typography = new Typography { … }` blocks if added in the future).

- [ ] **Step 2.1: Update the theme definition**

In `D:\TaikoLocalServer\TaikoWebUI\Components\MainLayout.razor`, replace the `taikoWebUiTheme` definition with:

```csharp
readonly MudTheme taikoWebUiTheme = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = Colors.Indigo.Default,
            PrimaryLighten = Colors.Indigo.Lighten2,
            PrimaryDarken = Colors.Indigo.Darken2,
            AppbarBackground = Colors.Indigo.Darken3,
        },
        PaletteDark = new PaletteDark()
        {
            Primary = Colors.Indigo.Accent1,
            PrimaryLighten = Colors.Indigo.Lighten4,
            PrimaryDarken = Colors.Indigo.Accent1,
        },
    };
```

Single change: `Palette = new PaletteLight()` → `PaletteLight = new PaletteLight()`.

- [ ] **Step 2.2: Don't build yet**

The WebUI will still have many compile errors elsewhere — Tasks 3–4 work through them.

---

## Task 3: Triage compile errors and fix component-level breaking changes

**Files:** various `*.razor` and `*.razor.cs` files in `TaikoWebUI/`. Concrete list determined by the build output.

This is the unpredictable part of the migration. The strategy: build, read errors, fix the highest-impact ones first, repeat until clean.

- [ ] **Step 3.1: Capture the full error list**

Run:
```bash
dotnet build TaikoWebUI/TaikoWebUI.csproj 2>&1 | tee pr3-build-errors.log
```

Expected: build fails with multiple errors. Save the log so you can work through it systematically.

- [ ] **Step 3.2: Categorize the errors**

Open `pr3-build-errors.log` and bucket the errors. Common categories you'll see:

| Category | Symptom | Fix pattern |
|---|---|---|
| Renamed Color enum values | `Colors.Indigo.Default` not found | Map to the new value per [v7 release notes](https://github.com/MudBlazor/MudBlazor/releases). Most stayed; some renamed. |
| Renamed component parameters | `Property X does not exist on MudFoo` | Look up the v7/v8/v9 doc for that component; rename to the new parameter or remove if it became obsolete. |
| Removed parameters | `Ripple`/`Href`/`ForceLoad`/`OnClick` on `MudSelectItem` | Remove the parameter — these were no-ops in v6 anyway. |
| Disposal pattern | `Cannot override Dispose(bool)` on a `MudFormComponent` subclass | Replace with `protected override ValueTask DisposeAsyncCore()`. |
| Namespace move | `EditButtonContext` not found in `MudBlazorFix` | Change `using MudBlazorFix;` to `using MudBlazor;` (or remove — v8 moved this type into the main namespace). |
| Renamed types (typography) | Errors only if you set typography overrides in `MudTheme` | Apply `Default` → `DefaultTypography`, `H1` → `H1Typography`, etc. (see [v8 migration guide](https://github.com/MudBlazor/MudBlazor/issues/9953)). |
| `IResizeObserver` / `IEventListener` | `IDisposable not implemented` | Switch to factory injection (`IResizeObserverFactory` / `IEventListenerFactory`) — likely unused in this app. |

- [ ] **Step 3.3: Fix errors in `MainLayout.razor` first**

Run:
```bash
dotnet build TaikoWebUI/TaikoWebUI.csproj 2>&1 | grep "MainLayout"
```

Apply each fix in `MainLayout.razor` per the table above. Re-build to confirm `MainLayout` is clean before moving on.

- [ ] **Step 3.4: Fix errors in heavy `MudTable` users**

These four files are the biggest `MudTable` consumers and are most likely to surface breaking changes:

- `TaikoWebUI/Components/Song/PlayHistoryCard.razor`
- `TaikoWebUI/Components/Song/SongLeaderboardCard.razor`
- `TaikoWebUI/Pages/PlayHistory.razor`
- `TaikoWebUI/Pages/HighScores.razor`

For each:
1. `dotnet build TaikoWebUI/TaikoWebUI.csproj 2>&1 | grep "<filename>"`
2. Apply fixes per the table in 3.2.
3. Re-build.

Specific patterns to expect:

**`MudTableSortLabel<T>` with `SortBy`** — works the same in v6/v7/v8/v9. No change needed to the lambda expressions.

**`Breakpoint=Breakpoint.None`** — works in all versions. Don't change.

**`RowClassFunc="@GetActiveRowClass"`** — works in all versions. Don't change. (But verify at runtime in Step 7.3 — the row class function semantics changed subtly in v7.)

**`<MudTablePager RowsPerPageString="…" />`** — works in all versions. No change.

If `EditButtonContext` is referenced: change `using MudBlazorFix;` to `using MudBlazor;` (or remove the using if `MudBlazor` is already in scope from `_Imports.razor`).

- [ ] **Step 3.5: Fix errors in `MudTabs` users**

These two files use `MudTabs`:

- `TaikoWebUI/Pages/Profile.razor` (×2 — outer Profile tabs and inner per-section tabs)
- `TaikoWebUI/Pages/HighScores.razor`

Specific parameters used in this codebase that may be affected by v7/v8/v9 changes:
- `MinimumTabWidth` — verify it still exists in v9 (likely yes).
- `Rounded`, `Border`, `PanelClass`, `ApplyEffectsToContainer`, `Outlined` — most still exist; `Outlined` was renamed in v7 (became a sub-property of `MudTabs.Variant` or similar). Apply the build-error-driven fix.
- `ActivePanelIndexChanged`, `ActivePanelIndex` — still present.

For each error, look up the component in the [MudBlazor v9 docs](https://mudblazor.com/) and apply the new parameter name.

- [ ] **Step 3.6: Fix errors in remaining files**

Loop:
1. `dotnet build TaikoWebUI/TaikoWebUI.csproj` and read the next error.
2. Apply the fix per the migration guides.
3. Repeat until 0 errors.

There is no shortcut — each error is a localized read-the-docs fix. Most fixes are 1–3 lines; a handful (typography overrides, disposal patterns) are 5–10 lines.

- [ ] **Step 3.7: Confirm clean build**

Run:
```bash
dotnet build TaikoWebUI/TaikoWebUI.csproj
```
Expected: 0 errors. Warnings are acceptable but should be reviewed; specifically watch for:
- `BL0007: Component parameter should be auto property` — informational; ignore unless it points to actual issues.
- `MUD0001`/`MUD0002` from MudBlazor's own analyzer — read each one.

- [ ] **Step 3.8: Build the full solution**

Run:
```bash
dotnet build
```
Expected: 0 errors. Confirms TaikoLocalServer (which references TaikoWebUI) still publishes correctly.

- [ ] **Step 3.9: Commit Tasks 1, 2, 3 together**

```bash
rm pr3-build-errors.log
git add Directory.Packages.props \
  TaikoWebUI/Components/MainLayout.razor \
  TaikoWebUI/
git commit -m "Migrate TaikoWebUI to MudBlazor 9.x

- Bump MudBlazor 6.20.0 -> 9.4.0 and CodeBeam.MudBlazor.Extensions to matching 9.x
- Theme: rename Palette property to PaletteLight (v7 breaking change)
- Component fixes per v7/v8/v9 migration guides"
```

---

## Task 4: Handle CodeBeam.MudBlazor.Extensions if no v9 is available

**Files:**
- Possibly modify: `TaikoWebUI/Pages/Dialogs/UserQrCodeDialog.razor`

This task only applies if Task 1.2 forced you to choose between pinning MudBlazor at v8 or dropping CodeBeam. If you successfully pinned `CodeBeam.MudBlazor.Extensions 9.x` in Task 1, **skip this task**.

CodeBeam is used in exactly one file: `TaikoWebUI/Pages/Dialogs/UserQrCodeDialog.razor`, which uses `MudExtensions.MudBarcode` to render a QR code:

```razor
<MudExtensions.MudBarcode Value="@qrCode" BarcodeFormat="ZXing.BarcodeFormat.QR_CODE" Height="300" Width="300" />
```

- [ ] **Step 4.1: If you opted to drop CodeBeam, replace the QR component**

Option A — use a lightweight standalone QR library. Add `<PackageVersion Include="QRCoder" Version="1.6.0" />` to `Directory.Packages.props` and `<PackageReference Include="QRCoder" />` to `TaikoWebUI.csproj`, then replace the dialog content:

```razor
@using QRCoder

<MudDialog Class="dialog-user-qr-code">
    <DialogContent>
        @if (!string.IsNullOrEmpty(qrPngBase64))
        {
            <img src="data:image/png;base64,@qrPngBase64" width="300" height="300" alt="QR code" />
        }
    </DialogContent>
    <DialogActions>
        <MudButton Color="Color.Primary" OnClick="Submit">@Localizer["Dialog OK"]</MudButton>
    </DialogActions>
</MudDialog>

@code {

    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public User User { get; set; } = new();

    private string qrCode = string.Empty;
    private string qrPngBase64 = string.Empty;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        qrCode = "BNTTCNID" + User.AccessCodes.First();

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrCode, QRCodeGenerator.ECCLevel.Q);
        using var pngQrCode = new PngByteQRCode(qrCodeData);
        qrPngBase64 = Convert.ToBase64String(pngQrCode.GetGraphic(20));
    }

    private void Submit()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }
}
```

Note: `QRCoder` works on Blazor WASM. The PR description should call out the dependency change.

Option B — pin MudBlazor at v8.10.x (or whatever version CodeBeam supports) and accept the smaller upgrade. In that case, no source change here; just adjust the CPM version and update the PR description.

- [ ] **Step 4.2: Build and verify**

Run:
```bash
dotnet build
```
Expected: succeeds.

- [ ] **Step 4.3: Commit**

```bash
git add Directory.Packages.props \
  TaikoWebUI/TaikoWebUI.csproj \
  TaikoWebUI/Pages/Dialogs/UserQrCodeDialog.razor
git commit -m "Replace MudExtensions.MudBarcode with QRCoder for v9 compat"
```

---

## Task 5: Static asset hosting verification

**Files:** none modified — observational only.

PR2's Task 14 may have already switched `app.UseStaticFiles()` → `app.MapStaticAssets()`. If MudBlazor 9 changes how the WASM bundle is shaped, the static-asset issue may resurface. Verify here.

- [ ] **Step 5.1: Run the server**

Run:
```bash
dotnet run --project TaikoLocalServer
```

- [ ] **Step 5.2: Open the admin UI in a browser**

Visit `http://localhost:5000/`.

- [ ] **Step 5.3: Check the browser dev tools Network tab**

Reload the page with dev tools open. Confirm these all return 200:
- `/_framework/blazor.webassembly.js`
- `/_framework/dotnet.js`
- `/_framework/dotnet.runtime.*.js`
- `/_framework/dotnet.native.*.js`
- `/_framework/MudBlazor.styles.css`
- One or more `/_framework/*.wasm`

If anything 404s, return to PR2's Task 14 — switch `UseStaticFiles` to `MapStaticAssets` in `Program.cs`. (This is the only PR2 follow-up that PR3 might need; everything else PR2 finalized stays put.)

Stop the server.

---

## Task 6: Visual smoke test

**Files:** none modified — observational only.

This is the main validation gate for PR3. With no automated tests, manual exercise is the only way to catch UI regressions.

- [ ] **Step 6.1: Run the server**

```bash
dotnet run --project TaikoLocalServer
```

- [ ] **Step 6.2: Walk through every page and feature**

Open `http://localhost:5000/` and verify each item below. **Open the browser dev tools console** — any red error there is a failure of this task.

| Page / feature | What to verify |
|---|---|
| `/` (Dashboard) | Markdown content from `Dashboard.md` renders. No console errors. |
| `/Login` | Form fields render; submit doesn't throw. (If `LoginRequired=false`, this page may be hidden — that's fine.) |
| `/Register` | Form renders; password field works. |
| `/ChangePassword` | Form renders. |
| `/Users` | Admin list renders (requires `IsAdmin=true` on the logged-in user). Sort, pagination work. |
| `/Profile/<baid>` | All 8+ tabs (User, Costume, Title, Tone, Settings, etc.) render. Tab switching works. Forms inside tabs save. |
| `/HighScores/<baid>` | All 5 difficulty tabs (Easy/Normal/Hard/Oni/Ura) render. Filter input works. Sort labels work on every column. |
| `/PlayHistory/<baid>` | Table renders. Pagination works. Filter input works. Expansion rows (per-song play list) expand correctly. |
| `/DaniDojo/<baid>` | Dani list renders with correct icons/states. |
| `/Song/<songId>` | Leaderboard table renders. Play history card renders. Both tables have correct row highlighting for the current user. |
| `/SongList` | Song list renders. Search/filter works. |
| `/AccessCode/<baid>` | List of bound access codes renders. Add/delete dialogs open. |
| Dialogs | Open `UserDelete`, `AccessCodeDelete`, `ChooseTitle`, `OTP`, `ResetPassword`, `UserQrCode` from their entry points. Each one renders, accepts input, closes correctly. |
| Dark mode | Click the moon/sun icon in the app bar. Verify the whole UI switches palettes. Reload the page; the setting persists (LocalStorage). |
| Language toggle | Click the language menu. Switch to each of: English, Français, 简体中文, 繁體中文, 日本語. Verify text re-renders in the chosen language. Reload; setting persists. |
| Drawer | Click the hamburger icon. Drawer collapses/expands. State persists across reload. |
| Breadcrumbs | Navigate Profile → HighScores → SongList. Breadcrumbs update on each navigation. |

- [ ] **Step 6.3: Compare screenshots if you took baselines in Step 0.4**

Open each baseline screenshot side-by-side with the current page. The visual diffs we accept (per the spec):

- Slightly different paddings/borders on buttons, inputs, cards (v7 changed defaults).
- Different elevation shadows on dialogs and toolbars (v7 changed defaults).
- Different ripple/hover animations (v8 changed timing).

The visual diffs we DON'T accept:

- Misaligned tables (column widths jumping or text truncating that didn't before).
- Sort labels that don't sort or sort the wrong direction.
- Row highlighting (e.g. "current user" highlight in leaderboard) missing or applied to wrong rows.
- Theme colors not applied (page renders in MudBlazor's default purple instead of indigo).
- Dialogs that don't open or don't close.

If any "don't accept" diff is found: read the relevant component's v9 docs and the migration guides. Common cause: a parameter that was renamed in v7/v8/v9 was left at its old name, silently doing nothing.

Stop the server.

---

## Task 7: Update `README.md`

**Files:**
- Modify: `README.md`

- [ ] **Step 7.1: Add a contributor note about MudBlazor 9**

In `D:\TaikoLocalServer\README.md`, append to the "For developers" section:

```markdown

The admin UI uses **MudBlazor 9.x**. If you customize `TaikoWebUI/`, refer to the [MudBlazor v9 docs](https://mudblazor.com/) — components and parameters changed across the 7→8→9 majors compared to older forks.
```

- [ ] **Step 7.2: Commit**

```bash
git add README.md
git commit -m "Document MudBlazor 9 in README"
```

---

## Task 8: Push and open PR

- [ ] **Step 8.1: Confirm branch is clean and ahead of `dev`**

Run:
```bash
git status
git log dev..HEAD --oneline
```
Expected: clean tree; commit list shows the Task 3 (and possibly Task 4) and Task 7 commits.

- [ ] **Step 8.2: Push the branch**

```bash
git push -u origin dev/mudblazor-9
```

- [ ] **Step 8.3: Open the PR**

```bash
gh pr create --base dev --title "Migrate TaikoWebUI to MudBlazor 9" --body "$(cat <<'EOF'
## Summary

Sequenced as PR3 of three (PR1 = modernize on .NET 8 [merged], PR2 = .NET 10 runtime + Mediator [merged]). See \`docs/superpowers/specs/2026-05-03-net10-upgrade-design.md\`.

This PR:
- Bumps **MudBlazor 6.20.0 → 9.x** (latest stable), absorbing v6→v7→v8→v9 breaking changes in one pass.
- Bumps \`CodeBeam.MudBlazor.Extensions\` to the matching v9.x. (Or drops it and replaces the single \`MudBarcode\` usage with \`QRCoder\` — see commits.)
- Migrates the theme definition to v7's renamed \`PaletteLight\` property.
- Applies component-level breaking-change fixes per the [v8 migration guide](https://github.com/MudBlazor/MudBlazor/issues/9953).

After this PR, the \`dev\` branch is fully on .NET 10 + MudBlazor 9 with all three upgrade goals (modernization, runtime, UI) complete.

## What did NOT change

- Server source code, game endpoints, mappers, handlers (PR2's territory).
- DB schema or migrations.
- Localization strings (\`*.resx\`).
- Configuration formats.

## Visual changes accepted as v9 baseline

Per the spec: minor padding / border / elevation / animation differences from v7's default-style changes are accepted as the new visual baseline. We do NOT try to recreate v6 visuals.

## Test plan

- [x] \`dotnet build\` clean — 0 errors
- [x] \`dotnet publish -c Release\` produces the WASM bundle
- [x] All 7 Kestrel endpoints bind on startup
- [x] Browser network tab shows no 404s on \`/_framework/*\`
- [x] Walked through every admin page (Dashboard, Login, Register, Users, Profile, HighScores, PlayHistory, DaniDojo, Song, SongList, AccessCode)
- [x] All dialogs open, accept input, close (UserDelete, AccessCodeDelete, ChooseTitle, OTP, ResetPassword, UserQrCode)
- [x] Dark mode toggle works; setting persists across reload
- [x] Language toggle works for all 5 cultures (en-US, fr-FR, zh-Hans, zh-Hant, ja); setting persists
- [x] Tables: sort, filter, pagination, expansion rows all functional
- [x] No console errors on any page

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

- [ ] **Step 8.4: Confirm CI passes**

Visit the PR URL printed by the previous command. Confirm the GitHub Actions workflow runs to green and produces the `TLS 1.2.<run>` artifact.

If CI fails on a build error that you didn't see locally, the most likely cause is a minor analyzer rule difference between local SDK and CI's `setup-dotnet@v4 dotnet-version: 10.0.x`. Read the CI log; either fix the local code to satisfy the rule, or pin the CI to the exact SDK version your local uses.

---

## Self-review checklist

Before declaring PR3 complete:

- [ ] `Directory.Packages.props` shows `<PackageVersion Include="MudBlazor" …>` at 9.x (or the latest you could pin, with rationale in the PR description if you fell back to 8.x).
- [ ] `MainLayout.razor` uses `PaletteLight = new PaletteLight()` (not `Palette = new PaletteLight()`).
- [ ] `dotnet build` is clean — 0 errors.
- [ ] Browser console is clean on every page from the smoke-test list.
- [ ] Theme is applied correctly (indigo, not MudBlazor's default purple).
- [ ] Dark mode toggle, language toggle, drawer state all persist via LocalStorage.
- [ ] Tables sort and filter correctly in the four heavy users (PlayHistoryCard, SongLeaderboardCard, PlayHistory, HighScores).
- [ ] CI is green on the PR.

## Done

After this PR merges, the upgrade is complete:
- All five projects on .NET 10 LTS.
- All package versions deduped and current.
- MediatR replaced with Mediator (martinothamar) using full conventions.
- MudBlazor v9, CodeBeam matched.
- CPM, Directory.Build.props, .slnx, global.json infrastructure in place.

Future maintenance: bump versions in `Directory.Packages.props` periodically; SDK band roll-forward in `global.json` happens automatically.
