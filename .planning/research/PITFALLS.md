# Pitfalls Research: Murasaki AC15 Support

**Domain:** Brownfield ASP.NET Core Taiko AC15 era adapter, protobuf wire mapping, SQLite persistence, local game-data catalogs, and Blazor AdminApi/WebUI support
**Researched:** 2026-06-21
**Confidence:** MEDIUM overall. HIGH for current repo/proto/data facts verified in this checkout; MEDIUM for route/root/runtime semantics until IDA table evidence, request logs, captures, or cabinet/RPCS3 proof lock them.

## Critical Pitfalls

### Pitfall 1: Treating Murasaki as White With Renamed Folders

**What goes wrong:**
Runtime handlers, Mapperly mappers, AdminApi readback, and tests are copied from White and adjusted until they compile. This hides real protocol shape differences: Murasaki does not expose the White-style monolithic `initialdatacheck` message in `proto/murasaki/taiko.proto`, and instead splits metadata across `defaultsong`, `mainichisong`, `foldercheck`, `getfolder`, `telopcheck`, `gettelop`, `songhash`, and `bestscore`.

**Why it happens:**
Murasaki is feature-similar to White at the product level, White support just shipped, and many older AC15 capability names still look familiar. Familiar names make it tempting to reuse White response assembly before proving Murasaki wire placement and request flow.

**How to avoid:**
Start with a Murasaki evidence and feature-inventory phase. Mark each surface as `Murasaki proven`, `White-like but changed`, `proto-only`, `binary lead only`, `data-only`, or `absent`. Reuse White/shared AC15 capabilities only after a Murasaki-specific mapper and capability profile prove the same semantics, byte widths, limits, and response placement.

**Warning signs:**
- New Murasaki code references White wire DTOs, White profiles, White constants, or White tests.
- A plan says "copy White initialdata" even though no `InitialdatacheckRequest/Response` exists in Murasaki proto.
- Tests assert that Murasaki has the same fields as White instead of exercising Murasaki request/response behavior.

**Phase to address:**
Phase 1, "Murasaki Evidence and Era Foundation"; Phase 2, "Murasaki Catalog/Profile and Protocol Limits".

---

### Pitfall 2: Locking Route Prefixes or Data Roots From Hints and Filenames

**What goes wrong:**
The implementation hardcodes `/v06r00/chassis/*`, `/v01r00/chassis/*`, or one of `ST5100-1`, `ST5100-7`, or `ST6100-1` because those strings are plausible. If the active root or exact route table differs, the adapter can compile and still fail cabinet flow or load the wrong catalog.

**Why it happens:**
The user provided a useful route hint, the data directory exposes multiple plausible roots, and White/Red phase history makes route scaffolding feel routine. But local data presence is not runtime root selection proof, and raw binary string hits are leads, not route table evidence.

**How to avoid:**
Make route/root proof a blocking first-phase artifact. Use `.tools/murasaki/EBOOT.ELF.i64` through the existing IDA workflow or equivalent request logs/captures to prove the game prefix, startup/version ownership, approved `.php` suffix set, transport, and active config root. Cross-check approved route suffixes against `proto/murasaki/taiko.proto` request/response pairs. Record root selection separately from data availability.

**Warning signs:**
- A controller route is added before a `MURASAKI-EVIDENCE.md` route table exists.
- `ST5100-1`, `ST5100-7`, or `ST6100-1` is selected because it has the largest files, newest names, or matching public-era expectations.
- Host missing-content-type fallback is broadened to all `/v06r00` or all AC15 routes instead of a proven exact prefix.

**Phase to address:**
Phase 1, "Murasaki Evidence and Era Foundation".

---

### Pitfall 3: Treating Unknown Byte Fields as Ordinary Flags

