---
quick_id: 260628-ama
slug: now-we-have-finished-the-patch-for-white
status: complete
completed: 2026-06-27
---

# Quick Task 260628-ama Summary

## Result

No new RPCS3 patch YAML was created.

The White source patch at `.tools/white/rpcs3_taikojuku_pack1_patch.yml` is not a safe drop-in port to White final or base Murasaki. Those builds have the same type-9 suppression gates, but their row emitters use the newer inline `r8` row path instead of White's helper-style `r4` append path.

Murasaki final is the only full-hook candidate found in this pass. It has the same helper-style `li r4, 0xD` row site as patched White, but the hook still needs a target-specific assembly pass because the helper addresses, raw table constants, and return address are different. I did not emit a gate-only patch because that would be partial and easy to confuse with the full Taikojuku pack-row patch.

## Binary Evidence

| Target | Taikojuku present | Row builder | Count gate | Emit gate | Row site | Shape | Decision |
|--------|-------------------|-------------|------------|-----------|----------|-------|----------|
| White source | Yes | `sub_67DC98` | `0x67DD3C: xori r9,r9,9` | `0x67DE0C: beq cr7, loc_67DE44` | `0x67EB7C: li r4,0xD` | Helper append through `sub_679DC8`, continuation `0x67DE40` | Existing source patch applies here |
| White final | Yes | `sub_719B18` | `0x719C10: xori r9,r9,9` | `0x719CD4: beq cr7, loc_719D64` | `0x71AD24: li r8,0xD` | Inline vector append at `0x719CE4`, no helper-style `r4` path | Gate-only portable; full hook needs rewrite |
| Murasaki final | Yes | `sub_67C5E4` | `0x67C688: xori r9,r9,9` | `0x67C74C: beq cr7, loc_67C784` | `0x67D4BC: li r4,0xD` | Helper append through `sub_67A8AC`, continuation `0x67C780` | Best full-hook port candidate, but not drop-in |
| Murasaki | Yes | `sub_5D7A00` | `0x5D7A98: xori r9,r9,9` | `0x5D7B68: beq cr7, loc_5D7BF8` | `0x5D8A24: li r8,0xD` | Inline vector append at `0x5D7B78`, no helper-style `r4` path | Gate-only portable; full hook needs rewrite |

## Loader/Helper Notes

- White final Taikojuku response loader is `sub_CAB08`; raw pack table base appears as `unk_F2DF18`.
- Murasaki final Taikojuku response loader is `sub_91E80`; raw pack table base appears as `unk_DF3A88`.
- Murasaki Taikojuku response loader is `sub_8FC7C`; raw pack table base appears as `unk_D44A28`.
- Murasaki final raw-pack helpers near the row builder include `sub_8BF6C`, `sub_8BFCC`, and `sub_8BFE8`; its row append helper is `sub_67A8AC`.
- White final/base Murasaki share the newer inline row-emitter family where helpers like `sub_714C9C`/`sub_714E00` and `sub_5D60F0`/`sub_5D6844` handle backing-vector work, so the White hook body cannot be relocated mechanically.

## Recommendation

1. Port Murasaki final first if a full patch is needed. Use `0x67D4BC` as the hook site, preserve the original `0x0D` append by calling `sub_67A8AC`, return to `0x67C780`, and reassemble the raw `0x11` row block with Murasaki-final helper/table constants after validating the selected-pack field offset.
2. Treat White final and base Murasaki as new hook work, not simple ports. Their `r8` inline append path must be emulated or safely bypassed before adding the raw `0x11` row.
3. Do not publish gate-only YAML as a Taikojuku pack patch. It can remove the type-9 suppression but does not implement the White patch's raw practice-pack row behavior.

## Verification

- Connected to IDA daemons for `.tools/white-final/EBOOT.ELF.i64`, `.tools/murasaki-final/EBOOT.ELF.i64`, and `.tools/murasaki/EBOOT.ELF.i64`.
- Queried the existing White daemon directly on port `64698` because its daemon registry entry used an older relative path hash.
- Verified Taikojuku route/protocol strings in all three target IDBs.
- No RPCS3 runtime validation was performed.
