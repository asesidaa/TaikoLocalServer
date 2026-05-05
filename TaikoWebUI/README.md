# Taiko Web UI

This is the solution for the front end part.
It is implemented with Blazor Webassembly (also in C#).

## TaikoWebUI appsettings.json config

This section is for configuring the TaikoWebUI [appsettings.json](./wwwroot/appsettings.json) file.
This file holds **WebUI-only** presentation settings. Authentication / authorization knobs
(`AuthenticationRequired`, `OnlyAdmin`, `BoundAccessCodeUpperLimit`, `RegisterWithLastPlayTime`,
`AllowUserDelete`, `AllowFreeProfileEditing`) live in the server's
[Host/Configurations/AuthSettings.json](../Host/Configurations/AuthSettings.json) and are
fetched at startup via `GET /api/Auth/Config`. The server is the source of truth — the WebUI
never needs to be restarted for an auth-policy change there.

```json
{
  "WebUiSettings": {
    "Title": "TaikoWebUI",
    "DisplayUnplayedDans": "false", //Display all Dans, even ones that haven't been played yet.
    "MaxWidth": "3", //0:Large, 1:Medium, 2:Small, 3:ExtraLarge, 4:ExtraExtraLarge
    "SongLeaderboardSettings": {
      "DisablePagination": "false",
      "PageSize": "10"
    },
    "SupportedLanguages": [
      {
        "CultureCode": "en-US",
        "DisplayName": "English"
      },
      {
        "CultureCode": "fr-FR",
        "DisplayName": "Français"
      },
      {
        "CultureCode": "zh-Hans",
        "DisplayName": "简体中文"
      },
      {
        "CultureCode": "zh-Hant",
        "DisplayName": "繁體中文"
      },
      {
        "CultureCode": "ja",
        "DisplayName": "日本語"
      }
    ]
  }
}
```
