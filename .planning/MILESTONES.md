# Milestones

## v1.6 KIMIDORI AC15 Support (Shipped: 2026-06-25)

**Phases completed:** 4 phases, 4 plans, 0 tasks

**Delivered:** KIMIDORI 0.12 is now a first-class older AC15 era with KIMIDORI-owned `/v05r00` game routes, shared `/v01r00` startup/version routing, generated wire DTOs, root-level catalog support, runtime state, Dani Dojo, normal play, AdminApi/WebUI support, songhash-backed catalog enablement, and accepted runtime closeout.

**Key accomplishments:**

- Proved KIMIDORI route/proto/data evidence from `proto/kimidori`, `.tools/kimidori/EBOOT.ELF.i64`, and linked root-level game data before runtime support.
- Added first-class KIMIDORI adapter identity with generated wire DTOs, Host settings/DI/application-part gating, and `/v05r00/chassis/*.php` route ownership while preserving shared `/v01r00` startup/version behavior.
- Bound KIMIDORI root-level catalog and metadata loading without assuming newer `config/STxxxx-*` AC15 layouts.
- Added KIMIDORI-owned identity, userdata, self-best, crowns, favorites, recent songs, release-song readback, normal playresult mutation, Dani Dojo state, Don Point/reward/unlock state, and bounded shopping-result compatibility.
- Fixed the runtime songhash enablement gap from KIMIDORI binary evidence by returning the file-order song hash table and compacting hash-indexed payloads.
- Exposed implemented KIMIDORI-owned state through AdminApi/WebUI while keeping Taikojuku, Tokkun, Banacoin, battle, Don Challenge, ChallengeCompe, and full shop-authority controls absent.
- Closed with user-observed KIMIDORI cabinet/RPCS3 runtime acceptance recorded on 2026-06-25.

**Stats:**

- 4 phases complete
- 4 GSD plans complete
- 19/19 v1.6 requirements complete
- Open artifact audit clear at close

**Archived:**

- `.planning/milestones/v1.6-ROADMAP.md`
- `.planning/milestones/v1.6-REQUIREMENTS.md`
- `.planning/milestones/v1.6-phases/`

**What's next:** Start a fresh milestone with `$gsd-new-milestone`.

---

## v1.5 Murasaki AC15 Support (Shipped: 2026-06-23)

**Delivered:** Murasaki is now a first-class older AC15 era with Murasaki-owned `/v06r00` compatibility routes, final `/v06r01` route parity where binary/proto evidence supports it, generated wire DTOs, catalog/profile binding, runtime persistence, split metadata readback, normal play, Dani/Taikojuku, reward/Don Point state, AdminApi/WebUI support, and accepted in-game closeout.

**Phases completed:** 7 phases, 7 plans, plus quick final-route task 260623-2ff

**Key accomplishments:**

- Proved Murasaki route/root/transport evidence before runtime work, including shared `/v01r00` startup/version routing and `/v06r00` game-route ownership.
- Added first-class Murasaki adapter identity with generated wire DTOs, Host settings/DI/application-part gating, direct-protobuf fallback, and Murasaki-owned controller files.
- Bound the `ST6100-1` catalog root and AC15 capability/profile limits, including favorite cap 10, Don Point/root boundaries, present/special-BAID data, folders, telops, movies, recommendations, and Taikojuku sidecars.
- Implemented Murasaki split metadata readback without adding a White-style `initialdatacheck.php` contract.
- Added Murasaki-owned identity, userdata, self-best, crowns, favorites, recent songs, release-song readback, normal playresult mutation, Dani state, reward/progress, profile counters, and unlock state with no cross-era gameplay writes.
- Resolved special surfaces conservatively: `bestscore.php`, `songhash.php`, `shoppingresult.php`, Don Challenge, ChallengeCompe, Yellow shop, Banacoin, battle, and global-score persistence remain absent until Murasaki-specific evidence proves them.
- Exposed implemented Murasaki-owned state through AdminApi/WebUI while keeping unsupported controls absent.
- Added final `/v06r01/chassis` support with schema-separated final and compatibility `getfolder.php` behavior, while preserving binary-supported final Dani/Taikojuku.
- Closed with user-observed Murasaki in-game acceptance, no vulnerable packages, 865 passing tests, and a full solution build with 0 warnings and 0 errors.

**Stats:**

- 7 phases complete
- 7 GSD plans complete
- 1 quick final-route task complete
- Milestone audit passed: 19/19 requirements satisfied
- Final verification at close: no vulnerable packages, `dotnet test Tests/Tests.csproj --no-build` passed 865/865, and `dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true` passed with 0 warnings/errors.

**Archived:**

- `.planning/milestones/v1.5-ROADMAP.md`
- `.planning/milestones/v1.5-REQUIREMENTS.md`
- `.planning/milestones/v1.5-MILESTONE-AUDIT.md`
- `.planning/milestones/v1.5-phases/`

**What's next:** Start a fresh milestone with `$gsd-new-milestone`.

---

## v1.4 White AC15 0.13 Support (Shipped: 2026-06-21)

