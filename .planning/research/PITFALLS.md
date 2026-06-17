# Pitfalls Research: White AC15 0.13 Support

## Summary

- White 0.13 is an older AC15 target, not "Red minus a few fields." The local White proto and `ST7100-1` data show useful leads, and the live `.tools/white/EBOOT.ELF.i64` is nonzero (`129893515` bytes), but route strings are still not proven, so route/root/runtime assumptions need a separate evidence gate.
- White 0.13 must stay version-scoped. Later White updates, Red Tokkun, Yellow shop/medal/WaiWai, and Blue battle behavior stay absent unless White 0.13 proto, local data, logs, IDA/client evidence, or RPCS3/cabinet traces prove them.
- Reuse should be capability-first: shared `Application/Ac15` behavior is appropriate only where White limits, wire placement, and semantics match; White routes, generated wire DTOs, catalog sidecars, EF tables, and AdminApi/WebUI readback remain White-owned.
- The highest White-specific protocol risk is placement drift. White embeds ChallengeCompe-like response rows in `UserDataResponse` and has playresult challenge arrays, but it lacks Red's separate `ChallengeCompeRequest/Response` and Red Tokkun fields.
- Confidence is HIGH for repo/code/proto/test guardrails, MEDIUM for White route/root/runtime behavior until IDB route strings are extracted or equivalent local runtime evidence is captured.

## High-Risk Pitfalls

1. **Treating `ST7100-1` as route/root proof**
   - What goes wrong: implementation hardcodes White routes and catalog root from file layout alone.
   - Prevention: first phase must produce a White evidence artifact covering game route prefix, startup/version ownership, transport/content-type behavior, HDD/version mapping, and active data root. The superseded zero-byte IDB note is no longer current, but file size alone is not route proof; require IDB route extraction, cabinet/RPCS3 logs, request captures, or another local proof source before finalizing runtime routes.

2. **Copying later White update behavior into 0.13**
   - What goes wrong: later White features or data are implemented because public scoping says they existed somewhere in White.
   - Prevention: every feature must be tagged as `White 0.13 proven`, `later White only`, `other-era only`, or `unknown`. Unknown and later-version behavior stays absent. Public/wiki material may scope what to investigate, but cannot define runtime contracts.

3. **Over-copying Red and Yellow runtime behavior**
   - What goes wrong: Red Tokkun, Red `challengecompe.php`, Yellow medals, Yellow item shop, Yellow WaiWai, or Banacoin routes are cloned into White.
   - Prevention: start from `proto/white/taiko.proto`. Current White proto shows normal/profile/userdata/playresult/self-best/crowns/recommend/folder/telop/Taikojuku/reward surfaces, but does not show Red Tokkun fields, Red `ChallengeCompeRequest/Response`, Yellow item-shop purchase/info messages, gacha rows, Blue battle, or Banacoin wallet/payment surfaces. Missing features stay missing.

4. **Misplacing ChallengeCompe/Don Challenge data**
   - What goes wrong: White gets Red's separate ChallengeCompe endpoint, Red's readback semantics, or Red's AdminApi model without proof.
   - Prevention: treat White ChallengeCompe as an older-AC15 candidate whose White 0.13 wire placement differs from Red. White has `ary_challenge_stat`, `ary_user_compe_stat`, and `ary_bng_compe_stat` inside `UserDataResponse`, plus playresult challenge arrays. Prove whether White expects readback through userdata, a separate route, both, or neither before adding stateful behavior.

5. **Letting collectable data drive behavior before runtime gates**
   - What goes wrong: `present.xml`, `spacialbaid.xml`, wiki/OCR data, or copied Red sidecars cause reward, Don Challenge, or special BAID semantics to be implemented too early.
   - Prevention: collectable data belongs late in the milestone after core White identity/catalog/runtime behavior is stable. Sidecars must preserve provenance, use explicit schema fields, and distinguish unsupported rules from executable rules. Do not use generic `threshold` fields that hide different rule meanings.

