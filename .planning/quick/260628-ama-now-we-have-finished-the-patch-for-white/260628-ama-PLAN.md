---
quick_id: 260628-ama
slug: now-we-have-finished-the-patch-for-white
status: complete
created: 2026-06-27
---

# Quick Task 260628-ama: Port Taikojuku client patch candidates

## Goal

Check whether the White Taikojuku RPCS3 patch can be ported to older Taikojuku-capable binaries, including White final and currently Murasaki plus Murasaki final.

## Tasks

1. [x] Establish source patch and IDA targets.
   - Files: `.tools/white/rpcs3_taikojuku_pack1_patch.yml`, `.tools/white-final/EBOOT.ELF.i64`, `.tools/murasaki-final/EBOOT.ELF.i64`, `.tools/murasaki/EBOOT.ELF.i64`
   - Action: Treat the era-local White patch as source of truth, connect to existing IDA daemons or start target daemons, and identify equivalent Taikojuku row builder and gate sites.
   - Verify: Record exact binary addresses and explain any non-portable differences.

2. [x] Produce patch artifacts where binary equivalence is strong enough.
   - Files: `.tools/white-final/rpcs3_taikojuku_pack1_patch.yml`, `.tools/murasaki-final/rpcs3_taikojuku_pack1_patch.yml`, `.tools/murasaki/rpcs3_taikojuku_pack1_patch.yml`
   - Action: Port address-specific gate edits and hook bodies only when IDA evidence proves matching control-flow and helper contracts.
   - Outcome: No patch artifacts were emitted because no target had enough verified equivalence for a safe full hook drop-in.
   - Verify: Confirm candidate patch paths remain absent and record the target-specific rewrite requirements.

3. [x] Summarize evidence and limits.
   - Files: `.planning/quick/260628-ama-now-we-have-finished-the-patch-for-white/260628-ama-SUMMARY.md`, `.planning/STATE.md`
   - Action: Document what was ported, what was not, and what runtime validation remains.
   - Verify: Patch YAML parses structurally and git status contains only intended quick-task artifacts and patch files.
