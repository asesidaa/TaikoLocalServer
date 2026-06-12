---
phase: 19-red-capability-profile-and-catalog-binding
plan: 02
subsystem: red-metadata-routes
tags: [red, ac15, protobuf, metadata]
completed: 2026-06-13
---

# Phase 19 Plan 02 Summary

## Accomplishments

- Added Red handler partials for initial data, folders, telops, recommendations, Taikojuku, and startup movie readback.
- Mapped HDD major version `8` to Red in shared startup movie lookup.
- Added Red Mapperly mappers for generated Red wire DTOs.
- Replaced Phase 18 no-state probes with catalog-backed behavior for Phase 19-owned metadata routes only.

## Boundaries Preserved

- Non-Phase-19 Red probes remain no-state.
- Generated Red wire files were not manually edited.
- No Red gameplay persistence, profile state mutation, AdminApi, WebUI, ChallengeCompe semantics, or item-shop authority was introduced.

## Verification

- Covered by Red metadata mapper tests, Red handler tests, Red catalog parsing tests, focused Red/AC15 tests, and full test suite.
