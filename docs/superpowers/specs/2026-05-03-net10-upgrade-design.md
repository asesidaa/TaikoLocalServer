# .NET 10 Upgrade — Design

**Status:** Draft — design content approved; awaiting file review before plan handoff
**Date:** 2026-05-03
**Scope:** All five projects in the TaikoLocalServer solution, plus shared infrastructure

## 1. Goals & non-goals

### Goal

Move all five projects from .NET 8 to .NET 10 LTS, dedupe and refresh every NuGet dependency, modernize project infrastructure (Central Package Management, `Directory.Build.props`, `.slnx` solution format), and replace MediatR with martinothamar's source-generated `Mediator`. Functionality is unchanged: same game endpoints, same DB schema, same admin UI surface — operators pull the new release and have it work without touching their existing `taiko.db3`, datatables, or `Configurations/` files.

### Non-goals

- DB schema or migration changes — `GameDatabase/Migrations` is frozen for this work.
- Functional changes to game endpoints or admin API.
- Replacing protobuf-net, EF Core, Serilog, or BCrypt (just version bumps).
- Adding tests. There are none today; introducing them is its own project.
- Touching `wwwroot/data/` formats or `Configurations/` JSON shapes.

### Decisions locked in during brainstorming

| Decision | Choice |
|---|---|
| Upgrade scope | Lift + full modernization (CPM, Directory.Build.props, `.slnx`) |
| Mediator library | `Mediator` (martinothamar) — MIT, source-generated |
| Mediator migration depth | Full convention adoption (ValueTask, record struct, CancellationToken plumbing) |
| MudBlazor target | v9.x latest |
| PR sequencing | Three staged PRs |
| UI PR layout | Single PR for all UI changes |
| `.slnx` adoption | Yes |
| `TreatWarningsAsErrors` | No (avoid scope creep) |

## 2. Sequencing — three staged PRs

| # | Branch | Base | Runtime at end of PR | Smoke test |
|---|---|---|---|---|
| 1 | `dev/modernize-on-net8` | `dev` | .NET 8 (unchanged) | Build green; server starts; DB migrates; admin UI loads; one game endpoint hand-tested |
| 2 | `dev/net10-runtime` | merged PR1 | .NET 10 | Same as PR1 plus: Mediator handlers route correctly; multi-port Kestrel binds all 7 endpoints; JWT works |
| 3 | `dev/mudblazor-9` | merged PR2 | .NET 10 (UI fixed) | All admin pages render without console errors; dark mode + 5 cultures still work |

PR1 deliberately ships on .NET 8 so the infrastructure changes can be reverted without dragging the runtime move with them. PR2 is the engine swap (TFM + server packages + Mediator). PR3 is the UI migration. Each merge is independently revertible on `dev`.

## 3. PR1 — Modernize project structure on .NET 8

### New repo-root files

- **`Directory.Build.props`** — shared `TargetFramework=net8.0`, `Nullable=enable`, `ImplicitUsings=enable`, `LangVersion=12`, `TreatWarningsAsErrors=false`. Each `.csproj` removes the corresponding lines.
- **`Directory.Packages.props`** — `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` plus every `<PackageVersion Include="…" Version="…" />`. Project files keep `<PackageReference Include="…" />` *without* a version attribute.
- **`global.json`** — pin the .NET 8 SDK band so contributors get consistent restores. PR2 bumps this to .NET 10.
- **`TaikoLocalServer.slnx`** — XML replacement for the existing `.sln`. Same five projects, same configurations. Delete the old `.sln`. `.sln.DotSettings` keeps working; ReSharper picks it up by basename.

### Per-project `.csproj` changes (no behavioral change)

- All five projects: drop redundant `TargetFramework`, `Nullable`, `ImplicitUsings`, `LangVersion` lines (now inherited).
- All five projects: drop `Version="…"` from every `<PackageReference>` (now in CPM).
- **`GameDatabase.csproj`**: replace `Microsoft.EntityFrameworkCore.Sqlite 8.0.0-rc.2.23480.1` and `Microsoft.EntityFrameworkCore.Tools 8.0.0-rc.2.23480.1` with stable `8.0.4`. Align `Microsoft.EntityFrameworkCore` from `8.0.3` to `8.0.4`.
- **`LocalSaveModScoreMigrator.csproj`**: same EF Core alignment to `8.0.4`. Bump `SharpZipLib` from `1.4.0` → `1.4.2` to match other projects.
- **`SharedProject.csproj`**: bump `Throw` from `1.3.0` → `1.4.0`.
- **`TaikoLocalServer.csproj`**: keep `EnableConfigurationBindingGenerator=false` for now (re-evaluated in PR2).
- **`TaikoLocalServer.csproj`** retains its custom blocks: `<BuildTime>` metadata attribute, certificate/configuration/datatable `<None>` and `<Content>` items, `<PublishSingleFile>` on Release.

