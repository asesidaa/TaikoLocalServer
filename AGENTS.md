# Agent Notes

TaikoLocalServer is an ASP.NET Core 10 host for Taiko cabinet protocol endpoints, SQLite persistence, era-specific game-data catalogs, and the Blazor WebAssembly admin UI. The repo now supports Nijiiro, Green AC15, Blue AC15, Yellow AC15, Red AC15, and White AC15 in the same process. Blue, Yellow, Red, and White are first-class eras, not variants of Green or each other.

## Current Blue State

- Blue game routes live under `/v10r03/chassis/*`.
- Shared AC15 startup and version routes remain under `/v01r00/chassis/*`.
- Blue request bodies are direct protobuf for game endpoints; preserve that transport unless current client evidence proves otherwise.
- Blue normal play supports profile/login, userdata, initial data, self-best, crowns, recent/favorite songs, Dani Dojo, item shop purchase/unlock state, event folders, telops, movies, recommendations, tournaments, gacha data, and WebUI/AdminApi era routing.
- Blue battle support uses Blue-owned persistence and handlers for `battleuserdata.php`, `initialdatacheck.php`, and battle-classified `playresult.php` payloads.
- Blue Tokkun support uses `PlayMode.Tokkun = 3`, accepts Tokkun-classified `playresult.php` uploads before battle or normal handling, persists nullable `tokkun_tutorial_flg` on `UserSaveDataBlue`, appends raw protocol-backed history rows in `BlueTokkunStageResults`, upserts recent-song rows from practiced `tookun_songno` values, and reads back only the tutorial flag plus ordinary recent-song arrays through `userdata.php`.
- Blue Banacoin-adjacent support is stateless compatibility for Tokkun availability. `getbanacoininfo.php` returns minimal success, and Banacoin payment/error routes log and return success without wallet, balance, payment, coupon, or transaction persistence.
- Blue battle runtime state is store-and-echo where semantics are not proven. Do not invent token rewards, boss completion, stage graph behavior, stage 33 behavior, or normal unlock mirrors without concrete client/log/proto/IDA evidence.
- Battle playresults must not write normal Blue score, crown, Dani, profile, favorite, or normal unlock state. Active shop Don medals and recent songs are allowed Blue side effects.
- Tokkun playresults must not write normal Blue score, crown, Dani, profile, favorite, normal unlock, battle, or shop state. Recent-song rows derived from Tokkun practiced song numbers are an allowed side effect and must stay separate from score, best, and favorite state.
- Preserve known battle ID contracts: runtime token ids and NPC ids are zero-based; response-side persisted token rows use the `TokenId - 1` mapping with `0` guarded.
- New users receive the IDA-backed starter battle state only when no persisted Blue battle state exists; after playresult, `battleuserdata.php` reads back persisted BlueBattle rows.

## Current White State

