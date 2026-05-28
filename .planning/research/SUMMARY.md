# Blue Support Research Summary

**Date:** 2026-05-28
**Mode:** constrained project research

## Research Boundary

The user asked for research only when the original Superpowers spec is unclear or insufficient. This pass therefore used the existing Blue roadmap/specs, codebase map, current source files, and local docs as the primary evidence base. No broad external research was needed to define GSD requirements.

## Main Findings

- A0-A5 are completed prior work and should be represented as validated project capabilities.
- A6 has enough local design input to proceed to requirements and planning: Blue item-shop catalog loading already exists, Green item-shop behavior provides a proven local analog, and the Blue roadmap defines the remaining behavior.
- A7 has enough local design input to proceed: WebUI era routing already recognizes Blue, and AdminApi has some Blue Dani/game-data branches from A5.
- A8 should be treated as a verification and hardening phase, not just more server implementation.
- Track B is intentionally under-specified for runtime implementation. It needs a battle evidence/spec phase before battle code.

## Requirements Implications

- Requirements should separate A6, A7, A8, battle evidence/design, battle implementation, and final full-Blue smoke verification.
- Track B belongs in v1 because the user chose full Blue scope, but runtime battle implementation must come after strict evidence collection.
- Tokkun, Banacoin/payment, Yellow-or-earlier support, Green AI Battle changes, and runtime scraping remain out of scope.

## Roadmap Implications

Recommended roadmap order:

1. Blue A6 item shop and unlocking.
2. Blue A7 AdminApi and WebUI parity.
3. Blue A8 normal-mode cabinet smoke and hardening.
4. Blue battle evidence and design.
5. Blue battle implementation.
6. Full Blue cabinet/RPCS3 smoke and release hardening.

## Open Questions For Later Phases

- Which official/curated Blue shop season data should ship by default?
- Which Blue WebUI edit surfaces are safe for first release versus read-only?
- Which battle data files under `config/S10100-1/battle` are required for menu entry?
- What are the exact byte widths/defaults for battle release flags, NPC state, costumes, specials, tokens, and stage assignments?
- Do battle playresults update normal Blue self-best/crown state?

