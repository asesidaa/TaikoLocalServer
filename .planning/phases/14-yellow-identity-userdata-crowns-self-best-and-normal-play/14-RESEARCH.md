# Phase 14: Yellow Identity, Userdata, Crowns, Self-Best, and Normal Play - Research

**Researched:** 2026-06-08
**Domain:** ASP.NET Core AC15 identity, Yellow-owned EF persistence, protobuf route mapping, normal play, self-best, and crown byte encoding
**Confidence:** HIGH for repo/proto/code patterns; MEDIUM for crown transport until Phase 14 byte tests prove raw-vs-gzip behavior

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- D-01: Implement Yellow BAID and mydon flows as first-class Yellow routes under `/v09r00/chassis/*`, replacing the Phase 12 no-state `baidcheck.php` and `mydonentry.php` scaffolds with Mediator-backed behavior.
- D-02: Reuse shared card/access-code identity only as shared identity state; create and read Yellow gameplay/profile state through Yellow-owned save tables, entities, migrations, `ITaikoDbContext` partials, and `GetOrCreateYellowSaveDataAsync`-style helpers.
- D-03: Default Yellow save state should mirror proven AC15 defaults from Blue/Green only where Yellow proto fields support them: mydon name, title/titleplate, colors/costumes, costume flags, tone/title flags, auto-costume flag, display/default settings, medal counters, tutorial flags, and last-play timestamps.
- D-04: A new card response may follow the existing Green/Blue `PlayerType = 1` create path, but existing-card responses must map full Yellow profile/default-save state into Yellow `BAIDResponse` and `MydonEntryResponse` fields, not Green/Blue wire DTOs.
- D-05: Replace Yellow `userdata.php` no-state scaffold with a Yellow `UserDataQuery` partial and Yellow adapter mapper that compose `UserDataResponse` from Yellow-owned save state, Yellow catalog snapshot, favorites, recent songs, and unlock flags.
- D-06: Use `Ac15UserDataService` and `Ac15UserDataSnapshot` for shared profile/readback composition where fields match, with a Yellow adapter/snapshot that supplies Yellow save flags, catalog release rows, locked shop/catalog rows, favorites, recent songs, recommendation rows, counters, display Dan fallback, and tutorial flags.
- D-07: Read back only Yellow-supported Phase 14 fields now: profile/settings, release song flags, tone/title flags, option/default settings, favorites, recent songs, category/play counters, recommendation fields, challenge/tojiru flags, `difficulty_played_course`, `difficulty_played_star`, and currently stored tutorial flags.
- D-08: `tokkun_tutorial_flg` exists in Yellow proto but Phase 16 owns Tokkun persistence/readback. Phase 14 may include a nullable Yellow save column or response mapping only if needed for schema shape, but it must not classify or persist Tokkun playresults.
- D-09: Fields whose semantics belong to Phase 15/16, including Dani clear state, item-purchase/shop unlock state, WaiWai tutorial semantics beyond normal play logging, and Tokkun history, should remain absent, defaulted, or explicitly deferred unless Phase 14 normal-play requirements need a protocol-safe placeholder.
- D-10: Replace Yellow `playresult.php` no-state scaffold with a Yellow playresult mapper, `UpdatePlayResultCommand.Yellow` handler, and Yellow normal-play persistence adapter.
- D-11: Normal Yellow playresults should persist Yellow-owned play history rows, best-score rows, profile counters, unlock bitsets, favorites, and recent songs through Yellow tables only. Add no writes to Blue, Green, Nijiiro, battle, Tokkun, or shop-purchase tables.
- D-12: Use `Ac15NormalPlayService`, `IAc15NormalPlayPersistence`, `DefaultAc15EraHooks`, and `Ac15EraProfiles.Yellow` for the core save loop if Yellow stage modes/course limits match the shared AC15 profile. Yellow-specific validation belongs in a Yellow handler/adapter, not in Blue/Green code.
- D-13: Treat `play_mode` and `stage_mode` conservatively: Phase 14 handles normal play only. Tokkun-shaped uploads must not be consumed as normal score/crown writes; if the mapper can detect Tokkun evidence, leave classification/no-write behavior for Phase 16 and add guard tests that Phase 14 does not invent Tokkun writes.
- D-14: Normal playresult fields for Don/Katsu medal totals can be captured in Yellow-owned save state when they are direct upload counters, but item purchase, spending, shop season state, and duplicate-purchase behavior remain Phase 15.
- D-15: Preserve raw Yellow song order/duplicates only where a Phase 14 normal-history table naturally records each stage row. Do not use Tokkun-style append-only history semantics for normal play.
- D-16: Replace Yellow `selfbest.php` no-state scaffold with a Mediator-backed query that returns Yellow-owned best rows for requested songs and difficulty through a Yellow mapper.
- D-17: Use `Ac15SelfBestService` for response row construction, but verify Yellow wire mapping explicitly because Yellow `SelfBestData` includes `self_best_score` and `ura_best_score` in each repeated row plus a separate `ary_shin_selfbest_score` collection.
- D-18: Replace Yellow `crownsdata.php` no-state scaffold with a Yellow crown readback route that uses Yellow best rows, Yellow catalog song numbers, `Ac15CrownService`, and `Ac15EraProfiles.Yellow.Limits`.
- D-19: YCRN-01 requires explicit proof of both shared crown packing and exact Yellow response placement/encoding. Planning must include a raw protobuf/byte-level test for `CrownsDataResponse.hash_crown_flg` and an explicit compression-vs-raw assertion instead of assuming Blue/Green gzip behavior.
- D-20: If Yellow crown transport is not proven by existing local proto/wire tests alone, downstream research must inspect local Yellow runtime/client evidence before choosing gzip or raw `hash_crown_flg`. Do not silently copy Blue/Green `GZipBytesUtil.GetGZipBytes` behavior.

