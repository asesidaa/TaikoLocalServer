# Phase 07: Tokkun Evidence Contract And Guardrail Reset - Research

**Researched:** 2026-06-03
**Domain:** Blue AC15 Tokkun protocol evidence contract, classifier policy, and guardrail reset
**Confidence:** HIGH for codebase and planning constraints; MEDIUM for binary-derived Tokkun runtime constants because no new IDA probe was required in this research pass.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
## Implementation Decisions

### Evidence Ledger Shape
- **D-01:** Phase 7 should use a battle-style row-by-row evidence matrix, following the proven Phase 4/5 battle workflow pattern rather than a loose narrative-only document.
- **D-02:** The Tokkun matrix should classify rows with four statuses: proven, observed, deliberately ignored, and unknown/blocked.
- **D-03:** A Tokkun row can be marked proven from local proto/wire/current-code evidence plus Blue IDA/client evidence. Cabinet/RPCS3 proof is reserved for final verification unless a specific row cannot be resolved without it.
- **D-04:** The matrix should enumerate protocol fields, endpoint/route questions, and guard reset decisions together so follow-on planners get one contract.

### Tokkun Classifier Boundary
- **D-05:** `ary_tokkunstage_info` is the likely Tokkun upload signal and should be the primary classifier input until stronger client evidence proves a different rule.
- **D-06:** `tokkun_tutorial_flg` is tutorial-state/readback evidence, not a reliable standalone playresult classifier. It should not by itself classify a payload as Tokkun.
- **D-07:** Do not add `PlayMode.Tokkun` or assign a Tokkun numeric play mode until RPCS3/cabinet logs or deeper IDA evidence proves the value.
- **D-08:** If a payload is classified as Tokkun mode, be lenient and ignore other-mode material rather than failing or writing normal/battle state. One credit has one mode, and Tokkun should not save scores.
- **D-09:** Downstream classifier output may expose an `IsTokkun` branch plus protocol-backed Tokkun facts, including Tokkun-stage summary fields, tutorial-state presence, and ignored-other-mode indicators. It must not expose a guessed numeric Tokkun play-mode enum.

### Guard Reset
- **D-10:** Remove the old Tokkun-term source guard. It was not a reasonable guardrail and should not be replaced with a named word allowlist.
- **D-11:** Do not add new Tokkun-specific source-scanning guardrail tests. The correct Tokkun implementation should skip normal, battle, crown, score, and other unrelated routines through runtime control flow.
- **D-12:** Proof that Tokkun does not save normal or battle state should come from real behavior: implementation structure plus focused checks that execute actual Tokkun handling, not source-text scans.
- **D-13:** Interpret TKEV-03 as a reset: delete stale source guards and prevent side effects through runtime control flow. Do not plan replacement word-scan guardrails.

### Follow-on Handoff Surface
- **D-14:** Phase 8 should receive Banacoin-adjacent route unknowns and evidence gates, especially around `getbanacoininfo.php`. Phase 7 should not invent Banacoin response semantics.
- **D-15:** Phase 9 should receive the classifier contract: exact classifier inputs, the lenient ignored-other-mode rule, logging boundaries, and the no normal/battle write rule.
- **D-16:** Phase 10 may consider only protocol-backed Tokkun facts for persistence: tutorial state, Tokkun-stage summary fields, song count/list, speed-change count, autoplay count, jump count, timestamps, and upload time.
- **D-17:** Phase 11 must prove v1.1 with cabinet/RPCS3 Tokkun selection/upload/readback evidence plus real behavior checks that Tokkun does not write unrelated state.

### the agent's Discretion
None. The user made explicit decisions for all selected gray areas.

### Deferred Ideas (OUT OF SCOPE)
## Deferred Ideas

None - discussion stayed within phase scope.
</user_constraints>

## Summary

Phase 7 should produce a documentation-and-test-boundary contract before runtime Tokkun behavior changes. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] The highest-value output is a battle-style Tokkun evidence matrix that places protocol fields, route-surface questions, classifier inputs, logging boundaries, persistence boundaries, and guard-reset decisions in one inspectable artifact. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] The battle precedent is a row-level evidence pack plus a gate/resolution matrix, not a narrative-only writeup. [CITED: .planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md] [CITED: .planning/milestones/v1.0-phases/05-blue-battle-runtime-support/05-RESOLUTION.md]

