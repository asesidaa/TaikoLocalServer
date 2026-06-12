# Feature Research

**Domain:** Red AC15 era support in TaikoLocalServer
**Milestone:** v1.3 Red AC15 Support
**Researched:** 2026-06-12
**Confidence:** HIGH for local proto/data/code-backed feature inventory; MEDIUM for full Don Challenge semantics until Red runtime traces prove call order and state meaning

## Correction Note

The requirements review corrected this file's first-pass challenge framing. Treat wiki Don Challenge behavior as product scope, but do not claim local Red data already proves challenge catalog/state semantics. Requirements should not list absent Red proto surfaces as features or exclusions. Red catalog should be one shared-AC15 integration unless actual Red parser/runtime deltas are found. Previous supported eras are no-touch boundaries for this milestone.

## Feature Landscape

### Evidence Summary

Red should be planned as a first-class older AC15 era, not as a Yellow copy. The local Red proto exposes the normal AC15 profile, userdata, initial data, self-best, crown, Dani, tournament/gacha, reward, Tokkun, Banacoin-adjacent, and challenge competition surfaces. Local Red data exists under `Host/wwwroot/data/red/data` with config roots `ST5100-1`, `ST5100-7`, `ST7100-1`, and `ST8100-1`; the milestone must prove the active runtime root before hardcoding catalog paths.

Red has no local WaiWai proto surface in `proto/red/taiko.proto`, and the Red data scan did not find WaiWai-named assets. Red also lacks Blue battle fields and item-shop request/response messages. Do not import Yellow WaiWai handling, Blue battle handling, or Yellow item-shop/Don-Katsu medal behavior into Red unless new Red-local evidence appears.

Don Challenge / challenge competition is the main Red-specific feature. Red proto exposes `ChallengeCompeRequest` / `ChallengeCompeResponse`, `UserDataResponse.is_challengecompe`, and playresult stage arrays for `ary_challenge_id`, `ary_user_compe_id`, and `ary_bng_compe_id`. Red local data also contains challenge-style `musicmedleyinfo.xml` rows with `challengelv` 101-113 under `ST8100-1`, plus `dojochallenge_*` local script assets. That is enough to include Red challenge competition in v1.3, but not enough to invent all schedule, category, reward, or ranking semantics without runtime traces.

Important correction: challengecompe route/proto presence in Blue, Green, and Yellow is not active behavior evidence. Current Blue and Green controllers only log and return `Result = 1`; current Yellow routes through a common mapper but the Yellow handler returns an empty response. Treat these as compatibility/scaffolding artifacts, not proof that newer eras meaningfully use Don Challenge.

