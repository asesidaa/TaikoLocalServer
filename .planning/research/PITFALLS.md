# Pitfalls Research: MOMOIRO AC15 0.11 Support

**Domain:** Brownfield ASP.NET Core Taiko AC15 era adapter, protobuf wire mapping, SQLite persistence, root-level MOMOIRO game-data catalogs, and Blazor AdminApi/WebUI support
**Researched:** 2026-06-25
**Confidence:** MEDIUM overall. HIGH for live repo/proto/data facts verified in this checkout; LOW for wiki pages per GSD confidence classification; MEDIUM for Mapperly guidance only after emitted generated source is inspected in this repo. Route inventory, byte widths, unlock behavior, and cabinet acceptance remain intentionally gated on MOMOIRO binary/log/RPCS3 evidence.

## Critical Pitfalls

### Pitfall 1: Copying KIMIDORI or Murasaki Behavior Instead of Proving MOMOIRO

**What goes wrong:**
MOMOIRO compiles as a renamed KIMIDORI or Murasaki adapter, but runtime behavior diverges in exactly the places this milestone calls out: `/v04r00` game routes, `/v01r00` startup/version routes, root-level data, crowns inside `userdata.php`, changed limits, and binary-backed song unlocking. The result is a server that passes copied handler tests while the cabinet receives wrong field placement or silently ignores state.

**Why it happens:**
MOMOIRO is adjacent to KIMIDORI and older than Murasaki in the AC15 family, so shared capability composition looks attractive. The current code also makes KIMIDORI and Murasaki easy copy sources through `Ac15EraProfiles`, shared AC15 services, era-specific save tables, and Mapperly mappers. Similar names are not evidence of identical wire contracts.

**How to avoid:**
Start with a MOMOIRO evidence inventory before adding runtime behavior. For each route/feature, require both a `proto/momoiro` request/response shape and a matching binary `.php` route or capture. Reuse AC15 shared services only after MOMOIRO has its own `GameEra.Momoiro` profile, limits, persistence, catalog adapter, mapper declarations, and generated-source verification. Keep copied tests focused on observable MOMOIRO behavior, not on parity with another era's implementation.

**Warning signs:**
- New code references `GameEra.Kimidori`, `GameEra.Murasaki`, `UserSaveDataKimidori`, or `UserSaveDataMurasaki` from a MOMOIRO path.
- A plan says "same as KIMIDORI" without citing `proto/momoiro`, `.tools/momoiro/EBOOT.ELF.i64`, or a MOMOIRO log.
- Tests assert copied route/controller shape instead of exercising MOMOIRO request/response behavior.
- MOMOIRO gets `Ac15EraProfiles.Kimidori` limits or feature flags before changed limits are researched.

**Phase to address:**
Phase 1, "MOMOIRO Evidence and Era Foundation"; Phase 2, "MOMOIRO Protocol Limits and Catalog Binding".

---

### Pitfall 2: Treating WikiWiki or Public Update History as Protocol Authority

**What goes wrong:**
Public pages are used to decide route families, feature availability, Don Point/shop authority, crown behavior, or update-version semantics. The wiki can be useful chronology, but it does not prove what the 0.11 client calls, which fields it reads, how it packs hashes/crowns, or whether a server-side state model is required.

**Why it happens:**
The wiki is easy to read and does mention MOMOIRO 0.11, public-facing Donder Hiroba/customization/shop changes, and later updates. Those details are tempting when local binary research is slower. The project history already shows adjacent-era notes can conflict with binary-supported behavior.

**How to avoid:**
Use wiki pages only as LOW-confidence background and route all implementation decisions through local evidence. The implementation gate is: `proto/momoiro` contains the feature, the MOMOIRO binary proves the corresponding `.php` route, and request/cabinet evidence shows the server contract. If the wiki suggests a feature family but proto or binary evidence is missing, document it as unsupported for MOMOIRO 0.11.

**Warning signs:**
- A feature is added because a public page mentions a gameplay or Donder Hiroba change.
- A later MOMOIRO update is used to justify behavior in 0.11.
- A route is implemented without a local binary route table entry.
- A roadmap phase treats wiki chronology as stronger evidence than local proto or logs.

**Phase to address:**
Phase 1, "MOMOIRO Evidence and Era Foundation"; every later phase should cite the evidence table instead of the wiki.

---

### Pitfall 3: Missing Crowns-In-Userdata Semantics

