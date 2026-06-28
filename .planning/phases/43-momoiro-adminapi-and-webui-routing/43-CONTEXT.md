---
phase: 43-momoiro-adminapi-and-webui-routing
type: context
created: 2026-06-28
requirements:
  - MOADMIN-01
  - MOADMIN-02
---

# Phase 43 Context

## Objective

Expose implemented MOMOIRO-owned state through existing AdminApi and WebUI surfaces without adding unsupported MOMOIRO feature controls.

## Source Inputs

- `.planning/ROADMAP.md`: Phase 43 success criteria require `/api/momoiro/...` AdminApi routes, WebUI era selection, unsupported-control absence, and round-trip edits through cabinet-readback state.
- `.planning/REQUIREMENTS.md`: MOADMIN-01 and MOADMIN-02 are the only Phase 43 requirements.
- Phase 41 and 42 implementation state: MOMOIRO has dedicated save, score, play-history, favorite/recent, and bounded Dan tables plus catalog-backed music/Dan metadata.
- `TaikoWebUI/Utilities/WebUiEra.cs`: Era normalization currently stops at KIMIDORI, so enabled MOMOIRO is filtered out of the WebUI.
- `Adapters.AdminApi/Controllers/*`: Neighboring AC15 eras already use partial controller files and era switch arms for profile settings, play data, history, favorites, leaderboard, Dan, and catalog routes.

## Scope

In scope:

- Add `WebUiEra.Momoiro` as a supported AC15 era.
- Add MOMOIRO AdminApi switch arms and partials for implemented state: profile settings, music/Dan data, play data, play history, favorites, leaderboards, Dan best data, and customization catalog lookups.
- Preserve MOMOIRO favorite `DisplayOrder` semantics for AdminApi read/write.
- Return empty MOMOIRO customization catalogs until MOMOIRO-specific customization sidecars or extraction evidence exist.
- Add focused tests over controller behavior and WebUI era/page gating.

Out of scope:

- Taikojuku, Tokkun, Banacoin, battle, Don Challenge, ChallengeCompe, event folders, newer item-shop authority, or proto-only route-family controls.
- Rich packed crown, release-song, challenge, or Dan editors beyond existing surfaces.
- Cabinet/RPCS3 acceptance. That remains Phase 44.
- Raw game-data link repair, copying, deletion, or catalog sidecar generation.

## Decisions

- MOMOIRO bounded Dan uses `DaniFileOrder` in the AdminApi `GameData/DanData` response, matching Phase 42 naming and avoiding Taikojuku route implications.
- The WebUI exposes the same profile, song list, high score, play history, and Dani Dojo page slots used by supported older AC15 eras. Don Challenge remains Red/White only.
- MOMOIRO customization catalog endpoints should succeed with empty catalogs instead of borrowing KIMIDORI/Murasaki/Nijiiro catalogs. Profile settings still edit the MOMOIRO-owned numeric save fields and unlock bitsets.
