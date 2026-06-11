# Phase 16: Yellow Tokkun and Banacoin Compatibility - Context

**Gathered:** 2026-06-08
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 16 turns the existing Yellow Tokkun placeholders into evidence-backed Yellow-owned runtime behavior. It must accept Yellow Tokkun `playresult.php` uploads before normal/Dani/shop handling, persist only protocol-backed Tokkun tutorial and raw stage-history facts, read back only the proven Yellow `UserDataResponse.tokkun_tutorial_flg`, and preserve stateless Banacoin-adjacent compatibility routes under `/v09r00/chassis/*`.

This phase does not implement Yellow normal-play changes, Dani changes, item-shop purchase changes, AdminApi/WebUI Tokkun history views, real Banacoin wallet/payment/coupon/settlement/transaction behavior, Yellow battle behavior, runtime cabinet/RPCS3 smoke, or final Yellow contract documentation. Phase 17 owns Yellow normal/Tokkun runtime smoke and closeout.

</domain>

<decisions>
## Implementation Decisions

### Yellow Tokkun Classification And Acceptance
- **D-01:** Classify Yellow Tokkun before normal play handling, Dani handling, shop/medal updates, profile mutations, unlock writes, favorite/recent writes, self-best/crown writes, and any future battle-like branch. The current Yellow handler already checks `IsYellowTokkunShaped(...)` before normal validation/mutation; Phase 16 should replace that success/no-write placeholder with Yellow Tokkun persistence at the same early branch point.
- **D-02:** Use `PlayMode.Tokkun = 3` as the primary Tokkun classifier for Yellow, carrying forward the Blue runtime-proven value and current shared `Domain/Enums/PlayMode.cs` value. Continue treating non-null `ary_tokkunstage_info` as Tokkun-shaped protocol evidence so older/current tests that classify by the Tokkun section remain valid.
- **D-03:** `tokkun_tutorial_flg` remains state/readback evidence, not a standalone classifier. Tutorial-flag-only non-Tokkun uploads must not update Yellow Tokkun tutorial state.
- **D-04:** Tokkun-classified Yellow uploads should return protocol success. They must not fail because normal-looking, Dani-looking, shop-looking, unlock-looking, or medal fields are present; those fields are ignored for gameplay writes once Tokkun wins classification.
- **D-05:** Preserve the current Yellow direct-protobuf controller flow: Yellow `playresult.php` maps adapter-local Yellow wire DTOs to `CommonPlayResultData`, sends `UpdatePlayResultCommand(..., GameEra.Yellow, ...)`, and maps `Result = 1` back to Yellow wire.

### No-Cross-Write Boundary
- **D-06:** Yellow Tokkun uploads may write only allowed Yellow Tokkun state: nullable tutorial state on `UserSaveDataYellow` when the optional Tokkun tutorial field is present, plus append-only Yellow-owned raw stage-history rows when `ary_tokkunstage_info` is present.
- **D-07:** Yellow Tokkun uploads must not write Yellow normal play rows, Yellow best/crown state, Yellow Dani rows, Yellow favorite/recent rows, Yellow shop season/item rows, Yellow Don/Katsu medal totals, Yellow profile counters/settings, Yellow unlock flags, Blue/Green/Nijiiro gameplay rows, Blue battle rows, Blue Tokkun rows, or any Banacoin/payment state.
- **D-08:** Tests should update the existing Phase 15 placeholder test `UpdatePlayResult_Yellow_TokkunShapedPayloadReturnsSuccessWithoutNormalOrProfileWrites` so it now allows only Yellow Tokkun tutorial/history persistence and still proves every forbidden state bucket remains unchanged.
- **D-09:** Proof must be behavior-based: execute the real Yellow mapper, handler, EF reload, and userdata mapper paths. Do not add Tokkun word-scan guards or source allowlists.