**What goes wrong:**
Byte-heavy protocol fields are given friendly names and persisted as gameplay state before their length, indexing, defaults, and consumer behavior are known. High-risk fields include `hash_default_song_flg`, `hash_mainichidojo_all`, `hash_mainichidojo_rare`, `hash_release_song_flg`, `hash_crown_flg`, `song_hash_tbl`, `content_info`, `default_option_setting`, `option_flg`, `tone_flg`, costume/title flags, and `reserved`.

**Why it happens:**
Earlier eras already have byte helpers and release/crown/favorite logic, so a byte array can look self-explanatory. Murasaki adds several byte surfaces where the field name describes a broad category but not the binary layout or limits.

**How to avoid:**
Require a binary/client evidence pass before modeling each new byte field as a feature. Until proven, log bounded lengths and hex summaries, preserve exact request facts only where useful, and return conservative fixed-width defaults only when the cabinet requires them. Treat each byte helper as part of an era capability profile with explicit source evidence.

**Warning signs:**
- A byte array is initialized with a White/Red helper width without a Murasaki evidence citation.
- Tests assert only "not null" or "length copied from White".
- `reserved` or `content_info` is persisted with a guessed semantic column name.

**Phase to address:**
Phase 2, "Murasaki Catalog/Profile and Protocol Limits"; Phase 4, "Murasaki Byte-Field and Global Score Capabilities".

---

### Pitfall 4: Turning Global Best Score Leads Into Normal User Score State

**What goes wrong:**
`BestScoreRequest/Response` is implemented by reading or writing per-user self-best rows, or global score rows are exposed through AdminApi as if their ranking semantics are known. This can corrupt Murasaki personal score behavior and produce convincing but wrong readback.

**Why it happens:**
Murasaki proto has both `SelfBestResponse` and `BestScoreResponse`, and `BestScoreResponse` contains nested per-song rank scores with `high_score` and `mydon_name`. The names make it look like a simple leaderboard, but request sequencing, paging via `seq_id`/`last_seq_id`, rank count, privacy, and data source are not proven by proto alone.

**How to avoid:**
Model global high-score/readback as a separate capability only after binary/client or request-log evidence explains call order, sequence semantics, row limits, sort order, and whether data should be global, shop-local, seeded, empty, or compatibility-only. Keep `selfbest.php` and `bestscore` persistence/readback separate.

**Warning signs:**
- `BestScoreResponse` is filled from the user's own `SelfBest` table.
- `seq_id` is ignored without a compatibility note.
- AdminApi exposes global ranking data before cabinet behavior proves it is meaningful.

**Phase to address:**
Phase 4, "Murasaki Byte-Field and Global Score Capabilities".

---

### Pitfall 5: Editing Dumped Proto Inputs or Generated Wire to Make Mapping Easier

**What goes wrong:**
`proto/murasaki/*.proto` or generated `Wire/` classes are manually changed to resemble White, remove awkward optionality, rename fields, add missing routes, or make Mapperly compile. Later regeneration or binary evidence invalidates the hand edits, and the repo loses traceability to dumped protocol inputs.

**Why it happens:**
Generated C# can feel like local implementation code, and older agents have already been corrected for wrong-layer proto edits in this repository. Mapperly errors can also create pressure to "fix" the schema instead of fixing the mapping boundary.

**How to avoid:**
Do not edit `proto/` unless the user explicitly allows schema work. Generate adapter-local Murasaki wire DTOs from immutable `proto/murasaki/taiko.proto` and `proto/murasaki/vsinterface.proto`. If generated output is wrong, capture the evidence and regenerate through the approved toolchain; do not hand-clean generated `Wire/` files.

**Warning signs:**
- `git status --porcelain -- proto/murasaki` has output after a Murasaki implementation step.
- A generated `Wire/` file has manual cleanup unrelated to regeneration.
- A plan says "add missing field to proto" before proving the dumped schema is wrong.

**Phase to address:**
Phase 1, "Murasaki Evidence and Era Foundation"; every phase verification checklist.

---

### Pitfall 6: Replacing Mapperly Projections With Handwritten Aggregators

