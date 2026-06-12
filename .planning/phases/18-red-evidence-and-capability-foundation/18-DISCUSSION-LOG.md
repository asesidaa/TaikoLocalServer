# Phase 18: Red Evidence and Capability Foundation - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md - this log preserves the alternatives considered.

**Date:** 2026-06-13
**Phase:** 18-red-evidence-and-capability-foundation
**Areas discussed:** Evidence lock level, Capability inventory shape, Adapter scaffold exposure, Preservation gates

---

## Evidence Lock Level

### Route Prefix And Version Boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Provisional + gaps | Use strongest local evidence to name a candidate route/version, but mark runtime/IDA/log proof gaps as blockers for dependent behavior. | |
| Proof before route | Do not add a concrete Red game route prefix until runtime, IDA, or client logs prove it. | |
| Planner decides | Let the planner choose the strictness after reviewing proto, data, and current route patterns. | |
| Other | Use the Red IDB under `.tools/red/EBOOT.ELF.i64`; route format is `/vxxryy`, with `xx` from startup-auth version and `yy` from matching revision/proto. | Yes |

**User's choice:** Other.
**Notes:** User directed the workflow to use `$ida-cli` and the Red IDB. IDA found `/v08r01` at `0xDA7660` and later confirmed `/v01r00` at `0xDA76A0`.

### Active Root

| Option | Description | Selected |
|--------|-------------|----------|
| Lock ST8100-1 | Use ST8100-1 as the active Red runtime root; inventory ST5100/ST7100 as local inactive/historical roots only. | Yes |
| Inventory all roots | Do not choose an active catalog root yet; require the implementation plan to compare ST5100/ST7100/ST8100 before locking. | |
| Runtime first | Leave active root unresolved until cabinet/RPCS3 logs confirm which root is used. | |

**User's choice:** Lock ST8100-1.
**Notes:** IDB strings include `ST8100-1`, `/data/config/ST8100-1`, `/data/nutdata/ST8100-1`, `/updates/ST8100-1`, and `ST8100-1-NA-MPR0-K01`.

### Byte Limits

| Option | Description | Selected |
|--------|-------------|----------|
| Prove from IDB | Do not reuse Blue/Yellow limits by default; require IDB-backed evidence for widths, packing, and response byte formats before adding a Red AC15 profile. | Yes |
| Use AC15 defaults | Start from existing Blue/Green/Yellow limits and only override if runtime evidence later breaks them. | |
| Planner decides | Let the planner choose after comparing proto, IDB, and current AC15 profile code. | |

**User's choice:** Prove from IDB.
**Notes:** Red limits and byte-format compatibility must be proven before profile binding.

### Evidence Artifact Depth

| Option | Description | Selected |
|--------|-------------|----------|
| Addresses + findings | Record exact IDB string/function/table addresses plus conclusions, so later agents can re-check the evidence quickly. | Yes |
| Findings only | Keep the artifact concise: route, root, transport, gaps, and capability inventory, without IDB address detail. | |
| Full RE notes | Create deeper reverse-engineering notes with decompile snippets and call-path commentary for every checked field. | |

**User's choice:** Addresses + findings.
**Notes:** Known route evidence addresses include `0xDA7660`, `0xDA76A0`, `0xDA8330`, `0xDA8348`, `0xDA8360`, `0xDA96F8`, `0xDAA758`, and `0xD72FC0`.

### Runtime Gap Policy

| Option | Description | Selected |
|--------|-------------|----------|
| Mark acceptance gaps | Implement foundation from IDB/local proto/data evidence, but mark runtime/RPCS3 request traces as acceptance gaps for later closeout. | Yes |
| Block foundation | Do not add the Red adapter foundation until runtime logs confirm the same route and flow. | |
| Ignore for now | Treat IDB proof as enough for Phase 18 and leave runtime traces entirely to Phase 22. | |

**User's choice:** Mark acceptance gaps.
**Notes:** User clarified that Phase 18 needs a minimal adapter controller shape so request routing and game startup can be checked.

---

## Capability Inventory Shape

### Inventory Format

