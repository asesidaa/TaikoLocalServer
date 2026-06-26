# Phase 42: MOMOIRO Normal Playresult, Unlocks, Rewards, and Dan Compatibility - Context

**Gathered:** 2026-06-26
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 42 turns the binary-proven MOMOIRO `playresult.php` route from a scaffold into evidence-backed normal-play mutation. It may persist only MOMOIRO-owned gameplay state that can be read back through the Phase 41 `userdata.php`, `selfbest.php`, and BAID/MyDon surfaces: scores, best rows, userdata-owned crowns, profile counters, recent/favorite lists, release-song flags, Don Point/reward counters, and Dan/Dani-compatible fields where MOMOIRO playresult/userdata/binary evidence proves the contract.

Out of scope: proto edits, new route families, `shoppingresult.php`, `bestscore.php`, `communicationlog.php`, `mainichisong.php`, standalone `crownsdata.php`, Taikojuku practice-folder behavior, Tokkun, Banacoin, battle, gacha, tournament, event folders, newer item-shop authority, Don Challenge, ChallengeCompe management, AdminApi/WebUI, and cabinet/RPCS3 acceptance.

</domain>

<decisions>
## Implementation Decisions

### Evidence and Scope
- Treat `proto/momoiro` as field evidence and `.tools/momoiro/EBOOT.ELF.i64` as route/native evidence; require both before adding stateful behavior beyond ordinary `playresult.php`.
- Use MOMOIRO binary/client research before planning song unlock, crown mutation, Don Point/reward mutation, Dan/Dani, and challenge-shaped field handling.
- If a field is present in proto but the route/runtime contract is not proven, accept/log/ignore it rather than creating new persistence authority.
- Keep all writes MOMOIRO-owned except shared identity rows already established by Phase 41.

### Normal Play Mutation
- Reuse shared AC15 normal-play helpers where the MOMOIRO wire shape matches adjacent AC15 normal play, but add MOMOIRO entity/DbContext surfaces instead of writing adjacent-era tables.
- Normal playresult may update score history, self-best, crown source rows, release-song flags, recent rows, favorite rows, profile counters, and profile/reward counters only through MOMOIRO-owned tables and save fields.
- Favorite ordering remains the Phase 41 explicit `DisplayOrder` contract; playresult favorite mutation must preserve or derive display order without falling back to unproven raw song-number sorting.
- Tests must prove no writes to KIMIDORI, Murasaki, White, Red, Yellow, Blue, Green, Nijiiro, Tokkun, battle, Don Challenge, or unsupported feature tables.

### Unlocks, Rewards, and Crowns
- Song unlock mutation writes MOMOIRO `ReleaseSongFlg` only if the playresult `release_song_no` contract is verified from generated wire and binary/client strings/control flow.
- Crowns continue to be represented through userdata-owned `hash_crown_flg` sourced from MOMOIRO best rows; do not add a dedicated crown route or crown table.
- Don Point and reward fields may update the MOMOIRO save row only within Phase 40/41 limits and only after research confirms the fields are normal playresult read/write fields.
- Do not implement live item-shop, wallet, payment, season, purchase, coupon, or shop authority from reward/Don Point fields.

### Dan and Challenge-Shaped Data
- MOMOIRO Dan/Dani support is allowed only if `play_dan`, `dan_result`, BAID/userdata Dan fields, and binary/client evidence prove the normal Dan contract.
- Do not infer Taikojuku practice-folder behavior from Dan/Dani fields; no Taikojuku route is in MOMOIRO scope.
- Challenge-shaped arrays may be parsed, logged, ignored, or stored only according to MOMOIRO evidence; they do not create Don Challenge, ChallengeCompe, or reward-management semantics.
- If challenge semantics remain unproven, tests should assert non-creation of challenge state and no cross-feature side effects.

### the agent's Discretion
- Choose conservative internal names and task splits that match existing AC15 partial-file patterns.
- Add focused tests for observable state transitions and no-cross-era/no-cross-feature boundaries; do not add implementation-shape or generated-wire property tests.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Application/Handlers/UpdatePlayResultCommand.*.cs` contains the existing per-era AC15 normal-play, Dani, Tokkun, battle, and Don Challenge split patterns.
- `Application/Ac15/Ac15NormalPlayWriter.cs` owns reusable score, self-best, crown-source, favorite, and recent mutation mechanics.
- `Application/Ac15/Ac15DaniWriter.cs`, `Ac15DaniMapper.cs`, and per-era `DanScoreDatum*` tables provide the adjacent Dan/Dani pattern if MOMOIRO evidence proves it.
- `Application/Ac15/Ac15UnlockFlagAccess.cs`, `Ac15ProtocolBytes`, and `Ac15SongHashCodec` are the shared byte/flag helpers for release and crown readback.
- Phase 41 added MOMOIRO readback surfaces: `UserSaveDataMomoiro`, `SongBestDatumMomoiro`, `MomoiroFavoriteSongs`, `MomoiroRecentSongs`, `MomoiroAc15UserDataAdapter`, and Mapperly-backed MOMOIRO readback controllers.

### Established Patterns
- AC15 era behavior is dispatched from shared unsuffixed handlers into era-specific `.Momoiro.cs`, `.Kimidori.cs`, `.Murasaki.cs`, etc. partials.
- Controllers deserialize direct protobuf, map wire DTOs into Application/common DTOs, send Mediator requests, and map back to generated wire.
- Gameplay state is era-owned; shared AC15 services may share algorithms, not persistence tables.
- Mapperly-generated source must be inspected when mapper behavior is added or changed.

### Integration Points
- `Adapters.GameProtocol.Momoiro/Controllers/PlayResultController.cs` is currently the scaffold route to replace.
- `Application/Handlers/UpdatePlayResultCommand.cs` needs a `GameEra.Momoiro` dispatch arm only after MOMOIRO playresult input mapping exists.
- New persistence, if needed, belongs under Domain entities, `ITaikoDbContext.Momoiro.cs`, `TaikoDbContext.Momoiro.cs`, and EF migrations.
- Tests belong under `Tests/Momoiro/` and should reuse `MomoiroHandlerFixture` where possible, extending it only for observable Phase 42 state transitions.

</code_context>

<specifics>
## Specific Ideas

- The user explicitly asked not to complicate the design: most core logic should already be present, so prefer proper MOMOIRO wiring, proper limits, and proper data over new abstractions.
- Unlock and crown representation must be researched from the MOMOIRO binary properly before implementation claims.
- Cabinet/RPCS3 acceptance is not part of Phase 42 closeout unless the user provides runtime evidence early.

</specifics>

<deferred>
## Deferred Ideas

- AdminApi/WebUI exposure belongs to Phase 43.
- Full automated plus cabinet/RPCS3 acceptance belongs to Phase 44.
- Later MOMOIRO versions and all special feature families listed in MOSPEC-01 remain future scope.

</deferred>
