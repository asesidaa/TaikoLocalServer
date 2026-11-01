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

### Local or Server mode

Set `"Mode"` in [ServerSettings.json](./Host/Configurations/ServerSettings.json):

- **`Local`** (the default): no accounts or logins. Everyone who can open the web UI can edit any profile and use the admin pages. Good for a home cabinet.
- **`Server`**: players log in with their access code and a password, and only admins can use the admin pages. Account rules (who may register, deleting users, free profile editing, the access-code limit) are changed by admins in the web UI.

There is nothing else to configure for logins: the server generates its own signing key on first start.

### Web UI admins (Server mode)

The first time the server starts in Server mode without an admin, it prints a **setup code** in its console, for example `K7QF-2M9X`. Open the web UI: it goes to the setup page until an admin exists. Enter the code, then your card and a password, and that account becomes the admin. The card doesn't need to have played on this server yet. The code is printed again whenever the web UI is opened, and changes each start until an admin exists.

Admins can also be managed from the command line, run from the server folder:

```bash
TaikoLocalServer.exe admin promote <accessCode>       # make this card's user an admin
TaikoLocalServer.exe admin demote <accessCode>        # remove admin rights
TaikoLocalServer.exe admin set-password <accessCode>  # set the web UI password (prompts, input hidden)
```

These commands apply database migrations, make the change and exit; they don't start the server. To create your first admin, run `promote` and then `set-password` on your own card, and log in. On Windows, run them from an administrator terminal, like the server itself.

### Backing up

Everything players have (accounts, scores, profiles) is in one SQLite file, `wwwroot/taiko.db3` next to `TaikoLocalServer.exe`. Back it up with your own scheduled task or cron job. Either copy it while the server is stopped, or, while it runs, use SQLite's online backup: `sqlite3 wwwroot/taiko.db3 ".backup taiko-backup.db3"`. To restore, stop the server and put the copy back as `taiko.db3`.

### TaikoWebUI configuration

The WebUI has a few settings you can change in [appsettings.json](./TaikoWebUI/wwwroot/appsettings.json)  
Please refer to the [taikowebui readme file](./TaikoWebUI/README.md) for documentation.

## For developers

This solution uses [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) — package versions live in `Directory.Packages.props` at the repo root, not in individual `.csproj` files. To add or bump a package, edit `Directory.Packages.props` and add a versionless `<PackageReference Include="…" />` to the project that uses it.

Shared MSBuild defaults (TFM, Nullable, ImplicitUsings, LangVersion) live in `Directory.Build.props`.

The repo pins the .NET SDK band via `global.json`.

This solution targets **.NET 10 LTS**. Install the .NET 10 SDK from <https://dotnet.microsoft.com/download> (or any 10.0.x patch — the `global.json` allows latestFeature roll-forward).

The admin UI uses **MudBlazor 9.x**. If you customize `TaikoWebUI/`, refer to the [MudBlazor v9 docs](https://mudblazor.com/) — components and parameters changed across the 7→8→9 majors compared to older forks.