### the agent's Discretion
- The executor may choose the smallest Yellow persistence/entity surface that satisfies YUSR-01, YUSR-02, YPLY-01, YPLY-02, and YCRN-01, but table names, handlers, route mappers, and tests must stay auditable as Yellow-owned.
- Additional shared AC15 helpers are allowed only when they reduce real duplication and preserve Yellow-owned routes, wire DTOs, and persistence adapters.

### Deferred Ideas (OUT OF SCOPE)
- Yellow Dani persistence/readback, item purchase/shop season state, medal spend/use accounting, WaiWai semantics beyond protocol-safe normal logging, Tokkun classification/history/readback, Banacoin compatibility, AdminApi/WebUI readback, cabinet/RPCS3 smoke, and any Yellow battle behavior.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| YUSR-01 | Yellow cabinet can register or find a card, obtain BAID/profile data, and create default Yellow-owned save state without writing Blue, Green, or Nijiiro save tables. | `BaidQuery` and `AddMyDonEntryCommand` already use shared `Cards`/`UserData` identity with era partials. Yellow needs dispatch branches, Yellow partials, `UserSaveDataYellow`, context partials, and mapper/controller replacements. |
| YUSR-02 | Yellow cabinet can read userdata with profile fields, settings, unlock flags, favorite songs, recent songs, tutorial flags, and supported Yellow readback fields from Yellow-owned state. | `Ac15UserDataService` is already shared. Yellow needs a `YellowAc15UserDataAdapter`, Yellow save/favorite/recent tables, Yellow mapper, and route source tests. |
| YPLY-01 | Yellow normal playresult uploads persist Yellow-owned play history, best scores, profile counters, unlocks, favorites, and recent songs without touching Blue, Green, or Nijiiro gameplay state. | `Ac15NormalPlayService` and `IAc15NormalPlayPersistence` are reusable. Yellow needs `YellowAc15NormalPlayAdapter`, `UpdatePlayResultCommand.Yellow`, Yellow playresult mapper, and no-cross-era-write tests. |
| YPLY-02 | Yellow self-best requests return Yellow-owned best score rows for requested songs and difficulties with correct normal/Ura/Shin support. | `Ac15SelfBestService` builds common responses; Yellow generated wire has `ary_selfbest_score` and `ary_shin_selfbest_score`. Add Yellow handler, mapper, route, and protocol tests. |
| YCRN-01 | Yellow crown readback uses proven Yellow crown placement and response encoding, including explicit compression/raw-byte test. | `Ac15CrownService` builds inflated body; Yellow generated wire places `hash_crown_flg` at field 3. The plan must prove raw-vs-gzip transport with protobuf byte assertions before choosing route output. |
</phase_requirements>

<research_summary>
## Summary

Phase 14 is the first stateful Yellow gameplay slice. The correct foundation is not a broad AC15 table or Green/Blue state reuse; it is Yellow-owned EF entities and context partials for save data, best rows, play rows, favorites, and recent songs, while shared `Cards`, `Credentials`, and `UserData` remain shared identity. Existing Blue/Green identity and normal-play code provide close analogs, but Yellow must use Yellow wire DTOs and Yellow table names.

