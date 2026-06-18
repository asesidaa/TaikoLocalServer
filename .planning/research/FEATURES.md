# Feature Research: White AC15 0.13 Support

## Summary

- White 0.13 should be planned as a first-class older-AC15 era with White-owned routes, generated wire DTOs, persistence, catalog binding, and AdminApi/WebUI readback for only the surfaces proven by White proto/data/log/client evidence.
- The local White proto covers the normal AC15 cabinet loop: startup/verup, bookkeeping/heartbeat, BAID, mydon entry, userdata, initial data, playresult, self-best, crowns, recommendations, folders, telops, Taikojuku, tournament check, headclerk2, getreitai, reward card check, and reward execution.
- White local data currently exposes `config/ST7100-1` with `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, `chassisinfo.xml`, and `fumen/tuning.bin`; no committed White sidecar JSON exists yet.
- White is closer to the Red older-AC15 capability set than Yellow, but it is smaller than Red in important places: the White proto has embedded ChallengeCompe-shaped userdata/playresult fields but no standalone `ChallengeCompeRequest` / `ChallengeCompeResponse`.
- Collectable data, including Don Challenge if any proven White 0.13 content is in range, should be the last functional category because it depends on stable White identity, catalog, playresult, reward, and challenge-readback decisions.

## Table Stakes For v1.4

### 1. White Evidence And Era Foundation

Requirement category:
- Add White as an enableable first-class AC15 era with generated wire DTOs from `proto/white`, Host settings, DI registration, application-part gating, and White-owned adapter/controller files.
- Prove the game route prefix, shared `/v01r00` startup/version ownership, direct-protobuf transport, and active catalog root from local logs, cabinet/RPCS3 traces, or corrected IDA/client evidence before locking route names.
- Treat `.tools/white/EBOOT.ELF.i64` as present and nonzero in the current checkout (`129893515` bytes), with earlier zero-byte notes superseded. Do not derive route/root certainty from file size; require route extraction, logs, captures, or equivalent local evidence.

Likely matching capability:
- Reuse the existing AC15 era-foundation pattern from Yellow/Red, but keep route strings, wire classes, settings, and tests White-owned.

### 2. White Catalog And Sidecar Binding

Requirement category:
- Bind `ST7100-1` catalog inputs through existing AC15 loaders where formats match: `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `fumen/tuning.bin`.
- Add White sidecar JSON only for server-authored data that is not present in raw operator data, such as telops, event folders, movies, recommendations, Taikojuku verup metadata, and eventually challenge data.
- Keep White sidecars explicit and era-named; do not reuse Red or Yellow JSON files as runtime data.

Likely matching capability:
- Red/Yellow already use committed sidecars for event folders, movies, recommendations, Taikojuku verup metadata, and telops.
- Red additionally has `red_don_challenge_data.json`; Yellow additionally has `yellow_item_shop_data.json`. White currently has its own `white_don_challenge_data.json` after the 2026-06-18 correction.

### 3. Identity, Profile, And Userdata

Requirement category:
- Support BAID card lookup/registration, mydon entry, default save creation, returning-user userdata, favorite/recent songs, profile counters, option/tone/title/costume flags, Don Point totals, reward progress, and display settings through White-owned persistence.
- Map White generated protobuf DTOs into existing common/capability DTOs before handler logic.
- Share only true identity data across eras; White gameplay save state must be separate from Blue, Green, Yellow, Red, and Nijiiro state.

Likely matching capability:
- Reuse the shared AC15 identity/profile/userdata services where behavior and field meaning match, with White-specific limits and Mapperly projections.

### 4. Initial Data And Menu Metadata

Requirement category:
- Implement `initialdatacheck.php`, `getfolder.php`, `gettelop.php`, `recommend.php`, `taikojuku.php`, `tournamentcheck.php`, `heartbeat.php`, `bookkeeping.php`, and `headclerk2.php` where White route evidence proves the cabinet calls them.
- `initialdatacheck` must advertise default songs, mainichi dojo hashes, telops, event folders, and Taikojuku metadata in White's wire placement.
- `recommend`, `telop`, and `eventfolder` should use White sidecars when raw data is absent or intentionally empty, following the Red/Yellow pattern.

