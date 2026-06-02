# Contracts.AdminApi

Contracts.AdminApi is the DTO project shared by `Adapters.AdminApi` and `TaikoWebUI`. It contains JSON shapes, view models, shared server-data contracts, and converters used on both sides of the admin API.

## Role

This project keeps browser and server serialization in sync without depending on Application or Infrastructure. It may reference Domain for enums, but it should not contain runtime behavior.

## Key Folders

- `Requests/` - DTOs accepted by AdminApi controllers.
- `Responses/` - DTOs returned by AdminApi controllers.
- `ViewModels/` - UI-facing shapes bound by WebUI components and pages.
- `ServerData/` - JSON shapes that the WebUI also needs to read.
- `Converters/` - shared JSON converters.
- `Authorization/` - admin authorization contract helpers.

## Era Notes

Admin contracts may carry era values or era-scoped fields when the WebUI needs them. Keep Blue-specific contract data explicit; do not overload Green fields for Blue behavior.

## When To Add Code Here

- Add a request, response, or view model consumed by TaikoWebUI.
- Add a shared JSON shape used by both server and WebUI.
- Add a converter required by both sides of admin serialization.

Do not add EF entities, Application handlers, Infrastructure services, or game protocol wire models here.
