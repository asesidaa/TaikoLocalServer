# Research Summary: White AC15 0.13 Support

**Synthesized:** 2026-06-16

## Executive Summary

- White 0.13 should be added as a first-class older-AC15 era with White-owned routes, generated wire DTOs, catalog paths, persistence tables, Mapperly mappers, AdminApi/WebUI routing, and verification evidence.
- No new runtime stack is needed. Reuse ASP.NET Core, Mediator, EF Core SQLite, protobuf-net/protogen, Mapperly, existing AC15 services, and the current GSD verification style.
- The first implementation gate is evidence: `proto/white` and `Host/wwwroot/data/white/data/config/ST7100-1` are present, but `.tools/white/EBOOT.ELF.i64` is zero bytes, so route/root/runtime claims need logs, captures, corrected IDB/binary evidence, or another local proof source.
- White local proto/data support a normal AC15 flow: BAID, mydon entry, userdata, initial data, normal playresult, self-best, crowns, recommendations, folders, telops, Taikojuku/Dani leads, reward/present fields, heartbeat/bookkeeping, and related compatibility probes.
- Missing or unproven surfaces stay absent: item shop, Banacoin wallet/payment, Blue battle, Tokkun, WaiWai, gacha runtime, later White updates, and Red's standalone `challengecompe.php` behavior.
- White collectable data, including Don Challenge if it is proven inside the White 0.13 range, belongs late in the milestone after identity, catalog, playresult, reward, and readback surfaces are stable.

## Stack Additions

- Add `Adapters.GameProtocol.White` with generated `Wire/Game.cs` from `proto/white/taiko.proto` and `Wire/VsInterface.cs` from `proto/white/vsinterface.proto`.
- Use existing `protogen 3.2.52` with nullable optional primitive generation:

```powershell
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.White\Wire -Iproto\white +nullablevaluetype=yes proto\white\taiko.proto
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.White\Wire -Iproto\white +nullablevaluetype=yes proto\white\vsinterface.proto
```

- Add White to `GameEra`, Host project/solution/test references, Host config, data-root handling, application-part gating, adapter DI, and AC15 catalog registration.
- Keep Mapperly source-generator driven. Verify important White mappings with `dotnet build /p:EmitCompilerGeneratedFiles=true` and inspect emitted `Riok.Mapperly` generated source.
- Add White server-authored JSON sidecars only where the runtime expects committed server data outside raw operator data; keep `Host/wwwroot/data/white/data` operator-supplied and excluded from publish content.

## Feature Table Stakes

### Evidence And Foundation

- Prove White route prefix, startup/version ownership, direct-protobuf assumptions, active data root, and unresolved gaps before route behavior is treated as final.
- Add first-class White adapter identity, generated wire, Host registration, settings, enabled-era gating, and minimal route probes only where evidence supports them.

### Catalog And Profile

- Bind White `ST7100-1` data through shared AC15 loaders where formats match: `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `fumen/tuning.bin`, and later `present.xml`/`spacialbaid.xml` after parser ownership is defined.
- Add `Ac15EraProfiles.White` with White feature flags, limits, and wire placement only after White byte widths and field placement are verified.

### Runtime

- Implement White-owned identity/profile/userdata, mydon entry, normal playresult, self-best, crowns, favorites/recent songs, recommendations, folders/telops, and reward/profile counters where White evidence supports matching AC15 behavior.
- Treat Taikojuku/Dani as a requirement category because White proto and `musicmedleyinfo.xml` expose clear leads, but still prove runtime classification and readback before claiming end-to-end Dani behavior.

### Collectables And Don Challenge

- Collect White unlock/reward data from local White files and proven supporting data: songs, tones, costumes, titles, Don Point presents, special BAID rows, and any Don Challenge/ChallengeCompe bundles in the White 0.13 range.
- Do not copy Red ChallengeCompe behavior blindly. White has embedded challenge stat/playresult fields but no standalone `ChallengeCompeRequest/Response` in the current proto.
- Bind collectables late, after White identity, catalog, playresult, reward flags, and userdata readback are stable.

### Admin And Verification

- Extend existing era-routed AdminApi/WebUI contracts for implemented White-owned surfaces only.
- Close v1.4 only after focused White tests, relevant shared AC15 regression coverage, generated-source mapper inspection for nontrivial mappings, Host build verification, and user-accepted RPCS3/cabinet smoke evidence.

## Watch Outs

- The zero-byte White IDB means binary-backed route/root claims are currently unavailable.
- `ST7100-1` proves local data availability, not necessarily active runtime selection.
- Later White update behavior must not slip into the 0.13 milestone from wiki context.
- Red/Yellow route and state copying is the biggest implementation risk; White must start from White proto/data evidence.
- White protocol limits and packing need proof before using shared AC15 helpers.
- Business logic must stay in Application handlers/services, not controllers or Mapperly mapper bodies.
- Tests must protect observable behavior and state boundaries, not generated type existence, route attribute inventory, source text, or superficial stateless echoes.

## Recommended Phase Shape

1. White evidence and era foundation.
2. White catalog/profile binding and protocol limits.
3. White runtime capability binding for identity, userdata, initial data, normal play, self-best, crowns, recommendations, folders/telops, rewards, and Dani where proven.
4. White collectable data and optional Don Challenge/ChallengeCompe binding if White 0.13 evidence proves it.
5. White AdminApi/WebUI and runtime closeout.

## Research Inputs

- `.planning/research/STACK.md`
- `.planning/research/FEATURES.md`
- `.planning/research/ARCHITECTURE.md`
- `.planning/research/PITFALLS.md`