Likely matching capability:
- Existing AC15 catalog snapshot and metadata handlers are the right starting point, but White field widths, byte packing, compression, and verup placement need generated-source/runtime verification.

### 5. Normal Play Runtime

Requirement category:
- Support White normal `playresult.php` upload and readback through White-owned score history, self-best, crowns, favorites, recent songs, release songs, reward progress, Don Point totals, profile counters, selected folder, and category counters.
- Implement `selfbest.php` and `crownsdata.php` against White-owned tables and White protocol byte lengths.
- Persist only normal-mode state from normal playresults; preserve no-cross-era and no-cross-mode boundaries.

Likely matching capability:
- Reuse AC15 normal play, crown, self-best, favorites/recent, reward, and profile mutation services where White fields map cleanly.

### 6. Taikojuku / Dani

Requirement category:
- Treat White Taikojuku/Dani as a requirement category because the proto exposes `TaikojukuRequest/Response`, `UserDataResponse.disp_taikojuku_dan`, `PlayResultRequest.dan_result`, and local `musicmedleyinfo.xml` has 25 medley rows.
- Runtime support still needs White cabinet/log proof for call order and playresult classification before claiming end-to-end Dani behavior.
- Keep White Dani persistence separate even if the shared AC15 Dan writer can be reused.

Likely matching capability:
- Red/Yellow/Blue AC15 Dani support should provide the implementation pattern, with White-specific medley data and wire placement.

### 7. Reward, Present, And Special Compatibility Routes

Requirement category:
- Bind White reward/present progression from `present.xml`, `reward_ptn`, `reward_progress`, `get_donpoint`, `total_get_donpoint`, `total_use_donpoint`, `release_song_no`, `get_tone_no`, `get_costume_no_*`, and `get_title_no`.
- Implement `rewardcardcheck.php` and `rewardexecution.php` only to the level proven by White flow; these are not Yellow item-shop purchases.
- Treat `getreitai.php` as a proto surface and compatibility candidate, not a required route, until logs/client evidence prove it is called.

Likely matching capability:
- Red reward/present separation is the closest prior model: ordinary Don Point / present progression is distinct from Don Challenge, and Don Challenge is distinct from ChallengeCompe protocol stubs.

### 8. AdminApi And WebUI Readback

Requirement category:
- Add White to era-routed AdminApi and WebUI surfaces only after the corresponding White runtime persistence exists.
- Expected readback categories are profile, score history, self-best/crowns, favorites/recent songs, Dani if implemented, catalog/customization flags, reward progress, and later Don Challenge if collected and bound.
- Hide or omit unsupported White surfaces rather than showing Blue/Yellow/Red controls that have no White evidence.

Likely matching capability:
- Extend the existing era-routed AdminApi/WebUI contracts used for Red/Yellow; add new contracts only where the capability requires them, such as Don Challenge.

### 9. Verification And Closeout

Requirement category:
- Require targeted automated tests for observable White behavior: route serialization, handler state changes, SQLite persistence, no-cross-era boundaries, catalog parsing, reward byte packing, and AdminApi/WebUI readback.
- Close the milestone only after a temp-output Host build if normal build output is locked and user-confirmed cabinet/RPCS3 smoke evidence for implemented White flows.
- Do not treat route inventory tests, controller attribute tests, generated wire type tests, or source-text tests as feature proof.

## White 0.13 Local Evidence

Proto observations:
- `proto/white/vsinterface.proto` contains startup/version messages: `StartupAuth*`, `VerupAuth*`, and `VerupComplete*`.
- `proto/white/taiko.proto` contains cabinet/account and normal runtime messages: `BAID*`, `MydonEntry*`, `UserData*`, `PlayResult*`, `SelfBest*`, and `CrownsData*`.
- `UserDataResponse` includes favorites, recent songs, release song flags, challenge/user/bng competition stat arrays, reward progress, profile counters, recommendation fields, default option/shin settings, Don Point totals, and displayed Taikojuku Dan.
- `PlayResultRequest.StageData` includes normal score facts plus `ary_challenge_id`, `ary_user_compe_id`, `ary_bng_compe_id`, `play_dan`, `stage_mode`, and `selected_folder_id`.
- Metadata/support messages include `Initialdatacheck*`, `Getfolder*`, `Gettelop*`, `Taikojuku*`, `Recommend*`, `Tournamentcheck*`, `BookKeeping*`, `HeartBeat*`, `HeadClerk2*`, `Getreitai*`, `Rewardcardcheck*`, and `Rewardexecution*`.
- The White proto does not define standalone `ChallengeCompeRequest/Response`, `Getitemshopinfo`, `Itempurchase`, `Getbanacoininfo`, `Balancecheck`, `Banacoinpayment`, `Banacoinerrorlog`, Blue battle messages, Tokkun fields, WaiWai fields, or gacha response payloads.

