# 07 — On-disk file move and operator documentation

**Surface:** Move the existing `wwwroot/data/*` files into the new per-era layout (`wwwroot/data/nijiiro/`, `wwwroot/data/shared/`, plus an empty `wwwroot/data/green/datatable/` directory for future operator drops). Update `Host/README.md` and the top-level `README.md` documenting the new layout.

**Why this is parallel-safe with 03–06:** This task only touches files on disk (under `Host/wwwroot/data/`) and Markdown docs. It doesn't change C# code. It can be executed in parallel with 03–06 or saved until just before smoke testing.

**Why this must happen before Task 08:** Nijiiro loaders in Task 04 now look at `wwwroot/data/nijiiro/...` paths. If the files are still at the old `wwwroot/data/...` location, Nijiiro startup will hard-fail.

**Verification cadence:** Visual inspection of the new directory structure + Nijiiro startup smoke (deferred to Task 08).

---

## Task 07.1: Move committed Nijiiro data files into `wwwroot/data/nijiiro/`

**Goal:** The Host project commits a baseline set of JSON datatables under `Host/wwwroot/data/` (referenced by `Host/Host.csproj`'s `<None Include="wwwroot/data/...">` blocks). Move them into `Host/wwwroot/data/nijiiro/` and update the csproj include paths.

**Files moved:**
- `Host/wwwroot/data/dan_data.json` → `Host/wwwroot/data/nijiiro/dan_data.json`
- `Host/wwwroot/data/event_folder_data.json` → `Host/wwwroot/data/nijiiro/event_folder_data.json`
- `Host/wwwroot/data/gaiden_data.json` → `Host/wwwroot/data/nijiiro/gaiden_data.json`
- `Host/wwwroot/data/intro_data.json` → `Host/wwwroot/data/nijiiro/intro_data.json`
- `Host/wwwroot/data/locked_*_data.json` (multiple files) → `Host/wwwroot/data/nijiiro/locked_*_data.json`
- `Host/wwwroot/data/movie_data.json` → `Host/wwwroot/data/nijiiro/movie_data.json`
- `Host/wwwroot/data/shop_folder_data.json` → `Host/wwwroot/data/nijiiro/shop_folder_data.json`
- `Host/wwwroot/data/special_songs_data.json` → `Host/wwwroot/data/nijiiro/special_songs_data.json`

**Files moved to shared:**
- `Host/wwwroot/data/token_data.json` → `Host/wwwroot/data/shared/token_data.json`
- `Host/wwwroot/data/qrcode_data.json` → `Host/wwwroot/data/shared/qrcode_data.json`

**Note on `datatable/`:** The `wwwroot/data/datatable/` folder is **operator-supplied** (game-owned binary files like `musicinfo.bin`) — it is NOT committed to git per the top-level `README.md`. Only update the documentation; don't try to move uncommitted files.

**Files modified:**
- `Host/Host.csproj` — update `<None Include="wwwroot/data/...">` paths
- Possibly create `Host/wwwroot/data/green/.gitkeep` and `Host/wwwroot/data/green/datatable/.gitkeep` so the empty directories exist in the repo

**Acceptance Criteria:**
- [ ] No files remain directly under `Host/wwwroot/data/` (other than the new subdirectories `nijiiro/`, `shared/`, `green/`).
- [ ] All previous JSON files exist under their new paths.
- [ ] `Host/Host.csproj` `<None Include=...>` blocks point at the new paths.
- [ ] `Host/wwwroot/data/green/` and `Host/wwwroot/data/green/datatable/` exist (possibly with `.gitkeep` placeholders).
- [ ] `dotnet build` succeeds.
- [ ] `dotnet publish Host/Host.csproj` produces a `publish/wwwroot/data/nijiiro/dan_data.json` etc. — verify with `find` / `tree` on the publish output.

**Verify:**
```bash
ls Host/wwwroot/data
# Expected: nijiiro/  shared/  green/
ls Host/wwwroot/data/nijiiro/
# Expected: dan_data.json, event_folder_data.json, ..., movie_data.json, intro_data.json, ...
ls Host/wwwroot/data/shared/
# Expected: token_data.json, qrcode_data.json
dotnet build
dotnet publish Host/Host.csproj -c Release -o /tmp/taiko-publish 2>&1 | tail -20
ls /tmp/taiko-publish/wwwroot/data/nijiiro/
```
Expected: publish output mirrors source layout.

**Steps:**

- [ ] **Step 1: Move files with `git mv`** (preserves history)

```bash
mkdir -p Host/wwwroot/data/nijiiro Host/wwwroot/data/shared Host/wwwroot/data/green/datatable
git mv Host/wwwroot/data/dan_data.json          Host/wwwroot/data/nijiiro/dan_data.json
git mv Host/wwwroot/data/event_folder_data.json Host/wwwroot/data/nijiiro/event_folder_data.json
git mv Host/wwwroot/data/gaiden_data.json       Host/wwwroot/data/nijiiro/gaiden_data.json 2>/dev/null || true
git mv Host/wwwroot/data/intro_data.json        Host/wwwroot/data/nijiiro/intro_data.json
git mv Host/wwwroot/data/movie_data.json        Host/wwwroot/data/nijiiro/movie_data.json
git mv Host/wwwroot/data/shop_folder_data.json  Host/wwwroot/data/nijiiro/shop_folder_data.json
git mv Host/wwwroot/data/special_songs_data.json Host/wwwroot/data/nijiiro/special_songs_data.json
# locked_*_data.json — there may be several
for f in Host/wwwroot/data/locked_*_data.json; do
    git mv "$f" "Host/wwwroot/data/nijiiro/$(basename "$f")" 2>/dev/null || true
done
# Shared
git mv Host/wwwroot/data/token_data.json   Host/wwwroot/data/shared/token_data.json
git mv Host/wwwroot/data/qrcode_data.json  Host/wwwroot/data/shared/qrcode_data.json
```

If any of these files don't currently exist (this repo might commit only a subset), the `git mv` for that file is harmless to skip (the `|| true` allows the script to continue).

- [ ] **Step 2: Add `.gitkeep` placeholders for empty Green directories**

```bash
touch Host/wwwroot/data/green/.gitkeep
touch Host/wwwroot/data/green/datatable/.gitkeep
git add Host/wwwroot/data/green
```

- [ ] **Step 3: Update `Host/Host.csproj` `<None Include="wwwroot/...">` paths**

Search for the `<None Include="wwwroot/data/...">` blocks. For each, update the path to include the era subfolder. Example:

```xml
<!-- before -->
<None Include="wwwroot/data/dan_data.json" CopyToOutputDirectory="PreserveNewest" />
<!-- after -->
<None Include="wwwroot/data/nijiiro/dan_data.json" CopyToOutputDirectory="PreserveNewest" />
```

(In MSBuild, the published path mirrors the source path because no `Link=` attribute is used. Keep that — publishes will end up at `<publish>/wwwroot/data/nijiiro/dan_data.json` automatically.)

- [ ] **Step 4: Build + publish smoke**

```bash
dotnet build
dotnet publish Host/Host.csproj -c Release -o /tmp/taiko-publish
ls /tmp/taiko-publish/wwwroot/data/
ls /tmp/taiko-publish/wwwroot/data/nijiiro/
ls /tmp/taiko-publish/wwwroot/data/shared/
```

Expected: each `ls` shows the moved files at their new publish paths.

- [ ] **Step 5: Commit**

```bash
git add Host/wwwroot/data Host/Host.csproj
git commit -m "chore(host): move per-era data files into wwwroot/data/<era>/ subfolders"
```

---

## Task 07.2: Update `Host/README.md` to document the new layout

**Goal:** Operator-facing docs reflect the new per-era directory structure.

**Files:**
- Modify: `Host/README.md` (the per-host README)
- Modify: `README.md` (top-level — the operator-installation README mentions `wwwroot/data/datatable/` in the current state; update it)

**Acceptance Criteria:**
- [ ] `Host/README.md` documents the `wwwroot/data/nijiiro/`, `wwwroot/data/green/`, `wwwroot/data/shared/` split with examples of which files go where.
- [ ] Top-level `README.md` operator-install steps reference the new per-era paths for `musicinfo.bin` etc.
- [ ] Both READMEs mention `ServerSettings.json:Eras` opt-in.

**Steps:**

- [ ] **Step 1: Update `Host/README.md`**

Add a section near the existing "where do data files live" content:

```markdown
## Data file layout (per-era)

`wwwroot/data/` is now partitioned per game era:

```
wwwroot/data/
├── nijiiro/                    Nijiiro-era (39.06 WW + CHN) operator-edited datatables
│   ├── dan_data.json
│   ├── event_folder_data.json
│   ├── movie_data.json
│   ├── intro_data.json
│   ├── locked_*_data.json
│   ├── shop_folder_data.json
│   ├── special_songs_data.json
│   ├── gaiden_data.json
│   └── datatable/              Operator-supplied game binaries (NOT in git)
│       ├── musicinfo.bin
│       ├── music_order.bin
│       ├── wordlist.bin
│       ├── don_cos_reward.bin
│       ├── shougou.bin
│       └── neiro.bin
├── green/                      Green-era (AC15) data — empty until iter 2
│   ├── taikojuku_data.json     (TBD — added in iter 2)
│   ├── itemshop_data.json      (TBD — added in iter 2)
│   ├── eventfolder_data.json   (TBD — added in iter 2)
│   └── datatable/              Operator-supplied Green binaries
│       └── musicinfo.bin       (Green binary format — parser arrives in iter 2)
└── shared/                     Cross-era operator-edited tables
    ├── token_data.json
    └── qrcode_data.json
```

Enabling a Green cabinet against this server requires:
1. Setting `ServerSettings.Eras.Green.Enabled: true` in `Configurations/ServerSettings.json`.
2. Dropping Green's `musicinfo.bin` (and any other Green binaries) into `wwwroot/data/green/datatable/`.
3. In iter 1 (current), Green endpoints return success-shaped empty responses regardless of file presence. Real Green behavior requires iter 2.
```

- [ ] **Step 2: Update top-level `README.md`**

Search the top-level `README.md` for any mention of `wwwroot/data/datatable/` or "drop your binaries". Update to reference the per-era subfolder path (`wwwroot/data/nijiiro/datatable/`) for the existing Nijiiro setup.

- [ ] **Step 3: Commit**

```bash
git add Host/README.md README.md
git commit -m "docs(host): document per-era wwwroot/data/<era>/ layout and ServerSettings.Eras opt-in"
```

---

## Task 07.3: Inform the user about their local `wwwroot/data/` state

**Goal:** The user runs the server with their own `taiko.db3` and operator-supplied datatables under `wwwroot/data/datatable/`. Those uncommitted binaries also need to be moved to `wwwroot/data/nijiiro/datatable/` before they can run Task 08's smoke test.

**This is documentation, not code change.**

**Steps:**

- [ ] **Step 1: Add a note to the plan README pointing the user to the manual file-move step**

Inline note added to the plan's [README.md](README.md):

> **Before Task 08 (smoke testing):** if you have a local working `wwwroot/data/datatable/` folder with operator-supplied binaries (`musicinfo.bin`, `music_order.bin`, etc.) that aren't in git, **move them** to `wwwroot/data/nijiiro/datatable/` before starting the server. The Nijiiro catalog loader now looks for them at the per-era path.

- [ ] **Step 2: No commit** — the README update is part of this same Task 07 series; if you've already committed 07.1 and 07.2, add this note in the README during 07.2's commit, or amend it.
