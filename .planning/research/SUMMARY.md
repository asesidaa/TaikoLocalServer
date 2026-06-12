# Project Research Summary

**Project:** TaikoLocalServer AC15 Era Support
**Domain:** Red AC15 capability composition and shared older-AC15 ChallengeCompe scoping
**Researched:** 2026-06-12
**Corrected:** 2026-06-12 after requirements review
**Confidence:** HIGH for Red proto inventory and shared AC15 reuse; MEDIUM for route/version/config-root proof; LOW for ChallengeCompe implementation details until runtime evidence exists

## Executive Summary

Red should be added as a first-class AC15 era by composing supported AC15 capabilities with Red config, limits, wire placement, typed Red persistence, and Red/older-version evidence. This is not a new architecture problem: the meaningful baseline work is Red wire generation, Red adapter/Host registration, Red capability/profile binding, Red-owned state, shared AC15 catalog/runtime capability hookup, tutorial-only Tokkun, simple compatibility, AdminApi/WebUI readback, and final runtime verification.

The first research pass over-specified absence and challenge behavior. That is corrected here. If a surface is absent from Red proto, it should simply stay absent; it is not a requirement to implement its absence. Work on Red should not change Blue, Green, Yellow, or Nijiiro behavior except where shared code must compile and still pass existing tests.

Don Challenge / ChallengeCompe is shared older-AC15 product scope, with Red as the first binding/proof point in this milestone. The wiki documents monthly Red-era Don Challenge tasks and rewards, but it does not prove which endpoint, proto field, database schema, or response shape carries that behavior. Local Red proto exposes `ChallengeCompe*` and playresult challenge arrays, but local Red data does not currently provide enough challenge catalog/behavior evidence to specify a stateful implementation. Therefore v1.3 should include a shared older-AC15 ChallengeCompe evidence/compatibility slice, with stateful challenge progress only after runtime/client evidence identifies the actual contract.

## Key Findings

### Stack

No new broad technology is needed.

- Generate Red wire from `proto/red/taiko.proto` and `proto/red/vsinterface.proto`.
- Keep dumped Red proto files read-only.
- Use the existing ASP.NET Core host, protobuf-net, Mapperly, Mediator handlers, EF Core SQLite, shared `Application/Ac15` capability modules, AdminApi/WebUI routing, and xUnit verification.
- Keep Red gameplay persistence Red-owned. Reuse shared AC15 behavior through Red config/limits/wire placement, concrete Red DbSets, Mapperly delegates, and typed helpers, not shared gameplay tables or repository-shaped wrappers.

### Feature Scope

**Must have:**
- Red first-class era foundation and route/version/config-root evidence.
- Red shared-AC15 capability profile and catalog binding, kept small unless Red data proves parser differences.
- Red-owned profile/userdata/normal play/self-best/crowns/favorites/recent/Dani state over existing shared AC15 capability modules.
- Simple Red compatibility for reward card, reward execution, Don point fields, and Banacoin-adjacent routes only where runtime evidence requires it.
- Red Tokkun classification/readback limited to tutorial state, with no normal-score side effects or raw history unless evidence proves more.
- Red Banacoin-adjacent compatibility only as needed for observed runtime flow, with no wallet/payment authority.
- Shared older-AC15 ChallengeCompe scoping/evidence: model wiki-level product behavior as target scope, but do not invent endpoint/schema semantics.
- Red AdminApi/WebUI readback for implemented Red-owned state.
- Automated verification and cabinet/RPCS3 smoke before closeout.

**No-touch boundaries:**
- Do not alter Blue, Green, Yellow, or Nijiiro behavior for Red work unless shared code changes require preserving existing behavior.
- Do not create Red item-shop, medal, WaiWai, battle, AI/ghost, token-count, or shop-folder surfaces without Red proto/client evidence.

### ChallengeCompe / Don Challenge

Wiki-supported product behavior:

- Red was active from 2016-07-14 to 2017-03-14.
- Red had monthly Don Challenge tasks from August 2016 through February 2017.
- Don Challenge typically had 10 individual tasks plus one community task.
- Tasks were shown in Donder Hiroba and required a Donder Hiroba login.
- Task conditions included normal-play clears, full combos, score thresholds, genre/song-name filters, difficulty constraints, and grouped conditions.
- Tokkun did not count toward Don Challenge.
- Eight completed tasks unlocked a song; ten completed tasks awarded a title; reward reflection timing was next-day 07:00, with later general song unlocks.

Open implementation questions:

- Which Red cabinet endpoint or payload carries task lists and progress, and which parts are shared older-AC15 behavior versus Red binding details.
- Whether `challengecompe.php` is actually called by the Red client and what response shape is accepted.
- Whether the local server must compute progress, advertise tasks, simulate community counts, or only provide enough compatibility for the client flow.
- Which parts, if any, can be backed by local Red data instead of sidecar configuration or captured runtime evidence.

### Architecture

Use the capability-composition AC15 layered path:

1. Red controller under the proven Red route prefix.
2. Red generated wire DTO.
3. Red Mapperly/manual protocol mapper.
4. Application common DTO.
5. Mediator dispatch to `.Red.cs` handler partial.
6. Red config/limits/wire placement and Red-owned DbSets/catalogs bound into shared AC15 capability modules where behavior matches.
7. Red wire response mapping.

Do not split Red catalog into many roadmap requirements. The default requirement is one shared AC15 catalog integration plus route/root proof. Add more only if Red data proves a parser or runtime-data delta.

### Pitfalls

- Treating absence as a feature requirement. If Red proto lacks a surface, do not implement it.
- Touching previous versions while adding Red. Existing eras should remain unchanged except for shared-code preservation.
- Treating Red as a clone instead of a composition root for shared capabilities.
- Claiming ChallengeCompe implementation details from wiki or from `ChallengeCompe*` proto names alone.
- Turning Don Challenge into Red `present.xml`/Don-point progression. Wiki Don Challenge rewards are song/title thresholds, not the same thing as `present.xml`.
- Copying Yellow shop/medal/WaiWai assumptions into Red.
- Dropping Red Tokkun into normal play before classification.
- Writing Red state into any non-Red gameplay table.

## Suggested Roadmap Shape

### Phase 18: Red Evidence and Capability Foundation

Prove Red route prefix, startup/version behavior, direct-protobuf assumptions, active data root, and Red-supported capability inventory. Add `GameEra.Red`, Red adapter/wire project, Host/DI/config registration, and route scaffolding for proto-supported endpoints.

### Phase 19: Red Capability Profile and Catalog Binding

Bind Red catalog/config data into shared AC15 catalog capabilities and define the Red capability profile. Keep this small: one capability-profile/catalog phase, not a broad rewrite.

### Phase 20: Red Runtime Capability Binding and Simple Compatibility

Bind shared identity, userdata, normal-play, crown, self-best, Dani, tutorial-only Tokkun, and simple compatibility capabilities to Red-owned rows and Red wire placement.

### Phase 21: Older-AC15 ChallengeCompe Capability and Red Binding

Define the shared older-AC15 ChallengeCompe capability and prove/bind it through Red evidence. Start with evidence and compatibility; only implement stateful challenge progress when the actual client contract is known.

### Phase 22: Red AdminApi/WebUI and Runtime Closeout

Expose Red-owned readback surfaces and implemented shared capability readback, run full automated verification, and close only after cabinet/RPCS3 smoke.

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
*Research corrected: 2026-06-13*
*Ready for requirements: yes*
