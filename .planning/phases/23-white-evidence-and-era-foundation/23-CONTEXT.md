# Phase 23: White Evidence and Era Foundation - Context

**Gathered:** 2026-06-17
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 23 proves White route/version/transport/data-root evidence and adds a first-class White protocol adapter foundation. It may add generated White wire DTOs, `GameEra.White`, Host configuration/DI/application-part gating, exact-prefix protobuf fallback, and no-state route scaffolds only where route evidence supports them.

It does not add White catalog/profile binding, AC15 protocol limits, runtime persistence, AdminApi/WebUI readback, collectable data, or reward/ChallengeCompe behavior. Those stay in later White phases.

</domain>

<decisions>
## Implementation Decisions

### Route, Root, And Transport Evidence
- **D-01:** White game routes are expected under `/v07r00/chassis/*`, but Phase 23 must verify `/v07r00` from White IDB route strings before route attributes are finalized.
- **D-02:** White startup/version routes stay under shared `/v01r00/chassis/*`. Treat this as stable older-AC15 behavior; no extra proof is needed unless White evidence contradicts it.
- **D-03:** Treat `Host/wwwroot/data/white/data/config/ST7100-1` as the active White data root from the local game-directory symlink/inventory. Do not require separate IDB root proof for Phase 23.
- **D-04:** Treat White game endpoints as direct-protobuf AC15 requests unless White evidence contradicts this. Runtime HTTP content-type capture is not a Phase 23 blocker.

### Scaffold Breadth
- **D-05:** Determine the concrete White scaffold route set by searching `.php` route strings in `.tools/white/EBOOT.ELF.i64`, then cross-checking those suffixes against `proto/white` request/response pairs. Do not guess route inventory from Red, Yellow, or proto-only presence.
- **D-06:** Phase 23 controllers should be thin no-state scaffolds: deserialize White wire DTOs, log bounded request information, and return safe success/default responses where appropriate. They must not add Mediator handlers, EF writes, catalog reads, profile state, or runtime semantics.
- **D-07:** After `/v07r00` is verified, add White only to the existing exact-prefix missing-content-type protobuf fallback. Do not replace it with a broad generic AC15 fallback.
- **D-08:** Stop Phase 23 at foundation work: generated wire, adapter project/marker/DI, enum/config/Host gating, route scaffolds, fallback scope, and existing-era preservation checks. White catalog/profile/runtime work begins in Phase 24+.

### Unsupported Or Unproven White Surfaces
- **D-09:** Keep this rule simple: if a surface is not supported by White 0.13, ignore it. If behavior already exists as shared behavior or a shared capability, it may be extracted or kept reusable, but White must not be wired to it unless White evidence supports that surface. Do not turn this into a feature-by-feature absence matrix.

### Active Planning Corrections
- **D-10:** Current filesystem evidence shows `.tools/white/EBOOT.ELF.i64` is present and nonzero. Phase 23 should correct active planning notes that still say the White IDB is zero bytes, and record current IDB file-size evidence in the Phase 23 evidence artifact.

### the agent's Discretion
None. The user made the relevant Phase 23 boundaries explicit.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Scope And Repo Rules
- `AGENTS.md` - Repo architecture/testing rules, White milestone notes, Mapperly verification rules, and era-state boundaries.
- `.planning/ROADMAP.md` - Phase 23 goal, deliverables, and success criteria.
- `.planning/REQUIREMENTS.md` - WFND-01, WFND-02, and WFND-03 requirement coverage.
- `.planning/PROJECT.md` - Milestone-level White scope, active constraints, and key decisions.
- `.planning/STATE.md` - Current planning state and stale zero-byte IDB note to correct during Phase 23.

