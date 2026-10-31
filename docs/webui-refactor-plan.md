# TaikoWebUI refactor plan

Branch: `webui-refactor` (off `dev`). Status: **planning**, nothing implemented yet.

Three workstreams, in the order they should land:

1. **Audit fixes** (auth + perf + dead weight) — small, independent, ship first on MudBlazor.
2. **Game-data extractor (`nijiiro-extract`)** — one command-line tool that takes the operator's game folder and produces every file the server and WebUI need, including the 3D don-chan assets.
3. **UI kit migration + 3D don-chan previewer** — page-by-page rewrite on the new kit, with the Profile costume tab swapped from stacked PNGs to the three.js previewer.

---

## 1. Audit

Scope: `TaikoWebUI/` (~4.6k lines of razor/C#) and the admin auth endpoints it calls (`Adapters.AdminApi/Controllers/AuthController.cs`).

### 1a. Security (server side — fix regardless of UI work)

| # | Finding | Where | Fix |
|---|---|---|---|
| S1 | **No rate limiting on `Login` / `Register`.** Invite codes are 6-digit TOTPs accepted across 25 one-hour windows, so a register attempt guesses right ~1 in 40k. Brute-forcing that, or passwords, over HTTP is cheap. | `AuthController` | ASP.NET Core built-in `AddRateLimiter` + `[EnableRateLimiting("auth")]` (fixed window per IP, ~10/min). No new package. |
| S2 | **Without `RegisterWithLastPlayTime`, anyone who knows an access code can claim that account.** Access codes are printed on the card. | `AuthController.Register` | **Decided:** replace the `RegisterWithLastPlayTime` bool with `AuthSettings.RegistrationMode` = `InviteCode` (**default**) \| `LastPlayTime` \| `InviteCodeOrLastPlayTime` \| `Open`. `Open` keeps today's behaviour for operators who want it, with a warning comment in `AuthSettings.json`. Reading the old bool still works (`true` → `InviteCodeOrLastPlayTime`, which is today's semantics), so existing configs don't break. The client gets the mode through `ClientAuthConfigResponse` so the Register page shows the right fields. |
| S3 | **Username enumeration.** Login returns different messages for "Access Code Not Found", "User Not Registered" and "Invalid Password". | `AuthController.Login` | Return a single "invalid credentials" message. Keep "not registered" only if the UX needs it (it's a hint to go register). |
| S4 | Password check re-hashes and compares strings (`credential.Password != hashedPassword`). That works, but the compare isn't constant-time and it hand-rolls BCrypt. | `Login`, `ChangePassword` | `BCrypt.Net.BCrypt.Verify(password, credential.Password)`. The salt column becomes redundant (BCrypt embeds it) but leave it for now so no migration is needed. |
| S5 | `ChangePassword` takes the access code from the request body even though the caller is authenticated. | `AuthController.ChangePassword`, `AuthService.ChangePassword` | Derive the BAID from the token (`User.GetBaid()`), and drop `AccessCode` from `ChangePasswordRequest`. |
| S6 | DB queries live in the controller, against the CLAUDE.md rule ("never query `ITaikoDbContext` directly from a controller"). | `AuthController` | Move into `Application` handlers (`LoginCommand`, `RegisterCommand`, …). Do it while fixing S1–S5, since the same code is being touched. |

### 1b. Login / auth state (client)

The app has **two sources of truth** for auth: `JwtAuthenticationStateProvider` (the Blazor-native one) and the `AuthService` facade with its own `IsLoggedIn` / `IsAdmin` / `LoginStatusChanged` event. The facade's own doc comment calls itself transitional.

| # | Finding | Fix |
|---|---|---|
| A1 | `AuthService` constructor fires `_ = RefreshFromStateAsync(...)` (fire-and-forget). The first render can read stale `IsLoggedIn=false`, and exceptions get swallowed. | Delete the facade's state. Components use `<AuthorizeView>` and `[CascadingParameter] Task<AuthenticationState>`. `AuthService` keeps only the HTTP calls (Login/Register/ChangePassword/Bind). |
| A2 | `GetAuthenticationStateAsync()` reads localStorage, re-parses the JWT and **mutates `HttpClient.DefaultRequestHeaders`** on every call. It's a getter with side effects, and every `AuthorizeView` triggers it. | Cache the principal in a field and refresh it only on login/logout/expiry. Attach the bearer with a `DelegatingHandler` that reads the cached token. |
| A3 | No 401 handling. When the 24h token expires mid-session, calls fail silently and the user still looks logged in. | The same `DelegatingHandler`: on a 401, call `MarkUserAsLoggedOutAsync()` and navigate to `/Login?returnUrl=…`. |
| A4 | No `returnUrl`. A deep link means login, then a dump onto `/Users`. | `RedirectToLogin` passes `returnUrl`, and Login navigates back to it (local paths only, so it can't become an open redirect). |
| A5 | `Login.razor` calls `NavigationManager.NavigateTo("/")` **inside the render tree** (the `else` branch). | Move it to `OnInitialized`, or let a policy redirect handle it. |
| A6 | Magic `int` results (`Login` → 1..5, `Register` → 0..6, …) plus a `switch` per page, and `case 0` in `Login` can't happen. | Use a single `enum AuthResult` (or return the localizer key directly). |
| A7 | `HandleKeyDown` + `debounce` hack to submit on Enter. | Wrap the fields in a real `<form @onsubmit>` / `EditForm`, so Enter submits natively. The new kit handles this either way. |
| A8 | `Local` mode synthesizes an admin principal, and the facade then reports `IsLoggedIn=false`. The `IsAdmin \|\| !LoginRequired` idiom is repeated in NavMenu/pages. | Once A1 lands, the synthetic principal is the only path: `<AuthorizeView Policy="Admin">` works the same in both modes. |
| A9 | The token lives in localStorage (XSS-readable). | Acceptable for a LAN tool with no third-party scripts. **Note it, don't change it.** Moving to an HttpOnly cookie would need server cookie auth plus CSRF, which is out of scope. |

### 1c. Performance / payload

| # | Finding | Fix | Win |
|---|---|---|---|
| P1 | `BlazorWebAssemblyLoadAllGlobalizationData=true` ships full ICU (~several MB). | Remove it. The 5 supported cultures are covered by the default sharded ICU (`en`/`fr`/`ja`/`zh` ship in `icudt_CJK` + `icudt_EFIGS`). Verify that date/number formatting still works in `ja`/`zh-Hant`. | Large first-load cut |
| P2 | `WasmEnableSIMD=false`, `WasmEnableExceptionHandling=false` turn off .NET 10 defaults that make WASM faster and smaller. Probably a leftover from an old-browser workaround. | Delete both lines. If a target cabinet browser is old, document which one. | Faster runtime |
| P3 | Unused packages: `Swan.Core`, `SharpZipLib`, `Otp.NET`, `CodeBeam.MudBlazor.Extensions` (no references in WebUI). | Remove them. `System.IdentityModel.Tokens.Jwt` is also heavy for "decode payload + read `exp`", so replace it with a ~15-line base64url decode. | Smaller bundle |
| P4 | Startup is serial: appsettings + auth config, **then** `dan_data.json`, **then** a culture JS call, then render. | Make dan data lazy like the other catalogs (only DaniDojo uses it). Read the culture alongside the other bootstrap fetches. | Faster first paint |
| P5 | `GameDataService` lazy init has no concurrency guard, so two components on one page can both fetch `MusicDetails` (it's big). | Cache the `Task` (`musicTask ??= Load()`), not a bool flag. | Fewer duplicate fetches |
| P6 | `GetHeadTitle/GetBodyTitle/...` are 6 copy-pasted methods with 7-arm switches, each doing a linear `FirstOrDefault` over the whole costume list. Profile calls them per dropdown item, which makes it O(n²) per render. | One `GetCostumeName(type, id, lang)` over a `Dictionary<(string,uint),Costume>`, and one shared `lang → field` selector. The `"jp"` vs `"ja"` key mismatch in some of them is a **bug**: those arms never match. | Less code, and a bug fixed |
| P7 | `index.html` pulls Roboto from Google Fonts. The server is often on an offline LAN or cabinet network. | Self-host it or drop it (the new kit decides). | Works offline |
| P8 | `Nijiiro.woff2` is 3.2 MB. | Subset it to the glyphs actually used (pyftsubset), or load it only on pages that show song titles. | -3 MB |
| P9 | **21 MB of costume PNG/WebP and nameplate art committed under `wwwroot/images`.** It's game art in the repo, and it ships in every publish. | Generate it from the operator's game files with `nijiiro-extract` (§2). The 3D previewer replaces the costume layers. Nameplates and rank panels come from `Data/x64/lumen` / `textures` later. | Repo/publish -21 MB, and no game art in git |
| P10 | `textFit.min.js` + `updateTextFit.js` for nameplate text sizing. | CSS `clamp()` / container query units will probably replace it. Check during the Profile rewrite. | One less script |
| P11 | csproj has a stale `Pages\Pages\...` `_ContentIncludedByDefault` / `UpToDateCheckInput` block, and copy rules for `wwwroot\data\datatable\*` that WebUI doesn't own. | Delete them. | Tidy |

### 1d. Smaller cleanups (do while touching each page)

- `BreadcrumbsStateContainer` has public mutable `breadcrumbs` and every page clears and re-adds them by hand. Use a `<PageBreadcrumbs Items=...>` component, or derive the trail from the route.
- `MainLayout` reads `drawerOpen` and `isDarkMode` from localStorage with 4 awaited JS round-trips. Use one key holding a small JSON object, and default to `prefers-color-scheme`.
- `Profile.razor.cs` (479 lines) hardcodes 63 `CostumeColors` plus 63 hand-tuned CSS `filter:` strings for tinting. The 3D previewer tints from the real `don_color.bin` palette, so **all of that goes away**.
- `alert("Copied to clipboard!")` becomes a snackbar/toast.

---

## 2. Game-data extractor (`nijiiro-extract`)

### What the operator does today

From `README.md`: copy 6 datatable `.bin` files into `Host/wwwroot/data/datatable/` by hand. The WebUI art is committed in the repo.

### Target

There's one standalone command-line tool that extracts everything, the don-chan model included:

```bash
nijiiro-extract --game "G:\...\Taiko no Tatsujin Nijiiro Version (SBWY 39.06)" --out "<server-dir>\wwwroot"
```

It validates the folder, then writes everything into `<exe-dir>/wwwroot/` and says what it found and what was missing. It's idempotent, and it skips anything already up to date (by size + mtime).

### What it pulls (verified against the 39.06 install)

All paths are relative to `Data/x64/`. `Data_mods/` is ignored.

| Source | Count / size | Output | Used by |
|---|---|---|---|
| `datatable/{musicinfo,music_order,wordlist,don_cos_reward,shougou,neiro}.bin` | 6 files | copied as-is to `wwwroot/data/datatable/` (the server already decrypts them on startup) | server catalog |
| `datatable/don_cos.bin`, `don_color.bin` | 2 files | copied as-is. The server decrypts them on startup like the other tables, so the CLI needs no keys, and exposes them via `api/GameData/...` | previewer: costume ID → model file map and the color palette (replaces the hardcoded `CostumeColors`) |
| `model/{body,head,cos}/<name>/` (`.numdlb/.numshb/.nusktb/.numatb` + `textures/*.nutexb`) | 163 body, 140 head, 179 cos (~340 MB raw) | one `.glb` per part, to `wwwroot/don-model/{body,head,cos}/` | previewer |
| `model/{face,paint,acce}/*.nutexb` | 22 face, 58 paint, 149 acce | `.png` (or `.webp`), to `wwwroot/don-model/{face,paint,acce}/` | previewer |
| `animation/*.nuanmb` | 54 files | `Don_3D_Nij.glb` + `facekeys.json` (the existing `nuanmb_to_glb` tool) | previewer |
| (later) `lumen/`, `textures/` | nameplates, rank panels | `.png` | Profile nameplate (replaces the committed `images/Nameplates`) |

Expected output size is ~230 MB (the previewer's converted `public/don-model` today is 227 MB). It's served lazily per costume, so the browser only downloads what's on screen.

### How the conversion is built

Every 3D asset is in Namco's **SSBH** format (the same family as Smash Ultimate). The previewer repo already has a Rust tool for the animation half (`scripts/nuanmb_to_glb`, built on `ssbh_lib`/`ssbh_data`). The same ecosystem covers the rest: `ssbh_data` reads mesh/skel/matl, and the `nutexb` + `image_dds` crates decode textures.

**Model export is the new piece.** The current `body/head/cos` GLBs came from a closed-source tool (`numd-exporter.exe`, usage `numd-exporter <input_file> <output_file>`). We're writing our own open-source exporter. It's a **clean-room** build on `ssbh_data`: we use the closed tool only as a black box (run it on a few models and diff its GLB output against ours: node names, skin, material names/extras, UV sets), and we don't decompile it. The diff tells us which quirks `normalize.ts` relies on.

The plan is one Rust CLI, `nijiiro-extract`, that grows out of `nuanmb_to_glb` and lives in **this repo at `tools/nijiiro-extract/`** (a Rust crate; `nuanmb_to_glb` moves there from `don-chan-previewer/scripts/`). Its subcommands:

- `nijiiro-extract --game <dir> --out <wwwroot>`: everything below, in order, with a summary of what it found and what was missing.
- `datatables`: copies the `.bin` files (plain file copy).
- `models`: `body`/`head`/`cos` → `.glb`, via `ssbh_data` mesh/skel/matl plus the `gltf` writer.
- `textures`: `face`/`paint`/`acce` `.nutexb` → `.png`, via `nutexb` + `image_dds`.
- `anims`: the existing `nuanmb_to_glb` logic → `Don_3D_Nij.glb` + `facekeys.json`.

Then:

- This repo's CI (`.github/workflows`) builds `tools/nijiiro-extract` with `cargo build --release` on the Windows runner, in the same publish job, and drops the exe next to `TaikoLocalServer.exe`. No cross-repo release fetching is needed.
- **README:** the top-level `README.md` setup section swaps "copy 6 datatable files" for a "Run `nijiiro-extract`" section (what it needs, the exact command, what it writes, how to re-run it after a game update, and what to do when it reports a missing file). `tools/nijiiro-extract/README.md` holds the developer details (subcommands, building, format notes; it absorbs the current `nuanmb_to_glb` README). `Host/README.md` notes the new `wwwroot/don-model/` folder.
- No C# setup command is needed. The server keeps owning decryption and only needs `don_cos` + `don_color` added to `FileGameDataCatalog`.

Rejected alternatives:
- Port SSBH parsing to C#: a lot of reverse-engineered format code exists only in Rust, so this means rewriting it for no gain.
- A PowerShell script: it can't decode `.nutexb`/BCn, so it would still need the Rust binary.

**Open questions:**
- What does `numd-exporter` emit that `normalize.ts` (782 lines of material fixes) depends on? The black-box diff answers this. Our exporter either matches it or `normalize.ts` gets simpler.
- Which models actually need the `nusrcmdlb` / `nuhlpb` / `nusubf/` files, and does the exporter need them?
- How does `don_cos.bin` map `UserSetting.Body/Head/Kigurumi/Face/Puchi` IDs to file names? The previewer's `cosinfo.json` suggests `dataId = uniqueId * 1000` → `cos_{dataId:000000}`, plus variant suffixes (`cos_036001`). Is puchi `acce`? The counts suggest so (149 vs 149).

---

## 3. UI kit + 3D previewer

### 3a. Kit options (Blazor WASM, .NET 10)

Be honest about the baseline first. **MudBlazor 9 is already a current, maintained, component-rich kit.** Switching rewrites ~4.6k lines of razor. It's worth it only if the look/feel is the actual goal. Re-theming MudBlazor (custom palette, typography, radius, density) gets maybe 70% of "modern" for ~5% of the work.

If switching:

**Tailwind / shadcn-style (the modern look)**

| Kit | License | Breadth | Delivery | Notes |
|---|---|---|---|---|
| **Blazor Blueprint** | OSS | 115+ components | NuGet | shadcn/ui port. Tailwind is optional: themes are shadcn CSS variables, so you can paste themes from ui.shadcn.com/themes or tweakcn.com. Strong "modern + broad" candidate. |
| **Lumeo** | OSS | 130+ components, 8 themes, dark mode | CLI copies the source into the repo | Tailwind v4, .NET 10. You own the component code, which is easy to tweak and has no package churn, but upstream fixes have to be pulled by hand. |
| **BlazorCN** | OSS | ~200 components / 55 groups (per NuGet) | NuGet | Aims for 1:1 shadcn parity, with C# ports of `cva` and `tailwind-merge`. Newest; check its maturity. |
| **ShellUI** | OSS | 73 components | CLI copies the source | Tailwind v4 via the standalone CLI, so **no Node.js** in the build. |
| **LumexUI** | MIT | Medium (Data Grid, Datebox, Toast, …) | NuGet (`LumexUI` 2.3.x) | Tailwind, HeroUI/NextUI-inspired. The most polished "app" look of this group, but fewer components. |
| **Blazorise + Tailwind provider** | **Commercial** for companies/freelancers. Community is free for individuals only (LinkedIn-verified) | 90+ core | NuGet | ⚠ **Not a good fit for an OSS project.** Without a commercial license, DataGrid is capped at **1,000 rows** and charts at **10 points**. HighScores/PlayHistory/Users will pass 1,000 rows on a busy server. The license doesn't say whether OSS contributors qualify for Community. The abstraction layer also means you're styling Blazorise's markup, not writing Tailwind. |

**Classic component suites**

| Kit | License | Breadth | Look | Notes |
|---|---|---|---|---|
| **Radzen.Blazor** | MIT (Material 3 / Fluent themes are Pro) | Very large (DataGrid, charts, scheduler, …) | Material / Standard / Humanistic / Software | Mature (9.x, .NET 10). The safest "lots of components" pick. |
| **Fluent UI Blazor v5** (Microsoft) | MIT | Medium-large | Fluent 2 | v5 hit RC3 in May 2026 (API stable). It has a theme designer, toast, autocomplete, wizard and DataGrid pinned columns. First-party. |
| **AntDesign Blazor** | MIT | Large | Ant Design | Solid, but looks enterprise-y. |
| **BootstrapBlazor** | Apache-2.0 | 200+ | Bootstrap 5 | Huge, but looks like an admin template. |
| Syncfusion / Telerik / DevExpress | Commercial | Largest | Polished | Not for an OSS project. |

**Tailwind build note (applies to any Tailwind kit):** use the **standalone Tailwind CLI** binary from an MSBuild `<Exec>` target. That way there's no Node.js in the .NET build or CI, and the generated CSS is a static file served offline. That fixes P7 too.

**Decided: LumexUI** (`LumexUI` on NuGet, MIT, Tailwind v4, .NET 8+).

#### LumexUI fit check

These are the MudBlazor components the WebUI uses today (counts from `grep <Mud*`), mapped to their replacements:

| Today (MudBlazor) | Uses | LumexUI | Gap → plan |
|---|---|---|---|
| `MudText`, `MudGrid/Item`, `MudStack`, `MudContainer`, `MudPaper`, `MudSpacer` | ~360 | none (it's layout) | Plain Tailwind utility classes (`flex`, `grid`, `gap-*`, `text-*`). This is most of the diff, but it's mechanical. |
| `MudTable` + `MudTh/Td/TableSortLabel/TablePager` | 7 tables | **Data Grid** | Check it for sorting + paging. If paging is missing, add a small `Pager` component (~40 lines). |
| `MudCard*` | 28 | **Card** | 1:1 |
| `MudButton`, `MudIconButton`, `MudToggleIconButton` | 25 | **Button** | 1:1 |
| `MudTextField`, `MudInput` | 19 | **Textbox** / **Numbox** | 1:1 |
| `MudSelect/SelectItem` | 19 | **Select** / **Listbox** | 1:1. The costume pickers (100+ items) need a searchable select; check whether LumexUI's Select filters. |
| `MudSwitch`, `MudChip`, `MudTooltip`, `MudDivider`, `MudSkeleton`, `MudTabs/TabPanel` | ~30 | **Switch, Chip, Tooltip, Divider, Skeleton, Tabs** | 1:1 |
| `MudMenu/MenuItem` | 15 | **Dropdown** | 1:1 |
| `MudDialog` + `DialogService.ShowMessageBoxAsync` | 6 dialogs + many message boxes | **Modal** | LumexUI has a declarative Modal but (as far as the docs show) no `DialogService`. Add one small `ConfirmService` + `<ConfirmHost/>` (~60 lines) for the message-box calls. |
| `MudSnackbarProvider` | 1 | **Toast Provider** | 1:1 (also replaces `alert("Copied")`) |
| `MudDatePicker`, `MudTimePicker` | 2 (Register) | **Datebox** + native `<input type="time">` | Fine for one form |
| `MudProgressLinear/Circular` | 16 | **Spinner** + native `<progress>` | Fine |
| `MudSlider` | 1 | none | Native `<input type="range">` styled with Tailwind `accent-*` |
| `MudToggleGroup` | 1 | **Radio Group** (or Button group) | Fine |
| `MudLayout/AppBar/Drawer/NavMenu/NavLink/NavGroup/MainContent` | shell | **Navbar** + **Accordion** | The sidebar drawer is hand-rolled: a Tailwind `aside` plus a toggle, ~50 lines. It's one component, written once. |
| `MudBreadcrumbs` | 1 | none | A ~15-line component (this also fixes the `BreadcrumbsStateContainer` cleanup in §1d) |
| `MudPagination` | 2 | none | The same `Pager` as above |
| `MudScrollToTop` + `MudFab` | 1 | none | Drop it. Browsers and phones already scroll to the top. |
| `MudIcon` / `Icons.Material.*` | 21+ | **no icon set** | Add **inline SVG icons** in a tiny `Icon` component, using Lucide SVGs (ISC license) copied in for only the ~25 icons used. No icon package. |
| `MudThemeProvider` (dark mode) | 1 | Tailwind `dark:` + LumexUI theme tokens | Toggle `class="dark"` on `<html>`, defaulting to `prefers-color-scheme` |
| `MudImage` | 15 | none | Plain `<img>`. Most of these are the costume layers that the 3D previewer deletes anyway. |
| `MudLocalizer` / `ResXMudLocalizer` | | none | Delete it. The `.resx` + `IStringLocalizer` stay. |

There's no blocker. The things we hand-write are confirm dialogs, the sidebar, breadcrumbs, a pager, icons and a slider, about 250 lines in total, all in `TaikoWebUI/Components/Ui/`.

#### Tailwind build

- Use the standalone **Tailwind v4 CLI** (`tailwindcss.exe`, no Node). Pin its version and download it in CI.
- Add an MSBuild target in `TaikoWebUI.csproj` that runs before build: `tailwindcss -i Styles/app.css -o wwwroot/css/app.css --minify`.
- In `Styles/app.css`: `@import "tailwindcss"; @import "../bin/lumexui/theme"; @source "../bin/lumexui/*.cs";` plus `@source "../**/*.razor"`.
- Brand the theme by overriding LumexUI's CSS variables (Taiko red/orange primary, Nijiiro font for display headings only).
- Open item: confirm how `bin/lumexui/` gets populated (the package presumably copies it at build). It's undocumented on the install page, so verify it in the spike.

#### Spike before committing to the full migration

Port **Login + Users + one table page (HighScores)** first. They exercise forms, a modal, toasts, the Data Grid (sort/page with 1k+ rows), the shell/sidebar and dark mode. If the Data Grid falls short on 1k+ rows, the fallback is a plain Tailwind `<table>` with our `Pager`.

### 3b. Migration approach

- Do it in place on this branch, page by page. Keep MudBlazor installed until the last page moves, then remove it (both kits can coexist; only the theme/provider components need care).
- Order (simple to complex): Login/Register/ChangePassword → Dashboard → Users → AccessCode → SongList/Song → HighScores/PlayHistory → DaniDojo → Profile (last, because it gets the 3D previewer).
- `ResXMudLocalizer` gets deleted. The `.resx` files stay.
- MudBlazor and LumexUI coexist during the migration: MudBlazor's CSS is scoped to `.mud-*` classes. Tailwind's preflight reset may restyle unported Mud pages slightly, which is acceptable on a dev branch.
- Contracts and the admin API don't change (except S5).

### 3c. 3D don-chan in Profile

The previewer is React 19 + react-three-fiber + Mantine + tweakpane. Embedding React inside Blazor is the wrong move.

**Recommendation: an iframe embed build.**

1. In `don-chan-previewer`, add a second Vite entry, `embed.html`. It renders only `<DonCharacter>` on a transparent canvas (no Mantine, tweakpane, debug panel or stats) and reads its state from the URL plus `postMessage`: `{ body, head, kigurumi, face, puchi, bodyColor, faceColor, limbColor, anim }`.
2. Its build output (`dist-embed/`, a few hundred KB of JS) is vendored into `TaikoWebUI/wwwroot/don-preview/` by a script, or pulled as a CI artifact.
3. A Blazor `<DonPreview Setting="@userSetting" />` component renders the iframe and `postMessage`s on every change. That's about 20 lines of C# and one JS interop call, and the costume dropdowns update the model live.
4. Model URLs point at `/don-model/...` produced by §2. If the assets are missing (extractor not run), the component shows a "run nijiiro-extract" notice instead of broken images.

Why an iframe: it fully isolates the React/three.js runtime from Blazor's DOM diffing, needs no build coupling between the two repos beyond copying static files, and keeps the previewer independently developable.

Deletes: `wwwroot/images/Costumes/**`, the `Masks` list, `CostumeColors`, `CostumeColorFilters`, and `CostumeOrDefault` in `Profile.razor.cs`.

---

## Phasing / PRs

| PR | Contents | Depends on |
|---|---|---|
| 1 | Server auth: S1–S6 (incl. `RegistrationMode`) | none |
| 2 | Client auth: A1–A8 | 1 (S5) |
| 3 | Perf: P1–P5, P7, P11 (+ P6 bug fix) | none |
| 4 | `tools/nijiiro-extract`: move `nuanmb_to_glb` in, add the open-source model exporter + texture export, its README | open questions answered |
| 5 | Server: load `don_cos`/`don_color`, build + bundle `nijiiro-extract.exe` in CI publish, top-level README "Run nijiiro-extract" section | 4 |
| 6 | Previewer `embed` entry (in `don-chan-previewer`) | none (can start now with existing GLBs) |
| 7 | LumexUI spike: Tailwind CLI build, shell, Login, Users, HighScores | 2 |
| 8+ | Remaining pages, Profile + `<DonPreview>` last; delete `images/Costumes`, remove MudBlazor | 5, 6, 7 |

PRs 1, 3, 4 and 6 are independent and can run in parallel.

## Decisions needed

1. ~~UI kit~~: decided, **LumexUI** (§3a fit check + spike plan).
2. ~~S2~~: decided. `RegistrationMode` option, default `InviteCode`.
3. ~~Extractor repo home~~: decided, `tools/nijiiro-extract/` in this repo.
4. ~~Model GLB provenance~~: decided. They came from the closed-source `numd-exporter`, and we'll write an open-source replacement (clean-room, black-box diffing only).
