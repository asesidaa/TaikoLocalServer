# Phase 23: White Evidence and Era Foundation - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md - this log preserves the alternatives considered.

**Date:** 2026-06-17
**Phase:** 23-White Evidence and Era Foundation
**Areas discussed:** Route/root proof source, Scaffold breadth, Absence contracts, Stale planning correction

---

## Route/root proof source

| Option | Description | Selected |
|--------|-------------|----------|
| IDB route strings (Recommended) | Use the current nonzero White IDB for route strings/xrefs first, with fallback only if IDA access is blocked. | Yes |
| Any local evidence | Accept IDB strings, RPCS3/cabinet HTTP logs, captures, or another local route artifact as equivalent proof. | |
| User approval | Allow an explicit user-named prefix if local route evidence is incomplete, with the source clearly recorded. | |

**User's choice:** IDB route strings. User note: "Should be /v07r00 for game and /v01r00 for startup auth".
**Notes:** Follow-up locked `/v07r00` as expected but still to be verified from White IDB route strings before route code is finalized.

| Option | Description | Selected |
|--------|-------------|----------|
| Verify then lock (Recommended) | Treat `/v07r00` as the expected prefix, but require Phase 23 to confirm it from White IDB route strings before route code is finalized. | Yes |
| User-locked now | Record the user's `/v07r00` statement as sufficient route-prefix authority, with IDB used only as supporting evidence. | |
| Candidate only | Record `/v07r00` as a lead, but keep route scaffolding blocked until binary or runtime evidence confirms it. | |

**User's choice:** Verify then lock.
**Notes:** Route code should not be finalized from filename guessing, Red/Yellow precedent, or proto-only presence.

| Option | Description | Selected |
|--------|-------------|----------|
| IDB/root strings (Recommended) | Use White IDB config-root strings or equivalent client evidence; local file presence alone is not enough. | |
| Data inventory enough | Treat the existing `Host/wwwroot/data/white/data/config/ST7100-1` inventory as sufficient for Phase 23 foundation. | Yes |
| Leave unresolved | Record `ST7100-1` as available local data but defer active-root proof to Phase 24 catalog work. | |

**User's choice:** Data inventory enough.
**Notes:** User clarified no extra check is needed because this is a symlink of the game directory.

| Option | Description | Selected |
|--------|-------------|----------|
| Verify representative DTOs (Recommended) | Confirm White game routes use direct request DTOs and White `vsinterface` remains compatible with shared `/v01r00` startup/version routes. | |
| Carry AC15 default | Assume direct protobuf and shared startup/version from older AC15 precedent unless the White IDB/proto contradicts it. | Yes |
| Runtime capture required | Block transport/fallback decisions until an RPCS3/cabinet HTTP capture confirms request framing and content-type behavior. | |

**User's choice:** Carry AC15 default.
**Notes:** User clarified no extra check is needed because this is the same across versions.

---

## Scaffold breadth

| Option | Description | Selected |
|--------|-------------|----------|
| IDB-confirmed set (Recommended) | Create route scaffolds only for White suffixes confirmed by IDB route strings and matching `proto/white` request/response pairs. | Yes |
| Full proto set | Scaffold every request/response pair in `proto/white` once `/v07r00` is verified, even if route-string proof is incomplete. | |
| Adapter only | Add generated wire, project, enum, Host gating, and config only; defer all concrete controllers to later phases. | |

**User's choice:** IDB-confirmed set.
**Notes:** User clarified: "Do not over complicate it, just search for .php in IDB and you get the list".

| Option | Description | Selected |
|--------|-------------|----------|
| Thin log/success (Recommended) | Deserialize White wire DTOs, log bounded request info, and return success/default responses only where safe, with no Mediator or EF writes. | Yes |
| Mediator shells | Wire route controllers through Mediator request types immediately, even if handlers still return no-state defaults. | |
| No responses yet | Create route files and tests only; leave response behavior unimplemented until catalog/runtime phases. | |

**User's choice:** Thin log/success.
**Notes:** Phase 23 should not add runtime behavior.

| Option | Description | Selected |
|--------|-------------|----------|
| Scope to `/v07r00` (Recommended) | Add White only to the existing exact-prefix AC15 fallback after `/v07r00` is verified, preserving disabled-era route safety. | Yes |
| Delay fallback | Do not add White to the fallback until a runtime HTTP capture proves White omits or blanks `Content-Type`. | |
| General AC15 fallback | Replace exact route prefixes with a broader AC15/chassis fallback that includes White by pattern. | |

**User's choice:** Scope to `/v07r00`.
**Notes:** Preserve exact-prefix fallback behavior.

| Option | Description | Selected |
|--------|-------------|----------|
| Foundation only (Recommended) | Stop at generated wire, enum/config/Host gating, route scaffolds, fallback scope, and preservation checks; no catalog/profile/runtime state. | Yes |
| Include catalog paths | Also add `WhiteGameDataPaths` and required file checks while leaving runtime state for Phase 24. | |
| Include profile shell | Also add `Ac15EraProfiles.White` with provisional limits/feature flags for later phases to refine. | |

**User's choice:** Foundation only.
**Notes:** Catalog/profile/runtime state belongs to later White phases.

---

## Absence contracts

| Option | Description | Selected |
|--------|-------------|----------|
| Full absence matrix (Recommended) | Record item shop, Banacoin authority, battle, Tokkun, WaiWai, gacha, later White, and Red-style ChallengeCompe as absent unless White evidence proves otherwise. | |
| Major gaps only | Call out only the biggest risks: item shop, battle, Tokkun, and standalone ChallengeCompe. | |
| Keep it brief | Use a short scope note and leave detailed absence handling to later research/planning. | |
| None of the above | User supplied a simpler rule. | Yes |

**User's choice:** None of the above.
**Notes:** User clarified: "If something is not supported, we ignore it. If it is previously in a shared bahavior/capability, we extract it and do not wire for white. This should be very simple and clear". CONTEXT.md records this as the implementation rule without a feature-by-feature matrix.

---

## Stale planning correction

| Option | Description | Selected |
|--------|-------------|----------|
| Correct in Phase 23 (Recommended) | Record current IDB file-size evidence in the Phase 23 evidence artifact and update stale active planning notes touched by this phase. | Yes |
| Record only | Leave older milestone-start notes alone; only the new Phase 23 context/evidence artifact says the IDB is now usable. | |
| Ignore stale note | Do not mention the old zero-byte note; planners should use current filesystem evidence directly. | |

**User's choice:** Correct in Phase 23.
**Notes:** Current filesystem evidence observed during discussion: `.tools/white/EBOOT.ELF.i64` length `129893515` bytes.

## the agent's Discretion

None.

## Deferred Ideas

None.
