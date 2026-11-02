# Phase 4: Blue Battle Evidence And Design - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md - this log preserves the alternatives considered.

**Date:** 2026-05-30T03:20:08.4800704+08:00
**Phase:** 4-Blue Battle Evidence And Design
**Areas discussed:** Battle evidence capture, Battle data-file inventory, Battle wire/default-state contract, Battle playresult effects, Phase 5 runtime boundary

---

## Battle Evidence Capture

| Option | Description | Selected |
|--------|-------------|----------|
| RPCS3 logs first | Require a repeatable RPCS3 battle menu/attempt run with server logs and request/response artifacts before runtime code. | |
| IDA/proto first | Allow static client/proto evidence to define the design before cabinet/RPCS3 battle attempts are available. | Yes |
| Both required | Require both client/proto analysis and at least one repeatable RPCS3/cabinet battle attempt before Phase 5 starts. | |

**User's choice:** IDA/proto first.
**Notes:** Do not use RPCS3 logs here; those are only used in verification stage.

| Option | Description | Selected |
|--------|-------------|----------|
| IDA/client wins | Use proto/wire and XML to find candidates, but lock semantics only when client behavior proves them. | Yes |
| Proto/wire wins | Treat `proto/blue/taiko.proto` and generated wire types as the contract unless IDA clearly contradicts it. | |
| Data files win | Treat the Blue battle XML files as the practical source of defaults and row domains unless client code contradicts them. | |

**User's choice:** IDA/client wins.

| Option | Description | Selected |
|--------|-------------|----------|
| Traceable evidence pack | Produce a design doc with field-by-field source citations, local file inventory, IDA/client notes, and explicit unknowns. | Yes |
| Lean design only | Produce only the battle design decisions and defer raw evidence details to comments or future investigation. | |
| Source guards only | Rely mainly on tests/source guards plus a short notes file rather than a full evidence pack. | |

**User's choice:** Traceable evidence pack.

| Option | Description | Selected |
|--------|-------------|----------|
| Block runtime field | Phase 5 must not implement that field's behavior; document the gap and keep it omitted or inert. | |
| Allow safe stub | Permit zero/empty/default responses when they seem low risk, with follow-up verification later. | |
| Ask case by case | Stop and ask for each unresolved field before writing the Phase 5 plan. | Yes |

**User's choice:** Ask case by case.

---

## Battle Data-File Inventory

| Option | Description | Selected |
|--------|-------------|----------|
| Candidate inputs | Inventory and parse them as candidate menu/runtime data, but do not mark any file required until client evidence proves it. | Yes |
| Required unless empty | Treat all present battle XML files as required inputs unless a file is empty or clearly unused. | |
| Reference only | Use them only as human-readable references; do not plan loaders until IDA/client evidence names the files. | |

**User's choice:** Candidate inputs.

| Option | Description | Selected |
|--------|-------------|----------|
| All battle files | Inventory `battleadjsetting`, `battlenpcinfo`, `battlestageinfo`, `battlesupportinfo`, and `battletokeninfo` with row counts and key fields. | Yes |
| Entry path only | Focus only on files needed for battle menu entry and first stage assignment. | |
| Client-named only | Inventory only files directly referenced by IDA/client strings or file-open traces. | |

**User's choice:** All battle files.

| Option | Description | Selected |
|--------|-------------|----------|
| Document only | Write field/row inventory and evidence notes; leave committed runtime JSON/loaders for Phase 5 after the design gate. | Yes |
| Parsed summaries | Create committed non-runtime summaries, such as CSV/Markdown tables, to support review without adding loaders. | |
| Runtime JSON now | Generate committed Blue battle JSON defaults during Phase 4, even before runtime code consumes them. | |

**User's choice:** Document only.

| Option | Description | Selected |
|--------|-------------|----------|
| Evidence-local only | Phase 4 may use local files as evidence, but Phase 5 must treat runtime data as operator-supplied and validate missing files explicitly. | |
| Commit fixtures | Create small committed battle fixtures so development and tests do not need local game data. | |
| Require local data | Make missing battle files a hard prerequisite for all Phase 4 planning and Phase 5 implementation work. | Yes |

**User's choice:** Require local data.

---

## Battle Wire/Default-State Contract

| Option | Description | Selected |
|--------|-------------|----------|
| All battle arrays | Require proof for release_info_flg, release_battle_stage_flg, release_battle_special_flg, NPC costume/special flags, and any token/state arrays. | Yes |
| Menu flags only | Require explicit proof only for initialdatacheck and battleuserdata menu-entry flags; defer deeper NPC arrays. | |
| Infer from max IDs | Allow Phase 5 to size arrays from the highest IDs in battle XML unless IDA contradicts it. | |

**User's choice:** All battle arrays.

| Option | Description | Selected |
|--------|-------------|----------|
| Omit unknowns | Use protobuf presence semantics and omit optional battle fields until evidence proves values and widths. | Yes |
| Zero-fill arrays | Send fixed zero arrays for byte fields once candidate widths are inferred, even before behavior is proven. | |
| Return error | Make battle endpoints fail or return non-success until all fields are proven. | |

**User's choice:** Omit unknowns.