Data observations:
- `Host/wwwroot/data/white/data/config/ST7100-1` contains `chassisinfo.xml`, one duplicate chassisinfo copy, `defmusic.bin`, `musicinfo.xml`, `musicmedleyinfo.xml`, `present.xml`, and `spacialbaid.xml`.
- `musicinfo.xml` has 568 `Data` rows. Genre counts observed locally include 199 Namco Original, 97 Game Music, 75 Anime, 70 J-POP, 41 Vocaloid, 33 Variety, 25 Medley, 23 Classical, and 5 Kids/Folk rows.
- `musicmedleyinfo.xml` has 25 `MusicMedleyInfoData` rows with `challengelv` values 1 through 25. These look like Taikojuku/Dani rows, not Red-style high-number ChallengeCompe rows.
- `present.xml` has 10 `PresentItemData` entries with type/item/donPoint thresholds from 1,000 through 30,000 Don Points.
- `spacialbaid.xml` has a disabled BAID 0 example row and one non-zero special BAID entry; treat this as catalog evidence only until runtime behavior is understood.
- `Host/wwwroot/data/white/data/fumen` contains `tuning.bin` and `tuning_ext.bin`.
- No committed `Host/wwwroot/data/white/*.json` sidecars exist yet.

Red/Yellow comparison observations:
- Red sidecars: Don Challenge data, event folders, movies, recommendations, Taikojuku verup metadata, and telops.
- Yellow sidecars: event folders, item shop data, movies, recommendations, Taikojuku verup metadata, and telops.
- Red controllers include the normal older-AC15 surface plus `challengecompe.php` and stateless Banacoin/balance/coinsetting probes.
- Yellow controllers include the normal older-AC15 surface plus item shop and Banacoin-adjacent compatibility.
- White should not clone either route set wholesale. Start from White proto plus route/client evidence; absent White proto surfaces stay absent.

## Collectable Data And Don Challenge

What to collect:
- White 0.13 collectable catalog rows for unlockable songs, tones, costumes, titles, and Don Point presents from local `musicinfo.xml`, `present.xml`, any shared title/name data, and White-specific local assets.
- Don Challenge bundles only if evidence proves they fall inside the White 0.13 server contract. For each bundle, collect bundle id, active window, 10 personal tasks, optional community task, typed rule data, eligible songs, minimum difficulty, required score/count, reward song ids, and reward title ids.
- White song id mapping must come from the local White `musicinfo.xml`; title/reward ids must come from local committed/shared title data or other provenance that can be rechecked.

Evidence required before runtime binding:
- Prove whether White expects a standalone route, embedded userdata readback only, or no Don Challenge readback at all. The current White proto has embedded `ary_challenge_stat`, `ary_user_compe_stat`, and `ary_bng_compe_stat` plus playresult challenge arrays, but no standalone `ChallengeCompeRequest/Response`.
- Prove call order and field placement with White logs, cabinet/RPCS3 traces, corrected IDA/client evidence, or captured payloads.
- Prove whether challenge progress should be evaluated from actual stage results, preserved challenge marker arrays, or both. Red's accepted shared behavior evaluates from stage results; do not assume White is identical without evidence.
- Prove category semantics for personal/user/bng arrays, track numbering, reward grant timing, and whether there is any opt-in state. Do not import Red UI/API assumptions blindly.