**What goes wrong:**
MOMOIRO crown readback is implemented as a copied `crownsdata.php` endpoint or dedicated crown response, so the cabinet never sees crowns where its proto expects them. `proto/momoiro/taiko.proto` places `hash_crown_flg` at `UserDataResponse` field 8. There is no MOMOIRO `CrownsDataRequest/Response` message in the local proto.

**Why it happens:**
Newer supported eras commonly use dedicated crown placement. The current shared profiles for Blue, Green, Yellow, Red, White, Murasaki, and KIMIDORI use `Ac15CrownWirePlacement.DedicatedEndpoint`, and `Ac15CrownService` can build a 10-bit packed crown body with existing limits. That path is familiar but wrong unless MOMOIRO binary evidence proves a separate `crownsdata.php` route.

**How to avoid:**
Model MOMOIRO crown placement as `UserData` unless binary evidence proves otherwise. Wire `hash_crown_flg` through MOMOIRO `userdata.php`, build the crown bytes from MOMOIRO-owned score/crown state, and keep separate crown endpoint code absent. Add a focused regression that creates MOMOIRO best rows and verifies `userdata.php` contains `hash_crown_flg` while no cross-era crown endpoint/state is involved.

**Warning signs:**
- A MOMOIRO `crownsdata.php` controller appears before a binary route proof.
- `Ac15EraProfiles.Momoiro` is copied with `CrownPlacement: DedicatedEndpoint`.
- Crown assertions only call a crown endpoint and never inspect MOMOIRO `userdata.php`.
- `hash_crown_flg` is left null/empty in `UserDataResponse` while crown rows exist.

**Phase to address:**
Phase 2, "MOMOIRO Protocol Limits and Catalog Binding"; Phase 3, "MOMOIRO Runtime State and Readback".

---

### Pitfall 4: Guessing Unlock, Hash, and Crown Packing From Later-Era Constants

**What goes wrong:**
Song unlock flags, `song_hash_tbl`, release hashes, default/mainichi hashes, and crowns use Blue/KIMIDORI byte widths or indexing without proof. This can truncate high song ids, expose locked songs, hide unlocked songs, or produce crown bytes the client reads at the wrong offsets. The risk is higher because this milestone explicitly requires binary research for song unlocking, crowns data, and changed limits.

**Why it happens:**
The shared code has convenient constants: `SongFlagBytes = 128`, `TitleFlagBytes = 128`, `CostumeFlagBytes = 32`, `CrownInflatedBytes = 1280`, and `CrownSongCount = 1024`. Those values have been useful for other AC15 eras, but MOMOIRO 0.11 may have earlier limits or different consumer paths. The proto names describe category, not width or packing.

**How to avoid:**
Make a protocol-limits artifact before runtime implementation. For each byte surface in MOMOIRO proto, record source evidence for width, indexing basis, default fill, and readback route: `hash_release_song_flg`, `hash_crown_flg`, `hash_default_song_flg`, `hash_mainichidojo_all`, `hash_mainichidojo_rare`, `song_hash_tbl`, `tone_flg`, costume flags, `title_flg`, `content_info`, `default_option_setting`, and `reserved`. Keep unknown surfaces fixed-size/log-only or absent until binary/log evidence is available.

**Warning signs:**
- MOMOIRO limits are created by calling `CreateCommonLimits()` with no MOMOIRO-specific evidence note.
- Tests only check array length or non-null bytes, not cabinet-consumed behavior.
- Song unlocks are stored using song ids without confirming file order, hash table order, and bit index basis.
- `ShoppingResultResponse.hash_release_song_flg`, `UserDataResponse.hash_release_song_flg`, and `SonghashResponse.song_hash_tbl` are assembled by unrelated helpers without a shared evidence-backed contract.

**Phase to address:**
Phase 2, "MOMOIRO Protocol Limits and Catalog Binding"; Phase 3, "MOMOIRO Runtime State and Readback".

---

### Pitfall 5: Assuming a Later `config/STxxxx-*` Data Root

**What goes wrong:**
Catalog loading looks for `Host/wwwroot/data/momoiro/data/config/ST4100-1`, `S04100-1`, or another later-era style config root and fails startup validation or silently loads empty data. The live MOMOIRO data inventory in this checkout is root-level: `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin` exist directly under `Host/wwwroot/data/momoiro/data`; no `config/` directory exists.

**Why it happens:**
White, Red, Yellow, Blue, and Green use `config/<root>` shapes, and KIMIDORI recently required root-level handling. It is easy to regress by using the more common newer-era path helper or by generalizing from `STxxxx-*` names.

