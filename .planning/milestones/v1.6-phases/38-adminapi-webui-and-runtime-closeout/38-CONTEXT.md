# Phase 38: AdminApi, WebUI, and Runtime Closeout - Context

**Gathered:** 2026-06-23
**Status:** Ready for planning

<domain>
## Phase Boundary

Expose implemented KIMIDORI-owned state through AdminApi and WebUI, run automated closeout proof, and stop before marking Phase 38 complete until the user accepts cabinet/RPCS3 runtime verification.

</domain>

<decisions>
## Implementation Decisions

### AdminApi
- Add KIMIDORI era-routed AdminApi support for implemented profile, score/history, favorite, reward, catalog, and customization surfaces.
- Preserve existing legacy route patterns where the AdminApi already supports older AC15 eras.
- Keep AdminApi reads/writes tied to KIMIDORI-owned tables and catalogs.

### WebUI
- Expose KIMIDORI as a supported AC15 era.
- Show controls only for implemented KIMIDORI-owned state.
- Do not show Taikojuku practice-folder settings, challenge, battle, Tokkun, Banacoin, Don Challenge, ChallengeCompe, or full shop-authority controls.

### Verification
- Automated verification should cover route ownership, catalog loading, persistence/readback, mapper/classifier behavior, protocol packing, output copy, AdminApi/WebUI readback, and no-cross-era boundaries.
- Inspect generated Mapperly source for nontrivial mappings.
- Run temp-output Host build if the default output is locked.
- Stop with `human_needed` verification for cabinet/RPCS3 smoke evidence; do not mark Phase 38 complete automatically.

### the agent's Discretion
- Keep UI structure consistent with existing AC15 era surfaces and avoid layout restructuring while adding KIMIDORI capability visibility.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- Murasaki AdminApi/WebUI tests and controllers are the closest parity model.
- `TaikoWebUI` uses `WebUiEra` helpers for era normalization, route building, capability visibility, and favorite limits.
- `Contracts.AdminApi` DTOs are shared across AdminApi and WebUI.

### Established Patterns
- Era-routed AdminApi endpoints use `EraRoute.TryParse`.
- WebUI should hide unsupported era capabilities rather than rendering disabled controls for unsupported features.

### Integration Points
- Add KIMIDORI branches to AdminApi controllers, WebUI era helpers/services, tests, documentation, and verification artifacts.

</code_context>

<specifics>
## Specific Ideas

KIMIDORI UI should look like the existing older AC15 surfaces for profile/state, but exclude Taikojuku settings.

</specifics>

<deferred>
## Deferred Ideas

Full milestone closeout and Phase 38 completion wait for user-observed KIMIDORI runtime evidence.

</deferred>
