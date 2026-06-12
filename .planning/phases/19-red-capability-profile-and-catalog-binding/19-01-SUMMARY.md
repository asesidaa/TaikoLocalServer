---
phase: 19-red-capability-profile-and-catalog-binding
plan: 01
subsystem: red-catalog-profile
tags: [red, ac15, catalog, profile]
completed: 2026-06-13
---

# Phase 19 Plan 01 Summary

## Accomplishments

- Added `IRedCatalog`, `CatalogExtensions.Red()`, `RedEraGameDataCatalog`, Red active data paths, and Red required-file validation for `ST8100-1`.
- Bound Red to shared AC15 musicinfo, tuning, Taikojuku, event-folder, telop, recommendation, movie, and customization catalog loaders.
- Added `Ac15EraProfiles.Red` with Red support enabled for shared AC15 metadata/runtime capabilities and item shop disabled.
- Added Red catalog projection through `Ac15CatalogSnapshotFactory.FromRed`.
- Added committed empty Red sidecar JSON for optional server-authored metadata.

## Reuse Decisions

- Red uses `Application.Catalog.Ac15` catalog shapes directly where Red has no era-specific fields.
- Blue first-run customization extraction was moved onto shared AC15 customization support instead of copying that bootstrap logic into Red.
- Red item-shop catalog is represented as `Ac15ItemShopCatalog.Disabled`; no Red item-shop loader or state was added.

## Verification

- Covered by Red profile, snapshot, handler, sidecar, and catalog parsing tests.
- Full verification is recorded in `19-VERIFICATION.md`.
