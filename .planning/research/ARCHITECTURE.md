# Architecture Research: v1.5 Murasaki AC15 Support

**Domain:** Brownfield AC15 cabinet protocol era integration  
**Researched:** 2026-06-21  
**Confidence:** MEDIUM overall. Existing repo integration patterns and `ST6100-1` root evidence are strong; route prefix binding and global-score request semantics still need runtime/cabinet confirmation.

## Standard Architecture

### System Overview

Murasaki should be added as a first-class AC15 era, not as a White compatibility mode. The existing server architecture already has the right seams: adapter-local wire DTOs and controllers at the edge, Application handlers and AC15 capability services in the middle, and era-owned EF/catalog infrastructure at the bottom.

```
Cabinet / RPCS3
    |
    | direct protobuf POSTs
    v
Adapters.GameProtocol.Murasaki
    - /v06r00/chassis/*.php game routes
    - adapter-local Wire DTOs generated from proto/murasaki
    - thin controllers, Mapperly mappers, route prefix constants
    |
    | Common / Ac15 application DTOs
    v
Application
    - Murasaki partial handlers
    - Ac15 shared services where profile and wire placement match
    - new split-metadata and global-score capabilities where needed
    |
    | typed era tables and catalog interfaces
    v
Infrastructure / Domain
    - UserSaveDataMurasaki and Murasaki-owned rows
    - IMurasakiCatalog over Host/wwwroot/data/murasaki/data
    - active config root selected from evidence, not filenames

Shared startup/version routes remain under /v01r00/chassis/* unless runtime evidence contradicts the current user-provided prefix split and Murasaki vsinterface shape.
```

### Evidence Summary

| Finding | Evidence | Confidence | Architectural consequence |
|---------|----------|------------|---------------------------|
| Game prefix is expected as `/v06r00/chassis` | User-provided context plus IDA literal `v06r00` at `.tools/murasaki/EBOOT.ELF.i64` ea `12094456` | MEDIUM | Implement `MurasakiRoutePrefixes.Game = "/v06r00/chassis"` behind first-phase route probes; verify with cabinet/RPCS3 logs before closeout. |
| Startup/version prefix is expected as `/v01r00/chassis` | User-provided context, IDA literal `v01r00` at ea `12094464`, and `proto/murasaki/vsinterface.proto` startup/verup messages | MEDIUM | Reuse shared older-AC15 startup/version handling; do not duplicate startup routes under `/v06r00`. |
| Startup suffixes are in the Murasaki IDB | IDA string/data table: `chassis/startupauth.php`, `chassis/verupauth.php`, `chassis/verupcomplete.php` at eas `12098440`, `12098464`, `12098488` | HIGH | Treat startup/version route family as present, but keep shared ownership under `/v01r00`. |
| Core game suffixes are in the Murasaki IDB | IDA strings: `playresult.php`, `baidcheck.php`, `mydonentry.php`, `userdata.php`, `crownsdata.php`, `recommend.php`, `selfbest.php`, `heartbeat.php` | HIGH | Foundation can expose no-state probes for these, then later bind handlers. |
| Metadata is split, not monolithic | `proto/murasaki/taiko.proto` has `Defaultsong`, `Mainichisong`, `Foldercheck`, `Getfolder`, `Telopcheck`, `Gettelop`, `Taikojuku`; IDA route table has matching suffixes | HIGH | Do not clone White `initialdatacheck.php`; build endpoint-specific Murasaki metadata queries/mappers. |
| No Murasaki `initialdatacheck.php` route was found | Murasaki proto has no `Initialdatacheck` message; focused IDA route table scan found no `chassis/initialdatacheck.php` | HIGH | `GetInitialDataQuery` may remain useful internally, but Murasaki should not expose an initial-data endpoint unless new evidence appears. |
| Active runtime root is `ST6100-1` | IDA strings: `/data/config/ST6100-1/musicinfo.xml`, `/data/config/ST6100-1/musicmedleyinfo.xml`, `/data/nutdata/pack/ST6100-1`, `/updates/ST6100-1/localranking.bin` | HIGH | `MurasakiGameDataPaths.ConfigRoot` should use `ST6100-1`; `ST5100-1` and `ST5100-7` are present local data but should be inactive unless later evidence changes target version. |
| Local data contains competing roots | `Host/wwwroot/data/murasaki/data/config` contains `ST5100-1`, `ST5100-7`, and `ST6100-1`, each with AC15 catalog files | HIGH | First phase must record the root decision; catalog code must not auto-pick the first matching directory. |
| Global-score protocol exists, route semantics are not locked | Proto and IDA contain `SonghashRequest/Response`, `BestScoreRequest/Response`, `SetBestScore`, and `localranking.bin`; focused route scan did not find `chassis/songhash.php` or `chassis/bestscore.php` suffixes | LOW to MEDIUM | Treat global-score/song-hash readback as a separate capability after targeted binary/log proof, not as part of normal self-best. |
| Shopping/result-like proto surfaces are not route-ready | Proto has `ShoppingResult`, `HeadClerk2`, and `CommunicationLog`; IDA found message strings but no focused `chassis/*.php` suffixes for those families | LOW to MEDIUM | Do not implement Don Point shopping or logging routes from proto alone. |

