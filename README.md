# Taiko Local Server

TaikoLocalServer is a local ASP.NET Core server for Taiko no Tatsujin cabinet protocols. It serves game endpoints, AllNet/Mucha lifecycle endpoints, SQLite persistence, era-specific filesystem catalogs, and the Blazor WebAssembly admin UI from one process.

Supported eras:

- Nijiiro CHN and WW
- Green AC15
- Blue AC15, including normal play, battle runtime support, and Tokkun practice mode

## Project Readmes

- [Host](./Host/README.md) - server runtime, data layout, era setup, and operator JSON files
- [TaikoWebUI](./TaikoWebUI/README.md) - WebUI presentation settings
- [Application](./Application/README.md) - Mediator handlers, ports, common DTOs, and use-case layer
- [Infrastructure](./Infrastructure/README.md) - SQLite, migrations, filesystem catalogs, identity, and time
- [Domain](./Domain/README.md) - entities, enums, and constants
- [Contracts.AdminApi](./Contracts.AdminApi/README.md) - DTOs shared by AdminApi and WebUI
- [Adapters.AdminApi](./Adapters.AdminApi/README.md) - admin REST API routes
- [Adapters.AllnetMucha](./Adapters.AllnetMucha/README.md) - AllNet/Mucha lifecycle routes
- [Adapters.GameProtocol.Shared](./Adapters.GameProtocol.Shared/README.md) - shared game-protocol helpers
- [Adapters.GameProtocol.WwR08](./Adapters.GameProtocol.WwR08/README.md) - Nijiiro WW adapter
- [Adapters.GameProtocol.CnR00](./Adapters.GameProtocol.CnR00/README.md) - Nijiiro CN adapter
- [Adapters.GameProtocol.Green](./Adapters.GameProtocol.Green/README.md) - Green AC15 adapter
- [Adapters.GameProtocol.Blue](./Adapters.GameProtocol.Blue/README.md) - Blue AC15 adapter
- [GreenCatalogExtractor](./GreenCatalogExtractor/README.md) - Green AC15 catalog extraction utility
- [LocalSaveModScoreMigrator](./LocalSaveModScoreMigrator/README.md) - local-save import utility

## Installation

### Prerequisites

- Install the .NET 10 SDK when running from source.
- Use a working game install with dongle and QR reader emulation. [TaikoArcadeLoader](https://github.com/esuo1198/TaikoArcadeLoader) can provide these pieces; Teknoparrot is not suitable for this server.

### Setup Steps

1. Extract a release anywhere, or use a source checkout.
2. Configure enabled eras in [Host/Configurations/ServerSettings.json](./Host/Configurations/ServerSettings.json). At least one of `Nijiiro`, `Green`, or `Blue` must be enabled.
3. For Nijiiro, copy the game datatable files from `Data/x64/datatable` into `wwwroot/data/nijiiro/datatable` in a release folder, or `Host/wwwroot/data/nijiiro/datatable` in a source checkout:
   - `music_order.bin`
   - `musicinfo.bin`
   - `wordlist.bin`
   - `don_cos_reward.bin`
   - `shougou.bin`
   - `neiro.bin`
4. For Green and Blue AC15, provide each enabled era's original `USRDIR/data` folder under the matching era data root:
   - Green: `wwwroot/data/green/data` in a release folder, or `Host/wwwroot/data/green/data` in a source checkout.
   - Blue: `wwwroot/data/blue/data` in a release folder, or `Host/wwwroot/data/blue/data` in a source checkout.
5. Verify the AC15 catalog inputs for each enabled era:
   - Green requires `config/S11100-1/musicinfo.xml`, `config/S11100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
   - For Blue AC15, normal startup requires `config/S10100-1/musicinfo.xml`, `config/S10100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
6. For Blue battle availability, keep the complete battle XML folder under `wwwroot/data/blue/data/config/S10100-1/battle`:
   - `battleadjsetting.xml`
   - `battlenpcinfo.xml`
   - `battlestageinfo.xml`
   - `battlesupportinfo.xml`
   - `battletokeninfo.xml`
7. Optionally import `root.pfx` into the trusted root store and `cert.pfx` into the personal store from the `Certificates` folder.
8. Start the server and visit [http://localhost](http://localhost). If the WebUI loads, the server and UI are being served from the same host.

### AC15 Data Symlink Example

From a source checkout:

```powershell
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\Host\wwwroot\data\green\data'
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\Host\wwwroot\data\blue\data'
```

From an extracted release folder:

```powershell
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\wwwroot\data\green\data'
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\wwwroot\data\blue\data'
```

PowerShell may need Administrator privileges unless Windows Developer Mode allows unprivileged symlink creation.

## Configuration

Server configuration lives under [Host/Configurations](./Host/Configurations/). Runtime data lives under [Host/wwwroot/data](./Host/wwwroot/data/) in a source checkout and under `wwwroot/data` next to a published executable.

The most important server settings are:

- `ServerSettings:Eras:<Era>:Enabled` - registers or removes era routes.
- `ServerSettings:Eras:<Era>:GameDataPath` - AC15 source data path for Green and Blue.
- `ServerSettings:Eras:<Era>:AutoExtractCatalog` - allows first-run AC15 customization catalog extraction.
- `ServerSettings:Eras:<Era>:CustomizationNameDataPath` - optional override directory for AC15 customization display names.
- `ServerSettings:Eras:<Era>:EnableShop` and `ActiveShopSeasonId` - controls Green and Blue item-shop availability.

See [Host/README.md](./Host/README.md) for data file details.

The WebUI reads presentation settings from [TaikoWebUI/wwwroot/appsettings.json](./TaikoWebUI/wwwroot/appsettings.json). See [TaikoWebUI/README.md](./TaikoWebUI/README.md).

## For Developers

This solution uses Central Package Management. Package versions live in `Directory.Packages.props`; individual project files use versionless package references.

Shared MSBuild defaults live in `Directory.Build.props`, and the SDK band is pinned by `global.json`.

Useful commands from the repo root:

```powershell
dotnet build TaikoLocalServer.slnx
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
dotnet run --project Host
dotnet publish Host/Host.csproj
```

Use the temp-output Host build when a running server locks `Host/bin/Debug/net10.0`.
