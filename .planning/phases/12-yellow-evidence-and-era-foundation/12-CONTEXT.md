# Phase 12: Yellow Evidence and Era Foundation - Context

**Gathered:** 2026-06-07
**Status:** Ready for planning
**Mode:** Smart discuss infrastructure-only context

<domain>
## Phase Boundary

Phase 12 proves Yellow route/version/transport boundaries and adds first-class Yellow adapter scaffolding. It covers evidence documentation, generated Yellow wire DTO compilation, host/settings/application-part registration, enabled/disabled route tests, and explicit no-Blue-battle guardrails for Yellow.

</domain>

<decisions>
## Implementation Decisions

### Foundation Scope
- Yellow is a first-class era, not a Blue or Green variant.
- Yellow adapter routes, generated wire DTOs, tests, settings, and host registration must be era-owned.
- Shared AC15 startup/version routes may remain under `/v01r00/chassis/*` only where current Yellow evidence supports shared ownership.
- Yellow game routes should use direct protobuf transport where local Yellow proto/client evidence supports it.

### Evidence Boundary
- Local proto, generated wire, route/source tests, local data layout, and later runtime logs outrank public wiki text.
- Phase 12 should record unresolved client-evidence gaps rather than invent route behavior.
- Yellow battle behavior is absent unless concrete Yellow proto/log/client evidence proves otherwise.

### Architecture Constraints
- Keep generated wire models inside a Yellow adapter project; do not create a shared AC15 wire assembly.
- Keep controller logic thin: deserialize/map/call Mediator/map response. Business behavior belongs in Application handlers in later phases.
- Use existing era adapter patterns from Blue/Green and shared protocol controller helpers where behavior really matches.
- Do not create Yellow persistence tables or runtime normal/Tokkun behavior in this phase unless a minimal compile/route scaffold requires a no-state placeholder.

### the agent's Discretion
- Choose the smallest adapter scaffold and test set that proves the Phase 12 success criteria.
- Reuse existing Blue/Green route skeleton and source-guard test patterns where they fit Yellow exactly.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Adapters.GameProtocol.Blue/` and `Adapters.GameProtocol.Green/` show the AC15 adapter project, controller, mapper, generated wire, and dependency-injection patterns.
- `Adapters.GameProtocol.Shared/` owns startup/version controllers and protocol base helpers.
- `Host/Program.cs` gates enabled-era application parts and composes protocol adapters.
- `Domain/Enums/GameEra.cs`, `Application/Settings/ServerSettings.cs`, and `Host/Configurations/ServerSettings.json` are likely era-enablement touch points.
- `Tests/Blue/BlueRouteSkeletonTests.cs`, `Tests/Blue/BlueInfrastructureRegistrationTests.cs`, `Tests/Blue/BlueHostProgramSourceTests.cs`, and similar Green tests are useful route/source guard examples.

### Established Patterns
- Era behavior uses unsuffixed dispatcher files plus `.Blue.cs` / `.Green.cs` partial implementations.
- Generated protobuf DTOs live in adapter `Wire/` folders and should not be hand-cleaned outside generation.
- Route availability is tested both through controller attributes/source guards and host registration/application-part gates.
- AC15 shared core may be used for behavior sharing, but routes, wire DTOs, and persistence stay era-owned.

### Integration Points
- Add Yellow adapter project to the solution and host references.
- Add Yellow DI/application-part registration guarded by server settings.
- Add Yellow protocol route tests under `Tests/Yellow/`.
- Add a Phase 12 evidence artifact under the phase directory or project docs that records route/version/transport conclusions and gaps.

</code_context>

<specifics>
## Specific Ideas

Use `proto/yellow/yellow.proto` and `proto/yellow/vsinterface.proto` as local protocol inputs. Use `Host/wwwroot/data/yellow/data` and observed Yellow config root `config/ST9100-1` as data-layout context, but avoid catalog implementation until Phase 13.

</specifics>

<deferred>
## Deferred Ideas

Yellow catalog loading, profile/userdata, normal play, Dani, shop/medals, WaiWai, Tokkun, Banacoin compatibility, AdminApi/WebUI readback, and runtime RPCS3/cabinet smoke verification are deferred to Phases 13-17.

</deferred>
