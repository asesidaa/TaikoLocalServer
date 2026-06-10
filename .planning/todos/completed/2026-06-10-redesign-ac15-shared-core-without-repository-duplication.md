completed: 2026-06-10
---
created: 2026-06-10T20:46:16.298Z
title: Redesign AC15 shared core without repository duplication
area: architecture
files:

  - Application/Ac15
  - Application/Handlers
  - Infrastructure
  - Domain/Entities
  - .planning/phases/16.2-ac15-shared-core-simplification-and-reuse-cleanup

---

## Problem

Phase 16.2 implementation feedback identified major architectural issues that must be resolved before continuing AC15 shared-core refactors:

- Do not add a repository or persistence layer over EF. EF/ITaikoDbContext is already the traceable persistence boundary.
- Do not preserve duplicated normal-mode, base Dani, crown, self-best, or profile-counter logic behind more interfaces, adapters, or era switching.
- Do not create near-identical era adapters whose only real difference is the target table name. AC15 has many eras, and copying this pattern across future eras would multiply duplicate code.
- Do not hand-write mechanical mappers where Mapperly should own the projection.
- Preserve era-owned tables and protocol boundaries, but make shared logic generic/type-safe enough that different tables do not force duplicated behavior.

The likely design direction is a shared AC15 application DTO/canonical model plus Mapperly-generated projections into era-specific storage entities, with generic shared services operating over typed entity sets or strongly typed delegates directly against ITaikoDbContext. If adapters remain, they must have real era behavior, not table-name-only boilerplate.

## Solution

Before further Phase 16.2 execution, rework the architecture plan and implementation to:

1. Remove repository-shaped AC15 persistence interfaces/adapters introduced only to hide EF access.
2. Keep ITaikoDbContext/EF as the persistence boundary and make database access easy to trace.
3. Use Mapperly for mechanical Application/Ac15 <-> era-storage projections.
4. Centralize shared AC15 normal-mode/base Dani/crown logic once, using generics, typed callbacks, or shared DTOs where appropriate.
5. Keep only meaningful era-specific policy/hooks: protocol placement, unsupported features, era limits, proven semantic differences, and table/entity ownership.
6. Add tests only for observable behavior and no-cross-era/no-cross-mode boundaries, not for adapter existence or Mapperly one-to-one copying.
