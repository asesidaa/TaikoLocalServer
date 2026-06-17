# Stack Research: White AC15 0.13 Support

## Summary
- Reuse the existing .NET 10 / ASP.NET Core adapter stack. White should be a first-class era beside Red and Yellow, not a new transport or framework.
- Generate White protobuf-net wire DTOs from `proto/white/taiko.proto` and `proto/white/vsinterface.proto` into a new `Adapters.GameProtocol.White/Wire` folder with `+nullablevaluetype=yes`.
- Mirror the Red/Yellow adapter project shape: `FrameworkReference` to `Microsoft.AspNetCore.App`, project references to `Adapters.GameProtocol.Shared`, `Application`, and `Contracts.AdminApi`, and package references to `protobuf-net` and `Riok.Mapperly`.
- White requires foundation wiring not present yet: `GameEra.White`, `ServerSettings:Eras:White`, Host project/solution/test references, Host DI and application-part gating, Infrastructure catalog registration, and White data-path/catalog abstractions.
- No new external stack is needed. The only tooling dependency is the existing protobuf-net `protogen` generator already available in this checkout.
- White evidence is enough for proto/data-driven setup, but not enough for binary-backed route/root claims. The live `.tools/white/EBOOT.ELF.i64` is nonzero (`129893515` bytes), but route strings are still not proven.

## Tooling And Generation
- Use the existing repo-local generator first:

```powershell
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.White\Wire -Iproto\white +nullablevaluetype=yes proto\white\taiko.proto
.\.tools\protogen.exe --csharp_out=Adapters.GameProtocol.White\Wire -Iproto\white +nullablevaluetype=yes proto\white\vsinterface.proto
```

- `.\.tools\protogen.exe --version` reports `protogen 3.2.52+f4db4afce3`. Global `protogen --version` reports the same version, and `dotnet tool list --global` includes `protobuf-net.protogen 3.2.52`.
- Rename or place generated files to match existing adapter convention: `Adapters.GameProtocol.White/Wire/Game.cs` for `taiko.proto` and `Adapters.GameProtocol.White/Wire/VsInterface.cs` for `vsinterface.proto`. Adjust namespace to `TaikoLocalServer.Adapters.GameProtocol.White.Wire` only if the generator output does not already match.
- Do not hand-edit generated fields, tags, presence helpers, or generated spelling. Fix proto/generator inputs or mapper code instead.
- Red has an extra `Wire/V08R00/Game.cs` alias for a proven route variant. Do not create a White variant wire folder until local White route evidence proves another protocol prefix/version.
- Add a White adapter project to `TaikoLocalServer.slnx`, `Host/Host.csproj`, and `Tests/Tests.csproj` alongside Red/Yellow.
- Add a White debug data junction target in `Host/Host.csproj`, mirroring `CreateRedGameDataSymlinkForDebug`, and exclude `wwwroot\data\white\data\**` from publish/content like the other operator-supplied AC15 data roots.
- Add copy entries only for committed White server-authored JSON sidecars that actually exist or are intentionally created, such as White recommend/telop/movie/event-folder/taikojuku/ChallengeCompe data. Do not try to publish the operator `data` symlink tree.
- Verify generation and Mapperly output with:

```powershell
dotnet build TaikoLocalServer.slnx
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
dotnet build /p:EmitCompilerGeneratedFiles=true
```

- For Mapperly implementation inspection, review emitted `.g.cs` files under the relevant project `obj/.../generated/.../Riok.Mapperly/` path after a build. Current official Mapperly 4.3.1 docs confirm generated-source emission through `EmitCompilerGeneratedFiles`.

## Existing Stack To Reuse
- **ASP.NET Core controllers**: use adapter-local controllers that deserialize White direct-protobuf requests, call Mediator, and map responses back to White wire DTOs. Do not put business behavior in controllers.
- **Adapters.GameProtocol.Shared**: reuse `BaseProtocolController`, protobuf response conventions, shared startup/version controller patterns, compression helpers where already used, and direct-protobuf behavior unless White client evidence proves a different transport.
- **protobuf-net**: keep generated `[ProtoContract]` wire DTOs in the White adapter. The repo central package version is `protobuf-net 3.2.56`; the existing generator version is `3.2.52`.
- **Mapperly 4.3.1**: add `MapperlyDefaults.cs` with:

```csharp
[assembly: MapperDefaults(
    AutoUserMappings = false,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
```