**What goes wrong:**
Murasaki adapter mappers start containing business decisions, default object creation, route-specific assembly, or handwritten one-to-one copying. The code works for one endpoint but bypasses the repo's source-generator boundary and repeats the AC15 mapper architecture problems that were already corrected.

**Why it happens:**
Murasaki split request families create awkward mapping targets. It can seem faster to write a method body than to define semantic Application sections, controller-owned final assembly, and Mapperly-generated mechanical projections.

**How to avoid:**
Keep controllers responsible for deserializing, mapping, calling Mediator, and assembling final wire responses. Keep handlers in `Application/Handlers`. Keep Mapperly projections source-generator driven; handwritten mapper code is limited to narrow helper conversions that Mapperly discovers or is explicitly configured to use. Verify nontrivial mappings by building with `dotnet build /p:EmitCompilerGeneratedFiles=true` and inspecting emitted `.g.cs` under `obj/.../generated/.../Riok.Mapperly/`.

**Warning signs:**
- Mapper classes contain large response-building methods.
- Business rules such as feature availability, reward grants, challenge progress, or score selection live in mapper code.
- Verification says "build passed" but does not inspect generated Mapperly output for nontrivial mappings.

**Phase to address:**
Phase 2 for profile/wire placement; Phase 3 for runtime binding; final closeout for generated-source inspection.

---

### Pitfall 7: Merging Murasaki State With White or Red State

**What goes wrong:**
Murasaki writes or reads White/Red save, score, crown, favorite, recent, Dani, Don Challenge, reward, or collectable tables because shapes match. AdminApi/WebUI then displays plausible values while cabinet state is actually cross-era contaminated.

**Why it happens:**
The shared AC15 helper architecture intentionally reuses algorithms, and Murasaki is older-era adjacent to White/Red. Without explicit typed DbSet boundaries, "same columns" can become "same storage".

**How to avoid:**
Add Murasaki-owned EF entities, DbSets, mappings, migrations, profile rows, play history, self-best, crowns, favorites/recent, Dani, reward/Don Point, byte-fact tables, and any global-score tables that are proven. Share algorithms only through narrow Domain row-shape interfaces and generic helpers over concrete Murasaki DbSets. Keep `ITaikoDbContext` visible as the persistence boundary.

**Warning signs:**
- A Murasaki handler references `White*`, `Red*`, `Yellow*`, or `Blue*` gameplay tables.
- AdminApi routes serve Murasaki by passing `GameEra.White` into existing services.
- Tests cover happy-path readback but not no-cross-era writes.

**Phase to address:**
Phase 3, "Murasaki Runtime Capability Binding"; Phase 6, "Murasaki AdminApi/WebUI and Runtime Closeout".

---

### Pitfall 8: Treating Challenge Arrays, Shopping, or Don Point Fields as Proven Feature Semantics

**What goes wrong:**
Murasaki gets server-side Don Challenge progress, shopping unlocks, Don Point spending, reward grants, or ChallengeCompe-style readback because the proto has `ary_challenge_stat`, playresult challenge arrays, `ShoppingResultRequest/Response`, `get_donpoint`, `reward_ptn`, `reward_progress`, and release-song hashes.

**Why it happens:**
Red and White already have Don Challenge/reward implementations, and Murasaki product context mentions Don Point behavior. Proto fields prove wire shape, not state authority, task source, readback target, reward timing, or shopping semantics.

**How to avoid:**
Create a separate collectable/shopping/challenge evidence phase after normal runtime works. Distinguish product context, local data, proto fields, request call order, IDA consumer evidence, and cabinet behavior. Implement only the parts with a proven runtime contract; otherwise return conservative compatibility/defaults and keep AdminApi/WebUI hidden.

**Warning signs:**
- Red/White Don Challenge sidecar schema is copied with only file names changed.
- `ShoppingResultRequest` mutates unlock state before a binary or cabinet pass proves shopping behavior.
- `ary_user_compe_stat` and `ary_bng_compe_stat` receive state rows because the fields exist.

