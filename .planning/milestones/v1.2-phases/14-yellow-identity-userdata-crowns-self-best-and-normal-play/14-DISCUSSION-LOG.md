# Phase 14: Yellow Identity, Userdata, Crowns, Self-Best, and Normal Play - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md - this log preserves the alternatives considered.

**Date:** 2026-06-08
**Phase:** 14-Yellow Identity, Userdata, Crowns, Self-Best, and Normal Play
**Areas discussed:** Identity and default save state, Userdata readback contract, Normal playresult side effects, Self-best and crown encoding proof

---

## Area Selection

| Option | Description | Selected |
|--------|-------------|----------|
| Identity and default save state | BAID/mydon create/find behavior, Yellow-owned default profile tables, and shared identity vs gameplay-state boundaries. | Yes |
| Userdata readback contract | Supported Yellow profile/settings/unlock/tutorial/favorite/recent fields now, and which fields stay deferred to Phases 15-16. | Yes |
| Normal playresult side effects | Which normal upload fields update Yellow history, best rows, counters, unlocks, favorites, and recent songs, while blocking cross-era and special-mode writes. | Yes |
| Self-best and crown encoding proof | Yellow normal/Ura/Shin self-best support and exact `crownsdata.php` placement/encoding/compression proof required by YCRN-01. | Yes |

**User's choice:** Cover all four areas.
**Notes:** Coordinator rationale: all four correspond directly to Phase 14 success criteria and requirements, so narrowing would risk under-scoping the phase.

---

## Identity and Default Save State

| Option | Description | Selected |
|--------|-------------|----------|
| Yellow-owned runtime state | Reuse shared card/access-code identity only, but create/read all Yellow gameplay save state in Yellow-owned entities/tables and handler partials. | Yes |
| Green/Blue state reuse | Point Yellow BAID/userdata at existing Green or Blue save data. | No |
| No-state compatibility | Keep BAID/mydon as result-only scaffold until later. | No |

**User's choice:** Yellow-owned runtime state.
**Notes:** Selected as the workflow default because it directly satisfies YUSR-01 and the repo state-separation rule. BAID and mydon should replace Phase 12 no-state scaffolds with Mediator-backed Yellow behavior.

---

## Userdata Readback Contract

| Option | Description | Selected |
|--------|-------------|----------|
| Shared AC15 userdata service plus Yellow adapter | Build a Yellow save snapshot and use `Ac15UserDataService` where protocol fields match, with adapter-local Yellow response mapping. | Yes |
| Fully hand-written Yellow userdata | Build every field directly in the Yellow controller or handler. | No |
| Minimal result-only userdata | Return success with only required fields and defer readback. | No |

**User's choice:** Shared AC15 userdata service plus Yellow adapter.
**Notes:** Selected as the default because Phase 13 already established Yellow AC15 profile/core contracts and Phase 14 needs readback rather than a scaffold. Tokkun-specific readback semantics remain Phase 16.

---

## Normal Playresult Side Effects

| Option | Description | Selected |
|--------|-------------|----------|
| Yellow-owned normal-play persistence through shared AC15 core | Map Yellow playresult to common DTOs, validate normal mode, persist Yellow history/best/counters/unlocks/favorites/recent through Yellow tables, and use shared AC15 normal-play service where it fits. | Yes |
| Copy Green/Blue handler wholesale | Duplicate an existing era handler and adjust field names later. | No |
| Accept uploads without writes | Keep `playresult.php` as log-and-success for Phase 14. | No |

**User's choice:** Yellow-owned normal-play persistence through shared AC15 core.
**Notes:** Selected as the default because it satisfies YPLY-01 while preserving no-cross-era-write constraints. Phase 15 shop purchase/medal spend and Phase 16 Tokkun classification are explicitly excluded.

---

## Self-Best and Crown Encoding Proof

| Option | Description | Selected |
|--------|-------------|----------|
| Shared packing with Yellow wire proof | Use shared `Ac15SelfBestService`/`Ac15CrownService` but add Yellow mapper tests and raw protobuf/compression tests proving exact Yellow response placement and encoding. | Yes |
| Assume Blue/Green gzip behavior | Reuse Blue/Green crown route behavior without Yellow-specific raw byte proof. | No |
| Defer crown proof to runtime verification | Implement self-best only and leave crown encoding to Phase 17. | No |

**User's choice:** Shared packing with Yellow wire proof.
**Notes:** Selected as the default because YCRN-01 explicitly requires crown encoding proof in Phase 14. The context requires an explicit compression-vs-raw assertion and raw `hash_crown_flg` placement test.

---

## the agent's Discretion

- Use the smallest Yellow entity/table/handler/mapper surface that satisfies Phase 14 requirements.
- Extract shared AC15 helpers only when they reduce real duplication and preserve Yellow-owned routes, wire DTOs, and persistence.
- Keep detailed verification planning for the plan stage; this discuss stage only captures decisions and context.

## Deferred Ideas

- Phase 15 owns Yellow Dani, item shop purchases, medal spend/use accounting, WaiWai tutorial/logging semantics, and AdminApi/WebUI readback.
- Phase 16 owns Yellow Tokkun classification, no-cross-mode persistence, Tokkun tutorial/history readback, and Banacoin-adjacent compatibility.
- Phase 17 owns Yellow cabinet/RPCS3 smoke and final contract closeout.
- Yellow battle remains absent unless new concrete evidence appears.
