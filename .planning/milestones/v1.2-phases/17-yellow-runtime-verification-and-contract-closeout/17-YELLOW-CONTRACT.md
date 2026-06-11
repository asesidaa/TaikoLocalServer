# Yellow AC15 Final Contract

**Milestone:** v1.2 Yellow AC15 Support
**Status:** Shipped 2026-06-12

## Supported Runtime Surface

- Yellow game routes are served by the Yellow adapter under `/v09r02/chassis/*`.
- Shared AC15 startup and version routes remain owned by `/v01r00/chassis/*`.
- Yellow game endpoints use direct protobuf request/response bodies.
- Yellow is enableable through Host era settings and disabled Yellow routes stay absent.
- Yellow catalog data loads from operator-supplied `Host/wwwroot/data/yellow/data` source checkouts or `wwwroot/data/yellow/data` published folders.

## Supported State

- Yellow card/profile/default save state is Yellow-owned.
- Yellow userdata, self-best, crowns, recent songs, favorite songs, profile counters, and normal play history use Yellow-owned rows.
- Yellow Dani/Taikojuku score and stage-score state is Yellow-owned.
- Yellow item-shop season, item purchase, unlock, and Don/Katsu medal state is Yellow-owned.
- Yellow WaiWai handling is limited to current protocol-backed tutorial/readback and playresult logging behavior; WaiWai is not a special play mode.
- Yellow Tokkun playresult handling runs before normal play handling and persists only protocol-backed nullable tutorial state plus append-only raw stage history.
- Yellow Banacoin-adjacent routes log requests and return compatibility success without wallet/payment authority.

## Explicit Non-Goals

- No Yellow battle route, battle fields, battle persistence, or Blue battle fallback.
- No shared AC15 generated wire assembly.
- No shared AC15 EF save table or cross-era gameplay persistence.
- No real Banacoin balance, payment, coupon, settlement, receipt, BNID result, deduction, or transaction-history authority.
- No Tokkun score, crown, Dani, favorite, recent, shop, medal, profile, battle, unlock, reward, paid-coin, practice-time, jump-point, autoplay, or speed-change side effects.
- No runtime behavior derived only from public wiki or official pages.

## Evidence Gaps

- Raw RPCS3 smoke logs were not committed during closeout; Phase 17 records user-confirmed external runtime verification.
- Yellow battle remains unsupported unless future local proto/log/client evidence proves a server-facing contract.
- WaiWai behavior beyond tutorial/readback and playresult logging remains future work unless Yellow-specific evidence proves more.
- Real Banacoin authority remains out of scope unless the repo intentionally becomes a Banacoin authority and receives concrete client/protocol evidence.

## Operator Data Expectations

- Yellow normal catalog data comes from `config/ST9100-1/musicinfo.xml`, `config/ST9100-1/musicmedleyinfo.xml`, `defmusic.bin`, and related local Yellow AC15 data.
- Committed Yellow server-authored sidecars under `Host/wwwroot/data/yellow/` must be copied to runtime output even when a given sidecar is intentionally empty.
- Published deployments should preserve the same data layout under `wwwroot/data/yellow/`.
- If `Host/bin/Debug/net10.0` is locked by a running server, use a temp-output Host build for verification.

