# Project Research Summary

**Project:** TaikoLocalServer AC15 Era Support
**Domain:** Red AC15 support and Don Challenge scoping
**Researched:** 2026-06-12
**Corrected:** 2026-06-12 after requirements review
**Confidence:** HIGH for Red proto inventory and shared AC15 reuse; MEDIUM for route/version/config-root proof; LOW for Don Challenge implementation details until runtime evidence exists

## Executive Summary

Red should be added as a first-class AC15 era using the existing stack and shared AC15 implementation where the Red proto/data shape matches. This is not a new architecture problem: the meaningful baseline work is Red wire generation, Red adapter/Host registration, Red-owned state, shared AC15 catalog/profile hookup, normal play, Dani, Tokkun, reward/progression compatibility, AdminApi/WebUI readback, and final runtime verification.

The first research pass over-specified absence and challenge behavior. That is corrected here. If a surface is absent from Red proto, it should simply stay absent; it is not a requirement to implement its absence. Work on Red should not change Blue, Green, Yellow, or Nijiiro behavior except where shared code must compile and still pass existing tests.

Don Challenge is real Red product scope because the wiki documents monthly Red-era Don Challenge tasks and rewards. The wiki does not prove which Red endpoint, proto field, database schema, or response shape carries that behavior. Local Red proto exposes `ChallengeCompe*` and playresult challenge arrays, but local Red data does not currently provide enough challenge catalog/behavior evidence to specify a stateful implementation. Therefore v1.3 should include a Don Challenge evidence/compatibility slice, with stateful challenge progress only after runtime/client evidence identifies the actual contract.

## Key Findings

### Stack

No new broad technology is needed.

- Generate Red wire from `proto/red/taiko.proto` and `proto/red/vsinterface.proto`.
- Keep dumped Red proto files read-only.
- Use the existing ASP.NET Core host, protobuf-net, Mapperly, Mediator handlers, EF Core SQLite, shared `Application/Ac15` helpers, AdminApi/WebUI routing, and xUnit verification.
- Keep Red gameplay persistence Red-owned. Reuse shared AC15 algorithms through concrete Red DbSets and typed helpers, not shared gameplay tables or repository-shaped wrappers.

### Feature Scope

**Must have:**
- Red first-class era foundation and route/version/config-root evidence.
- Red shared-AC15 catalog integration, kept small unless Red data proves parser differences.
- Red-owned profile/userdata/normal play/self-best/crowns/favorites/recent/Dani state over existing shared AC15 services.
- Red reward/progression compatibility for `reward_ptn`, `reward_progress`, Don point fields, `rewardcardcheck`, and `rewardexecution`.
- Red Tokkun classification/persistence/readback with no normal-score side effects.
- Red Banacoin-adjacent compatibility only as needed for observed runtime flow, with no wallet/payment authority.
- Don Challenge scoping/evidence: model wiki-level product behavior as target scope, but do not invent endpoint/schema semantics.
- Red AdminApi/WebUI readback for implemented Red-owned state.
- Automated verification and cabinet/RPCS3 smoke before closeout.

**No-touch boundaries:**
- Do not alter Blue, Green, Yellow, or Nijiiro behavior for Red work unless shared code changes require preserving existing behavior.
- Do not create Red item-shop, medal, WaiWai, battle, AI/ghost, token-count, or shop-folder surfaces without Red proto/client evidence.

### Don Challenge

Wiki-supported product behavior:

- Red was active from 2016-07-14 to 2017-03-14.
- Red had monthly Don Challenge tasks from August 2016 through February 2017.
- Don Challenge typically had 10 individual tasks plus one community task.
- Tasks were shown in Donder Hiroba and required a Donder Hiroba login.
- Task conditions included normal-play clears, full combos, score thresholds, genre/song-name filters, difficulty constraints, and grouped conditions.
- Tokkun did not count toward Don Challenge.
- Eight completed tasks unlocked a song; ten completed tasks awarded a title; reward reflection timing was next-day 07:00, with later general song unlocks.