**Phase to address:**
Phase 5, "Murasaki Collectables, Shopping, and Challenge Evidence".

---

### Pitfall 9: Proving Behavior With Source Grep or Proto Presence Alone

**What goes wrong:**
Route support, field semantics, data roots, byte limits, and test coverage are marked done because `rg` finds a string or the proto declares a message. This creates implementation confidence without evidence that the cabinet calls the route, consumes the response, or expects the encoded shape.

**Why it happens:**
Grep is fast and useful for scouting, and Murasaki binary scans do reveal message-name leads. But this repo's evidence rules require behavior proof from route tables, logs, captures, generated mappings, persisted state, loader output, and cabinet/RPCS3 acceptance.

**How to avoid:**
Use source/proto/binary string search as a lead generator only. For route/root claims, require IDA table proof or request captures. For mappings, inspect generated source. For persistence, test SQLite state transitions and no-cross-era boundaries. For cabinet compatibility, require user-observed RPCS3/cabinet smoke before closeout.

**Warning signs:**
- A research or plan artifact says "found in proto, therefore supported".
- Tests read `.cs`, `.csproj`, route attributes, migrations, or generated property names.
- No request log, IDA pointer/table note, or cabinet/RPCS3 observation exists for a runtime claim.

**Phase to address:**
All phases; especially Phase 1 and final closeout.

---

### Pitfall 10: Overclaiming Runtime and WebUI Verification

**What goes wrong:**
The milestone is called complete after server tests, a solution build, and a temp Host build, while actual cabinet/RPCS3 flow and WebUI operator review remain unrun. The repo then records compatibility claims stronger than the evidence supports.

**Why it happens:**
Automated verification is repeatable and often comprehensive on server behavior. It is still not the same as a cabinet client accepting routes, payloads, sequencing, and admin surfaces.

**How to avoid:**
Separate agent verification from user-observed acceptance. Automated checks can prove code behavior, persistence boundaries, mapper generation, and build output. Final Murasaki support requires recorded cabinet/RPCS3 smoke for implemented flows and user-accepted WebUI/AdminApi review. If the user owns the manual gate, stop before it and state exactly what was not run.

**Warning signs:**
- `PITFALLS`, `ROADMAP`, or verification docs say "runtime verified" with only `dotnet test` and `dotnet build` evidence.
- WebUI parity is marked complete without checking era-routed API responses and actual visible pages.
- A final summary omits "manual cabinet/RPCS3 not run" when that is true.

**Phase to address:**
Phase 6, "Murasaki AdminApi/WebUI and Runtime Closeout".

---

### Pitfall 11: Touching Unrelated Dirty Docs or Planning State

**What goes wrong:**
While researching or implementing Murasaki, an agent normalizes unrelated `.planning` docs, old White/Red artifacts, roadmap metrics, generated summaries, or quick-task docs. The actual diff becomes hard to review and may trample user or orchestrator changes.

**Why it happens:**
The repo has a large planning history and some docs contain stale or odd-looking state. Cleanup feels helpful, but the user's request is usually narrowly scoped.

**How to avoid:**
Check `git status --porcelain` before and after each task. Modify only phase-owned files and the requested artifact. If unrelated docs are dirty, leave them alone and work around them. Capture broader cleanup as a todo only when the user asks.

**Warning signs:**
- A Murasaki research task edits `STATE.md`, archived White/Red docs, or quick-task artifacts without being asked.
- A commit/diff contains formatting churn outside the active phase.
- Verification includes "fixed stale planning docs" when the task was protocol/runtime work.

**Phase to address:**
All phases; first task and final verification of each phase.

## Technical Debt Patterns

