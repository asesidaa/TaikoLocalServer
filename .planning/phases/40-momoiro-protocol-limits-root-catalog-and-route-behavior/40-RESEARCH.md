# Phase 40: MOMOIRO Protocol Limits, Root Catalog, and Route Behavior - Research

**Researched:** 2026-06-26  
**Domain:** ASP.NET Core AC15 protocol catalog/profile binding with IDA-backed MOMOIRO route and field evidence  
**Confidence:** MEDIUM

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

#### Scope

- Bind MOMOIRO root-level catalog data from `Host/wwwroot/data/momoiro/data`; do not assume a newer `config/STxxxx-*` layout.
- Required MOMOIRO catalog inputs for this phase are `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- Runtime data roots must resolve through existing settings/path abstractions and era catalog interfaces, not handler-local hardcoded filesystem paths.
- Add an explicit `Ac15EraProfiles.Momoiro` only after researching limits and field placement; do not copy KIMIDORI or Murasaki numeric limits blindly.
- Phase 40 may implement catalog-backed behavior for `recommend.php`, `defaultsong.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php`.
- `heartbeat.php` and `bookkeeping.php` should stay explicit static-result operational stubs unless binary/client evidence proves stateful behavior.
- Do not implement MOMOIRO identity persistence, userdata readback, self-best readback, normal playresult mutation, AdminApi, or WebUI behavior in this phase; those belong to Phases 41-43.
- Do not add MOMOIRO route families absent from Phase 39 evidence, including `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, `mainichisong.php`, standalone `crownsdata.php`, Taikojuku/Tokkun/Banacoin/battle/Don Challenge/ChallengeCompe/event-folder/gacha/tournament/newer item-shop surfaces.

#### Evidence Inputs

- Phase 39 verified the MOMOIRO route foundation, generated wire, enabled/disabled application-part gating, and no-state controller scaffolds.
- Local MOMOIRO data exists under `Host/wwwroot/data/momoiro/data` with root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- `proto/momoiro` is an evidence input for fields and generated DTOs, but proto presence alone is not route or behavior authority.
- `.tools/momoiro/EBOOT.ELF.i64` opens successfully through IDA-CLI idalib. Use this for local binary research where limits or field roles are not already proven.
- Initial IDA-CLI route-table evidence:
  - `sub_17BFD4` registers `baidcheck.php`, `mydonentry.php`, `userdata.php`, `recommend.php`, `selfbest.php`, and `heartbeat.php`.
  - `sub_17CD44` registers `defaultsong.php`, `bookkeeping.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php`.
  - Route strings are stored as 32-bit pointer table entries around `0xACDA78..0xACDA8C` and `0xACDD20..0xACDD30`.
- Protobuf descriptor strings in the binary include `song_hash_ver`, `hash_default_song_flg`, `hash_release_song_flg`, `song_hash_tbl`, `hash_crown_flg`, favorite/recent arrays/counts, reward/Don Point fields, and challenge-shaped arrays; these strings prove field presence but not packing sizes or server semantics.

#### Required Research Before Planning

- Determine the smallest evidence-backed MOMOIRO profile: song flag byte count, crown byte count/song count, favorite/recent limits, default-song flag size, song-hash table encoding, release-song flag size, Don Point/reward limits, and absent-feature flags.
- Research `hash_crown_flg` from binary/client evidence before implementing any crown packing or readback. Crown support remains userdata-owned unless new evidence proves a standalone crown route.
- Research unlock/release representation from binary/client evidence before implementing release-song flags or playresult unlock mutation. Phase 40 can define catalog/readback metadata; Phase 42 owns mutation.
- Compare KIMIDORI/Murasaki implementations only as structural analogs. If numeric limits differ or are unproven, record the gap rather than copying.

### the agent's Discretion

#### Known Existing Patterns To Reuse

- KIMIDORI is the closest root-level catalog layout analog.
- Murasaki/KIMIDORI handlers are useful for `recommend.php`, `defaultsong.php`, `songhash.php`, telop routes, and AC15 readback composition, but MOMOIRO route support must remain restricted to the Phase 39 route inventory.
- Shared AC15 services should be preferred where they already encode catalog snapshots, recommendation selection, song-hash encoding, telop projection, protocol byte helpers, and era profiles.
- Tests should target observable catalog parsing, profile limits/packing, route responses, and no-cross-feature boundaries. Do not test generated wire property existence, controller attributes as source text, project file strings, or implementation-only constants without behavior.

### Deferred Ideas (OUT OF SCOPE)

- Phase 41: MOMOIRO-owned BAID/mydon/userdata/self-best/favorite/recent/crown readback and save-state persistence.
- Phase 42: MOMOIRO normal playresult mutation, score/crown persistence, release/unlock mutation, reward/Don Point mutation, Dan/challenge-compatible state where proven.
- Phase 43: MOMOIRO AdminApi/WebUI routing for implemented state only.
- Phase 44: full automated verification and cabinet/RPCS3 acceptance.
</user_constraints>

## Project Constraints (from AGENTS.md)

