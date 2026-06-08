# Phase 16: Yellow Tokkun and Banacoin Compatibility - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md - this log preserves the alternatives considered.

**Date:** 2026-06-08
**Phase:** 16-Yellow Tokkun and Banacoin Compatibility
**Areas discussed:** Yellow Tokkun classification and acceptance, Tokkun no-cross-write boundary, Yellow Tokkun persistence shape, Yellow userdata tutorial readback, Yellow Banacoin-adjacent compatibility

---

## Yellow Tokkun Classification and Acceptance

| Option | Description | Selected |
|--------|-------------|----------|
| Use `PlayMode.Tokkun = 3` plus Tokkun section evidence | Primary classifier is runtime-proven play mode 3; non-null `ary_tokkunstage_info` also classifies as Tokkun-shaped. | Yes |
| Use only `ary_tokkunstage_info` | Matches early Blue Phase 9 contract but ignores later runtime proof for play mode 3. | |
| Treat tutorial flag as classifier | Would let tutorial-only non-Tokkun uploads mutate Tokkun state. | |

**User's choice:** Coordinator rule selected the required Phase 16 area. The locked decision is to use play mode 3 as primary Yellow Tokkun classifier, with `ary_tokkunstage_info` still Tokkun-shaped evidence and tutorial flag not a standalone classifier.
**Notes:** This carries forward Blue Phase 10 runtime evidence and current shared `PlayMode.Tokkun = 3`. The existing Yellow handler branch already sits before normal handling and should remain the branch point.

---

## Tokkun No-Cross-Write Boundary

| Option | Description | Selected |
|--------|-------------|----------|
| Allow only Yellow Tokkun tutorial/history writes | Satisfies YTOK-01/YTOK-02 while proving no normal, crown, Dani, favorite, recent, shop, medal, profile, battle, or unlock writes. | Yes |
| Keep Phase 15 no-write placeholder | Preserves safety but fails Phase 16 persistence/readback requirements. | |
| Let Tokkun fall through normal/Dani/shop paths | Would corrupt normal Yellow state and violate success criteria. | |

**User's choice:** Coordinator rule selected the required no-cross-write area. The locked decision allows only Yellow Tokkun state and forbids every other state bucket.
**Notes:** Proof must be behavior-based through real mapper/handler/EF/userdata paths. Source-word scans should not be added.

---

## Yellow Tokkun Persistence Shape

| Option | Description | Selected |
|--------|-------------|----------|
| `UserSaveDataYellow` nullable tutorial plus Yellow-owned append-only history | Mirrors the proven Blue role while preserving Yellow-owned state and raw protocol fidelity. | Yes |
| Separate Yellow Tokkun user-state table | Extra table for tutorial state is unnecessary because Yellow save data already has nullable `TokkunTutorialFlg`. | |
| Shared AC15 or Blue Tokkun history table | Violates era-state separation and makes audits ambiguous. | |

**User's choice:** Coordinator rule selected the required persistence area. The locked decision uses existing nullable `UserSaveDataYellow.TokkunTutorialFlg` and a new Yellow-owned append-only raw history table.
**Notes:** Preserve raw `tookun_songno` order and duplicates, store client protocol timestamps only, and do not add server upload timestamps.

---

## Yellow Userdata Tutorial Readback

| Option | Description | Selected |
|--------|-------------|----------|
| Read back only optional `tokkun_tutorial_flg` when persisted | Matches Yellow proto field 37 and Phase 16 success criterion 4. | Yes |
| Continue omitting Yellow Tokkun tutorial | Current Phase 15 placeholder behavior; fails YTOK-03. | |
| Expose Tokkun history/summary in userdata | Invents an unproven response surface and exceeds Phase 16. | |

**User's choice:** Coordinator rule selected the required readback area. The locked decision is tutorial-only readback, optional when non-null.
**Notes:** Yellow profile/wire placement currently blocks readback even though `YellowAc15UserDataAdapter` passes the nullable value into the snapshot. Change Yellow only; Green should remain omitted and Blue unchanged.

---

## Yellow Banacoin-Adjacent Compatibility

| Option | Description | Selected |
|--------|-------------|----------|
| Stateless direct-protobuf log/success routes | Keeps Tokkun compatibility without wallet/payment/transaction authority. | Yes |
| Add payment/wallet/coupon persistence | Out of scope and unsupported by local Yellow evidence. | |
| Remove or delay compatibility routes | Conflicts with Phase 12 route evidence and YBAN-01. | |

**User's choice:** Coordinator rule selected the required Banacoin area. The locked decision is stateless compatibility under Yellow `/v09r00/chassis/*` with request logging and success responses only.
**Notes:** Required `personid` echoes are protocol compatibility, not payment authority. Optional `getbanacoininfo.php` fields should stay unset unless Yellow-specific evidence proves they are required.

---

## the agent's Discretion

- Choose exact file split and naming for Yellow Tokkun handler/entity/tests while preserving existing partial-file and era-owned-state patterns.
- Choose exact raw song-list storage representation, provided tests prove order and duplicates are preserved.
- Choose focused test grouping, provided behavior tests cover mapper, handler, EF reload, userdata optional readback, and stateless Banacoin boundaries.

## Deferred Ideas

- Yellow Tokkun AdminApi/WebUI history inspection is outside Phase 16.
- Yellow normal/Tokkun cabinet or RPCS3 runtime smoke is Phase 17.
- Final Yellow contract documentation is Phase 17.
- Real Banacoin wallet/payment/coupon/settlement/receipt/transaction persistence remains out of scope.
- Yellow battle remains out of scope without new concrete Yellow evidence.
