# Blue Support Roadmap - Design

**Date:** 2026-05-27
**Status:** high-level roadmap for future stage specs and plans
**Scope:** Break Blue support into serial work packages with evidence references. This is not an implementation plan.

## Purpose

Add proper Taiko no Tatsujin AC15 Blue support without forcing the whole effort into one implementation context. Future sessions should use this document as the entry point, then write a focused design/spec and implementation plan for one stage at a time.

The first delivery target is Blue normal cabinet support: enso, Dani Dojo, card/profile flow, self-best/crowns, rewards/unlocks, item shop, and basic AdminApi/WebUI parity with the Green-era surfaces already in this repo. Blue battle mode is a later delivery track. Tokkun and Banacoin/payment support are out of scope for Blue.

## Evidence Base

### Local Proto Inputs

- `proto/blue/taiko.proto`
- `proto/blue/vsinterface.proto`
- `proto/green/green.proto`
- `proto/green/vsinterface.proto`

The Blue and Green `vsinterface.proto` files are identical, so startup/verup wire types should remain shared unless client traffic proves a Blue-specific route or field behavior.

Most top-level Blue/Green game messages are shared:

- bookkeeping, heartbeat, telop, folder, taikojuku
- tournament, item shop, BAID, mydon entry, userdata
- playresult, selfbest, crowns, recommend, headclerk2, reitai
- reward card check, reward execution, challenge competition, item purchase

Blue-only protocol messages:

- `CoinsettingRequest` / `CoinsettingResponse`
- `BalancecheckRequest` / `BalancecheckResponse`
- `BanacoinpaymentRequest` / `BanacoinpaymentResponse`
- `BanacoinerrorlogRequest` / `BanacoinerrorlogResponse`
- `GetbanacoininfoRequest` / `GetbanacoininfoResponse`
- `BattleUserDataRequest` / `BattleUserDataResponse`

Green-only protocol messages:

- `GetghostdataRequest` / `GetghostdataResponse`
- `GetghostscoreRequest` / `GetghostscoreResponse`
- `PlayResultDataRequest`

Important shape differences:

- Blue `PlayResultRequest` carries the normal playresult fields directly.
- Green `PlayResultRequest` has only `baid_conf`, `chassis_id_conf`, `shop_id_conf`, `play_datetime_conf`, and compressed `playresult_data`.
- Blue playresult stage data has `BattleStageData`; Green playresult stage data has `GhostStageData`.
- Blue `InitialdatacheckResponse` has legal-terms and battle fields; Green has `is_ghostbattleplay`.
- Blue `UserDataResponse` includes `tokkun_tutorial_flg`, but Blue support should not implement Tokkun behavior because Tokkun had already ended before Blue service.

### Public Gameplay References

- Blue version page: <https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%83%96%E3%83%AB%E3%83%BC>
- Green version page: <https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%82%B0%E3%83%AA%E3%83%BC%E3%83%B3>
- Tokkun mode reference: <https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/%E5%9F%BA%E6%9C%AC%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0#tokkunmode>
- Blue battle mode reference: <https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB>
- Green AI battle reference: <https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/AI%E3%83%90%E3%83%88%E3%83%AB%E6%BC%94%E5%A5%8F/%E3%82%B0%E3%83%AA%E3%83%BC%E3%83%B3>

Gameplay facts to carry forward:

- Blue ran before Green and used Enso Battle; Green replaced that with AI Battle.
- Tokkun was implemented in White and ended when Yellow ended; it should not be part of Blue support.
- Blue item shop seasons are visible on the Blue wiki page and should be handled as Blue data, not reused Green season data.

### Existing Repo References