**How to avoid:**
Create a MOMOIRO catalog loader path that resolves the root-level layout explicitly through `PathHelper` and era data helpers. Startup validation should report root-level missing files by their actual paths. Do not scan for plausible config roots. Keep source game data out of commits and commit only server-authored sidecar JSON if the implementation creates one.

**Warning signs:**
- Loader logs mention `config/` under MOMOIRO.
- Startup has zero songs while raw root `musicinfo.xml` is present.
- Tests construct a fake `config/ST4100-1` tree for MOMOIRO.
- Code hardcodes `wwwroot/data/momoiro` instead of using the existing path helper pattern.

**Phase to address:**
Phase 2, "MOMOIRO Protocol Limits and Catalog Binding".

---

### Pitfall 6: Accidental Cross-Era Gameplay Writes

**What goes wrong:**
MOMOIRO play results, favorites, recent songs, Don Point, unlocks, crowns, or challenge arrays write to KIMIDORI/Murasaki/White tables or shared helper paths that were only meant as composition sources. The AdminApi/WebUI then appears to work for one era while corrupting another era's local state.

**Why it happens:**
The repo intentionally shares identity but keeps gameplay state per era. Adding a new era means adding many similarly named DbSets, save-data extensions, mappers, and UI routes. Copying a handler but missing one generic type or `DbSet` reference is enough to leak writes.

**How to avoid:**
Add MOMOIRO-owned domain entities, `ITaikoDbContext.Momoiro.cs`, EF mapping, migrations, save-data extension helpers, and AdminApi/WebUI era routes before runtime writes. Any shared AC15 service must take a MOMOIRO profile/snapshot rather than reaching into another era. Add no-cross-era persistence tests around playresult, userdata, shoppingresult, and AdminApi edits that assert other AC15 tables remain unchanged.

**Warning signs:**
- MOMOIRO code imports `UserSaveDataKimidoriExtensions` or `MurasakiAc15UserDataAdapter`.
- A handler receives `GameEra.Momoiro` but queries another era's `DbSet`.
- AdminApi tests only verify response DTOs and do not inspect persisted era tables.
- New migration adds columns to an existing era table instead of creating MOMOIRO-owned tables.

**Phase to address:**
Phase 1, "MOMOIRO Evidence and Era Foundation"; Phase 3, "MOMOIRO Runtime State and Readback"; Phase 4, "MOMOIRO AdminApi/WebUI".

---

### Pitfall 7: Route/Proto Mismatch

**What goes wrong:**
Controllers are added for routes that exist in another era but not in MOMOIRO 0.11, or a MOMOIRO proto message is exposed as a route the binary never calls. Either side of the mismatch causes misleading green tests and broken cabinet flow. Local `proto/momoiro/taiko.proto` contains messages for `BAID`, `BestScore`, `BookKeeping`, `CommunicationLog`, `Defaultsong`, `GetTelop`, `HeartBeat`, `Mainichisong`, `MydonEntry`, `PlayResult`, `Recommend`, `SelfBest`, `ShoppingResult`, `Songhash`, `TelopCheck`, and `UserData`. Local `vsinterface.proto` contains startup/version messages only. The binary route table still needs explicit proof.

**Why it happens:**
All routes are `.php`, and many AC15 route names repeat across eras. That makes route scaffolding feel mechanical. But KIMIDORI and Murasaki already showed that feature presence must be gated by both proto and binary route evidence.

**How to avoid:**
Build a route matrix before implementation with columns for route suffix, prefix (`/v01r00` or `/v04r00`), proto request/response, binary route proof, transport, handler owner, and state owner. A route is supported only when both proto and binary columns are proven. If one side is missing, record a stub/absence decision instead of adding behavior.

**Warning signs:**
- `initialdatacheck.php`, `getfolder.php`, `taikojuku.php`, `crownsdata.php`, `banacoin*.php`, Tokkun, battle, or Don Challenge routes appear without MOMOIRO proto plus binary evidence.
- Startup/version routes are registered under `/v04r00` instead of shared `/v01r00`.
- A binary route lead is treated as enough even though no MOMOIRO wire DTO exists.
- A route inventory test checks attribute strings instead of route behavior and disabled-era gating.

**Phase to address:**
Phase 1, "MOMOIRO Evidence and Era Foundation".

---

