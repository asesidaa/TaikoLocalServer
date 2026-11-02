# Adapters.AdminApi

The inbound adapter for the admin REST API consumed by `TaikoWebUI`.
Routes are under `/api/...`.

## Role

HTTP-facing layer between the WebUI and the server. Controllers take and
return `Contracts.AdminApi` types; most read and write through
`ITaikoDbContext` and the other ports directly, and a few send Mediator
requests shared with the game protocol. Auth uses `[Authorize]` and the admin policy; in
Local mode (`ServerSettings.Mode`) `AuthAwarePolicyEvaluator` lets every request through.

## Dependencies

- Inbound: `Host` only.
- Outbound: `Application`, `Infrastructure`, `Contracts.AdminApi`.
- Notable packages: `Microsoft.AspNetCore.Authentication.JwtBearer`,
  `Riok.Mapperly`, `Otp.NET`, `QRCoder`, `Swan.Core`, `Throw`.

## Key folders

- `Controllers/` — REST controllers. Inherit `BaseAdminController` (which
  lazily resolves `IMediator` and `Logger` from request services).
- `Authorization/` — API-token scopes (`ApiScope`), the server-state filter,
  `AuthorizeOwnerOrAdmin`, Activity logging, 2-step codes.
- `Mapping/` — Mapperly mappers from entities to the contracts.
- `BaseAdminController.cs` — base class for all admin controllers.
- `DependencyInjection.cs` — `AddAdminApi(IConfiguration)` extension that
  registers JWT bearer auth, the OTP/QR services, and the admin
  controllers.

## When to add code here

- New admin API endpoint → add a controller under `Controllers/`,
  inheriting `BaseAdminController`, with `[Authorize]` (or the admin policy)
  and `[ApiScope]` if API tokens may use it.
- The request/response DTO it accepts/returns lives in
  `Contracts.AdminApi/{Requests,Responses,ViewModels}` so the WebUI can
  consume it.

See the root `CLAUDE.md` for the full hexagonal layout.

## Era Routing

Era-aware admin routes should support `/api/{era}/...` with `EraRoute.TryParse`. Preserve existing legacy routes where a controller already exposes them.

Blue, Yellow, Red, White, and Murasaki WebUI/AdminApi parity is present for profile, normal play history, favorites, Dani, customization data, and user settings surfaces where the era implements those flows. Keep routes backed by the selected era's state and catalogs rather than another era's state.

Blue Tokkun history is persisted for protocol support but is not currently exposed through AdminApi/WebUI routes.

Red and White Don Challenge use dedicated `/api/{era}/DonChallenge/...` routes over server-side progress/reward contracts. Do not expose cabinet ChallengeCompe wire DTOs or raw protocol buckets through AdminApi unless new evidence proves a separate admin workflow.

White Tokkun history is persisted for protocol support but is not currently exposed through AdminApi/WebUI routes.

Murasaki global-score, shopping, Don Challenge, ChallengeCompe, Tokkun, battle, and proto-only special request families are not exposed through AdminApi/WebUI unless newer Murasaki route/client evidence proves those workflows.
