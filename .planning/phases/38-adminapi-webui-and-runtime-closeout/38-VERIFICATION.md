---
phase: 38-adminapi-webui-and-runtime-closeout
verified: 2026-06-23T16:06:40Z
status: human_needed
score: "automated closeout verified; manual KIMIDORI runtime verification pending"
acceptance: "pending user-observed cabinet/RPCS3 smoke evidence"
overrides_applied: 0
---

# Phase 38 Verification

## Automated Verification

| Check | Command | Result |
| --- | --- | --- |
| Profile capability focused tests | `dotnet test Tests\Tests.csproj --filter FullyQualifiedName~Ac15ProfileCapabilitiesTests` | PASS: 5 tests |
| Profile settings service focused tests | `dotnet test Tests\Tests.csproj --filter FullyQualifiedName~Ac15ProfileSettingsServiceTests` | PASS: 7 tests |
| WebUI era/profile settings focused tests | `dotnet test Tests\Tests.csproj --filter FullyQualifiedName~Ac15ProfileSettingsWebUiTests` | PASS: 7 tests |
| Auth config regression tests | `dotnet test Tests\Tests.csproj --filter FullyQualifiedName~GreenAuthConfigTests` | PASS: 2 tests |
| Full automated suite | `dotnet test Tests\Tests.csproj` | PASS: 868 tests |
| Full solution build | `dotnet build TaikoLocalServer.slnx` | PASS: 0 warnings, 0 errors |
| Host temp-output build with generated source | `dotnet build Host\Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build" /p:EmitCompilerGeneratedFiles=true` | PASS: 0 warnings, 0 errors |
| Vulnerable package audit | `dotnet list TaikoLocalServer.slnx package --vulnerable --include-transitive` | PASS: no vulnerable packages reported |

## Mapperly Generated-Source Inspection

| Mapper | Generated Evidence | Status |
| --- | --- | --- |
| `Adapters.GameProtocol.Kimidori/Mappers/BaidResponseMapper` | `BaidResponseMapper.g.cs` emits KIMIDORI BAID apply methods and omits unsupported `DefaultToneSetting`. | VERIFIED |
| `Adapters.GameProtocol.Kimidori/Mappers/PlayResultMappers` | `PlayResultMappers.g.cs` maps KIMIDORI playresult metadata, profile, normal, Dani, stage, costume, and compe fact DTOs while omitting unsupported selected-folder state. | VERIFIED |
| `Adapters.GameProtocol.Kimidori/Mappers/UserDataMappers` | `UserDataMappers.g.cs` maps implemented user-data song flags, lists, recommendations, profile counters, display settings, mode flags, and reward fields while omitting unsupported Taikojuku/ChallengeCompe readback. | VERIFIED |
| `Adapters.AdminApi` generated output | No KIMIDORI-specific generated Mapperly body was emitted; KIMIDORI AdminApi work is controller/service branching over existing DTO contracts. | VERIFIED |

## Human Verification

status: pending

The user still needs to run KIMIDORI cabinet/RPCS3 smoke flows against the server and accept the runtime behavior. Minimum smoke scope:

- Startup/version flow uses shared `/v01r00/chassis/*` and KIMIDORI game requests use `/v05r00/chassis/*`.
- Profile/login/mydon/userdata readback works for a KIMIDORI card.
- Normal playresult updates KIMIDORI-owned score, crown, recent/favorite, Don Point, reward/unlock, and profile counters acceptably.
- Dani Dojo result/readback works without exposing Taikojuku practice-folder settings.
- WebUI shows KIMIDORI as an AC15 era and does not show Taikojuku, Tokkun, Banacoin, battle, Don Challenge, ChallengeCompe, or full shop-authority controls.

## Closeout Decision

Phase 38 is not complete. Automated implementation is verified, but milestone closeout is blocked on user-observed KIMIDORI runtime acceptance.