### Component Responsibilities

| Component | Responsibility | Typical implementation |
|-----------|----------------|------------------------|
| `Adapters.GameProtocol.Murasaki` | Own Murasaki routes, generated wire DTOs, Mapperly mappers, and direct-protobuf controller edge | New adapter project mirroring White/Red structure, with `MurasakiRoutePrefixes` and per-endpoint controllers |
| `Application/Handlers/*.Murasaki.cs` | Own Murasaki behavior dispatch and business decisions | Partial handler methods selected by `GameEra.Murasaki`; call shared AC15 services only through Murasaki profile/catalog/table inputs |
| `Application/Ac15` | Provide reusable AC15 capability algorithms | Extend `Ac15EraProfiles`, `Ac15CatalogSnapshotFactory`, and existing helpers; add new split-metadata/global-score service types only where existing shapes do not fit |
| `Domain/Entities` | Own persisted Murasaki state shape | New `UserSaveDataMurasaki`, best/play/favorite/recent/Dani/reward rows, and later global-score rows only if needed |
| `Infrastructure/GameDataCatalog/Murasaki` | Own Murasaki filesystem catalog loading and sidecar data | `MurasakiGameDataPaths`, `MurasakiRequiredDataFiles`, `MurasakiEraGameDataCatalog`, `IMurasakiCatalog`, active root `ST6100-1` |
| `Adapters.AdminApi` and `TaikoWebUI` | Expose only implemented Murasaki-owned state | Extend existing era-routed controllers and `WebUiEra`; hide unsupported Don Challenge/global-score/shopping surfaces until implemented |
| `Host` | Compose era enablement, adapter references, fallback content-type rules, packaging, and data junctions | Add Murasaki to enabled-era parsing, app-part removal, DI, protobuf fallback, config, content copy rules, and debug junction targets |

## Recommended Project Structure

```text
Adapters.GameProtocol.Murasaki/
  Controllers/
    BaidController.cs
    UserDataController.cs
    PlayResultController.cs
    DefaultSongController.cs
    MainichiSongController.cs
    FolderCheckController.cs
    GetFolderController.cs
    TelopCheckController.cs
    GetTelopController.cs
  Mappers/
    BaidMappers.cs
    UserDataMappers.cs
    PlayResultMappers.cs
    SplitMetadataMappers.cs
  Wire/
    Game.cs
    VsInterface.cs
  MurasakiRoutePrefixes.cs

Application/
  Ac15/
    Ac15EraProfiles.cs
    Ac15CatalogSnapshotFactory.cs
    MurasakiSplitMetadataService.cs
    MurasakiGlobalScoreReadback.cs        # add only after route/sequence proof
  Handlers/
    BaidQuery.Murasaki.cs
    UserDataQuery.Murasaki.cs
    UpdatePlayResultCommand.Murasaki.cs
    GetDefaultSongQuery.Murasaki.cs
    CheckFolderQuery.Murasaki.cs
    CheckTelopQuery.Murasaki.cs

Infrastructure/GameDataCatalog/Murasaki/
  MurasakiGameDataPaths.cs
  MurasakiRequiredDataFiles.cs
  MurasakiEraGameDataCatalog.cs

Domain/Entities/
  UserSaveDataMurasaki.cs
  SongBestDatumMurasaki.cs
  SongPlayDatumMurasaki.cs
  MurasakiFavoriteSongs.cs
  MurasakiRecentSongs.cs
```

### Structure Rationale

