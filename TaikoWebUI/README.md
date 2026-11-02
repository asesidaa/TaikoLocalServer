# Taiko Web UI

This is the solution for the front end part.
It is implemented with Blazor Webassembly (also in C#).

## TaikoWebUI appsettings.json config

This section is for configuring the TaikoWebUI [appsettings.json](./wwwroot/appsettings.json) file.
This file holds **WebUI-only** presentation settings. The mode (Local or Server, from the server's
[ServerSettings.json](../Host/Configurations/ServerSettings.json)) and the Server-mode access rules
(stored in the server's database, changed by admins) are fetched at startup via `GET /api/Auth/Config`.

```json
{
  "WebUiSettings": {
    "Title": "TaikoWebUI",
    "DisplayUnplayedDans": "false", //Display all Dans, even ones that haven't been played yet.
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
