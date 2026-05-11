# 08 - Build, Smoke, And Docs

**Surface:** Final verification pass, operator notes, and cabinet smoke checklist.

**Why last:** This validates all implementation slices together.

**Files:**
- Modify: `Host/README.md`
- Modify: `docs/superpowers/plans/2026-05-12-green-real-support/README.md` if task ordering changed during implementation
- Modify: `Host/Host.csproj` only for Green XML content copy rules

---

## Task 08.1: Full Build And Test Pass

**Acceptance Criteria:**
- [ ] `dotnet test` passes.
- [ ] `dotnet build` passes.
- [ ] Green required datatables are copied to Host output or otherwise available at runtime.

**Steps:**

- [ ] **Step 1: Run tests**

Run: `dotnet test`

Expected: PASS.

- [ ] **Step 2: Run build**

Run: `dotnet build`

Expected: PASS.

- [ ] **Step 3: Add Green datatable content items**

Add these entries to the existing Host content `ItemGroup`:

```xml
<Content Update="wwwroot\data\green\datatable\musicinfo.xml"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>
<Content Update="wwwroot\data\green\datatable\musicmedleyinfo.xml"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>
```

- [ ] **Step 4: Rebuild after csproj change**

Run: `dotnet build`

Expected: PASS.

---

## Task 08.2: Document Green Phase-2 Behavior

**Acceptance Criteria:**
- [ ] `Host/README.md` explains Green required files and test unlock behavior.
- [ ] Smoke checklist names expected 10/20-song behavior, fake scores/crowns, options, and fake Dan.

**Steps:**

- [ ] **Step 1: Add Green notes to `Host/README.md`**

Add a section:

```markdown
## Green AC15 Test Support

When `ServerSettings:Eras:Green:Enabled` is `true`, the server requires:

- `wwwroot/data/green/datatable/musicinfo.xml`
- `wwwroot/data/green/datatable/musicmedleyinfo.xml`

The current Green implementation intentionally unlocks a small deterministic song set for cabinet validation:

- no-card/default song flags: first 10 `uniqueid` values from `musicinfo.xml`
- logged-in user release flags: first 20 `uniqueid` values from `musicinfo.xml`

New Green users also receive deterministic fake best scores/crowns and receive the first fake Dan on their first known-card login after registration. This is test scaffolding for verifying Green bitset, self-best, crown, and Dan response formats.
```

- [ ] **Step 2: Add smoke checklist**

Add:

```markdown
### Green Cabinet Smoke Checklist

1. Start server with `ServerSettings:Eras:Green:Enabled = true`.
2. Boot cabinet and reach song select without card. Confirm 10 songs are visible.
3. Register a card through `mydonentry`.
4. Log in with that card. Confirm 20 songs are visible.
5. Confirm fake self-best/crown data appears for the seeded songs.
6. Log out and log in again. Confirm first fake Dan appears.
7. Change gameplay options, play a song, and log in again. Confirm option state did not reset.
8. Play a song and confirm `selfbest` and `crownsdata` reflect the upload.
9. Enter any ghost/AI-battle-like flow available on the cabinet. Confirm it does not crash and request logs show bounded byte previews.
```

- [ ] **Step 3: Commit docs**

```bash
git add Host/README.md Host/Host.csproj
git commit -m "docs(green): document Green test support behavior"
```

---

## Task 08.3: Local Endpoint Smoke

**Acceptance Criteria:**
- [ ] Server starts with Green enabled.
- [ ] Simple protobuf endpoint smoke can be done by cabinet or a local client.

**Steps:**

- [ ] **Step 1: Start server**

Run:

```bash
dotnet run --project Host
```

Expected: server starts; logs include Green catalog loaded with a nonzero song count.

- [ ] **Step 2: Stop server after startup verification**

Stop with Ctrl+C.

- [ ] **Step 3: Do not commit runtime artifacts**

Run: `git status --short`

Expected: no new runtime files staged. Ignore local DB/log/output changes unless the user explicitly asks to keep them.

---

## Task 08.4: Final Commit And Handoff

**Acceptance Criteria:**
- [ ] All planned implementation commits exist.
- [ ] Final status summarizes verification commands and any cabinet-only checks not run by the agent.

**Steps:**

- [ ] **Step 1: Run final verification**

Run:

```bash
dotnet test
dotnet build
```

Expected: both PASS.

- [ ] **Step 2: Check worktree**

Run: `git status --short`

Expected: only intentional docs/code changes are present. Commit any remaining intentional changes with a focused message.

- [ ] **Step 3: Final response**

Report:

- tests/build run and result
- files changed at a high level
- cabinet smoke checks that still require the user's hardware
- any fields logged for future reverse-engineering
