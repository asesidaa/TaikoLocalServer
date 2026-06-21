# Phase 33: Evidence-Gated Special Capabilities - Discussion Log

> Audit trail only. Do not use as input to planning, research, or execution agents.
> Decisions are captured in `33-CONTEXT.md`; this log preserves the alternatives considered.

**Date:** 2026-06-21
**Phase:** 33-evidence-gated-special-capabilities
**Areas discussed:** byte field research scope, proto-only endpoints, upload-side bytes

## Byte Field Research Scope

| Option | Description | Selected |
|--------|-------------|----------|
| Deep IDA use analysis for every byte field | Trace each byte field to consumer semantics before deciding values. | |
| Size-only parity for existing AC15 byte families | For byte fields already present in other AC15 eras, use the established size and do not research semantics further. | yes |

**User's choice:** Size-only parity for byte fields already present in other eras.
**Notes:** Existing bytes present in other eras only need their size. The important part is byte fields that are not implemented yet.

## Upload-Side Bytes

| Option | Description | Selected |
|--------|-------------|----------|
| Research upload byte payloads | Trace request-side byte fields and derive validation/state semantics. | |
| Ignore upload bytes | Accept whatever the client sends; do not add semantics from upload bytes alone. | yes |

**User's choice:** Ignore upload-side byte fields.
**Notes:** Upload side bytes can be ignored because the server accepts whatever the client sends.

## Proto-Only Endpoints

| Option | Description | Selected |
|--------|-------------|----------|
| Implement proto-defined endpoints | Add `bestscore.php`, `songhash.php`, or `shoppingresult.php` from generated messages alone. | |
| Require real client route/request evidence | Leave proto-only endpoint families absent if the client does not request them. | yes |

**User's choice:** Require real route/request evidence.
**Notes:** Targeted Murasaki IDB search found no `chassis/bestscore.php`, `chassis/songhash.php`, or `chassis/shoppingresult.php` route strings. `SonghashRequest`, `BestScoreRequest`, and `ShoppingResultRequest` strings are descriptor inventory with zero code xrefs.

## Deferred Ideas

No additional ideas were deferred.