- Era enum: `Domain/Enums/GameEra.cs`
- Era settings: `Host/Configurations/ServerSettings.json`
- Shared startup route and wire types: `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`, `Adapters.GameProtocol.Shared/Wire/StartupAuth.cs`
- Green adapter pattern: `Adapters.GameProtocol.Green/`
- Green wire types: `Adapters.GameProtocol.Green/Wire/Game.cs`
- Green controllers: `Adapters.GameProtocol.Green/Controllers/`
- Green mappers: `Adapters.GameProtocol.Green/Mappers/`
- Green playresult transport boundary: `Adapters.GameProtocol.Green/GreenPlayResultPayloadDecoder.cs`
- Common DTO era splits: `Application/Dtos/CommonBaidResponse.Green.cs`, `Application/Dtos/CommonPlayResultData.Green.cs`, `Application/Dtos/CommonUserDataResponse.Green.cs`
- Green handler pattern: `Application/Handlers/*.Green.cs`
- Green catalog pattern: `Infrastructure/GameDataCatalog/Green/`
- Admin/WebUI era routing: `Adapters.AdminApi/`, `Contracts.AdminApi/`, `TaikoWebUI/`
- Prior Green specs/plans:
  - `docs/superpowers/specs/2026-05-12-green-version-support-design.md`
  - `docs/superpowers/specs/2026-05-12-green-real-support-design.md`
  - `docs/superpowers/specs/2026-05-16-green-webui-support-design.md`
  - `docs/superpowers/specs/2026-05-17-green-customization-design.md`
  - `docs/superpowers/specs/2026-05-26-green-item-shop-support-design.md`
  - `docs/superpowers/plans/2026-05-16-green-webui-support/`
  - `docs/superpowers/plans/2026-05-17-green-customization/`
  - `docs/superpowers/plans/2026-05-26-green-item-shop-support/`

## Recommended Delivery Shape

Treat Blue as its own era, not as a Green flag. The implementation should follow the existing era architecture: `GameEra`, one game-protocol adapter, per-era `Common*` DTO partials, per-era handlers, per-era persistence, per-era catalog loading, and era-aware AdminApi/WebUI routing.

The work should be split into two major tracks:

- **Track A: Blue normal support.** This is the first user-visible target.
- **Track B: Blue battle mode.** This starts only after Track A is stable enough for normal cabinet smoke testing.

Within Track A, split the work into serial stages so each future session has one clear design/spec and one implementation plan.

## Track A - Blue Normal Support

### Stage A0 - Evidence And Bootstrap Spec

Goal: confirm the minimum facts needed before writing code.

Future spec should cover:

- Blue route prefix and route ownership.
- Whether Blue uses the same missing-content-type protobuf workaround as Green.
- Whether Blue game requests are gzip/header-wrapped per endpoint.
- Whether `playresult.php` is direct protobuf, gzip-wrapped direct protobuf, or otherwise framed.
- Whether Blue `vsinterface.proto` can use the existing shared startup route unchanged.
- Which ignored local data directories/files exist for Blue, and their expected release/source layout.

References:

- `proto/blue/taiko.proto`
- `proto/blue/vsinterface.proto`
- `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`
- `Host/Program.cs`
- existing Green cabinet evidence notes under `proto/green/ida-byte-field-findings.md`

Exit criteria:

- A short stage spec lists confirmed transport/framing assumptions and explicitly marks any unknowns that require cabinet logs or IDA before implementation.

### Stage A1 - Era Foundation And Adapter Skeleton

Goal: add Blue as a first-class era with route ownership, generated wire types, settings, dependency injection, and empty success-shaped controller flow.

Future spec should cover:

- `GameEra.Blue` and `ServerSettings:Eras:Blue`.
- `Adapters.GameProtocol.Blue` project layout.
- Blue generated protobuf types under a Blue namespace.
- Shared startup/verup behavior through `Adapters.GameProtocol.Shared` where possible.
- Controller list from `proto/blue/taiko.proto`.
- Explicit no-op or ignored handling for Tokkun/Banacoin endpoints if the cabinet calls them unexpectedly.