The current repo already exposes Tokkun-related Blue protocol fields in `proto/blue/taiko.proto` and generated Blue wire types. [VERIFIED: codebase grep] Current Blue playresult mapping does not map Tokkun facts into `CommonPlayResultData`, and current Blue playresult handling routes every non-battle payload through normal Blue save logic. [VERIFIED: codebase grep] The planner must therefore keep Phase 7 limited to contract and stale guard removal while handing runtime mapper/handler work to Phase 9. [CITED: .planning/ROADMAP.md]

**Primary recommendation:** create `07-01-TOKKUN-EVIDENCE-CONTRACT.md`, remove the stale Tokkun-term source guard from `Tests/Blue/BlueA4SourceGuardTests.cs`, and document that future protection must be behavior-based through Tokkun branch tests rather than replacement word scans. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] [VERIFIED: codebase grep]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Tokkun evidence contract | Planning/docs | Tests | The phase goal is an inspectable evidence-tagged protocol contract before runtime behavior changes. [CITED: .planning/ROADMAP.md] |
| Tokkun classifier policy | Application | Adapter mapper | Future classifier facts belong after Blue wire-to-common mapping and before handler writes; Phase 7 only documents the accepted inputs. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] [VERIFIED: codebase grep] |
| Stale source guard reset | Tests | Planning/docs | The stale guard lives in `Tests/Blue/BlueA4SourceGuardTests.cs`, and the context forbids replacing it with a Tokkun word allowlist. [VERIFIED: codebase grep] [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] |
| Banacoin route unknown handoff | Adapter | Planning/docs | Existing Blue Banacoin-adjacent controllers are stateless, while `getbanacoininfo.php` is explicitly absent from the route skeleton. [VERIFIED: codebase grep] |
| Tokkun persistence boundary | Database / Storage | Application | Persistence is deferred to Phase 10 and only for protocol-backed Tokkun facts. [CITED: .planning/ROADMAP.md] |
| Cabinet/RPCS3 proof boundary | External client evidence | Planning/docs | Final proof is deferred to Phase 11 and must cover selection, upload, readback, and no unrelated state writes. [CITED: .planning/ROADMAP.md] |

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| TKEV-01 | Operator/developer can review an evidence-tagged Blue Tokkun protocol contract covering proven, observed, and deliberately ignored fields. [CITED: .planning/REQUIREMENTS.md] | Use a row-by-row evidence matrix with statuses `proven`, `observed`, `deliberately ignored`, and `unknown/blocked`. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] |
| TKEV-02 | Blue Tokkun classification does not depend on a guessed numeric `play_mode`; the numeric value remains unknown until proven by RPCS3/cabinet logs or deeper IDA evidence. [CITED: .planning/REQUIREMENTS.md] | Document `ary_tokkunstage_info` as the primary likely upload signal, `tokkun_tutorial_flg` as tutorial/readback evidence, and `PlayMode.Tokkun` as forbidden until numeric proof exists. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] |
| TKEV-03 | The old source guard that banned Tokkun terms is replaced by bounded guards that allow only named Blue Tokkun support paths and still block invented reward, score, Banacoin, or battle semantics. [CITED: .planning/REQUIREMENTS.md] | Interpret this as deletion of the stale word-scan guard plus documented behavior-based guard policy for later phases, because the locked context forbids new Tokkun-specific source-scanning guard tests. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] |
</phase_requirements>

## Project Constraints (from AGENTS.md)

- Blue is a first-class era, not a Green variant. [CITED: AGENTS.md]
- Blue game routes live under `/v10r03/chassis/*`; shared AC15 startup/version routes stay under `/v01r00/chassis/*`. [CITED: AGENTS.md]
- Blue game request bodies are direct protobuf unless current client evidence proves otherwise. [CITED: AGENTS.md]
- Blue battle runtime is store-and-echo where semantics are not proven; do not invent token rewards, boss completion, stage graph behavior, stage 33 behavior, or normal unlock mirrors without evidence. [CITED: AGENTS.md]
- Battle playresults must not write normal Blue score, crown, Dani, profile, favorite, or normal unlock state. [CITED: AGENTS.md]
- Keep Blue, Green, and Nijiiro persistence separate unless the state is shared identity data. [CITED: AGENTS.md]
- Use existing partial-file era behavior: shared dispatcher in the unsuffixed file and era behavior in `.Nijiiro.cs`, `.Green.cs`, or `.Blue.cs`. [CITED: AGENTS.md]
- Map generated protobuf DTOs through `Application/Dtos/Common*` shapes before handler logic; do not persist wire DTOs directly. [CITED: AGENTS.md]
- Controllers should deserialize, map, call Mediator, and map back; business behavior belongs in `Application/Handlers`. [CITED: AGENTS.md]
- Use `IGameDataCatalog.For(GameEra)` and era catalog interfaces instead of hardcoded filesystem access from handlers. [CITED: AGENTS.md]
- Resolve runtime data roots through `PathHelper` and era data path helpers. [CITED: AGENTS.md]
- For AdminApi era routes, preserve legacy routes where they exist and `/api/{era}/...` routes validated by `EraRoute.TryParse`. [CITED: AGENTS.md]
- Keep generated `Wire/` files out of manual cleanup unless regenerating protocol output. [CITED: AGENTS.md]
- Blue item shop data is committed JSON, while `rewardshopdata.bin` remains local provenance and is not a runtime dependency. [CITED: AGENTS.md]
- Treat title id `0` as the explicit empty/default title state. [CITED: AGENTS.md]
- Use the temp-output Host build when a running server locks `Host/bin/Debug/net10.0`. [CITED: AGENTS.md]

