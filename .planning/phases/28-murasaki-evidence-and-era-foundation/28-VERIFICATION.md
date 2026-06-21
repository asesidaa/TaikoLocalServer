---
phase: 28-murasaki-evidence-and-era-foundation
verified: 2026-06-21
status: passed
score: "4/4 must-haves verified"
---

# Phase 28 Verification

| Check | Result |
|-------|--------|
| Evidence record identifies `v01r00`, `v06r00`, `.php` endpoints, direct-protobuf transport, active-root decision, absent `initialdatacheck.php`, and Phase 33 gaps | Passed |
| `GameEra.Murasaki`, generated wire, adapter project, settings, Host/DI, `/v06r00/chassis` ownership, and shared `/v01r00/chassis` boundary exist | Passed |
| Enabled-era gating keeps Murasaki adapter removable when disabled and does not change existing era startup/game registration | Passed |
| `proto/murasaki` remains unchanged | Passed |

## Commands

- `dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true` - passed
- `dotnet test Tests/Tests.csproj --no-build` - passed, 845 tests

## Notes

The solution build emitted Mapperly generated source under `obj/Debug/net10.0/generated/Riok.Mapperly/Riok.Mapperly.MapperGenerator`.