- Keep era state separate; MOMOIRO must not share persistence with Blue, Green, Yellow, Red, White, Murasaki, KIMIDORI, or Nijiiro unless it is truly shared identity data. [VERIFIED: AGENTS.md]
- Controllers should deserialize, map, call Mediator where behavior exists, and map back; business behavior belongs in `Application/Handlers`. [VERIFIED: AGENTS.md]
- Use era catalog interfaces and `IGameDataCatalog.For(GameEra)` instead of handler-local filesystem access. [VERIFIED: AGENTS.md]
- Resolve runtime data roots through settings/path helpers, not hardcoded `wwwroot/data/<era>` paths in handlers. [VERIFIED: AGENTS.md]
- Mapperly mappers must stay source-generator driven; verify generated source for Mapperly behavior if mapping behavior is nontrivial. [VERIFIED: AGENTS.md]
- Generated `Wire/` files are not manually cleaned up unless regenerating protocol output. [VERIFIED: AGENTS.md]
- Tests should protect observable behavior such as catalog parsing, persistence boundaries, byte/bit packing owned by this repo, and route responses that drive client/WebUI behavior. [VERIFIED: AGENTS.md]
- Do not test generated protobuf property existence, controller attribute source text, project file strings, enum numeric values, migrations, private methods, or implementation-only constants without a demonstrated runtime failure. [VERIFIED: AGENTS.md]
- The current user request forbids source/proto edits for this research turn and forbids touching the unrelated dirty `Host/.gitignore`. [VERIFIED: current user request]

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MOCAT-01 | MOMOIRO catalog loading supports root-level `Host/wwwroot/data/momoiro/data` with `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`. | Local data probe found all four files in that root-level shape, and KIMIDORI provides the closest root-layout loader pattern. [VERIFIED: local data + repo grep] |
| MOCAT-02 | MOMOIRO data paths are resolved through existing path/settings abstractions instead of handler-local hardcoded filesystem access. | Existing KIMIDORI paths use `PathHelper.GetDataPath(GameEra.Kimidori)` and catalog abstractions; MOMOIRO should copy that responsibility boundary. [VERIFIED: repo grep] |
| MOCAT-03 | MOMOIRO has an explicit AC15 profile/limits model for byte widths, song ordering, favorite/recent limits, default-song flags, song hash, release flags, crown placement, Don Point/reward limits, and absent feature flags, backed by local evidence. | Route/field placement is binary/proto backed; catalog-driven hash sizes are locally computable; several native numeric caps remain inferred and are labeled below. [VERIFIED: IDA idalib + generated wire + local data] |
| MOCAT-04 | MOMOIRO metadata and operational routes expose catalog-backed behavior where data exists or static stubs where route role is static. | `recommend.php`, `defaultsong.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php` match existing shared AC15 catalog services; `heartbeat.php` and `bookkeeping.php` have no stateful evidence and should remain static. [VERIFIED: IDA idalib + repo grep] |
| MOCAT-05 | MOMOIRO crown readback is userdata-owned through `UserDataResponse.hash_crown_flg`; no standalone `crownsdata.php` contract is added unless new evidence proves one. | MOMOIRO generated wire has `UserDataResponse.hash_crown_flg`, and Phase 39 route inventory has no `crownsdata.php`. [VERIFIED: generated wire + Phase 39 evidence] |
</phase_requirements>

## Summary

Phase 40 should be a narrow wiring phase: add MOMOIRO root-level catalog loading, add an explicit MOMOIRO AC15 profile, and replace metadata route scaffolds with shared AC15 catalog-backed behavior where the route is binary-proven. [VERIFIED: 40-CONTEXT.md + repo grep] The planner should not add identity persistence, userdata readback, self-best readback, playresult mutation, AdminApi/WebUI behavior, or proto-only route families in this phase. [VERIFIED: 40-CONTEXT.md + Phase 39 evidence]

The strongest evidence is for route presence, route absence, field placement, root catalog shape, and reusable code ownership. [VERIFIED: IDA idalib + generated wire + local data + repo grep] The weakest evidence is exact native numeric caps for favorites, recent songs, Don Point/reward limits, and the native crown byte constant; the binary descriptor strings confirm fields but not those sizes. [VERIFIED: IDA idalib] Those values can still be planned conservatively by reusing shared AC15 codecs and by keeping mutation deferred to Phase 42. [INFERRED: shared AC15 analog]

**Primary recommendation:** implement MOMOIRO as a KIMIDORI-style root catalog plus shared AC15 metadata routes, with `hash_crown_flg` modeled as userdata-owned, `heartbeat.php` and `bookkeeping.php` static, and all unsupported feature families explicitly absent. [VERIFIED: 40-CONTEXT.md + Phase 39 evidence]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Root catalog discovery and required-file validation | Infrastructure | Application | Filesystem access belongs in era catalog implementations, while Application consumes catalog abstractions. [VERIFIED: AGENTS.md + KIMIDORI catalog code] |
| Song hash/default-song/release/crown packing rules | Application | Adapter | Shared AC15 byte helpers and profile limits should own packing; controllers only map to Momoiro wire DTOs. [VERIFIED: Application/Ac15 code] |
| `recommend.php` behavior | Application | Adapter | Recommendation selection is already centralized in `Ac15RecommendationService`; Momoiro controller should call Mediator and map. [VERIFIED: repo grep] |
| `defaultsong.php` and `songhash.php` | Application | Adapter | Initial-data/default-song composition and song-hash encoding already exist in AC15 services and codecs. [VERIFIED: repo grep] |
| `telopcheck.php` and `gettelop.php` | Application | Adapter | Telop row projection is a shared AC15 catalog readback concern, with wire mapping in the adapter. [VERIFIED: repo grep] |
| `heartbeat.php` and `bookkeeping.php` | Adapter | Application logging only | No stateful native role was proven; explicit static responses are the appropriate compatibility surface. [VERIFIED: IDA idalib + generated wire] |
| MOMOIRO persistence and mutation | Database / Storage | Application | Deferred to Phases 41-42; Phase 40 should not introduce state tables or mutation paths. [VERIFIED: 40-CONTEXT.md] |

## Standard Stack

No new external package should be installed for Phase 40. [VERIFIED: repo grep] Use the existing repo stack:

### Core

| Library / Tool | Version | Purpose | Why Standard |
|----------------|---------|---------|--------------|
| .NET SDK | 10.0.201 | Build/test ASP.NET Core 10 solution | Current machine SDK used by this repo. [VERIFIED: `dotnet --version`] |
| ASP.NET Core | repo target net10.0 | Cabinet HTTP endpoints and Host process | Existing Host and adapters are ASP.NET Core projects. [VERIFIED: repo grep] |
| Mediator.Abstractions / Mediator.SourceGenerator | 3.0.2 | Application request dispatch | Existing handlers use Mediator boundaries. [VERIFIED: Directory.Packages.props] |
| protobuf-net / protobuf-net.AspNetCore | 3.2.56 / 3.2.52 | Direct protobuf game endpoint transport | Momoiro generated wire and Host fallback use protobuf-net. [VERIFIED: Directory.Packages.props + generated wire] |
| Riok.Mapperly | 4.3.1 | Source-generated wire/common DTO mapping | Existing adapter mappers use Mapperly. [VERIFIED: Directory.Packages.props] |
| xUnit / Microsoft.NET.Test.Sdk | 2.9.3 / 17.14.1 | Automated regression tests | Existing `Tests/Tests.csproj` uses xUnit. [VERIFIED: Directory.Packages.props] |

