# Phase 18: Red Evidence and Capability Foundation - Context

**Gathered:** 2026-06-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 18 proves Red AC15 route/version/transport/data-root evidence, inventories Red-supported and Red-candidate capabilities, and adds the first-class Red adapter foundation. It should create enough Red route-probe surface to run manual RPCS3/cabinet request-routing smoke and collect logs, but it must not implement Red gameplay persistence, normal-play state, Dani state, Tokkun state, ChallengeCompe state, item-shop behavior, medal behavior, wallet/payment behavior, or copied Yellow/Blue runtime semantics.

</domain>

<decisions>
## Implementation Decisions

### Evidence Lock Level
- **D-01:** Use `.tools/red/EBOOT.ELF.i64` through an `ida-cli` daemon as the authority for Red route/version/root/limit evidence. Route prefixes are in `/vxxryy` form, where `xx` is the startup-auth version and `yy` is the matching revision/proto revision.
- **D-02:** Red startup/version routes remain shared `/v01r00` routes. The Red IDB contains `https://%s:%s@%s:%d/v01r00` at `0xDA76A0`, plus `chassis/startupauth.php` at `0xDA8330`, `chassis/verupauth.php` at `0xDA8348`, and `chassis/verupcomplete.php` at `0xDA8360`.
- **D-03:** Red game routes use `/v08r01`. The Red IDB contains `https://%s:%s@%s:%d/v08r01` at `0xDA7660`.
- **D-04:** Lock `ST8100-1` as the active Red runtime root. Inventory `ST5100` and `ST7100` local roots as inactive or historical only. IDB evidence includes `/data/config/ST8100-1`, `/data/nutdata/ST8100-1`, `/updates/ST8100-1`, `/cache/ST8100-1`, and `ST8100-1-NA-MPR0-K01`.
- **D-05:** Prove Red flag-array widths, packing, and response byte formats from the IDB before adding or accepting a Red AC15 profile. Do not silently inherit Blue/Yellow protocol limits.
- **D-06:** The Phase 18 evidence artifact should include both findings and exact IDB addresses so later agents can re-check the proof quickly. Known route/string evidence includes `0xDA7660`, `0xDA76A0`, `0xDA8330`, `0xDA8348`, `0xDA8360`, `0xDA96F8`, `0xDAA758`, and `0xD72FC0`.
- **D-07:** Runtime logs are acceptance evidence, not a foundation blocker. Phase 18 must still create enough minimal Red route-probe controller shape to let the user run the game and verify request routing.

### Capability Inventory Shape
- **D-08:** Produce a Red capability evidence matrix. Classify each Red proto/data/IDB route or surface as supported, candidate, absent, or later-phase, with evidence refs and without implying behavior implementation.
- **D-09:** Do not draft the full Red `Ac15EraProfile` in Phase 18. Phase 19 owns the profile after the evidence matrix proves what can be bound.
- **D-10:** Group matrix rows by phase owner: Phase 18 foundation, Phase 19 catalog/profile, Phase 20 runtime/simple compatibility, Phase 21 ChallengeCompe, and explicit absent surfaces.
- **D-11:** Classify Red ChallengeCompe as a shared older-AC15 capability candidate with Red route/proto/IDB evidence. Phase 21 owns the shared contract and any stateful semantics.
- **D-12:** Classify rewardcard, rewardexecution, and Banacoin-adjacent rows as simple compatibility candidates only when IDB or runtime flow requires them. Do not infer item-shop, medal, wallet, payment, or unlock semantics.

### Adapter Scaffold Exposure
- **D-13:** Expose IDB-known Red game-route suffixes as minimal route probes under `/v08r01` so runtime smoke can reveal the real request sequence. These probes do not prove feature semantics.
- **D-14:** Route probes should deserialize or map enough to log full requests and return the minimal safe Red protobuf response. They must not write DB state, call shared gameplay services, or copy Yellow/Blue behavior.
- **D-15:** Generate adapter-local Red `Wire/` output from immutable `proto/red` inputs. Do not copy Yellow wire and do not hand-author a DTO subset. Dumped proto inputs remain evidence and should not be edited except for documented protogen compatibility.
- **D-16:** Add Red host/settings support with Red enabled in the committed local config and `GameDataPath=wwwroot/data/red/data`, so immediate RPCS3/request-routing smoke is possible in this checkout.
- **D-17:** Keep Red route ownership adapter-local: new Red controllers live in a Red adapter assembly, generated Red wire stays in that adapter, and Host application-part gating controls Red route availability.