- **Adapter-local wire:** Murasaki generated DTOs must stay in the Murasaki adapter. White final/legacy already proves why wire packages should not be shared across route families.
- **Application common shapes:** Controllers should deserialize, map, send Mediator requests, and map back. Application handlers should never persist generated wire DTOs.
- **Profile-driven reuse:** Murasaki should join `Ac15EraProfiles` with explicit features, limits, and wire placement. Reuse should flow through `Ac15EraProfile`, `IMurasakiCatalog`, and typed Murasaki DbSets.
- **Split metadata is first-class:** Murasaki needs endpoint-specific metadata queries because its proto and IDA route table split what White/Red put in `initialdatacheck.php`.
- **Global score is separate:** `BestScoreResponse` is not self-best. It is a cabinet/global ranking readback shape with chunking (`seq_id`, `last_seq_id`) and top-rank arrays, so it deserves a separate capability after binary/log proof.

## Architectural Patterns

### Pattern 1: Evidence-First Era Foundation

**What:** Add a first-class era with only proven route/root/transport scaffolding first.

**When to use:** Every new AC15 era, especially when local data exposes multiple version roots.

**Trade-offs:** This delays runtime feature work, but prevents hard-to-reverse route and catalog mistakes.

**Example:**

```csharp
public static class MurasakiRoutePrefixes
{
    public const string Game = "/v06r00/chassis";
}

[HttpPost(MurasakiRoutePrefixes.Game + "/defaultsong.php")]
[Produces("application/protobuf")]
public async Task<IActionResult> DefaultSong([FromBody] DefaultsongRequest request)
{
    var common = await Mediator.Send(new GetDefaultSongQuery(GameEra.Murasaki), HttpContext.RequestAborted);
    return Ok(DefaultSongMappers.Map(common));
}
```

### Pattern 2: Adapter Wire to Application DTO Boundary

**What:** Generated protobuf DTOs are translated into Application DTOs at the controller/mapper edge. Business behavior consumes Application records only.

**When to use:** All BAID, userdata, playresult, metadata, and future global-score endpoints.

**Trade-offs:** More mapper code, but it keeps protocol churn adapter-local and makes shared AC15 services usable without leaking wire types.

**Example:**

```csharp
var playResult = PlayResultMappers.Map(request);
var result = await Mediator.Send(
    new UpdateAc15PlayResultCommand(request.Baid, GameEra.Murasaki, playResult),
    HttpContext.RequestAborted);
return Ok(PlayResultMappers.Map(result));
```

### Pattern 3: Capability-Owned Sharing

**What:** Share algorithms, not era state. A shared AC15 service can operate on Murasaki only when the Murasaki profile, catalog snapshot, and typed table inputs prove the same behavior.

**When to use:** Normal play writes, self-best, crowns, folders, telops, Taikojuku, favorites/recent, profile mutation, and Dani if the Murasaki limits match.

**Trade-offs:** Requires typed adapters and explicit profile entries, but avoids Red/White assumptions becoming hidden defaults.

**Example:**

```csharp
var snapshot = Ac15CatalogSnapshotFactory.FromMurasaki(murasakiCatalog);
var response = Ac15UserDataService.BuildResponse(
    MurasakiAc15UserDataAdapter.CreateSnapshot(saveData, snapshot, favorites, recent, lockedSongs),
    Ac15EraProfiles.Murasaki);
```

### Pattern 4: Split Metadata Queries

**What:** Murasaki's metadata routes should map to narrow Application queries rather than faking a White-style initial-data response.

**When to use:** `defaultsong.php`, `mainichisong.php`, `foldercheck.php`, `getfolder.php`, `telopcheck.php`, `gettelop.php`, and `taikojuku.php`.

**Trade-offs:** Slightly more endpoint DTOs, but response placement becomes obvious and testable.

**Recommended mapping:**

| Route | Application source | Response ownership |
|-------|--------------------|--------------------|
| `defaultsong.php` | Song hash version plus default song bitset | Murasaki mapper maps `SongHashVer` and `HashDefaultSongFlg` |
| `mainichisong.php` | Mainichi/achievement/rare bitsets | Murasaki mapper maps `HashMainichidojoAll` and `HashMainichidojoRare` |
| `foldercheck.php` | Catalog event folder IDs | Murasaki mapper returns repeated `folder_id` |
| `getfolder.php` | One requested folder ID plus HDD version | Murasaki mapper returns one `folder_id` and songs |
| `telopcheck.php` | Catalog telop IDs | Murasaki mapper returns repeated `telop_id` |
| `gettelop.php` | One telop body | Murasaki mapper returns dates and text |
| `taikojuku.php` | Existing AC15 Taikojuku service | Murasaki mapper uses Murasaki wire shape |

