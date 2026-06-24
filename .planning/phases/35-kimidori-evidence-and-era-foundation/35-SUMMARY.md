# Phase 35 Summary: KIMIDORI Evidence and Era Foundation

## Implemented

- Recorded KIMIDORI route/proto/data evidence from `proto/kimidori`, `.tools/kimidori/EBOOT.ELF.i64`, and linked root-level data assumptions.
- Generated adapter-local KIMIDORI wire DTOs from existing proto inputs without editing `proto/`.
- Added `GameEra.Kimidori`, Host settings, adapter project registration, solution/project references, application-part gating, and `/v05r00/chassis/*.php` route ownership.
- Kept KIMIDORI controllers separated from Murasaki, including scaffold-state controllers.
- Moved copied unsupported scaffold files to `.planning/batch-delete/kimidori-scaffold/` for user-managed cleanup instead of deleting them directly.

## Preserved Boundaries

- No `proto/` edits.
- No KIMIDORI legacy wire route family.
- No KIMIDORI Taikojuku, Tokkun, Banacoin, battle, Don Challenge, or ChallengeCompe route/state support.
- Shared `/v01r00/chassis/*` startup/version behavior remains shared AC15 behavior.

