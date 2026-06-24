# Phase 35: KIMIDORI Evidence and Era Foundation - Context

**Gathered:** 2026-06-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Introduce KIMIDORI as a first-class, initially no-state AC15 era only after recording live evidence for startup/version route ownership, game route ownership, direct protobuf transport, route inventory, data layout, and unsupported feature gaps.

</domain>

<decisions>
## Implementation Decisions

### Evidence Boundary
- Use `proto/kimidori`, `.tools/kimidori/EBOOT.ELF.i64`, and linked `Host/wwwroot/data/kimidori/data` as current KIMIDORI evidence.
- Startup/version behavior stays shared under `/v01r00/chassis/*`.
- KIMIDORI game behavior is owned under `/v05r00/chassis/*`.
- Treat direct protobuf as the request/response transport, matching the KIMIDORI proto and older AC15 adapter pattern.

### Route Inclusion
- Include only request families present in `proto/kimidori` and supported by binary route/name evidence.
- IDA inventory confirms `chassis/startupauth.php`, `chassis/verupauth.php`, `chassis/verupcomplete.php`, `chassis/playresult.php`, `chassis/baidcheck.php`, `chassis/userdata.php`, `chassis/defaultsong.php`, `chassis/songhash.php`, and `chassis/mainichisong.php`.
- IDA/proto evidence also confirms request/response types for bookkeeping, telopcheck/gettelop, heartbeat, foldercheck/getfolder, mydonentry, selfbest, recommend, crownsdata, bestscore, communicationlog, and shoppingresult.
- Do not add Taikojuku, Tokkun, Banacoin, battle, Don Challenge, or ChallengeCompe route/state behavior for KIMIDORI 0.12.

### Scaffold Shape
- KIMIDORI should mirror Murasaki where the protocol/data shapes match.
- Controllers must be separate KIMIDORI-owned classes even while scaffolded.
- Generated wire DTOs must be adapter-local under `Adapters.GameProtocol.Kimidori.Wire`.
- The scaffold must be disabled when the KIMIDORI era is not enabled and must not alter existing Blue, Green, Yellow, Red, White, Murasaki, or Nijiiro routes.
- Do not delete copied scaffold files directly. Move unsupported copied files to `.planning/batch-delete/kimidori-scaffold/` for the user to delete manually later. Do not pollute project files with compile exclusions just to avoid deleting copied scaffold leftovers.

### the agent's Discretion
- Use the smallest adapter, DI, Host, settings, route, and test changes needed to prove first-class era registration before runtime state exists.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Adapters.GameProtocol.Murasaki` is the closest adapter pattern for direct-protobuf older AC15 game routes.
- `Adapters.GameProtocol.Shared` already owns the shared protocol controller and protobuf transport helpers.
- `Host/Configurations/ServerSettings.json`, `Host/Program.cs`, and `Host/Host.csproj` are the live Host composition points for enabled eras and adapter projects.

### Established Patterns
- AC15 era behavior uses separated adapter projects and era-specific `GameEra` branches.
- Generated `Wire/` files are committed in adapter projects; `proto/` inputs are not manually edited.
- Era routes are gated by enabled-era application parts.

### Integration Points
- Add `GameEra.Kimidori`.
- Add `Adapters.GameProtocol.Kimidori`.
- Add Host project reference, using, DI registration, settings, data ignore/copy rules, and debug data junction.
- Add route/disabled-era tests focused on KIMIDORI scaffold behavior.

</code_context>

<specifics>
## Specific Ideas

Implement KIMIDORI very similarly to Murasaki, but keep Taikojuku practice-folder support absent. Binary analysis must use `ida-cli` daemon mode when needed.

</specifics>

<deferred>
## Deferred Ideas

Phase 38 manual cabinet/RPCS3 verification remains a stop gate and must not be marked complete by the agent.

</deferred>