### Version dedupe table (after PR1)

| Package | Before | After |
|---|---|---|
| Microsoft.EntityFrameworkCore | 8.0.3 / 8.0.4 | 8.0.4 |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.0-rc.2.23480.1 / 8.0.4 | 8.0.4 |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0-rc.2.23480.1 / 8.0.4 | 8.0.4 |
| SharpZipLib | 1.4.0 / 1.4.2 | 1.4.2 |
| Throw | 1.3.0 / 1.4.0 | 1.4.0 |
| BCrypt.Net-Next, Swan.Core, others | already aligned | confirmed via CPM |

### Validation for PR1

1. `dotnet restore` succeeds with no version-conflict warnings.
2. `dotnet build` is clean.
3. `dotnet publish` produces the same self-contained Windows exe shape as before.
4. `dotnet run --project TaikoLocalServer` starts; existing `wwwroot/taiko.db3` migrates without errors; admin UI loads.
5. One game endpoint smoke-tested by hand (e.g. `/v12r08_ww/chassis/userdata_*.php` returns the expected protobuf response).

### What PR1 explicitly does NOT touch

Source code in `Controllers/`, `Handlers/`, `Mappers/`, `Services/`, `GameDatabase/Entities/`, or any `.razor` / `.razor.cs` file.

## 4. PR2 — .NET 10 runtime, server packages, MediatR → Mediator

PR2 is the engine swap. It targets `net10.0`, refreshes everything that ships in the server process, and replaces MediatR with martinothamar's source-generated Mediator. WebUI also targets `net10.0` here; its MudBlazor packages stay at v6.20.0 — that's PR3.

### Runtime / SDK changes

- `global.json` → pin `10.0.100` band.
- `Directory.Build.props` → `TargetFramework=net10.0`, `LangVersion=13` (stable on .NET 10, not `preview`).
- `.github/workflows/publishTLS.yml` → `actions/setup-dotnet@v4` with `dotnet-version: 10.0.x`. Bump `Minor_Version_Number` from `1` to `2` so artifacts read `TLS 1.2.<run>`.

### Server-side package refresh (consolidated in `Directory.Packages.props`)

Exact pinned versions are determined during PR2 implementation by `dotnet list package --outdated`; the table below is the upgrade *intent*.

| Package | From | Target |
|---|---|---|
| Microsoft.AspNetCore.* | 8.0.4 | 10.0.x latest |
| Microsoft.EntityFrameworkCore | 8.0.4 | 10.0.x latest |
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.4 | 10.0.x latest |
| Microsoft.EntityFrameworkCore.Tools | 8.0.4 | 10.0.x latest |
| EntityFrameworkCore.Exceptions.Sqlite | 8.1.0 | latest compatible with EF 10 |
| Riok.Mapperly | 3.5.1 | 4.x latest |
| protobuf-net | 3.2.30 | 3.2.x latest |
| protobuf-net.AspNetCore | 3.2.12 | 3.2.x latest |
| Serilog.AspNetCore | 8.0.2-dev-00334 | latest stable |
| Serilog.Expressions | 4.0.0 | latest |
| Serilog.Sinks.File.Header | 1.0.2 | latest |
| Microsoft.Extensions.Localization | 8.0.4 | 10.0.x |
| Yoh.Text.Json.NamingPolicies | 1.1.2 | latest |
| Swashbuckle.AspNetCore | 6.5.0 | latest |
| BCrypt.Net-Next, DotNetZip, SharpZipLib, Otp.NET, Swan.*, Throw | various | latest stable |

### MediatR → Mediator swap (full convention adoption)

