# Stack Research

**Domain:** Brownfield MOMOIRO 0.11 AC15 cabinet protocol support in TaikoLocalServer  
**Researched:** 2026-06-25  
**Confidence:** HIGH for stack/tooling decision from repo evidence; MEDIUM for workflow details that depend on later MOMOIRO IDA inspection; LOW for public wiki scoping context

## Executive Recommendation

Do not add a new runtime stack for MOMOIRO. No new runtime stack is expected unless current repo evidence proves otherwise. Implement v1.7 as another first-class AC15 era on the existing ASP.NET Core 10, EF Core SQLite, protobuf-net, Mapperly, Mediator, and MudBlazor stack. The only expected additions are MOMOIRO-owned project artifacts and evidence workflows: adapter project, generated wire DTOs, route controllers, catalog bindings, persistence where proven, AdminApi/WebUI era routing, and focused verification.

Route/tooling scope is fixed for this milestone: shared startup/version routes remain `/v01r00/chassis/*.php`, MOMOIRO game routes are `/v04r00/chassis/*.php`, and all cabinet routes stay `.php`.

MOMOIRO differs from the later completed eras in where evidence must come from, not in the core technology. The milestone should reuse KIMIDORI/Murasaki-era root-level catalog patterns and shared older-AC15 capabilities only after `proto/momoiro`, `.tools/momoiro/EBOOT.ELF.i64`, `Host/wwwroot/data/momoiro/data`, and runtime/cabinet evidence prove the corresponding behavior.

Public wiki pages are useful only for release/context scoping. The wiki confirms MOMOIRO 0.11 as the 2013-12-11 large update and lists broad gameplay changes, but it must not override local proto, data, binary, generated wire, logs, or cabinet evidence.

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| .NET SDK / C# | SDK `10.0.100`, `net10.0` | Host runtime and project target | Already repo-wide via `global.json` and existing projects. Changing target would create milestone risk without solving a MOMOIRO problem. |
| ASP.NET Core MVC | Package family `10.0.7` / framework reference | Cabinet `.php` endpoints and hosted AdminApi/WebUI | Existing era adapters use controllers, shared protocol helpers, and Host application-part gating. MOMOIRO should add `Adapters.GameProtocol.Momoiro` instead of introducing a second web stack. |
| EF Core SQLite | `10.0.7` plus `SQLitePCLRaw.bundle_e_sqlite3 3.0.3` | Local persistence for MOMOIRO-owned state | Existing AC15 eras use era-owned tables over SQLite. MOMOIRO must not write KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, or Nijiiro gameplay state. |
| protobuf-net | Runtime `3.2.56`; ASP.NET package `3.2.52`; local protogen `3.2.52+f4db4afce3` | Direct protobuf request/response transport and generated wire DTOs | `proto/momoiro/taiko.proto` and `proto/momoiro/vsinterface.proto` are proto2-style inputs. Existing AC15 adapters keep generated wire classes under adapter-local `Wire/` folders. |
| Riok.Mapperly | `4.3.1` | Source-generated DTO projection | Repo policy and existing adapters require Mapperly-driven mechanical projection. Use generated-source inspection for nontrivial mappings. |
| Mediator | `3.0.2` | Controller-to-application dispatch | Preserve the existing boundary: controllers deserialize/map, call Mediator, then map back. Runtime behavior belongs in `Application/Handlers`, not controllers. |
| Blazor WebAssembly + MudBlazor | MudBlazor `9.4.0` | Admin UI parity for implemented MOMOIRO state | Extend existing era-routed UI/AdminApi surfaces after backend behavior exists. No UI framework change is warranted. |

### Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `protobuf-net.AspNetCore` | `3.2.52` | Existing protobuf formatter integration | Use for direct-protobuf cabinet endpoints. Do not switch MOMOIRO to JSON or Google.Protobuf. |
| `Riok.Mapperly` | `4.3.1` | Adapter and application projections | Use partial mapper methods, configured helper conversions, and generated `.g.cs` review. |
| `EntityFrameworkCore.Exceptions.Sqlite` | `10.0.0` | SQLite exception normalization | Reuse through existing Infrastructure patterns if MOMOIRO adds unique constraints or new save-state tables. |
| `Serilog.AspNetCore` | `10.0.0` | Runtime and protocol-edge diagnostics | Use for unsupported shape logs, stateless compatibility logs, and evidence capture. |
| xUnit / Microsoft.NET.Test.Sdk | xUnit `2.9.3`, test SDK `17.14.1` | Regression tests for observable behavior | Add tests for evidence-backed parser behavior, persistence transitions, protocol classification, no-cross-era writes, and AdminApi readback. Do not test generated DTO existence. |

### Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| `.tools/protogen.exe` | Generate MOMOIRO `Wire/Game.cs` and `Wire/VsInterface.cs` | Present locally and reports `protogen 3.2.52+f4db4afce3`. Reuse it with `+nullablevaluetype=yes`. |
| Mapperly generated-source emission | Prove mapper implementation | Run `dotnet build ... /p:EmitCompilerGeneratedFiles=true` and inspect emitted `.g.cs` under `obj/Debug/net10.0/generated/.../Riok.Mapperly/...`. |
| Local IDA database | Binary-backed route, limit, and packing evidence | `.tools/momoiro/EBOOT.ELF.i64` is present. Use IDA/ida-cli as research tooling only; it is not a runtime dependency. |
| Root-level data inventory | Prove active catalog shape | `Host/wwwroot/data/momoiro/data` is root-level and contains `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`. Do not assume a later `config/STxxxx-*` root. |
| `dotnet build` / `dotnet test` | Build and behavioral verification | Use full solution builds for source-generator warnings and focused tests for behavior. Use temp Host output when `Host/bin` is locked. |

## Proto Generation Workflow

Use the existing standalone protogen workflow. Do not add `protobuf-net.BuildTools`, a new global tool manifest, Google.Protobuf generation, or handwritten C# wire models for this milestone.

Recommended generation shape:

```powershell
New-Item -ItemType Directory -Force Adapters.GameProtocol.Momoiro\Wire

.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.Momoiro\Wire -Iproto\momoiro --package=TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire +nullablevaluetype=yes taiko.proto
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.Momoiro\Wire -Iproto\momoiro --package=TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire +nullablevaluetype=yes vsinterface.proto
```

Expected tracked convention after generation:

| Proto input | Generated file convention | Verification |
|-------------|---------------------------|--------------|
| `proto/momoiro/taiko.proto` | `Adapters.GameProtocol.Momoiro/Wire/Game.cs` | Header should say `Input: taiko.proto` and namespace `TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire`. |
| `proto/momoiro/vsinterface.proto` | `Adapters.GameProtocol.Momoiro/Wire/VsInterface.cs` | Header should say `Input: vsinterface.proto` and namespace `TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire`. |

If protogen emits filenames based on proto input names, rename the generated files to the repo convention without editing generated contents. The generated field names, tags, `ShouldSerialize*` helpers, and spelling must remain generator output.

Verification commands:

```powershell
.\.tools\protogen.exe --version
git status --porcelain -- proto/momoiro
Select-String -Path Adapters.GameProtocol.Momoiro\Wire\Game.cs -Pattern 'Input: taiko.proto','namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire'
Select-String -Path Adapters.GameProtocol.Momoiro\Wire\VsInterface.cs -Pattern 'Input: vsinterface.proto','namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire'
```

## Mapperly Workflow

MOMOIRO mappers should follow the current adapter defaults:

```csharp
using Riok.Mapperly.Abstractions;

[assembly: MapperDefaults(
    AutoUserMappings = false,
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
```

Use Mapperly for mechanical projection between MOMOIRO wire DTOs and application/common AC15 records. Manual code in mapper files should be limited to narrow helper conversions that Mapperly calls for byte arrays, optional-presence normalization, constants, or field-name mismatches. Do not put mode classification, persistence policy, unlock policy, or catalog decisions in mapper bodies.

Generated-source inspection is mandatory when a MOMOIRO phase adds or changes nontrivial mappers:

```powershell
dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true
```

Inspect the emitted files under the touched project, typically:

```text
Adapters.GameProtocol.Momoiro/obj/Debug/net10.0/generated/.../Riok.Mapperly/.../*.g.cs
Application/obj/Debug/net10.0/generated/.../Riok.Mapperly/.../*.g.cs
```

Acceptance criteria for mapper work:

