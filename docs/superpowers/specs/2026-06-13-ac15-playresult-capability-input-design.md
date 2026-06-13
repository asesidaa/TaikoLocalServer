# AC15 Playresult Capability Input - Design

**Date:** 2026-06-13
**Status:** design approved in discussion; pending written-spec review
**Scope:** Redesign AC15 playresult input after adapter mapping so Blue, Green, Yellow, and Red stop targeting `CommonPlayResultData` as an all-era union. Nijiiro stays on the existing path.

## Purpose

The AC15 playresult mapper refactor exposed a real boundary problem. `CommonPlayResultData` now contains fields for Nijiiro, AC15 normal play, unlocks, customization, Dani, Tokkun, Blue battle, Green ghost, Yellow WaiWai, Red reward facts, and older-AC15 ChallengeCompe facts. Because Mapperly is configured with `RequiredMappingStrategy.Target`, every AC15 mapper must account for target members that its era wire model does not own. The resulting ignore lists and `RMG012` warnings are symptoms of an all-era union DTO, not mapper defects.

This design replaces the AC15 playresult input target with capability-shaped records. Mapperly should map generated wire DTOs into small records whose members match the capability being projected. Application handlers remain the visible era workflow roots, and shared `Application/Ac15` writers migrate fully to the new records without changing business behavior.

## Current Problem

Current AC15 mappers map into:

```csharp
public static partial CommonPlayResultData Map(PlayResultRequest request);
```

That target type includes fields that are not common to the source era. For example:

- Blue mappers see Green ghost and Red reward target fields.
- Green mappers see Blue battle and Tokkun target fields.
- Red mappers see WaiWai and battle target fields.
- Stage mappings inherit the same problem through the nested `CommonPlayResultData.StageData` union.

With target-strict Mapperly, this forces large `[MapperIgnoreTarget]` lists and still leaves `RMG012` warnings for nested members. Adding more ignores would encode negative knowledge in every era mapper and keep the design smell.

## Goals

- Make AC15 playresult input capability-shaped rather than era-union-shaped.
- Keep Nijiiro out of this refactor.
- Preserve all current AC15 playresult business behavior and side-effect boundaries.
- Keep handlers as era orchestration roots.
- Keep shared AC15 modules switch-free and bound at the era edge through concrete tables, policies, and Mapperly delegates.
- Use Mapperly's native supported mapping features before manual helper code.
- Keep generated `Wire/` files untouched.
- Remove AC15 shared-writer dependency on `CommonPlayResultData` in the accepted end state.
- Eliminate Mapperly warnings caused by mapping AC15 wire DTOs into the old all-era union.

## Non-Goals

- Do not redesign Nijiiro playresult.
- Do not merge AC15 persistence tables.
- Do not add repository-shaped persistence abstractions.
- Do not infer missing era features from another era's wire model.
- Do not implement stateful ChallengeCompe semantics in this refactor.
- Do not add source-shape tests for mapper internals, project files, route inventory, or generated wire member existence.

## Architecture

Add an AC15-only playresult input boundary under `Application/Dtos/Ac15`.

The top-level shape should be equivalent to:

```csharp
public sealed record Ac15PlayResultEnvelope(
    Ac15PlayResultMetadata Metadata,
    Ac15NormalPlayResult? Normal,
    Ac15DaniPlayResult? Dani,
    Ac15ProfileMutationFacts? Profile,
    Ac15TokkunPlayResult? Tokkun,
    BlueBattlePlayResult? BlueBattle,
    GreenGhostPlayResult? GreenGhost,
    RedChallengeCompeFacts? ChallengeCompe);
```

The envelope is not a shared database model and not a generated wire DTO. It is the Application input produced by era adapter mapping. Each capability record contains only the facts owned by that capability.

Era controllers remain transport-only:

1. Deserialize generated wire DTOs.
2. Log transport facts.
3. Map wire DTOs to `Ac15PlayResultEnvelope`.
4. Send the Application command.
5. Map the protocol result back to the generated response DTO.

Era handler partials remain orchestration roots:

- Blue handles Tokkun before battle before normal.
- Yellow and Red handle Tokkun before normal.
- Green handles ghost facts as Green-owned normal-path enrichment.
- Red ChallengeCompe facts remain preserved input facts for Phase 21 and do not imply stateful behavior here.

## Capability Records