### Yellow Tokkun Persistence
- **D-10:** Reuse existing `UserSaveDataYellow.TokkunTutorialFlg` as the nullable raw tutorial-state column. Do not default it in `UserSaveDataYellowExtensions.CreateDefaultYellowSaveData`, do not normalize it to bool, and do not clamp raw values.
- **D-11:** Add a Yellow-owned append-only history entity/table for Tokkun stage facts, analogous in role to `BlueTokkunStageResult` but named and mapped as Yellow-owned state. Add it to Yellow DbContext surfaces and EF mappings; do not reuse `BlueTokkunStageResults` or create a shared/discriminator Tokkun table.
- **D-12:** Store only protocol-backed raw fields from Yellow `PlayResultRequest.TokkunstageData`: `play_datetime`, `play_mode`, `banacoin_datetime`, `tokkun_song_cnt`, repeated `tookun_songno`, `tokkun_speedchange_cnt`, `tokkun_autoplay_cnt`, and `tokkun_jump_cnt`.
- **D-13:** Preserve `tookun_songno` raw order and duplicates. JSON storage is acceptable because Blue already uses it, but the exact representation is downstream discretion as long as tests prove no sorting, dedupe, or catalog normalization.
- **D-14:** Preserve client protocol timestamps only. Do not add server-observed `UploadedAtUtc`, `CreatedAt`, or write-time columns for Phase 16 Tokkun history.
- **D-15:** History rows are append-only per classified upload. Do not dedupe by `banacoin_datetime`, song list, play time, or BAID unless later client evidence proves idempotent replacement semantics.

### Yellow Userdata Readback
- **D-16:** Yellow userdata should read back only `tokkun_tutorial_flg` through the proven Yellow field `UserDataResponse.tokkun_tutorial_flg` at proto field 37.
- **D-17:** Update Yellow AC15 wire-placement/profile behavior so `Ac15UserDataService` carries `UserSaveDataYellow.TokkunTutorialFlg` into `CommonUserDataResponse.TokkunTutorialFlg` for Yellow. Green should continue omitting Tokkun tutorial; Blue behavior remains unchanged.
- **D-18:** Update `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs` so the Yellow wire response serializes `TokkunTutorialFlg` only when the common value is non-null. Before a persisted value exists, Yellow userdata must omit the optional field rather than inventing `0` or `1`.
- **D-19:** Do not expose Tokkun stage summary/history through Yellow userdata, AdminApi, WebUI, initial data, or any invented protocol surface in Phase 16.

### Yellow Banacoin-Adjacent Compatibility
- **D-20:** Keep Yellow Banacoin-adjacent routes stateless direct-protobuf compatibility endpoints under `/v09r00/chassis/*`: `balancecheck.php`, `banacoinpayment.php`, `banacoinerrorlog.php`, and `getbanacoininfo.php`.
- **D-21:** Yellow Banacoin compatibility should log the full request object where practical, following current Blue `request: {@Request}` style rather than logging only chassis id. Logging is the operational visibility surface; no sequence tracker is needed.
- **D-22:** Return compatibility success without persistence. `getbanacoininfo.php` should remain the minimal `Result = 1` response unless Yellow-specific client evidence proves optional fields are required. Optional wallet/payment identity fields such as `bnid_result`, `chid`, `coin_coupon`, balance, coupon, settlement, receipt, and transaction data must stay unset/unpersisted unless concrete Yellow evidence proves a stateful role.
- **D-23:** Existing Yellow `balancecheck.php` and `banacoinpayment.php` may continue echoing required `personid` because the Yellow proto marks it required in those responses. Do not treat that echo as identity/payment authority.
- **D-24:** Do not add EF entities, migrations, Mediator handlers, AdminApi/WebUI pages, configuration, or external integrations for real Banacoin. Static/test proof should assert no Yellow Banacoin wallet/payment/coupon/transaction tables or stateful abstractions exist.

### the agent's Discretion
- The coordinator selected every Phase 16 area because they map directly to YTOK-01, YTOK-02, YTOK-03, YBAN-01, and all five ROADMAP success criteria. No narrowing prompt was needed.
- Downstream agents may decide exact file organization, such as `UpdatePlayResultCommand.YellowTokkun.cs`, `YellowTokkunStageResult.cs`, and focused Yellow Tokkun test files, as long as Yellow ownership and existing partial-file patterns remain clear.
- Downstream agents may choose whether to update shared AC15 user-data tests or add Yellow-specific tests first. The required outcome is Blue unchanged, Green omitted, and Yellow optional Tokkun tutorial readback enabled.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Scope and Requirements
- `.planning/ROADMAP.md` - Phase 16 goal, requirements, success criteria, and Phase 17 boundary.
- `.planning/REQUIREMENTS.md` - YTOK-01, YTOK-02, YTOK-03, YBAN-01, YVER/YDOC follow-on requirements, and out-of-scope Banacoin/Tokkun/battle constraints.
- `.planning/STATE.md` - Current milestone state: Phase 15 verified complete and Phase 16 pending.
- `.planning/PROJECT.md` - Yellow milestone evidence hierarchy, state separation, Yellow Tokkun as real, Yellow battle absence, and Banacoin compatibility boundary.
- `AGENTS.md` - Repo architecture rules, era-owned state, AC15 direct-protobuf/controller/Mediator patterns, and Blue Tokkun/Banacoin constraints.