- **Mapperly mapper pattern**: use source-generated partial mappers with helper conversions configured through Mapperly. Handwritten mapper bodies should be limited to explicit conversion helpers and not replace source-generated projection.
- **Application/Ac15 shared core**: reuse capability-owned services and DTOs for normal play, userdata, self-best, crowns, recommendations, folders/telops, Taikojuku, Dani where White 0.13 evidence supports them. Extend `Ac15EraProfiles` with a White profile and White protocol limits rather than copying Red/Yellow business logic.
- **Era-owned persistence and catalog contracts**: add White-owned entities, `IWhiteCatalog`, `WhiteEraGameDataCatalog`, `WhiteGameDataPaths`, and White EF tables/migrations. Reuse shared AC15 loaders where file formats match, but keep the catalog interface and state separate.
- **PathHelper / settings**: resolve runtime paths through `PathHelper.GetDataPath(GameEra.White)` and `ServerSettings:Eras:White:GameDataPath`, not hardcoded `wwwroot/data/white` paths in handlers.
- **Host settings/gating**: follow current enabled-era gating in `Host/Program.cs`. Add `AddGameProtocolWhite()` only when White is enabled and remove the White application part when disabled.
- **AdminApi/WebUI era routing**: extend existing era-routed surfaces and `EraRoute.TryParse` behavior after `GameEra.White` exists. Avoid White-only API shapes unless a White-only capability needs one.
- **Build/test commands**: use `dotnet build TaikoLocalServer.slnx`, `dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"` when normal output is locked, and focused `dotnet test Tests/Tests.csproj` slices that protect observable White behavior.

## New Stack
- No new stack is needed.
- Do not add a new proto generator, serialization library, mapping library, repository layer, codegen framework, test framework, or runtime data scraper for White.
- If `protogen` becomes unavailable on another machine, install or restore `protobuf-net.Protogen` only as a tooling prerequisite after approval; the current checkout already has usable generator binaries.

## Evidence Notes
- `proto/white/taiko.proto` and `proto/white/vsinterface.proto` are present and are the correct White generation inputs.
- White `vsinterface.proto` contains the same startup/version message family used by older AC15 support: `StartupAuth*`, `VerupAuth*`, and `VerupComplete*`.
- White `taiko.proto` exposes BAID, mydon entry, userdata, playresult, self-best, crowns, recommendations, folders, telops, Taikojuku, tournament-check stubs, bookkeeping, heartbeat, head-clerk, getreitai, rewardcardcheck, and rewardexecution.
- White proto evidence does not expose explicit battle, item-shop purchase/catalog, Tokkun stage, gacha, ChallengeCompe endpoint, or Banacoin wallet/payment messages. It does expose embedded challenge/competition fields in userdata/playresult, so Don Challenge should be treated as an older-AC15 capability lead, not as proof of Red's separate `challengecompe.php` route.
- White data is a symbolic link at `Host/wwwroot/data/white/data` pointing to `H:\RPCS3\rpcs3-blue\dev_hdd0\game\SCEEXE001 White\USRDIR\data`.
- The observed White config root is `config/ST7100-1`, containing `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, and `spacialbaid.xml`. `fumen/tuning.bin` also exists under the White data root.
- There are no committed White JSON sidecars under `Host/wwwroot/data/white` yet.
- `.tools/white/EBOOT.ELF.i64` exists and has live filesystem length `129893515`; earlier zero-byte notes are superseded. It is still not automatic route-prefix proof: route prefix, runtime root, and client-call sequencing need IDB route extraction, logs, captures, or another local evidence source before being treated as proven.
- Actual Yellow proto inputs in this checkout are `proto/yellow/yellow-final.proto`, `proto/yellow/yellow-00.proto`, and `proto/yellow/vsinterface.proto`; do not copy stale references to `proto/yellow/yellow.proto`.

## Implementation Risks
- **Route/version proof gap**: without extracted White route strings or logs/captures, selecting a game route prefix is the largest tooling/evidence risk. Keep route assumptions narrow and evidence-tagged.
- **Generator drift**: generation commands are documented in plans and `.tools/protogen.exe` exists, but there is no tracked `.config/dotnet-tools.json` manifest. Record the exact generator version used when regenerating White wire files.
- **Nullable presence fallout**: `+nullablevaluetype=yes` changes optional primitive properties to nullable where possible. White controllers/mappers must handle nullable request fields explicitly instead of relying on default `0` values.
- **Mapperly strictness**: `RequiredMappingStrategy.Target` will surface unmapped White response fields as warnings/errors depending on project settings. Treat that as useful protocol-boundary feedback, not as a reason to weaken defaults or hand-write mapper bodies.
- **Host gating omissions**: missing White entries in `GameEra`, `Host/Program.cs`, `Infrastructure/DependencyInjection.cs`, `ServerSettings.json`, `.slnx`, `Host.csproj`, or `Tests.csproj` will produce partial builds where the adapter compiles but routes/catalogs are absent at runtime.
- **Publish/debug data path omissions**: failing to add White content exclusion and debug junction logic can either copy a huge operator data tree into publish output or leave debug builds without `wwwroot/data/white/data`.
- **Feature overreach**: Red and Yellow are useful analogs, but White 0.13 has fewer proto surfaces. Do not import Red Tokkun/Banacoin/ChallengeCompe endpoint behavior, Yellow item-shop behavior, or Blue battle behavior without White-specific evidence.
- **Sidecar timing**: White collectable/reward data such as `present.xml` and `spacialbaid.xml` should inform later sidecar extraction, but core adapter generation should not depend on scraped or guessed JSON that is not yet committed.
