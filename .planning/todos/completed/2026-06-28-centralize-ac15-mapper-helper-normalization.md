---
created: 2026-06-28T18:44:34.636Z
title: Centralize AC15 mapper helper normalization
area: general
files:
  - Application/Ac15/Ac15UserDataService.cs:85
  - Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs:26
  - Adapters.GameProtocol.Blue/Mappers/UserDataMappers.cs:29
  - Adapters.GameProtocol.White/Mappers/UserDataMappers.cs:30
  - Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs:119
  - Adapters.GameProtocol.Murasaki/Mappers/PlayResultMappers.cs:135
  - Adapters.GameProtocol.Momoiro/Mappers/PlayResultMappers.cs:136
  - Adapters.GameProtocol.Blue/Mappers/BaidResponseMapper.cs:36
  - Adapters.GameProtocol.Momoiro/Mappers/BaidResponseMapper.cs:36
---

## Problem

AC15 protocol mapper helpers have accumulated cross-era duplication after successive Blue, Green, Yellow, Red, White, Murasaki, KIMIDORI, and MOMOIRO mapper work. The debt is not just cosmetic: several helpers encode normalization and protocol-limit behavior directly in adapter mapper files even though shared AC15 services/profiles already own the same concepts.

Concrete examples from the review:

- Taikojuku display-Dan normalization is repeated in userdata mappers with hard-coded `1..25` / fallback `1`, while `Ac15UserDataService` already normalizes through `Ac15EraProfile.Limits`.
- PlayResult mappers repeat the same helper conversions across eras: null-to-empty string/bytes, nullable uint defaulting, difficulty conversion, PlayDan zero-to-null, current-costume presence, list conversion, and stage/compe list mapping.
- BAID response mappers normalize costume and Dan flag byte arrays through a mix of era `*ProtocolBytes` helpers and `Ac15ProtocolBytes` plus `Ac15EraProfiles`, even though `Ac15ProtocolLimits` is the shared byte-width authority.
- Low-risk but noisy one-liners such as recommend-best-song `List<uint>` to `uint[]`, telop present-string normalization, and Taikojuku `Difficulty` to protocol conversion are repeated in many adapter projects.

The current Mapperly docs point to the right implementation mechanism: use external mappings (`UseStaticMapper` or direct `Use = nameof(@Helper.Method)` references) so these rules can be centralized without replacing Mapperly source-generated projections with handwritten mapper bodies.

## Solution

Plan and implement a focused mapper-helper cleanup that keeps Mapperly in control of mechanical projection while moving shared AC15 normalization into explicit shared helper surfaces.

Suggested approach:

1. Re-read current Mapperly external mapping docs before implementation: `https://mapperly.riok.app/docs/configuration/external-mappings/`.
2. Add shared AC15 Mapperly helper class(es) for protocol conversion primitives: nullable/default conversions, byte/list normalization, difficulty conversion, PlayDan zero-to-null, recommend-best-song array conversion, and display-Dan normalization.
3. Route adapter mapper attributes through external static mapper methods instead of per-era private helpers where the rule is genuinely shared.
4. Keep era-owned decisions local where wire shape or semantics differ, especially final-vs-legacy wire boundaries and mode-specific Blue battle / Green ghost / MOMOIRO crown behavior.
5. Prefer `Ac15EraProfile` / `Ac15ProtocolLimits` as the source for byte widths, Dan bounds, and fallback values instead of hard-coded constants in mapper files.
6. Verify with `dotnet build TaikoLocalServer.slnx /p:EmitCompilerGeneratedFiles=true` or the temp-output Host build fallback if the running server locks normal output, then inspect the emitted Mapperly `.g.cs` files for representative adapters.

Acceptance criteria:

- Shared mapper helpers cover duplicated AC15 normalization rules without handwritten mapper bodies.
- No adapter mapper contains repeated `1..25` display-Dan fallback logic.
- BAID flag normalization uses one shared AC15 byte-width path for shared eras, while preserving proven era-specific exceptions.
- Representative generated Mapperly source confirms external helpers are invoked as expected.
- Focused mapper or handler tests are added only where they protect behavior or no-cross-era/no-cross-mode boundaries.