### Supporting

| Library / Tool | Version | Purpose | When to Use |
|----------------|---------|---------|-------------|
| IDA idalib via `ida_cli.agent_bridge.AgentSession` | available, version not reported | Binary-backed route/field/constant inspection | Use only for targeted MOMOIRO binary questions. [VERIFIED: IDA backend probe] |
| PowerShell | Windows shell | Local probes and build/test commands | Existing workspace shell. [VERIFIED: environment context] |
| Git | 2.52.0.windows.1 | Commit planning artifact | Required by GSD doc commit flow. [VERIFIED: `git --version`] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Shared AC15 metadata services | New MOMOIRO-only service layer | Do not use unless MOMOIRO proves divergent behavior; current routes match shared catalog/snapshot patterns. [VERIFIED: repo grep + IDA route inventory] |
| Root-level KIMIDORI-style catalog loader | Later AC15 `config/STxxxx-*` loader | Do not use; MOMOIRO local data and context are root-level. [VERIFIED: local data + 40-CONTEXT.md] |
| Static route scaffolds for all Phase 40 routes | Catalog-backed metadata where data exists | Do not keep all scaffolds static; `recommend`, `defaultsong`, `songhash`, and telop routes have existing shared catalog services. [VERIFIED: repo grep] |

**Installation:**

```bash
# No new packages for Phase 40.
```

## Package Legitimacy Audit

No external packages are introduced by this phase, so the package legitimacy gate is not applicable. [VERIFIED: research scope + repo grep]

## Binary Evidence

IDA backend was probed with `AgentSession.start(path, require_ida=True)` against `.tools/momoiro/EBOOT.ELF.i64`; the backend reported `ida_available=true` and `database_opened=true`. [VERIFIED: IDA backend probe]

### Route Registration

| Address / Function | Evidence | Phase 40 Meaning | Confidence |
|--------------------|----------|------------------|------------|
| `sub_17BFD4` | Registers pointers for `baidcheck.php`, `mydonentry.php`, `userdata.php`, `recommend.php`, `selfbest.php`, and `heartbeat.php`. [VERIFIED: IDA idalib] | `recommend.php` and `heartbeat.php` are binary-backed routes; userdata/selfbest remain Phase 41. | HIGH |
| `0xACDA78..0xACDA8C` | 32-bit route pointer table entries resolve to `chassis/baidcheck.php`, `chassis/mydonentry.php`, `chassis/userdata.php`, `chassis/recommend.php`, `chassis/selfbest.php`, `chassis/heartbeat.php`. [VERIFIED: IDA idalib] | Keep route inventory exact under `/v04r00/chassis`. | HIGH |
| `sub_17CD44` | Registers pointers for `defaultsong.php`, `bookkeeping.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php`. [VERIFIED: IDA idalib] | Metadata route wiring in Phase 40 is in scope. | HIGH |
| `0xACDD20..0xACDD30` | 32-bit route pointer table entries resolve to `chassis/defaultsong.php`, `chassis/bookkeeping.php`, `chassis/songhash.php`, `chassis/telopcheck.php`, `chassis/gettelop.php`. [VERIFIED: IDA idalib] | Do not add routes outside this list from proto alone. | HIGH |

### Descriptor Strings And Caveats

| Descriptor Evidence | What It Proves | What It Does Not Prove |
|---------------------|----------------|------------------------|
| `song_hash_ver`, `hash_default_song_flg`, `song_hash_tbl` were found in binary descriptor strings. [VERIFIED: IDA idalib] | Defaults and song-hash fields are present in the native protocol descriptors. | Native byte count or server-side composition rules. |
| `hash_release_song_flg` was found in binary descriptor strings. [VERIFIED: IDA idalib] | Release-song readback field exists. | Unlock mutation semantics or exact native cap. |
| `hash_crown_flg` was found in binary descriptor strings and generated `UserDataResponse`. [VERIFIED: IDA idalib + generated wire] | Crown readback is carried by `userdata.php`, not by a standalone Momoiro `crownsdata.php` route. | Native crown byte count; current repo codec provides the safe implementation pattern. |
| `ary_favorite_song_no`, `ary_recent_song_no`, `song_favorite_cnt`, and `song_recent_cnt` were found in binary descriptor strings. [VERIFIED: IDA idalib] | Favorite/recent arrays and counts are protocol fields. | Native max counts; no clean constant was tied to these arrays in this pass. |
| `get_donpoint`, `reward_ptn`, `reward_progress`, `total_get_donpoint`, and `total_use_donpoint` exist in proto/generated descriptors. [VERIFIED: generated wire + IDA idalib] | Reward and Don Point fields exist. | Shop route authority, wallet semantics, reward unlock semantics, or exact cap. |

**Disqualified clue:** an inspected `0x18C` immediate near `sub_17E2E0` participates in a card/status bitmask path, not a song-count or crown-count path. [VERIFIED: IDA disassembly] Do not use `0x18C` as MOMOIRO song count evidence. [VERIFIED: IDA disassembly]

## MOMOIRO Limits And Profile Evidence