Open implementation questions:

- Which Red cabinet endpoint or payload carries task lists and progress.
- Whether `challengecompe.php` is actually called by the Red client and what response shape is accepted.
- Whether the local server must compute progress, advertise tasks, simulate community counts, or only provide enough compatibility for the client flow.
- Which parts, if any, can be backed by local Red data instead of sidecar configuration or captured runtime evidence.

### Architecture

Use the standard AC15 layered path:

1. Red controller under the proven Red route prefix.
2. Red generated wire DTO.
3. Red Mapperly/manual protocol mapper.
4. Application common DTO.
5. Mediator dispatch to `.Red.cs` handler partial.
6. Red-owned DbSets/catalogs bound into shared AC15 services where behavior matches.
7. Red wire response mapping.

Do not split Red catalog into many roadmap requirements. The default requirement is one shared AC15 catalog integration plus route/root proof. Add more only if Red data proves a parser or runtime-data delta.

### Pitfalls

- Treating absence as a feature requirement. If Red proto lacks a surface, do not implement it.
- Touching previous versions while adding Red. Existing eras should remain unchanged except for shared-code preservation.
- Claiming Don Challenge implementation details from wiki or from `ChallengeCompe*` proto names alone.
- Turning Don Challenge into Red `present.xml`/Don-point progression. Wiki Don Challenge rewards are song/title thresholds, not the same thing as `present.xml`.
- Copying Yellow shop/medal/WaiWai assumptions into Red.
- Dropping Red Tokkun into normal play before classification.
- Writing Red state into any non-Red gameplay table.

## Suggested Roadmap Shape

### Phase 18: Red Evidence and Era Foundation

Prove Red route prefix, startup/version behavior, direct-protobuf assumptions, and active data root. Add `GameEra.Red`, Red adapter/wire project, Host/DI/config registration, and route scaffolding for proto-supported endpoints.

### Phase 19: Red Catalog and Shared AC15 Profile

Add Red catalog/profile support by reusing existing shared AC15 loaders/services wherever file shape matches. Keep this small: one catalog/profile phase, not a broad rewrite.

### Phase 20: Red Profile, Userdata, Normal Play, Crowns, Self-Best, and Dani

Add Red-owned state and route normal cabinet flow through shared AC15 services where behavior matches.

### Phase 21: Red Reward, Tokkun, and Compatibility Routes

Handle Red reward/progression fields and reward routes as protocol compatibility, add narrow Red Tokkun support, and add Banacoin-adjacent compatibility only if runtime flow requires it.

### Phase 22: Red Don Challenge Evidence and Compatibility

Use wiki behavior as product scope and runtime/proto/client evidence as implementation authority. Start with evidence and compatibility; only implement stateful challenge progress when the actual client contract is known.

### Phase 23: Red AdminApi/WebUI and Runtime Closeout

Expose Red-owned readback surfaces, run full automated verification, and close only after cabinet/RPCS3 smoke.

## Sources

- `.planning/PROJECT.md`
- `proto/red/taiko.proto`
- `proto/red/vsinterface.proto`
- `Host/wwwroot/data/red/data`
- `Application/Ac15/*`
- `Infrastructure/GameDataCatalog/Ac15/*`
- Wiki Red page: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%83%AC%E3%83%83%E3%83%89
- Wiki Don Challenge page: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%83%89%E3%83%B3%E3%83%81%E3%83%A3%E3%83%AC
- Wiki Red Don Challenge history: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%83%89%E3%83%B3%E3%83%81%E3%83%A3%E3%83%AC/%E9%81%8E%E5%8E%BB%E3%81%AE%E3%81%8A%E9%A1%8C/%E3%83%AC%E3%83%83%E3%83%89

---
*Research corrected: 2026-06-12*
*Ready for requirements: yes*
