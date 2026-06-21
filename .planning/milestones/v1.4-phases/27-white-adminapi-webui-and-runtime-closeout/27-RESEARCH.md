# Phase 27 Research: White AdminApi WebUI and Closeout

## Live Code Findings

- `TaikoWebUI/Utilities/WebUiEra.cs` was the WebUI era allowlist and AC15 classifier. Before Phase 27 it knew Nijiiro, Green, Blue, Yellow, and Red, but not White.
- Existing AdminApi controllers already used era-routed generic contracts for AC15 score/history/favorite/Dani/catalog/customization surfaces. Before Phase 27 their switches ended at Red.
- `Ac15ProfileSettingsController` is the AC15 profile/user-settings contract. `UserSettingsController` remains legacy Nijiiro profile settings and should not be widened for White.
- `AuthController.GetConfig()` composes `FavoriteSongLimits` from `Ac15EraProfiles.GetMaxFavoriteSongs`, so adding White to enabled eras publishes the White limit without WebUI hardcoding.
- `DonChallengeController` delegates to application handlers. Before this correction it only returned Red data; White now needs its own branch over White-owned Don Challenge catalog/state without reading Red rows.

## Evidence Inputs

- Phase 25 bound White-owned runtime tables for identity, profile/save, normal play, favorites, recent songs, crowns/self-best, reward/Don Point, and Dani.
- Phase 26 bound White `present.xml` and `spacialbaid.xml` catalog provenance. Its Don Challenge absent/data-only decision is superseded by the user-supplied White Don Challenge range data and the correction that Don Challenge is not ChallengeCompe protocol readback.
- Current Mapperly documentation for version 4.3.1 was checked before closeout. Relevant behavior remains source-generator based, and generated source inspection is the verification authority for nontrivial mappings.

## Implementation Boundary

No new White cabinet ChallengeCompe route/readback is introduced in Phase 27. The correction adds server-side White Don Challenge data/state derived from normal playresult stages and exposes it through the existing dedicated AdminApi/WebUI page.