| Check | Required Result |
|-------|-----------------|
| Build diagnostics | No new Mapperly warnings hidden by incremental builds. Use full solution build if warning behavior looks inconsistent. |
| Generated `.g.cs` | Proves partial mapper methods generate expected assignments and helper calls. |
| Handwritten mapper code | Only helper conversions, not top-level mapping bodies that replace Mapperly. |
| Strict mapping | Large ignore lists are architecture feedback; redesign DTO/capability shape before suppressing warnings. |

## Local IDA And Binary Research Workflow

The current local inventory contains `.tools/momoiro/EBOOT.ELF.i64` only. That is sufficient as a binary-research input, but this research did not inspect IDA semantics and therefore makes no binary-behavior claims.

Use local binary research before implementing these MOMOIRO decisions:

| Topic | Why Binary Evidence Is Required | Minimum Evidence To Record |
|-------|---------------------------------|----------------------------|
| Route inventory | User-provided route prefix is `/v04r00/chassis/*.php`, but supported suffixes must be proven locally. | IDA route string table or xrefs showing each `.php` suffix and whether startup/version remains `/v01r00/chassis/*.php`. |
| Song unlocking | `proto/momoiro` exposes release/hash fields such as `hash_release_song_flg`, `release_song_no`, shopping song arrays, and `song_hash_tbl`; byte widths and semantics are not proven by proto alone. | Parser/writer functions, field offsets, byte-array lengths, and downstream consumer functions for unlock/readback behavior. |
| Crown data | `UserDataResponse` contains `hash_crown_flg`; there is no separate MOMOIRO `crownsdata.php` message in the inspected proto inventory. | Client parse and render/readback consumers for crown byte placement, width, difficulty packing, and absent/default handling. |
| Changed limits | MOMOIRO 0.11 may differ from KIMIDORI/Murasaki limits for favorites, recent songs, self-best rows, challenge arrays, Don Point cap, and related counts. | Binary constants or loops tied to request/response arrays, plus corroborating proto field shape where available. |

Evidence report format for future phases:

```markdown
## MOMOIRO BINARY EVIDENCE

| Route/Field | IDA Function/Address | Proto Field | Width/Limit | Consumer | Confidence | Notes |
|-------------|----------------------|-------------|-------------|----------|------------|-------|
```

Rules:

- Binary/IDA evidence outranks proto names and public wiki context for runtime mechanics.
- Proto presence plus matching binary `.php` route is the gate for supported feature families.
- Unknown byte arrays should stay omitted/null/defaulted until a phase records a safe value contract.
- IDA artifacts stay under `.tools/momoiro` or ignored scratch paths; they are not runtime assets and should not be committed unless explicitly requested.

## Root-Level Catalog Data Workflow

The MOMOIRO data inventory is root-level:

| Required Input | Present In Inventory | Notes |
|----------------|----------------------|-------|
| `musicinfo.xml` | yes | Root file, not under `config/STxxxx-*`. |
| `musicmedleyinfo.xml` | yes | Root file. |
| `defmusic.bin` | yes | Root file. |
| `fumen/tuning.bin` | yes | Present under `fumen`. |
| `config/STxxxx-*` root | not present in top-level inventory | Do not reuse later-era config-root assumptions. |

Top-level inventory also includes `chassisinfo.xml`, `config.xml`, `device.xml`, `don3d`, `font`, `forbidden.xml`, `libsmart`, `lumendata`, `movie`, `nutdata`, `shader`, and `sound`.

Files not found at the root during inventory included `songrelease.bin`, `defaultsong.xml`, `mainichisong.xml`, `folder.xml`, `telop.xml`, and `songhash.bin`. Do not build a runtime dependency on those names unless a later data or binary pass proves the correct source.

Recommended implementation pattern:

| Area | Use | Avoid |
|------|-----|-------|
| Path resolution | `PathHelper`, era data path helpers, and `IGameDataCatalog.For(GameEra.Momoiro)` | Hardcoded `Host/wwwroot/data/momoiro/data` in handlers. |
| Catalog loader | Reuse AC15 root-level parser pieces where formats match KIMIDORI/Murasaki | Assuming White/Red/Yellow `config/STxxxx-*` layout. |
| Missing committed sidecars | Add only if MOMOIRO needs server-authored JSON outside raw operator data | Empty or guessed JSON sidecars without a runtime consumer. |
| Large data assets | Treat as operator/local evidence | Ingesting or committing huge raw assets as part of stack research. |