Shortcuts that seem reasonable but create long-term problems.

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Copying White controllers/mappers and renaming namespaces | Fast compile path | Masks split Murasaki request flow and wrong wire placement | Never without endpoint-by-endpoint Murasaki evidence |
| Using first matching config root | Catalog loads quickly | Wrong songs, presents, folders, or release hashes under cabinet flow | Never; root must be evidence-tagged |
| Filling byte arrays with White/Red widths | Responses are non-null | Byte packing bugs become hard to distinguish from client rejection | Only as temporary scaffold defaults with explicit unknown status |
| Persisting raw unknown fields as semantic columns | Looks future-proof | Schema names bake in unproven meanings | Preserve bounded raw facts only in evidence tables when there is a concrete replay/debug need |
| Handwriting Mapperly projection bodies | Bypasses mapper diagnostics | Reintroduces rejected mapper architecture and hidden business behavior | Only narrow conversion helpers configured for Mapperly |
| Adding source-grep tests | Easy coverage | Protects implementation shape instead of cabinet-visible behavior | Never for generated types, route attributes, project files, or source strings |
| Editing archived docs while nearby | Cleaner-looking repo | Pollutes active Murasaki diff and can overwrite user/orchestrator state | Never unless explicitly requested |

## Integration Gotchas

Common mistakes when connecting this era into the existing system.

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| Host route registration | Enabling Murasaki routes unconditionally | Register adapter and MVC application parts only when `GameEra.Murasaki` is enabled |
| Missing content-type fallback | Adding a broad AC15 fallback | Add only a proven exact Murasaki game prefix after route proof |
| Shared startup/version | Duplicating `/v01r00/chassis/*` in the Murasaki adapter | Keep shared ownership only if `proto/murasaki/vsinterface.proto` and client evidence agree; otherwise document the exception before code |
| Catalog loading | Hardcoding `wwwroot/data/murasaki` inside handlers | Resolve through `PathHelper`, era data path helpers, and `IGameDataCatalog.For(GameEra.Murasaki)` |
| Wire DTO generation | Hand-editing generated classes | Regenerate adapter-local wire from immutable `proto/murasaki` inputs |
| AdminApi/WebUI | Showing pages for unimplemented or data-only features | Expose only Murasaki-owned state with implemented readback and capability-gated UI |
| EF persistence | Sharing White/Red tables | Add Murasaki-owned tables and bind shared algorithms through concrete Murasaki DbSets |

## Performance Traps

Patterns that work at small scale but fail as usage grows.

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| Recomputing release/crown/song hash bytes on every request from XML | Slow `userdata`, `crownsdata`, `songhash`, or default-song responses | Build catalog snapshots and byte buffers once per catalog/version where semantics are proven | Large local catalogs or repeated cabinet polling |
| Loading every global best-score row for `BestScoreResponse` | High memory and slow responses | Prove `seq_id` paging and rank limits, then query bounded slices | Any meaningful global leaderboard data |
| Storing every raw request byte blob forever | SQLite growth and slow diagnostics | Store structured facts and bounded summaries; raw append-only tables only for evidence-backed replay needs | Long-running local servers with active cabinets |
| Using AdminApi to compute byte-heavy cabinet payloads live | WebUI latency and duplicated packing rules | Keep cabinet packing in application/protocol services and expose operator-friendly DTOs separately | Score/history pages over many songs/users |
| Per-request filesystem scans of `Host/wwwroot/data/murasaki/data` | Random latency and file-lock sensitivity | Validate required files at startup and serve through catalog services | Startup data roots with many fumen/music files |

## Security Mistakes

Domain-specific security issues beyond general web security.

| Mistake | Risk | Prevention |
|---------|------|------------|
| Murasaki routes active while era is disabled | Unintended protocol surface exposed | Verify disabled-era application-part removal and route absence |
| Broad missing-content-type fallback | Non-Murasaki requests treated as protobuf | Scope fallback to exact proven prefixes only |
| Logging full BAID/access token/content payloads by default | Sensitive local identifiers leak into logs | Log bounded summaries; only enable full dumps for explicit evidence capture |
| Persisting unknown payment/shopping semantics | Server becomes accidental authority for currency/unlocks | Keep shopping/Don Point state evidence-gated and non-authoritative until proven |
| Cross-era AdminApi writes | Operator edits Murasaki but mutates White/Red state | Route through `EraRoute.TryParse`, Murasaki-owned handlers, and no-cross-era tests |

