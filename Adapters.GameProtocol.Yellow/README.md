# Adapters.GameProtocol.Yellow

Adapters.GameProtocol.Yellow is the Yellow AC15 game-protocol adapter. Game routes live under `/v09r02/chassis/*`; shared AC15 startup/version routes live under `/v01r00/chassis/*`.

## Role

Controllers translate Yellow direct-protobuf payloads into `Application/Dtos/Common*` DTOs, send Mediator requests with `GameEra.Yellow`, and map responses back to Yellow protobuf wire types.

Yellow is a first-class era. Keep Yellow controller routing, wire mappings, protocol quirks, and persisted state Yellow-owned.

## Key Folders

- `Controllers/` - Yellow endpoint controllers, including normal play, item shop, Tokkun, WaiWai-compatible facts, Banacoin-adjacent compatibility, and metadata surfaces.
- `Mappers/` - Mapperly and hand-written Yellow wire-to-common mappings.
- `Wire/` - generated or committed Yellow protobuf wire models.
- `DependencyInjection.cs` - registers the Yellow adapter.

## Yellow Caveats

- Preserve direct-protobuf request handling for Yellow game endpoints.
- Yellow game data is rooted at `config/ST9100-1`.
- Yellow has item-shop state, Don/Katsu medal accounting, Tokkun tutorial/history persistence, and limited WaiWai fact handling.
- Yellow Banacoin-adjacent routes are stateless compatibility surfaces; do not add wallet, payment, coupon, or transaction authority.
- Yellow ChallengeCompe route support is protocol compatibility only unless current evidence proves runtime behavior.
- Do not mirror Blue battle behavior into Yellow.

## When To Add Code Here

- Add a Yellow endpoint controller.
- Add or adjust Yellow wire mapping.
- Add Yellow adapter registration behavior.

Put runtime use-case behavior in `Application/Handlers/*.Yellow.cs` and Yellow catalog I/O in `Infrastructure/GameDataCatalog/Yellow`.
