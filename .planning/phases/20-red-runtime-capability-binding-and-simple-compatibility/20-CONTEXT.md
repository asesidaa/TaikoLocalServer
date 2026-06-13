# Phase 20: Red Runtime Capability Binding and Simple Compatibility - Context

**Gathered:** 2026-06-13
**Status:** Ready for execution
**Mode:** Autonomous with user-approved recommended options

<domain>
## Phase Boundary

Phase 20 binds Red AC15 runtime state to mechanisms already implemented for Green, Blue, and Yellow. It may add Red-owned EF entities, DbSets, migrations, shared AC15 table bindings, Red handler partials, Red adapter mappers/controllers, and focused runtime tests for identity, userdata, self-best, crowns, normal play, Dani, tutorial-only Tokkun, and simple compatibility routes.

It must not implement Red ChallengeCompe state, item-shop catalogs or purchases, medal/shop-season state, Banacoin wallet/payment authority, battle, WaiWai, AdminApi, or WebUI. Challenge arrays in Red playresults are accepted as payload fields but Phase 21 owns stateful ChallengeCompe behavior.

</domain>

<decisions>
## Implementation Decisions

### State Ownership
- Add Red-owned tables for profile/userdata, normal play history, self-best, crowns, favorites, recent songs, Dani scores, and nullable Tokkun tutorial state.
- Reuse shared card/user identity only for the existing cross-era identity contract; do not write Green, Blue, Yellow, or Nijiiro gameplay rows from Red requests.
- Persist Red Don points as Red profile fields, not as AC15 shop medals or shop-season balances.

### Shared Runtime Composition
- Bind Red through existing generic AC15 services: mydon entry, userdata snapshot construction, self-best response construction, crown packing, normal-play writer, normal-stage filtering, Dani writer, Dani mapper/readback helpers, customization mutation, and protocol byte helpers.
- Add Red table bindings and delegates where shared AC15 writers require concrete entity types.
- Add Red-specific glue only for wire placement or semantics that differ from later eras: Don points, reward progress, difficulty tutorial flag, absence of shop/medal season state, and tutorial-only Tokkun.

### Tokkun And Challenge Boundaries
- Classify Red Tokkun before normal, Dani, or ChallengeCompe handling when `PlayMode.Tokkun = 3`, `TokkunTutorialFlg` is present, or `AryTokkunstageInfo` is present.
- Persist/read back only `TokkunTutorialFlg` on Red userdata. Do not add raw Tokkun history rows, progression rows, reward rows, unlock writes, or normal-play side effects from Tokkun requests.
- Preserve Red playresult challenge-array mapping into the common DTO for Phase 21, but do not create ChallengeCompe tables or state mutations in this phase.

### Compatibility Routes
- Keep reward-card, reward-execution, balance-check, Banacoin payment, and Banacoin error routes as log-and-success compatibility endpoints.
- Do not make compatibility routes authoritative for song/tone/title/costume unlocks, Don points, wallet balance, coupons, payment, receipts, or transactions.

### Tests And Verification
- Tests must exercise observable handler/controller behavior, SQLite persistence, protocol mappers, crown/self-best packing, and no-cross-era/no-cross-mode boundaries.
- Avoid tests over generated wire type existence, controller attributes, implementation strings, migration source text, or stateless `Result = 1` echoes unless paired with no-write behavior.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Application/Ac15/Ac15NormalPlayWriter.cs` is generic over play/best/favorite/recent tables and can bind directly to Red concrete row types.
- `Application/Ac15/Ac15DaniWriter.cs` and `Ac15DaniMapper.cs` already own shared Taikojuku/Dani persistence semantics.
- `Application/Ac15/Ac15UserDataService.cs` builds shared AC15 userdata responses from catalog, save-data, favorite, recent, and counter snapshots.
- `Application/Common/UserSaveData*Extensions.cs` and `Application/Handlers/*.{Green,Blue,Yellow}.cs` show the existing partial-file binding pattern.
- `Adapters.GameProtocol.Yellow` has the closest wire/controller shape for Red identity/userdata/self-best/crowns/playresult, but Red wire names Don points rather than medals.

### Red Runtime Surfaces
- Red game routes are under `/v08r01/chassis/*`; startup/version routes stay shared under `/v01r00/chassis/*`.
- Red wire exposes BAID, mydon entry, userdata, self-best, crowns, playresult, reward-card, reward-execution, balance-check, Banacoin payment, and Banacoin error compatibility routes.
- Red userdata exposes `total_get_donpoint`, `total_use_donpoint`, `difficulty_tutorial_flg`, `difficulty_played_course`, `difficulty_played_star`, `tokkun_tutorial_flg`, `is_challengecompe`, and `is_tojiru`.
- Red wire does not expose a `danscore.php` route in the current adapter/proto surface; Dani readback for Phase 20 is Red userdata flags/display-Dan plus Taikojuku metadata.

</code_context>

<specifics>
## Specific Evidence And Guardrails

- User explicitly required no code duplication and Red-specific limits composed with mechanisms already implemented for Green/Blue/Yellow.
- User explicitly required Red binary evidence from `.tools/red/EBOOT.ELF.i64` through `ida-cli` with the existing daemon.
- Current IDA daemon probe for `.tools/red/EBOOT.ELF.i64` returned `database_opened: True`, `ida_available: True`, and backend `idalib`.
- Phase 19 evidence remains active for Phase 20 limits:
  - `EntryNetwork::OnCrownsDataResponse` expands 1024 ten-bit crown entries.
  - `EntryNetwork::OnUserDataResponse` copies Red 128/16/128-byte flag arrays.
  - `game::net::OnTaikoJukuResponse` caps songs per pack at 10.
  - Active `ST8100-1/musicmedleyinfo.xml` covers normal and extra Taikojuku challenge levels.

</specifics>

<deferred>
## Deferred Ideas

- Shared older-AC15 ChallengeCompe behavior and any Red ChallengeCompe state belong to Phase 21.
- Red AdminApi/WebUI readback and runtime closeout belong to Phase 22.
- Cabinet/RPCS3 acceptance evidence is expected in Phase 22 after Red runtime and ChallengeCompe scope are complete.

</deferred>