The record set separates these concerns. The implementation plan may adjust names only to match an existing local naming convention while preserving these boundaries:

- `Ac15PlayResultMetadata`: BAID/request facts, chassis/shop/request timestamps, card/player facts, play mode, area code, reserved/content bytes, and route-supplied facts if needed.
- `Ac15NormalPlayResult`: normal play stages plus normal play mode facts consumed by normal stage filtering and row writing.
- `Ac15StageResult`: song, level, score, crown/play result, hit counts, options, tone flags, favorite/recent flags, category/folder/pushed flags, and other normal-stage facts.
- `Ac15ProfileMutationFacts`: profile counters, medal or Don-point deltas, tutorial flags, current costume presence, current costume, unlock lists, previous area, and other save-row mutation facts.
- `Ac15DaniPlayResult`: Dan mode result, Dan result grade, aggregate combo/soul facts, and stage-level Dan ids/facts.
- `Ac15TokkunPlayResult`: tutorial flag and protocol-backed Tokkun stage facts.
- `BlueBattlePlayResult`: battle stage, NPC, release, token, and battle-side Don medal/recent-song facts.
- `GreenGhostPlayResult`: ghost release, token, performance, rank, winnings, and per-stage ghost-section facts.
- `RedChallengeCompeFacts`: challenge, user-compe, and BNG-compe ids preserved from Red playresult stage arrays only.

Records should be immutable where practical. Mutable lists are acceptable only where they match existing shared writer ergonomics and avoid unnecessary copying.

## Mapperly Rules

Keep Mapperly strict target mapping. The fix is to make the targets correct, not to weaken diagnostics.

Mapperly should be used for mechanical projection through native features first:

- Use constructor or record mapping for immutable capability records.
- Use `MapProperty` with property paths and `MapNestedProperties` for flattening and unflattening nested wire shapes.
- Use additional mapping parameters when route-level or caller-supplied facts need to enter metadata.
- Use `MapPropertyFromSource`, `MapValue`, and `UserMapping` only for mechanical conversions such as byte normalization, null-to-empty strings where protocol requires it, array/list conversion, optional primitive presence, constants, and capability null handling.
- Use external mappings or reused mapping configuration for repeated conversion helpers across Blue, Yellow, and Red rather than copying helper bodies.

Manual mapper code is allowed only when Mapperly cannot express a mechanical conversion cleanly. Manual mapper code must not:

- classify play modes;
- decide which Application branch should run;
- encode no-write or persistence policy;
- mutate request facts;
- hide unsupported era fields behind default-filled capability records.

Capability presence must mean the wire payload supplied facts for that capability or the handler/classifier intentionally activated that capability from evidence-backed request facts. Unsupported capabilities should be `null`, not default objects.

Relevant Mapperly documentation for the implementation plan:

- `https://mapperly.riok.app/docs/configuration/ctor-mappings/`
- `https://mapperly.riok.app/docs/configuration/flattening/`
- `https://mapperly.riok.app/docs/configuration/additional-mapping-parameters/`
- `https://mapperly.riok.app/docs/configuration/user-implemented-methods/`
- `https://mapperly.riok.app/docs/configuration/external-mappings/`
- `https://mapperly.riok.app/docs/configuration/reusing-mapping-configurations/`

## Shared Writer Migration

Shared writers should migrate fully to the new capability records. The accepted end state must not leave AC15 shared writers requiring `CommonPlayResultData`.

Affected modules include:

- `Ac15CommonProfileMutation`
- `Ac15NormalStageFilter`
- `Ac15NormalPlayWriter`
- `Ac15DaniWriter`
- `Ac15ProfileCounterUpdater`
- Green ghost insertion/update helpers
- Blue battle state/update helpers
- Blue, Yellow, and Red Tokkun handlers

This is a type-boundary migration, not a behavior rewrite. The same facts should be read and the same rows should be written as before. Existing table bindings, policies, limits, Mapperly entity delegates, and no-cross-era guarantees remain unchanged.

Do not plan a compatibility bridge from new records back into `CommonPlayResultData`. If a local mechanical helper is useful inside one implementation patch, it must be removed before verification and must not appear in tests or public method signatures as a supported boundary.

## Command Shape

The implementation may choose the least disruptive command shape, but the final boundary must keep AC15 separate from Nijiiro. Acceptable options:

- Add a dedicated `UpdateAc15PlayResultCommand` while leaving `UpdatePlayResultCommand` for Nijiiro.
- Convert `UpdatePlayResultCommand` into a wrapper that carries either Nijiiro common data or AC15 envelope data and validates the era/payload combination.

The preferred option is a dedicated AC15 command if it keeps handler signatures and compile-time safety clearer without unnecessary churn.

## Error Handling

Existing behavior must be preserved:

- `baid == 0` returns protocol success.
- Missing user logs and returns protocol success.
- Unsupported normal stages log and skip with protocol success.
- No valid normal stages returns protocol success without mutation.
- Medal and Don-point overflow returns protocol success without mutation and logs a warning.
- Tokkun uploads do not write normal, Dani, battle, shop, recent, favorite, crown, score, or unlock state except for the currently supported Tokkun tutorial/history facts per era.
- Blue battle uploads do not write normal score, crown, Dani, profile, favorite, or normal unlock state; active shop Don medals and recent songs remain allowed.
- Green ghost behavior remains Green-owned and must not leak into other eras.
- Red ChallengeCompe facts remain facts only until Phase 21 proves state/readback semantics.

Unsupported or unproven capabilities stay absent from the envelope instead of appearing as default-filled union fields.

## Testing Strategy

Tests must protect observable behavior and evidence-backed boundaries, not mapper source shape.

Required coverage:

- Focused AC15 writer tests proving migrated writers produce the same profile mutations, rows, best updates, favorites, recent songs, Dani updates, and special-mode side effects from the new records.
- Era handler tests preserving branch order: Blue Tokkun before battle before normal; Yellow and Red Tokkun before normal; Green ghost enrichment only on Green.
- No-cross-era persistence tests for Blue, Green, Yellow, and Red.
- No-cross-mode tests for Tokkun, Blue battle, normal, and Dani boundaries.
- Mapper tests only where they protect nontrivial placement, optional presence, capability presence, null omission, or protocol-backed field grouping.
- Focused Blue/Green/Yellow/Red playresult test slices, followed by broad regression such as full `Tests/Tests.csproj` when practical.
- Temp-output Host build if normal build output is locked.

Avoid tests that assert generated wire property existence, mapper method bodies, ignore attributes, source text, project files, controller attributes, or route inventory.

## Migration Plan

1. Add AC15 playresult capability records.
2. Add AC15 command boundary and keep Nijiiro on the current common DTO path.
3. Migrate AC15 adapter playresult mappers to produce the envelope using Mapperly-native features.
4. Migrate `Ac15NormalStageFilter`, `Ac15CommonProfileMutation`, `Ac15NormalPlayWriter`, and `Ac15DaniWriter` to consume capability records directly.
5. Migrate Blue battle, Tokkun, Green ghost, and Red Tokkun helper paths to consume the relevant capability records.
6. Remove AC15 usage of `CommonPlayResultData` from handlers and shared writers.
7. Prune obsolete AC15 fields from `CommonPlayResultData` only if Nijiiro and other non-AC15 consumers no longer need them; otherwise leave cleanup to a separate Nijiiro/common DTO task.
8. Build and test until AC15 playresult mappers no longer emit union-target Mapperly warnings.

Use scoped checkpoint commits if implementation becomes multi-step.

## Acceptance Criteria

The redesign is complete when:

- AC15 playresult mappers no longer target `CommonPlayResultData`.
- AC15 shared writers no longer require `CommonPlayResultData`.
- Mapperly `RMG012` warnings caused by all-era union target mapping are gone from Blue, Green, Yellow, and Red playresult mapper builds.
- Existing AC15 playresult behavior and no-write boundaries are preserved.
- Nijiiro behavior is unchanged.
- Blue, Green, Yellow, and Red persistence remains era-owned.
- No generated `Wire/` files or proto inputs are manually edited.
- Tests cover observable behavior and no-cross-era/no-cross-mode boundaries rather than mapper source shape.

## Design Decisions

- Prefer an AC15 capability envelope over era-specific top-level inputs because it fixes the mapper target problem while keeping handler orchestration and shared writer binding straightforward.
- Keep handlers as explicit workflow roots instead of hiding mode ordering behind generic hooks.
- Keep Mapperly as the mechanical projection layer and use its native configuration surface before manual helpers.
- Make capability absence explicit through `null` capability records.
- Treat Red ChallengeCompe as preserved input facts only in this refactor; Phase 21 owns stateful capability semantics.
