# Taiko Local Server

This is a server for Taiko no Tatsujin Nijiiro ver CHN and 39.06  
It is composed of two major components :

- [TaikoLocalServer](./Host/): The server handling the game's requests
- [TaikoWebUI](./TaikoWebUI/): The frontend handling user profiles.

## Installation

### Prerequisite

- You need a working game install, with dongle and QR reader emulation.  
  You can use [TaikoArcadeLoader](https://github.com/esuo1198/TaikoArcadeLoader) to have these working (Teknoparrot will NOT work).

### Setup Steps

1. Extract the Server's release anywhere
2. From the game files (Data/x64/datatable), copy `music_order.bin`, `musicinfo.bin`, `wordlist.bin`, `don_cos_reward.bin`, `shougou.bin`, `neiro.bin` to [Host/wwwroot/data/nijiiro/datatable](./Host/wwwroot/data/nijiiro/datatable/)
3. (Optional) In `Certificates` folder, import `root.pfx` to trusted root store and `cert.pfx` to personal store. All the other import options can be kept default
4. Visit [http://localhost](http://localhost). If the WebUI starts without errors, the config is fine
5. Start your game! (First boot with the server will take a good minute, be patient!)

## Configuration

### TaikoLocalServer configuration

There are various json files under [Host/wwwroot/data](./Host/wwwroot/data/) that can be customized. Nijiiro files live under `nijiiro/`, cross-era files live under `shared/`, and Green support is controlled by `ServerSettings:Eras` in [ServerSettings.json](./Host/Configurations/ServerSettings.json).  
Please refer to the [Host README file](./Host/README.md) for documentation.

### TaikoWebUI configuration

The WebUI has a few settings you can change in [appsettings.json](./TaikoWebUI/wwwroot/appsettings.json)  
Please refer to the [taikowebui readme file](./TaikoWebUI/README.md) for documentation.

## For developers

This solution uses [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) — package versions live in `Directory.Packages.props` at the repo root, not in individual `.csproj` files. To add or bump a package, edit `Directory.Packages.props` and add a versionless `<PackageReference Include="…" />` to the project that uses it.

Shared MSBuild defaults (TFM, Nullable, ImplicitUsings, LangVersion) live in `Directory.Build.props`.

The repo pins the .NET SDK band via `global.json`.

This solution targets **.NET 10 LTS**. Install the .NET 10 SDK from <https://dotnet.microsoft.com/download> (or any 10.0.x patch — the `global.json` allows latestFeature roll-forward).

The admin UI uses **MudBlazor 9.x**. If you customize `TaikoWebUI/`, refer to the [MudBlazor v9 docs](https://mudblazor.com/) — components and parameters changed across the 7→8→9 majors compared to older forks.
