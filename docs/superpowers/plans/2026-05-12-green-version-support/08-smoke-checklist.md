# 08 — Smoke checklist

**Surface:** End-to-end verification that the iter-1 stub-first delivery is complete and correct.

**Important:** **The user runs the server.** Claude builds and documents. Each smoke step below is something the user does, with the expected outcome described. Claude assists with parsing output if asked.

**Verification gates:**
1. Full solution build with zero warnings.
2. EF migration applies cleanly to an existing populated `taiko.db3`.
3. Server starts with Nijiiro enabled + Green disabled (default config) — fully backwards-compatible behavior.
4. Server starts with both Nijiiro + Green enabled.
5. Server hard-fails with no era enabled.
6. Green stub controllers respond to a manual protobuf POST.
7. Nijiiro game flow remains functional (one round-trip from a real cabinet or `curl` harness, depending on what the user has available).

---

## Task 08.1: Full-solution build with zero warnings

**Goal:** Confirm the iter-1 changes don't introduce any new warnings.

**Steps:**

- [ ] **Step 1: Clean + build**

Run:
```bash
dotnet clean
dotnet build /warnaserror
```

Expected: build PASSES with zero warnings. If new warnings appear, address them inline (no warning suppression). Common Mapperly-generator warnings about unmapped fields are acceptable IF they're documented as `// TODO iter 2` — but if `/warnaserror` flags them as errors, suppress per-method with `[SuppressMessage(...)]` or by adding `[MapperIgnoreSource(nameof(Field))]` on the unmapped fields.

- [ ] **Step 2: No commit** — verification only.

---

## Task 08.2: Migration dry-run + apply to a copy of the dev DB

**Goal:** Confirm `AddGreenEraSupport` migration applies cleanly to your real local `taiko.db3` without data loss.

**This is a user-run step.** Claude documents the procedure.

**Steps:**

- [ ] **Step 1 (user): Back up the live DB**

```bash
cp Host/wwwroot/taiko.db3 Host/wwwroot/taiko.db3.pre-green-backup
```

- [ ] **Step 2 (user): Run the migration in a copy first**

```bash
cp Host/wwwroot/taiko.db3 /tmp/taiko.db3.test
# Point a one-off DbContext at the test copy by editing Database.json or running:
DbFileName=/tmp/taiko.db3.test dotnet ef database update --project Infrastructure --startup-project Host
```

Expected: `Done.` with no errors. Use `sqlite3 /tmp/taiko.db3.test ".tables"` and confirm:
- `SongBestDatum_Nijiiro`, `SongPlayDatum_Nijiiro`, `DanScoreDatum_Nijiiro`, etc. exist
- `SongBestDatum_Green`, `SongPlayDatum_Green`, `UserSaveData_Green`, etc. exist (empty)
- `UserSaveData_Nijiiro` exists and has rows matching the player count
- `UserDatum` no longer has the save-state columns

```bash
sqlite3 /tmp/taiko.db3.test "PRAGMA table_info(UserDatum);"
# Expected: 4 rows — Baid, IsAdmin, MyDonName, MyDonNameLanguage

sqlite3 /tmp/taiko.db3.test "SELECT COUNT(*) FROM UserSaveData_Nijiiro;"
# Expected: same count as SELECT COUNT(*) FROM UserDatum
```

- [ ] **Step 3 (user): Spot-check data preservation**

```bash
sqlite3 /tmp/taiko.db3.test "SELECT Baid, Title, CurrentKigurumi FROM UserSaveData_Nijiiro LIMIT 5;"
```
Expected: rows match what was in `UserDatum` before the migration.

If anything looks off, restore from the backup and inspect the migration's `INSERT` statement for column-name mismatches.

- [ ] **Step 4 (user): Apply to the real DB only after the test copy passes**

```bash
dotnet ef database update --project Infrastructure --startup-project Host
```

Expected: `Done.` and `Host/wwwroot/taiko.db3` now has the new schema.

> If at any point the migration fails on the real DB, restore from `taiko.db3.pre-green-backup` and report the error to Claude for diagnosis.

---

## Task 08.3: Server startup smoke — default config (Nijiiro on, Green off)

**Goal:** Backwards-compat verification — with `ServerSettings.Eras.Green.Enabled: false`, the server behaves exactly as before for Nijiiro.

**Steps:**

- [ ] **Step 1 (user): Confirm config**

`Host/Configurations/ServerSettings.json` should contain:
```jsonc
"Eras": {
  "Nijiiro": { "Enabled": true },
  "Green":   { "Enabled": false }
}
```

- [ ] **Step 2 (user): Run the server**

The user has admin privileges for binding ports 80/443. Claude does not run this — Claude builds the binary.

```bash
dotnet build
# User runs:
sudo dotnet run --project Host
# or starts the existing TaikoLocalServer.exe with admin rights
```

Expected startup log lines:
- `TaikoLocalServer version ...`
- `Enabled game eras: Nijiiro`
- No `Fatal` or `Error` log lines
- Server reaches "Application started. Press Ctrl+C to shut down."

- [ ] **Step 3 (user): Hit a Nijiiro endpoint manually if a cabinet isn't handy**

If the user has a known-good test request (saved curl harness, postman collection), exercise one Nijiiro endpoint and confirm it returns the same response as before this branch.

Failing that, the smoke check is satisfied by clean startup + admin-API responsiveness (`curl http://localhost/api/cards` or whatever the user typically hits).