| Option | Description | Selected |
|--------|-------------|----------|
| Prove minimum rows | Document exactly whether zero rows, one default row, or full catalog rows are accepted; Phase 5 cannot guess. | Yes |
| Start empty | Assume empty repeated lists are safe unless later verification proves otherwise. | |
| Mirror XML rows | Plan to return one row per XML/catalog entry unless IDA/client behavior says not to. | |

**User's choice:** Prove minimum rows.

| Option | Description | Selected |
|--------|-------------|----------|
| Presence and width tests | Require tests for ShouldSerialize/omission behavior, exact byte lengths, required repeated-row counts, and no Green AI Battle references. | Yes |
| Source guards only | Use broad source guards to prevent Green battle leakage, but leave byte/default checks to manual review. | |
| Runtime smoke only | Rely on Phase 6 cabinet/RPCS3 smoke to validate wire/default behavior after implementation. | |

**User's choice:** Presence and width tests.

---

## Battle Playresult Effects

| Option | Description | Selected |
|--------|-------------|----------|
| Isolate by default | Battle stages do not update normal self-best/crown/history unless Phase 4 evidence explicitly approves that path. | |
| Merge normal fields | Because battle playresult contains normal stage fields, plan to update normal score/crown unless evidence forbids it. | |
| Ask per field | Decide separately for history, self-best, crown, favorite/recent, and profile counters during Phase 5 planning. | |
| Freeform | Per linked Wiki, score/crowns are recorded separately; Wiki is an important source of truth. | Yes |

**User's choice:** Freeform.
**Notes:** Use Wiki gameplay evidence as an important source of truth for gameplay semantics, while still using IDA/proto/client evidence for wire mechanics and runtime safety.

| Option | Description | Selected |
|--------|-------------|----------|
| Battle-only progress | Persist battle stage/progression, bonds level, best battle power, NPC/token/special state, and rewards, while keeping score/crown separate from normal Blue. | Yes |
| Minimal progress | Persist only enough to reopen battle mode safely: stage assignment, last boss/stage, and release flags. | |
| Full event model | Persist detailed battle result history, damage, critical/special counts, NPC state changes, and every unlock/reward event. | |

**User's choice:** Battle-only progress.

| Option | Description | Selected |
|--------|-------------|----------|
| Allow proven unlocks | Permit battle playresult to update normal Blue save unlock bits only for evidence-backed rewards like titles, songs, costumes, or medals. | |
| Battle-only rewards | Keep all battle rewards inside battle tables until cabinet verification proves normal save-state readback needs them. | |
| Normal unlock mirror | Treat all battle reward arrays as normal Blue unlocks whenever they use existing playresult release fields. | Yes |

**User's choice:** Normal unlock mirror.

| Option | Description | Selected |
|--------|-------------|----------|
| Battle-only history | Do not write `SongPlayDataBlue`, normal recent/favorites, profile play counts, or normal best/crown for battle stages. | Yes |
| Normal history too | Write normal play history/profile counters but keep self-best/crowns separate. | |
| Decide by evidence | Leave history/profile writes unresolved until Wiki/client evidence specifically addresses them. | |

**User's choice:** Battle-only history.

---

## Phase 5 Runtime Boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Approved design gate | Require Phase 4 evidence pack plus a battle design spec with user-approved unresolved cases before Phase 5 planning/execution. | Yes |
| Evidence pack enough | Allow Phase 5 planning once the evidence pack exists, even if the design spec has unresolved cases. | |
| Plan can decide | Let the Phase 5 planner resolve remaining implementation choices from the Phase 4 evidence pack. | |

**User's choice:** Approved design gate.

| Option | Description | Selected |
|--------|-------------|----------|
| Blue-owned only | Use Blue entities, handlers, mappers, controllers, migrations, tests, and catalog types; Green AI Battle is contrast only. | Yes |
| Shared helpers allowed | Allow shared helpers if names are neutral and tests prove no Green state or Green wire dependency. | |
| Reuse Green shape | Allow Green AI Battle code structure to be adapted when convenient, while keeping persistence tables Blue-owned. | |

**User's choice:** Blue-owned only.

| Option | Description | Selected |
|--------|-------------|----------|
| Full battle loop | Implement battleuserdata, initialdatacheck battle flags, battle playresult persistence, rewards/unlocks, progression, tests, and source guards. | Yes |
| Entry/readback first | Implement only initialdatacheck and battleuserdata safe state first; defer playresult/progression to a later phase. | |
| Vertical thin slice | Implement one proven stage/NPC/token flow end to end, leaving broader battle catalog coverage for follow-up. | |

**User's choice:** Full battle loop.

| Option | Description | Selected |
|--------|-------------|----------|
| Automated proof | Phase 5 must pass focused unit/integration tests, source guards, mapper presence/width tests, full tests, and temp Host build; cabinet/RPCS3 proof remains Phase 6. | Yes |
| Minimal tests | Phase 5 only needs targeted battle tests and build; broader proof happens in Phase 6. | |
| Require smoke too | Phase 5 is not done until a battle RPCS3/cabinet smoke run also passes. | |

**User's choice:** Automated proof.

## the agent's Discretion

None.

## Deferred Ideas

- RPCS3/cabinet battle logs are deferred to verification-stage proof.
- Runtime Blue battle JSON/default data generation is deferred until Phase 5, after the Phase 4 design gate.
