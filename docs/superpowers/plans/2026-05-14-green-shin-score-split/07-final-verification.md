# Task 7: Final Verification

**Files:**
- Modify only if needed: `docs/green-protocol-field-audit.md`
- Modify only if needed: `docs/green-client-evidence/03-score-crown-counter-fields.md`

## Steps

- [ ] **Step 1: Run all Green tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Green
```

Expected: exits `0`.

- [ ] **Step 2: Run full build**

Run:

```powershell
dotnet build
```

Expected: exits `0`.

- [ ] **Step 3: Check working tree for intended source changes**

Run:

```powershell
git status --short
```

Expected source changes are limited to the files touched by Tasks 1-5 and generated migration files. Existing unrelated audit files may still appear; leave them untouched.

- [ ] **Step 4: Optional docs update if audit docs are still claiming Shin rows are always zero**

If `docs/green-client-evidence/03-score-crown-counter-fields.md` still says Shin selfbest rows are always zero, replace that line with:

```markdown
| `selfbest.ary_shin_selfbest_score` | IDA-proven field string `0xF026F2`; runtime capture maps Green `stage_mode=1` to Shin score mode. | Source-inferred: same requested-song row shape as normal selfbest. | `0` valid as no saved Shin score. | Same row count as requested songs. | Green selfbest now reads saved `SongBestDatum_Green` rows where `IsShin=true`; missing rows remain zero. | Keep `stage_mode` evidence documented and do not infer Shin from score range. |
```

- [ ] **Step 5: Commit final docs update if made**

If Step 4 changed docs, run:

```powershell
git add -- docs/green-client-evidence/03-score-crown-counter-fields.md docs/green-protocol-field-audit.md
git commit -m "Document Green shin score evidence"
```

If Step 4 made no docs changes, skip this commit.

- [ ] **Step 6: Final status summary**

Run:

```powershell
git log -5 --oneline
git status --short
```

Expected: recent commits show the mapper, schema, persistence, selfbest, and reward changes. Runtime database backup may appear if not ignored; do not add it.