- [ ] **Step 4 (user): Stop the server**

`Ctrl+C`. The shutdown log should show `Shut down complete`.

---

## Task 08.4: Server startup smoke — both eras enabled

**Goal:** Confirm both adapters register and Green's stub endpoints respond.

**Steps:**

- [ ] **Step 1 (user): Flip Green on**

Edit `Host/Configurations/ServerSettings.json`:
```jsonc
"Eras": {
  "Nijiiro": { "Enabled": true },
  "Green":   { "Enabled": true }
}
```

- [ ] **Step 2 (user): Run the server**

```bash
sudo dotnet run --project Host
```

Expected startup log lines:
- `Enabled game eras: Nijiiro, Green`
- A warning from `GreenEraGameDataCatalog`: `Green data path .../wwwroot/data/green does not exist...` IF the green data folder is missing, OR no warning if `wwwroot/data/green/` exists (from Task 07.1's `.gitkeep`).
- No Fatal / Error log lines.
- Server reaches "Application started."

- [ ] **Step 3 (user): Send a stub-controller smoke POST**

If the user has `curl` and a way to construct a protobuf body, send a heartbeat to Green:

```bash
# Construct a binary HeartBeatRequest body — protobuf-encode the message
# (use protoc, grpcurl with --proto, or a known-good cabinet capture).
# Then POST to /v11r01/chassis/heartbeat.php with Content-Type: application/protobuf.
curl -X POST -H "Content-Type: application/protobuf" \
     --data-binary @green-heartbeat.bin \
     http://localhost/v11r01/chassis/heartbeat.php
```

Expected: HTTP 200, response body is a protobuf-encoded `HeartBeatResponse { Result = 1, ComSvrStat = 1, GameSvrStat = 1, BnidSvrStat = 1, BanacoinStat = 1 }`.

If protobuf encoding by hand is painful, skip this step — clean startup with both eras enabled is the primary smoke target. The real cabinet round-trip is iter 2's verification.

- [ ] **Step 4 (user): Confirm Nijiiro still routes**

A Nijiiro endpoint (e.g. `/v12r08_ww/chassis/heartbeat.php`) should still respond as before — both adapters share the same Kestrel server.

- [ ] **Step 5 (user): Stop the server**

`Ctrl+C`.

---

## Task 08.5: Server startup smoke — no era enabled

**Goal:** Confirm the hard-fail path works.

**Steps:**

- [ ] **Step 1 (user): Disable both eras**

```jsonc
"Eras": {
  "Nijiiro": { "Enabled": false },
  "Green":   { "Enabled": false }
}
```

- [ ] **Step 2 (user): Try to run the server**

```bash
sudo dotnet run --project Host
```

Expected: Fatal log line `ServerSettings.Eras has no enabled era. ... Refusing to start.` and an `InvalidOperationException` propagates. Process exits non-zero.

- [ ] **Step 3 (user): Restore default config**

Re-enable Nijiiro (and Green if you want to keep both on):
```jsonc
"Eras": {
  "Nijiiro": { "Enabled": true },
  "Green":   { "Enabled": false }
}
```

---

## Task 08.6: Final state check + branch readiness

**Goal:** Confirm the branch is in a state the user is ready to merge into `dev`.

**Steps:**

- [ ] **Step 1: Branch + commit listing**

```bash
git log --oneline feat/green-version-support ^dev | head -40
```
Expected: ~25–35 commits, one per task, prefixed with `feat/refactor/docs/chore` etc.

- [ ] **Step 2: Diff summary**

```bash
git diff --stat dev feat/green-version-support | tail -20
```
Expected: many files touched but each change is contained — no spurious whitespace-only files, no leftover scratch files.

- [ ] **Step 3: Run the full build one final time**

```bash
dotnet clean
dotnet build /warnaserror
```
Expected: PASS, zero warnings.

- [ ] **Step 4: Verify .gitignore working-tree change is unrelated**

```bash
git status
```
The `.gitignore` modification from before this work is still in the working tree (we deliberately didn't include it in any commit). Confirm with the user whether to commit it now or leave for separate handling.

- [ ] **Step 5: User decides to merge / push**

Per the user's preference (push branch, no PR), the merge into `dev` happens locally on the user's machine. Claude does not push. Once the user is happy with the smoke results, they:

```bash
# user runs locally
git checkout dev
git merge --ff-only feat/green-version-support   # or no-ff for a merge commit
git push origin dev   # only after merge is verified
```

- [ ] **Step 6: Brief the user on iter-2 follow-ups**

Iter 2's work, ready to be queued for a follow-up brainstorming / planning cycle:

1. Real Green handler bodies (each `HandleGreen` partial in `Application/Handlers/`).
2. Real Mapperly bodies for the Green adapter's mappers.
3. Real Green catalog loaders — most importantly `musicinfo.bin` (binary format diverges from Nijiiro), plus `taikojuku_data.json`, `itemshop_data.json`, Green telop / gacha / tournament tables.
4. Operator documentation for what files to put in `wwwroot/data/green/`.
5. Admin API + TaikoWebUI Green surfaces (iter 3).

> If smoke testing surfaces a defect in iter 1's stub posture (e.g. a Green controller crashes instead of returning a default), the fix belongs in this same iter-1 plan as a follow-up task. Open a new branch from `feat/green-version-support` for the fix.
