---
phase: 23
slug: 23-white-evidence-and-era-foundation
status: verified
threats_open: 0
asvs_level: 1
created: 2026-06-18
updated: 2026-06-18
---

# Phase 23 - Security

Per-phase security contract: threat register, accepted risks, and audit trail.

## Trust Boundaries

| Boundary | Description | Data Crossing |
|----------|-------------|---------------|
| White cabinet HTTP -> Host routing | Direct protobuf White cabinet requests enter the ASP.NET Core routing and model-binding surface. | Cabinet identifiers, BAID, shop/chassis identifiers, scaffold request fields |
| ServerSettings -> MVC application parts | Local operator era configuration decides which game protocol adapter assemblies expose controllers. | Enabled-era flags |
| Missing content type -> protobuf fallback | Host may assign `application/protobuf` for AC15 cabinet POSTs with missing content type. | HTTP method, path, content type |
| `proto/white` -> generated White wire DTOs | Dumped protocol files feed generated adapter-local protobuf contracts. | Protocol schema |
| Local operator data -> build output | Host project rules decide whether operator White data is copied or debug-linked. | Local game data paths |

## Threat Register

| Threat ID | Category | Component | Disposition | Mitigation | Status |
|-----------|----------|-----------|-------------|------------|--------|
| T-23-01 | Elevation of Privilege | Disabled White adapter routable because Host references assembly | mitigate | `GameProtocolApplicationParts.RemoveDisabledGameProtocolApplicationParts` removes `TaikoLocalServer.Adapters.GameProtocol.White` when `GameEra.White` is disabled; `WhiteHostRouteGatingTests` verifies enabled and disabled exposure through the shared helper. | closed |
| T-23-02 | Spoofing/Tampering | Missing-content-type protobuf fallback | mitigate | `ShouldAssumeProtobufRequest` adds only exact `/v07r00/chassis` White scope after route proof; tests and verification record no broad `/v07r00` fallback. | closed |
| T-23-03 | Information Disclosure | No-state scaffold request logging | mitigate | White scaffold controllers log bounded scalar identifiers and counts only, not full request objects or repeated payloads. | closed |
| T-23-04 | Tampering | `proto/white` and generated White wire | mitigate | Generated wire came from immutable `proto/white` inputs; verification keeps `git status --porcelain -- proto/white` clean. The 2026-06-18 fix did not edit `proto/white` or generated wire files. | closed |
| T-23-SC | Tampering | npm/pip/cargo installs | accept | Phase 23 reused existing .NET/protogen tooling and did not add external package installation. | closed |

## Accepted Risks Log

| Risk ID | Threat Ref | Rationale | Accepted By | Date |
|---------|------------|-----------|-------------|------|
| AR-23-01 | T-23-SC | No package install occurred; existing repository tooling and central package references were reused. | agent | 2026-06-18 |

## Security Audit Trail

| Audit Date | Threats Total | Closed | Open | Run By |
|------------|---------------|--------|------|--------|
| 2026-06-18 | 5 | 5 | 0 | agent |

## Evidence

- `Host/Program.cs` conditionally registers `AddGameProtocolWhite()` only when `GameEra.White` is enabled and scopes the fallback to `/v07r00/chassis`.
- `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs` removes disabled game protocol adapter assemblies, including White.
- `Tests/White/WhiteHostRouteGatingTests.cs` verifies enabled White route exposure and disabled White route absence through the shared helper.
- `Adapters.GameProtocol.White/Controllers/*Controller.cs` contains per-route scaffold controllers that log bounded scalar fields and return no-state success/default responses.
- `git status --porcelain -- proto/white` was clean during Phase 23 verification and the 2026-06-18 setup fix.
- Focused 2026-06-18 regression tests passed 10/10 and the temp-output Host build passed with 0 warnings and 0 errors.

## Sign-Off

- [x] All threats have a disposition (mitigate / accept / transfer).
- [x] Accepted risks documented in Accepted Risks Log.
- [x] `threats_open: 0` confirmed.
- [x] `status: verified` set in frontmatter.

**Approval:** verified 2026-06-18