| Concept | MediatR (today) | Mediator (martinothamar) |
|---|---|---|
| NuGet packages | `MediatR` 12.2.0 | `Mediator.SourceGenerator` (gen) + `Mediator.Abstractions` (runtime) |
| Request marker | `IRequest<TResponse>` | `IRequest<TResponse>` (same name, namespace `Mediator`) |
| Handler interface | `IRequestHandler<TRequest, TResponse>` | `IRequestHandler<TRequest, TResponse>` |
| Handler return type | `Task<TResponse>` | **`ValueTask<TResponse>`** |
| Controller dispatch type | `ISender` (in `BaseController<T>`) | **`IMediator`** |
| DI registration | `services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly))` | `services.AddMediator(opt => { opt.ServiceLifetime = ServiceLifetime.Scoped; opt.Namespace = "TaikoLocalServer"; })` |

### Mediator-native conventions adopted in PR2

- **Handler return type** changes to `ValueTask<TResponse>` for all 17 handlers. Controllers `await` either `Task` or `ValueTask`, so call sites are unaffected.
- **Request types become `readonly record struct`** where the payload is small (e.g. `BaidQuery`, `GetTokenCountQuery`). For requests wrapping a reference DTO (e.g. `UpdatePlayResultCommand(uint, CommonPlayResultData)`), the wrapper is also struct'd; the inner reference is unchanged. Roughly 16 of the 17 requests convert; any with a copy-semantics concern stay `record class`.
- **Service lifetime: Scoped**, not Singleton. Handlers inject `TaikoDbContext` (scoped); switching to `IDbContextFactory` is out of scope. The Mediator source-generator + scoped lifetime is still substantially faster than reflection-based MediatR; the singleton-vs-scoped delta is microseconds and irrelevant at this app's request rate.
- **Source-gen-friendly dispatch.** Controllers writing `await Mediator.Send(new UserDataQuery(...))` already get the monomorphized overload via overload resolution (the static argument type is the concrete request, not `IRequest<T>`). No call-site rewrites required.
- **CancellationToken plumbing.** Every `Mediator.Send(query)` becomes `Mediator.Send(query, HttpContext.RequestAborted)`. ~21 controller call sites get the argument added. Handlers already accept `CancellationToken` parameters and most already pass them to EF Core; audit and fill any gaps.
- **`BaseController<T>`** field type changes from `ISender` to `IMediator` to expose Mediator's typed overloads.

### Concrete touch list

- `TaikoLocalServer/GlobalUsings.cs`: `global using MediatR;` → `global using Mediator;`
- `TaikoLocalServer/Program.cs` (line 92): `AddMediatR(...)` → `AddMediator(opt => ...)` per the convention block above.
- `TaikoLocalServer/Controllers/BaseController.cs`: change `mediator` field type and accessor return type from `ISender` to `IMediator`.
- `TaikoLocalServer/Handlers/*.cs` (17 files): `using MediatR;` removed (covered by GlobalUsings); handler `Handle` signatures change return type `Task<T>` → `ValueTask<T>`; request types reshape to `readonly record struct` where applicable.
- `TaikoLocalServer/Controllers/**/*.cs` (~21 call sites): add `, HttpContext.RequestAborted` to every `Mediator.Send(...)` call.

### Other PR2 code-level changes