### Pitfall 8: Mapperly Blind Spots and Stale Generated Source

**What goes wrong:**
Mapper declarations compile, but generated code omits fields, assigns null/default values unexpectedly, maps constants incorrectly, or preserves stale generated state from an earlier build. MOMOIRO is especially sensitive because wire placement differs from KIMIDORI/Murasaki for crowns and may differ for limits/defaults.

**Why it happens:**
Mapperly hides final generated mapping behind partial methods. Official docs show null behavior is configurable, constant/generated values require exact target types, and generated source can be emitted with `dotnet build /p:EmitCompilerGeneratedFiles=true`. Prior repo work also found Mapperly warning behavior can look inconsistent until a full rebuild emits fresh `.g.cs`.

**How to avoid:**
Keep Mapperly source-generator driven. Use mapper declarations for mechanical DTO projection only, keep business assembly in controllers/handlers, and inspect emitted `Adapters.GameProtocol.Momoiro/obj/.../generated/.../Riok.Mapperly/.../*.g.cs` after a clean or forced build. Verify `hash_crown_flg`, release flags, Don Point totals, option/default fields, and constants are actually emitted where expected. Compare sibling adapter Mapperly defaults before suppressing warnings.

**Warning signs:**
- A mapper warning is suppressed before checking generated output and sibling adapter defaults.
- Manual mapper bodies replace Mapperly projections instead of using configured helpers.
- Verification reads only `.cs` partial declarations and not emitted `.g.cs`.
- A build passes incrementally but a rebuild with `EmitCompilerGeneratedFiles=true` changes generated output.

**Phase to address:**
Phase 1, "MOMOIRO Evidence and Era Foundation"; Phase 3, "MOMOIRO Runtime State and Readback"; Phase 5, "MOMOIRO Verification and Acceptance".

---

### Pitfall 9: Overclaiming Runtime Verification

**What goes wrong:**
The milestone is marked runtime-ready after `dotnet build`, route probes, unit tests, or AdminApi smoke tests. Those checks are necessary, but they do not prove the MOMOIRO 0.11 client accepted startup, login, song select/readback, crown/userdata semantics, playresult upload, unlock behavior, and post-upload readback.

**Why it happens:**
Prior eras have strong automated coverage and similar flows, and a temp-output Host build catches many real integration issues. It is tempting to call that "runtime verified" when the remaining cabinet/RPCS3 pass is manual or external.

**How to avoid:**
Separate verification categories in every phase artifact: automated build/test evidence, server log/route evidence, generated-source evidence, and cabinet/RPCS3 acceptance. Close implementation phases with automated proof, but keep milestone runtime acceptance open until the user or captured client run confirms the supported MOMOIRO flows. Do not broaden acceptance wording beyond what was actually observed.

**Warning signs:**
- `STATE.md` or verification docs say "runtime checked" with only build/test commands.
- The final report says "cabinet compatible" without request logs or user confirmation.
- A route returns `Result = 1` and is treated as complete despite no client readback proof.
- Known manual gates are moved to "done" because no tests fail.

**Phase to address:**
Phase 5, "MOMOIRO Verification and Acceptance"; keep acceptance status explicit from Phase 1 onward.

## Technical Debt Patterns

Shortcuts that seem reasonable but create long-term problems.

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Reuse `CreateCommonLimits()` for MOMOIRO | Fast compile and easy shared-service wiring | Wrong byte widths or song/crown bounds become embedded in persistence and tests | Only as a temporary scaffold before Phase 2, never in accepted runtime behavior |
| Add route stubs for every familiar AC15 `.php` route | Cabinet probes get fewer 404s during early smoke | Unsupported behavior looks implemented and blocks future evidence-based design | Only log-and-404/disabled scaffolds during evidence gathering, removed or documented before runtime phases |
| Store MOMOIRO facts in KIMIDORI/Murasaki tables | Avoids migration work | Cross-era corruption and impossible AdminApi debugging | Never for gameplay state |
| Treat wiki update notes as feature backlog | Quick requirements list | Builds public-feature expectations instead of 0.11 protocol support | Only as LOW-confidence research notes |
| Skip generated-source inspection after Mapperly changes | Faster verification loop | Silent field placement/default bugs reach cabinet testing | Never for MOMOIRO mappers that touch userdata, hashes, crowns, or playresult |

## Integration Gotchas