Do not design battle internals in this stage. Battle route ownership can exist, but real battle state belongs to Track B.

References:

- `docs/superpowers/specs/2026-05-12-green-version-support-design.md`
- `Adapters.GameProtocol.Green/Adapters.GameProtocol.Green.csproj`
- `Adapters.GameProtocol.Green/Controllers/`
- `Adapters.GameProtocol.Green/Wire/Game.cs`
- `proto/blue/taiko.proto`

Exit criteria:

- Blue adapter can be enabled/disabled independently.
- Blue routes are discoverable when enabled and absent when disabled.
- Future normal stages can replace success-shaped responses with real handler calls.

### Stage A2 - Blue Catalog And Data Layout

Goal: define Blue catalog sources and required runtime data paths before normal handlers depend on them.

Future spec should cover:

- Blue equivalent of `IGreenCatalog`, likely `IBlueCatalog`.
- Blue game data path under `wwwroot/data/blue/...`.
- Required datatables for normal song list, music metadata, Dani/taikojuku, telop, folders, customization, and item shop.
- Whether Blue can reuse Green XML/datatable parsers or needs separate loaders.
- Default behavior for optional data files.
- Operator docs for supplying ignored Blue game files.

References:

- `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
- `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs`
- `Infrastructure/GameDataCatalog/Green/GreenTaikojukuLoader.cs`
- `Infrastructure/GameDataCatalog/Green/GreenRequiredDataFiles.cs`
- `README.md`
- `Host/README.md`

Exit criteria:

- Later stages have named Blue catalog interfaces and source file expectations.
- Missing required files and optional files have clear startup behavior.

### Stage A3 - Identity, Profile, Initial Data, And User Data

Goal: make card registration/login and profile readback work for Blue.

Future spec should cover:

- Blue default save data.
- `baidcheck.php`, `mydonentry.php`, `initialdatacheck.php`, and `userdata.php`.
- Shared identity rows versus Blue-specific save rows.
- Blue unlock bitsets and fixed-width byte fields.
- Blue legal-terms fields in `InitialdatacheckResponse`.
- Blue profile fields that match Green exactly versus fields that need Blue-specific DTO partials.
- Optional-field omission rules to avoid sending unsafe zero defaults.

References:

- `Application/Handlers/BaidQuery.Green.cs`
- `Application/Handlers/AddMyDonEntryCommand.Green.cs`
- `Application/Handlers/GetInitialDataQuery.Green.cs`
- `Application/Handlers/UserDataQuery.Green.cs`
- `Adapters.GameProtocol.Green/Mappers/BaidResponseMapper.cs`
- `Adapters.GameProtocol.Green/Mappers/InitialDataMappers.cs`
- `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs`
- `Application/Common/UserSaveDataGreenExtensions.cs`
- `proto/blue/taiko.proto` messages `BAID*`, `MydonEntry*`, `Initialdatacheck*`, `UserData*`

Exit criteria:

- A Blue card can register, log in again, and receive a stable profile/userdata response.
- Tokkun fields are either omitted/defaulted harmlessly or logged, with no Tokkun behavior.

### Stage A4 - Enso Play Result, Score Readback, Crowns, And Rewards

Goal: support normal Blue song play persistence and readback.

Future spec should cover:

- Blue direct `PlayResultRequest` mapping.
- Normal-stage result persistence.
- Best score and crown update rules.
- `selfbest.php` and `crownsdata.php`.
- Favorites/recent songs if present.
- reward execution and reward card check.
- Title, tone, costume, song unlock arrays.
- What to log and defer for unclear fields.

Do not include battle-stage persistence except to preserve/ignore/log it safely when normal mode sends no battle data.

References:

- `Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs`
- `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- `Application/Handlers/GetSelfBestQuery.Green.cs`
- `Application/Handlers/GetCrownsQuery.Green.cs` if present, otherwise current crowns handler path
- `Application/Handlers/RewardExecutionCommand.Green.cs`
- `Application/Handlers/RewardCardCheckQuery.Green.cs`
- `Adapters.GameProtocol.Green/GreenPlayResultPayloadDecoder.cs` as a cautionary transport-boundary example
- `proto/blue/taiko.proto` messages `PlayResult*`, `SelfBest*`, `CrownsData*`, `Reward*`