## Focused Verification Workflow

Use verification that proves the exact thing changed. Avoid route-list and generated-type tests that only confirm source shape.

### Foundation / Wire Phase

```powershell
.\.tools\protogen.exe --version
dotnet build TaikoLocalServer.slnx
dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true
git status --porcelain -- proto/momoiro
```

Required checks:

| Check | Pass Criteria |
|-------|---------------|
| Wire generation | `Game.cs` and `VsInterface.cs` headers reference the MOMOIRO proto inputs and adapter namespace. |
| Proto immutability | `proto/momoiro` remains clean unless the user explicitly permits schema edits. |
| Adapter registration | Disabled MOMOIRO routes are absent; enabled routes appear through Host application-part gating. |
| Mapperly source | Any nontrivial mapper emits expected `.g.cs` source and no new warning is hidden by incremental build state. |

### Catalog Phase

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build" /p:EmitCompilerGeneratedFiles=true
```

Required checks:

| Check | Pass Criteria |
|-------|---------------|
| Data root | Loader reads root-level MOMOIRO `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`. |
| No later-era root assumption | Loader does not require `config/STxxxx-*` for MOMOIRO. |
| Parser behavior | Tests cover observable parsed catalog behavior, not file existence strings. |

### Runtime State Phase

```powershell
dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true
dotnet test Tests/Tests.csproj --no-build
```

Required checks:

| Check | Pass Criteria |
|-------|---------------|
| No cross-era writes | MOMOIRO playresult/userdata flows do not mutate KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, or Nijiiro gameplay tables. |
| Crowns in userdata | Crown readback uses MOMOIRO `hash_crown_flg` only after binary-backed packing/width is documented. |
| Song unlocking | Release/hash state uses binary-backed widths and limits; unknown unlock semantics remain conservative. |
| AdminApi/WebUI | Expose only implemented MOMOIRO-owned state through existing era-routed surfaces. |

### Runtime Acceptance

Final compatibility still needs cabinet/RPCS3 evidence. Automated tests and builds are regression guards, not proof that the MOMOIRO client accepts the protocol.

Record final manual evidence narrowly:

| Flow | Evidence To Capture |
|------|---------------------|
| Startup/version | `/v01r00/chassis/startupauth.php`, `verupauth.php`, and `verupcomplete.php` request/response sequence. |
| Game route smoke | `/v04r00/chassis/*.php` route sequence observed from client logs. |
| Userdata | Profile/readback succeeds and crown/unlock arrays do not crash client parsing. |
| Normal playresult | Upload accepted and only MOMOIRO-owned state mutates. |
| Admin/WebUI | MOMOIRO era routes display/edit only supported surfaces. |

## Alternatives Considered

| Recommended | Alternative | When To Use Alternative |
|-------------|-------------|-------------------------|
| Existing adapter project + generated tracked wire files | `protobuf-net.BuildTools` generated-on-build flow | Use only in a separate tooling-hardening milestone. The repo currently tracks adapter `Wire/` files and already has a local protogen workflow. |
| protobuf-net | Google.Protobuf | Do not use for MOMOIRO. Existing Host and adapter code are protobuf-net based, and migration would affect all eras. |
| Mapperly partial mappers | Handwritten mapping bodies | Use handwritten code only for helper conversions Mapperly calls. If mapping cannot be expressed cleanly, revisit DTO shape before bypassing Mapperly. |
| EF Core SQLite era-owned tables | Shared gameplay tables across older AC15 eras | Share only identity data that is truly shared. Gameplay state must stay MOMOIRO-owned. |
| Existing Blazor/MudBlazor Admin UI | New admin frontend | Not justified; extend existing era-routed surfaces. |
| Local IDA evidence workflow | Public wiki or route-name inference | Wiki is scoping context only. Runtime mechanics require local binary/proto/data/log/cabinet proof. |

## What NOT To Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| New runtime framework, database, or serializer | No repo evidence indicates MOMOIRO needs it; it would widen risk across shipped eras. | Existing ASP.NET Core, SQLite, protobuf-net, Mapperly, Mediator, MudBlazor stack. |
| Editing `proto/momoiro` | User rules require proto edits only with explicit permission; dumped proto is evidence. | Generate wire from current proto and adapt at mapper/application boundaries. |
| Hand-editing generated `Wire/` files | Breaks repeatable generation and hides protocol drift. | Regenerate with protogen and inspect headers/diffs. |
| `protobuf-net.BuildTools` as a milestone change | Official docs describe it as convenient, but repo precedent uses tracked generated wire and local protogen. | Keep the current standalone generation workflow. |
| Runtime dependency on IDA output | IDA is evidence tooling, not application code. | Record binary findings in planning artifacts and implement normal C# code. |
| Wiki-derived feature implementation | Public wiki pages do not prove route, field, byte width, or client parser behavior. | Require `proto/momoiro` plus binary route/client evidence. |
| New feature families | User explicitly scoped no new feature families. | Implement only proto-plus-route-proven MOMOIRO surfaces. |
| Separate `crownsdata.php` assumptions | MOMOIRO proto inventory shows crowns inside `userdata` as `hash_crown_flg`. | Binary-backed `userdata` crown packing/readback. |
| Tests for generated DTO/property existence | Repo testing rules reject generated-shape tests without runtime failure. | Behavioral tests over handlers, persistence, parsers, byte packing, and AdminApi readback. |
| Broad adjacent-era research | Completed eras are patterns, not MOMOIRO proof. | Reuse KIMIDORI/Murasaki/White patterns only after MOMOIRO evidence matches. |

## Version Compatibility

| Package / Tool | Compatible With | Notes |
|----------------|-----------------|-------|
| .NET SDK `10.0.100` | ASP.NET/EF package family `10.0.7` | Verified from `global.json` and `Directory.Packages.props`. |
| `protobuf-net 3.2.56` | local `protogen 3.2.52+f4db4afce3` | Existing AC15 adapters use generated protobuf-net DTOs. Preserve optional primitive presence with `+nullablevaluetype=yes`. |
| `Riok.Mapperly 4.3.1` | `EmitCompilerGeneratedFiles=true` source inspection | Official docs document emitted generated source; repo defaults require strict target mapping. |
| `Mediator.SourceGenerator 3.0.2` | Existing controller/Mediator boundary | Add MOMOIRO handlers through existing request/handler patterns. |
| MudBlazor `9.4.0` | Existing Blazor WASM Admin UI | Extend existing era-routing, not UI framework. |

## Sources

| Source | Confidence | Used For |
|--------|------------|----------|
| `.planning/PROJECT.md` | HIGH local | Active v1.7 MOMOIRO scope, route prefix, root-level data expectation, no-new-feature-family constraint. |
| `.planning/config.json` | HIGH local | GSD workflow and disabled external search-provider config. |
| `proto/momoiro/taiko.proto` | HIGH local | MOMOIRO message inventory, `UserDataResponse.hash_crown_flg`, release/hash fields, absence of standalone `crownsdata.php` proto family. |
| `proto/momoiro/vsinterface.proto` | HIGH local | Shared startup/version message family. |
| `Host/wwwroot/data/momoiro/data` inventory | HIGH local inventory | Root-level catalog layout and required file presence. |
| `.tools/momoiro` inventory | HIGH local inventory for presence only | Confirms `EBOOT.ELF.i64` exists; no binary semantics claimed here. |
| `.tools/protogen.exe --version` | HIGH local command | Confirms `protogen 3.2.52+f4db4afce3`. |
| `Directory.Packages.props`, `global.json` | HIGH local | Current package/tool versions. |
| Existing KIMIDORI/Murasaki/White adapter files and planning summaries | MEDIUM pattern | Generation, adapter, Mapperly, and verification precedent; not MOMOIRO behavior proof. |
| Mapperly generated-source docs | LOW by webfetch classifier; official support source | Supports `dotnet build /p:EmitCompilerGeneratedFiles=true` and generated output path. |
| Mapperly mapper/null-value docs | LOW by webfetch classifier; official support source | Supports checking Mapperly null/strict behavior instead of assuming manual mapping. |
| protobuf-net contract-first docs | LOW by webfetch classifier; official support source | Confirms BuildTools/protogen options; repo precedent still favors local protogen for this milestone. |
| WikiWiki AC15 history and MOMOIRO update pages | LOW context only | Confirms public MOMOIRO 0.11 context and broad gameplay notes; not implementation authority. |

---
*Stack research for: v1.7 MOMOIRO AC15 0.11 Support*  
*Researched: 2026-06-25*
