# Phase 28: Murasaki Evidence and Era Foundation - Context

**Gathered:** 2026-06-21
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 28 establishes a first-class, no-runtime-state Murasaki foundation. It records route/root/transport evidence, adds the `GameEra.Murasaki` identity and adapter scaffold, generates adapter-local wire DTOs from `proto/murasaki`, and wires Host/DI/application-part gating so enabled Murasaki routes can be owned by the Murasaki adapter without exposing unsupported behavior.

This phase does not implement catalog loading, userdata state, playresult mutation, AdminApi/WebUI surfaces, or Murasaki special capabilities. Those are owned by phases 29-34.

</domain>

<decisions>
## Implementation Decisions

### Route And Transport Evidence
- Use local IDA-backed evidence from `.tools/murasaki/EBOOT.ELF.i64`; the IDA backend opened successfully on 2026-06-21.
- Route prefix evidence is exact string based: `v06r00` at `0xb88bf8` is the Murasaki game route version, and `v01r00` at `0xb88c00` is the shared startup/version route version.
- Endpoint evidence is `.php` suffix based and should not require slash-prefixed route strings. IDA strings prove `chassis/startupauth.php`, `chassis/verupauth.php`, `chassis/verupcomplete.php`, `chassis/playresult.php`, `chassis/baidcheck.php`, `chassis/mydonentry.php`, `chassis/userdata.php`, `chassis/crownsdata.php`, `chassis/recommend.php`, `chassis/selfbest.php`, `chassis/heartbeat.php`, `chassis/defaultsong.php`, `chassis/bookkeeping.php`, `chassis/telopcheck.php`, `chassis/gettelop.php`, `chassis/foldercheck.php`, `chassis/getfolder.php`, `chassis/mainichisong.php`, and `chassis/taikojuku.php`.
- Preserve direct-protobuf request bodies for Murasaki game endpoints unless later cabinet/client evidence proves otherwise.

### Adapter Foundation
- Add `GameEra.Murasaki` as a first-class era after White without changing existing enum values for supported eras.
- Create an `Adapters.GameProtocol.Murasaki` project with adapter-local `Wire/` DTOs generated from `proto/murasaki`; do not manually edit generated wire cleanup.
- Register the adapter through Host, solution, settings validation, and enabled-era application-part gating following Red/White patterns.
- Keep controllers thin: deserialize/map, call Mediator where behavior exists, and map back. Business behavior belongs in `Application/Handlers`.

### Controller Shape
- Controllers need to be in separate files in every stage. Do not combine Murasaki endpoint groups into one large controller while adding route scaffolding.
- For Phase 28, only no-state route scaffolding is allowed. Any response must be a conservative compatibility response or a clear no-runtime-state bridge to already supported shared startup/version behavior.
- Do not add a White-style `initialdatacheck.php` Murasaki controller. Murasaki initial-data behavior is split across route families and is planned for Phase 30.

### Data Root Evidence
- Local Murasaki data is under `Host/wwwroot/data/murasaki/data`.
- Current config roots present are `ST5100-1`, `ST5100-7`, and `ST6100-1`, each with `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, and `spacialbaid.xml`.
- Phase 28 may record these roots, but it must not lock catalog behavior or active-root selection beyond foundation evidence. Catalog binding belongs to Phase 29.

### Scope Guardrails
- Do not implement `bestscore.php`, `songhash.php`, `shoppingresult.php`, challenge arrays, `content_info`, `default_option_setting`, or reserved bytes in Phase 28.
- Do not copy Red `challengecompe.php` or White Don Challenge behavior into Murasaki.
- Do not edit `proto/murasaki` except by explicit user request; generated adapter wire belongs under the Murasaki adapter.
- Keep Blue, Green, Yellow, Red, White, Nijiiro, and shared startup behavior unchanged except for intentional Murasaki enablement plumbing.

### the agent's Discretion
- Implementation can mirror Red/White project, marker, DI, Mapperly-default, and route-prefix patterns where those are structural scaffolding rather than behavioral inference.
- Test coverage should focus on observable enablement/gating/build behavior and disabled-route absence. Avoid low-value route inventory tests unless they protect the enabled-era gating contract.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Adapters.GameProtocol.White` and `Adapters.GameProtocol.Red` provide the closest adapter project patterns for older AC15 direct-protobuf routes.
- `WhiteRoutePrefixes` shows how one adapter can centralize route version constants while keeping route controllers separate.
- `Host/Program.cs` owns enabled-era adapter registration and disabled application-part removal.
- `Domain/Enums/GameEra.cs` is the canonical era identity list.
- `Infrastructure/Settings/ServerSettingsValidator.cs` and related tests cover supported-era validation.

### Established Patterns
- Each era owns its protocol adapter assembly, `Wire/` DTOs, mappers, controllers, marker type, and `DependencyInjection` extension.
- Shared startup/version routes stay in `Adapters.GameProtocol.Shared` under `v01r00`.
- AC15 application behavior dispatches through unsuffixed handler files and era-specific partial files.
- Generated wire DTOs are mapped to `Application/Dtos/Common*` shapes before handler logic.
- Runtime data roots are resolved through `PathHelper` and era path helpers, not hardcoded from handlers.

### Integration Points
- Add Murasaki to `GameEra`, Host project references, solution project list, enabled-era registration, and application-part gating.
- Add adapter-local generated `Wire/Game.cs` and `Wire/VsInterface.cs` from `proto/murasaki`.
- Add route-prefix constants for `v06r00` game routes while relying on shared `v01r00` startup/version routes.
- Add focused tests for era enablement validation, disabled route absence, and non-regression of existing era startup/game route gating.

</code_context>

<specifics>
## Specific Ideas

- User explicitly corrected the evidence search path: use exact `v01r00`, `v06r00`, and `.php` strings; do not waste time searching slash-prefixed non-existing strings.
- User expects phases through 32 to implement existing Murasaki features by assembling existing AC15 capabilities where compatible, before Phase 33 investigates Murasaki-specific or older-version special features.
- User explicitly requires controllers to be in separate files in any stage.

</specifics>

<deferred>
## Deferred Ideas

- Catalog active-root binding, protocol limits, and sidecar data move to Phase 29.
- Split metadata responses such as default song, mainichi song, folder, and telop move to Phase 30.
- Identity/userdata/self-best/crown/favorite/recent read paths move to Phase 31.
- Normal playresult, Dani, reward, present, special-BAID, and Don Point mutation move to Phase 32.
- `bestscore.php`, `songhash.php`, `shoppingresult.php`, challenge arrays, `content_info`, `default_option_setting`, and reserved byte semantics move to Phase 33.

</deferred>