The repo already has strong shared AC15 services for userdata, normal play, self-best, protocol byte packing, and crowns. These are the right reuse seams, but only behind Yellow adapters: `YellowAc15UserDataAdapter`, `YellowAc15NormalPlayAdapter`, Yellow handler partials, and Yellow mapper classes. Current Yellow controller actions for `baidcheck.php`, `mydonentry.php`, `userdata.php`, `playresult.php`, `selfbest.php`, and `crownsdata.php` are no-state scaffolds in `YellowScaffoldControllers.cs`; Phase 14 should replace only those six route surfaces.

The crown requirement is the highest-risk contract. Blue/Green gzip their inflated crown bytes before assigning `HashCrownFlg`, but Yellow only proves field placement from local proto/wire today. Phase 14 must add explicit tests that serialize Yellow `CrownsDataResponse`, locate field 3 (`hash_crown_flg`), and assert whether the bytes are raw inflated crown bytes or gzip-wrapped bytes. If local wire/protobuf tests cannot prove the transport choice, the implementation should perform local Yellow runtime/client evidence inspection before finalizing the route behavior.

**Primary recommendation:** Plan three waves: Yellow persistence/default identity, Yellow userdata plus self-best/crown readback proof, then Yellow normal playresult persistence and no-cross-era-write tests.
</research_summary>

<architectural_responsibility_map>
## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Yellow default save/profile state | Domain + Infrastructure persistence | Application common helpers | Physical tables and EF mappings must be Yellow-owned; creation helper keeps defaults centralized. |
| BAID/mydon route behavior | Application handlers | Yellow adapter controllers/mappers | Controllers deserialize/map/call Mediator; handlers own shared identity and Yellow save creation. |
| Userdata readback | Application AC15 adapter/service | Yellow adapter mapper | Shared service composes common response; Yellow mapper places fields into Yellow wire DTOs. |
| Normal playresult writes | Application handler + Yellow persistence adapter | Yellow adapter mapper | Common play DTO enters handler; Yellow adapter writes only Yellow tables. |
| Self-best readback | Application handler | Yellow adapter mapper | Handler queries Yellow best rows; mapper proves normal/Ura/Shin wire placement. |
| Crown readback | Yellow adapter route + Application crown service | Tests/protobuf byte proof | Route chooses transport bytes; tests must prove field 3 placement and encoding. |
</architectural_responsibility_map>

<standard_stack>
## Standard Stack

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET / ASP.NET Core | net10.0 | Host, controllers, handlers, EF | Existing solution baseline. |
| EF Core SQLite | repo package set | Yellow-owned persistence and migrations | Existing Blue/Green/Nijiiro persistence stack. |
| Mediator | repo package set | Handler dispatch from controllers | Existing protocol controller pattern. |
| protobuf-net | repo package set | Yellow wire serialization and byte tests | Generated Yellow DTOs are protobuf-net classes. |
| xUnit | repo package set | Handler, mapper, route, EF, and byte tests | Existing test baseline. |

### Reuse Points

| Problem | Use | Notes |
|---------|-----|-------|
| Userdata composition | `Ac15UserDataService` + `Ac15UserDataSnapshot` | Add Yellow adapter rather than building response in controller. |
| Normal play save loop | `Ac15NormalPlayService` + `IAc15NormalPlayPersistence` | Add Yellow adapter and guard non-normal/special modes. |
| Self-best rows | `Ac15SelfBestService` | Yellow handler maps Yellow best rows to `Ac15BestRow`. |
| Crown body | `Ac15CrownService.BuildInflatedBody` | Route transport must be proven per Yellow. |
| Bitsets | `Ac15ProtocolBytes` and `Ac15EraProfiles.Yellow.Limits` | Avoid hardcoded byte counts in handlers/controllers. |
</standard_stack>

<architecture_patterns>
## Architecture Patterns

### System Architecture Diagram

```text
Yellow cabinet request
  /v09r00/chassis/{baid,mydon,userdata,playresult,selfbest,crowns}.php
        |
        v
Yellow controller action in Adapters.GameProtocol.Yellow
  - direct Yellow protobuf request
  - Yellow mapper to Common DTO/query
        |
        v
Mediator handler dispatch with GameEra.Yellow
        |
        +--> Shared identity tables (Cards, UserData, Credentials)
        |
        +--> Yellow-owned EF tables
             UserSaveData_Yellow
             SongBestDatum_Yellow
             SongPlayDatum_Yellow
             YellowFavoriteSongs
             YellowRecentSongs
        |
        v
Shared AC15 services where behavior matches
  Ac15UserDataService
  Ac15NormalPlayService
  Ac15SelfBestService
  Ac15CrownService
        |
        v
Yellow wire response mapper / Yellow crown byte response
```

### Recommended Project Structure