**Delivered:** White is now a first-class older AC15 era with White-owned protocol routes, generated wire DTOs, catalog/profile binding, runtime persistence, normal play, Dani, reward/Don Point state, server-side Don Challenge readback, AdminApi/WebUI support, and accepted runtime/WebUI closeout.

**Phases completed:** 6 phases, 10 plans, 4 tracked summary tasks

**Key accomplishments:**

- Proved White `/v07r00/chassis` route boundaries, shared `/v01r00/chassis` startup/version ownership, direct-protobuf transport, active `ST7100-1` data root, and no-state scaffold limits before runtime work.
- Added first-class White adapter, generated wire DTOs, Host settings/DI/application-part gating, and exact content-type fallback without modifying dumped proto inputs.
- Bound White catalog/profile/runtime behavior through White-owned tables and explicit AC15 capability profiles for identity, userdata, normal play, self-best, crowns, favorites, recent songs, Dani, recommendations, folders/telops, reward fields, and Don Points.
- Collected White present/special-BAID provenance and implemented White Don Challenge only as server-side stage-derived progress with White-owned data/state/AdminApi/WebUI readback.
- Exposed White AdminApi/WebUI routes for implemented White-owned surfaces while keeping item shop, battle, Tokkun, WaiWai, gacha runtime, Banacoin authority, later White behavior, and ChallengeCompe cabinet controls absent.
- Closed with clear automated verification, generated-source inspection, temp-output Host build evidence, and user-accepted RPCS3/cabinet/WebUI verification.

**Stats:**

- 6 phases complete
- 10 GSD plans complete
- 4 tracked summary tasks reported by the archive helper
- Full verification at close: `dotnet test Tests/Tests.csproj --no-restore` passed 829/829, solution build passed, and temp-output Host build with generated-source emission passed with 0 warnings/errors.

**Archived:**

- `.planning/milestones/v1.4-ROADMAP.md`
- `.planning/milestones/v1.4-REQUIREMENTS.md`
- `.planning/milestones/v1.4-phases/`

**What's next:** Start a fresh milestone with `$gsd-new-milestone`.

---

## v1.3 Red AC15 Support (Shipped: 2026-06-16)

**Phases completed:** 6 phases, 29 plans, 47 tasks

**Delivered:** Red is now a first-class AC15 era with Red-owned protocol routes, generated wire DTOs, catalog/profile binding, runtime persistence, normal play, Dani, Tokkun tutorial readback, simple compatibility routes, server-side Don Challenge behavior, separate ChallengeCompe protocol compatibility stubs, AdminApi/WebUI readback, and final runtime closeout evidence.

**Key accomplishments:**

- IDA-backed Red route/root evidence and phase-owned capability matrix before Red runtime implementation.
- Red AC15 adapter identity with generated adapter-local protobuf DTOs from immutable `proto/red` inputs.
- Red Host enablement with scoped protobuf fallback and settings validation that preserves unsupported-shop boundaries.
- No-state Red route probes with user-confirmed basic RPCS3/cabinet connection.
- AC15 BAID/userdata assembly now uses semantic application sections, controller-owned final wire assembly, and Mapperly-generated section application across Blue, Green, Yellow, and Red.
- Evidence-gated Don Challenge catalog contract with Red sidecar loading.
- Red-owned Don Challenge state with active-task matching, Tokkun exclusion, and shared progress evaluation.
- Configured Red Don Challenge rewards now grant Red unlock flags and hide active unearned reward songs through userdata.
- Red `challengecompe.php` remains a separate empty protocol compatibility stub.
- Final Don Challenge and protocol-stub verification with command evidence, decision audit, phase closeout, and Phase 22 handoff.
- Red normal AdminApi readback and profile editing over Red-owned runtime tables and Red catalog slices
- Dedicated Red Don Challenge AdminApi read model with no opt-in gates and cabinet ChallengeCompe kept as a separate compatibility surface
- Red-aware generic WebUI routing plus a read-only Don Challenge Play Data page backed by dedicated availability/readback services
- Red AdminApi/WebUI and runtime closeout passed with automated verification, temp-output Host build, and user-accepted manual runtime evidence.

**Stats:**

- 6 phases complete
- 29 GSD plans complete
- 47 tracked summary tasks
- Full verification at close: `dotnet test Tests/Tests.csproj` passed 778/778, `dotnet build TaikoLocalServer.slnx` passed, and temp-output Host build passed with 0 warnings/errors.

**Archived:**

- `.planning/milestones/v1.3-ROADMAP.md`
- `.planning/milestones/v1.3-REQUIREMENTS.md`

**What's next:** Start a fresh milestone with `$gsd-new-milestone`.

---

## v1.2 Yellow AC15 Support (Shipped: 2026-06-12)

**Delivered:** Yellow is a first-class AC15 era with Yellow-owned protocol routes, catalog loading, normal play, Dani, shop/medals, WaiWai logging/readback, Tokkun, stateless Banacoin-adjacent compatibility, AdminApi/WebUI readback, runtime verification, and final contract documentation.

**Phases completed:** 8 phases, 38 plans, 69 tasks

**Key accomplishments:**