| Concern | Recommended Phase 40 Value | Evidence | Confidence |
|---------|----------------------------|----------|------------|
| Catalog song ordering | Use `musicinfo.xml` file order, not raw dense song IDs. The local file declares size `380`, parsed `uniqueid` count is `380`, and observed IDs range from `0` to `20028`. [VERIFIED: local data] | Local `musicinfo.xml` signature/version/size plus existing KIMIDORI root loader pattern. [VERIFIED: local data + repo grep] | HIGH |
| Song hash version | `538116869`. [VERIFIED: local data] | `musicinfo.xml` declares `<version>538116869</version>`. [VERIFIED: local data] | HIGH |
| Song hash table | `ushort` table from `musicinfo.xml` unique IDs in file order; encoded response is big-endian 2 bytes per entry, so local table encodes to 760 bytes. [VERIFIED: local data + Ac15SongHashCodec] | Existing `Ac15SongHashCodec.EncodeTable` encodes `ushort` entries big-endian; local count is 380. [VERIFIED: repo grep + local data] | HIGH |
| Medley/Dani catalog rows | `musicmedleyinfo.xml` declares size `15`; use catalog loading only in Phase 40, with Dan/challenge mutation deferred. [VERIFIED: local data + 40-CONTEXT.md] | Root file header declares `TaikoAC15 MusicMedleyInfo`, version `538054930`, size `15`. [VERIFIED: local data] | HIGH for catalog count, MEDIUM for later runtime role |
| Default-song flag response | Compact the default-song inflated bitset through the Momoiro song-hash table; expected local wire length is `ceil(380 / 8) = 48` bytes. [INFERRED: shared AC15 codec math from verified local count] | Proto/generated wire has `hash_default_song_flg`; KIMIDORI already uses `Ac15SongHashCodec.CompactBitset` for this route. [VERIFIED: generated wire + repo grep] | MEDIUM |
| Release-song flag readback | Compact release flags through the Momoiro song-hash table; expected local wire length is `48` bytes for 380 hash entries. [INFERRED: shared AC15 codec math from verified local count] | Proto/generated wire and IDA descriptors expose `hash_release_song_flg`; mutation is out of scope until Phase 42. [VERIFIED: generated wire + IDA idalib + 40-CONTEXT.md] | MEDIUM |
| Internal song flag bytes | Use the existing shared AC15 inflated representation only as an internal safety envelope; current common limits are `128` bytes / 1024 bits. [VERIFIED: repo grep] | Momoiro local `musicinfo.xml` includes medley-range IDs above 1024, so response packing must be driven by file/hash order and shared compaction, not by treating raw `uniqueid` as a dense bit index. [VERIFIED: local data + repo grep] The native binary did not expose a clean `128` constant tied to flags in this pass. [INFERRED: shared AC15 analog] | MEDIUM |
| Crown placement | `Ac15CrownWirePlacement.UserData`; do not create `crownsdata.php`. [VERIFIED: generated wire + Phase 39 evidence] | `UserDataResponse.hash_crown_flg` exists and Phase 39 did not find a Momoiro crown route. [VERIFIED: generated wire + Phase 39 evidence] | HIGH |
| Crown wire packing | Use existing AC15 10-bit-per-song crown values compacted through Momoiro song-hash order; expected local compacted wire length is `ceil(380 * 10 / 8) = 475` bytes. [INFERRED: shared AC15 codec math from verified local count] | Existing repo crown helper uses 10 bits per song across 5 difficulties and `Ac15SongHashCodec.CompactTenBitValues` already exists. [VERIFIED: repo grep] Native byte constant was not directly proven. [VERIFIED: IDA disassembly caveat] | MEDIUM-LOW |
| Favorite songs | Use a conservative `MaxFavoriteSongs = 5` unless the planner decides to require another native proof checkpoint before implementation. [ASSUMED] | Binary/proto prove arrays/count fields; this pass did not tie a native numeric constant to the array. [VERIFIED: IDA idalib] Older-era shrinkage is a conservative inference, not proof. [ASSUMED] | LOW |
| Recent songs | Use `MaxRecentSongs = 5`. [INFERRED: shared AC15 analog] | Shared AC15 tests and profiles use recent limit 5; binary/proto prove arrays/count fields but not the native numeric constant. [VERIFIED: repo grep + IDA idalib] | MEDIUM |
| Don Point/reward fields | Record field presence only in Phase 40; do not implement mutation or reward authority yet. [VERIFIED: generated wire + 40-CONTEXT.md] | Fields exist, but `shoppingresult.php` is absent and mutation belongs to Phase 42. [VERIFIED: Phase 39 evidence + 40-CONTEXT.md] | HIGH for deferral, LOW for exact cap |
| Initial data route | Feature flag absent for `initialdatacheck.php`. [VERIFIED: Phase 39 evidence] | No binary-backed Momoiro `initialdatacheck.php` route was found. [VERIFIED: Phase 39 evidence] | HIGH |
| Folders/event folders | Feature flag absent for folder routes in Phase 40. [VERIFIED: Phase 39 evidence] | No `foldercheck.php` or `getfolder.php` route is in the inventory; do not invent event-folder behavior. [VERIFIED: Phase 39 evidence] | HIGH |
| Taikojuku/Tokkun/Banacoin/battle/Don Challenge/ChallengeCompe/gacha/tournament/newer item shop | Feature flags absent. [VERIFIED: 40-CONTEXT.md + Phase 39 evidence] | No matching route family plus proto/wire authority exists for Phase 40. [VERIFIED: 40-CONTEXT.md + Phase 39 evidence] | HIGH |

## Recommended MOMOIRO Profile Shape

Use a dedicated `Ac15EraProfiles.Momoiro` entry rather than reusing `Kimidori`. [VERIFIED: 40-CONTEXT.md] The smallest defensible profile is:

```csharp
// Source: Application/Ac15 patterns + Phase 40 IDA/proto/local-data evidence.
public static Ac15EraProfile Momoiro { get; } = new(
    GameEra.Momoiro,
    MomoiroFeatures,
    CreateMomoiroLimits(),
    new Ac15WirePlacement(
        CrownPlacement: Ac15CrownWirePlacement.UserData,
        HasInitialDataItemShopRows: false,
        HasInitialDataLegalTermsRows: false,
        HasTokkunTutorialFlagInUserData: false));
```

`MomoiroFeatures` should enable normal AC15 metadata support needed by binary-backed routes, while disabling unsupported surfaces: initial-data route, folders, Taikojuku, item shop authority, Tokkun, Banacoin, battle, Don Challenge, ChallengeCompe, gacha, tournament, and newer shop behavior. [VERIFIED: Phase 39 evidence + 40-CONTEXT.md]