Exit criteria:

- A Blue normal song result persists.
- Self-best and crown readback reflect normal Blue play.
- Reward unlock arrays update Blue save state.

### Stage A5 - Dani Dojo

Goal: support Blue Dani Dojo at the same product level currently expected for Green.

Future spec should cover:

- Blue Dani/taikojuku catalog source.
- Blue playresult fields used for Dani completion.
- Blue save fields and flags for Dan state.
- AdminApi readback.
- WebUI Dani page behavior.
- Differences from Green Dani, especially whether Blue has fields Green lacks or vice versa.

References:

- `docs/superpowers/specs/2026-05-15-green-dani-flow-design.md`
- `docs/superpowers/plans/2026-05-15-green-dani-flow/`
- `Application/Handlers/UpdatePlayResultCommand.Green.cs`
- `Application/Handlers/GetDanScoreQuery.Green.cs`
- `Application/Handlers/GetDanOdaiQuery.Green.cs`
- `TaikoWebUI/Pages/DaniDojo.razor`
- `proto/blue/taiko.proto` messages `Taikojuku*`, `PlayResult*`, `BAIDResponse`

Exit criteria:

- Blue Dan progress can be saved and displayed.
- The WebUI does not conflate Blue with Green or Nijiiro Dan semantics.

### Stage A6 - Blue Item Shop And Unlocking

Goal: support Blue item shop behavior using the Green shop work as the nearest local model, but with Blue season data and Blue save state.

Future spec should cover:

- Blue shop settings and active season selection.
- Blue shop JSON shape and official-data curation path.
- Whether Blue `item_type` mapping matches the proven Green mapping.
- Season-scoped Don medal state.
- Purchase flow and reward execution flow.
- Locking configured shop items until purchase/reward execution unlocks them.
- Blue shop seasons from the Blue wiki page and official sources.

References:

- `docs/superpowers/specs/2026-05-26-green-item-shop-support-design.md`
- `docs/superpowers/plans/2026-05-26-green-item-shop-support/`
- `Infrastructure/GameDataCatalog/Green/GreenItemShopLoader.cs`
- `Application/Handlers/GetItemShopInfoQuery.Green.cs`
- `Application/Handlers/ItemPurchaseCommand.Green.cs`
- `Application/Handlers/RewardExecutionCommand.Green.cs`
- `Adapters.GameProtocol.Green/Mappers/ItemShopMappers.cs`
- `proto/blue/taiko.proto` messages `Getitemshopinfo*`, `Itempurchase*`, `Rewardexecution*`

Exit criteria:

- Blue advertises one active shop season.
- Blue purchase and reward execution unlock configured items.
- Blue medals and shop state do not reuse Green rows.

### Stage A7 - Basic AdminApi And WebUI Parity

Goal: make Blue visible and manageable through the same basic surfaces already available for Green.

Future spec should cover:

- Era routing for Blue in AdminApi.
- Profile, score history, favorites, Dani, customization, and item-shop relevant surfaces.
- WebUI era selector changes.
- Blue customization preview reuse if assets and catalogs are available.
- Read-only versus editable surfaces for first release.

References:

- `docs/superpowers/specs/2026-05-16-green-webui-support-design.md`
- `docs/superpowers/plans/2026-05-16-green-webui-support/`
- `Adapters.AdminApi/Controllers/`
- `Contracts.AdminApi/`
- `TaikoWebUI/Utilities/WebUiEra.cs`
- `TaikoWebUI/Pages/Profile.razor`
- `TaikoWebUI/Shared/Customize/`

