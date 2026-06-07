# Pitfalls Research

**Domain:** Yellow AC15 era support in TaikoLocalServer
**Researched:** 2026-06-07
**Confidence:** HIGH for known repo pitfalls, MEDIUM for Yellow-only runtime quirks until cabinet evidence lands

## Critical Pitfalls

### Pitfall 1: Copying Blue Battle Into Yellow

**What goes wrong:** Yellow exposes battle routes, fields, tables, or source guards even though local Yellow proto evidence lacks the Blue battle surface.

**Why it happens:** Yellow is close to Blue, so a copy-first implementation can accidentally keep Blue-only behavior.

**How to avoid:** Start from `proto/yellow/yellow.proto` route/message inventory. Add source/route tests asserting Blue battle routes are absent for Yellow.

**Warning signs:** `BattleUserData`, `is_battleplay`, `ReleaseBattleData`, or battle route names appear in Yellow code without new Yellow evidence.

**Phase to address:** Foundation and route/evidence phase.

---

### Pitfall 2: Treating Wiki Gameplay Notes As Protocol Contract

**What goes wrong:** Server implements Wai Wai or medal behavior from wiki prose without matching local route/proto/log evidence.

**Why it happens:** Public history pages are useful and current enough for gameplay scoping, but they are not wire contracts.

**How to avoid:** Record wiki-derived items as scope leads. Promote them to requirements only when local proto, client logs, IDA, or cabinet/RPCS3 evidence supports a server behavior.

**Warning signs:** Requirements mention a feature not visible in Yellow proto or logs, with no explicit evidence-gap label.

**Phase to address:** Evidence/catalog phase and requirements review.

---

### Pitfall 3: Over-Generic AC15 Core

**What goes wrong:** A broad abstraction hides era-specific route, wire, and persistence boundaries or makes unsupported features appear supported.

**Why it happens:** Blue/Green/Yellow similarity invites a generic "AC15 era" implementation before differences are modeled.

**How to avoid:** Use capability profiles, wire placement, hooks, and typed era persistence adapters. Keep generated wire classes and EF entities era-owned.

**Warning signs:** Shared controllers, shared generated wire DTOs, shared save tables, or feature flags returning empty success for absent routes.

**Phase to address:** AC15 core foundation and every shared-service phase.

---

### Pitfall 4: Misclassifying Yellow Tokkun

**What goes wrong:** Tokkun uploads write normal score/crowns/shop/Dani/recent/favorite state, or Tokkun state is stored in Blue tables.

**Why it happens:** Tokkun payloads carry normal-looking fields, and Blue Tokkun implementation is tempting to reuse directly.

**How to avoid:** Add a Yellow Tokkun classifier before normal handling, persist only protocol-backed Tokkun tutorial/history facts, and assert no cross-mode writes.

**Warning signs:** Tokkun tests only assert result success, or Yellow Tokkun code calls normal save paths before classification.

**Phase to address:** Yellow Tokkun phase.

---

### Pitfall 5: Medal Semantics Drift

**What goes wrong:** Don/Katsu medals are treated like Blue shop medals or Banacoin balance, leading to wrong totals or cross-era state writes.

**Why it happens:** Yellow wiki says medals changed, while proto exposes multiple medal fields with limited response totals.

**How to avoid:** Separate Yellow medal state from shop purchases, normal play rewards, and Banacoin compatibility. Confirm spend/readback behavior with proto/runtime tests.

**Warning signs:** Katsu medals disappear from persistence, purchase responses invent Katsu totals not present in proto, or Banacoin routes mutate medal balances.

**Phase to address:** Item shop/medal phase.

## Looks Done But Is Not Checklist

- [ ] Yellow route tests prove only Yellow-supported endpoints exist.
- [ ] Yellow wire generation is adapter-local and generated files are not manually edited.
- [ ] Yellow catalog tests load `ST9100-1` data through path helpers.
- [ ] Yellow userdata, crowns, self-best, Dani, shop, and Tokkun tests assert Yellow-owned tables.
- [ ] Tokkun tests prove no normal, shop, battle, favorite, recent, Dani, or crown writes.
- [ ] Banacoin-adjacent routes are no-state unless new evidence proves otherwise.
- [ ] Final closeout includes cabinet/RPCS3 smoke evidence for Yellow normal and Tokkun flows.

## Pitfall-To-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Blue battle copied into Yellow | Foundation/evidence | Route absence and proto inventory tests. |
| Wiki treated as protocol contract | Evidence/catalog | Evidence matrix labels wiki-only items as unimplemented gaps. |
| Over-generic AC15 core | AC15 core foundation | Profile/adapter contract tests and no shared EF table. |
| Tokkun cross-writes | Tokkun runtime | SQLite no-write assertions and tutorial/history readback tests. |
| Medal semantics drift | Shop/medal | Yellow medal persistence tests and purchase response shape tests. |

## Sources

- `.planning/PROJECT.md`
- `proto/yellow/yellow.proto`
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md`
- Memory-backed lessons from prior Blue Tokkun and AC15 planning work, rechecked against current local planning files.

---
*Pitfalls research for: Yellow AC15 Support*
*Researched: 2026-06-07*
