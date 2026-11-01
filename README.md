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
2. Run `extract.exe` from the server folder, pointing it at your game install (the folder that contains `Data`):
   ```bash
   extract.exe --game "C:\Games\Taiko no Tatsujin Nijiiro Version"
   ```
   This copies the datatables the server needs and converts the don-chan models, textures and animations for the web UI's 3D preview (about 175 MB, in a few seconds). It writes into the `wwwroot` folder next to it, never into your game folder. Run it again after a game update; files that are already up to date are skipped. If it reports anything as `MISSING` or `FAILED`, check that `--game` points at a complete install. See [tools/extract](./tools/extract/README.md) for details.
3. (Optional) In `Certificates` folder, import `root.pfx` to trusted root store and `cert.pfx` to personal store. All the other import options can be kept default
4. Visit [http://localhost](http://localhost). If the WebUI starts without errors, the config is fine
5. Start your game! (First boot with the server will take a good minute, be patient!)

## Configuration

### TaikoLocalServer configuration

There are various json files under [Host/wwwroot/data](./Host/wwwroot/data/) that can be customized.  
Please refer to the [Host README file](./Host/README.md) for documentation.

### Web UI admins

When `AuthenticationRequired` is `true` in [AuthSettings.json](./Host/Configurations/AuthSettings.json), players log in to the web UI with their access code and a password. Admins are managed from the command line, run from the server folder. Each card must have played at least one credit first.

```bash
TaikoLocalServer.exe admin promote <accessCode>       # make this card's user an admin
TaikoLocalServer.exe admin demote <accessCode>        # remove admin rights
TaikoLocalServer.exe admin set-password <accessCode>  # set the web UI password (prompts, input hidden)
```

These commands apply database migrations, make the change and exit; they don't start the server. To create your first admin, run `promote` and then `set-password` on your own card, and log in. On Windows, run them from an administrator terminal, like the server itself.

### TaikoWebUI configuration

The WebUI has a few settings you can change in [appsettings.json](./TaikoWebUI/wwwroot/appsettings.json)  
Please refer to the [taikowebui readme file](./TaikoWebUI/README.md) for documentation.

## For developers

This solution uses [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) — package versions live in `Directory.Packages.props` at the repo root, not in individual `.csproj` files. To add or bump a package, edit `Directory.Packages.props` and add a versionless `<PackageReference Include="…" />` to the project that uses it.

Shared MSBuild defaults (TFM, Nullable, ImplicitUsings, LangVersion) live in `Directory.Build.props`.

The repo pins the .NET SDK band via `global.json`.

This solution targets **.NET 10 LTS**. Install the .NET 10 SDK from <https://dotnet.microsoft.com/download> (or any 10.0.x patch — the `global.json` allows latestFeature roll-forward).

The admin UI uses **MudBlazor 9.x**. If you customize `TaikoWebUI/`, refer to the [MudBlazor v9 docs](https://mudblazor.com/) — components and parameters changed across the 7→8→9 majors compared to older forks.