## UX Pitfalls

Common user experience mistakes in this domain.

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| Showing Murasaki AdminApi/WebUI tabs before backing state exists | Operator trusts empty or wrong data | Hide or show unavailable state until Murasaki-owned readback is implemented |
| Reusing White labels for Murasaki-specific concepts | Confusing diagnostics and support notes | Use capability labels tied to Murasaki evidence, especially for split metadata and global score surfaces |
| Displaying raw byte fields as editable settings | Users can corrupt protocol state | Keep unknown bytes diagnostic-only or hidden until semantics are proven |
| Claiming manual verification inside automated status | User cannot tell what still needs cabinet/WebUI review | Separate automated checks from user-observed runtime acceptance |
| Mixing Don Challenge, ChallengeCompe, shopping, and Don Point wording | Operators infer unsupported reward behavior | Use explicit supported/unsupported feature names and evidence status |

## "Looks Done But Isn't" Checklist

Things that appear complete but are missing critical pieces.

- [ ] **Route foundation:** Controllers exist - verify IDA/capture evidence proves every Murasaki `.php` suffix and exact prefix.
- [ ] **Startup/version:** Shared `/v01r00` works - verify Murasaki `vsinterface` and client evidence support shared ownership.
- [ ] **Data root:** Catalog loads - verify active root selection, not just `ST5100-1`, `ST5100-7`, or `ST6100-1` file presence.
- [ ] **Wire generation:** C# compiles - verify `git status --porcelain -- proto/murasaki` is clean and generated wire is not hand-edited.
- [ ] **Mapperly:** Mapper declarations exist - verify emitted `.g.cs` for nontrivial Murasaki mappings.
- [ ] **Initial data replacement:** Metadata routes return data - verify split request families replace White initial-data assumptions deliberately.
- [ ] **Byte fields:** Responses contain byte arrays - verify lengths, defaults, and semantics with binary/client evidence.
- [ ] **Best score:** `bestscore` returns rows - verify `seq_id`, global/shop/local scope, rank count, and source data.
- [ ] **Persistence:** Normal play works - verify Murasaki-owned SQLite rows and no writes to White/Red/Yellow/Blue/Green/Nijiiro gameplay tables.
- [ ] **Shopping/Don Point:** Don Point counters change - verify request flow and authority before mutating unlocks or balances.
- [ ] **AdminApi/WebUI:** Pages render - verify they read/write only implemented Murasaki-owned state and hide unsupported features.
- [ ] **Closeout:** Tests pass - verify cabinet/RPCS3 smoke and user-accepted WebUI/AdminApi review are recorded or explicitly deferred.

## Recovery Strategies

When pitfalls occur despite prevention, how to recover.

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| White parity copied too broadly | HIGH | Stop runtime expansion, create endpoint-by-endpoint Murasaki evidence matrix, remove unsupported wiring, rebind only proven surfaces |
| Wrong route/root locked | HIGH | Freeze route work, extract IDA/request proof, update evidence artifact, change routes/catalog paths before adding runtime state |
| Unknown byte semantics persisted | MEDIUM | Rename semantic columns to raw facts or remove migration if not shipped, add bounded logging, rerun binary pass before modeling |
| Proto or generated wire hand-edited | MEDIUM | Revert schema/generated edits, regenerate from `proto/murasaki`, fix mapper/Application boundaries instead |
| Handwritten Mapperly aggregation introduced | MEDIUM | Move behavior to handlers/Application DTOs, reduce mappers to generated projections, inspect emitted `.g.cs` |
| Cross-era persistence found | HIGH | Add no-cross-era regression tests, migrate/correct contaminated local state if needed, replace shared table access with Murasaki-owned DbSets |
| Source-grep verification accepted | LOW | Replace with behavior-facing tests, generated-source inspection, route/capture evidence, and artifact review |
| Runtime verification overclaimed | LOW | Amend verification docs with exact commands run and manual gates not run; do not close milestone until user acceptance exists |
| Unrelated docs changed | LOW | Leave user/orchestrator changes intact, revert only agent-made unrelated edits if clearly safe, or ask before touching ambiguous dirty files |