For `CreateMomoiroLimits()`, use verified catalog-derived hash table size at runtime and keep mutation-sensitive caps isolated in the profile so Phase 42 can adjust them without changing route contracts. [INFERRED: shared AC15 analog] The favorite limit is the only Phase 40 profile value that remains low-confidence; use `5` as a conservative cap or insert a human checkpoint before locking it. [ASSUMED]

## Architecture Patterns

### System Architecture Diagram

```text
Host startup/settings
  -> registered Momoiro adapter and root data path
  -> Infrastructure Momoiro catalog loader
  -> IMomoiroCatalog / IGameDataCatalog.For(GameEra.Momoiro)
  -> Application AC15 snapshot/profile/services
  -> Mediator handlers for recommend/defaultsong/telop
  -> Momoiro adapter Mapperly mappers
  -> /v04r00/chassis/*.php protobuf responses

/v04r00/chassis/heartbeat.php and /bookkeeping.php
  -> Momoiro controller
  -> explicit static success DTO
```

### Recommended Project Structure

```text
Application/
+-- Abstractions/IMomoiroCatalog.cs
+-- Ac15/Ac15EraProfiles.cs
+-- Ac15/Ac15CatalogSnapshotFactory.cs
+-- Handlers/Get{InitialData,Recommend,Telop}Query.Momoiro.cs

Infrastructure/
+-- GameDataCatalog/Momoiro/
    +-- MomoiroGameDataPaths.cs
    +-- MomoiroRequiredDataFiles.cs
    +-- MomoiroEraGameDataCatalog.cs

Adapters.GameProtocol.Momoiro/
+-- Controllers/{Recommend,DefaultSong,SongHash,TelopCheck,GetTelop}.cs
+-- Controllers/{Heartbeat,Bookkeeping}.cs
+-- Mappers/{Recommend,GetTelop}.cs

Tests/
+-- Momoiro/MomoiroCatalogLoaderTests.cs
+-- Momoiro/MomoiroMetadataRouteTests.cs
+-- Momoiro/MomoiroProtocolLimitsTests.cs
```

### Pattern 1: Root-Level Catalog Loader

**What:** mirror KIMIDORI's root-level `data` path class and required-file checker, replacing `GameEra.Kimidori` with `GameEra.Momoiro`. [VERIFIED: repo grep]  
**When to use:** all Momoiro catalog reads in Infrastructure. [VERIFIED: AGENTS.md]  
**Example:**

```csharp
// Source: Infrastructure/GameDataCatalog/Kimidori/KimidoriGameDataPaths.cs
public static string GameDataRoot => Path.Combine(PathHelper.GetDataPath(GameEra.Momoiro), "data");
public static string MusicInfo => Path.Combine(GameDataRoot, "musicinfo.xml");
public static string MusicMedleyInfo => Path.Combine(GameDataRoot, "musicmedleyinfo.xml");
public static string DefMusic => Path.Combine(GameDataRoot, "defmusic.bin");
public static string Tuning => Path.Combine(GameDataRoot, "fumen", "tuning.bin");
```

### Pattern 2: Metadata Route Controllers

**What:** replace Momoiro scaffold controllers for catalog-backed routes with the KIMIDORI controller shape: controller maps request, calls Mediator or catalog service, maps response. [VERIFIED: repo grep]  
**When to use:** `recommend.php`, `defaultsong.php`, `songhash.php`, `telopcheck.php`, and `gettelop.php`. [VERIFIED: IDA idalib]  
**Example:**

```csharp
// Source: Adapters.GameProtocol.Kimidori/Controllers/SongHashController.cs
var momoiro = gameDataService.Momoiro();

return Ok(new SonghashResponse
{
    Result = 1,
    SongHashVer = momoiro.SongHashVersion,
    SongHashTbl = Ac15SongHashCodec.EncodeTable(momoiro.SongHashTable),
});
```

### Pattern 3: Static Operational Routes

**What:** leave operational routes static when no native stateful role is proven. [VERIFIED: IDA idalib]  
**When to use:** `heartbeat.php` and `bookkeeping.php`. [VERIFIED: 40-CONTEXT.md]  
**Example:**

```csharp
// Source: Adapters.GameProtocol.Kimidori/Controllers/HeartbeatController.cs
return Ok(new HeartBeatResponse
{
    Result = 1,
    ComSvrStat = 1,
    GameSvrStat = 1,
});
```

### Anti-Patterns to Avoid

