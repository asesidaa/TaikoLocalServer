# Feature Research

**Domain:** Yellow AC15 era support in TaikoLocalServer
**Researched:** 2026-06-07
**Confidence:** HIGH for proto-backed features, MEDIUM for wiki-only gameplay deltas

## Feature Landscape

### Table Stakes

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Yellow era foundation | Every supported era must be enableable and independently routable. | MEDIUM | Add `GameEra.Yellow`, adapter project, settings, generated wire DTOs, and route ownership tests. |
| Yellow catalog bootstrap | Cabinet responses depend on local music, medley, default-song, present, and specialty data. | MEDIUM | Use `Host/wwwroot/data/yellow/data/config/ST9100-1` plus shared AC15 loaders where shapes match. |
| Profile/login/userdata | Blue-equivalent normal support requires stable card/profile and save readback. | HIGH | Yellow-owned save table; shared identity only where already shared. |
| Normal playresult persistence | Normal score, crowns, favorites, recent songs, profile counters, and unlocks are the core cabinet loop. | HIGH | Prefer AC15 core services plus Yellow persistence adapter. |
| Self-best and crowns | Cabinet expects score/crown readback from prior plays. | MEDIUM | Reuse shared crown/self-best algorithms, map to Yellow wire placement. |
| Dani/Taikojuku | Yellow proto exposes `taikojuku.php` and Dan fields. | MEDIUM | Reuse AC15 Dani helpers with Yellow profile limits. |
| Item shop and medals | Wiki identifies Don/Katsu medals as a Yellow system change; proto exposes shop and medal upload fields. | HIGH | Yellow-owned medal/shop state; confirm exact response and spend behavior from proto/runtime. |
| WaiWai tutorial/logging | User clarified WaiWai is a folder/two-player chart surface, not a mode. | MEDIUM | Persist/read back tutorial flag if Yellow wire exposes it; log playresult extras and do not invent readback. |
| Tokkun | User confirmed Tokkun is real in Yellow; proto exposes tutorial and stage-result fields. | HIGH | Mirror Blue Tokkun boundaries but through Yellow-owned state. |
| Banacoin-adjacent compatibility | Yellow proto exposes balance/payment/error/info messages. | MEDIUM | Keep stateless compatibility unless concrete evidence proves stateful authority. |
| AdminApi/WebUI era routing | Existing admin UI should inspect Yellow profiles/scores once data exists. | MEDIUM | Preserve legacy routes and `/api/{era}/...` patterns. |

### Differentiators

| Feature | Value | Complexity | Notes |
|---------|-------|------------|-------|
| AC15 shared core | Enables Yellow without copying Blue/Green logic repeatedly. | HIGH | Include only where it directly supports Yellow and preserves separate state. |
| Evidence-tagged Yellow contract | Prevents Blue battle/Tokkun assumptions from leaking into Yellow. | MEDIUM | Record proto/data/wiki/runtime rows like previous Tokkun evidence work. |
| Runtime smoke checklist | Gives confidence beyond tests for a legacy cabinet protocol. | MEDIUM | Require cabinet/RPCS3 Yellow normal and Tokkun smoke evidence before closeout. |

### Anti-Features

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| Copy Blue battle mode into Yellow | Blue is close to Yellow and already implemented. | Yellow proto lacks Blue battle surface; copying creates fake support. | Treat battle as absent until Yellow-specific evidence appears. |
| Treat wiki as protocol authority | Wiki gives useful gameplay history. | It can describe gameplay not represented in this client/proto build. | Use wiki as scoping context; local proto/data/logs decide server behavior. |
| Persist Banacoin balances/payments | Payment routes exist in proto. | Repo is not an external payment authority. | Return compatibility success with logging unless runtime proof requires persistence. |
| Treat WaiWai as a mode | The name looks like a gameplay mode. | It is a folder/two-player-chart surface; special-mode branching would corrupt normal/Tokkun boundaries. | Tutorial flag persistence/readback plus playresult extra logging only. |
| Assume Blue/Green crown compression | Current Blue/Green controllers gzip crown bytes. | Older versions may use a different crown transport. | Prove Yellow crown compression/placement before mapping the response. |

## Feature Dependencies

- Era foundation -> wire generation -> route skeletons -> handler/mappers.
- Catalog bootstrap -> initial data, folder/telop/recommendation, shop, Dani.
- Yellow save schema -> userdata, normal playresult, item shop, Tokkun.
- AC15 core profiles/adapters -> shared self-best, crowns, Dani, shop, normal play services.
- Crown compression/placement proof -> Yellow crownsdata response mapping.
- WaiWai wire proof -> tutorial flag persistence/readback and playresult extra logging.
- Tokkun classifier and no-cross-write proof -> Tokkun persistence/readback.
- AdminApi/WebUI Yellow routing -> inspectable Yellow-owned persisted data.

## MVP Definition For v1.2

### Launch With

- [ ] Yellow adapter and host wiring.
- [ ] Yellow catalog bootstrap from `ST9100-1`.
- [ ] Yellow profile/login/userdata foundation.
- [ ] Yellow normal playresult, self-best, crowns, favorites, recent songs, and unlock readback.
- [ ] Yellow Dani/Taikojuku.
- [ ] Yellow item shop and Don/Katsu medal behavior.
- [ ] Yellow WaiWai tutorial flag write/readback and playresult extra logging where current Yellow evidence exposes fields.
- [ ] Yellow Tokkun acceptance, persistence, and tutorial readback.
- [ ] Yellow Banacoin-adjacent compatibility without wallet authority.
- [ ] AdminApi/WebUI Yellow routing for supported readback.
- [ ] Final automated and cabinet/RPCS3 smoke verification.

### Future Consideration

- [ ] Red support.
- [ ] Any WaiWai behavior beyond tutorial flag persistence/readback and playresult logging.
- [ ] Real Banacoin semantics if the repo is intentionally made a payment authority later.
- [ ] Any Yellow battle-like mode if evidence appears.

## Sources

- `.planning/PROJECT.md`
- `proto/yellow/yellow.proto`
- `Host/wwwroot/data/yellow/data/config/ST9100-1`
- Wiki AC15 Yellow section: https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/AC%E3%81%AE%E6%AD%B4%E5%8F%B2/AC15

---
*Feature research for: Yellow AC15 Support*
*Researched: 2026-06-07*
