# Phase 23 White Evidence Gate

**Created:** 2026-06-17
**Scope:** White AC15 0.13 route, startup/version, transport, data-root, and feature-surface evidence before route/controller implementation.

## Route Prefix Decision

| Question | Status | Evidence Source | Decision |
|----------|--------|-----------------|----------|
| Exact White game route prefix | `UNRESOLVED_BLOCKS_ROUTE_CODE` | Current context expects `/v07r00/chassis`, but live IDB raw scans found no `/v07r00/chassis`, no `/chassis/*.php` strings, and no `.php` suffix strings. The scan only found generic `chassis` and protobuf/message-name fragments. | Treat `/v07r00/chassis` as an expected hypothesis, not a proven route prefix. Do not add White game route attributes or Host fallback until route strings, request logs, cabinet/RPCS3 captures, or equivalent local evidence prove the prefix. |

`/v07r00/chassis` is recorded here because Phase 23 context D-01 expects it. It is not marked proven by this artifact. The current nonzero IDB proves an evidence source exists locally; it does not prove route strings.

## Route Suffix Gate

No route suffix is `SCAFFOLD_APPROVED` in this evidence pass. The live IDB contains proto/message-name fragments for many White request/response families, but the route-inventory pass did not find full `.php` suffix strings. Proto presence is a cross-check only; it is not enough to scaffold a controller route.

| Endpoint suffix | Proto request | Proto response | White route evidence | Plan 03 treatment |
|-----------------|---------------|----------------|----------------------|-------------------|
| `bookkeeping.php` | `BookKeepingRequest` | `BookKeepingResponse` | No `.php` suffix string found. Message-name fragments such as `bookkeepingr` were found in the IDB. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `gettelop.php` | `GettelopRequest` | `GettelopResponse` | No `.php` suffix string found. Message-name fragments such as `getteloprequ` / `gettelopresp` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `heartbeat.php` | `HeartBeatRequest` | `HeartBeatResponse` | No `.php` suffix string found. Message-name fragments such as `heartbeatreq` / `heartbeatres` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `getfolder.php` | `GetfolderRequest` | `GetfolderResponse` | No `.php` suffix string found. Message-name fragments such as `getfolderreq` / `getfolderres` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `taikojuku.php` | `TaikojukuRequest` | `TaikojukuResponse` | No `.php` suffix string found. Message-name fragments such as `taikojukureq` / `taikojukures` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `initialdatacheck.php` | `InitialdatacheckRequest` | `InitialdatacheckResponse` | No `.php` suffix string found. Message-name fragments such as `initialdatac` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `tournamentcheck.php` | `TournamentcheckRequest` | `TournamentcheckResponse` | No `.php` suffix string found. Message-name fragments such as `tournamentch` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `baidcheck.php` | `BAIDRequest` | `BAIDResponse` | No `.php` suffix string found. Message-name fragments such as `baidrequest` / `baidresponse` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; expected older-AC15 suffix name, not approved. |
| `mydonentry.php` | `MydonEntryRequest` | `MydonEntryResponse` | No `.php` suffix string found. Message-name fragments such as `mydonentryre` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; expected older-AC15 suffix name, not approved. |
| `userdata.php` | `UserDataRequest` | `UserDataResponse` | No `.php` suffix string found. Message-name fragments such as `userdatarequ` / `userdataresp` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `playresult.php` | `PlayResultRequest` | `PlayResultResponse` | No `.php` suffix string found. Message-name fragments such as `playresultre` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `selfbest.php` | `SelfBestRequest` | `SelfBestResponse` | No `.php` suffix string found. Message-name fragments such as `selfbestrequ` / `selfbestresp` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `recommend.php` | `RecommendRequest` | `RecommendResponse` | No `.php` suffix string found. Message-name fragments such as `recommendreq` / `recommendres` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `crownsdata.php` | `CrownsDataRequest` | `CrownsDataResponse` | No `.php` suffix string found. Message-name fragments such as `crownsdatare` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `headclerk2.php` | `HeadClerk2Request` | `HeadClerk2Response` | No `.php` suffix string found. Message-name fragments such as `headclerk2re` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `getreitai.php` | `GetreitaiRequest` | `GetreitaiResponse` | No `.php` suffix string found. Message-name fragments such as `getreitaireq` / `getreitaires` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `rewardcardcheck.php` | `RewardcardcheckRequest` | `RewardcardcheckResponse` | No `.php` suffix string found. Message-name fragments such as `rewardcardch` were found. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |
| `rewardexecution.php` | `RewardexecutionRequest` | `RewardexecutionResponse` | No `.php` suffix string found. Only truncated `execut` fragments were found near reward message strings. | `UNRESOLVED_BLOCKS_ROUTE_CODE`; proto-only candidate, not approved. |

### SCAFFOLD_APPROVED Allowlist

There are currently zero `SCAFFOLD_APPROVED` White game route suffixes. To approve a suffix for Plan 03, update this artifact with:

- exact route prefix evidence,
- exact `.php` suffix evidence,
- evidence source path or capture id,
- matching `proto/white` request and response,
- bounded no-state response expectation for the Phase 23 scaffold.

## Shared Startup/Version Ownership