- **Proto-only routes:** do not add `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, `mainichisong.php`, or `crownsdata.php` from proto presence alone. [VERIFIED: Phase 39 evidence]
- **KIMIDORI numeric copy:** do not silently copy KIMIDORI/Murasaki favorite/crown/flag limits where Momoiro evidence is weak; isolate inferred values in `Ac15EraProfiles.Momoiro`. [VERIFIED: 40-CONTEXT.md]
- **Handler-local file reads:** do not read `Host/wwwroot/data/momoiro/data` directly from controllers or handlers. [VERIFIED: AGENTS.md]
- **Generated-wire tests:** do not add tests for generated DTO property presence or controller route attributes as source text. [VERIFIED: AGENTS.md]
- **Stateful bookkeeping:** do not persist cabinet accounting from `bookkeeping.php` in Phase 40. [VERIFIED: 40-CONTEXT.md]
- **Unsupported feature bleed:** do not enable Taikojuku, Tokkun, Banacoin, battle, Don Challenge, ChallengeCompe, event-folder, gacha, tournament, or newer item-shop features without new proto plus binary route evidence. [VERIFIED: 40-CONTEXT.md + Phase 39 evidence]

## Route Behavior Plan

| Route | Behavior | Reuse | Notes |
|-------|----------|-------|-------|
| `recommend.php` | Catalog-backed recommendation response. [VERIFIED: IDA idalib] | `GetRecommendQuery` + `Ac15CatalogReadbackService.BuildRecommendResponse`. [VERIFIED: repo grep] | Keep medley/reserved rows out through existing recommendation seed filtering. [VERIFIED: repo grep] |
| `defaultsong.php` | Return result, song hash version, compacted default-song flag. [VERIFIED: generated wire] | `GetInitialDataQuery(GameEra.Momoiro)` + `Ac15SongHashCodec.CompactBitset`. [INFERRED: KIMIDORI analog] | Wire length should be 48 bytes for current local data. [INFERRED: local count + codec math] |
| `songhash.php` | Return result, song hash version, big-endian encoded table. [VERIFIED: generated wire] | `Ac15SongHashCodec.EncodeTable`. [VERIFIED: repo grep] | Wire length should be 760 bytes for current local data. [VERIFIED: local count + codec] |
| `telopcheck.php` | Return available telop IDs from catalog-backed common initial data. [VERIFIED: IDA idalib] | `GetInitialDataQuery(GameEra.Momoiro)` + shared telop projection. [INFERRED: KIMIDORI analog] | If no Momoiro telop sidecar exists, success with an empty list is acceptable. [INFERRED: shared AC15 catalog pattern] |
| `gettelop.php` | Return telop content by requested ID. [VERIFIED: IDA idalib] | `GetTelopQuery(GameEra.Momoiro)` + Mapperly adapter mapper. [INFERRED: KIMIDORI analog] | Missing telop should follow shared readback behavior rather than throw from controller. [INFERRED: shared AC15 pattern] |
| `heartbeat.php` | Static success response with communication/game server status. [VERIFIED: generated wire] | Existing scaffold / KIMIDORI heartbeat controller. [VERIFIED: repo grep] | No stateful role proven. [VERIFIED: IDA idalib] |
| `bookkeeping.php` | Static success response with request logging only. [VERIFIED: generated wire] | Existing scaffold / KIMIDORI bookkeeping controller. [VERIFIED: repo grep] | Do not add accounting persistence. [VERIFIED: 40-CONTEXT.md] |

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Song-hash table response | Manual byte loop in Momoiro controller | `Ac15SongHashCodec.EncodeTable` | Existing codec already encodes big-endian `ushort` table entries. [VERIFIED: repo grep] |
| Default/release bitset compaction | Per-route custom bit packing | `Ac15SongHashCodec.CompactBitset` | Existing codec reorders by hash-table ordinal. [VERIFIED: repo grep] |
| Crown compaction | Momoiro-only packing routine | Existing AC15 crown value helpers plus `CompactTenBitValues` | Existing code owns 10-bit crown value packing and hash-order compaction. [VERIFIED: repo grep] |
| Recommendation selection | Momoiro sidecar or random logic | `Ac15RecommendationService` | Shared service already excludes reserved medley ranges. [VERIFIED: repo grep] |
| Telop projection | Controller-side filtering | `Ac15CatalogReadbackService` and `GetTelopQuery` | Keeps catalog readback in Application. [VERIFIED: repo grep + AGENTS.md] |
| Catalog filesystem probing | Hardcoded controller paths | `IMomoiroCatalog` via `IGameDataCatalog.For(GameEra.Momoiro)` | Preserves era separation and settings/path abstractions. [VERIFIED: AGENTS.md] |

**Key insight:** Phase 40 should wire existing AC15 catalog/packing services around a new Momoiro catalog/profile boundary, not create new protocol logic. [INFERRED: repo architecture]

## Common Pitfalls

### Pitfall 1: Treating Proto Presence As Route Authority

**What goes wrong:** proto-only families become controller stubs. [VERIFIED: Phase 39 evidence]  
**Why it happens:** `proto/momoiro` contains message families that Phase 39 route inventory did not prove as live endpoints. [VERIFIED: proto + Phase 39 evidence]  
**How to avoid:** require both proto/generated DTO presence and binary `.php` route evidence before exposing a route. [VERIFIED: 40-CONTEXT.md]  
**Warning signs:** new controllers for `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, or `mainichisong.php`. [VERIFIED: Phase 39 evidence]

### Pitfall 2: Overclaiming Descriptor Strings

**What goes wrong:** descriptor string presence is treated as numeric size or mutation semantics. [VERIFIED: IDA idalib caveat]  
**Why it happens:** strings such as `hash_crown_flg` and `hash_release_song_flg` prove field names, not byte counts. [VERIFIED: IDA idalib]  
**How to avoid:** compute catalog-driven wire sizes from local catalog count and shared codecs, and mark native constants as open where not proven. [INFERRED: shared AC15 analog]  
**Warning signs:** research or plan claims an IDA-proven crown byte size without an instruction/data-flow tie. [VERIFIED: IDA disassembly caveat]

### Pitfall 3: Putting Catalog Work In Controllers

**What goes wrong:** controllers start reading `Host/wwwroot/data/momoiro/data` or parsing XML. [VERIFIED: AGENTS.md anti-pattern]  
**Why it happens:** metadata routes look simple enough to implement inline. [ASSUMED]  
**How to avoid:** create `IMomoiroCatalog`, root path helpers, and Application handlers; controllers only handle transport. [VERIFIED: AGENTS.md + KIMIDORI pattern]  
**Warning signs:** `Path.Combine`, XML loaders, or `File.*` APIs in Momoiro controller files. [VERIFIED: AGENTS.md]

### Pitfall 4: Testing Implementation Shape Instead Of Runtime Behavior

**What goes wrong:** tests assert generated wire properties, route attributes as source text, or project-file strings. [VERIFIED: AGENTS.md]  
**Why it happens:** Phase 40 includes many wiring changes. [ASSUMED]  
**How to avoid:** test parsed catalog output, encoded route response bytes, feature absence through discovered route surface, and no-state operational route behavior. [VERIFIED: AGENTS.md]

## Code Examples

### Catalog Snapshot Factory

```csharp
// Source: Application/Ac15/Ac15CatalogSnapshotFactory.cs KIMIDORI pattern
public static Ac15CatalogSnapshot FromMomoiro(IMomoiroCatalog catalog)
{
    return new Ac15CatalogSnapshot(
        catalog.SongHashVersion,
        catalog.SongHashTable,
        catalog.MusicInfoFileOrder,
        EventFolders: new Dictionary<uint, EventFolderData>(),
        Telops: catalog.Telops,
        Movies: Array.Empty<MovieData>(),
        ItemShop: Ac15ItemShopSnapshot.Empty,
        DaniFileOrder: catalog.DaniFileOrder);
}
```