Exit criteria:

- Blue users can be inspected in the WebUI.
- Basic settings/edit flows do not accidentally write Green save state.

### Stage A8 - Normal-Mode Cabinet Smoke And Hardening

Goal: close Track A with repeatable cabinet-level checks and documented unresolved items.

Future spec should cover:

- Cabinet boot with Blue enabled and other eras disabled.
- New card registration.
- Known card login.
- Song list and normal play.
- Play result save.
- Self-best/crown readback.
- Dani flow.
- Item shop purchase and reward unlock.
- WebUI readback.
- Logs to capture for unexpected Blue-only endpoint calls.

References:

- `Tests/Green/`
- `Tests/WebUi/`
- `Host/README.md`
- `README.md`
- local cabinet/RPCS3 run logs

Exit criteria:

- Track A has a written smoke checklist with pass/fail evidence.
- Unknowns are moved into either Track B or a new focused follow-up.

## Track B - Blue Battle Mode

Battle mode should be its own design/spec family after Track A. It is not just Green AI battle with different names.

Future battle specs should cover:

- `BattleUserDataRequest` / `BattleUserDataResponse`
- Blue `PlayResultRequest.StageData.BattleStageData`
- `InitialdatacheckResponse.is_battleplay`
- `release_battle_stage_flg`, `release_battle_special_flg`, and `battle_bonds_lv_cap`
- NPC identity, costume, special move, exp, DPN, bonds level
- stage unlock progression
- boss life and last stage state
- battle tokens and reward unlocks
- whether battle results affect normal scores/crowns, based on client evidence

References:

- `proto/blue/taiko.proto` battle-related messages
- Blue battle wiki: <https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E6%BC%94%E5%A5%8F%E3%83%90%E3%83%88%E3%83%AB>
- Green AI battle code only as a contrast:
  - `Application/Handlers/GetAiDataQuery.Green.cs`
  - `Application/Handlers/GetAiScoreQuery.Green.cs`
  - `Application/Handlers/UpdatePlayResultCommand.Green.cs`

Binary/client evidence needed before implementation:

- Exact byte widths for battle release flags.
- Accepted/default values for uninitialized NPC/stage/token state.
- Which battle fields are required for menu entry versus result persistence.
- Whether battle playresult transport differs from normal Blue playresult.
- Safe behavior for battle endpoints before the player has battle state.

Track B exit criteria should be defined in a separate battle-mode design doc.

## Explicit Non-Goals

- Tokkun mode behavior.
- Banacoin balance/payment/error/info behavior.
- Yellow-or-earlier support.
- Green AI battle changes.
- Full Blue battle mode in Track A.
- Runtime scraping of wiki or official pages.
- Treating OCR output as authoritative official data.

If Blue cabinet traffic calls Banacoin or Tokkun-related endpoints, the first response should be to log the request and identify why the cabinet requested it. Do not build payment state unless new evidence proves Blue requires it.

## Cross-Stage Rules

- Keep Blue, Green, and Nijiiro persistence separate unless the entity is truly shared identity state.
- Decide field scope first: shared, Nijiiro-only, Green-only, or Blue-only.
- Avoid adding Blue-only concepts to central `Common*` DTO files when a `.Blue.cs` partial can hold them.
- Prefer local repo evidence, cabinet logs, SQLite state, proto files, and IDA/client evidence over guesses.
- Use wiki pages for high-level gameplay scoping, not protocol truth.
- Preserve source/image provenance for official shop data curation.
- Keep future implementation plans serial. Do not merge unrelated stages into a single plan.

## Suggested Future Session Order

1. Write and approve a focused Stage A0 evidence/bootstrap spec.
2. Write and approve a Stage A1 adapter skeleton implementation plan.
3. Implement and verify Stage A1.
4. Repeat spec -> plan -> implementation for A2 through A8.
5. Start a new Track B battle-mode design only after A8 has cabinet evidence.
