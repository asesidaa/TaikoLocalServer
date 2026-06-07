# Stack Research

**Domain:** Yellow AC15 era support in TaikoLocalServer
**Researched:** 2026-06-07
**Confidence:** HIGH for repo stack, MEDIUM for Yellow route/client quirks until runtime evidence lands

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| .NET / ASP.NET Core | 10 | Host, controller adapters, dependency injection | Existing host stack; no new runtime stack needed for Yellow. |
| EF Core SQLite | Current repo version | Yellow-owned persistence tables and migrations | Matches Blue/Green persistence model while keeping era state separate. |
| protobuf-net | Current repo version | Generate Yellow wire DTOs from `proto/yellow/yellow.proto` | Existing adapters already use generated protobuf DTOs per era. |
| Mediator.SourceGenerator | Current repo version | Dispatch request handlers with era-specific partials | Fits the current Blue/Green handler pattern. |
| MudBlazor WebAssembly | Current repo version | Admin UI era routing and readback | Existing WebUI/AdminApi parity work should extend to Yellow. |

### Supporting Libraries And Assets

| Asset | Purpose | When to Use |
|-------|---------|-------------|
| `proto/yellow/yellow.proto` | Yellow protocol source | Generate adapter-local `Wire/` classes and prove route/message support. |
| `proto/yellow/vsinterface.proto` | Yellow startup/version wire source | Compare against shared `/v01r00/chassis/*` assumptions. |
| `Host/wwwroot/data/yellow/data/config/ST9100-1` | Yellow normal catalog source | Load `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, and related files. |
| `Application/Catalog/Ac15` and `Infrastructure/GameDataCatalog/Ac15` | Shared AC15 loader foundation | Reuse only where Yellow file shapes match Blue/Green formats. |
| `Application/Ac15` planned core | Shared AC15 gameplay algorithms | Implement where it directly enables Yellow without merging era state. |

## What Not To Add

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| New framework or service stack | Yellow support is protocol/catalog work inside the existing host. | Extend existing adapter, handler, catalog, and EF patterns. |
| Shared `Ac15SaveData` EF table | Violates the established era-state separation rule. | Yellow-specific tables plus typed AC15 persistence adapters. |
| Generated shared AC15 wire assembly | Generated DTOs differ by era and route ownership must remain explicit. | Adapter-local Yellow `Wire/` output and Yellow mappers. |
| Battle compatibility stubs | `proto/yellow/yellow.proto` lacks Blue battle messages/routes. | Do not expose Yellow battle routes unless Yellow evidence appears. |
| Real Banacoin wallet/payment authority | Repo is not a Banacoin authority. | Stateless compatibility unless concrete Yellow evidence requires more. |

## Version And Compatibility Notes

- The local Yellow config root is `ST9100-1`; do not infer an HTTP route prefix from that alone.
- Yellow `GetitemshopinfoResponse` differs from Blue by omitting Blue shop timing fields in the observed proto.
- Yellow `PlayResultRequest` includes `get_donmedal` and `get_katsumedal`; purchase response exposes Don-medal totals only in the observed proto.
- Yellow has `tokkun_tutorial_flg` and `ary_tokkunstage_info`; it does not have Blue battle userdata messages.
- Wiki context says Yellow later added "Issho ni Wai Wai Ensou", but the local Yellow proto examined here does not expose Blue `waiwai_*` fields. Treat Wai Wai as an evidence gap until route/proto/log evidence appears.

## Sources

- `.planning/PROJECT.md` - approved v1.2 milestone scope and evidence hierarchy.
- `proto/yellow/yellow.proto` - Yellow protocol surface.
- `Host/wwwroot/data/yellow/data/config/ST9100-1` - Yellow catalog data root.
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - approved AC15 core boundary.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/README.md` - staged implementation plan for shared AC15 core.
- Wiki AC15 Yellow section - gameplay scoping context: https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/AC%E3%81%AE%E6%AD%B4%E5%8F%B2/AC15

---
*Stack research for: Yellow AC15 Support*
*Researched: 2026-06-07*
