# Adapters.GameProtocol.Red

Adapters.GameProtocol.Red is the Red AC15 game-protocol adapter. Primary game routes live under `/v08r01/chassis/*`; some compatibility routes also live under `/v08r00_tw/chassis/*`. Shared AC15 startup/version routes live under `/v01r00/chassis/*`.

## Role

Controllers translate Red direct-protobuf payloads into `Application/Dtos/Common*` DTOs, send Mediator requests with `GameEra.Red`, and map responses back to Red protobuf wire types.

Red is a first-class era. Keep Red controller routing, wire mappings, protocol quirks, and persisted state Red-owned.

## Key Folders

- `Controllers/` - Red endpoint controllers, including normal play, Don Challenge compatibility/readback boundaries, Tokkun tutorial readback, Banacoin-adjacent compatibility, and metadata surfaces.
- `Mappers/` - Mapperly and hand-written Red wire-to-common mappings.
- `Wire/` - generated or committed Red protobuf wire models.
- `DependencyInjection.cs` - registers the Red adapter.

## Red Caveats

- Preserve direct-protobuf request handling for Red game endpoints.
- Red game data is rooted at `config/ST8100-1`.
- Red has Red-owned normal play, profile, favorites, recents, self-best, crowns, Dani, and Don Point state.
- Red Tokkun support is tutorial/readback scoped; do not invent raw Red Tokkun history tables without evidence.
- Red Don Challenge is server-side and stage-derived through `red_don_challenge_data.json` plus Red-owned raw-fact/progress tables.
- Red `challengecompe.php` is a separate protocol compatibility/readback surface. Do not treat it as Don Challenge opt-in, reward authority, or raw upload replay.
- Red Banacoin-adjacent routes are stateless compatibility surfaces.
- Do not mirror Blue battle or Yellow WaiWai behavior into Red without concrete Red evidence.

## When To Add Code Here

- Add a Red endpoint controller.
- Add or adjust Red wire mapping.
- Add Red adapter registration behavior.

Put runtime use-case behavior in `Application/Handlers/*.Red.cs` and Red catalog I/O in `Infrastructure/GameDataCatalog/Red`.