- Added Yellow as an enableable adapter with generated Yellow wire DTOs, `/v09r02/chassis/*` game routes, shared `/v01r00/chassis/*` startup/version ownership, and no-battle absence guardrails.
- Loaded Yellow `ST9100-1` catalog data and implemented Yellow-owned identity, userdata, self-best, crown, normal play, Dani, shop, medal, favorite, recent, AdminApi, and WebUI readback paths.
- Implemented evidence-bounded Yellow WaiWai and Tokkun behavior, including Tokkun classification before normal play, nullable tutorial readback, append-only raw Tokkun history, and no-cross-mode writes.
- Kept Yellow Banacoin-adjacent routes stateless and non-authoritative while logging compatibility requests.
- Regenerated AC15 wire DTOs with nullable optional primitives, moved AC15 protocol projection to Mapperly, and simplified shared AC15 core behavior without merging era-owned persistence.
- Closed the milestone with full `dotnet test Tests/Tests.csproj` verification (683 passed), temp-output Host build (0 warnings/errors), and user-confirmed RPCS3 Yellow smoke.

**Stats:**

- 8 phases complete
- 38 GSD plans complete
- 69 tracked summary tasks
- Full verification at close: `dotnet test Tests/Tests.csproj` passed 683/683 and Host temp-output build passed with 0 warnings/errors
- GSD range before archive: `2f33a6af` -> `4ff19868`

**Archived:**

- `.planning/milestones/v1.2-ROADMAP.md`
- `.planning/milestones/v1.2-REQUIREMENTS.md`
- `.planning/milestones/v1.2-phases/`

**What's next:** Start a fresh milestone with `$gsd-new-milestone`.

---
## v1.1 Blue Tokkun Mode Support (Shipped: 2026-06-07)

**Delivered:** Evidence-backed Blue Tokkun support with stateless Banacoin-adjacent compatibility, Tokkun playresult handling, Blue-owned Tokkun persistence/readback, runtime verification, and final contract documentation.

**Phases completed:** 5 phases, 7 plans, 17 tasks

**Key accomplishments:**

- Blue Tokkun evidence contract plus stale source-guard reset, with archive-aware Blue regression tests restored
- Blue getbanacoininfo.php stateless compatibility route returning only `Result = 1`, with route ownership and no-state verification.
- Blue Tokkun playresult uploads now preserve protocol-backed raw facts and return success without normal, battle, shop, Dani, favorite, recent-song, profile, unlock, medal, customization, or title writes.
- Blue Tokkun mode classification and EF schema for nullable tutorial state plus append-only raw stage history
- Classified Blue Tokkun uploads now persist only raw tutorial and stage-history facts while preserving Phase 9 no-cross-write boundaries
- Blue userdata now reads back persisted Tokkun tutorial state through the proven optional protocol field only
- Blue Tokkun runtime verification is recorded and the final v1.1 contract is documented.

**Stats:**

- 5 phases complete
- 7 GSD plans complete
- 17 tracked summary tasks
- Full verification at close: `dotnet test Tests/Tests.csproj` passed 638/638 and Host temp-output build passed with 0 warnings/errors
- GSD range before archive: `a1b4d362` -> `6cf245a7`

**Archived:**

- `.planning/milestones/v1.1-ROADMAP.md`
- `.planning/milestones/v1.1-REQUIREMENTS.md`
- `.planning/milestones/v1.1-phases/`

**What's next:** Start a fresh milestone with `$gsd-new-milestone`.

---

## v1.0 Blue Support (Shipped: 2026-06-03)

**Delivered:** Full Blue-era support across normal play, AdminApi/WebUI readback, item shop/unlocking, battle evidence/design, battle runtime behavior, final guardrails, and operator documentation.

**Phases completed:** Phases 1-6, 30 roadmap plans. 20 GSD plan summaries were tracked on disk; phases 2, 3, and 6 were closed through quick-task and external verification records.

**Key accomplishments:**

- Implemented Blue item-shop advertisement, purchases, season Don medal state, configured unlocks, and locked userdata/BAID readback without Green shop state.
- Completed Blue AdminApi and WebUI parity for supported readback surfaces, including Blue customization catalog data.
- Proved and hardened normal Blue support before battle runtime work.
- Produced a strict Blue battle evidence/design gate from proto, local XML inventory, IDA/client evidence, and explicit unresolved-row handling.
- Implemented Blue-owned battle persistence, `battleuserdata.php`, battle initialdata, battle playresult storage, store/echo rewards, and normal-state protection.
- Closed final verification/docs state after external normal and battle smoke confirmation, with stale debug/UAT artifacts archived as resolved.

**Stats:**

- 6 phases complete
- 30 roadmap plans complete
- 20 tracked GSD plan summaries and 33 tracked summary tasks
- GSD range before archive: `1fbc4391` -> `6ca3cb92`

**Archived:**

- `.planning/milestones/v1.0-ROADMAP.md`
- `.planning/milestones/v1.0-REQUIREMENTS.md`
- `.planning/milestones/v1.0-phases/`

**What's next:** Start a fresh milestone with `$gsd-new-milestone`.

---
