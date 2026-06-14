# Phase 22: Red AdminApi/WebUI and Runtime Closeout - Context

**Gathered:** 2026-06-15
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 22 exposes implemented Red-owned runtime state through AdminApi and WebUI, adds a Don Challenge page for the shared older-AC15 ChallengeCompe capability with Red as the first supported binding, and prepares the Red v1.3 closeout path.

This phase may add Red AdminApi/WebUI support for implemented profile, score/history, favorite/recent, Dani, catalog/customization, Tokkun tutorial readback, and ChallengeCompe progress/reward surfaces. It may correct the Phase 21 ChallengeCompe opt-in assumption so configured Don Challenge participation applies automatically. It must not add Red WaiWai, battle, item-shop, medal/shop-season, user-created challenge-letter, BNG/official competition bucket, community/global simulation, operator schedule editing, or compatibility-route diagnostics UI without new evidence.

Final cabinet/RPCS3 verification is manual and user-reported. Implementation work still needs meaningful automated tests for the UI/API and state behavior it changes.

</domain>

<decisions>
## Implementation Decisions

### Red Normal AdminApi And WebUI Readouts
- **D-01:** Phase 22 should add full Red parity for implemented AC15 readout surfaces: profile/user settings, play data, play history, favorites/recent song behavior, Dani, game-data catalog, customization catalog, and WebUI era routing.
- **D-02:** Red normal AdminApi/WebUI surfaces should use the existing AC15 edit model where backing state exists. Profile, favorite, customization, and supported settings edits may behave like Green/Blue/Yellow under the existing permissions model.
- **D-03:** Red normal readouts should use existing `/api/{era}/...` AdminApi routes and the generic WebUI pages instead of Red-only API shapes when the shared contracts fit.
- **D-04:** Do not add AdminApi/WebUI diagnostics for simple compatibility routes. Keep reward-card, reward-execution, Banacoin-adjacent, and similar compatibility evidence in logs or verification artifacts, not operator-facing pages.

### Don Challenge Page
- **D-05:** Add Don Challenge as a standalone Play Data page, adjacent to existing user-era pages such as Songs, High Scores, Play History, and Dani Dojo.
- **D-06:** The Don Challenge page is capability-gated, not Red-branded. It is available for Red now and should be reusable for older AC15 eras when they bind the shared ChallengeCompe capability.
- **D-07:** The page should be driven by a dedicated AdminApi surface for Don Challenge rather than folding this data into `UserSettings` or profile responses.
- **D-08:** The page should show challenge cards/progress as the main content: active bundle context, personal task cards, task progress/completion, and configured song/title reward status.
- **D-09:** Do not expose raw uploaded ChallengeCompe facts on the Don Challenge page in Phase 22. Keep raw facts as server-side diagnostic/persistence evidence unless later requested.
- **D-10:** Show configured reward status on the Don Challenge cards, including whether configured song/title rewards are locked, earned, or unavailable from current progress.
- **D-11:** Hide the Don Challenge navigation entry when no active ChallengeCompe bundle is configured for the selected supported era. A direct route may still fail gracefully, but normal navigation should not advertise an unconfigured challenge.

### Don Challenge Opt-In Correction
- **D-12:** There is no Don Challenge opt-in at all. Phase 22 supersedes Phase 21 decisions that treated `UserSaveDataRed.IsChallengeCompe` as an enrollment gate.
- **D-13:** Configured Don Challenge should apply to all applicable Red/older-capability users. Progress mutation, readback, reward-song locks, and reward grants must not be gated by `IsChallengeCompe`.
- **D-14:** Do not add an opt-in toggle or editable opt-in field to AdminApi/WebUI. If the Red wire field remains for protocol compatibility, it is not a local participation control.

### Testing And Final Verification
- **D-15:** Add focused tests for meaningful Red AdminApi/WebUI behavior: Red era routes, Red-owned read/write boundaries, Don Challenge API data, Don Challenge navigation gating, reward-status data, and no-cross-era/no-cross-mode behavior.
- **D-16:** WebUI tests are required only when they protect meaningful behavior. Do not add superficial component tests or source-shape tests just to satisfy a checkbox.
- **D-17:** Final cabinet/RPCS3 verification is manual and user-reported. The agent should not claim final client compatibility from automated tests alone and should not require a fixed agent-run smoke matrix.
- **D-18:** The phase should maintain a user-reported manual verification record that captures manual verification status, issues found, and unresolved manual gaps.
- **D-19:** User-reported manual verification issues in implemented Phase 22 scope are blockers to closeout and should be fixed in the phase before Red v1.3 completion.

