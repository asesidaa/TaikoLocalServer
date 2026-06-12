# Project Research Summary

**Project:** TaikoLocalServer AC15 Era Support
**Domain:** Red AC15 protocol, catalog, persistence, and Red/older Don Challenge support
**Researched:** 2026-06-12
**Confidence:** HIGH for repo architecture and local proto inventory; MEDIUM for exact Red route prefix, runtime config root, reward semantics, and challenge competition behavior until RPCS3/cabinet evidence lands

## Executive Summary

Red should be added as a first-class AC15 era, not as a Yellow clone. The existing stack is sufficient: .NET 10, ASP.NET Core, protobuf-net/protogen, Mapperly, Mediator, EF Core SQLite, MudBlazor, and xUnit all remain the right tools. The main architectural work is adding Red-owned adapter, wire DTOs, routes, catalog, EF tables, mappers, tests, AdminApi/WebUI routing, and capability flags that prevent Red from inheriting unsupported Yellow item-shop, Don/Katsu medal, and WaiWai behavior.

The defining Red-specific surface is Don Challenge / challenge competition. Blue, Green, and Yellow contain challenge competition proto/controller shapes, but the user correction and current code show those newer-era surfaces are inactive compatibility residue, not runtime behavior evidence. Red and older versions are the meaningful challenge scope. Red support must therefore include a dedicated challenge phase backed by Red proto/data/runtime evidence instead of returning empty `Result = 1` style stubs.

The highest risk is losing Red-specific facts by pushing Red through existing Yellow-like paths too early. Red has Don-point/reward fields, `present.xml`, `ChallengeCompe*` messages, `is_challengecompe`, and per-stage challenge id arrays. Those are not Yellow item-shop fields and not Blue battle fields. The roadmap should prove route/version/config-root evidence first, then catalog/profile, normal persistence, Red reward/Tokkun compatibility, challenge competition, AdminApi/WebUI, and final runtime verification.

## Key Findings

### Recommended Stack

No broad stack additions are needed.

**Core technologies:**
- ASP.NET Core / .NET 10: host, route gating, controller application parts, and direct-protobuf routes.
- protobuf-net + repo-local `.tools/protogen.exe`: generated adapter-local Red wire DTOs from `proto/red`.
- Mapperly: mechanical projection between Red wire DTOs and Application common DTOs.
- Mediator.SourceGenerator: existing era-dispatch handler pattern with `.Red.cs` partials.
- EF Core SQLite: Red-owned gameplay tables through direct `ITaikoDbContext` and concrete Red DbSets.
- MudBlazor / Contracts.AdminApi: Red readback only after Red-owned state exists.
- xUnit: behavior and persistence boundary regression tests, not generated-wire/source-shape tests.

**Stack constraints:**
- Keep `proto/red/*.proto` read-only.
- Replace generated Red wire files instead of manually editing them.
- Use `+nullablevaluetype=yes` so proto2 optional primitive presence is preserved.
- Do not add repository-shaped persistence layers.
- Do not add Yellow item-shop or medal settings to Red unless Red evidence appears.

### Expected Features