## Standard Stack

### Core

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK | `10.0.100` target with installed `10.0.201` SDK | Build and test the repo. [VERIFIED: global.json] [VERIFIED: dotnet --version] | The solution targets `net10.0`. [VERIFIED: Directory.Build.props via .planning/codebase/STACK.md] |
| ASP.NET Core | `10.0.7` packages | Blue protocol controllers and hosted WebUI. [VERIFIED: Directory.Packages.props] | Existing Blue game endpoints are ASP.NET Core MVC controllers. [VERIFIED: codebase grep] |
| protobuf-net | `3.2.56` / `3.2.52` | Direct Blue protobuf serialization. [VERIFIED: Directory.Packages.props] | Existing Blue wire models and controllers use protobuf request/response DTOs. [VERIFIED: codebase grep] |
| Mediator.SourceGenerator | `3.0.2` | Controller-to-application dispatch. [VERIFIED: Directory.Packages.props] | Current `PlayResultController` dispatches `UpdatePlayResultCommand`. [VERIFIED: codebase grep] |
| Riok.Mapperly | `4.3.1` | Wire-to-common DTO mapper pattern. [VERIFIED: Directory.Packages.props] | Existing Blue mappers use `[Mapper]` static partial classes. [VERIFIED: codebase grep] |
| EF Core SQLite | `10.0.7` | Blue-owned persistence when later phases prove Tokkun state. [VERIFIED: Directory.Packages.props] | Existing Blue normal, shop, Dani, and battle state are persisted through EF Core SQLite. [VERIFIED: codebase grep] |
| xUnit | `2.9.3` | Focused mapper, route, handler, and guard tests. [VERIFIED: Directory.Packages.props] | Tests live in `Tests/Tests.csproj` and use xUnit. [VERIFIED: Tests/Tests.csproj] |

### Supporting

| Tool | Version / Status | Purpose | When to Use |
|------|------------------|---------|-------------|
| GSD CLI | available through `.codex/get-shit-done/bin/gsd-tools.cjs` | Phase init, graph status, and optional commit. [VERIFIED: init.phase-op output] | Use for phase workflow metadata and doc commit. [CITED: .codex/skills/gsd-plan-phase/SKILL.md] |
| Python | `3.13.2` | Blue IDA daemon driver and local scripts. [VERIFIED: python --version] | Use `.tools/blue/idadrv.py` if a specific Tokkun row needs new binary proof. [CITED: .tools/blue/idadrv.py] |
| Blue IDA daemon driver | present; daemon not running | Shared Blue EBOOT probing without racing IDB opens. [VERIFIED: python .tools/blue/idadrv.py status] | Start only when Phase 7 needs a concrete unresolved binary answer. [CITED: C:/Users/10614/.codex/memories/skills/blue-shared-ida-daemon/SKILL.md] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Existing Blue direct-protobuf route stack | New protocol adapter or payment service | Do not use; the existing route, wire, mapper, and Mediator stack already owns Blue protocol traffic. [VERIFIED: codebase grep] |
| Row-by-row evidence matrix | Loose narrative design note | Do not use; Phase 7 locked the battle-style matrix pattern. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] |
| Runtime source word scans | Behavior tests on future Tokkun branch | Use behavior tests later; the locked context rejects new Tokkun word allowlists. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] |

**Installation:** no package installation is required for Phase 7. [VERIFIED: codebase grep]

## Package Legitimacy Audit

Not applicable because Phase 7 should not install external packages. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

## Architecture Patterns

### System Architecture Diagram