- White final-version game routes live under `/v07r03/chassis/*`.
- White legacy compatibility game routes live under `/v07r00/chassis/*`.
- Shared AC15 startup and version routes remain under `/v01r00/chassis/*`.
- Final `/v07r03` and legacy `/v07r00` White routes must stay schema-separated: final routes use `Adapters.GameProtocol.White.Wire`, while legacy routes use `Adapters.GameProtocol.White.LegacyWire`.
- White request bodies are direct protobuf for game endpoints; preserve that transport unless current client evidence proves otherwise.
- White catalog data uses the `ST7100-1` root from `Host/wwwroot/data/white/data/config/ST7100-1`.
- White required catalog inputs are `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `fumen/tuning.bin`.
- White normal play supports profile/login, userdata, initial data, self-best, crowns, recent/favorite songs, Dani Dojo, reward/Don Point state, present and special-BAID provenance, event folders, telops, movies, recommendations, AdminApi/WebUI era routing, and dedicated Don Challenge readback.
- White final 11.01 support adds proven Tokkun, Banacoin-adjacent, difficulty panel, and final heartbeat fields under `/v07r03`; do not force these fields onto legacy `/v07r00` wire.
- White Tokkun support accepts Tokkun-classified `playresult.php` uploads before normal/Dani/Don Challenge handling, persists nullable `TokkunTutorialFlg` plus append-only raw `WhiteTokkunStageResults`, upserts recent-song rows from practiced `tookun_songno` values, and keeps history out of userdata/AdminApi/WebUI readback.
- White Tokkun playresults must not write normal White score, crown, Dani, profile, favorite, normal unlock, Don Challenge, Red/Yellow/Blue Tokkun, battle, or shop state. Recent-song rows derived from Tokkun practiced song numbers are the allowed side effect.
- White Banacoin-adjacent support is stateless compatibility. `getbanacoininfo.php`, `banacoinpayment.php`, and `banacoinerrorlog.php` return success shapes without wallet, balance, payment, coupon, or transaction persistence.
- White Don Challenge is server-side and stage-derived from normal White playresult stages, `white_don_challenge_data.json`, and White-owned progress/raw-fact tables. It is exposed through the dedicated AdminApi/WebUI Don Challenge contract only.
- White must not add standalone `challengecompe.php` cabinet route/readback semantics, protocol opt-in state, user/BNG challenge buckets, Red Don Challenge state reads, or ChallengeCompe-derived reward behavior unless new White client/proto/log/IDA evidence proves it.
- White `userdata.php` does not expose Don Challenge progress arrays as stateful readback; reward-song locks are derived through ordinary locked-song readback.
- Do not infer White title-plate behavior from newer AC15 eras. Check generated White wire and `Application/Handlers/BaidQuery.White.cs`; current White BAID title handling resolves through title text/catalog rarity and the generated field layout must be verified before schema claims.

## Repo Layout

- `Host/` composes the ASP.NET Core process, loads `Host/Configurations/*.json`, applies migrations, initializes catalogs, registers enabled-era adapters, hosts static WebUI files, and serves fallback routing.
- `Domain/` owns entities, enums, and constants. It has no project references.
- `Contracts.AdminApi/` owns DTOs shared by the admin API and WebUI.
- `Application/` owns Mediator requests, handlers, ports, `Common*` DTOs, server data shapes, and protocol byte helpers.
- `Infrastructure/` implements persistence, filesystem catalogs, identity, time, settings, and EF migrations.
- `Adapters.AdminApi/` serves `/api/...` routes for the WebUI.
- `Adapters.AllnetMucha/` serves cabinet lifecycle, Mucha, updater, activation, and GARM endpoints.
- `Adapters.GameProtocol.Shared/` owns shared protocol controller and compression helpers.
- `Adapters.GameProtocol.WwR08/` serves Nijiiro WW routes under `/v12r08_ww/*`.
- `Adapters.GameProtocol.CnR00/` serves Nijiiro CN routes under `/v12r00_cn/*`.
- `Adapters.GameProtocol.Green/` serves Green AC15 routes under `/v11r01/*`.
- `Adapters.GameProtocol.Blue/` serves Blue AC15 routes under `/v10r03/*`.
- `Adapters.GameProtocol.Yellow/` serves Yellow AC15 routes under `/v09r02/*`.
- `Adapters.GameProtocol.Red/` serves Red AC15 routes under `/v08r01/*`.
- `Adapters.GameProtocol.White/` serves White final AC15 routes under `/v07r03/*` and legacy White compatibility routes under `/v07r00/*`.
- `TaikoWebUI/` is the MudBlazor WebAssembly admin UI hosted by `Host`.
- `GreenCatalogExtractor/` and `LocalSaveModScoreMigrator/` are standalone utilities.
- `proto/` contains protocol schema inputs; generated wire models live in adapter `Wire/` folders.
- `.tools/blue/` contains local Blue reverse-engineering material. Treat it as local evidence, not runtime code.
- `.tools/white/` contains local White reverse-engineering material. Treat it as local evidence, not runtime code.

## Architecture Rules

- Keep era state separate. Blue, Green, Yellow, Red, White, and Nijiiro persistence must remain separate unless the state is truly shared identity data such as card/user identity.
- Use the existing partial-file pattern for era behavior: shared dispatcher in the unsuffixed file, era implementation in `.Nijiiro.cs`, `.Green.cs`, `.Blue.cs`, `.Yellow.cs`, `.Red.cs`, or `.White.cs`.
- Map generated protobuf DTOs through `Application/Dtos/Common*` shapes before handler logic. Do not persist wire DTOs directly.
- For Mapperly-specific behavior, do not rely on memory or prior agent summaries. Check the current official Mapperly documentation online, especially null-value behavior at `https://mapperly.riok.app/docs/configuration/mapper/#null-values`, constant/generated values at `https://mapperly.riok.app/docs/configuration/constant-generated-values/`, and generated-source inspection at `https://mapperly.riok.app/docs/configuration/generated-source/`.
- Mapperly mappers must remain source-generator driven. Do not replace Mapperly projections with hand-written mapper bodies; handwritten code in mapper classes is limited to helper conversions that are configured for Mapperly or discovered by Mapperly.
- When verifying Mapperly mapper implementation, inspect generated source, not only the handwritten partial declarations. Use `dotnet build /p:EmitCompilerGeneratedFiles=true` and review the emitted `.g.cs` files under the project `obj/.../generated/.../Riok.Mapperly/` path.
- Controllers should deserialize, map, call Mediator, and map back. Put business behavior in `Application/Handlers`.
- Use `IGameDataCatalog.For(GameEra)` and era catalog interfaces instead of hardcoded filesystem access from handlers.
- Resolve runtime data roots through `PathHelper` and era data path helpers. Do not hardcode `wwwroot/data/<era>` in new runtime code.
- For AdminApi era routes, preserve both legacy routes where they exist and `/api/{era}/...` routes validated by `EraRoute.TryParse`.
- Keep generated `Wire/` files out of manual cleanup unless regenerating protocol output.
- Never modify, regenerate, format, stage, or track anything under `proto/`. Protocol schema files are binary-derived hard truth and are read-only for agents. Never describe them as wrong or incomplete; if code, generated wire, runtime behavior, or analysis appears to conflict with `proto/`, treat the non-`proto/` side as wrong until proven otherwise and do not edit `proto/`.

## Local Game Data Link Rules

- `Host/wwwroot/data/<era>/data` is operator-supplied game data and must be treated as external local evidence, even when tests or builds depend on it.
- These `data` entries must be Windows directory symbolic links or junctions to local `USRDIR/data` roots, never copied directories. Do not copy RPCS3/game-install data into the repo tree.
- Never delete, move, overwrite, clean, recreate, or "repair" `Host/wwwroot/data/<era>/data` or its contents from an agent without explicit same-turn user approval for the exact path and exact action.
- Never run `Remove-Item`, `rm`, `git clean`, `robocopy /MIR`, cleanup scripts, broad restore commands, or recursive move/delete operations against `Host/wwwroot`, `Host/wwwroot/data`, or any `Host/wwwroot/data/<era>/data` link.
- If a `data` link is missing, broken, or no longer a link, stop and report the environment blocker. The only acceptable repair is an explicitly approved symlink/junction operation plus `.gitignore` coverage.
- Do not stage or commit game data, symlink targets, copied catalog XML/bin/NDP files, or generated local catalog outputs. Only tracked server-authored JSON sidecars already owned by the repo may be staged.
- Preserve NTFS ACL protections on local data links. If ACLs block an operation, treat that as a safety signal and ask before changing permissions.

## Testing Rules

- Do not add tests just to satisfy a TDD checkbox. Every new test must protect a specific evidence-backed cabinet behavior, runtime state transition, parser/packing rule, AdminApi/WebUI workflow, or no-cross-era/no-cross-mode persistence boundary.
- Game-facing tests are regression guards after evidence, not proof of client compatibility. Cabinet/RPCS3/client acceptance remains the compatibility gate.
- Do not test generated protobuf output, generated `Wire/` type/property existence, route inventory, controller attribute lists, DI registration shape, enum numeric values, static config key presence, source text, project files, migrations, private methods, or "returns `Result = 1`" stateless echoes unless there is a demonstrated runtime failure that only that assertion can catch.
- Do not assert literal values from committed config/sidecar data such as JSON catalog rows, shop `verup_no`, dates, ids, prices, text, or file contents. Tests may validate parser behavior and data shape only when that protects runtime behavior; exact config values are free data, not test contracts.
- Avoid assertions over `.cs`, `.csproj`, `Program.cs`, migrations, controller method bodies, class/file names, `Mediator.Send`, `SaveChanges`, reflection-only metadata, or other implementation strings.
- Useful tests exercise observable behavior: handler/service state changes, SQLite persistence and no-write boundaries, catalog/parser behavior, byte/bit packing owned by this repo, protocol payload classification backed by real captures/proto evidence, build/publish output when it affects deployed runtime files, API responses that drive WebUI behavior, and readback paths consumed by the cabinet.
- Mapper tests are allowed only when they protect nontrivial classification, omission, packing, or evidence-backed field placement. Do not write one-to-one copy/echo mapper tests.

## Data Caveats

- AC15 game data shares the same setup shape: operator-supplied `USRDIR/data` lives under `Host/wwwroot/data/<era>/data` in source checkouts, or `wwwroot/data/<era>/data` in published folders. Debug builds create output junctions for Green, Blue, Yellow, Red, and White when those source paths exist.
- Green required startup data comes from `config/S11100-1/musicinfo.xml`, `config/S11100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
- Blue normal catalog data comes from `config/S10100-1/musicinfo.xml`, `config/S10100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
- Yellow catalog data comes from `config/ST9100-1/musicinfo.xml`, `config/ST9100-1/musicmedleyinfo.xml`, `config/ST9100-1/defmusic.bin`, and `fumen/tuning.bin`.
- Red catalog data comes from `config/ST8100-1/musicinfo.xml`, `config/ST8100-1/musicmedleyinfo.xml`, `config/ST8100-1/defmusic.bin`, and `fumen/tuning.bin`.
- White catalog data comes from `config/ST7100-1/musicinfo.xml`, `config/ST7100-1/musicmedleyinfo.xml`, `config/ST7100-1/defmusic.bin`, `config/ST7100-1/present.xml`, `config/ST7100-1/spacialbaid.xml`, and `fumen/tuning.bin`.
- Blue battle availability requires the five parsed files under `config/S10100-1/battle`: `battleadjsetting.xml`, `battlenpcinfo.xml`, `battlestageinfo.xml`, `battlesupportinfo.xml`, and `battletokeninfo.xml`.
- Green, Blue, and Yellow item shop data is committed JSON under `Host/wwwroot/data/<era>/`; `rewardshopdata.bin` remains local provenance and is not a runtime dependency.
- Red and White Don Challenge data is committed JSON under `Host/wwwroot/data/<era>/<era>_don_challenge_data.json` and is the runtime sidecar for server-side progress/reward behavior.
- For every era, if a feature exists and expects committed server-authored data outside raw operator game data, the corresponding `Host/wwwroot/data/<era>/...` JSON should exist and be copied even when its data is intentionally empty.
- Blue customization JSON can be bootstrapped from Blue AC15 data when `AutoExtractCatalog` is enabled, with display names composed from shared and optional override name data.
- Treat title id `0` as the explicit empty/default title state.
- `rewardexecution.php` is log-and-success for Blue item-shop flow unless newer evidence proves a state-changing role.

## Common Commands

Run from the repo root:

```powershell
dotnet build TaikoLocalServer.slnx
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
dotnet run --project Host
dotnet publish Host/Host.csproj
dotnet ef migrations add <Name> --project Infrastructure --startup-project Host
dotnet ef database update --project Infrastructure --startup-project Host
```

Use the temp-output Host build when a running server locks `Host/bin/Debug/net10.0`.
