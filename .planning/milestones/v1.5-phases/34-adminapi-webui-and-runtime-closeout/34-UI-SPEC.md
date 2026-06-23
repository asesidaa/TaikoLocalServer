# Phase 34 UI Spec: Murasaki Admin Surfaces

## Goal

Operators can select Murasaki in the existing WebUI era selector and use the same implemented AC15 operator workflows as other supported older AC15 eras, without seeing controls for unsupported Murasaki features.

## Supported Murasaki UI Surfaces

- User card era menu entry.
- Profile page in AC15 mode:
  - My Don name.
  - Costume slots, title, tone, and color controls backed by Murasaki catalog/profile data.
  - AC15 profile options already modeled by `Ac15ProfileCapabilities`.
- Song list and high score pages backed by Murasaki catalog and best-score state.
- Play history backed by Murasaki play rows and Murasaki favorites.
- Favorite song toggles using the Murasaki favorite-song cap.
- Dani Dojo page backed by Murasaki Dani score rows and Murasaki Taikojuku catalog data.

## Hidden or Unsupported Murasaki UI Surfaces

- Don Challenge link and user-card Don Challenge menu item.
- ChallengeCompe, global-score, shopping, Banacoin, battle, Tokkun history, and proto-only special request controls.
- Any control that would require Red, White, Yellow, Blue, Green, or Nijiiro state to represent Murasaki behavior.

## Interaction Rules

- Era route helpers must preserve `Murasaki` in user page links and API links.
- Murasaki must be treated as AC15 for profile/high-score/history page rendering.
- Unsupported Murasaki features should be absent from navigation rather than shown disabled.
- Existing layout and page structure should remain unchanged.

## Verification

- WebUI tests should prove Murasaki normalization, routing, catalog API request paths, AC15 classification, and Don Challenge exclusion.
- AdminApi tests should prove the API returns Murasaki-owned state and ignores other era rows for the same BAID/song.
