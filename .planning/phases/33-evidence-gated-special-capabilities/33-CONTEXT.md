# Phase 33: Evidence-Gated Special Capabilities - Context

**Gathered:** 2026-06-21
**Status:** Ready for planning

## Boundary

Resolve the remaining Murasaki special surfaces conservatively: implement only endpoints and response fields that are route-backed by the client or already share established AC15 byte shapes, and leave proto-only families absent.

## Decisions

- Existing AC15 byte families do not need deeper reverse engineering. Use the existing shared sizes: song/release/default/Mainichi flags are 128 bytes, tone flags are 16 bytes, title flags are 128 bytes, costume flags are 32 bytes each, Dan flags are 18 bytes, content info is 32 bytes, crown packed data is 1280 bytes, and `default_option_setting` is 2 bytes.
- Upload-side byte fields are not a Phase 33 research target. The server accepts what the client sends and should not add validation or state semantics from upload bytes alone.
- `bestscore.php`, `songhash.php`, and `shoppingresult.php` stay unimplemented unless a real client request/route is proven. The Murasaki IDB currently shows no `chassis/bestscore.php`, `chassis/songhash.php`, or `chassis/shoppingresult.php` route strings; their request/response names are protobuf descriptor inventory only.
- `BestScoreResponse` must not be filled from local self-best data. If the route is not requested, ignore it.
- `SonghashResponse.song_hash_tbl` and the `ShoppingResultResponse` byte fields have no route-backed implementation requirement until cabinet/log/IDA evidence shows the client asks for those endpoints.
- Challenge arrays remain evidence-gated. Do not copy Red/White Don Challenge or ChallengeCompe behavior into Murasaki without Murasaki-specific route/readback evidence.

## Canonical References

- `.planning/ROADMAP.md` - Phase 33 success criteria and non-goals.
- `.planning/REQUIREMENTS.md` - `MSPEC-02` and `MSPEC-03` evidence gates.
- `.planning/research/FEATURES.md` - existing research notes for Murasaki special surfaces.
- `.planning/research/ARCHITECTURE.md` - current guidance that proto-only global score, song-hash, and shopping surfaces are not route-ready.
- `proto/murasaki/taiko.proto` - inventory only for generated message fields; not proof that an endpoint is requested.
- `.tools/murasaki/EBOOT.ELF.i64` - current IDA evidence source for route strings and byte-field sizing.

## Existing Code Insights

- `Application/Ac15/Ac15EraProfiles.cs` already assigns Murasaki the shared AC15 protocol byte limits.
- `Application/Common/BlueProtocolBytes.cs` and `Application/Common/GreenProtocolBytes.cs` define the shared AC15 byte sizes reused by Murasaki.
- `Adapters.GameProtocol.Murasaki` already maps the standard readback byte fields for BAID, MyDon entry, userdata, default songs, Mainichi songs, and crowns.

## Deferred Ideas

None. Unsupported proto-only surfaces should remain absent until new route/cabinet evidence exists.

---

*Phase: 33-evidence-gated-special-capabilities*
*Context gathered: 2026-06-21*