```text
--------------------------+
| Phase 7 inputs          |
| CONTEXT / REQ / ROADMAP |
| proto / wire / code     |
| archived battle gates   |
+------------+-------------+
             |
             v
+--------------------------+
| Tokkun evidence matrix   |
| status per row:          |
| proven / observed /      |
| deliberately ignored /   |
| unknown-blocked          |
+------------+-------------+
             |
             v
+--------------------------+       +--------------------------+
| Classifier contract      | ----> | Phase 9 handoff         |
| no guessed PlayMode      |       | mapper + safe accept    |
+------------+-------------+       +--------------------------+
             |
             v
+--------------------------+       +--------------------------+
| Guard reset              | ----> | Future behavior checks  |
| delete stale word guard  |       | no normal/battle writes |
+------------+-------------+       +--------------------------+
             |
             v
+--------------------------+
| Route/persistence        |
| unknown handoffs         |
| Phase 8 / 10 / 11        |
+--------------------------+
```

### Recommended Project Structure

```text
.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/
  07-RESEARCH.md
  07-01-TOKKUN-EVIDENCE-CONTRACT.md
  07-02-SUMMARY.md

Tests/Blue/
  BlueA4SourceGuardTests.cs      # remove stale Tokkun term guard only

No runtime Tokkun handler, DTO, persistence, route, migration, or wire edits in Phase 7.
```

The artifact naming mirrors archived battle evidence slices such as `04-01-BATTLE-EVIDENCE.md` and `04-03-BLUE-BATTLE-DESIGN-GATE.md`. [CITED: .planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md] [CITED: .planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md]

### Pattern 1: Battle-Style Tokkun Evidence Matrix

**What:** Use one matrix with row identity, status, sources, allowed Phase 7 action, follow-on owner, and blocked assumptions. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

**When to use:** Use for every Tokkun field, classifier input, route question, logging boundary, persistence boundary, and guard reset decision in Phase 7. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

**Example:**

```markdown
| Row | Subject | Status | Evidence | Phase 7 Contract | Follow-on Gate |
|-----|---------|--------|----------|------------------|----------------|
| TK-01 | `PlayResultRequest.ary_tokkunstage_info` | observed | `proto/blue/taiko.proto`, `Adapters.GameProtocol.Blue/Wire/Game.cs`, current mapper omission | Primary likely classifier signal; map only in Phase 9 | Cabinet/RPCS3 or deeper IDA can tighten values |
```

### Pattern 2: Contract Before Runtime

**What:** Phase 7 documents what later code may do, then removes the stale guard that blocks Tokkun terms. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

**When to use:** Use because the phase explicitly excludes Banacoin compatibility, playresult persistence, Tokkun readback persistence, and cabinet/RPCS3 verification. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

**Example:**

```text
Phase 7 allowed:
- Write Tokkun evidence contract.
- Delete stale Tokkun-term source guard.
- Document future behavior-test guard policy.

Phase 7 blocked:
- Add `PlayMode.Tokkun`.
- Add `CommonPlayResultData.BlueTokkun.cs`.
- Add `UpdatePlayResultCommand.BlueTokkun.cs`.
- Add Banacoin route semantics.
- Add Tokkun persistence or migrations.
```

### Pattern 3: Handoff Rows For Future Phases

**What:** Every `unknown/blocked` row should name the future phase that owns resolution. [CITED: .planning/ROADMAP.md]

**When to use:** Use for numeric `play_mode`, `stage_mode`, `getbanacoininfo.php`, tutorial readback, Tokkun summary persistence, and cabinet/RPCS3 final proof. [CITED: .planning/REQUIREMENTS.md]

### Anti-Patterns to Avoid

- **Replacing one word scan with another:** The user rejected Tokkun word allowlists, so the planner must not create `BlueTokkunSourceGuardTests` that merely scans strings. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]
- **Adding runtime Tokkun DTOs in Phase 7:** Runtime mapper/DTO/handler work belongs to Phase 9. [CITED: .planning/ROADMAP.md]
- **Adding `PlayMode.Tokkun`:** The numeric value is explicitly unknown until RPCS3/cabinet logs or deeper IDA prove it. [CITED: .planning/REQUIREMENTS.md]
- **Adding `getbanacoininfo.php` because generated types exist:** The route is currently excluded by route tests, and Phase 8 owns any evidence-backed route change. [VERIFIED: codebase grep] [CITED: .planning/ROADMAP.md]

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Evidence contract | Ad hoc narrative | Battle-style row matrix | Archived battle phases used row-specific gates and resolution matrices to prevent speculative runtime behavior. [CITED: .planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md] [CITED: .planning/milestones/v1.0-phases/05-blue-battle-runtime-support/05-RESOLUTION.md] |
| Tokkun classification | Numeric `PlayMode.Tokkun` guess | Field-presence policy in the contract | Numeric Tokkun play mode is explicitly unproven. [CITED: .planning/REQUIREMENTS.md] |
| Guardrails | Source-text Tokkun word allowlist | Runtime behavior checks in later phases | Locked decisions D-10 through D-13 reject source-scan replacements. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] |
| Banacoin compatibility | Wallet/payment model | Stateless route compatibility in Phase 8 | Real Banacoin state is out of scope for this repo. [CITED: .planning/REQUIREMENTS.md] |
| Binary access | Competing IDB sessions | `.tools/blue/idadrv.py` shared daemon | Project guidance requires a shared daemon for Blue binary work. [CITED: C:/Users/10614/.codex/memories/skills/blue-shared-ida-daemon/SKILL.md] |

