---
phase: 43-momoiro-adminapi-and-webui-routing
type: ui-spec
created: 2026-06-28
requirements:
  - MOADMIN-02
---

# Phase 43 UI Spec

## User Story

As an admin, I can select MOMOIRO in the existing WebUI era navigation and inspect or edit implemented MOMOIRO-owned profile and play state without seeing unsupported MOMOIRO feature controls.

## Interaction Contract

- MOMOIRO appears anywhere enabled eras are normalized through `WebUiEra`.
- MOMOIRO uses existing user pages:
  - Profile
  - Song List
  - High Scores
  - Play History
  - Dani Dojo
- MOMOIRO does not expose Don Challenge navigation.
- MOMOIRO profile editing uses `/api/Momoiro/Ac15ProfileSettings/{baid}`.
- MOMOIRO catalog lookups use `/api/Momoiro/GameData/...` and `/api/Momoiro/customization/...`.

## Unsupported Controls

The UI must not surface active MOMOIRO controls for:

- Taikojuku practice folders
- Tokkun
- Banacoin
- Battle
- Don Challenge
- ChallengeCompe
- Event folders
- Newer item-shop authority
- Proto-only route families

Existing profile settings capabilities decide which profile option groups render. The Phase 43 implementation must not add new text, tabs, cards, or panels for unsupported MOMOIRO features.

## Visual Scope

No layout redesign is planned. MOMOIRO inherits the existing WebUI shell, menu density, tables, tabs, and profile editor controls. This phase is routing and capability exposure, not a visual refresh.
