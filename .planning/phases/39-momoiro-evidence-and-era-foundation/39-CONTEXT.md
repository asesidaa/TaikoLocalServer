# Phase 39: MOMOIRO Evidence and Era Foundation - Context

**Gathered:** 2026-06-26
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 39 establishes MOMOIRO as a first-class, evidence-gated era foundation only. It records the binary route inventory and wires the adapter/era scaffolding so enabled MOMOIRO exposes `/v04r00/chassis/*.php` game routes and disabled MOMOIRO exposes no MOMOIRO game routes. Runtime catalog loading, userdata/readback mutation, playresult persistence, and AdminApi/WebUI behavior belong to later phases.

</domain>

<decisions>
## Implementation Decisions

### Route and Evidence Scope
- Use the supplied MOMOIRO binary route inventory as the active game surface: `/v04r00/chassis/playresult.php`, `/baidcheck.php`, `/mydonentry.php`, `/userdata.php`, `/recommend.php`, `/selfbest.php`, `/heartbeat.php`, `/defaultsong.php`, `/bookkeeping.php`, `/songhash.php`, `/telopcheck.php`, and `/gettelop.php`.
- Preserve shared AC15 startup/version behavior under `/v01r00/chassis/startupauth.php`, `/verupauth.php`, and `/verupcomplete.php`; do not copy these into the MOMOIRO adapter.
- Keep proto-only route families absent when the binary route inventory does not include them, including `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, and `mainichisong.php`.
- Treat `.tools/momoiro/` and `proto/momoiro` as local evidence inputs; repo implementation alone is not proof of MOMOIRO semantics.

### Foundation Wiring
- Add MOMOIRO as a first-class `GameEra` and adapter registration following KIMIDORI/Murasaki patterns, with era-owned generated wire DTOs and route controllers under `Adapters.GameProtocol.Momoiro`.
- Use direct protobuf request/response transport for game endpoints unless current MOMOIRO client evidence proves otherwise.
- Integrate MOMOIRO with Host settings, DI, application-part gating, and startup validation without changing other era behavior.
- Keep runtime endpoints conservative in this phase: controllers may map to existing no-state scaffolding only where the route is binary-proven, but no stateful runtime behavior should be claimed here.

### State and Scope Boundaries
- Do not introduce MOMOIRO gameplay persistence in Phase 39 except if a build requires inert type references; state tables and migrations belong to Phase 41/42.
- Do not wire AdminApi/WebUI MOMOIRO surfaces in Phase 39; those belong to Phase 43.
- Do not infer MOMOIRO Taikojuku, Tokkun, Banacoin, battle, Don Challenge, ChallengeCompe, event-folder, gacha, tournament, or item-shop authority from later AC15 eras.
- Keep the design simple: reuse existing older-AC15 composition patterns and only add MOMOIRO-specific seams where route prefix, generated wire, or evidence gates require them.

### the agent's Discretion
- The agent may choose the closest existing era pattern, expected to be KIMIDORI for root-era scaffolding and Murasaki/KIMIDORI for older AC15 route/controller organization, after checking live code.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Domain/Enums/GameEra.cs` owns era identity.
- `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs` removes disabled adapter assemblies and is the right gating point for MOMOIRO.
- `Adapters.GameProtocol.Kimidori/` and `Adapters.GameProtocol.Murasaki/` provide the closest adapter skeletons for older AC15 eras.
- `Host/Program.cs` and each adapter `DependencyInjection.cs` compose enabled game protocol adapters.
- `proto/momoiro` is the schema source; generated wire belongs under a MOMOIRO adapter `Wire/` folder and must not be hand-cleaned after generation.

### Established Patterns
- Era behavior uses unsuffixed dispatchers with `.Era.cs` partial implementations in `Application/Handlers`.
- Controllers deserialize/map/send Mediator/map response; business behavior belongs in `Application/Handlers`.
- Shared AC15 behavior lives in `Application/Ac15/` only where semantics are value-identical and era-owned tables/profiles are still explicit.
- Host application-part gating removes disabled adapter assemblies rather than adding runtime checks inside every controller.

### Integration Points
- Add MOMOIRO to Host era settings and adapter registration only when enabled.
- Keep shared `/v01r00` startup/version routes in `Adapters.GameProtocol.Shared`.
- Add MOMOIRO route inventory evidence under planning/docs so future phases can distinguish binary-proven routes from proto-only message families.
- Tests should cover observable enabled/disabled route ownership or adapter gating behavior, not source-string route inventory.

</code_context>

<specifics>
## Specific Ideas

User direction for this run: implement MOMOIRO support through Phase 43 without complicating the design; most core logic should already be present, but proper wiring, limits, and data are required. Unlock and crown representation questions must be researched from the MOMOIRO binary before stateful behavior relies on them.

</specifics>

<deferred>
## Deferred Ideas

- Phase 40: root-level catalog binding, limits, route behavior, song hash, default song, telop, recommendation, and crown placement research.
- Phase 41: MOMOIRO-owned userdata, self-best, favorites/recent, release, hash, and crown readback.
- Phase 42: MOMOIRO-owned normal playresult mutation, unlocks, rewards, Dan compatibility, and no-cross-era writes.
- Phase 43: AdminApi/WebUI MOMOIRO routing and supported-control filtering.
- Phase 44: full automated verification and cabinet/RPCS3 acceptance.

</deferred>
