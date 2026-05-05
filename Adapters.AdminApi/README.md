# Adapters.AdminApi

The inbound adapter for the admin REST API consumed by `TaikoWebUI`.
Routes are under `/api/...`.

## Role

HTTP-facing translation layer between the WebUI and the `Application`
handlers. Each controller deserializes a `Contracts.AdminApi` request
type, sends it through `IMediator`, then returns a `Contracts.AdminApi`
response type. Auth is opt-in via `[AuthorizeIfRequired]`, which short-
circuits when `AuthSettings.AuthenticationRequired` is false.

## Dependencies

- Inbound: `Host` only.
- Outbound: `Application`, `Infrastructure`, `Contracts.AdminApi`.
- Notable packages: `Microsoft.AspNetCore.Authentication.JwtBearer`,
  `Riok.Mapperly`, `Otp.NET`, `Swan.Core`, `Throw`.

## Key folders

- `Controllers/` — REST controllers. Inherit `BaseAdminController` (which
  lazily resolves `IMediator` and `Logger` from request services).
- `Filters/` — `AuthorizeIfRequiredAttribute` (the toggleable auth
  filter).
- `BaseAdminController.cs` — base class for all admin controllers.
- `DependencyInjection.cs` — `AddAdminApi(IConfiguration)` extension that
  registers JWT bearer auth, the OTP/QR services, and the admin
  controllers.

## When to add code here

- New admin API endpoint → add a controller under `Controllers/`,
  inheriting `BaseAdminController`, with `[AuthorizeIfRequired]` if it
  needs auth.
- The request/response DTO it accepts/returns lives in
  `Contracts.AdminApi/{Requests,Responses,ViewModels}` so the WebUI can
  consume it.

Do **not** query `ITaikoDbContext` directly from a controller — that's a
handler's job. Send through `IMediator` instead.

See the root `CLAUDE.md` for the full hexagonal layout.