Why this belongs late:
- Don Challenge depends on stable White identity, userdata mapping, playresult classification, normal score/crown persistence, reward unlocks, and catalog song/title id mapping.
- The local White `musicmedleyinfo.xml` supports Dani planning but does not provide Don Challenge bundle data.
- The wiki page is useful timing context only: it scopes White 0.13 to 2015-12-10 and shows visible Don Challenge song notes beginning in later White updates, not in the initial 0.13 section. That makes Don Challenge a late evidence pass, not a foundation blocker.
- The existing `Ac15DonChallengeLoader` and schema define the sidecar contract: `enabled`, `monthly_bundles`, exactly 10 personal tasks per bundle, optional community task, typed rules, and rewards. White should use that shape only after White-specific data is collected.

## Absent Or Evidence-Gated Surfaces

- Standalone ChallengeCompe route: absent from White proto. Do not add `challengecompe.php` unless White logs/client/IDA evidence proves the cabinet calls it.
- Yellow item shop: absent from White proto. Do not add `getitemshopinfo.php`, `itempurchase.php`, item-shop seasons, Don/Katsu medal spend state, or Yellow shop UI.
- Banacoin wallet/payment/balance: absent from White proto. Do not add `getbanacoininfo.php`, `balancecheck.php`, `banacoinpayment.php`, `banacoinerrorlog.php`, coupon, receipt, BNID, settlement, or transaction state.
- Blue battle: absent from White proto/data. Do not add battle userdata, battle initialdata advertisements, battle playresult handling, token/NPC state, or battle AdminApi/WebUI.
- Tokkun: absent from White proto. Do not add `tokkun_tutorial_flg`, `ary_tokkunstage_info`, Tokkun playresult classification, Banacoin-for-Tokkun compatibility, or Tokkun history tables.
- WaiWai: absent from White proto. Do not add WaiWai tutorial/readback or special play mode behavior.
- Gacha runtime: absent as explicit White response payloads. Do not add gacha route/runtime state from Red/Yellow generated surfaces unless White evidence appears.
- Tournament runtime: White has `Tournamentcheck*` and `tournament_mode`; treat this as a route/probe or playresult fact until evidence proves a fuller tournament system.
- Getreitai route: present in proto but evidence-gated. Prior AC15 work treated reitai as proto-only unless route strings/logs proved a cabinet call.
- Later White updates: keep White 1.10+ and later behavior out unless local White 0.13 evidence proves it belongs to this target or the milestone scope changes.

## Source Notes

- `.planning/PROJECT.md`: active v1.4 scope, White 0.13 evidence hierarchy, observed `ST7100-1` root, superseded zero-byte White IDB note corrected by current nonzero evidence, active requirements, out-of-scope surfaces, and late collectable-data decision.
- `proto/white/taiko.proto`: normal White game protocol message inventory and field placement.
- `proto/white/vsinterface.proto`: startup/verup protocol message inventory.
- `Host/wwwroot/data/white/data/config/ST7100-1`: local White config inventory and parsed observations from `musicinfo.xml`, `musicmedleyinfo.xml`, `present.xml`, `spacialbaid.xml`, and `defmusic.bin`.
- `Host/wwwroot/data/white/data/fumen/tuning.bin` and `Host/wwwroot/data/white/data/fumen/tuning_ext.bin`: local White fumen/tuning inputs.
- `Host/wwwroot/data/red/*.json` and `Host/wwwroot/data/yellow/*.json`: sidecar comparison for event folder, movie, recommendation, Taikojuku, telop, Red challenge, and Yellow item-shop data.
- `Application/Ac15/DonChallenge/*`: shared Don Challenge catalog, evaluator, reward, and track-definition shape.
- `Infrastructure/GameDataCatalog/Ac15/Ac15DonChallengeLoader.cs` and `Infrastructure/GameDataCatalog/Ac15/Schemas/ac15-don-challenge-catalog.schema.json`: Don Challenge sidecar loader contract.
- `Adapters.GameProtocol.Red/Controllers/*` and `Adapters.GameProtocol.Yellow/Controllers/*`: route-surface comparison only; not implementation authority for White.
- Wiki scoping note only: https://wikiwiki.jp/taiko-fumen/%E4%BD%9C%E5%93%81/%E6%96%B0AC/%E3%82%A2%E3%83%83%E3%83%97%E3%83%87%E3%83%BC%E3%83%88%E5%B1%A5%E6%AD%B4/%E3%83%9B%E3%83%AF%E3%82%A4%E3%83%88