### White Evidence Inputs
- `.planning/research/SUMMARY.md` - Prior White milestone research summary; note that its zero-byte IDB statement is stale in the current checkout.
- `.planning/research/STACK.md` - White stack additions and generated-wire guidance.
- `.planning/research/FEATURES.md` - White feature/surface inventory research.
- `.planning/research/ARCHITECTURE.md` - White architecture and integration guidance.
- `.planning/research/PITFALLS.md` - White implementation risks and anti-patterns.
- `proto/white/taiko.proto` - White game request/response schema input.
- `proto/white/vsinterface.proto` - White startup/version schema input.
- `.tools/white/EBOOT.ELF.i64` - Current White IDB/client evidence source for route strings; current file-size evidence should be recorded in the Phase 23 artifact.
- `Host/wwwroot/data/white/data/config/ST7100-1` - Accepted White active config root from the local game-directory symlink/inventory.

### Existing Foundation Patterns
- `.planning/milestones/v1.2-phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md` - Prior evidence artifact shape for route prefix, shared startup/version ownership, transport notes, and deferred runtime scope.
- `Adapters.GameProtocol.Red/` - Existing older-AC15 first-class adapter, controller, mapper, and generated-wire precedent.
- `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs` - Shared `/v01r00/chassis/startupauth.php` ownership pattern.
- `Host/Program.cs` - Era-enabled adapter registration, disabled-era application-part filtering, and exact-prefix protobuf fallback.
- `Host/Host.csproj` - Era adapter project references, operator-data publish exclusions, and debug data junction pattern.
- `Host/Configurations/ServerSettings.json` - Enabled-era configuration shape.
- `Domain/Enums/GameEra.cs` - Era enum extension point.

### Shared AC15 Architecture
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - Approved shared AC15 extraction direction.
- `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md` - Approved capability-composition direction for older AC15 eras.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction` - Prior shared-core plan artifacts relevant to preserving era-owned routes/wire/state while sharing matching behavior.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Adapters.GameProtocol.Red/` provides the nearest adapter foundation pattern for an older AC15 era.
- `Adapters.GameProtocol.Shared/Controllers/BaseProtocolController.cs` and shared startup/version controllers provide the controller base and shared `/v01r00/chassis/*` ownership pattern.
- `Host/Program.cs` already has enabled-era adapter registration, disabled-era application-part filtering, and exact-prefix content-type fallback logic.
- `.tools/protogen.exe` plus existing generated AC15 wire folders provide the wire-generation path for `proto/white`.

### Established Patterns
- Era routes and generated wire DTOs remain adapter-owned; shared behavior starts only after mapping through common application DTOs.
- Disabled-era route safety is enforced by Host application-part filtering.
- Operator-supplied AC15 data under `Host/wwwroot/data/<era>/data` is excluded from publish and debug-linked by era-specific Host project targets.
- Shared AC15 behavior is capability/profile driven, but Phase 23 should not create a provisional White runtime profile or wire White into runtime capabilities.

### Integration Points
- Add `GameEra.White` without changing existing enum values.
- Add `Adapters.GameProtocol.White` and Host project/solution references.
- Add White Host settings and enabled-era registration/removal logic.
- Add `/v07r00/chassis` to missing-content-type fallback only after route-prefix verification.
- Preserve existing Blue, Green, Yellow, Red, Nijiiro, and shared startup/version behavior with focused checks for touched shared code.

</code_context>

<specifics>
## Specific Ideas

- Expected White game prefix: `/v07r00/chassis/*`.
- Shared startup/version prefix: `/v01r00/chassis/*`.
- Route inventory method: search `.php` strings in `.tools/white/EBOOT.ELF.i64`, then cross-check against `proto/white`.
- Accepted data root: `Host/wwwroot/data/white/data/config/ST7100-1`.
- Current White IDB file-size evidence observed during discussion: `.tools/white/EBOOT.ELF.i64` length `129893515` bytes.

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within Phase 23 scope.

</deferred>

---

*Phase: 23-White Evidence and Era Foundation*
*Context gathered: 2026-06-17*