**Key insight:** Phase 7 reduces risk by making unknowns explicit and by removing stale guard posture without opening runtime state changes. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

## Common Pitfalls

### Pitfall 1: Treating Phase 7 As Runtime Implementation

**What goes wrong:** The plan adds Tokkun mapper DTOs, handler branches, persistence, migrations, or routes during the evidence contract phase. [CITED: .planning/ROADMAP.md]

**Why it happens:** Current code shows obvious runtime touch points in `PlayResultMappers.cs` and `UpdatePlayResultCommand.Blue.cs`, but those belong to Phase 9. [VERIFIED: codebase grep] [CITED: .planning/ROADMAP.md]

**How to avoid:** Limit Phase 7 to `07-01-TOKKUN-EVIDENCE-CONTRACT.md` and stale guard deletion. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

**Warning signs:** New files named `CommonPlayResultData.BlueTokkun.cs`, `UpdatePlayResultCommand.BlueTokkun.cs`, `BlueTokkunPlaySummary.cs`, or `GetBanacoinInfoController.cs` appear in a Phase 7 plan. [CITED: .planning/research/ARCHITECTURE.md]

### Pitfall 2: Recreating The Rejected Source Guard

**What goes wrong:** The stale test that banned `Tokkun` terms is replaced with another Tokkun word allowlist. [VERIFIED: codebase grep] [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

**Why it happens:** TKEV-03 mentions guardrails, but the context clarifies that this phase is a reset and not a new source-scanning regime. [CITED: .planning/REQUIREMENTS.md] [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

**How to avoid:** Delete `BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics` and document future behavior-test obligations for Phase 9 and Phase 11. [VERIFIED: codebase grep] [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

**Warning signs:** A planned test uses `Assert.DoesNotContain("Tokkun"...` or scans named Tokkun paths instead of executing Tokkun handling. [VERIFIED: codebase grep]

### Pitfall 3: Promoting `tokkun_tutorial_flg` Into The Classifier

**What goes wrong:** A payload is classified as Tokkun only because `tokkun_tutorial_flg` is present. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

**Why it happens:** The field exists in both userdata and playresult protocol shapes. [VERIFIED: codebase grep]

**How to avoid:** Contract it as tutorial-state/readback evidence and not a standalone playresult classifier. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

**Warning signs:** The matrix marks `tokkun_tutorial_flg` as `proven classifier` instead of `observed tutorial/readback evidence`. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

### Pitfall 4: Route Expansion From Proto Alone

**What goes wrong:** The plan adds `/v10r03/chassis/getbanacoininfo.php` because `Getbanacoininfo*` wire types exist. [VERIFIED: codebase grep]

**Why it happens:** Proto/wire message presence proves schema shape, not a required route call. [CITED: .planning/research/PITFALLS.md]

**How to avoid:** Hand `getbanacoininfo.php` to Phase 8 as an unknown requiring cabinet/RPCS3 logs or IDA route proof. [CITED: .planning/ROADMAP.md]

**Warning signs:** `BlueRouteSkeletonTests` is changed in Phase 7. [VERIFIED: codebase grep]

## Code Examples

### Evidence Matrix Row Shape

```markdown
| Row | Subject | Status | Evidence Source | Contract | Blocked Assumptions | Follow-on Owner |
|-----|---------|--------|-----------------|----------|---------------------|-----------------|
| TK-PLAYRESULT-ARY-TOKKUNSTAGE | `PlayResultRequest.ary_tokkunstage_info` | observed | `proto/blue/taiko.proto`; `Adapters.GameProtocol.Blue/Wire/Game.cs` | Primary likely Tokkun upload signal | numeric `PlayMode.Tokkun`; score/reward effects | Phase 9 |
```

Source: locked Phase 7 matrix statuses plus battle gate precedent. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] [CITED: .planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md]

### Guard Reset Target

```csharp
// Remove this stale Phase A4 method in Phase 7:
// BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics
```

Source: the method currently scans Blue playresult code for `Tokkun`, `Tookun`, and `deferred fields`. [VERIFIED: codebase grep]

### Future Behavior Guard Contract

```text
Future Phase 9 tests should execute Tokkun handling and assert:
- result = 1
- no SongPlayDataBlue / SongBestDataBlue writes
- no DanScoreDataBlue / DanStageScoreDataBlue writes
- no BlueBattle* writes
- no unlock, score, reward, Banacoin, favorite, recent, profile, or shop state writes
```

Source: no-normal/battle-write policy from Phase 7 decisions and Phase 9 success criteria. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] [CITED: .planning/ROADMAP.md]

## State Of The Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Tokkun treated as a Blue non-goal with source guards banning Tokkun terms. [CITED: docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md] | v1.1 reopens Blue Tokkun from local proto/binary evidence and requires an evidence contract. [CITED: .planning/PROJECT.md] | 2026-06-03 v1.1 milestone start. [CITED: .planning/STATE.md] | Phase 7 must remove stale no-Tokkun guard posture. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] |
| Battle Phase 4 failed closed with missing evidence rows. [CITED: .planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-03-BLUE-BATTLE-DESIGN-GATE.md] | Battle Phase 5 used row-level proof or named approval before runtime reliance. [CITED: .planning/milestones/v1.0-phases/05-blue-battle-runtime-support/05-RESOLUTION.md] | v1.0 battle runtime work. [CITED: .planning/RETROSPECTIVE.md] | Tokkun should mirror the evidence matrix pattern before runtime behavior. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] |
| Source scans protected "Tokkun must not exist". [VERIFIED: codebase grep] | Future protection must execute actual Tokkun handling and assert no unrelated writes. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] | Phase 7 guard reset. [CITED: .planning/ROADMAP.md] | The planner should not add a new word-scan guard. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] |