### The Agent's Discretion
- The planner may choose exact AdminApi route names, DTO names, WebUI page/component names, and plan splits.
- The planner may decide whether Don Challenge API, page/navigation, opt-in-gate removal, and normal Red parity are split into separate checkpoint plans.
- The planner may decide how direct navigation behaves when no active bundle is configured, as long as normal navigation hides the page and no server/client error leaks to users.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Phase Scope And Requirements
- `.planning/ROADMAP.md` - Phase 22 scope, deliverables, success criteria, and UI hint.
- `.planning/REQUIREMENTS.md` - RVER-01, RVER-02, RVER-03, Red active requirements, and out-of-scope boundaries.
- `.planning/STATE.md` - Current state after Phase 21 completion and Phase 22 planning handoff.
- `.planning/PROJECT.md` - Red milestone scope, evidence hierarchy, AC15 state separation, and ChallengeCompe as shared older-AC15 capability.

### Prior Phase Handoff
- `.planning/phases/21-older-ac15-challengecompe-capability-and-red-binding/21-CONTEXT.md` - ChallengeCompe implementation contract and Phase 22 handoff. Treat opt-in decisions D-01 through D-05 as superseded by this Phase 22 context.
- `.planning/phases/21-older-ac15-challengecompe-capability-and-red-binding/21-SUMMARY.md` - Implemented ChallengeCompe state, rewards, readback, and Phase 22 handoff.
- `.planning/phases/21-older-ac15-challengecompe-capability-and-red-binding/21-VERIFICATION.md` - Final Phase 21 verification, explicit remaining manual/runtime gaps, and boundary audit.
- `.planning/phases/21-older-ac15-challengecompe-capability-and-red-binding/21-CHALLENGECOMPE-EVIDENCE.md` - Product vs local evidence authority, accepted ChallengeCompe surface, and explicit unsupported buckets.
- `.planning/phases/20-red-runtime-capability-binding-and-simple-compatibility/20-CONTEXT.md` - Red runtime state, normal/Tokkun/simple compatibility boundaries, and ChallengeCompe array preservation.
- `.planning/phases/20.1-ac15-capability-dto-and-mapper-boundary-refactor/20.1-CONTEXT.md` - Mapper/response boundary rules: handlers return semantic sections, controllers assemble wire, Mapperly stays mechanical.

### Code And Contract References
- `Adapters.AdminApi/Controllers/UserSettingsController.cs` - Existing `/api/{era}/UserSettings/{baid}` dispatch currently needs Red support.
- `Adapters.AdminApi/Controllers/PlayDataController.cs` - Existing `/api/{era}/PlayData/{baid}` pattern and AC15 score readback surface currently need Red support.
- `Adapters.AdminApi/Controllers/PlayHistoryController.cs` - Existing `/api/{era}/PlayHistory/{baid}` pattern currently needs Red support.
- `Adapters.AdminApi/Controllers/FavoriteSongsController.cs` - Existing favorite song read/write route pattern currently needs Red support.
- `Adapters.AdminApi/Controllers/DanBestDataController.cs` - Existing `/api/{era}/DanBestData/{baid}` pattern currently needs Red support.
- `Adapters.AdminApi/Controllers/GameDataController.cs` - Existing era game-data catalog readback pattern currently needs Red catalog support.
- `TaikoWebUI/Utilities/WebUiEra.cs` - WebUI era normalization and API/user route helper currently needs Red support and Don Challenge capability gating.
- `TaikoWebUI/Components/NavMenu.razor` - Existing Play Data navigation where Don Challenge should be added conditionally.
- `TaikoWebUI/Pages/Profile.razor.cs` - Existing generic user-era profile page and AC15 edit model.
- `Contracts.AdminApi/ViewModels/UserSetting.cs` - Existing shared profile/settings DTO; Don Challenge should use a dedicated API contract instead of adding opt-in state here.

### Phase 21 ChallengeCompe Implementation
- `Application/Ac15/ChallengeCompe/` - Shared transport-agnostic ChallengeCompe catalog/evaluator behavior.
- `Host/wwwroot/data/red/red_challenge_compe_data.json` - Red-owned ChallengeCompe sidecar catalog.
- `Infrastructure/GameDataCatalog/Ac15/Schemas/ac15-challenge-compe-catalog.schema.json` - ChallengeCompe sidecar schema copied to Host output.
- `Application/Handlers/UpdatePlayResultCommand.Red.cs` - Red playresult flow where ChallengeCompe mutation and opt-in-gate removal must be reviewed.
- `Application/Handlers/GetChallengeCompeQuery.cs` - ChallengeCompe readback handler path that must not gate Red readback on opt-in after Phase 22.
- `Domain/Entities/RedChallengeCompeRawFact.cs` - Red-owned raw fact persistence.
- `Domain/Entities/RedChallengeCompeProgress.cs` - Red-owned derived progress persistence.