6. **Reusing Red/Blue protocol limits and packing without proof**
   - What goes wrong: White uses `Ac15EraProfiles.Red` or Blue byte widths/field placement by inertia.
   - Prevention: prove White `Ac15ProtocolLimits` and `Ac15WirePlacement` from White proto/data/generated wire before adding `Ac15EraProfiles.White`. Pay special attention to BAID field numbers, absence of `got_danextra_flg`, `content_info` placement, crown byte length/encoding, release/tone/title/costume flag widths, favorite/recent limits, and Dan ranges.

7. **Putting business logic in Mapperly mappers**
   - What goes wrong: White mapper helpers decide feature availability, ChallengeCompe progress, reward grants, or mode semantics.
   - Prevention: mappers stay mechanical: generated wire DTOs to Application DTOs and Application sections back to generated wire. Controllers deserialize/map/call Mediator/map back. Handler/Application code owns business behavior. Mapper helpers may do explicit mechanical conversions only, such as null-to-empty protocol defaults, byte normalization, constants, nullable presence, and field grouping.

8. **Fake Mapperly verification**
   - What goes wrong: a `[Mapper]` class with hand-written bodies is treated as source-generated, or build success is accepted without checking emitted mapping code.
   - Prevention: use the current Mapperly docs and repo rule: build with `dotnet build /p:EmitCompilerGeneratedFiles=true`, then inspect `obj/.../generated/.../Riok.Mapperly/*.g.cs` for White adapter and changed Application mappers. Confirm Mapperly, not handwritten code, implements mechanical mappings.

9. **Sharing gameplay persistence**
   - What goes wrong: White reuses Red/Yellow save, score, Dan, favorite, recent, challenge, or collectable tables because shapes match.
   - Prevention: add White-owned EF entities, DbSets, mappings, and migrations for every White runtime state surface. Share algorithms via narrow row-shape interfaces and generic helpers over concrete DbSets. Keep `ITaikoDbContext` as the visible persistence boundary; do not add repository-shaped table adapters.

10. **Testing implementation shape instead of behavior**
    - What goes wrong: tests assert controller attributes, generated property existence, route inventory, DI shape, migrations, source strings, or `Result = 1`, while missing state corruption and wire placement bugs.
    - Prevention: tests must protect observed White behavior: parser output, catalog data, protocol packing/field placement, optional zero-vs-absent semantics, handler state transitions, no-cross-era writes, no-cross-mode writes, runtime output copy, and AdminApi/WebUI flows only after the backing state exists.

## Evidence Gates

- **Route/version/transport gate:** prove White game route prefix, shared startup/version route ownership, direct-protobuf assumptions, scoped no-content-type fallback, and disabled-era Host gating before broad controllers or route probes are treated as final.
- **IDB/binary gate:** `.tools/white/EBOOT.ELF.i64` is nonzero (`129893515` bytes) in this checkout, so the superseded zero-byte note is no longer current. Any plan that needs client strings, route root proof, response parsing, or runtime mechanics must first extract usable IDB evidence or use concrete cabinet/RPCS3/request-log evidence.
- **Data-root gate:** `Host/wwwroot/data/white/data/config/ST7100-1` is observed and contains `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `chassisinfo.xml`. That proves local data availability, not necessarily active runtime root selection.
- **Feature inventory gate:** classify each White proto/data surface before implementation: normal profile/userdata, self-best, crowns, recommendations, folders, telops, Taikojuku/Dani, reward/donpoint, ChallengeCompe-like rows, special BAID, tournament, collabo, and absent later-era surfaces.
- **Limits and packing gate:** prove White byte widths, crown encoding, BAID field placement, Dan extra absence, favorite/recent limits, and optional primitive presence before adding a White era profile or mapper tests.
- **ChallengeCompe/collectable gate:** prove whether White 0.13 actually uses Don Challenge during the target date range, where readback is consumed, which buckets are meaningful, and whether rewards lock/unlock songs or titles through userdata. If not proven, keep it absent or data-only.
- **Runtime acceptance gate:** before milestone close, record automated verification, a temp-output Host build if normal output is locked, and RPCS3/cabinet smoke evidence for the White flows actually implemented.

## Testing Guardrails

- Prefer focused behavior tests:
  - White catalog loaders parse `ST7100-1` files and fail clearly for required missing files.
  - White sidecar JSON, if introduced, is schema-validated and copied to Host build/publish output.
  - White mappers preserve nontrivial wire placement, optional presence, byte packing, and field-number-sensitive response shape.
  - White handlers write only White-owned tables and never write Blue, Green, Yellow, Red, or Nijiiro gameplay rows.
  - White normal/Taikojuku/crowns/self-best/reward behavior reads back through the surfaces the cabinet consumes.
  - White AdminApi/WebUI tests request `/api/White/...` only for implemented White-owned surfaces.
- Do not add tests for generated wire property existence, route attribute lists, DI registration shape, enum numeric values, project files, migrations, source text, private methods, `Mediator.Send`, `SaveChanges`, or stateless `Result = 1` echoes.
- Absence tests should be behavior-facing. It is valid to prove Blue battle, Red Tokkun, Yellow shop, or Banacoin authority are not reachable through White runtime/API flows; it is not useful to scan source for missing words.
- Mapper tests are allowed only for nontrivial classification, omission, optional presence, field placement, byte packing, or evidence-backed grouping. Do not write one-to-one copy tests.
- Game-facing tests are regression guards after evidence. Passing server tests does not prove White cabinet compatibility; RPCS3/cabinet smoke remains the compatibility gate.

## Mapperly And Generated Source Guardrails

- Use the official Mapperly 4.3.1 docs for current behavior:
  - Null values: `https://mapperly.riok.app/docs/configuration/mapper/#null-values`
  - Constant/generated values: `https://mapperly.riok.app/docs/configuration/constant-generated-values/`
  - Generated source: `https://mapperly.riok.app/docs/configuration/generated-source/`