**Deprecated/outdated:**
- The historical statement that Tokkun is out of Blue scope is stale for v1.1. [CITED: docs/superpowers/specs/2026-05-27-blue-support-roadmap-design.md] [CITED: .planning/PROJECT.md]
- `BlueA4SourceGuardTests.BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics` is stale because it blocks legitimate Tokkun contract and future support paths. [VERIFIED: codebase grep] [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| - | No `[ASSUMED]` claims are used; recommendations are sourced to current planning docs, codebase grep, or local command probes. | All | - |

## Open Questions (RESOLVED)

1. **Numeric Tokkun `play_mode`**
   - What we know: `Domain/Enums/PlayMode.cs` has `Normal = 0`, `DanMode = 1`, `GaidenMode = 4`, and `AiBattle = 6`. [VERIFIED: codebase grep]
   - What's unclear: the Tokkun numeric value is not proven. [CITED: .planning/REQUIREMENTS.md]
   - Phase 7 resolution: unknown/blocked handoff. Phase 7 must document that `PlayMode.Tokkun` is not added and no numeric value is assigned. Phase 9 must classify Tokkun from protocol-backed fields without depending on a numeric Tokkun play mode. Phase 11 owns final cabinet/RPCS3 or deeper-IDA proof before any numeric `PlayMode.Tokkun` value can be introduced. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] [CITED: .planning/ROADMAP.md]

2. **`getbanacoininfo.php` route surface**
   - What we know: generated `Getbanacoininfo*` wire types exist and `BlueRouteSkeletonTests` excludes `/v10r03/chassis/getbanacoininfo.php`. [VERIFIED: codebase grep]
   - What's unclear: whether Tokkun traffic calls that route and whether absence blocks play. [CITED: .planning/REQUIREMENTS.md]
   - Phase 7 resolution: unknown/blocked handoff to Phase 8. Phase 7 must document the route as unresolved and must not add route semantics. Phase 8 owns evidence-backed availability decisions for `getbanacoininfo.php`, and the route is added only if cabinet/RPCS3 logs or IDA route evidence proves Blue Tokkun calls it and current absence blocks play. [CITED: .planning/ROADMAP.md] [CITED: .planning/REQUIREMENTS.md]