### Tests And Codebase Maps
- `Tests/Yellow/YellowAdminApiTests.cs` - Useful pattern for era-owned AdminApi read/write and no-cross-era assertions.
- `Tests/WebUi/GameDataServiceTests.cs` - Useful pattern for WebUI service route assertions; current test shows Red is not normalized as supported yet.
- `.planning/codebase/ARCHITECTURE.md` - AC15 shared-core, direct `ITaikoDbContext`, Mapperly, AdminApi/WebUI, and route ownership patterns.
- `.planning/codebase/STRUCTURE.md` - Where to add AdminApi, WebUI, contracts, Red state, and tests.
- `.planning/codebase/TESTING.md` - Test value gate and recommended focused verification commands.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Adapters.AdminApi/Controllers/*Controller.cs` already preserve legacy Nijiiro routes and expose `/api/{era}/...` routes for Green/Blue/Yellow; Red should be added to those switches where implemented state exists.
- `TaikoWebUI/Utilities/WebUiEra.cs` centralizes supported era normalization, user routes, and API paths; adding Red here enables generic pages to request Red routes.
- Existing pages `Profile`, `SongList`, `HighScores`, `PlayHistory`, `Song`, and `DaniDojo` already consume `WebUiEra.Api` and `WebUiEra.UserRoute`, so most normal Red readout parity should reuse those pages.
- `TaikoWebUI/Components/NavMenu.razor` already owns Play Data navigation and should conditionally show Don Challenge only when the selected era has an active configured ChallengeCompe bundle.
- `Application/Ac15/ChallengeCompe` plus Red ChallengeCompe rows and catalog sidecar provide the page's backing runtime data.

### Established Patterns
- AdminApi controllers validate era strings through `EraRoute.TryParse`, authorize owner/admin access, then route to era-owned table/catalog logic.
- WebUI pages should use `WebUiEra` helpers instead of hardcoding era paths.
- AC15 state remains era-owned. Red AdminApi additions must read/write Red tables and must not touch Green, Blue, Yellow, Nijiiro, or shared gameplay rows except shared identity where already established.
- Tests should protect observable behavior, SQLite persistence, AdminApi/WebUI route behavior, and no-cross-era boundaries. Avoid route inventory, generated wire existence, source text, or one-to-one mapper tests.
- Mapperly remains mechanical and source-generator driven. Business behavior belongs in application handlers/services, not mapper aggregation.

### Integration Points
- Add Red support to AdminApi controllers for user settings, play data, play history, favorite songs, song leaderboard if needed by existing pages, Dani, game data, and customization catalog.
- Add Red to `WebUiEra` known/supported eras and update WebUI tests so Red-enabled clients request Red era routes.
- Add a dedicated Don Challenge AdminApi contract, likely under an era-routed user path, returning active-bundle presence, task cards/progress, completion state, and reward status.
- Add a Don Challenge WebUI page under the user-era Play Data area. Gate the navigation entry on supported capability plus active bundle presence, not on user opt-in.
- Remove or bypass `UserSaveDataRed.IsChallengeCompe` as a local ChallengeCompe gate in progress mutation, readback, reward locking, and reward grants. Keep the wire field only as protocol compatibility if needed.
- Add focused tests patterned after `Tests/Yellow/YellowAdminApiTests.cs` and `Tests/WebUi/GameDataServiceTests.cs`, plus Red ChallengeCompe behavior tests for no-opt-in participation.

</code_context>

<specifics>
## Specific Ideas

- Don Challenge cards should be "always in" for users when a supported era has an active configured challenge bundle. There is no UI or runtime opt-in.
- The page should show reward status because Phase 21 already implemented configured song/title locks and grants.
- The normal WebUI navigation should hide Don Challenge when no active bundle exists. Direct links should be handled gracefully at the planner's discretion.
- Final verification artifacts should clearly distinguish automated implementation verification from manual cabinet/RPCS3 acceptance. Manual user-reported issues in Phase 22 scope block closeout until fixed.

</specifics>

<deferred>
## Deferred Ideas

- Operator schedule editing, authored bundle management UI, JSON/schema editing, and challenge reward-management tooling remain future scope.
- User-created challenge letters, official/BNG competition buckets, community/global progress aggregation, and `ary_user_compe_*` / `ary_bng_compe_*` UI remain evidence-gated.
- Red WaiWai, battle, item-shop, medal/shop-season, and compatibility-route diagnostics UI remain out of scope.

</deferred>

---

*Phase: 22-Red AdminApi/WebUI and Runtime Closeout*
*Context gathered: 2026-06-15*
