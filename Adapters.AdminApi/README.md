# Adapters.AdminApi

Adapters.AdminApi is the inbound REST adapter consumed by `TaikoWebUI`. Routes are under `/api/...`.

## Role

Controllers translate HTTP requests into Application requests or admin DTO operations, then return `Contracts.AdminApi` response shapes. Authentication is opt-in through `[AuthorizeIfRequired]`, which follows `Host/Configurations/AuthSettings.json`.

## Key Folders

- `Controllers/` - admin REST controllers.
- `Filters/` - authorization filter helpers.
- `Mapping/` - mapper helpers for admin contract shapes.
- `BaseAdminController.cs` - shared controller base with lazy service access.
- `DependencyInjection.cs` - registers auth, OTP/QR services, and controllers.

## Era Routing

Era-aware admin routes should support `/api/{era}/...` with `EraRoute.TryParse`. Preserve existing legacy routes where a controller already exposes them.

Blue, Yellow, Red, White, and Murasaki WebUI/AdminApi parity is present for profile, normal play history, favorites, Dani, customization data, and user settings surfaces where the era implements those flows. Keep routes backed by the selected era's state and catalogs rather than another era's state.

Blue Tokkun history is persisted for protocol support but is not currently exposed through AdminApi/WebUI routes.

Red and White Don Challenge use dedicated `/api/{era}/DonChallenge/...` routes over server-side progress/reward contracts. Do not expose cabinet ChallengeCompe wire DTOs or raw protocol buckets through AdminApi unless new evidence proves a separate admin workflow.

White Tokkun history is persisted for protocol support but is not currently exposed through AdminApi/WebUI routes.

Murasaki global-score, shopping, Don Challenge, ChallengeCompe, Tokkun, battle, and proto-only special request families are not exposed through AdminApi/WebUI unless newer Murasaki route/client evidence proves those workflows.

## When To Add Code Here

- Add an admin endpoint used by the WebUI.
- Add route-level validation for admin inputs.
- Add an adapter mapper for admin contract shapes.

Put request and response DTOs in `Contracts.AdminApi`. Put runtime behavior in Application handlers when the operation belongs to a game use case.