- `TaikoLocalServer/Program.cs`: re-evaluate `EnableConfigurationBindingGenerator=false`. .NET 10's source generator is more permissive; if the build is clean without it, remove the property. Otherwise keep with a one-line comment explaining why.
- `TaikoLocalServer/Program.cs`: switch `app.UseStaticFiles()` → `app.MapStaticAssets()` only if Blazor WASM hosting on .NET 10 requires it (per the [known regression](https://learn.microsoft.com/en-us/answers/questions/5746146/error-when-upgrading-a-blazor-app-to-net-10) for `dotnet.js`/`*.wasm` 404s under the old call). Keep the old call if smoke shows the bundle still loads.
- `BaseController<T>`'s lazy resolution pattern (`HttpContext.RequestServices.GetService<...>`) is unchanged.

### Important non-changes in PR2

- No DB migrations added or modified. EF Core 10 reads .NET-8-era migrations.
- No changes to game protobuf models, mappers, or Game/Api controllers' route shapes.
- WebUI MudBlazor stays at v6.20.0. Between merging PR2 and PR3, the admin UI may render with visual glitches or fail to load entirely on .NET 10. **Do not deploy this intermediate state.** The PR description must call this out.

### Validation for PR2

1. `dotnet restore` clean — no transitive version conflicts.
2. `dotnet build` clean. Investigate any Riok.Mapperly source-gen warnings from the 3.x → 4.x churn.
3. `dotnet publish` produces a `net10.0/win-x64` self-contained single-file exe.
4. Server starts; existing `taiko.db3` migrates via EF 10 with no schema diff (verify `__EFMigrationsHistory` table is byte-identical).
5. **Mediator handler routing smoke test**: hit `/v12r08_ww/chassis/userdata_*.php` with a real game request (or curl using a captured payload) and confirm a non-empty protobuf response. This exercises `Mediator.Send(new UserDataQuery(...), HttpContext.RequestAborted)` end-to-end.
6. JWT auth works on a protected admin API call with `AuthenticationRequired=true`.
7. Multi-port Kestrel binding (5000, 80, 10122, 54430, 54431, 57402, 443) all come up — most likely regression source for ASP.NET Core 10.

## 5. PR3 — TaikoWebUI MudBlazor 6 → 9

PR3 absorbs three majors of MudBlazor breakage in one branch (v6 → v7 → v8 → v9). 148 MudBlazor references across 12 files; the heaviest users are `Profile.razor` (58 refs across 8+ tabs) and `HighScores.razor` (33 refs across 5 difficulty tabs).

### Package changes (Directory.Packages.props)

| Package | From | Target | Notes |
|---|---|---|---|
| MudBlazor | 6.20.0 | 9.4.0 (or latest 9.x) | the big one |
| CodeBeam.MudBlazor.Extensions | 6.9.2 | matching 9.x release | tracks MudBlazor major |
| Blazored.LocalStorage | 4.5.0 | latest stable | minor breaks unlikely |
| System.IdentityModel.Tokens.Jwt | 7.7.1 | latest 8.x | already passes through PR2 |
| Markdig | 0.37.0 | latest | low-risk |
| Autocomplete.Clients | 1.1.0 | latest compatible | niche dependency; verify .NET 10 compat at impl time, replace if abandoned |

### v8 theme rename — biggest single-file impact

`MainLayout.razor:12` consumes a custom `taikoWebUiTheme` via `<MudThemeProvider Theme="@taikoWebUiTheme" />`. The theme definition (in `MainLayout.razor.cs` or a sibling file) almost certainly uses the renamed typography classes. v8 made these changes for STJ source-generator compatibility:

| v6/v7 name | v8/v9 name |
|---|---|
| `Default` | `DefaultTypography` |
| `H1`–`H6` | `H1Typography`–`H6Typography` |
| `Subtitle1` / `Subtitle2` | `Subtitle1Typography` / `Subtitle2Typography` |
| `Body1` / `Body2` | `Body1Typography` / `Body2Typography` |
| `new BaseTypography()` | use a concrete `*Typography` (BaseTypography is abstract) |

### Component-level fixes (mechanical, against the v7/v8/v9 migration guides)

- `MudTable` heavy users (`PlayHistoryCard.razor`, `SongLeaderboardCard.razor`, `PlayHistory.razor`, `HighScores.razor`): `EditButtonContext` namespace `MudBlazorFix` → `MudBlazor`. `CancelledEditingItem` → `CanceledEditingItem` if any handler subscribes. Sort labels (`MudTableSortLabel`), `RowClassFunc`, `Filter`, `RowsPerPage`, `Breakpoint=Breakpoint.None` — all unchanged.
- `MudTabs` (`Profile.razor` ×2, `HighScores.razor`): verify `MinimumTabWidth`, `Rounded`, `Border`, `PanelClass`, `ApplyEffectsToContainer`, `ActivePanelIndexChanged`, `ActivePanelIndex`, `Outlined` against the v9 docs (some boolean params got reshaped in v7).
- `MudFormComponent` subclasses: `Dispose(bool disposing)` → `protected virtual ValueTask DisposeAsyncCore()`. Grep for inheritors in `Pages/Dialogs/` and `Components/`.
- `MudSelectItem`: drop any `Ripple` / `Href` / `ForceLoad` / `OnClick` parameters (they were always no-ops).
- `IResizeObserver` / `IEventListener` direct usage: switch to factory injection (`IResizeObserverFactory` / `IEventListenerFactory`). Likely unused in this app.
- `TimeSeriesChartSeries` / `TimeSeriesDisplayType` namespace move — verify the codebase doesn't use chart components.

### Visual diffs we accept without further work

- v7 changed default Outlined / Filled / Text styling for several components. Buttons, inputs, and cards may render with slightly different paddings / borders. We accept v9 defaults as the new baseline.
- v7 changed Color contrast / elevation defaults. Dashboard, dialogs, and toolbars may shift visually.
- Dark mode behavior (`IsDarkMode` on `MudThemeProvider`) is preserved.

### Localization

Five resources (en-US, fr-FR, zh-Hans, zh-Hant, ja) are component-agnostic strings. No content changes; verify culture switching still works under MudBlazor 9.

### Validation for PR3

1. `dotnet build` clean. Expect a first-pass full-error build, then work through compile errors against the [MudBlazor v8 migration guide](https://github.com/MudBlazor/MudBlazor/issues/9953) and v9 release notes.
2. `dotnet publish` produces the WASM bundle, and the bundle gets served by TaikoLocalServer (no `dotnet.js` 404s).
3. **Visual smoke checklist** — every page, every user-visible feature:
   - Login, Register, ChangePassword
   - Users (admin list)
   - Profile — every one of the 8+ tabs renders without console errors
   - HighScores — all 5 difficulty tabs, sort/filter still work
   - PlayHistory — table, pagination, filtering, expansion rows
   - DaniDojo, Song (leaderboard + history cards), SongList, AccessCode
   - Dialogs: UserDelete, AccessCodeDelete, ChooseTitle, OTP, ResetPassword, UserQrCode
   - Dark mode toggle
   - Language toggle across all 5 cultures
4. Hit one game endpoint while the UI is open (paranoia check that the WASM bundle isn't blocking server-side traffic).

## 6. Risk register & cross-PR watchlist

### High-risk items

| Risk | PR | Detection | Mitigation |
|---|---|---|---|
| Multi-port Kestrel binding regresses on ASP.NET Core 10 | PR2 | Server fails to bind one of: 5000, 80, 10122, 54430, 54431, 57402, 443. Check log "Now listening on:" lines for all 7 endpoints. | If a `Configurations/Kestrel.json` pattern no longer parses, port to in-code Kestrel options. |
| Blazor WASM hosting under .NET 10 — known reports of `dotnet.js` / `*.wasm` 404s when using `UseStaticFiles()` | PR2 (carries to PR3) | Browser network tab shows 404 on `/_framework/dotnet.js` or `/_framework/blazor.webassembly.js`. | Switch `app.UseStaticFiles()` → `app.MapStaticAssets()` in `Program.cs`. |
| EF Core 10 reads existing `taiko.db3` differently than EF 8 (model snapshot diff) | PR2 | Server logs "model has pending changes" warning on startup, or `Migrate()` tries to add a no-op migration. | Verify `__EFMigrationsHistory` is byte-identical on a copy of an existing prod DB. If EF 10 thinks the model has drifted, run `dotnet ef migrations script` to confirm it's a no-op. |
| Mediator's source generator misses a handler registration | PR2 | At runtime, `Mediator.Send(new SomeQuery(...))` throws "no handler registered". | Source gen runs at build time and emits diagnostics — review build output for `MEDIATOR*` warnings. Inspect generated `Mediator` partial to confirm all 17 handler types are present. |
| `ValueTask<T>` awaited multiple times by mistake | PR2 | Test failure or "value task awaited twice" exception. | Audit handlers — they all `return await context.…` or build a result and `return result;`. No fan-out patterns to worry about. |
| protobuf-net 3.2.30 → latest 3.2.x affects on-the-wire game compat | PR2 | Game client receives malformed responses; manifests as game-side disconnection or freezing. | protobuf-net is conservative within 3.x. Smoke test by pointing a real game client at the server and playing one round. |
| MudBlazor 9 visual regressions, especially `MudTable` row-class function or sort-label generics | PR3 | UI loads but tables misaligned, sort doesn't work, or row highlighting (e.g. "current user" highlight) breaks. | Compare each table page against pre-upgrade screenshots. `RowClassFunc="@GetActiveRowClass"` in `SongLeaderboardCard.razor` / `PlayHistoryCard.razor` is the highest-likelihood breakage point. |
| `CodeBeam.MudBlazor.Extensions` lags behind MudBlazor 9 | PR3 | Compile error or runtime missing-component error from `MudExtensions`. | Check NuGet for the matching CodeBeam version before merging PR3. If no v9 release exists, evaluate replacing the two usages (`UserQrCodeDialog.razor`) with native MudBlazor 9 components. |
| Theme typography rename breaks at runtime, not compile time | PR3 | Build succeeds; UI loads with default theme instead of the custom `taikoWebUiTheme`. | After theme migration, confirm a known-themed element (custom font weight, color) still applies. |

### Lower-risk items

- `Riok.Mapperly` 3.x → 4.x: source-gen behavior is generally stable, but partial-method signatures may need adjustment if Mapperly tightens nullable analysis.
- `Serilog.AspNetCore` `8.0.2-dev` → stable: prerelease → stable transition. Confirm the CSV sink filter expression `StartsWith(@m, 'CSV WRITE:')` still parses.
- `Swashbuckle.AspNetCore` → latest: API doc generation; no code consumes this beyond enabling the swagger UI route.
- `BCrypt.Net-Next` → latest: hashing format unchanged across 4.x — existing user password hashes in `Credential` keep working.
- `System.IdentityModel.Tokens.Jwt` 7.7.1 → 8.x: some default-claim mapping changes; verify `AuthService` issues tokens that the JWT bearer middleware accepts.
- `protobuf-net.AspNetCore` formatter registration order: ensure `[Produces("application/protobuf")]` still routes through the protobuf formatter, not JSON.
- `EntityFrameworkCore.Exceptions.Sqlite`: depends on the maintainer publishing an EF Core 10-compatible release. If unavailable at PR2 implementation time, fall back to catching `DbUpdateException` directly in `TaikoDbContext.OnConfiguring` and dropping the package.

### Pre-flight checks before each PR's smoke test

- `git stash` any local datatable customizations operators may have made in `wwwroot/data/*.json` (the build copies in-repo defaults over them).
- Back up `wwwroot/taiko.db3` before running PR2's first build (EF 10 migrations are forward-only).
- For PR3, snapshot a few key UI pages first so we have a v6 visual baseline to compare against.

### Rollback story

Each PR is an independently-revertible single merge commit on `dev`. PR2 includes the runtime move + Mediator swap together (intentionally — rolling back Mediator alone would be awkward). If PR2 is reverted, PR1's CPM / Directory.Build.props infrastructure stays — it works on .NET 8 too.

### Documentation updates

- **PR1**: brief CHANGELOG note about CPM and `.slnx`. README.md gets a sentence about new SDK requirements (still .NET 8 in PR1).
- **PR2**: README.md updated to "requires .NET 10 SDK". `CLAUDE.md` updated — the "MediatR-based CQRS" note becomes "Mediator-based CQRS" with the convention summary (ValueTask handlers, `record struct` requests, scoped lifetime, CT plumbing).
- **PR3**: no doc update beyond the PR description.

## 7. Open items deferred to implementation planning

These are deliberately unspecified here because they're determined at PR-execution time:

- **Exact .NET 10 package versions** — pinned via `dotnet list package --outdated` against the live NuGet feed when each PR is implemented.
- **Exact MudBlazor 9.x patch level** — same; pin the latest stable at PR3 implementation time.
- **Whether `EnableConfigurationBindingGenerator=false` can be removed in PR2** — depends on whether .NET 10's source generator handles this codebase's `Configure<T>` calls.
- **Whether `app.UseStaticFiles()` → `app.MapStaticAssets()` is needed** — depends on whether the .NET 10 Blazor WASM static-asset regression manifests in PR2 smoke testing.
- **Which exact request types in `Handlers/` warrant `record class` over `record struct`** — decided per-handler during PR2 implementation based on copy semantics of their parameter types.

## 8. Implementation skill handoff

Once this spec is approved, hand off to `superpowers:writing-plans` to produce the per-PR implementation plan. The plan should produce three discrete checklists (one per PR) that an implementing agent or human can execute step-by-step with explicit checkpoints.
