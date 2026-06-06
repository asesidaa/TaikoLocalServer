# TaikoWebUI

TaikoWebUI is the Blazor WebAssembly admin interface hosted by `Host`. It uses MudBlazor and consumes `Contracts.AdminApi` DTOs over the server's `/api/...` routes.

## Role

The WebUI provides browser access to profiles, score/history views, Dani data, favorites, customization catalogs, game data, and server administration surfaces. It is built into the Host publish output and does not run as a separate deployment.

Era routing is handled through `TaikoWebUI.Utilities.WebUiEra`. Use that helper for user URLs and API URLs instead of composing era paths by hand.

## Configuration

Presentation settings live in [wwwroot/appsettings.json](./wwwroot/appsettings.json):

```json
{
  "WebUiSettings": {
    "Title": "TaikoWebUI",
    "DisplayUnplayedDans": "false",
    "MaxWidth": "3",
    "SongLeaderboardSettings": {
      "DisablePagination": "false",
      "PageSize": "10"
    }
  }
}
```

Authentication and account-policy settings live on the server in [../Host/Configurations/AuthSettings.json](../Host/Configurations/AuthSettings.json). The WebUI fetches that policy from `GET /api/Auth/Config`.

## Blue Notes

Blue is a supported WebUI era. Blue routes should use Blue AdminApi endpoints and Blue catalog data for customization, Dani, normal play history, favorites, and profile settings.

Blue Tokkun history is not currently a WebUI surface.

Treat title id `0` as the empty/default title in UI surfaces.

## When To Add Code Here

- Add browser UI for an AdminApi feature.
- Add client-side presentation state.
- Add WebUI route helpers or services that consume `Contracts.AdminApi`.

Keep server behavior, persistence, and game-protocol logic outside this project.