### Prior Yellow Evidence and State
- `.planning/phases/12-yellow-evidence-and-era-foundation/12-YELLOW-EVIDENCE.md` - Yellow `/v09r00` route prefix, direct-protobuf transport, Banacoin-adjacent route suffixes, `playresult.php`, `userdata.php`, and no-battle absence contract.
- `.planning/phases/12-yellow-evidence-and-era-foundation/12-CONTEXT.md` - First-class Yellow adapter, adapter-local wire, transport, and evidence-boundary decisions.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-CONTEXT.md` - Yellow AC15 profile/core decisions and deferral of Tokkun/Banacoin runtime behavior.
- `.planning/phases/13-yellow-catalog-and-ac15-core-foundation/13-VERIFICATION.md` - Verified catalog/profile/core routes and deferred runtime route status.
- `.planning/phases/14-yellow-identity-userdata-crowns-self-best-and-normal-play/14-CONTEXT.md` - Yellow identity/userdata/normal play decisions, including Phase 16 ownership of Tokkun classification/persistence/readback.
- `.planning/phases/14-yellow-identity-userdata-crowns-self-best-and-normal-play/14-VERIFICATION.md` - Verified Yellow normal/userdata state and tests that Tokkun tutorial readback was omitted before Phase 16.
- `.planning/phases/15-yellow-dani-shop-medals-waiwai-and-admin/15-CONTEXT.md` - Yellow Dani/shop/medal/WaiWai/Admin decisions and explicit Phase 16 deferrals.
- `.planning/phases/15-yellow-dani-shop-medals-waiwai-and-admin/15-RESEARCH.md` - Current Yellow runtime hooks and Phase 16 readiness points.
- `.planning/phases/15-yellow-dani-shop-medals-waiwai-and-admin/15-PATTERNS.md` - Preserved Yellow playresult Tokkun guard order and boundary guard patterns.
- `.planning/phases/15-yellow-dani-shop-medals-waiwai-and-admin/15-VERIFICATION.md` - Verified Phase 15 behavior and deferred Yellow Tokkun/Banacoin/runtime smoke.
- `.planning/phases/15-yellow-dani-shop-medals-waiwai-and-admin/15-REVIEW.md` - Clean Phase 15 review confirming no Yellow Tokkun persistence, Banacoin wallet state, or Yellow battle behavior was introduced.

### Blue Tokkun and Banacoin Precedent
- `.planning/milestones/v1.1-phases/07-tokkun-evidence-contract-and-guardrail-reset/07-CONTEXT.md` - Tokkun classifier/evidence/no-source-scan guardrail decisions.
- `.planning/milestones/v1.1-phases/08-stateless-banacoin-compatibility-and-availability/08-CONTEXT.md` - Stateless Banacoin compatibility, minimal optional fields, and no wallet/payment persistence.
- `.planning/milestones/v1.1-phases/09-tokkun-mapper-and-safe-playresult-acceptance/09-CONTEXT.md` - Tokkun mapper, raw field preservation, safe acceptance, and no-cross-write behavior proof.
- `.planning/milestones/v1.1-phases/10-evidence-backed-tokkun-state-persistence-and-readback/10-CONTEXT.md` - `PlayMode.Tokkun = 3`, nullable tutorial state, append-only raw history, raw order/duplicates, protocol timestamps only, and tutorial-only userdata readback.

### Protocol and Wire Evidence
- `proto/yellow/yellow.proto` - Yellow `UserDataResponse.tokkun_tutorial_flg`, `PlayResultRequest.play_mode`, `tokkun_tutorial_flg`, `ary_tokkunstage_info`, `TokkunstageData`, and Banacoin-adjacent response field contracts.
- `Adapters.GameProtocol.Yellow/Wire/Game.cs` - Generated Yellow optional presence helpers and DTO placement for Tokkun and Banacoin-adjacent messages.
- `proto/blue/taiko.proto` - Blue Tokkun/Banacoin schema precedent for matching field names and runtime-tested Tokkun contract.
- `Adapters.GameProtocol.Blue/Wire/Game.cs` - Generated Blue wire precedent for Tokkun and Banacoin optional-field behavior.

### Current Code Touch Points
- `Domain/Enums/PlayMode.cs` - Contains `Tokkun = 3`; Yellow should use it for primary classification.
- `Application/Dtos/CommonPlayResultData.BlueTokkun.cs` - Current shared-in-practice Tokkun common DTO surface for classifier, nullable tutorial, and raw stage data.
- `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs` - Current Yellow mapper already maps `play_mode`, optional `tokkun_tutorial_flg`, and raw `AryTokkunstageInfo` fields.
- `Application/Handlers/UpdatePlayResultCommand.Yellow.cs` - Current Yellow handler has an early Tokkun-shaped success/no-write placeholder to replace with Yellow Tokkun persistence.
- `Domain/Entities/UserSaveDataYellow.cs` - Existing nullable `TokkunTutorialFlg` column on Yellow save state.
- `Application/Common/UserSaveDataYellowExtensions.cs` - Default Yellow save creation; must continue not defaulting `TokkunTutorialFlg`.
- `Application/Ac15/Ac15EraProfiles.cs` and `Application/Ac15/Ac15WirePlacement.cs` - Current Yellow profile omits Tokkun tutorial readback; Phase 16 should change Yellow only.
- `Application/Ac15/YellowAc15UserDataAdapter.cs` - Already passes `saveData.TokkunTutorialFlg` into the shared snapshot; readback is currently blocked by Yellow profile placement.
- `Application/Handlers/UserDataQuery.Yellow.cs` - Yellow userdata query path for persisted tutorial readback.
- `Adapters.GameProtocol.Yellow/Mappers/UserDataMappers.cs` - Current Yellow wire mapper omits `TokkunTutorialFlg`; Phase 16 should map optional readback.
- `Adapters.GameProtocol.Yellow/Controllers/YellowScaffoldControllers.cs` - Current Yellow playresult/userdata/Banacoin route implementations and stateless Banacoin-adjacent endpoints.
- `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs` - Blue implementation precedent for tutorial/history persistence.
- `Domain/Entities/BlueTokkunStageResult.cs` - Blue-owned append-only raw history shape to mirror only as a Yellow-owned analog.
- `Adapters.GameProtocol.Blue/Controllers/GetBanacoinInfoController.cs`, `BanacoinPaymentController.cs`, and `BanacoinErrorLogController.cs` - Blue stateless direct-protobuf Banacoin compatibility precedent.

### Focused Tests to Update or Add
- `Tests/Yellow/YellowPlayResultHandlerTests.cs` - Current Yellow mapper/handler Tokkun placeholder tests; update for allowed Yellow Tokkun tutorial/history persistence plus forbidden cross-writes.
- `Tests/Yellow/YellowUserDataProtocolTests.cs` - Current Yellow userdata tests assert Tokkun tutorial omission; update to optional absence/presence readback.
- `Tests/Yellow/YellowPersistenceBoundaryTests.cs` - Current Yellow persistence boundary test forbids deferred Tokkun tables; update to allow only Yellow-owned Tokkun history and still forbid battle/Banacoin/shared tables.
- `Tests/Ac15/Ac15UserDataServiceTests.cs` and `Tests/Ac15/Ac15EraProfileTests.cs` - Shared profile/readback tests proving Yellow now places Tokkun tutorial in userdata while Green remains omitted.
- `Tests/Blue/BlueTokkunPersistenceTests.cs`, `Tests/Blue/BlueTokkunPersistenceShapeTests.cs`, `Tests/Blue/BluePlayResultHandlerTests.cs`, and `Tests/Blue/BlueUserDataTests.cs` - Blue precedent for persistence shape, raw replay, no-cross-write, and optional tutorial readback.
- `Tests/Yellow/YellowRouteSkeletonTests.cs` and `Tests/Yellow/YellowCatalogBoundaryTests.cs` - Existing Yellow route/source tests that include Banacoin-adjacent route ownership and deferred state boundaries.

### AC15 Shared-Core Direction
- `docs/superpowers/specs/2026-06-07-ac15-core-extraction-design.md` - Approved capability-driven AC15 sharing direction while preserving era-owned routes, wire DTOs, and persistence.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/05-userdata-core-and-era-adapters.md` - Userdata core and era-adapter guidance relevant to Yellow Tokkun tutorial readback.
- `docs/superpowers/plans/2026-06-07-ac15-core-extraction/06-normal-play-core-and-hooks.md` - Normal-play hook guidance relevant to keeping Tokkun out of normal save paths.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Application/Handlers/UpdatePlayResultCommand.BlueTokkun.cs`: closest implementation precedent for classified Tokkun persistence and tutorial/history-only state changes.
- `Domain/Entities/BlueTokkunStageResult.cs`: raw protocol-backed history shape; use only as a Yellow-owned analog, not a shared or Blue table.
- `Application/Dtos/CommonPlayResultData.BlueTokkun.cs`: existing common Tokkun DTO fields already used by Yellow mapper; planners may keep this name for now or rename/split if they can do so without broad churn.
- `Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs`: already preserves Yellow Tokkun raw fields, including the generated `TookunSongnoes` spelling and optional tutorial presence.
- `YellowAc15UserDataAdapter.CreateSnapshot`: already passes `UserSaveDataYellow.TokkunTutorialFlg` into the snapshot.
- `Ac15UserDataService.BuildResponse`: already gates Tokkun tutorial readback by `profile.WirePlacement.HasTokkunTutorialFlagInUserData`.
- Blue Banacoin controllers: direct-protobuf log/success shape with no Mediator/EF dependency.
- `YellowHandlerFixture`: focused in-memory SQLite/catalog fixture for handler, EF, and readback tests.

### Established Patterns
- Era-owned state remains physically separate. Yellow Tokkun history needs Yellow entity, DbSet, EF mapping, migration, and tests.
- Controllers deserialize/map/call Mediator/map response. Business behavior belongs in `Application/Handlers`.
- Runtime branch ordering matters: Tokkun must be handled before normal/Dani/shop mutations.
- Generated `Wire/` files are evidence and should not be manually edited.
- Optional protobuf fields should be serialized only when backed by persisted nullable values.
- No source-word Tokkun guardrails. Use behavior tests and persistence shape tests instead.
- Compatibility endpoints that emulate Banacoin-adjacent behavior stay stateless and local; the repo has no outbound Banacoin integration.

### Integration Points
- Add a Yellow Tokkun handler partial or helper called from the early Yellow playresult branch.
- Add `YellowTokkunStageResult` and expose/configure it through `ITaikoDbContext.Yellow.cs` and `TaikoDbContext.Yellow.cs`.
- Add or update EF migration/model snapshot to include only the Yellow Tokkun history table; `UserSaveDataYellow.TokkunTutorialFlg` already exists.
- Update Yellow profile/wire placement and Yellow userdata mapper to surface optional `tokkun_tutorial_flg`.
- Update Banacoin-adjacent Yellow controller logging/response tests while keeping the endpoints stateless.
- Update focused Yellow tests and shared AC15 tests to reflect the Phase 16 contract; ensure existing Blue/Green behavior remains unchanged.

</code_context>

<specifics>
## Specific Ideas

Coordinator-selected discussion areas and requirement mapping:

- Tokkun classification/acceptance maps to YTOK-01 and success criterion 1.
- Tokkun no-cross-write proof maps to YTOK-01 and success criterion 2.
- Tokkun tutorial/history persistence maps to YTOK-02 and success criterion 3.
- Userdata tutorial-only readback maps to YTOK-03 and success criterion 4.
- Stateless Banacoin-adjacent compatibility maps to YBAN-01 and success criterion 5.

Current-source details to preserve:

- `UpdatePlayResultCommand.Yellow.cs` already returns success before normal handling for Tokkun-shaped uploads. Phase 16 should keep that branch before all normal/Dani/shop mutations.
- `UserSaveDataYellow.TokkunTutorialFlg` already exists and is nullable. It is currently not defaulted.
- `Ac15EraProfiles.Yellow` currently has `HasTokkunTutorialFlagInUserData: false`; this is the current readback blocker to change for Phase 16.
- Yellow `BalancecheckResponse` and `BanacoinpaymentResponse` have required `personid` fields in `proto/yellow/yellow.proto`; echoing `Personid` is protocol compatibility, not real Banacoin authority.

</specifics>

<deferred>
## Deferred Ideas

- Yellow Tokkun history/AdminApi/WebUI inspection remains outside Phase 16 unless a later requirement maps it. Phase 15 only enabled generic Yellow WebUI/AdminApi readback, not Tokkun history views.
- Yellow normal/Tokkun cabinet or RPCS3 runtime smoke belongs to Phase 17.
- Final Yellow contract documentation belongs to Phase 17.
- Real Banacoin wallet/payment/coupon/settlement/receipt/transaction persistence remains out of scope for this repo unless a later milestone explicitly changes the project boundary with concrete evidence.
- Yellow battle remains out of scope unless new concrete Yellow proto/log/client evidence appears.

</deferred>

---

*Phase: 16-Yellow Tokkun and Banacoin Compatibility*
*Context gathered: 2026-06-08*
