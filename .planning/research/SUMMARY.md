# Project Research Summary

**Project:** TaikoLocalServer AC15 Era Support
**Domain:** Yellow AC15 era support
**Researched:** 2026-06-07
**Confidence:** HIGH for repo/proto/data-backed scope, MEDIUM for wiki-only Yellow gameplay deltas

## Executive Summary

Yellow support should be planned as a first-class AC15 era, not as a Blue flag. The implementation should reuse the approved AC15 shared-core design where it directly enables Yellow, but Yellow still needs separate routes, generated wire DTOs, mappers, persistence tables, catalog contracts, tests, and AdminApi/WebUI routing.

The local Yellow proto and data establish the main server shape: Yellow has Tokkun tutorial/stage fields, item shop and Banacoin-adjacent messages, Don/Katsu medal upload fields, and `ST9100-1` catalog data. Blue battle behavior is absent in the Yellow proto and must not be mirrored into Yellow. Wiki context is useful for broad gameplay deltas, especially Don/Katsu medals and the later WaiWai folder/chart addition, but local proto/log/runtime evidence remains the protocol authority.

User clarification narrows WaiWai: it is not a mode. Where Yellow evidence exposes WaiWai fields, the server should persist/read back only the tutorial flag and log additional playresult fields that are not read back. Crown data also needs an explicit Yellow encoding check because older-version crown compression may differ from the current Blue/Green gzip response path.

## Key Findings

### Recommended Stack

No new stack is needed. Use existing .NET 10, ASP.NET Core controllers, protobuf-net wire generation, Mediator partial handlers, EF Core SQLite, AC15 catalog loaders, xUnit tests, and MudBlazor WebUI/AdminApi routing.

### Expected Features

**Must have:**
- Yellow era foundation: generated wire DTOs, adapter, settings, route tests, direct-protobuf transport.
- Yellow catalog bootstrap from `Host/wwwroot/data/yellow/data/config/ST9100-1`.
- Yellow-owned profile, userdata, normal playresult, self-best, crowns, favorites, recent songs, Dani, shop/medals, WaiWai tutorial/logging, Tokkun, Banacoin-adjacent compatibility, and AdminApi/WebUI support.
- Explicit absence of Yellow battle behavior.

**Should have:**
- AC15 shared-core extraction for modules Yellow needs: profiles, byte helpers, crowns, self-best, Dani, item shop, userdata, normal play, and special-mode hooks.
- Evidence matrix for Yellow proto/data/wiki/runtime rows.

**Defer:**
- Red support.
- Any WaiWai behavior beyond tutorial flag persistence/readback and playresult logging.
- Real Banacoin wallet/payment semantics.
- Any Yellow battle-like behavior unless concrete Yellow evidence appears.

### Architecture Approach

Use era-owned outer adapters and typed AC15 core services behind canonical DTOs. Yellow controllers own route, transport, logging, and wire DTO mapping. Mediator handlers dispatch to `.Yellow.cs` partials, which resolve a Yellow AC15 profile plus Yellow persistence/catalog adapters. Shared AC15 services handle behavior only where the profile says Yellow supports it.

### Critical Pitfalls

1. **Copying Blue battle into Yellow** - prevent with route absence tests and proto inventory.
2. **Treating wiki notes as wire contract** - keep wiki items bounded by local proof; WaiWai is tutorial/logging only, not a mode.
3. **Over-generic AC15 core** - use typed profiles/adapters; no shared generated wire or EF table.
4. **Tokkun cross-writes** - classify before normal handling and assert no normal/shop/Dani/favorite/recent/crown writes.
5. **Medal semantics drift** - keep Yellow Don/Katsu medal state separate from Banacoin and Blue shop assumptions.
6. **Crown compression assumption** - test Yellow crown response encoding before choosing gzip/raw mapping.

## Implications For Roadmap

### Phase 12: Yellow Evidence And Era Foundation

**Rationale:** Route/version and proto boundaries must be proven before runtime code expands.
**Delivers:** Yellow route inventory, adapter skeleton, wire generation, `GameEra.Yellow`, settings, route absence tests for battle, and startup/version assumption proof.
**Avoids:** Route prefix guessing and Blue battle leakage.

### Phase 13: Yellow Catalog And AC15 Core Foundation

**Rationale:** Catalog readback and shared AC15 profile contracts are prerequisites for most runtime routes.
**Delivers:** Yellow catalog over `ST9100-1`, AC15 profile/limits/wire-placement contracts, shared loaders where valid, and tests.
**Avoids:** Hardcoded data paths and over-generic core.

### Phase 14: Yellow Identity, Userdata, Crowns, Self-Best, And Normal Play

**Rationale:** This is the core cabinet loop and should land before shops or Tokkun.
**Delivers:** Yellow-owned save/score/best/favorite/recent persistence, userdata readback, normal playresult persistence, self-best, crowns, and exact crown response encoding proof.

### Phase 15: Yellow Dani, Shop, Medals, Metadata, And AdminApi/WebUI

**Rationale:** These features depend on catalog and save state but are distinct from Tokkun.
**Delivers:** Taikojuku/Dani, getfolder/telop/recommend/tournament/gacha/challenge surfaces, item shop, Don/Katsu medal state, WaiWai tutorial flag/readback plus playresult logging if exposed, and Yellow admin readback.

### Phase 16: Yellow Tokkun And Banacoin Compatibility

**Rationale:** Tokkun has special no-cross-write boundaries and Banacoin-adjacent compatibility should stay stateless.
**Delivers:** Yellow Tokkun classifier, tutorial/history persistence, userdata tutorial readback, no-cross-write tests, and Banacoin compatibility routes.

### Phase 17: Yellow Runtime Verification And Contract Closeout

**Rationale:** The milestone should close only after automated proof and cabinet/RPCS3 smoke evidence.
**Delivers:** Final Yellow contract docs, full test/build verification, runtime smoke record, and requirement traceability closeout.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Existing repo stack is the right implementation stack. |
| Features | HIGH/MEDIUM | Proto-backed features are high confidence; WaiWai is bounded to tutorial/logging pending Yellow-specific field proof. |
| Architecture | HIGH | Existing Blue/Green patterns and approved AC15 core design align with Yellow work. |
| Pitfalls | HIGH | Prior Blue Tokkun/battle work exposed the exact failure modes to avoid. |

**Overall confidence:** HIGH for milestone planning, with route prefix, Yellow WaiWai field placement, and crown response encoding explicitly left as evidence gaps.

## Sources

### Primary

- `.planning/PROJECT.md`
- `proto/yellow/yellow.proto`
- `Host/wwwroot/data/yellow/data/config/ST9100-1`
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md`
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/README.md`

### Secondary

- Wiki AC15 Yellow section: https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/AC%E3%81%AE%E6%AD%B4%E5%8F%B2/AC15

---
*Research completed: 2026-06-07*
*Ready for roadmap: yes*
