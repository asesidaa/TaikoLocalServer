# Phase 25 Research: White Runtime Capability Binding and Rewards

## Evidence

- Phase 23 approved White game endpoints under `/v07r00/chassis/*` and shared startup/version routes under `/v01r00/chassis/*`.
- Phase 24 added `Ac15EraProfiles.White`, White catalog access, and White sidecar binding for implemented metadata surfaces.
- Generated White wire includes playresult `get_donpoint`, `reward_ptn`, `reward_progress`, release song, tone, costume, title, stage, Dani, and embedded challenge-id fields.
- Generated White userdata includes reward/Don Point readback and embedded challenge-stat lists, but no Tokkun tutorial field and no Yellow shop/medal placement.
- Local White raw data includes `present.xml` and `spacialbaid.xml`; there is no existing server-side present/special-BAID loader in the shared AC15 codebase. Phase 26 owns provenance collection and any collectable sidecar decision.

## Implementation Shape

- Add White save, normal play, favorite, recent, and Dani entities plus `ITaikoDbContext`/`TaikoDbContext` partials and EF migration.
- Extend existing AC15 Mapperly mappers and typed helper accessors for White rows.
- Add White handler partials for BAID, mydon entry, userdata, playresult, catalog metadata, self-best, crowns, and Taikojuku.
- Add White adapter mappers that map only fields present in White wire and omit unsupported Tokkun, battle, ghost, legal terms, item shop, and Don Challenge stateful sections.
- Add focused tests for observable state mutation/readback and no-cross-era/no-cross-mode boundaries.

## Deferred

- `present.xml`, `spacialbaid.xml`, detailed collectable provenance, and any White Don Challenge decision remain Phase 26 work.
- Manual cabinet/RPCS3 and WebUI review remains Phase 27 work.