3. **Tokkun tutorial readback**
   - What we know: `UserDataResponse.tokkun_tutorial_flg` and `PlayResultRequest.tokkun_tutorial_flg` exist in proto/wire, and current userdata mapper omits the userdata field. [VERIFIED: codebase grep]
   - What's unclear: whether the Blue client requires readback after first Tokkun use. [CITED: .planning/REQUIREMENTS.md]
   - Phase 7 resolution: unknown/blocked handoff to Phase 10. Phase 7 must document `tokkun_tutorial_flg` as tutorial/readback evidence, not a standalone classifier. Phase 10 owns any Blue-owned tutorial persistence/readback implementation if the Phase 7 contract and later runtime evidence continue to support it. Phase 11 owns final cabinet/RPCS3 confirmation. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] [CITED: .planning/ROADMAP.md]

4. **Tokkun summary persistence**
   - What we know: `TokkunstageData` carries `banacoin_datetime`, song count/list, speed-change count, autoplay count, and jump count fields. [VERIFIED: codebase grep]
   - What's unclear: whether those fields require server persistence or readback. [CITED: .planning/REQUIREMENTS.md]
   - Phase 7 resolution: unknown/blocked handoff to Phase 10. Phase 7 must document these as protocol-backed Tokkun summary candidates only. Phase 10 owns any persistence/readback design and must store raw/protocol-backed facts without reward, score, payment, practice-time, ranking, unlock, or progression semantics unless later concrete Blue evidence proves a bounded behavior. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md] [CITED: .planning/REQUIREMENTS.md]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build/test validation | yes | `10.0.201` installed, target `10.0.100` roll-forward | Use temp-output Host build if `Host/bin` is locked. [VERIFIED: dotnet --version] [VERIFIED: global.json] [CITED: AGENTS.md] |
| Node.js | GSD CLI | yes | `v24.12.0` | Use direct file reads if GSD CLI command is not needed. [VERIFIED: node --version] |
| Python | IDA daemon driver | yes | `3.13.2` | Use existing code/proto/research evidence if no new binary row is needed. [VERIFIED: python --version] |
| Git | Optional doc commit | yes | `2.52.0.windows.1` | Skip commit only if GSD commit fails and report it. [VERIFIED: git --version] |
| Blue IDA daemon | New binary proof | driver present; daemon stopped | `running=false` | Start with `python .tools/blue/idadrv.py start` only for a concrete unresolved row. [VERIFIED: python .tools/blue/idadrv.py status] |

**Missing dependencies with no fallback:** none found for Phase 7 planning research. [VERIFIED: command probes]