### Preservation Gates
- **D-18:** Use focused automated verification plus a temp-output Host build. Add only meaningful tests that protect observable behavior, enabled-era gating, wire/protobuf shape, or real preservation boundaries.
- **D-19:** Do not add Red gameplay persistence or EF migrations in Phase 18. Red save, score, Dani, Tokkun, ChallengeCompe, and compatibility state belong to later capability phases.
- **D-20:** Phase 18 requires manual cabinet/RPCS3 request-routing smoke before close. The user will run it manually; implementation should make route-probe logging useful for that evidence.
- **D-21:** Reviews should verify existing Green, Blue, Yellow, Nijiiro, shared `/v01r00`, and AC15 shared-core behavior are unchanged except for covered Red additions. Keep the review lightweight and scoped to touched surfaces.

### the agent's Discretion
- Downstream agents may choose exact Red adapter project names, controller file splits, mapper class names, and route-probe response helpers as long as route ownership, generated-wire ownership, and no-state semantics are preserved.
- Downstream planning may decide whether to put route-probe evidence in one Red evidence document or split IDB findings and capability matrix into separate files, provided both are canonical Phase 18 outputs.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Scope And Requirements
- `.planning/ROADMAP.md` - Phase 18 goal, deliverables, success criteria, and dependency on Phase 17.
- `.planning/REQUIREMENTS.md` - RFND-01, RFND-02, RFND-03, Red out-of-scope boundaries, and v1.3 traceability.
- `.planning/STATE.md` - Current v1.3 Red milestone state and current position.
- `.planning/PROJECT.md` - Evidence hierarchy, Red milestone scope, ChallengeCompe boundary, and AC15 sharing decisions.
- `AGENTS.md` - Repo-local architecture, testing, data, and era-boundary rules.

### Red Evidence Inputs
- `proto/red/taiko.proto` - Red game protocol source evidence, including normal route messages, Tokkun fields, Banacoin-adjacent messages, and ChallengeCompe messages.
- `proto/red/vsinterface.proto` - Red startup/version protocol input.
- `.tools/red/EBOOT.ELF.i64` - Red IDB authority for route prefixes, active root, protocol limits, byte formats, and endpoint suffixes.
- `Host/wwwroot/data/red/data` - Local Red operator data root; active runtime root is `config/ST8100-1` unless later IDB/runtime evidence overturns it.

