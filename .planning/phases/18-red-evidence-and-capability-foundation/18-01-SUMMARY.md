---
phase: 18-red-evidence-and-capability-foundation
plan: 01
subsystem: planning
tags: [red, ac15, evidence, ida, capability-matrix]

requires:
  - phase: 17
    provides: "Closed Yellow runtime contract and v1.3 Red milestone start"
provides:
  - "Canonical Red route/version/root evidence artifact"
  - "Phase-owner Red capability matrix"
  - "Explicit Red preservation and absent-surface guardrails"
affects: [phase-18, phase-19, phase-20, phase-21, red-ac15]

tech-stack:
  added: []
  patterns:
    - "IDA/proto/data evidence summarized before Red runtime implementation"
    - "Capability rows grouped by owning future phase"

key-files:
  created:
    - ".planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md"
  modified: []

key-decisions:
  - "Red game routes are documented as /v08r01 while shared startup/version stays on /v01r00."
  - "ST8100-1 is the active Red runtime root; ST5100-* and ST7100-1 are inactive or historical only."
  - "Red ChallengeCompe is a shared older-AC15 candidate, not a Red-only stateful feature."
  - "Red gameplay persistence, EF migrations, Ac15EraProfiles.Red, AdminApi, and WebUI remain out of Plan 18-01 scope."

patterns-established:
  - "Phase evidence artifacts must keep proto-only candidates separate from IDB-known route suffixes."
  - "Banacoin-adjacent Red rows are compatibility/probe-only until runtime evidence proves more."

requirements-completed:
  - RFND-01
  - RFND-03

duration: 7 min
completed: 2026-06-12
---

# Phase 18 Plan 01: Red Evidence Artifact and Capability Matrix Summary

**IDA-backed Red route/root evidence and phase-owned capability matrix before Red runtime implementation.**

## Performance

- **Duration:** 7 min
- **Started:** 2026-06-12T21:01:43Z
- **Completed:** 2026-06-12T21:08:57Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments

- Created `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md`.
- Recorded `/v08r01`, shared `/v01r00`, startup/version suffix addresses, active `ST8100-1` root evidence, local Red data inventory, and unresolved transport/protocol-limit gaps.
- Added the Red capability matrix grouped by Phase 18 foundation, Phase 19 catalog/profile, Phase 20 runtime/simple compatibility, Phase 21 ChallengeCompe, and explicit absent surfaces.

## Task Commits

1. **Task 1: Record Red route, version, transport, and active-root evidence** - `89392b36` (docs)
2. **Task 2: Add the Red capability evidence matrix and preservation guardrails** - `4cf31ff2` (docs)

## Files Created/Modified

- `.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md` - Canonical Red evidence artifact with route/root proof, proto-only caveats, capability matrix, and guardrails.

## Decisions Made

- Red game prefix is documented as `/v08r01/chassis/*`, with shared startup/version ownership kept on `/v01r00/chassis/*`.
- `ST8100-1` is documented as the active Red runtime root from IDA evidence; `ST5100-*` and `ST7100-1` are historical/inactive local roots.
- `challengecompe.php` is documented as a shared older-AC15 candidate owned by Phase 21, not a Red-only stateful feature.
- Plan 18-01 explicitly keeps Red gameplay persistence, EF migrations, `Ac15EraProfiles.Red`, AdminApi, WebUI, wallet/payment authority, item shop, battle, and WaiWai out of scope.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Corrected metadata progress helper output**
- **Found during:** Metadata closeout
- **Issue:** `roadmap.update-plan-progress` checked off Plan 18-01 but also rewrote the high-level Phase 18 summary row into malformed progress text and missed the actual `Phase Progress` row. `state.update-progress` updated completed plan counts but left the frontmatter percent stale.
- **Fix:** Restored the Phase 18 summary row, updated the intended `Phase Progress` row to `1/4 In Progress`, aligned RFND-01/RFND-03 roadmap coverage statuses with `REQUIREMENTS.md`, and corrected `.planning/STATE.md` progress percent to `25`.
- **Files modified:** `.planning/ROADMAP.md`, `.planning/STATE.md`
- **Verification:** Reviewed the `ROADMAP.md` and `STATE.md` diffs after correction.
- **Committed in:** Plan metadata commit

---

**Total deviations:** 1 auto-fixed (Rule 1 bug).
**Impact on plan:** Metadata consistency fix only; no Red runtime, adapter, route, wire, Host, or gameplay behavior was added.

## Issues Encountered

Roadmap/state progress helper output needed the metadata correction documented above.

## Validation

- `Test-Path '.planning/phases/18-red-evidence-and-capability-foundation/18-RED-EVIDENCE.md'` - passed.
- Task 1 `Select-String` checks for `/v08r01`, `/v01r00`, IDB route/startup/root addresses, `ST8100-1`, `direct-protobuf`, and unresolved evidence gaps - passed.
- Task 2 `Select-String` checks for the capability matrix groups, ChallengeCompe, Banacoin/reward proto rows, proto-only candidates, and preservation guardrails - passed.
- `git diff --name-only -- proto/red .tools/red` - passed with no path output.
- Stub marker scan over `18-RED-EVIDENCE.md` - passed with no matches.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Ready for Plan 18-02. The evidence artifact gives the next plan the Red route prefix, shared startup/version boundary, active root, immutable proto inputs, and capability guardrails needed before adapter/wire foundation work.

## Self-Check: PASSED

- Verified `18-RED-EVIDENCE.md` exists.
- Verified `18-01-SUMMARY.md` exists.
- Verified task commits `89392b36` and `4cf31ff2` exist in git history.

---
*Phase: 18-red-evidence-and-capability-foundation*
*Completed: 2026-06-12*