## Pitfall-to-Phase Mapping

How roadmap phases should address these pitfalls.

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| White parity assumption | Phase 1 and Phase 2 | Murasaki feature inventory maps every reused capability to Murasaki proto/data/binary evidence |
| Route/root guessing | Phase 1 | Evidence artifact proves route prefix, suffixes, transport, startup/version ownership, and active root before route code |
| Unknown byte fields | Phase 2 and Phase 4 | Byte-field table records source, width, default, consumer, and modeled capability or unknown status |
| Global best-score confusion | Phase 4 | `bestscore` behavior is separate from self-best, with proven paging/scope or documented compatibility-only response |
| Proto/generated edits | Every phase | `git status --porcelain -- proto/murasaki` clean; generated wire changes tied to regeneration |
| Handwritten Mapperly projections | Phase 2, Phase 3, Phase 6 | `dotnet build /p:EmitCompilerGeneratedFiles=true` output inspected for nontrivial mappings |
| Cross-era persistence | Phase 3 and Phase 6 | Handler tests prove Murasaki-owned writes and no writes to other era gameplay tables |
| Challenge/shopping overreach | Phase 5 | Separate evidence record distinguishes proto fields, product context, local data, request flow, and implemented behavior |
| Source-grep proof | All phases | Tests exercise handler state, parser output, protocol packing, route behavior, build output, or API responses |
| Runtime overclaim | Phase 6 | Verification record separates automated tests/builds from user-observed cabinet/RPCS3 and WebUI acceptance |
| Unrelated dirty docs | All phases | Pre/post `git status --porcelain`; diffs limited to phase-owned or explicitly requested files |

## Sources

Primary local evidence:

- `.planning/PROJECT.md` - v1.5 Murasaki goal, active requirements, evidence hierarchy, route/root/byte-field warnings, out-of-scope invented semantics, and pending Murasaki decisions.
- `.planning/MILESTONES.md` - White/Red shipped patterns: route/root proof before runtime work, proto immutability, generated wire ownership, runtime closeout evidence.
- `.planning/STATE.md` - prior phase decisions on active-root proof, Mapperly rewrite, no source-grep tests, White final/legacy wire separation, and verification boundaries.
- `AGENTS.md` - repo architecture rules, generated protobuf mapping rules, Mapperly source-generation requirements, testing rules, data caveats, and common verification commands.
- `proto/murasaki/taiko.proto` - Murasaki game request/response schema, split metadata families, byte-heavy fields, challenge arrays, `BestScoreResponse`, and `ShoppingResultRequest/Response`.
- `proto/murasaki/vsinterface.proto` - Murasaki startup/version schema used to evaluate shared `/v01r00` compatibility.
- `Host/wwwroot/data/murasaki/data/config/ST5100-1`, `ST5100-7`, and `ST6100-1` - local Murasaki data roots; data availability is not root-selection proof.
- `.tools/murasaki/EBOOT.ELF.i64` - nonzero local binary/IDA evidence handle for route/root/field consumer investigation.
- `.planning/milestones/v1.4-ROADMAP.md` - White phase structure and evidence-first ordering.
- `.planning/milestones/v1.4-phases/23-white-evidence-and-era-foundation/23-WHITE-EVIDENCE.md` - precedent for route table proof, approved suffix allowlist, and separating route proof from runtime semantics.
- `.planning/milestones/v1.3-ROADMAP.md` - Red capability composition precedent, ChallengeCompe correction, runtime/AdminApi/WebUI closeout sequencing.

---
*Pitfalls research for: v1.5 Murasaki AC15 Support*
*Researched: 2026-06-21*