### AC15 Sharing Direction
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - Approved AC15 core direction: share behavior, not routes/wire/persistence.
- `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md` - Approved capability-composition correction: shared modules must not hide table-only era switches; era handlers/controllers are composition roots.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/README.md` - Historical staged AC15 extraction plan; use as context, not a literal Red implementation plan.
- `.planning/milestones/v1.2-phases/16.2-ac15-shared-core-simplification-and-reuse-cleanup/16.2-CONTEXT.md` - Locked decisions for direct `ITaikoDbContext`, row-shape interfaces, Mapperly, and no shared gameplay tables.
- `.planning/milestones/v1.2-phases/16.1-ac15-mapperly-mapper-rewrite-and-presence-semantics/16.1-CONTEXT.md` - Mapperly and generated-wire presence decisions.
- `.planning/milestones/v1.2-phases/16-yellow-tokkun-and-banacoin-compatibility/16-CONTEXT.md` - Tokkun and Banacoin compatibility precedent and no-cross-write boundaries.

### Current Code Touch Points
- `Domain/Enums/GameEra.cs` - Add `GameEra.Red`.
- `Host/Program.cs` - Enabled-era parsing, adapter DI registration, application-part gating, and missing-content-type protobuf fallback.
- `Host/Configurations/ServerSettings.json` - Add Red enabled settings and Red data path.
- `Host/Host.csproj` - Add Red adapter reference, Red data exclusion/copy rules, and Debug Red data junction.
- `Application/Settings/ServerSettingsOptionsValidationExtensions.cs` - Ensure Red settings validation does not imply unsupported item-shop behavior.
- `Application/Ac15/Ac15EraProfiles.cs` - Do not add Red profile until IDB-backed limits and capability matrix support it.
- `Application/Ac15/` - Existing shared AC15 services and profiles available for later phases, not for Phase 18 gameplay behavior.
- `Adapters.GameProtocol.Green/`, `Adapters.GameProtocol.Blue/`, `Adapters.GameProtocol.Yellow/` - Existing adapter-local route/wire/controller patterns to mirror structurally without copying semantics.
- `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs` and `Adapters.GameProtocol.Shared/Controllers/VerupAuthController.cs` - Shared `/v01r00` startup/version ownership precedent.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Application/Ac15/Ac15FeatureSet.cs`, `Ac15EraProfile.cs`, `Ac15ProtocolLimits.cs`, and `Ac15WirePlacement.cs`: existing capability/profile language. Red should not be added here until Phase 18 proves limits and Phase 19 binds the profile.
- `Application/Ac15` shared services: reusable later for Red only when evidence proves matching behavior. Phase 18 should not call gameplay services from route probes.
- Existing Green/Blue/Yellow adapters: structural precedent for adapter-local generated `Wire/`, controllers, mappers, and DI.
- `Host/Program.cs`: existing enabled-era application-part removal pattern for route gating.
- `Host/Host.csproj`: existing AC15 Debug data junction and operator-data exclusion pattern for Green, Blue, and Yellow.

### Established Patterns
- Controllers deserialize wire DTOs, log/map, call Mediator only for implemented behavior, and map back to adapter-local generated wire responses.
- Shared startup/version routes are owned by `Adapters.GameProtocol.Shared` under `/v01r00` where evidence supports them.
- Generated wire lives in each adapter and is regenerated from `proto/` inputs; dumped proto files are evidence.
- Era state stays physically separate. Phase 18 introduces no Red gameplay tables.
- Unsupported client features stay absent or route-probe-only during evidence capture; route probes are not runtime feature support.
- Meaningful tests protect observable behavior and preservation boundaries, not source text, route inventories, or implementation shape for their own sake.

### Integration Points
- Add a Red adapter project analogous in shape to existing AC15 adapters, then register it from Host only when `GameEra.Red` is enabled.
- Add Red route probes under `/v08r01/chassis/*` for IDB-known game suffixes, while keeping startupauth/verup/verupcomplete on shared `/v01r00`.
- Add Red config and build data handling without implying item shop, medals, gameplay persistence, or a Red `Ac15EraProfile`.
- The first runtime smoke should use route-probe logging to show which Red requests route and which are still missing.

</code_context>

<specifics>
## Specific Ideas

- User corrected the route rule: route prefixes are `/vxxryy`, with `xx` from the startup-auth version and `yy` matching the revision/proto.
- User corrected the first IDB sweep: `/v01r00` still exists in the Red IDB for startup/version. Follow-up IDA search confirmed it at `0xDA76A0`.
- User wants the IDA daemon kept running for cache reuse. The Red daemon was started with `IDA_CLI_DAEMON_DIR=C:\Users\10614\.ida-cli\daemons`, and cache refresh indexed 27,485 functions, 43,668 names, and 22,785 strings.
- User will run the Phase 18 runtime smoke manually after implementation.
- "As usual, only add meaningful tests" applies to this phase.
- Do not do a heavy review for Phase 18; keep it scoped to touched preservation surfaces.

</specifics>

<deferred>
## Deferred Ideas

- Red `Ac15EraProfile` binding belongs to Phase 19 after the evidence matrix and IDB-backed limits exist.
- Red runtime identity/userdata/normal/Dani/Tokkun/simple compatibility behavior belongs to Phase 20.
- Shared older-AC15 ChallengeCompe contract and any stateful behavior belong to Phase 21.
- AdminApi/WebUI Red readback and full runtime closeout belong to Phase 22.

</deferred>

---

*Phase: 18-Red Evidence and Capability Foundation*
*Context gathered: 2026-06-13*