```text
Domain/Entities/
  UserSaveDataYellow.cs
  SongBestDatumYellow.cs
  SongPlayDatumYellow.cs
  YellowFavoriteSongs.cs
  YellowRecentSongs.cs

Application/
  Abstractions/ITaikoDbContext.Yellow.cs
  Common/UserSaveDataYellowExtensions.cs
  Ac15/YellowAc15UserDataAdapter.cs
  Ac15/YellowAc15NormalPlayAdapter.cs
  Handlers/BaidQuery.Yellow.cs
  Handlers/AddMyDonEntryCommand.Yellow.cs
  Handlers/UserDataQuery.Yellow.cs
  Handlers/UpdatePlayResultCommand.Yellow.cs
  Handlers/GetSelfBestQuery.Yellow.cs

Infrastructure/Persistence/
  TaikoDbContext.Yellow.cs
  Migrations/*Yellow*

Adapters.GameProtocol.Yellow/
  Controllers/YellowScaffoldControllers.cs
  Mappers/BaidResponseMapper.cs
  Mappers/UserDataMappers.cs
  Mappers/PlayResultMappers.cs
  Mappers/SelfBestMappers.cs
  Mappers/CrownsDataMappers.cs or controller-local proven bytes

Tests/Yellow/
  YellowIdentityHandlerTests.cs
  YellowUserDataProtocolTests.cs
  YellowPlayResultHandlerTests.cs
  YellowSelfBestTests.cs
  YellowCrownsDataTests.cs
  YellowPersistenceBoundaryTests.cs
```

### Pattern 1: Era-Owned Persistence With Shared Identity

**What:** Add Yellow save/play/best/favorite/recent tables, but continue to use shared card/access-code identity rows.

**When to use:** BAID/mydon, userdata, playresult, self-best, and crowns in Phase 14.

**Analog:** Blue and Green use `UserSaveDataBlue` / `UserSaveDataGreen`, `SongBestDatum_*`, `SongPlayDatum_*`, favorite/recent tables, and `Cards`/`UserData` for shared identity.

### Pattern 2: Common AC15 Service Behind Yellow Adapter

**What:** Convert Yellow state into common `Ac15*` records, call shared service, then map common response to Yellow wire.

**When to use:** Userdata, normal play, self-best, and crowns.

### Pattern 3: Route Replacement Boundary

**What:** Replace only the six Phase 14-owned no-state Yellow routes with Mediator-backed behavior.

**When to use:** `baidcheck.php`, `mydonentry.php`, `userdata.php`, `playresult.php`, `selfbest.php`, and `crownsdata.php`.

### Anti-Patterns to Avoid

- Reusing `UserSaveDataGreen`, `UserSaveDataBlue`, or their score/history tables for Yellow.
- Creating a shared AC15 EF table with an era discriminator.
- Importing Blue/Green generated wire DTOs into Yellow mappers.
- Letting Yellow playresult write Dani, shop purchase, Tokkun, Banacoin, battle, or AdminApi/WebUI state.
- Copying Blue/Green gzip crown route behavior without Yellow field/transport tests.
</architecture_patterns>

<dont_hand_roll>
## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Protobuf response bytes | Manual protobuf writer | protobuf-net serialization in tests and generated Yellow DTOs | Field numbers and optional presence are generated. |
| Userdata flag packing | Controller-local byte loops | `Ac15UserDataService` and `Ac15ProtocolBytes` | Existing service handles catalog release plus locked rows. |
| Normal play stage loop | Yellow-only duplicated algorithm | `Ac15NormalPlayService` | Shared service already persists play rows, best rows, favorites, and recent songs through an adapter. |
| Self-best response construction | Per-route row assembly | `Ac15SelfBestService` | Handles normal/Ura/Shin common response shape. |
| Crown ten-bit packing | New Yellow-only packer | `Ac15CrownService` | Existing helper is tested; Yellow only needs transport proof. |
</dont_hand_roll>

<common_pitfalls>
## Common Pitfalls

### Pitfall 1: Phase 14 Starts Phase 15 Or 16
**What goes wrong:** Yellow playresult begins saving Dani/shop/Tokkun behavior because the fields are present in Yellow proto.
**How to avoid:** Normal play only. Store direct normal counters and normal history rows; leave Dani, purchases, Tokkun history/readback, and Banacoin compatibility deferred.
**Warning signs:** New `YellowTokkun*`, `YellowShop*`, `DanScoreDatumYellow`, `ItemPurchaseCommand.Yellow`, or Banacoin persistence in Phase 14.