### Pattern 5: Global Score as a Separate Capability

**What:** Model song-hash/global-score readback separately from self-best and normal score persistence.

**When to use:** Only after targeted proof establishes route path(s), sequencing, expected hash table bytes, and whether responses use persisted local rows, server-wide synthetic rows, or file-backed `localranking.bin` data.

**Trade-offs:** Delays a tempting feature, but avoids corrupting personal score semantics with cabinet-global rankings.

## Data Flow

### Startup and Version Flow

```text
Cabinet -> /v01r00/chassis/startupauth.php
        -> shared older-AC15 startup controller/handler
        -> StartupAuthResponse from vsinterface-compatible shape

Cabinet -> /v01r00/chassis/verupauth.php or verupcomplete.php
        -> shared older-AC15 version route
```

The Murasaki vsinterface proto matches the older startup/verup message family. The IDA table proves the suffixes exist, but prefix binding should still be verified with runtime logs.

### Murasaki Game Flow

```text
Cabinet -> /v06r00/chassis/baidcheck.php
        -> Murasaki BAID controller
        -> Ac15BaidQuery(GameEra.Murasaki)
        -> shared identity + Murasaki save defaults
        -> BAIDResponse mapped by Murasaki adapter
```

### Split Metadata Flow

```text
Cabinet -> /v06r00/chassis/defaultsong.php
        -> GetDefaultSongQuery(GameEra.Murasaki)
        -> IMurasakiCatalog over ST6100-1
        -> fixed bitset from Ac15ProtocolBytes using Murasaki limits
        -> DefaultsongResponse

Cabinet -> /v06r00/chassis/foldercheck.php
        -> CheckFolderQuery(GameEra.Murasaki)
        -> catalog folder ids
        -> FoldercheckResponse
```

### Normal Play Write Flow

```text
Cabinet -> /v06r00/chassis/playresult.php
        -> PlayResultMappers.Map(request)
        -> UpdateAc15PlayResultCommand(GameEra.Murasaki, Ac15PlayResultEnvelope)
        -> Murasaki handler classifies supported mode
        -> Ac15NormalPlayWriter with Murasaki tables
        -> Murasaki score/best/favorite/recent/profile rows only
```

### Global Score Flow After Proof

```text
Cabinet -> proven song-hash/global-score route
        -> Murasaki global-score query
        -> catalog hash/version + global score read model
        -> chunked BestScoreResponse by seq_id/last_seq_id
```

Do not route this through `GetSelfBestQuery`; self-best is per-user and already has a separate protocol shape.

## Critical Boundaries

- **Route ownership:** `/v06r00/chassis` is Murasaki game traffic; `/v01r00/chassis` is shared startup/version traffic. Keep the prefixes centralized and covered by route tests and runtime logs.
- **Root ownership:** Use `ST6100-1` for Murasaki initial support. Do not select `ST5100-1` or `ST5100-7` because they appear in local data.
- **Wire ownership:** Generate Murasaki `Game.cs` and `VsInterface.cs` under `Adapters.GameProtocol.Murasaki/Wire`. Do not reuse White wire DTOs.
- **Business ownership:** Application handlers own behavior. Controllers deserialize/map/send/map back. Mapperly mappers remain mechanical except for explicit protocol classification helpers.
- **Persistence ownership:** Add Murasaki EF entities/tables. Shared identity can remain shared; score, save, favorite, recent, Dani, reward, and future global-score state must be Murasaki-owned.
- **Capability reuse:** Reuse AC15 helpers only through Murasaki profile, catalog snapshot, and typed table inputs.
- **Unsupported proto surfaces:** Proto-only `ShoppingResult`, `CommunicationLog`, `HeadClerk2`, `Songhash`, and `BestScore` are not automatically route-ready. Add them only after route and runtime semantics are proven.
- **Admin/WebUI ownership:** Expose only implemented Murasaki-owned state. No Don Challenge/global-score/shopping UI until the server behavior exists.

## Scaling Considerations

