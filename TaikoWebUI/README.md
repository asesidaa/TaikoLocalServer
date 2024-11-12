# Taiko Web UI

This is the solution for the front end part.  
It is implemented with Blazor Webassembly (also in C#).

## TaikoWebUI appsettings.json config

This section is for configuring the TaikoWebUI [appsettings.json](./wwwroot/appsettings.json) file.  
This file is used to configure the web UI.

```json
{
  "WebUiSettings": {
    "Title": "TaikoWebUI",
    "LoginRequired": "false", //Setting this to true will change the UI to allow users to register / login.
    "OnlyAdmin": "false",
    "BoundAccessCodeUpperLimit": "3",
    "RegisterWithLastPlayTime": "false",
    "AllowUserDelete": "true",
    "AllowFreeProfileEditing": "true", //Enabling this allows user to set all their profile settings freely
                                       //Bypassing the need to unlock titles, costumes, etc.
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