### Pitfall 2: Crown Transport Is Assumed
**What goes wrong:** Route blindly gzip-wraps `hash_crown_flg` because Blue/Green do.
**How to avoid:** Add byte-level Yellow response tests and, if still inconclusive, inspect local Yellow runtime/client evidence before choosing raw/gzip.
**Warning signs:** No test serializes `CrownsDataResponse` and asserts field 3 payload bytes.

### Pitfall 3: Yellow State Is Hidden In Shared Tables
**What goes wrong:** Normal Yellow scores or saves use Blue/Green tables or shared EF tables.
**How to avoid:** Add no-cross-era-write tests and source guards for table names and context DbSets.
**Warning signs:** `SongBestDataGreen`, `SongBestDataBlue`, or `UserSaveDataGreen/Blue` appears in Yellow handlers/adapters.
</common_pitfalls>

<validation_architecture>
## Validation Architecture

Phase 14 should validate in four layers:

1. Persistence/schema layer: Yellow EF entities, context partials, migrations, default save helper, and no Blue/Green/Nijiiro writes.
2. Identity/userdata route layer: BAID/mydon/userdata handlers and Yellow wire mapper tests.
3. Score/readback route layer: normal playresult, self-best, and crown route tests using Yellow-owned best/play rows.
4. Boundary/source layer: deferred Phase 15/16 routes remain no-state; no battle route or Blue battle fields appear.

Recommended focused commands:

- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowIdentity|FullyQualifiedName~YellowPersistenceBoundary"`
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowUserData|FullyQualifiedName~YellowSelfBest|FullyQualifiedName~YellowCrownsData"`
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~YellowPlayResult|FullyQualifiedName~YellowPersistenceBoundary"`
- `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"`
- `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build-phase14-yellow"`
</validation_architecture>

<open_questions>
## Open Questions (RESOLVED FOR PLANNING)

1. **Should Phase 14 include a nullable Tokkun tutorial column?**
   - What we know: Yellow userdata and playresult proto expose `tokkun_tutorial_flg`; Phase 16 owns Tokkun semantics.
   - Resolution: The plan may include a nullable schema field only as a safe storage placeholder, but Phase 14 must not write/read Tokkun tutorial state from Tokkun playresults or claim Tokkun readback.

2. **Should Don/Katsu medal counters be persisted?**
   - What we know: Yellow `PlayResultRequest` directly uploads `get_donmedal` and `get_katsumedal`; shop spending belongs to Phase 15.
   - Resolution: Phase 14 can accumulate direct normal playresult medal counters in Yellow save state, but no spend/purchase/shop-season behavior.

3. **Can crown transport be copied from Blue/Green?**
   - What we know: Blue/Green gzip `HashCrownFlg`; Yellow wire proves field 3 placement but not transport.
   - Resolution: No. Plan requires byte-level proof and a raw-vs-gzip assertion.
</open_questions>

<sources>
## Sources

### Primary (HIGH confidence)
- `.planning/phases/14-yellow-identity-userdata-crowns-self-best-and-normal-play/14-CONTEXT.md` - locked Phase 14 decisions and deferrals.
- `.planning/ROADMAP.md` and `.planning/REQUIREMENTS.md` - Phase 14 requirements and success criteria.
- `.planning/phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md` - Yellow `/v09r00` route, direct protobuf, shared startup/version, and no-battle evidence.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-VERIFICATION.md` - Phase 13 catalog-backed metadata routes and deferred runtime-route state.
- `proto/yellow/yellow.proto` and `Adapters.GameProtocol.Yellow/Wire/Game.cs` - Yellow BAID, mydon, userdata, playresult, self-best, and crowns wire fields.
- `Application/Ac15/*` - shared AC15 userdata, normal play, self-best, crown, and byte helpers.
- Blue/Green identity, userdata, playresult, self-best, crown controllers/mappers/handlers/entities/tests.

### Secondary (MEDIUM confidence)
- Blue/Green crown gzip behavior - useful contrast but not authoritative for Yellow transport.
</sources>

<metadata>
## Metadata

**Research scope:** local repo, local proto/wire, Phase 12/13 artifacts, Blue/Green analogs, GSD workflow constraints.
**Confidence breakdown:**
- Standard stack: HIGH - current repo.
- Architecture: HIGH - current Blue/Green/Yellow code and phase artifacts.
- Persistence/entity patterns: HIGH - current EF partials and migrations pattern.
- Crown transport: MEDIUM - placement known; transport must be proven during execution.

**Research date:** 2026-06-08
**Valid until:** 2026-07-08 unless Yellow runtime/client evidence changes.
</metadata>

## RESEARCH COMPLETE

*Phase: 14-yellow-identity-userdata-crowns-self-best-and-normal-play*
*Research completed: 2026-06-08*
*Ready for planning: yes*
