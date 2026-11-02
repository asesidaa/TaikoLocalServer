---
phase: 15
slug: yellow-dani-shop-medals-waiwai-and-admin
status: approved
shadcn_initialized: false
preset: none
created: 2026-06-08
reviewed_at: 2026-06-08
---

# Phase 15 - UI Design Contract

> Visual and interaction contract for the Phase 15 AdminApi/WebUI Yellow readback work.

## Design System

| Property | Value |
|----------|-------|
| Tool | none |
| Preset | not applicable |
| Component library | MudBlazor |
| Icon library | MudBlazor Material icons |
| Font | Existing TaikoWebUI typography: MudBlazor defaults for admin surfaces, `Nijiiro` font only where existing preview/nameplate/game-themed components already use it |

## Spacing Scale

Declared values (must be multiples of 4):

| Token | Value | Usage |
|-------|-------|-------|
| xs | 4px | Table cells, icon padding, compact label offsets |
| sm | 8px | Dense tables, toolbar padding, small stacks |
| md | 16px | Form controls, page-level grid gaps, dialog body padding |
| lg | 24px | Panel/card internals, major rows |
| xl | 32px | Dani detail panel padding and grouped sections |
| 2xl | 48px | Large empty/loading state vertical space |
| 3xl | 64px | Not introduced for Phase 15 |

Exceptions: keep existing `DaniDojo` tab shell dimensions (`48px` tab nav, `24px` tab icon slot) and existing table cell padding from `TaikoWebUI/wwwroot/css/app.css`; do not add new arbitrary spacing values for Yellow.

## Typography

| Role | Size | Weight | Line Height |
|------|------|--------|-------------|
| Body | 16px through `Typo.body1`/MudBlazor default | 400 | 1.5 |
| Dense body/label | 14px through `Typo.body2`, `Typo.caption`, and breadcrumbs | 400 | 1.4 |
| Heading | 20px through `Typo.h6`/`Typo.h5` depending existing page context | 600 | 1.25 |
| Numeric emphasis | Existing `Typo.h4` or bold `Typo.body1` in Dani totals only | 600 | 1.2 |

Do not introduce new heading scale or viewport-based font sizing for Yellow. Keep long Dan titles in the existing horizontally scrollable Dani tab pattern rather than shrinking text.

## Color

| Role | Value | Usage |
|------|-------|-------|
| Dominant (60%) | MudBlazor surface/background palette (`var(--mud-palette-surface)`, `var(--mud-palette-background)`) | Existing admin pages, tables, cards, dialogs |
| Secondary (30%) | MudBlazor table/line palette (`var(--mud-palette-table-striped)`, `var(--mud-palette-lines-default)`) | Dense table headers, row grouping, outlined cards |
| Accent (10%) | MudBlazor primary (`var(--mud-palette-primary)`) | Active era/Dani tab affordance, primary action button, focus ring |
| Destructive | MudBlazor error (`Color.Error`) | Existing destructive menu/dialog actions only |

Accent reserved for: active navigation/tab indicator, primary action buttons, focus-visible outlines, selected row state where already used. Do not color every Yellow-specific item with a new Yellow palette.

## Copywriting Contract

| Element | Copy |
|---------|------|
| Primary CTA | "Save Profile" for settings changes; no new primary CTA for passive Yellow readback pages |
| Empty state heading | "No Dani Dojo data" for empty Yellow Dani readback; "No play history" for empty Yellow play history |
| Empty state body | "Play Yellow on a registered card, then refresh this page." |
| Error state | "Yellow data is unavailable. Check that Yellow is enabled and catalog data is loaded." |
| Destructive confirmation | No new destructive Yellow actions in Phase 15 |

Keep existing localized strings where already present (`Dani Dojo`, `Play History`, `Song List`, `High Scores`, `Profile`). New copy must be added through the existing localization resource flow if surfaced in UI.

## Interaction Contract

| Surface | Contract |
|---------|----------|
| Era selector | Add Yellow as a supported AC15 era in the existing user nav selector. Yellow routes must use `Users/{baid}/Yellow/...` and `api/Yellow/...`. |
| Profile/settings | Use existing `Profile` page controls and AdminApi `UserSettings` DTO. Do not add a separate Yellow profile page. |
| Dani Dojo | Reuse the existing `DaniDojo` page, tab shell, result image, totals, condition cards, and horizontal tab scrolling. Yellow data comes from `/api/Yellow/GameData/DanData` and `/api/Yellow/DanBestData/{baid}`. |
| Play history/high scores/song details | Reuse existing dense table/card layouts. Yellow is AC15, so rank-specific Nijiiro-only UI remains hidden. |
| Shop-relevant state | Phase 15 does not add a new shop management page. Shop unlock effects surface through existing profile/unlock and song/tone lock readback. |
| Tokkun placeholder | Do not expose Tokkun history in Phase 15. If a route is visible only because existing UI is era-generic, empty state copy must say data is not available until Tokkun support is implemented. |

## Registry Safety

| Registry | Blocks Used | Safety Gate |
|----------|-------------|-------------|
| shadcn official | none | not applicable |
| third-party | none | not applicable |

## Checker Sign-Off

- [x] Dimension 1 Copywriting: PASS
- [x] Dimension 2 Visuals: PASS
- [x] Dimension 3 Color: PASS
- [x] Dimension 4 Typography: PASS
- [x] Dimension 5 Spacing: PASS
- [x] Dimension 6 Registry Safety: PASS

**Approval:** approved 2026-06-08