| Scale | Architecture adjustments |
|-------|--------------------------|
| Single local cabinet | Current ASP.NET Core monolith, SQLite, and in-memory catalog singletons are appropriate. |
| Multiple local cabinets | Keep SQLite but add tests around global-score ordering, favorite/recent limits, and concurrent playresult writes before enabling global leaderboards. |
| Larger private deployments | Consider indexing high-score/global-score tables by song, difficulty, score, and update time. Keep catalog loading immutable after startup. |

### Scaling Priorities

1. **Byte-heavy metadata:** Song hash tables, default-song flags, crown flags, and release-song flags can be large and easy to corrupt. Build them from shared byte helpers with era-specific limits.
2. **Global score readback:** If implemented, chunking and ordering will become the first correctness bottleneck. Define sequence semantics before optimizing.
3. **Catalog startup:** Multiple roots and sidecars can make startup failures ambiguous. Required-file validation should print the exact active root and missing file.

## Anti-Patterns

### Anti-Pattern 1: Cloning White `initialdatacheck.php`

**What people do:** Copy White or Red initial-data controller and map Murasaki split metadata into it.

**Why it is wrong:** Murasaki proto and IDA route evidence show separate metadata requests instead of `initialdatacheck.php`.

**Do this instead:** Add Murasaki-specific split metadata controllers and Application queries.

### Anti-Pattern 2: Picking `ST5100-*` From Directory Names

**What people do:** Choose `ST5100-1` or `ST5100-7` because it looks older or appears first.

**Why it is wrong:** The IDA target references `ST6100-1` runtime paths, including `musicinfo.xml`, `musicmedleyinfo.xml`, update data, and nutdata pack paths.

**Do this instead:** Hard-code the active Murasaki config directory to `ST6100-1` for this milestone and document the inactive roots.

### Anti-Pattern 3: Treating Global Score as Self-Best

**What people do:** Reuse self-best rows for `BestScoreResponse`.

**Why it is wrong:** `BestScoreResponse` has `seq_id`, `last_seq_id`, per-song best-three/rank arrays, and no BAID in the request. It is a different read model.

**Do this instead:** Keep self-best per-user. Add a Murasaki global-score capability only after route, chunking, and ranking semantics are proven.

### Anti-Pattern 4: Implementing Proto-Only Surfaces

**What people do:** Add controllers for every message in `taiko.proto`.

**Why it is wrong:** The focused IDA route table proves many route suffixes, but not every proto message has a proven `.php` route.

**Do this instead:** Start with IDA-proven suffixes and user-provided prefix context; require logs or deeper binary proof for candidates.

### Anti-Pattern 5: Sharing White Tables or Wire

**What people do:** Route Murasaki through White handlers, DTOs, or EF rows because many fields look similar.

**Why it is wrong:** White final/legacy already diverged, and Murasaki has a different metadata surface. Sharing state risks cross-era corruption.

**Do this instead:** Share only capability algorithms through typed Murasaki inputs.

## Integration Points

### External Services

| Service | Integration pattern | Notes |
|---------|---------------------|-------|
| Cabinet/RPCS3 client | Direct protobuf POSTs to `/v06r00/chassis/*.php` and shared `/v01r00/chassis/*` startup routes | Runtime verification is required before closeout. |
| Local Murasaki IDB | `ida-cli` `AgentSession.start(..., require_ida=True)` with `probe_backend(require_ida=True)` first | Used narrowly for route/root strings; no GUI opened. |
| Local operator data | `Host/wwwroot/data/murasaki/data` via `PathHelper.GetDataPath(GameEra.Murasaki)` | Active config root should be `config/ST6100-1`; data folder is a junction in this checkout. |

### Internal Boundaries

| Boundary | Communication | Notes |
|----------|---------------|-------|
| Host -> Murasaki adapter | Project reference, DI registration, app-part gating | Disabled era must remove Murasaki controllers from routing. |
| Murasaki adapter -> Application | Mediator requests over Common/Ac15 DTOs | No generated wire DTOs past the adapter. |
| Application -> Infrastructure | `ITaikoDbContext` concrete Murasaki DbSets and `IMurasakiCatalog` | Avoid repository-shaped wrappers unless a real shared algorithm needs a narrow row interface. |
| Infrastructure -> data files | Path helpers plus Murasaki path constants | Required-file checks should name `ST6100-1`. |
| AdminApi/WebUI -> Application/state | Existing era-routed controllers and generic AC15 pages | Add Murasaki to supported eras only for implemented state. |

## Phase Order Recommendation

