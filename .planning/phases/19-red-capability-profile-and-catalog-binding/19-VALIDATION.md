---
phase: 19
slug: red-capability-profile-and-catalog-binding
status: draft
nyquist_compliant: true
created: 2026-06-13
---

# Phase 19 - Validation Strategy

## Test Infrastructure

| Property | Value |
|----------|-------|
| Framework | xUnit on `Tests/Tests.csproj` |
| Focused command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Red|FullyQualifiedName~Ac15|FullyQualifiedName~StartupMovie"` |
| Full command | `dotnet test Tests/Tests.csproj` |
| Build command | `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` |

## Per-Task Verification Map

| Task ID | Requirement | Behavior | Test Type | Automated Command |
|---------|-------------|----------|-----------|-------------------|
| 19-CATALOG | RCAT-01 | Red catalog initializes from active `ST8100-1`, parses music/tuning/Taikojuku, loads optional sidecars, and exposes movie/customization data. | loader behavior | Focused Red catalog tests |
| 19-PROFILE | RCAT-02 | `Ac15EraProfiles.Red` advertises supported Red capabilities and keeps item shop disabled. | service/profile behavior | Focused Red AC15 profile tests |
| 19-ROUTES | RCAT-01, RCAT-02 | Red initial-data/folder/telop/recommend/Taikojuku routes return catalog-backed response fields through shared AC15 services. | handler/protocol behavior | Focused Red protocol/handler tests |
| 19-STARTUP | RCAT-01 | Shared startup movie lookup maps HDD major version 8 to Red when Red is enabled. | handler behavior | Startup movie tests |
| 19-PRESERVE | RCAT-02 | Existing Green/Blue/Yellow shared AC15 behavior remains covered after shared profile/catalog changes. | regression/build | Focused AC15 tests, full tests if shared changes are broad |

## Manual-Only Verifications

None required for Phase 19 close. Cabinet/RPCS3 acceptance for implemented Red runtime flows is deferred to Phase 22; Phase 19 is catalog/profile binding.

## Sign-Off

- [ ] Red catalog/profile behavior is tested through public loader/service/handler outputs.
- [ ] Shared AC15 changes have focused regression coverage.
- [ ] Temp-output Host build passes.
- [ ] No unsupported Red item-shop, medal, battle, WaiWai, payment, ChallengeCompe, AdminApi, or WebUI surfaces were introduced.
