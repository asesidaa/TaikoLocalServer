---
status: ready
phase: 37
title: KIMIDORI Runtime State, Dani Dojo, and Normal Play
---

# Phase 37 Plan

## Goal

Implement KIMIDORI-owned runtime state for normal play and Dani Dojo while preventing unsupported cross-mode writes.

## Tasks

1. Add KIMIDORI persistence entities, DbContext surfaces, and migration.
2. Add application handler partials for BAID, mydon entry, userdata, playresult, self-best, crowns, recommendations, and supported no-state uploads.
3. Add KIMIDORI AC15 snapshot/profile/unlock/normal-play/Dani mapping support.
4. Keep challenge arrays inert and exclude Taikojuku practice-folder, Tokkun, Banacoin, battle, Don Challenge, and ChallengeCompe state.
5. Add persistence and no-cross-era tests.

## Verification

- Focused KIMIDORI runtime handler tests.
- SQLite persistence tests for KIMIDORI-owned tables.
- No-write boundary tests for unsupported modes/surfaces.