| Option | Description | Selected |
|--------|-------------|----------|
| Evidence matrix | Classify each Red proto/data/IDB route as supported, candidate, absent, or later-phase, with evidence refs and no behavior implementation implied. | Yes |
| Preliminary profile | Draft a Red Ac15EraProfile in Phase 18 so Phase 19 starts from a concrete profile shape. | |
| Route list only | Keep Phase 18 narrow: route/version/root and adapter scaffold only; leave capability classification to Phase 19. | |

**User's choice:** Evidence matrix.
**Notes:** The profile is deferred to Phase 19.

### Matrix Grouping

| Option | Description | Selected |
|--------|-------------|----------|
| By phase owner | Group rows by Phase 18 foundation, Phase 19 catalog/profile, Phase 20 runtime/simple compatibility, Phase 21 ChallengeCompe, and explicit absent surfaces. | Yes |
| By endpoint | Use one row per route/proto message only; downstream planners infer phase ownership. | |
| By shared module | Group by AC15 shared modules like userdata, initial data, normal play, Dani, item shop, and challenge. | |

**User's choice:** By phase owner.
**Notes:** This keeps Phase 18 from pulling later behavior into the scaffold.

### ChallengeCompe Classification

| Option | Description | Selected |
|--------|-------------|----------|
| Shared candidate | Mark it as a Red-proven lead for the Phase 21 shared older-AC15 capability; include route/proto/IDB evidence but no stateful semantics yet. | Yes |
| Red foundation route | Include a minimal routed controller in Phase 18 and treat ChallengeCompe as part of Red foundation compatibility. | |
| Defer entirely | Only mention that ChallengeCompe exists in proto; leave route and evidence details to Phase 21. | |

**User's choice:** Shared candidate.
**Notes:** Phase 21 owns shared ChallengeCompe semantics.

### Unsupported Surfaces

| Option | Description | Selected |
|--------|-------------|----------|
| Compatibility only | List rewardcard/rewardexecution/Banacoin-adjacent as simple compatibility candidates only when IDB/runtime flow requires them; no shop, medal, wallet, or unlock semantics. | Yes |
| Proto equals support | Any route or proto message present in Red becomes a supported capability unless runtime proves otherwise. | |
| Route absent by default | Do not add or classify reward/Banacoin-adjacent surfaces until runtime logs show the cabinet calls them. | |

**User's choice:** Compatibility only.
**Notes:** No inferred shop, medal, wallet, payment, or unlock behavior.

---

## Adapter Scaffold Exposure

### Route Breadth

| Option | Description | Selected |
|--------|-------------|----------|
| IDB route probes | Expose the IDB-known /v08r01 route suffixes as minimal log/protobuf route probes so runtime smoke can reveal the actual request sequence. | Yes |
| Core flow only | Expose only startup/version, identity, userdata, initialdata, and playresult routes; leave other IDB-known routes absent until later phases. | |
| No game routes | Generate wire and register the adapter, but keep controllers absent until Phase 19/20. | |

**User's choice:** IDB route probes.
**Notes:** Route probes do not prove feature semantics.

### Probe Semantics

| Option | Description | Selected |
|--------|-------------|----------|
| Log + minimal protobuf | Deserialize/map enough to log full requests and return the minimal safe Red protobuf response; no DB writes, no shared gameplay calls, and clear evidence-gap notes. | Yes |
| Return 404 for unknown | Expose only routes with already-understood response semantics; let other IDB-known requests fail until researched. | |
| Copy Yellow behavior | Use Yellow controller behavior as a temporary compatibility layer where proto messages look similar. | |

**User's choice:** Log + minimal protobuf.
**Notes:** No state writes and no Yellow/Blue behavior copy.

### Startup Route Ownership

| Option | Description | Selected |
|--------|-------------|----------|
| Red /v08r01 probes | Add Red-owned /v08r01 startupauth/verup/verupcomplete route probes for runtime verification, while documenting that existing /v01r00 ownership was only proven for later AC15 eras. | |
| Actual /v01r00 paths | Keep startupauth/verup only under /v01r00 for Red, despite the Red IDB base URL evidence. | |
| Expose both temporarily | Use both paths as discovery probes and let runtime logs decide which remains. | |
| Other | User said `/v01r00` likely exists in the IDB and the first sweep missed it. | Yes |

