# TaikoWebUI refactor plan

Branch: `webui-refactor` (off `dev`). Status: **planning**, nothing implemented yet.

Three workstreams, in the order they should land:

1. **Audit fixes** (auth + perf + dead weight) — small, independent, ship first on MudBlazor.
2. **Game-data extractor (`extract`)** — one command-line tool that takes the operator's game folder and produces every file the server and WebUI need, including the 3D don-chan assets.
3. **UI kit migration + 3D don-chan previewer** — page-by-page rewrite on the new kit, with the Profile costume tab swapped from stacked PNGs to the three.js previewer.

---

## 1. Audit

Scope: `TaikoWebUI/` (~4.6k lines of razor/C#) and the admin auth endpoints it calls (`Adapters.AdminApi/Controllers/AuthController.cs`).

### 1a. Security (server side — fix regardless of UI work)

| # | Finding | Where | Fix |
|---|---|---|---|
| S1 | **No rate limiting on `Login` / `Register`.** Invite codes are 6-digit TOTPs accepted across 25 one-hour windows, so a register attempt guesses right ~1 in 40k. Brute-forcing that, or passwords, over HTTP is cheap. | `AuthController` | ASP.NET Core built-in `AddRateLimiter` + `[EnableRateLimiting("auth")]` (fixed window per IP, ~10/min). No new package. |
| S2 | **Without `RegisterWithLastPlayTime`, anyone who knows an access code can claim that account.** Access codes are printed on the card. | `AuthController.Register` | **Deferred to the accounts phase (§1a-3).** A `RegistrationMode` setting was tried in PR 1 and reverted. |
| S3 | **Username enumeration.** Login returns different messages for "Access Code Not Found", "User Not Registered" and "Invalid Password". | `AuthController.Login` | Return a single "invalid credentials" message. Keep "not registered" only if the UX needs it (it's a hint to go register). |
| S4 | Password check re-hashes and compares strings (`credential.Password != hashedPassword`). That works, but the compare isn't constant-time and it hand-rolls BCrypt. | `Login`, `ChangePassword` | `BCrypt.Net.BCrypt.Verify(password, credential.Password)`. The salt column becomes redundant (BCrypt embeds it) but leave it for now so no migration is needed. |
| S5 | `ChangePassword` takes the access code from the request body even though the caller is authenticated. | `AuthController.ChangePassword`, `AuthService.ChangePassword` | Derive the BAID from the token (`User.GetBaid()`), and drop `AccessCode` from `ChangePasswordRequest`. |
| S6 | DB queries live in the controller, against the CLAUDE.md rule ("never query `ITaikoDbContext` directly from a controller"). | `AuthController` | **Deferred.** All 9 admin controllers query the DB directly and none use Mediator, so moving only auth would leave a one-off. Do it later as one admin-API-wide refactor. |
| S7 | No self-service password reset. The only path is an admin clearing the password on the Users page. | `AuthController` | **Deferred to the accounts phase (§1a-3).** A forgot-password flow was tried in PR 1 and reverted. |

### 1a-3. Accounts phase (later)

Registering with your last play time is the wrong model. This phase replaces it with an **admin panel plus an admin API**: admins create and claim accounts for cards, issue password resets and one-time login links, and see why a login failed (troubleshooting). Players never prove ownership through play data. The PR 1 admin CLI (`promote`, `demote`, `set-password`) is the bootstrap until the panel exists, and the panel calls the same operations through the API. 2FA and magic links (§1a-2) fold into this phase.

### 1a-2. 2FA and magic-link login (follow-up PR, toggleable)

Users have no email address, so there's nothing to send a magic link to. Both features come from what the server already has:

- **TOTP 2FA (authenticator app).** `AuthSettings.TwoFactor = Off | Optional | RequiredForAdmins | Required`.
  - Add a per-user `TotpSecret` (nullable) and `RecoveryCodes` (hashed) to `Credential`, with a new migration.
  - Enroll from Profile → Security: show a QR with an `otpauth://` URI (QRCoder is already in the WebUI), then confirm one code.
  - Login becomes two steps: the password returns a short-lived "2FA pending" token, and `POST api/Auth/Verify2fa` exchanges it for the real JWT.
  - `Otp.NET` is already on the server.
- **Magic link / device login.** `AuthSettings.MagicLink = Off | AdminIssued | DeviceHandoff`.
  - *AdminIssued:* an admin clicks "login link" on a user and gets a one-time URL (`/Login/Link?t=…`, random 32-byte token, stored hashed, 15-minute expiry, single use) to share however they like. This replaces "reset password and tell them to re-register".
  - *DeviceHandoff:* a logged-in device shows a QR code, and scanning it on a phone logs the phone in. It uses the same one-time token table.
  - Email delivery stays out of scope unless users get an email field.

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
| P9 | **21 MB of costume PNG/WebP and nameplate art committed under `wwwroot/images`.** It's game art in the repo, and it ships in every publish. | Generate it from the operator's game files with `extract` (§2). The 3D previewer replaces the costume layers. Nameplates and rank panels come from `Data/x64/lumen` / `textures` later. | Repo/publish -21 MB, and no game art in git |
| P10 | `textFit.min.js` + `updateTextFit.js` for nameplate text sizing. | CSS `clamp()` / container query units will probably replace it. Check during the Profile rewrite. | One less script |
| P11 | csproj has a stale `Pages\Pages\...` `_ContentIncludedByDefault` / `UpToDateCheckInput` block, and copy rules for `wwwroot\data\datatable\*` that WebUI doesn't own. | Delete them. | Tidy |

### 1d. Smaller cleanups (do while touching each page)

- `BreadcrumbsStateContainer` has public mutable `breadcrumbs` and every page clears and re-adds them by hand. Use a `<PageBreadcrumbs Items=...>` component, or derive the trail from the route.
- `MainLayout` reads `drawerOpen` and `isDarkMode` from localStorage with 4 awaited JS round-trips. Use one key holding a small JSON object, and default to `prefers-color-scheme`.
- `Profile.razor.cs` (479 lines) hardcodes 63 `CostumeColors` plus 63 hand-tuned CSS `filter:` strings for tinting. The 3D previewer tints from the real `don_color.bin` palette, so **all of that goes away**.
- `alert("Copied to clipboard!")` becomes a snackbar/toast.

---

## 2. Game-data extractor (`extract`)

### What the operator does today

From `README.md`: copy 6 datatable `.bin` files into `Host/wwwroot/data/datatable/` by hand. The WebUI art is committed in the repo.

### Target

There's one standalone command-line tool that extracts everything, the don-chan model included:

```bash
extract --game "G:\...\Taiko no Tatsujin Nijiiro Version (SBWY 39.06)" --out "<server-dir>\wwwroot"
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
| `animation/*.nuanmb` | 54 files | `don.glb` + `facekeys.json` (the existing `nuanmb_to_glb` tool) | previewer |
| (later) `lumen/`, `textures/` | nameplates, rank panels | `.png` | Profile nameplate (replaces the committed `images/Nameplates`) |

Expected output size is ~230 MB (the previewer's converted `public/don-model` today is 227 MB). It's served lazily per costume, so the browser only downloads what's on screen.

### How the conversion is built

Every 3D asset is in Namco's **SSBH** format (the same family as Smash Ultimate). The previewer repo already has a Rust tool for the animation half (`scripts/nuanmb_to_glb`, built on `ssbh_lib`/`ssbh_data`). The same ecosystem covers the rest: `ssbh_data` reads mesh/skel/matl, and the `nutexb` + `image_dds` crates decode textures.

**Model export is the new piece.** The current `body/head/cos` GLBs came from a closed-source tool (`numd-exporter.exe`, usage `numd-exporter <input_file> <output_file>`). We're writing our own open-source exporter. It's a **clean-room** build on `ssbh_data`: we use the closed tool only as a black box (run it on a few models and diff its GLB output against ours: node names, skin, material names/extras, UV sets), and we don't decompile it. The diff tells us which quirks `normalize.ts` relies on.

The plan is one Rust CLI, `extract`, that grows out of `nuanmb_to_glb` and lives in **this repo at `tools/extract/`** (a Rust crate; `nuanmb_to_glb` moves there from `don-chan-previewer/scripts/`). Its subcommands:

- `extract --game <dir> --out <wwwroot>`: everything below, in order, with a summary of what it found and what was missing.
- `datatables`: copies the `.bin` files (plain file copy).
- `models`: `body`/`head`/`cos` → `.glb`, via `ssbh_data` mesh/skel/matl plus the `gltf` writer.
- `textures`: `face`/`paint`/`acce` `.nutexb` → `.png`, via `nutexb` + `image_dds`.
- `anims`: the existing `nuanmb_to_glb` logic → `don.glb` + `facekeys.json`.

Then:

- This repo's CI (`.github/workflows`) builds `tools/extract` with `cargo build --release` on the Windows runner, in the same publish job, and drops the exe next to `TaikoLocalServer.exe`. No cross-repo release fetching is needed.
- **README:** the top-level `README.md` setup section swaps "copy 6 datatable files" for a "Run `extract`" section (what it needs, the exact command, what it writes, how to re-run it after a game update, and what to do when it reports a missing file). `tools/extract/README.md` holds the developer details (subcommands, building, format notes; it absorbs the current `nuanmb_to_glb` README). `Host/README.md` notes the new `wwwroot/don-model/` folder.
- No C# setup command is needed. The server keeps owning decryption and only needs `don_cos` + `don_color` added to `FileGameDataCatalog`.

Rejected alternatives:
- Port SSBH parsing to C#: a lot of reverse-engineered format code exists only in Rust, so this means rewriting it for no gain.
- A PowerShell script: it can't decode `.nutexb`/BCn, so it would still need the Rust binary.

### Extractor audit (done 2026-10-31, against SBWY 39.06)

All of the earlier open questions are answered.

**Toolchain.** Rust 1.93.1 (rustup) is installed on the dev machine. GitHub's `windows-latest` runners ship with Rust, so CI needs no setup step. Crates:
- `ssbh_lib` / `ssbh_data` (git, MIT) are already used by `nuanmb_to_glb`.
- Textures: `nutexb` + `image_dds` (MIT).
- Encoding: `gltf-json` (or hand-written JSON, like `nuanmb_to_glb` does today) + `png`.

**Source format.** Every one of the 483 models has the same layout:

```
model/<cat>/<name>/
  <name>/<name>.numdlb            MODL v1.7: points at the files below, plus mesh object → material pairs
  <name>/<name>.nusrcmdlb         MODL v1.7: same as .numdlb minus the nust ref (ignore)
  <name>/<name>.nuhlpb            HLPB v1.1: helper bones (ignore for preview)
  <name>/nusubf/<name>__maya__.numshb   MESH v1.9: geometry
  <name>/nusubf/<name>__maya__.nusktb   SKEL v1.0: the shared 40-joint don rig (BONE_root, BASE, ARM_L0…)
  <name>/nusubf/<name>__maya__.numatb   MATL v1.5: material → texture path (../../textures/<tex>)
  <name>/nusubf/<name>__nust__.numatb   MATL v1.5: material → shader label (taikoEffect / taikoEffectChangeColors / taikoEffectFace)
  <name>/nusubf/<name>__maya__.nuanmb   ANIM v1.2: rest/idle pose (ignore)
  textures/*.nutexb                     1–5 per model, incl. shared RGB_don_* colour masks
```

- These are the same SSBH versions as Smash Ultimate, so `ssbh_data` parses MODL, MESH, SKEL and MATL as-is. Only ANIM v1.2 needs the custom decoder that `nuanmb_to_glb` already has.
- All 1,819 `.nutexb` are format `0xA0` (BC3) or `0x00` (R8), with a 176-byte footer and the ` XET` magic.
- Sizes are mostly 512² / 256×128 / 128×8 / 8×8.

**Target GLB.** This is a black-box diff of `numd-exporter` against the previewer's `cos_001000.glb`. Ours must match it, because `normalize.ts` keys on it:
- One skinned mesh node per MESH object, named `DON_O_OBJShape-<n>`. Attributes are `POSITION, NORMAL, COLOR_0, TEXCOORD_0, JOINTS_0, WEIGHTS_0`. All 40 joints come from the SKEL, with node names kept.
- One material per mesh, **named with the MATL label verbatim** (e.g. `cos_001000_HEADBODY__color_S_CUS_0x10000000__AT_ONE_`). `normalize.ts` parses flags out of the name: `_AT_ONE`, `_AT_ZERO`, `_AA_ADD`, `_A_AB`, `FACIAL`, `_color_S_CUS_0x…`, `rgb_don_*`, `don_facehip`, `_nom_`, `tras`, `cullnone`.
- Material settings: `alphaMode: OPAQUE`, `doubleSided: false`, `metallic 0`, `roughness 1`, `baseColorTexture` = the MATL's texture as **PNG**, one image per material (named after the material).
- One sampler: linear/linear, repeat/repeat.
- **`extras.shaderType`** comes from the `__nust__` MATL. `numd-exporter` does *not* write this; the previewer's GLBs had it added afterwards. Ours writes it directly.
- The file is a single-buffer binary `.glb`. `numd-exporter` writes a `.gltf` with 37 base64 buffers, about 30% larger.

**Skeleton for animations.** `nuanmb_to_glb` currently needs `--skeleton-glb` (the old `Don_3D_Nij.glb`). The extractor builds the rig from any model's `__maya__.nusktb` instead, so no pre-made GLB is needed.

**ID → file mapping** (`don_cos_reward.bin`, which the server already loads). The file is `<folder>_<dataId:000000>`, where `dataId` is `uniqueId × 1000`:

| `cosType` | folder | reward entries | files | gaps |
|---|---|---|---|---|
| `kigurumi` | `cos/` (GLB) | 157 | 179 | IDs 40, 103 and 104 have **no model**, so fall back to `cos_000000`. The extra files are unlisted variants such as `cos_036001`. |
| `head` | `head/` (GLB) | 140 | 140 | none |
| `body` | `body/` (GLB) | 163 | 163 | none |
| `face` | `paint/` (PNG) | 58 | 58 | none |
| `puchi` | `acce/` (PNG) | 148 | 149 | none |

- `model/face/*.nutexb` (22) are *expression atlases* keyed by kigurumi `dataId`, plus `r` variants. They aren't user-selectable, and the previewer picks the matching one.
- `don_color.bin` has 63 `{red, green, blue}` floats. That's exactly the 63-entry `CostumeColors` palette hardcoded in `Profile.razor.cs` today.
- `don3d_anim.bin` (56 entries) maps animation keys to `.nuanmb` names (54 files).
- The previewer also has `cos_160000`–`167000`, which aren't in 39.06 (they came from the newer "asb52" build). The extractor only outputs what the operator's game has.

**Server-side gaps found:**
- `UseStaticFiles()` has **no MIME type for `.glb`**, so every model would 404.
- `UseStaticFiles()` serves the *content root's* `wwwroot` (the working directory), but `PathHelper` and the extractor use the *exe's* `wwwroot`. They only match when the server starts from its own folder.
- `don_cos.bin` (103 entries: `cool/cute/joke/choice/collabo` tags per costume) isn't loaded. It isn't needed for the preview, so skip it.

**Data exposure (found 2026-10-31).** The WebUI's files and the server's data share one `wwwroot`, and `UseStaticFiles()` serves all of it. When the server runs from its own folder (the README setup), these are **publicly downloadable**, with no login:
- `data/*.json`: every operator table (QR codes, tokens, locked songs, events, …).
- `data/datatable/*.bin`: the game's encrypted datatables.
- `data/datatable/*.json`: the **decrypted** copies `FileGameDataCatalog` writes beside them (`musicinfo.json`, `wordlist.json`, …).
- `don-model/**` once `extract` runs (if `.glb` gets a MIME type).
- `taiko.db3` (+ `-wal`, `-shm`) is *not* served, only because `.db3` has no MIME mapping. That is one `ServeUnknownFileTypes` away from leaking every account.

The WebUI itself reads one table raw: `GameDataService.InitializeAsync` fetches `data/dan_data.json`. Everything else already goes through `api/GameData/*`.

**Fix (part of step 5):**
1. **Stop serving server data as static files.** Short-circuit `/data/**`, `/don-model/**` and `*.db3*` with a 404 before `UseStaticFiles()`, so only the WebUI's own files stay public (`index.html`, `_framework`, `css`, `appsettings.json`, `Dashboard.md`). It's a path guard and not a folder move, so existing operator layouts keep working.
2. **Dan data via the API:** add `GET api/GameData/DanData` (from `IGameDataCatalog`, which already loads it), and switch `GameDataService` to it, lazily, like the other catalogs (this also covers perf item P4).
3. **Models and textures via an authenticated endpoint:** `GET api/Assets/{**path}` under `[Authorize]` (the policy-aware one, so local mode still works). It serves from `PathHelper.GetRootPath()/don-model` with an explicit content type (`.glb → model/gltf-binary`, `.png`, `.json`), rejects `..` or rooted paths, and sends `ETag`/`Cache-Control: private`. This fixes the MIME gap and the content-root mismatch in one place.
4. **The embed loads through `fetch()` → `ArrayBuffer` → `GLTFLoader.parse()`**, with blob URLs for PNGs, sending the bearer token. No asset URL appears in the DOM.
5. Later, consider moving `taiko.db3` out of `wwwroot` (a `Database.json` path plus a one-time move on startup). It's not urgent once the guard exists.

**Output and size.** About 230 MB under `<exe>/wwwroot/don-model/{body,head,cos,paint,acce,face,animation}/`. It's git-ignored and never included in the publish output. The extractor is idempotent: it skips any output newer than its source.

**Build order** (one PR each):
1. `tools/extract` skeleton: CLI args, copying the datatables, the summary report.
2. Textures: `.nutexb` → PNG, for `paint`, `acce` and `face`, and for embedding in GLBs.
3. Models: MODL + MESH + SKEL + MATL (both) → GLB. Check `cos_001000` field-by-field against the `numd-exporter` output, then check every model loads in the previewer.
4. Animations: port `nuanmb_to_glb`, building the skeleton from `.nusktb`.
5. Server: block static access to `/data`, `/don-model` and `*.db3*`; add `api/GameData/DanData` and the authenticated `api/Assets/{**path}`; point the WebUI at them; README "Run extract"; CI `cargo build` + bundle the exe.

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
4. Model URLs point at `/don-model/...` produced by §2. If the assets are missing (extractor not run), the component shows a "run extract" notice instead of broken images.

Why an iframe: it fully isolates the React/three.js runtime from Blazor's DOM diffing, needs no build coupling between the two repos beyond copying static files, and keeps the previewer independently developable.

Deletes: `wwwroot/images/Costumes/**`, the `Masks` list, `CostumeColors`, `CostumeColorFilters`, and `CostumeOrDefault` in `Profile.razor.cs`.

#### Discouraging asset downloads

Anything the browser renders can be pulled out with dev tools or a GPU capture. No scheme makes the models undownloadable, so the aim is only to stop casual grabbing (right-click, guessable URLs, a browsable folder). In order of value for effort:

1. **Serve `don-model/` through an authenticated endpoint**, not as public static files. Then only logged-in users can fetch, and there's no folder to browse or link to. This is a small controller plus the existing auth; it's also the fix for the `.glb` MIME gap and the exe-dir-vs-content-root mismatch from §2.
2. **Load through `fetch()` → `ArrayBuffer` → `GLTFLoader.parse()` (and blob URLs for PNGs)**, so no asset URL ever appears in the DOM or as a right-clickable image. This is about 10 lines in the embed.
3. **Optional light obfuscation:** for example, XOR or AES the bytes with a per-session key from the API, and decode in the embed before `parse()`. This defeats "save the network response" but costs CPU and HTTP caching, and the key is in the page anyway. Only do it if 1–2 prove insufficient.

Not worth doing: disabling dev tools, right-click blockers, or watermarking. They're trivially bypassed and annoy legitimate users. The operator extracted these files from their own game install, so the realistic goal is "not freely hot-linkable from the server", which 1 achieves.

---

## Phasing / PRs

| PR | Contents | Depends on |
|---|---|---|
| 1 | Server auth: S1, S3, S4, S5 + admin CLI (`promote`, `demote`, `set-password`) | none |
| 1b | 2FA + magic-link login (§1a-2), each toggleable | 1 |
| 2 | Client auth: A1–A8 | 1 (S5) |
| 3 | Perf: P1–P5, P7, P11 (+ P6 bug fix) | none |
| 4 | `tools/extract`: move `nuanmb_to_glb` in, add the open-source model exporter + texture export, its README | open questions answered |
| 5 | Server: stop serving `/data`, `/don-model`, `*.db3*` statically; `api/GameData/DanData` + authenticated `api/Assets`; load `don_color`; bundle `extract.exe` in CI; README "Run extract" | 4 |
| 6 | Previewer `embed` entry (in `don-chan-previewer`) | none (can start now with existing GLBs) |
| 7 | LumexUI spike: Tailwind CLI build, shell, Login, Users, HighScores | 2 |
| 8+ | Remaining pages, Profile + `<DonPreview>` last; delete `images/Costumes`, remove MudBlazor | 5, 6, 7 |

PRs 1, 3, 4 and 6 are independent and can run in parallel.

## Decisions needed

1. ~~UI kit~~: decided, **LumexUI** (§3a fit check + spike plan).
2. ~~S2~~: decided. Defer registration, reset and troubleshooting to the accounts phase (§1a-3).
3. ~~Extractor repo home~~: decided, `tools/extract/` in this repo.
4. ~~Model GLB provenance~~: decided. They came from the closed-source `numd-exporter`, and we'll write an open-source replacement (clean-room, black-box diffing only).
