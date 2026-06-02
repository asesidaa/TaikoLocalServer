# Adapters.GameProtocol.Blue

Adapters.GameProtocol.Blue is the Blue AC15 game-protocol adapter. Game routes live under `/v10r03/chassis/*`; shared AC15 startup/version routes live under `/v01r00/chassis/*`.

## Role

Controllers translate Blue direct-protobuf payloads into `Application/Dtos/Common*` DTOs, send Mediator requests with `GameEra.Blue`, and map responses back to Blue protobuf wire types.

Blue is not a Green flag. Keep Blue controller routing, wire mappings, and protocol quirks Blue-owned.

## Key Folders

- `Controllers/` - Blue endpoint controllers, including normal play, item shop, and battle surfaces.
- `Mappers/` - Mapperly and hand-written Blue wire-to-common mappings.
- `Wire/` - generated or committed Blue protobuf wire models.
- `DependencyInjection.cs` - registers the Blue adapter.

## Blue Caveats

- Preserve direct-protobuf request handling for Blue game endpoints.
- `playresult.php` can carry normal or battle payloads; battle classification is based on battle release/stage sections.
- `battleuserdata.php` uses the Blue-owned Mediator query and reads persisted BlueBattle state after first-use starter state.
- `initialdatacheck.php` advertises battle availability from parsed Blue battle catalog data.
- Do not invent server-side battle rewards, token thresholds, boss completion, or stage graph behavior without concrete evidence.

## When To Add Code Here

- Add a Blue endpoint controller.
- Add or adjust Blue wire mapping.
- Add Blue adapter registration behavior.

Put runtime use-case behavior in `Application/Handlers/*.Blue.cs` and Blue catalog I/O in `Infrastructure/GameDataCatalog/Blue`.
