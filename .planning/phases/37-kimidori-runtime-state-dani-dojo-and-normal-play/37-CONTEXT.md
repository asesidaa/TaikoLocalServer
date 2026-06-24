# Phase 37: KIMIDORI Runtime State, Dani Dojo, and Normal Play - Context

**Gathered:** 2026-06-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Implement KIMIDORI-owned card/profile/userdata, normal play, self-best, crowns, favorites/recent songs, rewards, Don Points, shopping-result compatibility, and Dani Dojo state without creating unsupported adjacent-era state.

</domain>

<decisions>
## Implementation Decisions

### Runtime State
- Add KIMIDORI-owned persistence tables and context surfaces; share only true identity/card data.
- Normal playresult writes must stay within KIMIDORI-owned score, history, crown, favorite, recent, reward, Don Point, unlock, and profile-counter state.
- Shopping-result compatibility may update KIMIDORI-owned Don Point/unlock/readback state where protocol-backed, but must not create wallet, payment, season, medal, or full shop authority.

### Dani Dojo
- Persist Dani Dojo through KIMIDORI-owned Dan score/stage rows where `proto/kimidori` and binary request names prove the normal Dan fields.
- Keep Dani Dojo separate from Taikojuku practice folders; missing Taikojuku proto/route evidence does not remove Dani result persistence.

### Boundaries
- Challenge arrays remain inert unless KIMIDORI-specific evidence proves concrete server semantics.
- Do not write Don Challenge, ChallengeCompe, battle, Tokkun, Banacoin, Taikojuku, or cross-era state.

### the agent's Discretion
- Reuse shared AC15 normal-play, Dani, profile, and byte-packing services where the generated KIMIDORI wire shape matches existing older AC15 patterns.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- Murasaki domain entities, EF mappings, handlers, and AC15 adapters are the closest runtime model.
- Shared AC15 services already handle normal play, Dani rows, profile counters, unlock flags, user-data snapshots, and protocol byte packing.

### Established Patterns
- Runtime handlers dispatch by `GameEra` into era-specific partials.
- Adapter controllers map generated wire DTOs into common application DTOs before Mediator calls.
- Tests should cover observable persistence and no-cross-era/no-cross-mode boundaries, not generated DTO existence.

### Integration Points
- Add KIMIDORI domain entities, `ITaikoDbContext` partial, EF context partial, migration, application handlers, mappers, and focused runtime tests.

</code_context>

<specifics>
## Specific Ideas

Implement KIMIDORI very close to Murasaki normal runtime behavior, with Taikojuku practice-folder routes and WebUI controls absent.

</specifics>

<deferred>
## Deferred Ideas

Manual cabinet/RPCS3 acceptance is deferred to Phase 38 and remains user-observed.

</deferred>