Common mistakes when connecting MOMOIRO to existing repo systems.

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| ASP.NET route registration | Register startup/version under `/v04r00` or game routes under `/v01r00` | Keep shared startup/version on `/v01r00/chassis/*.php`; register MOMOIRO game routes under `/v04r00/chassis/*.php` only after route proof |
| Protobuf wire generation | Edit generated `Wire/` DTOs or infer fields from another era | Regenerate from `proto/momoiro`, map through Common DTOs, and inspect generated Mapperly source |
| EF Core persistence | Reuse adjacent-era save rows | Add MOMOIRO-owned entities/DbSets/migration and assert no cross-era writes |
| Catalog loading | Search `config/STxxxx-*` roots | Load root-level MOMOIRO files from `Host/wwwroot/data/momoiro/data` through path helpers |
| AdminApi/WebUI | Show all older-AC15 panels because they exist elsewhere | Expose only implemented MOMOIRO-owned state; hide unsupported Taikojuku/Tokkun/battle/Banacoin/Don Challenge surfaces |
| Binary evidence | Treat raw IDB string scans as route proof | Use IDA route table/cross-reference work, request logs, or captures; record exact suffix/prefix evidence |

## Performance Traps

Patterns that work in small tests but fail or mislead with real MOMOIRO data.

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| Reparse root `musicinfo.xml` and packed customization data per request | Slow userdata/songhash responses and noisy startup logs | Cache through the era catalog loader, same as other AC15 eras | Cabinet repeatedly calls metadata/userdata routes |
| Build oversized bitsets from hardcoded newer-era maxes | Extra bytes, wrong truncation, client ignores or misreads flags | Use MOMOIRO binary-confirmed limits and normalize through `Ac15ProtocolBytes` | Song ids or crown indexes approach the real MOMOIRO boundary |
| Use exhaustive route tests over controller attributes | Slow tests that prove little | Test observable handler/API behavior and disabled-era routing where it affects runtime | Every new route family adds duplicated attribute tests |

## Security Mistakes

Domain-specific security issues beyond general web security.

| Mistake | Risk | Prevention |
|---------|------|------------|
| Logging full access codes, BAID tokens, or raw request bodies during binary/runtime research | Local privacy leakage and noisy artifacts | Log bounded route, length, and hex summaries; scrub identity-like values in shared artifacts |
| Sharing gameplay state across eras | One cabinet era can expose or mutate another era's local save state | Keep only identity shared; keep MOMOIRO gameplay tables and AdminApi routes era-owned |
| Inventing payment/Banacoin/shop authority from public notes | Fake financial or entitlement semantics enter local persistence | MOMOIRO 0.11 has no such scope unless proto plus binary evidence proves a route and state contract |

## UX Pitfalls

Common AdminApi/WebUI mistakes for this milestone.

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| Showing unsupported older-AC15 panels | User edits state the cabinet never reads | Gate WebUI features by MOMOIRO capability profile and hide unsupported panels |
| Displaying MOMOIRO as KIMIDORI/Murasaki in menus or labels | Era confusion and wrong manual-test targeting | Add first-class `WebUiEra.Momoiro` labels/routes |
| Claiming crown support through a separate crown page only | User sees data that does not match cabinet readback | Surface crown state through MOMOIRO score/userdata readback and document userdata placement |

## "Looks Done But Isn't" Checklist

Things that appear complete but are missing critical pieces.

- [ ] **Era foundation:** `GameEra.Momoiro` exists, but disabled-era routing, Host settings, application-part gating, and `/v04r00` route proof are not all verified.
- [ ] **Proto generation:** MOMOIRO wire DTOs compile, but Mapperly emitted `.g.cs` has not been inspected.
- [ ] **Catalog:** Song count loads, but the loader used a fake `config/` root or skipped `defmusic.bin`/`fumen/tuning.bin`.
- [ ] **Userdata:** Favorites/recent songs return, but `hash_crown_flg` and `hash_release_song_flg` are missing or copied from a wrong width.
- [ ] **Playresult:** Upload returns success, but scores/crowns/unlocks are not read back by MOMOIRO `userdata.php`/`selfbest.php`.
- [ ] **Shopping/unlocks:** Don Point totals update, but `ShoppingResultResponse.hash_release_song_flg` and `UserDataResponse.hash_release_song_flg` disagree.
- [ ] **Routes:** Familiar `.php` endpoints exist, but the route matrix lacks binary proof or proto pairing.
- [ ] **Verification:** Build/tests pass, but cabinet/RPCS3 acceptance is still unrecorded.