- White adapter projects should follow existing AC15 defaults: `AutoUserMappings = false` and strict target mapping unless a specific Apply/section mapper intentionally uses source-strict mapping.
- Regenerate White wire DTOs from immutable `proto/white` inputs using the repo-local nullable optional primitive convention where available. Do not hand-edit generated `Wire/` files and do not modify dumped proto just to simplify mapping.
- Preserve optional zero-vs-absent semantics. Do not collapse nullable optional primitives to `0` before handler logic when absence has meaning.
- Use Mapperly-native configuration first: `MapProperty`, `MapPropertyFromSource`, `MapValue`, `UserMapping`, `MappingTarget`, and explicit ignores. Remaining helper methods must be mechanical and narrow.
- Unsupported capabilities should map to `null` capability records or be omitted from section assembly, not default-filled objects that trick handlers into running later-era behavior.
- After mapper work, run `dotnet build /p:EmitCompilerGeneratedFiles=true` and inspect emitted `.g.cs` files under the relevant `obj/.../generated/.../Riok.Mapperly/` paths. Verify the generated code does not include unintended default object creation, null assignment, or helper selection.

## Phase Placement Advice

- **Foundation/evidence phase:** route/root/transport proof, current nonzero IDB evidence handling, White feature inventory, generated White wire project, Host enabled-era gating, and minimal no-state probes only. Do not add gameplay persistence, AdminApi/WebUI, ChallengeCompe state, or collectable sidecars here.
- **Catalog/profile phase:** bind `ST7100-1` music, tuning, Taikojuku, folders, telops, recommendations, present/reward context, and customization data through shared loaders only where shapes match. Establish `Ac15EraProfiles.White` with proven limits and wire placement.
- **Runtime capability phase:** add White-owned identity/userdata, normal play, self-best, crowns, favorites/recent, Dani, and reward/donpoint state only for proven White 0.13 surfaces. Bind shared AC15 helpers through concrete White DbSets and Mapperly delegates.
- **ChallengeCompe/collectables phase:** run late and only if White 0.13 evidence proves the capability. Resolve userdata-embedded readback vs endpoint readback before schema/state work. Keep community/user/BNG buckets, reward locks, and special BAID behavior absent unless proven.
- **AdminApi/WebUI phase:** expose White only through existing era-routed contracts and generic pages where backing White state exists. Do not add White-only UI for absent or data-only capabilities.
- **Verification/closeout phase:** require focused White tests, relevant shared AC15 regression tests, generated-source inspection for Mapperly changes, full solution/test verification as practical, temp-output Host build, and user-accepted RPCS3/cabinet smoke for implemented flows.