**Missing dependencies with fallback:** Blue IDA daemon is not running; fallback is existing proto/wire/code/research evidence unless a Phase 7 row requires new binary proof. [VERIFIED: python .tools/blue/idadrv.py status]

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit `2.9.3` with Microsoft.NET.Test.Sdk `17.14.1`. [VERIFIED: Directory.Packages.props] |
| Config file | `Tests/Tests.csproj`. [VERIFIED: Tests/Tests.csproj] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueA4SourceGuardTests` |
| Full suite command | `dotnet test Tests/Tests.csproj` |

### Phase Requirements To Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| TKEV-01 | Tokkun evidence contract exists and contains the four required status buckets. [CITED: .planning/REQUIREMENTS.md] | docs/source check | `Test-Path .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` | no - Wave 0 |
| TKEV-02 | Contract states classifier does not depend on guessed numeric `PlayMode.Tokkun`. [CITED: .planning/REQUIREMENTS.md] | docs/source check | `Select-String .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md -Pattern 'PlayMode.Tokkun'` | no - Wave 0 |
| TKEV-03 | Stale Tokkun-term source guard is removed without adding a replacement Tokkun word-scan guard. [CITED: .planning/REQUIREMENTS.md] | unit/source guard | `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueA4SourceGuardTests` | yes |

### Sampling Rate

- **Per task commit:** run the focused source-guard test when `Tests/Blue/BlueA4SourceGuardTests.cs` changes. [VERIFIED: Tests/Tests.csproj]
- **Per wave merge:** run `dotnet test Tests/Tests.csproj --filter FullyQualifiedName~Blue`. [CITED: .planning/codebase/TESTING.md]
- **Phase gate:** run full `dotnet test Tests/Tests.csproj` or report why it was not run. [CITED: .planning/codebase/TESTING.md]

### Wave 0 Gaps

- [ ] `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-01-TOKKUN-EVIDENCE-CONTRACT.md` - covers TKEV-01 and TKEV-02. [CITED: .planning/REQUIREMENTS.md]
- [ ] Remove or narrow `BlueA4SourceGuardTests.BluePlayResultCode_DoesNotPromoteTokkunFieldsIntoRuntimeSemantics` - covers TKEV-03 reset. [VERIFIED: codebase grep]
- [ ] Add no new Tokkun-specific source-scanning guard test in Phase 7. [CITED: .planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md]

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | no | Phase 7 does not change auth. [CITED: .planning/ROADMAP.md] |
| V3 Session Management | no | Phase 7 does not change sessions. [CITED: .planning/ROADMAP.md] |
| V4 Access Control | no | Phase 7 does not change AdminApi or user authorization. [CITED: .planning/ROADMAP.md] |
| V5 Input Validation | yes | Future Tokkun classifier inputs must come from generated protobuf DTOs mapped into `Common*` DTOs, not persisted wire DTOs. [CITED: AGENTS.md] |
| V6 Cryptography | no | Phase 7 must not add Banacoin/payment or cryptographic behavior. [CITED: .planning/REQUIREMENTS.md] |

### Known Threat Patterns For This Stack

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Tampering through crafted Tokkun payloads that look like normal play | Tampering | Future Phase 9 behavior tests must prove Tokkun-classified payloads do not write normal score, reward, battle, favorite, recent, profile, shop, or Banacoin state. [CITED: .planning/ROADMAP.md] |
| Spoofing payment state through Banacoin fields | Spoofing / Tampering | Do not persist balance, coupons, payments, deductions, `chid`, BNID result, or transaction state. [CITED: .planning/REQUIREMENTS.md] |
| Information leakage through overbroad request logs | Information Disclosure | Preserve full request logging for evidence, but keep Phase 7 docs to sanitized field contracts rather than raw cabinet secrets. [CITED: .planning/research/PITFALLS.md] |

## Sources

### Primary (HIGH confidence)

- `.planning/phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md` - locked Phase 7 decisions, boundary, and canonical references.
- `.planning/REQUIREMENTS.md` - TKEV-01 through TKEV-03 and milestone out-of-scope boundaries.
- `.planning/ROADMAP.md` - Phase 7 success criteria and Phase 8-11 handoff boundaries.
- `.planning/STATE.md` and `.planning/PROJECT.md` - v1.1 milestone state, Blue Tokkun scope, and evidence hierarchy.
- `AGENTS.md` - project-specific architecture and Blue support directives.
- `proto/blue/taiko.proto` and `Adapters.GameProtocol.Blue/Wire/Game.cs` - Tokkun and Banacoin schema/wire evidence.
- `Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs`, `Application/Dtos/CommonPlayResultData.cs`, `Application/Handlers/UpdatePlayResultCommand.Blue.cs`, and `Domain/Enums/PlayMode.cs` - current mapper/DTO/handler/enum behavior.
- `Tests/Blue/BlueA4SourceGuardTests.cs`, `Tests/Blue/BlueRouteSkeletonTests.cs`, `Tests/Blue/BlueMapperTests.cs`, and `Tests/Blue/BluePlayResultMapperTests.cs` - current stale guard, route, userdata, and mapper test posture.
- `.planning/milestones/v1.0-phases/04-blue-battle-evidence-and-design/04-01-BATTLE-EVIDENCE.md`, `04-03-BLUE-BATTLE-DESIGN-GATE.md`, and `.planning/milestones/v1.0-phases/05-blue-battle-runtime-support/05-RESOLUTION.md` - battle evidence matrix and row-resolution precedent.

### Secondary (MEDIUM confidence)

- `.planning/research/SUMMARY.md`, `FEATURES.md`, `ARCHITECTURE.md`, `PITFALLS.md`, and `STACK.md` - current milestone-level Tokkun research, used only where confirmed by live planning docs or code.
- `C:/Users/10614/.codex/memories/skills/blue-shared-ida-daemon/SKILL.md` - shared Blue IDA daemon workflow, used for binary-work process guidance.
- `.tools/blue/idadrv.py` - local daemon driver command shape and target.

### Tertiary (LOW confidence)

- None used as protocol authority in this Phase 7 research.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - versions and architecture were verified from repo files and local command probes.
- Architecture: HIGH - Phase 7 is constrained by locked context decisions and existing Blue/battle patterns.
- Pitfalls: HIGH for planning/code risks - stale guard and current non-battle fall-through are verified in current source.
- Tokkun runtime constants: MEDIUM - current planning docs say numeric play mode and some route behavior remain unproven.

**Research date:** 2026-06-03
**Valid until:** 2026-06-10, or sooner if cabinet/RPCS3 Tokkun logs or new IDA probes close route/classifier unknowns.
