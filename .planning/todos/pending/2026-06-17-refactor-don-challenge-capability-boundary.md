---
created: 2026-06-17T21:07:20.306Z
title: Refactor Don Challenge capability boundary
area: architecture
files:
  - Application/Handlers/GetDonChallengeQuery.cs:28
  - Application/Settings/ServerSettingsOptionsValidationExtensions.cs:30
  - Application/Handlers/GetChallengeCompeQuery.Red.cs:5
  - Application/Handlers/UpdatePlayResultCommand.Red.cs:87
  - Application/Handlers/UpdatePlayResultCommand.Red.cs:104
  - Application/Ac15/ChallengeCompe/Ac15ChallengeCompeCatalog.cs
  - Application/Ac15/ChallengeCompe/Ac15ChallengeCompeProgressEvaluator.cs
  - Application/Ac15/Ac15TokkunWriter.cs:18
---

## Problem

Red currently has a working Don Challenge implementation, but the capability boundary and naming are wrong enough that White support will inherit avoidable design debt.

Issues to fix:

- Don Challenge is Red-owned in code even though it is an older-AC15 capability that White is expected to support later. `GetDonChallengeQueryHandler` hard-gates to Red, reads `catalog.Red()`, and queries Red-only progress tables. Startup validation only validates `EnableChallengeCompe` / `ActiveChallengeCompeBundleId` for Red.
- The implemented feature is named Challenge Compe throughout the domain even though the actual Red `challengecompe.php` route is still a stub. The stateful behavior is Don Challenge progress/rewards derived from normal playresult stage data, stored in `RedChallengeCompeProgress` and `RedChallengeCompeRawFacts`.
- Don Challenge mutation and readback are too hand-written and too embedded in Red handlers. Red `playresult.php` directly evaluates tasks, writes raw facts, updates progress, and grants rewards; AdminApi readback directly builds task/reward DTOs instead of using mapper/service boundaries.
- Tokkun's core persistence is now shared through `Ac15TokkunWriter`, but Blue, Yellow, and Red still duplicate mode-detection and dispatch shape. Tokkun behavior should remain shared aside from era tables/history hooks.

## Solution

Refactor the current Red implementation into explicit shared capability boundaries:

1. Reserve `ChallengeCompe` naming for the real `challengecompe.php` protocol surface, which is currently a stub for Red.
2. Rename the current `Application/Ac15/ChallengeCompe` model/evaluator/reward code to a Don Challenge namespace and terms, for example `Application/Ac15/DonChallenge`.
3. Move normal-play Don Challenge mutation into a shared `Ac15DonChallengeWriter` or equivalent service that accepts era-owned table bindings, catalog binding, save-data reward mutators, limits, and play time.
4. Keep Red as the first era binding, not the owner. White should be able to add its own catalog/storage binding later without copying Red handler logic, while still waiting for White evidence/data before wiring runtime behavior.
5. Add Mapperly or focused mapper helpers for mechanical Don Challenge DTO projection instead of hand-building all AdminApi readback DTOs inside `GetDonChallengeQueryHandler`.
6. Clean up config/catalog naming from `EnableChallengeCompe` / `ActiveChallengeCompeBundleId` where it refers to Don Challenge data. Preserve compatibility with existing settings if needed through aliasing or migration notes.
7. Finish Tokkun cleanup by centralizing shared play-mode detection/dispatch and leaving only era-specific save-data access, recent-song table selection, and optional raw-history hooks in era files.