### Table Stakes

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Red era foundation | Every supported era must be enableable, routable, testable, and independently configured. | MEDIUM | Add `GameEra.Red`, Red adapter project/DI, generated wire DTOs from `proto/red`, host settings, route gating, logging, and route ownership tests. Do not edit dumped proto inputs. |
| Red route and transport proof | Red protocol version/prefix must match the client, not guesses from nearby eras. | MEDIUM | Use local proto, runtime logs, and cabinet/RPCS3 traces to prove concrete game route prefix and shared startup/version behavior before locking routes. `proto/red/vsinterface.proto` supports the shared startup/verup shape. |
| Red catalog bootstrap | Cabinet metadata and gameplay depend on local Red data. | HIGH | Load the proven active Red config root for `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, and `fumen/tuning.bin`; support committed sidecar JSON only when server-authored data is required. |
| Red identity/profile/userdata | BAID, mydon entry, default save creation, and userdata readback are the cabinet's normal entry path. | HIGH | Use Red-owned save state; share only user/card identity. Map through common DTOs before handler logic. |
| Initial data and metadata routes | Cabinet menu startup expects telop, folders, Taikojuku, tournament/gacha, recommendation, and release-song metadata. | MEDIUM | Reuse AC15 catalog snapshot helpers where shapes match, but keep Red route/wire/catalog ownership separate. |
| Normal playresult persistence | Normal score, crown, favorite, recent, counters, rewards, and profile progression are the core loop. | HIGH | Use shared AC15 normal-play services where behavior is identical, binding Red-owned tables and Mapperly projections explicitly. |
| Self-best and crowns | Prior scores and crowns must read back after normal play. | MEDIUM | Prove Red crown byte length/compression/placement before assuming Blue/Green/Yellow response encoding. |
| Dani/Taikojuku | Red `Taikojuku*` proto and local `musicmedleyinfo.xml` expose Dan challenge levels. | MEDIUM | Reuse AC15 Dani writer/readback with Red-owned Dan tables and Red catalog rows; separate normal Dan levels from Don Challenge levels. |
| Red reward / present progression | Red proto has `reward_ptn`, `reward_progress`, `get_donpoint`, and `Rewardexecution*`; local `present.xml` contains Don Point thresholds. | HIGH | Model Red reward progression from `present.xml` and playresult fields. This is not Yellow item shop or Don/Katsu medal spending. |
| Tokkun compatibility | Red proto exposes `tokkun_tutorial_flg` and `ary_tokkunstage_info`; local Red assets include Tokkun sound names. | HIGH | Mirror the bounded Blue/Yellow contract: classify from stage info, persist only protocol-backed tutorial/raw history facts, and prevent normal/challenge/Dani/reward/shop writes from Tokkun uploads. |
| Banacoin-adjacent compatibility | Red proto exposes heartbeat Banacoin status and balance/payment/error/info messages. | MEDIUM | Keep routes stateless/log-and-success where runtime needs them for Tokkun/menu flow. No wallet, coupon, receipt, CHID, BNID, settlement, or transaction persistence. |
| AdminApi/WebUI Red routing | Operators need to inspect Red profiles, scores, history, favorites, Dani, customization, and supported catalog data. | MEDIUM | Extend `/api/{era}/...` and WebUI era selection after Red-owned persistence exists. Preserve existing legacy admin routes where already present. |
| Runtime verification | Passing tests alone does not prove cabinet compatibility. | HIGH | Close v1.3 only after automated tests, temp-output Host build, and repeatable cabinet/RPCS3 smoke for Red normal flow, Tokkun, and Don Challenge route behavior. |

### Differentiators

| Feature | Value | Complexity | Notes |
|---------|-------|------------|-------|
| Evidence-backed Red Don Challenge | This is the feature that makes Red meaningfully different from Yellow-era normal support. | HIGH | Build `challengecompe.php` from Red proto, Red challenge catalog rows, and runtime call traces. Persist Red-owned per-user challenge high scores/state only after proving category and track semantics. |
| Older-AC15 challenge-ready model | Red can become the first correct implementation of a Red-and-older behavior instead of a one-off stub. | HIGH | Design Red challenge catalog/state names so later older eras can reuse matching behavior without claiming Blue/Green/Yellow support. |
| Red reward/present support | Red's Don Point/present system is distinct from Yellow's later item-shop/medal flow. | MEDIUM | Parse `present.xml`, persist reward progress/unlocks through Red-owned state, and keep `rewardexecution.php` scoped to Red evidence. |
| Capability-based AC15 reuse | Red can benefit from the completed AC15 shared-core cleanup without merging era state. | MEDIUM | Use shared switch-free AC15 helpers for identical normal play, Dani, catalog projection, and AdminApi workflows; bind concrete Red tables in Red handlers. |
| Verification-first challenge rollout | Challenge competition can be made trustworthy by requiring runtime proof for menu visibility and post-play readback. | MEDIUM | Add route logs and state inspection around `challengecompe.php`, userdata `is_challengecompe`, and playresult challenge arrays before declaring support complete. |

### Anti-Features

| Feature | Why Avoid | What to Do Instead |
|---------|-----------|-------------------|
| Treat Blue/Green/Yellow `challengecompe.php` as active evidence | Those eras currently have scaffold/stub behavior and the user corrected that they do not meaningfully call Don Challenge. | Use Red proto/data/runtime traces as the challenge authority; document newer-era presence as non-evidence. |
| Red WaiWai support | Red proto has no WaiWai fields and local Red data scan did not find WaiWai assets. | Keep WaiWai routes, fields, state, play mode, WebUI labels, and tests absent for Red. |
| Blue battle copied into Red | Red proto/data do not expose Blue battle userdata, initialdata battle flags, battle playresult fields, or battle catalogs. | Keep battle routes, persistence, and initialdata battle advertisement absent. |
| Yellow item shop / Don-Katsu medals copied into Red | Red proto lacks `getitemshopinfo` and `itempurchase`; Red has reward/present/Don Point fields instead. | Implement Red reward/present progression only from Red evidence. |
| Real Banacoin authority | TaikoLocalServer is not a wallet, payment, coupon, receipt, settlement, BNID, or transaction source. | Provide only stateless compatibility required by observed Red client flows. |
| Proto-only route stubs without runtime need | Unsupported surfaces create fake compatibility and hide missing evidence. | Add Red routes only when proto plus runtime/client evidence or existing route inventory requires them; otherwise keep surfaces absent. |
| Wiki-driven semantics | Wiki context is useful scoping material but can drift from this client/proto build. | Treat wiki as a lead; local proto, data, logs, RPCS3/cabinet traces, and IDA evidence decide behavior. |
| Shared Red/Yellow/Blue gameplay tables | It would corrupt era boundaries and make no-cross-era behavior hard to verify. | Add Red-owned EF tables/entities; share only algorithms and identity state where already shared. |

## Feature Dependencies

```text
Red route/transport proof -> Red adapter foundation -> generated Red wire DTOs -> route/controller skeletons
Red catalog root proof -> Red catalog loaders -> initial data / metadata / Taikojuku / reward / challenge catalog
Red save schema -> BAID/mydon/userdata -> normal playresult -> self-best/crowns/favorites/recent/reward readback
Red normal playresult -> Dani and challenge result routing can safely distinguish normal, Dan, Tokkun, and challenge facts
Challenge catalog + playresult challenge arrays -> challengecompe readback and high-score/state persistence
Tokkun classifier -> Tokkun persistence/readback -> no-cross-mode boundaries
Red persistence and catalogs -> AdminApi/WebUI Red readback
Automated verification -> cabinet/RPCS3 normal, Tokkun, and challenge smoke -> milestone closeout
```

## MVP Definition For v1.3

### Launch With

- [ ] Red first-class era foundation: `GameEra.Red`, adapter, settings, DI, route gating, generated wire, and shared startup/version integration.
- [ ] Proven Red route prefix and runtime config root, with catalog bootstrap from local Red data.
- [ ] Red profile/login/userdata/default save, initial data, self-best, crowns, favorites, recent songs, recommendations, telops, folders, tournaments/gacha, and Dani/Taikojuku.
- [ ] Red normal playresult persistence through Red-owned score, play-history, best, crown, favorite, recent, profile, and reward state.
- [ ] Red reward/present progression from Red `present.xml` and Red playresult reward fields.
- [ ] Red Tokkun handling with the Blue/Yellow no-cross-mode contract adapted only to Red-owned state.
- [ ] Red Banacoin-adjacent compatibility routes only where runtime flow proves they are needed, with no payment authority.
- [ ] Red Don Challenge / challenge competition support backed by Red proto/data/runtime evidence, including `challengecompe.php` readback and playresult challenge-array persistence where proven.
- [ ] AdminApi/WebUI Red routing for supported readback surfaces.
- [ ] Final automated verification, temp-output Host build, and user-confirmed cabinet/RPCS3 smoke for normal Red flow, Tokkun, and Don Challenge/challengecompe behavior.

### Defer

- [ ] Older-than-Red era reuse of Don Challenge until Red semantics are implemented and verified.
- [ ] Challenge schedule windows, BNG/global ranking semantics, category-specific reward semantics, and exact `ary_user_compe_stat` / `ary_bng_compe_stat` behavior until runtime traces prove them.
- [ ] AdminApi/WebUI editing tools for challenge schedules or challenge catalog data; expose readback/inspection first.
- [ ] Any Banacoin balance/payment/coupon/transaction feature unless the project scope changes to become a payment authority.
- [ ] Any Red route not proven by proto plus runtime/client evidence.

### Exclude

- [ ] Red WaiWai support.
- [ ] Red Blue-style battle support.
- [ ] Yellow item shop / Don-Katsu medal shop support in Red.
- [ ] Shared Red/Yellow/Blue gameplay persistence tables.
- [ ] Treating Blue/Green/Yellow challengecompe stubs as evidence of active challenge behavior.

## Runtime Verification Targets

| Flow | What To Prove | Notes |
|------|---------------|-------|
| Startup/version | Red cabinet reaches game routes with shared startup/verup behavior and correct route prefix. | Use logs and RPCS3/cabinet request sequence; do not infer prefix from directory names alone. |
| Normal profile loop | BAID/mydon/userdata/initialdata/self-best/crowns survive restart and write only Red tables. | Include new user and returning user paths. |
| Normal playresult | Scores, crowns, favorites/recent, profile counters, reward progress, and present unlocks read back correctly. | Include no-cross-era assertions against Blue/Green/Yellow state. |
| Dani | Dan result persists and reads back through Red-owned Dan tables and Red catalog levels. | Separate Dan levels from challenge levels 101+. |
| Tokkun | Tokkun entry/upload succeeds, persists only Red Tokkun tutorial/raw history facts, and writes no normal/challenge/Dani/reward state. | Confirm Banacoin-adjacent calls do not block Tokkun availability. |
| Don Challenge | Client calls `challengecompe.php` as expected, challenge rows are advertised/read back, and post-play challenge facts persist only after proven. | Capture before/after play request sequence and serialized response field presence. |
| AdminApi/WebUI | Red profile, scores, history, favorites, Dani, customization/catalog readback resolve under `/api/Red/...`. | No unsupported WaiWai/battle/item-shop controls. |

## Sources

- `.planning/PROJECT.md`
- `.planning/MILESTONES.md`
- `.planning/STATE.md`
- `proto/red/taiko.proto`
- `proto/red/vsinterface.proto`
- `Host/wwwroot/data/red/data/config/ST5100-1`
- `Host/wwwroot/data/red/data/config/ST5100-7`
- `Host/wwwroot/data/red/data/config/ST7100-1`
- `Host/wwwroot/data/red/data/config/ST8100-1`
- `Application/Dtos/CommonChallengeCompeResponse.cs`
- `Application/Handlers/GetChallengeCompeQuery.cs`
- `Application/Handlers/GetChallengeCompeQuery.Yellow.cs`
- `Adapters.GameProtocol.Yellow/Controllers/ChallengeCompeController.cs`
- `Adapters.GameProtocol.Green/Controllers/ChallengeCompeController.cs`
- `Adapters.GameProtocol.Blue/Controllers/ChallengeCompeController.cs`
- `Tests/Yellow/YellowMetadataRouteTests.cs`
- `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md`

---
*Feature research for: v1.3 Red AC15 Support*
*Researched: 2026-06-12*
