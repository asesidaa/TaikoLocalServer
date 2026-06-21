# Phase 24 Context: White Catalog Profile and Protocol Limits

## Goal

Bind White `ST7100-1` catalog data, committed sidecar contracts, AC15 profile, feature flags, protocol limits, and catalog accessors so later White runtime handlers can reuse proven AC15 services.

## Decisions

- **D-24-01:** White active operator data root is `Host/wwwroot/data/white/data/config/ST7100-1`, with required `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin`.
- **D-24-02:** White catalog loading may reuse shared AC15 XML/sidecar/customization loaders where the file shape matches Red/Yellow. White still owns `IWhiteCatalog`, `WhiteEraGameDataCatalog`, path constants, and sidecar file names.
- **D-24-03:** White Phase 24 feature flags mirror Red's non-shop normal AC15 surface: normal play, userdata, self-best, crowns, initial data, folders, telops, recommendations, Taikojuku, and Dani are supported; item shop is disabled.
- **D-24-04:** Do not add White battle, Tokkun, WaiWai, gacha, Banacoin, reward shop, or Don Challenge behavior in Phase 24.
- **D-24-05:** White gets a distinct `CreateWhiteLimits()` factory even where it currently delegates to common AC15 limits. The binary/IDB string pass found descriptor fields but did not expose a quick cap constant, so future IDA evidence can update White without changing Blue/Green/Yellow/Red.
- **D-24-06:** Server-authored sidecar data for implemented White metadata surfaces must exist under `Host/wwwroot/data/white` and be copied by `Host.csproj`, even when the intentional default is empty.

## Stop Rule

Manual RPCS3/cabinet and WebUI verification is reserved for Phase 27 closeout per user instruction. Phase 24 verification is automated build/test only.