1. **Route, root, and adapter foundation**
   - Add `GameEra.Murasaki`, generated Murasaki wire, adapter project, `MurasakiRoutePrefixes`, Host registration, disabled-era app-part gating, content-type fallback, and no-state probes for IDA-proven game suffixes.
   - Record IDA evidence for `v06r00`, `v01r00`, route suffixes, and `ST6100-1`.
   - Avoids: wrong route prefix, wrong active root, and accidental White route reuse.

2. **Catalog and AC15 profile binding**
   - Add `IMurasakiCatalog`, `MurasakiGameDataPaths`, `MurasakiRequiredDataFiles`, catalog loader, `Ac15EraProfiles.Murasaki`, `Ac15CatalogSnapshotFactory.FromMurasaki`, and Murasaki sidecar JSON files.
   - Required raw inputs should start with `ST6100-1/musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `fumen/tuning.bin`; `tuning_ext.bin` should be treated as optional until behavior needs it.
   - Avoids: relying on directory enumeration and missing publish/debug data rules.

3. **Split metadata readback**
   - Implement `defaultsong`, `mainichisong`, `foldercheck`, `getfolder`, `telopcheck`, `gettelop`, `taikojuku`, `recommend`, `crownsdata`, and `heartbeat` where evidence supports no-state/catalog-backed behavior.
   - This should precede complex persistence because the cabinet will likely ask these during startup/readiness flows.
   - Avoids: blocking cabinet flow by waiting for normal play persistence.

4. **Identity, profile, userdata, self-best, and normal read paths**
   - Add Murasaki-owned save/best/play/favorite/recent/Dani tables and `GetOrCreateMurasakiSaveDataAsync`.
   - Implement BAID, MyDon entry, userdata, self-best, favorite/recent readback, and AdminApi/WebUI read paths for implemented state.
   - Avoids: cross-era reads and unsupported UI surfaces.

5. **Normal playresult, Dani, Don Point, and reward mutation**
   - Add Murasaki playresult mapper/classifier, normal stage filtering, score/best/crown/favorite/recent writes, profile counters, Don Point totals, reward/progress fields, and Dani only where Murasaki data and limits prove the contract.
   - Keep unsupported modes success/no-write until proven.
   - Avoids: mixing Murasaki play uploads into White/Red state.

6. **Special capabilities after targeted proof**
   - Investigate and implement global-score/song-hash readback, shopping result, communication log, headclerk/local ranking, Don Challenge-like challenge arrays, or collectable behavior only after route path, request order, and read/write semantics are proven.
   - Global-score readback should be the first candidate because proto/IDA show the message family and `localranking.bin`, but route suffix and chunking behavior remain unresolved.
   - Avoids: proto-only implementation and self-best/global-score conflation.

7. **AdminApi/WebUI closeout and runtime verification**
   - Add remaining WebUI era support, settings surfaces, docs, targeted tests, generated Mapperly source inspection, solution/Host builds, and user-observed RPCS3/cabinet verification.
   - Done requires runtime evidence, not only build/test success.

## Sources

- `.planning/PROJECT.md` - current v1.5 Murasaki scope, constraints, evidence hierarchy, and milestone decisions.
- `.planning/MILESTONES.md` and `.planning/STATE.md` - shipped White/Red/Yellow context and current planning status.
- `proto/murasaki/taiko.proto` - Murasaki game protocol messages, especially split metadata routes and global-score shapes.
- `proto/murasaki/vsinterface.proto` - Murasaki startup/version message family.
- `proto/white/taiko.proto` and `proto/white/vsinterface.proto` - contrast evidence for White monolithic `initialdatacheck`.
- `Host/wwwroot/data/murasaki/data/config` - local roots `ST5100-1`, `ST5100-7`, and `ST6100-1`.
- `.tools/murasaki/EBOOT.ELF.i64` via `ida-cli` - `probe_backend(require_ida=True)` returned `ida_available=True`; route/root strings were gathered from this IDB.
- `Adapters.GameProtocol.White`, `Adapters.GameProtocol.Red`, `Application/Ac15`, `Application/Handlers/*.White.cs`, `Infrastructure/GameDataCatalog/White`, `Host/Program.cs`, and `Adapters.GameProtocol.Shared/GameProtocolApplicationParts.cs` - existing adapter, handler, catalog, Host, and app-part patterns.

---
*Architecture research for: v1.5 Murasaki AC15 Support*  
*Researched: 2026-06-21*