**Must have:**
- Red first-class era foundation: `GameEra.Red`, Red adapter, route gating, settings, DI, generated wire, and Host integration.
- Red route/transport/startup proof: route prefix, shared startup/version ownership, HDD/version mapping, and active config root must be evidence-backed before broad implementation.
- Red catalog support: selected Red data root, `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, tuning, `present.xml`, and supported sidecars.
- Red identity/userdata/initialdata/self-best/crowns/favorites/recent/Dani/normal play using Red-owned tables.
- Red Don-point/reward progression from Red proto/data evidence, not Yellow shop/medal semantics.
- Red Tokkun compatibility with the Blue/Yellow no-cross-mode contract adapted to Red-owned state.
- Red Banacoin-adjacent compatibility only where runtime flow requires it, with no wallet/payment authority.
- Red Don Challenge / challenge competition using Red `ChallengeCompe*`, playresult challenge arrays, and Red-local challenge evidence.
- Red AdminApi/WebUI readback for implemented Red surfaces.
- Automated verification plus cabinet/RPCS3 smoke before closeout.

**Defer:**
- Older-than-Red reuse until Red challenge semantics are verified.
- Full challenge schedule/global ranking/category semantics until runtime traces prove them.
- Admin editing tools for challenge schedules or reward catalogs.
- Any Red route not proven by Red proto plus runtime/client evidence.

**Exclude:**
- Red WaiWai support.
- Red Blue-style battle support.
- Yellow item-shop / Don-Katsu medal shop support in Red.
- Shared Red/Yellow/Blue gameplay persistence tables.
- Treating Blue/Green/Yellow challenge stubs as evidence of active challenge behavior.

### Architecture Approach

Red should follow the existing layered pattern:

1. Red controller receives direct-protobuf request under the proven Red prefix.
2. Red generated wire DTO maps through adapter-local Mapperly mappers.
3. Application common DTOs and Mediator handlers dispatch to `.Red.cs` partials.
4. Red handlers bind concrete Red DbSets and Red catalog snapshots into shared `Application/Ac15` helpers only where behavior matches.
5. Red-specific reward, Tokkun, and challenge behavior stays at the Red boundary until semantics are proven.
6. Responses map back through Red-specific wire DTOs.

**Major components:**
- `Adapters.GameProtocol.Red`: Red routes, generated wire, controllers, mappers, and adapter DI.
- `Application/Ac15`: shared algorithms and profiles, extended with `Ac15EraProfiles.Red` and Red capability flags.
- `Application/Handlers/*.Red.cs`: Red behavior composition roots and mode/write boundaries.
- `Infrastructure/GameDataCatalog/Red`: Red path helpers, required files, shared AC15 loader wrappers, reward/challenge loaders.
- `Infrastructure/Persistence` and `Domain/Entities`: Red-owned save, score, best, favorite/recent, Dani, Tokkun, reward, and challenge tables.
- `Adapters.AdminApi` / `TaikoWebUI`: Red route/readback support after Red state exists.

### Critical Pitfalls

1. **Treating newer-era challenge surfaces as runtime evidence**: Blue/Green/Yellow challenge proto/controllers are not proof of active behavior. Red challenge must be planned from Red evidence.
2. **Modeling challenge as a boolean**: `is_challengecompe` is only an availability/readback flag; Red needs real challenge catalog/state/readback when challenge support is claimed.
3. **Dropping challenge arrays in normal play**: `Ac15NormalPlayWriter` does not persist challenge metadata. Red playresult must preserve challenge facts before or beside normal writes.
4. **Copying Yellow shop/medal/WaiWai behavior**: Red uses Don-point/reward fields and lacks Yellow item-shop and WaiWai wire surfaces.
5. **Guessing route prefix or config root**: Red has multiple local roots (`ST5100-1`, `ST5100-7`, `ST7100-1`, `ST8100-1`); runtime evidence must choose.
6. **Polluting shared AC15 modules**: Red composition belongs in Red handlers; shared modules should stay table-independent.
7. **Weak tests**: Red tests must prove behavior, persistence, no-cross-era/no-cross-mode boundaries, field omission/packing, and runtime output copy where relevant.

## Implications For Roadmap

### Phase 18: Red Evidence and Era Foundation

**Rationale:** Red route/version/config-root proof is the dependency for every later implementation slice.
**Delivers:** Red evidence matrix, Red adapter project, generated wire DTOs, settings, route gating, startup/version decision, no-state supported route skeletons, and explicit absence guardrails.
**Addresses:** route/transport proof, proto immutability, unsupported WaiWai/battle/shop absence, challenge scope correction.
**Avoids:** guessed prefix/root, Yellow clone adapter, fake challenge support.

### Phase 19: Red Catalog and AC15 Profile Foundation

**Rationale:** Normal play and challenge readback depend on a proven Red catalog and capability model.
**Delivers:** `IRedCatalog`, Red path helpers, required-file checks, selected config root, Red catalog DTOs/loaders, Red sidecar copy rules, `Ac15EraProfiles.Red`, and initial data/metadata readback.
**Addresses:** catalog bootstrap, initial data, folders/telops/recommend/tournament/Dani catalog, Red item-shop absence.
**Avoids:** wrong `ST*` root and Yellow item-shop leakage.

### Phase 20: Red Identity, Userdata, Crowns, Self-Best, and Normal Play

**Rationale:** Red-owned save and normal persistence must exist before special modes or AdminApi readback are meaningful.
**Delivers:** Red EF tables, BAID/mydon/userdata/default save, self-best/crowns, favorites/recent, normal playresult, Don-point/reward field capture, and no-cross-era tests.
**Addresses:** core normal flow and Red-owned persistence.
**Avoids:** shared gameplay tables and normal writer dropping Red-specific fields silently.

### Phase 21: Red Dani, Rewards, Tokkun, and Banacoin Compatibility

**Rationale:** Red has multiple special/runtime-adjacent surfaces that should land after normal state exists but before challenge closeout.
**Delivers:** Red Dani read/write, Red `present.xml` reward progression, Red Tokkun classification/persistence/readback, and stateless Banacoin-adjacent compatibility if runtime calls require it.
**Addresses:** Dani, reward/present progression, Tokkun no-cross-mode behavior, compatibility endpoints.
**Avoids:** Banacoin authority and Yellow medal/shop semantics.

### Phase 22: Red Don Challenge / Challenge Competition

**Rationale:** Challenge competition is the distinctive Red/older behavior and needs its own evidence-backed implementation.
**Delivers:** Red challenge catalog discovery, Red challenge state tables, playresult challenge fact preservation, `challengecompe.php` readback, and runtime-oriented verification.
**Addresses:** `ChallengeCompe*`, `is_challengecompe`, challenge arrays, Red/older-only challenge capability.
**Avoids:** empty newer-era stubs and boolean-only support.

### Phase 23: Red AdminApi, WebUI, Runtime Verification, and Closeout

**Rationale:** Operator surfaces and final cabinet/RPCS3 proof should come after Red runtime behavior is implemented.
**Delivers:** Red AdminApi/WebUI readback for implemented surfaces, focused Red tests, full test suite, temp-output Host build, cabinet/RPCS3 smoke, and final Red contract documentation.
**Addresses:** supportability, verification, closeout.
**Avoids:** UI routing to unsupported or wrong-era state.

### Phase Ordering Rationale

- Foundation must prove route/version/root before catalog paths and controller routes harden.
- Catalog/profile must precede initialdata and behavior that advertises availability.
- Normal Red persistence must precede special-mode and AdminApi readback.
- Challenge competition deserves a separate phase because it is Red/older-specific and not solved by existing newer-era stubs.
- Runtime verification belongs at the end because compatibility requires cabinet/RPCS3 evidence, not server tests alone.

## Research Flags

Phases likely needing deeper research during planning:
- **Phase 18:** route prefix, startup/version ownership, HDD/version mapping, active config root.
- **Phase 19:** Red catalog root selection, `present.xml`, sidecar needs, challenge data discovery.
- **Phase 21:** Red reward mutation semantics, Banacoin-adjacent call sequence, Red Tokkun classification/readback.
- **Phase 22:** Don Challenge route cadence, challenge catalog source, score/state/readback semantics.
- **Phase 23:** runtime smoke checklist and parser-visible response shape checks.

Phases with standard patterns:
- **Phase 20:** Red-owned normal persistence can mostly follow Yellow plus shared AC15 helpers, with Red field and no-cross-era checks.
- **Phase 23 AdminApi/WebUI subset:** routing/readback can follow Yellow once Red state exists.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Existing .NET/ASP.NET/protobuf-net/Mapperly/EF/Mediator stack supports another AC15 era. |
| Features | HIGH for inventory, MEDIUM for runtime semantics | Red proto/data clearly identify surfaces; challenge/reward exact behavior still needs runtime evidence. |
| Architecture | HIGH | Yellow and Phase 16.2 shared-core patterns provide clear integration points. |
| Pitfalls | HIGH | Risks are grounded in repo history, user correction, and existing Blue/Green/Yellow boundaries. |

**Overall confidence:** MEDIUM-HIGH.

## Gaps To Address

- **Route prefix and startup/version mapping:** capture Red request/log/IDA/RPCS3 evidence before locking controllers.
- **Active Red config root:** prove whether the target uses `ST5100-1`, `ST5100-7`, `ST7100-1`, `ST8100-1`, or a configurable root.
- **Reward semantics:** determine how `present.xml`, `reward_ptn`, `reward_progress`, `get_donpoint`, `total_get_donpoint`, and `total_use_donpoint` mutate and read back.
- **Challenge competition semantics:** determine catalog source, response row meaning, playresult write rules, and whether challenge play also mutates normal play state.
- **Tokkun runtime behavior:** prove Red Tokkun classifier/readback and no-cross-mode boundaries rather than blindly copying Yellow.
- **Runtime verification:** define cabinet/RPCS3 smoke gates for normal Red, Tokkun, and Don Challenge.

## Sources

### Primary
- `.planning/research/STACK.md`
- `.planning/research/FEATURES.md`
- `.planning/research/ARCHITECTURE.md`
- `.planning/research/PITFALLS.md`
- `.planning/PROJECT.md`
- `proto/red/taiko.proto`
- `proto/red/vsinterface.proto`
- `Host/wwwroot/data/red/data`

### Secondary
- `Application/Ac15/*`
- `Application/Handlers/GetChallengeCompeQuery*.cs`
- `Application/Handlers/UpdatePlayResultCommand.Yellow*.cs`
- `Adapters.GameProtocol.Yellow`, `Adapters.GameProtocol.Green`, `Adapters.GameProtocol.Blue`
- `Infrastructure/GameDataCatalog/Yellow`
- `Infrastructure/Persistence`
- `Tests/Yellow`

### Tertiary
- Public wiki context for Red dates and Don Challenge scoping. Useful as a lead only; local proto/data/runtime evidence decides implementation.

---
*Research completed: 2026-06-12*
*Ready for roadmap: yes*
