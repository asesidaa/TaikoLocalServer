# Adapters.GameProtocol.White

Adapters.GameProtocol.White is the White AC15 game-protocol adapter. Final-version game routes live under `/v07r03/chassis/*`; legacy compatibility game routes live under `/v07r00/chassis/*`. Shared AC15 startup/version routes live under `/v01r00/chassis/*`.

## Role

Controllers translate White direct-protobuf payloads into `Application/Dtos/Common*` DTOs, send Mediator requests with `GameEra.White`, and map responses back to White protobuf wire types.

White is a first-class era. Keep White controller routing, wire mappings, protocol quirks, and persisted state White-owned.

## Key Folders

- `Controllers/` - White endpoint controllers. Most stateful endpoints expose both final and legacy action methods where both route families are supported.
- `Mappers/` - Mapperly and hand-written final White wire-to-common mappings.
- `LegacyWire/` - legacy `/v07r00` protobuf models and mapping helpers.
- `Wire/` - final `/v07r03` protobuf wire models.
- `WhiteRoutePrefixes.cs` - canonical final and compatibility route prefixes.
- `DependencyInjection.cs` - registers the White adapter.

## White Caveats

- Preserve final `/v07r03` and legacy `/v07r00` as separate protocol surfaces. Do not share generated DTOs or force final-only fields onto legacy wire.
- White game data is rooted at `config/ST7100-1`.
- White required data includes `musicinfo.xml`, `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`, and `fumen/tuning.bin`.
- White final support includes proven Tokkun, stateless Banacoin-adjacent routes, difficulty panel state, and final heartbeat Banacoin status fields.
- White Tokkun writes only White tutorial/history facts and recent-song rows from practiced songs. It must not fall through to normal, Dani, Don Challenge, profile, favorite, unlock, or cross-era state.
- White Don Challenge is server-side and stage-derived through `white_don_challenge_data.json` plus White-owned raw-fact/progress tables. It is exposed through dedicated AdminApi/WebUI contracts.
- White has no standalone `challengecompe.php` cabinet route/readback semantics in current evidence.
- Do not infer White title-plate or BAID field placement from newer AC15 eras. Inspect generated White wire and `Application/Handlers/BaidQuery.White.cs` before changing schema or mapper behavior.

## When To Add Code Here

- Add a White endpoint controller or a final/legacy route method.
- Add or adjust White final or legacy wire mapping.
- Add White adapter registration behavior.

Put runtime use-case behavior in `Application/Handlers/*.White.cs` and White catalog I/O in `Infrastructure/GameDataCatalog/White`.