Only include properties that `IMomoiroCatalog` actually exposes in Phase 40; do not add event-folder, item-shop, or movie authority without evidence. [VERIFIED: 40-CONTEXT.md]

### Default Song Response

```csharp
// Source: Adapters.GameProtocol.Kimidori/Controllers/DefaultSongController.cs pattern
var common = await mediator.Send(new GetInitialDataQuery(GameEra.Momoiro), cancellationToken);
var momoiro = gameDataService.Momoiro();

return Ok(new DefaultsongResponse
{
    Result = common.Result,
    SongHashVer = common.SongHashVer,
    HashDefaultSongFlg = Ac15SongHashCodec.CompactBitset(
        common.DefaultSongFlg,
        momoiro.SongHashTable),
});
```

The example reuses verified KIMIDORI route structure while substituting Momoiro catalog and wire types. [VERIFIED: repo grep]

## State Of The Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Treat Momoiro as route scaffolds only | Keep scaffolds only for static operational routes; use catalog-backed shared AC15 services for metadata routes | Phase 40 planning | Metadata routes become useful without inventing persistence. [VERIFIED: Phase 39 evidence + repo grep] |
| Copy KIMIDORI/Murasaki profile wholesale | Create explicit Momoiro profile with verified placement and labeled inferred caps | Phase 40 planning | Avoids silently importing unsupported features or wrong numeric limits. [VERIFIED: 40-CONTEXT.md] |
| Treat crown as standalone endpoint in all older AC15 eras | Model Momoiro crown as `UserDataResponse.hash_crown_flg` | Phase 40 planning | Matches Momoiro wire and route inventory. [VERIFIED: generated wire + Phase 39 evidence] |

**Deprecated/outdated:**

- Using `config/STxxxx-*` as the Momoiro catalog root is incorrect for this phase; local data is root-level under `Host/wwwroot/data/momoiro/data`. [VERIFIED: local data + 40-CONTEXT.md]
- Adding proto-only route stubs is out of scope and contradicted by Phase 39 route evidence. [VERIFIED: Phase 39 evidence]

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | `MaxFavoriteSongs = 5` is the conservative Momoiro profile cap. [ASSUMED] | MOMOIRO Limits And Profile Evidence | If native/client expects 10, favorites would be under-returned until Phase 41/42 adjustment. |
| A2 | Shared AC15 internal `128` song flag bytes / 1024-bit inflated storage is acceptable as Momoiro's internal envelope. [INFERRED: shared AC15 analog] | MOMOIRO Limits And Profile Evidence | If Momoiro native expects a smaller internal envelope, wire compaction still protects Phase 40 responses, but later persistence migration may need trimming. |
| A3 | Crown wire readback should reuse existing 10-bit-per-song crown values compacted through song-hash order. [INFERRED: shared AC15 analog] | MOMOIRO Limits And Profile Evidence | If native Momoiro crown packing differs, Phase 41 userdata crown readback would need a codec correction before cabinet acceptance. |
| A4 | Empty telop sidecar should produce success with no telops rather than a failure. [INFERRED: shared AC15 catalog pattern] | Route Behavior Plan | If the client requires at least one telop row, runtime validation would need a data sidecar or static fallback. |

## Open Questions

No blocking open questions for Phase 40 planning. [VERIFIED: research synthesis] The non-blocking native-proof gaps are favorite max count, exact native crown byte constant, and exact Don Point/reward cap. [VERIFIED: IDA disassembly caveat] They do not block catalog and metadata route wiring because mutation and full userdata readback are deferred. [VERIFIED: 40-CONTEXT.md]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| .NET SDK | Build/test | yes | 10.0.201 | None needed. [VERIFIED: `dotnet --version`] |
| Python | IDA-CLI bridge scripts | yes | 3.13.2 | None needed. [VERIFIED: `python --version`] |
| IDA idalib backend | Binary route/descriptor checks | yes | backend version not reported | Use existing Phase 39 evidence only if IDA becomes unavailable. [VERIFIED: IDA backend probe] |
| `.tools/momoiro/EBOOT.ELF.i64` | Binary research | yes | local IDB, last write 2026-06-25 | Blocking if removed. [VERIFIED: local file probe] |
| `Host/wwwroot/data/momoiro/data` | Catalog loading | yes | local data version `538116869` for musicinfo | Blocking if missing; planner should add required-file validation. [VERIFIED: local data] |
| Git | Commit research | yes | 2.52.0.windows.1 | None needed. [VERIFIED: `git --version`] |

**Missing dependencies with no fallback:** none. [VERIFIED: environment probes]  
**Missing dependencies with fallback:** none. [VERIFIED: environment probes]

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1. [VERIFIED: Directory.Packages.props] |
| Config file | `Tests/Tests.csproj`. [VERIFIED: repo grep] |
| Quick run command | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Momoiro|FullyQualifiedName~Ac15SongHashCodec|FullyQualifiedName~Ac15CatalogReadbackService" --no-restore` |
| Full suite command | `dotnet test Tests/Tests.csproj` |

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| MOCAT-01 | Momoiro root catalog loader reads root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`; song count is 380 and medley header size is 15. [VERIFIED: local data] | integration/unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroCatalogLoaderTests"` | No - Wave 0 |
| MOCAT-02 | Momoiro handlers/controllers consume catalog abstractions, not hardcoded filesystem paths. [VERIFIED: AGENTS.md] | unit/integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests"` | No - Wave 0 |
| MOCAT-03 | Momoiro profile exposes crown placement `UserData`, explicit unsupported features, and documented limits. [VERIFIED: generated wire + Phase 39 evidence] | unit | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroProtocolLimitsTests"` | No - Wave 0 |
| MOCAT-04 | Metadata routes return catalog-backed `recommend`, `defaultsong`, `songhash`, `telopcheck`, and `gettelop`; heartbeat/bookkeeping remain static success. [VERIFIED: IDA idalib + repo grep] | integration | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroMetadataRouteTests"` | No - Wave 0 |
| MOCAT-05 | Crown readback is profile-modeled as userdata-owned; no `crownsdata.php` route is exposed. [VERIFIED: generated wire + Phase 39 evidence] | unit/route-surface | `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~MomoiroRouteSurfaceTests|FullyQualifiedName~MomoiroProtocolLimitsTests"` | Partial - route-surface exists from Phase 39 |

