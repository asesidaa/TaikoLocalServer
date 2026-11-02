# Adapters.GameProtocol.Green

Adapters.GameProtocol.Green is the Green AC15 game-protocol adapter. Game routes live under `/v11r01/chassis/*`; shared AC15 startup/version routes live under `/v01r00/chassis/*`.

## Role

Controllers translate Green wire payloads into `Application/Dtos/Common*` DTOs, send Mediator requests with `GameEra.Green`, and map responses back to Green protobuf wire types.

Green payloads are AC15-specific and can use Green fixed-width byte helpers and Green payload decoding where required.

## Key Folders

- `Controllers/` - Green endpoint controllers.
- `Mappers/` - Mapperly and hand-written Green wire-to-common mappings.
- `Wire/` - generated or committed Green protobuf wire models.
- `DependencyInjection.cs` - registers the Green adapter.

## When To Add Code Here

- Add a Green endpoint controller.
- Add or adjust Green wire mapping.
- Add Green adapter registration behavior.

Put runtime use-case behavior in `Application/Handlers/*.Green.cs` and Green catalog I/O in `Infrastructure/GameDataCatalog/Green`.
