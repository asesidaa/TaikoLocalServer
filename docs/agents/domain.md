# Domain Docs

How the engineering skills should consume this repo's domain documentation when exploring the codebase.

## Layout

This repo uses a multi-context documentation layout:

- Root project context: `CONTEXT.md`
- Context map entry point: `CONTEXT-MAP.md`
- System-wide ADRs: `docs/adr/`
- Nijiiro CHN era context: `Adapters.GameProtocol.CnR00/CONTEXT.md`
- Nijiiro WW era context: `Adapters.GameProtocol.WwR08/CONTEXT.md`
- Green era context: `Adapters.GameProtocol.Green/CONTEXT.md`
- Blue era context: `Adapters.GameProtocol.Blue/CONTEXT.md`
- WebUI context: `TaikoWebUI/CONTEXT.md`

Context-specific ADRs may live under each context's `docs/adr/` directory, for example `Adapters.GameProtocol.Green/docs/adr/`.

## Before exploring, read these

- `CONTEXT-MAP.md` at the repo root if it exists. It points at the context files relevant to each area.
- `CONTEXT.md` at the repo root for overall project/server vocabulary and constraints.
- The area-specific `CONTEXT.md` for the adapter, era, or WebUI area being changed.
- `docs/adr/` for system-wide architectural decisions.
- The area-specific `docs/adr/` directory when one exists.

If any of these files don't exist, proceed silently. Don't flag their absence or suggest creating them upfront. The producer skill (`/grill-with-docs`) creates them lazily when terms or decisions actually get resolved.

## Use the glossary's vocabulary

When your output names a domain concept (in an issue title, a refactor proposal, a hypothesis, a test name), use the term as defined in the relevant `CONTEXT.md`. Don't drift to synonyms the glossary explicitly avoids.

If the concept you need isn't in the glossary yet, either you are inventing language the project doesn't use or there is a real gap. Reconsider the wording, or note the gap for `/grill-with-docs`.

## Flag ADR conflicts

If your output contradicts an existing ADR, surface it explicitly rather than silently overriding it.