### Sampling Rate

- **Per task commit:** run the focused Momoiro route/catalog/profile tests added by Wave 0. [VERIFIED: GSD validation policy]
- **Per wave merge:** run `dotnet test Tests/Tests.csproj`. [VERIFIED: existing repo command]
- **Phase gate:** run full test suite plus `dotnet build TaikoLocalServer.slnx` and `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"`. [VERIFIED: AGENTS.md + Phase 39 verification]

### Wave 0 Gaps

- [ ] `Tests/Momoiro/MomoiroCatalogLoaderTests.cs` - covers MOCAT-01 and root required-file validation.
- [ ] `Tests/Momoiro/MomoiroMetadataRouteTests.cs` - covers MOCAT-02 and MOCAT-04 observable route responses.
- [ ] `Tests/Momoiro/MomoiroProtocolLimitsTests.cs` - covers MOCAT-03 and MOCAT-05 profile/placement/absence behavior.
- [ ] Extend existing Momoiro route-surface tests only for behavior-backed absence checks; do not assert generated wire/source text. [VERIFIED: AGENTS.md]

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | no | Cabinet compatibility routes are not adding user-auth flows in Phase 40. [VERIFIED: 40-CONTEXT.md] |
| V3 Session Management | no | No session state is introduced in Phase 40. [VERIFIED: 40-CONTEXT.md] |
| V4 Access Control | yes | Keep enabled/disabled era application-part gating from Phase 39. [VERIFIED: Phase 39 verification] |
| V5 Input Validation | yes | Required-file validation, protobuf DTO mapping, and catalog parsers; no arbitrary filesystem input from requests. [VERIFIED: AGENTS.md + repo grep] |
| V6 Cryptography | no | No new crypto is introduced. [VERIFIED: research scope] |

### Known Threat Patterns For MOMOIRO Catalog/Routes

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Unsupported route exposure | Elevation of privilege / spoofing | Require proto plus binary route evidence and preserve disabled-era application-part removal. [VERIFIED: Phase 39 evidence] |
| Cross-era state writes | Tampering | Do not introduce persistence in Phase 40; future Momoiro state must be Momoiro-owned. [VERIFIED: 40-CONTEXT.md + AGENTS.md] |
| Path traversal or wrong data root | Tampering | Resolve paths through `PathHelper` and era catalog path helpers; never accept request-provided paths. [VERIFIED: AGENTS.md] |
| Malformed operator data | Denial of service | Required-file validation and parser tests for root-level files. [VERIFIED: AGENTS.md + KIMIDORI pattern] |
| Over-posted protobuf fields | Tampering | Controllers map wire DTOs to common/Application DTOs and keep mutation deferred. [VERIFIED: AGENTS.md + 40-CONTEXT.md] |

## Sources

### Primary (HIGH confidence)

- `AGENTS.md` - project architecture, testing, Mapperly, path, and era-separation constraints. [VERIFIED: repo file]
- `.planning/phases/40-momoiro-protocol-limits-root-catalog-and-route-behavior/40-CONTEXT.md` - locked scope, deferred work, and required research questions. [VERIFIED: repo file]
- `.planning/PROJECT.md`, `.planning/REQUIREMENTS.md`, `.planning/ROADMAP.md`, `.planning/STATE.md` - Phase 40 requirements and milestone state. [VERIFIED: repo files]
- `.planning/phases/39-momoiro-evidence-and-era-foundation/39-MOMOIRO-EVIDENCE.md` and `39-VERIFICATION.md` - route inventory, proto-only absences, and Phase 39 verification. [VERIFIED: repo files]
- `.tools/momoiro/EBOOT.ELF.i64` via IDA idalib - route pointer tables, registration functions, descriptor strings, and disassembly caveats. [VERIFIED: IDA idalib]
- `proto/momoiro/taiko.proto` and `Adapters.GameProtocol.Momoiro/Wire/Game.cs` - field placement and generated DTO names. [VERIFIED: repo grep]
- `Host/wwwroot/data/momoiro/data/musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin` - root catalog layout and local counts. [VERIFIED: local data]
- `Application/Ac15/*`, `Infrastructure/GameDataCatalog/Kimidori/*`, `Adapters.GameProtocol.Kimidori/*` - shared AC15 and KIMIDORI analog patterns. [VERIFIED: repo grep]

### Secondary (MEDIUM confidence)

- Existing shared AC15 behavior reused by Blue/Green/Yellow/Red/White/Murasaki/KIMIDORI for recommendation filtering, song-hash encoding, and recent-song limits. [VERIFIED: repo grep]

### Tertiary (LOW confidence)

- Conservative `MaxFavoriteSongs = 5` for Momoiro. [ASSUMED]
- Native Don Point/reward cap. [ASSUMED]

## Metadata

**Confidence breakdown:**

- Standard stack: HIGH - all tooling/packages are existing repo dependencies or local tools. [VERIFIED: Directory.Packages.props + environment probes]
- Architecture: HIGH - controller/Application/Infrastructure responsibilities are established by AGENTS.md and existing KIMIDORI patterns. [VERIFIED: AGENTS.md + repo grep]
- Route inventory and field placement: HIGH - backed by Phase 39 evidence, IDA route tables, proto, and generated wire. [VERIFIED: Phase 39 evidence + IDA idalib + generated wire]
- Catalog root and song-hash table: HIGH - backed by local root data and existing codecs. [VERIFIED: local data + repo grep]
- Numeric protocol caps: MEDIUM-LOW - catalog-derived wire lengths are computable, but favorite max, native crown byte constant, and reward cap were not directly proven from native instructions in this pass. [VERIFIED: IDA disassembly caveat]

**Research date:** 2026-06-26  
**Valid until:** 2026-07-26 for repo-local patterns; revisit sooner if MOMOIRO binary/client captures reveal native constants or route semantics. [INFERRED: research freshness policy]