## Recovery Strategies

When pitfalls occur despite prevention, how to recover.

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Crowns implemented as a dedicated endpoint | HIGH | Stop adding runtime behavior, move crown projection into MOMOIRO userdata, remove unsupported endpoint/state, migrate or recompute from score rows, add userdata readback regression |
| Wrong byte limits shipped into persistence | HIGH | Freeze writes, produce binary-backed limits table, add migration/backfill if persisted byte lengths are wrong, rebuild tests around confirmed widths |
| Cross-era writes found | HIGH | Identify affected tables, write one-time cleanup/migration only if needed, add no-cross-era regression tests, review all MOMOIRO handlers for copied generic types |
| Wrong data root | MEDIUM | Replace path helper/loader, add root-level loader tests using real inventory shape, delete fake config fixtures |
| Mapperly generated output wrong | MEDIUM | Rebuild with emitted generated files, fix mapper attributes/defaults/helpers, avoid handwritten mapper bodies except explicit conversions |
| Runtime acceptance overclaimed | LOW to MEDIUM | Update verification docs with exact evidence categories, reopen manual acceptance gate, avoid new code until the real blocking runtime issue is known |

## Pitfall-to-Phase Mapping

How roadmap phases should address these pitfalls.

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Copying KIMIDORI/Murasaki behavior | Phase 1 | MOMOIRO evidence matrix and first-class era profile exist before runtime handlers |
| Wiki treated as authority | Phase 1 | Every supported feature cites proto plus binary/log evidence; wiki is marked secondary |
| Crowns-in-userdata missed | Phase 2 and Phase 3 | `UserDataResponse.hash_crown_flg` is populated from MOMOIRO-owned state; no unproven `crownsdata.php` route |
| Unlock/hash/crown packing guessed | Phase 2 | Protocol-limits artifact records width/index/default source for each byte surface |
| Root-level data missed | Phase 2 | Catalog tests use root-level `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, and `fumen/tuning.bin` |
| Cross-era writes | Phase 3 and Phase 4 | Persistence tests prove MOMOIRO writes do not touch other AC15 gameplay tables |
| Route/proto mismatch | Phase 1 | Route matrix pairs `/v04r00` or `/v01r00` suffixes with proto and binary proof |
| Mapperly blind spots | Phase 3 and Phase 5 | Build with `EmitCompilerGeneratedFiles=true` and inspect MOMOIRO `.g.cs` output |
| Overclaimed runtime verification | Phase 5 | Verification separates build/test evidence from cabinet/RPCS3 acceptance |

## Sources

- Local project state: `.planning/PROJECT.md`, `.planning/STATE.md`.
- Local MOMOIRO proto: `proto/momoiro/taiko.proto`, `proto/momoiro/vsinterface.proto`.
- Local MOMOIRO data inventory: `Host/wwwroot/data/momoiro/data` root-level files.
- Local implementation patterns: `Application/Ac15/Ac15EraProfiles.cs`, `Application/Ac15/Ac15CrownService.cs`, `Application/Ac15/Ac15UserDataService.cs`, `Application/Ac15/Ac15WirePlacement.cs`, `Application/Ac15/Ac15ProtocolBytes.cs`, `Application/Ac15/Ac15UnlockFlagAccess.cs`, `Application/Common/BlueProtocolBytes.cs`.
- Local binary evidence location: `.tools/momoiro/EBOOT.ELF.i64` (route/limit research still required; raw string scan did not prove route inventory).
- Secondary wiki context, LOW confidence: https://wikiwiki.jp/taiko-fumen/%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0/AC%E3%81%AE%E6%AD%B4%E5%8F%B2/AC15#momoiro
- Secondary wiki context, LOW confidence: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%82%A2%E3%83%83%E3%83%97%E3%83%87%E3%83%BC%E3%83%88%E5%B1%A5%E6%AD%B4/%E3%83%A2%E3%83%A2%E3%82%A4%E3%83%AD
- Mapperly docs, official but final authority is emitted repo generated source: https://mapperly.riok.app/docs/configuration/mapper/#null-values
- Mapperly docs, official but final authority is emitted repo generated source: https://mapperly.riok.app/docs/configuration/constant-generated-values/
- Mapperly docs, official but final authority is emitted repo generated source: https://mapperly.riok.app/docs/configuration/generated-source/

---
*Pitfalls research for: MOMOIRO AC15 0.11 support in TaikoLocalServer*
*Researched: 2026-06-25*