| Route family | Owner | Evidence | Phase 23 decision |
|--------------|-------|----------|-------------------|
| `/v01r00/chassis/startupauth.php` | `Adapters.GameProtocol.Shared` | `proto/white/vsinterface.proto` contains `StartupAuthRequest` and `StartupAuthResponse`, matching the existing shared older-AC15 startup message family. | Keep shared ownership unless White route evidence contradicts D-02. |
| `/v01r00/chassis/verupauth.php` | `Adapters.GameProtocol.Shared` | `proto/white/vsinterface.proto` contains `VerupAuthRequest` and `VerupAuthResponse`. | Keep shared ownership unless White route evidence contradicts D-02. |
| `/v01r00/chassis/verupcomplete.php` | `Adapters.GameProtocol.Shared` | `proto/white/vsinterface.proto` contains `VerupCompleteRequest` and `VerupCompleteResponse`. | Keep shared ownership unless White route evidence contradicts D-02. |

White must not add duplicate startup/version controllers under `/v07r00/chassis/*` in Phase 23 without stronger White client evidence.

## Transport Expectation

| Surface | Status | Evidence | Phase 23 treatment |
|---------|--------|----------|--------------------|
| White game route body | `DIRECT_PROTOBUF_EXPECTED` | `proto/white/taiko.proto` exposes direct request/response message pairs such as `PlayResultRequest` / `PlayResultResponse`, not wrapper request envelopes. | Treat game endpoints as direct-protobuf for later implementation unless capture evidence contradicts this. |
| Missing/blank HTTP `Content-Type` fallback | `UNRESOLVED_BLOCKS_ROUTE_CODE` | No proven `/v07r00/chassis` route prefix exists in this artifact. | Do not add Host `/v07r00/chassis` fallback until the route prefix is proven. |
| Startup/version body | `DIRECT_PROTOBUF_EXPECTED` | `proto/white/vsinterface.proto` matches shared startup/version direct request/response messages. | Keep using the shared `/v01r00/chassis/*` protobuf route family. |

## Active Data Root

| Question | Status | Evidence Source | Decision |
|----------|--------|-----------------|----------|
| White active data root for Phase 23 planning | `ACCEPTED_FOR_FOUNDATION` | `Host/wwwroot/data/white/data/config/ST7100-1` exists and contains `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `chassisinfo.xml`. | Record `Host/wwwroot/data/white/data/config/ST7100-1` as the accepted Phase 23 data root. This is catalog/data evidence only, not route-prefix proof. |

The active root decision allows later catalog planning to target `ST7100-1`; it does not authorize route attributes, runtime catalog binding, profile persistence, or reward semantics.

## IDB Evidence

| Artifact | Current evidence | What it proves | What it does not prove |
|----------|------------------|----------------|------------------------|
| `.tools/white/EBOOT.ELF.i64` | Live filesystem length is `129893515` bytes. | The current checkout no longer has a zero-byte White IDB. Binary/IDA evidence may be available after a proper route extraction workflow. | Nonzero size does not prove `/v07r00/chassis`, `.php` suffixes, active runtime root, call order, content type, or response semantics. |

Route inventory commands run during this pass:

```powershell
Get-Item '.tools\white\EBOOT.ELF.i64' | Select-Object Length,FullName
rg -a -n "\.php" '.tools\white\EBOOT.ELF.i64'
rg -a -n "v07r00|v01r00|/chassis|chassis/|chassis" '.tools\white\EBOOT.ELF.i64'
rg -a -n "bookkeeping|heartbeat|baid|mydonentry|userdata|playresult|initialdatacheck|tournamentcheck|getfolder|gettelop|taikojuku|selfbest|crownsdata|recommend|rewardcardcheck|rewardexecution|getreitai|headclerk2" '.tools\white\EBOOT.ELF.i64'
```

Observed result:

- `.php` search returned no route suffix strings.
- route-prefix search returned only generic `chassis` fragments, not `/v07r00/chassis` or `/v01r00/chassis`.
- endpoint-name search returned protobuf/message-name fragments, which are useful proto cross-checks but not route strings.

## Unresolved Gaps

| Gap | Blocks | Needed evidence |
|-----|--------|-----------------|
| Exact White game route prefix | White route attributes and Host `/v07r00/chassis` fallback | IDA route export, request-log/cabinet capture, RPCS3 trace, or equivalent local client evidence proving `/v07r00/chassis` or another exact prefix. |
| Exact White game route suffixes | White controllers for Plan 03 | Exact `.php` route strings or request captures for each suffix, plus matching `proto/white` request/response. |
| Runtime call order | Any claim that a proto surface is mandatory cabinet flow | Request logs, captures, or IDA call graph evidence. |
| Active runtime data selection beyond local file presence | Catalog/profile runtime implementation | Later catalog/profile plans should prove how `ST7100-1` is selected before hard runtime assumptions. |
| Reward, ChallengeCompe, Taikojuku/Dani, tournament, and collectable semantics | Stateful behavior | White-specific proto/data/log/capture/IDA proof in later phases. |

## Deferred Runtime Scope

Phase 23 Plan 01 does not implement White catalog loading, profile creation, userdata, normal play, self-best, crowns, favorites, recent songs, Taikojuku/Dani, reward/present state, Don Challenge, collectables, AdminApi, WebUI, item shop, Banacoin, battle, Tokkun, WaiWai, gacha, or tournament runtime behavior.

Unsupported or unproven White 0.13 surfaces remain absent. Red, Yellow, and Blue route sets are comparison material only, not scaffold authority.

## Plan 03 Gate

Plan 03 may not add White game controllers, route attributes, or Host `/v07r00/chassis` missing-content-type fallback while this artifact contains `UNRESOLVED_BLOCKS_ROUTE_CODE` for the prefix and all suffixes.

Plan 03 may proceed with route code only after this artifact is updated to mark the exact prefix and at least one exact suffix as `SCAFFOLD_APPROVED` using White IDB route export, local logs, cabinet/RPCS3 request captures, or equivalent local evidence. Proto-only evidence must remain blocked.