**User's choice:** Other.
**Notes:** Follow-up IDA search confirmed `/v01r00` at `0xDA76A0`; startup/version remains shared `/v01r00`, game routes use `/v08r01`.

### Red Config Default

| Option | Description | Selected |
|--------|-------------|----------|
| Enabled locally | Add Red with Enabled=true and GameDataPath=wwwroot/data/red/data so immediate RPCS3/request-routing smoke is possible in this checkout. | Yes |
| Disabled by default | Add the Red settings shape but keep Enabled=false to avoid requiring Red local data on startup. | |
| No config entry | Add code support only; leave ServerSettings.json unchanged until runtime testing. | |

**User's choice:** Enabled locally.
**Notes:** Red local data is present in this checkout.

### Wire Generation Boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Generate from proto/red | Create adapter-local Red Wire output from immutable proto/red inputs; do not edit dumped proto sources except for documented protogen compatibility. | Yes |
| Copy Yellow wire first | Start with Yellow generated wire and adjust only compile breaks until Red proto generation is planned later. | |
| Manual DTO subset | Hand-author only route-probe request/response DTOs needed for the first runtime smoke. | |

**User's choice:** Generate from proto/red.
**Notes:** No copied Yellow wire and no hand-authored subset DTOs.

---

## Preservation Gates

### Verification Breadth

| Option | Description | Selected |
|--------|-------------|----------|
| Focused + build | Run focused host/adapter/gating/wire tests plus affected Green/Blue/Yellow shared-route tests and a temp-output Host build; use full suite only if broad shared code changes. | Yes |
| Full suite always | Require full Tests.csproj plus temp-output Host build before Phase 18 can close. | |
| Build only | Rely on compile and a temp-output Host build; leave behavior tests for later runtime phases. | |

**User's choice:** Focused + build.
**Notes:** User added: "As usual, only add meaningful tests."

### Persistence Scope

| Option | Description | Selected |
|--------|-------------|----------|
| No gameplay tables | Keep Phase 18 to adapter/wire/settings/evidence/route probes; Red save/score/Dani/Tokkun/Challenge tables wait for capability phases. | Yes |
| Identity/save only | Add Red-owned default save/profile tables now so route probes can create basic users. | |
| Skeleton all tables | Create empty Red-owned tables for expected future capabilities even before runtime behavior exists. | |

**User's choice:** No gameplay tables.
**Notes:** No Red EF gameplay migrations in Phase 18.

### Runtime Smoke Gate

| Option | Description | Selected |
|--------|-------------|----------|
| Best-effort smoke | After route probes compile, run or ask for a minimal RPCS3/cabinet smoke to confirm requests route; record gaps if not available. | |
| Required smoke | Do not close Phase 18 until runtime request-routing smoke is completed and logged. | Yes |
| Automated only | Leave all cabinet/RPCS3 proof to Phase 22 and close Phase 18 on automated verification only. | |

**User's choice:** Required smoke.
**Notes:** User will run it manually.

### Shared Route Preservation

| Option | Description | Selected |
|--------|-------------|----------|
| Existing eras unchanged | Review must prove Green/Blue/Yellow/Nijiiro route behavior, shared /v01r00 startup/version behavior, and AC15 shared-core behavior are unchanged except covered Red additions. | Yes |
| Red only | Review only checks Red adapter behavior; existing era preservation is implied by build/tests. | |
| Gating only | Focus review on enabled-era application-part gating and config parsing, not behavior surfaces. | |

**User's choice:** Existing eras unchanged.
**Notes:** User added: "Do not do a heavy review."

---

## the agent's Discretion

- Exact Red adapter project/file layout.
- Exact route-probe helper organization and minimal response helper names.
- Whether IDB findings and capability matrix are one evidence artifact or separate Phase 18 artifacts.

## Deferred Ideas

- Red profile/catalog binding: Phase 19.
- Red runtime capability binding and simple compatibility behavior: Phase 20.
- Shared older-AC15 ChallengeCompe contract and state: Phase 21.
- Red AdminApi/WebUI/runtime closeout: Phase 22.
