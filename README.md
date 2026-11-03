# Taiko Local Server

TaikoLocalServer is a local ASP.NET Core server for Taiko no Tatsujin cabinet protocols. It serves game endpoints, AllNet/Mucha lifecycle endpoints, SQLite persistence, era-specific filesystem catalogs, and the Blazor WebAssembly admin UI from one process.

Supported eras:

- Nijiiro CHN and WW
- Green AC15
- Blue AC15, including normal play, battle runtime support, and Tokkun practice mode
- Yellow AC15, including normal play, item shop, Tokkun, Banacoin compatibility, and WebUI/AdminApi routing
- Red AC15, including normal play, tutorial-only Tokkun readback, Don Challenge, and WebUI/AdminApi routing
- White AC15, including final `/v07r03` game routes, legacy `/v07r00` compatibility routes, White-owned runtime state, Tokkun, Don Challenge, and WebUI/AdminApi routing

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
- [Adapters.GameProtocol.Yellow](./Adapters.GameProtocol.Yellow/README.md) - Yellow AC15 adapter
- [Adapters.GameProtocol.Red](./Adapters.GameProtocol.Red/README.md) - Red AC15 adapter
- [Adapters.GameProtocol.White](./Adapters.GameProtocol.White/README.md) - White AC15 adapter
- [GreenCatalogExtractor](./GreenCatalogExtractor/README.md) - Green AC15 catalog extraction utility

## Installation

### Prerequisites

- Install the .NET 10 SDK when running from source.
- Use a working game install with dongle and QR reader emulation. [TaikoArcadeLoader](https://github.com/esuo1198/TaikoArcadeLoader) can provide these pieces; Teknoparrot is not suitable for this server.

### Setup Steps

1. Extract the server's release anywhere.
2. Open [Configurations/ServerSettings.json](./Host/Configurations/ServerSettings.json) and set:
   - `"Mode"`: `"Local"` (the default, no logins) or `"Server"` (accounts and admins). See [Local or Server mode](#local-or-server-mode).
   - `"ServerName"` (optional): the name the web UI shows, `"TaikoLocalServer"` by default.
   - `"Eras"` › `"Nijiiro"` › `"GameDataPath"`: your game install, the folder that contains `Data`, for example `"C:/Games/Taiko no Tatsujin Nijiiro Version"`. The web UI uses it to copy the game files the server needs. If the game is on another PC, leave it empty: you can send its folder from your browser instead (see step 5).
3. (Optional) In the `Certificates` folder, import `root.pfx` into the trusted root store and `cert.pfx` into the personal store. The other import options can stay at their defaults.
4. Start the server: `TaikoLocalServer.exe` on Windows, which asks for administrator rights, or `./TaikoLocalServer` on Linux, as root or with permission to bind low ports. It listens on ports 80 and 443 as well as the game's ports. Then open [http://localhost](http://localhost).
   - **Server mode:** the web UI opens on a setup page until an admin exists. Enter the **setup code** printed in the server's console, then your card's access code and a password for the admin account (see [Web UI admins](#web-ui-admins-server-mode)).
   - **Local mode:** the web UI opens straight away.
5. Add the game files. In Server mode this is the setup's **Game files** step. Afterwards, and in Local mode, use **Admin › Server Management › Games**; until it's done, admins see a "Game files are missing" banner. Tick Nijiiro and press **Extract**: the server reads that folder. It copies the datatables the server needs and converts the don-chan models, textures and animations for the web UI's 3D preview (about 175 MB). It writes only into the server's `wwwroot` folder, never into your game folder. Run it again after a game update; files that are already up to date are skipped.

   **The game is on another PC** (for example the server runs on a Linux box): open the web UI on the game PC, switch to **From this browser** and press **Choose Nijiiro's folder…** instead. Pick the game folder (the one that contains `Data`, so `Data_mods` comes along). The browser sends only the files the server reads (about 600 MB of a 15 GB install), and the server extracts from them, then deletes them. Only admins can do this, and only the files the extractor lists are accepted; set `"AllowRemoteGameDir": false` to turn it off.

   Or run it from a command line on the game PC with a copy of the server, then copy its `wwwroot` output into the server's `wwwroot` folder:
   ```bash
   TaikoLocalServer.exe extract --game "C:\Games\Taiko no Tatsujin Nijiiro Version"
   ```
   If it reports anything as `MISSING` or `FAILED`, check that the path points at a complete install. See [Infrastructure.Extract](./Infrastructure.Extract/README.md) for details.
6. Start your game! (The first boot with the server takes a good minute, be patient!)

### Older games (AC15 eras)

The server also runs Green, Blue, Yellow, Red, White, Murasaki, Kimidori and Momoiro cabinets. Each is off until you turn it on in [ServerSettings.json](./Host/Configurations/ServerSettings.json) (`"Eras"` › `"<Era>"` › `"Enabled": true`); at least one era must be on. For each one you turn on, the server needs that game's files at `wwwroot/data/<era>/data` (for example `wwwroot/data/green/data`):
   - **Extract them (recommended):** set the game's `"GamePath"` to its RPCS3 install (or its `USRDIR/data` folder, or a `.zip` of it), restart, then in **Admin › Server Management › Games** tick the game and press **Extract**. Several games run one after another. Extract copies the files the server reads and converts the game's models, art and drum sounds for the web UI, and afterwards says what the copy lacks (an early Red's config, Blue's battle files). You can also pick the game's folder in the browser instead (`"AllowRemoteGameDir"`, on by default).
   - **Or copy them:** put the game's original `USRDIR/data` folder there yourself, or set its `"GameDataPath"`.

The files each era reads from it:
   - Green requires `config/S11100-1/musicinfo.xml`, `config/S11100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
   - Blue requires `config/S10100-1/musicinfo.xml`, `config/S10100-1/musicmedleyinfo.xml`, and `fumen/tuning.bin`.
   - Yellow requires `config/ST9100-1/musicinfo.xml`, `config/ST9100-1/musicmedleyinfo.xml`, `config/ST9100-1/defmusic.bin`, and `fumen/tuning.bin`.
   - Red requires `config/ST8100-1/musicinfo.xml`, `config/ST8100-1/musicmedleyinfo.xml`, `config/ST8100-1/defmusic.bin`, and `fumen/tuning.bin`.
   - White requires `config/ST7100-1/musicinfo.xml`, `config/ST7100-1/musicmedleyinfo.xml`, `config/ST7100-1/defmusic.bin`, `config/ST7100-1/present.xml`, `config/ST7100-1/spacialbaid.xml`, and `fumen/tuning.bin`.

Blue battles are offered only when the whole battle XML folder is there, `wwwroot/data/blue/data/config/S10100-1/battle`:
   - `battleadjsetting.xml`
   - `battlenpcinfo.xml`
   - `battlestageinfo.xml`
   - `battlesupportinfo.xml`
   - `battletokeninfo.xml`

Settings for each era:

- `ServerSettings:Eras:<Era>:Enabled` - registers or removes era routes.
- `ServerSettings:Eras:<Era>:GameDataPath` - AC15 source data path for Green, Blue, Yellow, Red, and White.
- `ServerSettings:Eras:<Era>:GamePath` - the game's install (RPCS3 folder, its `USRDIR/data`, or a `.zip`), read by **Extract** in Server Management › Games.
- `ServerSettings:Eras:<Era>:AutoExtractCatalog` - allows first-run AC15 customization catalog extraction.
- `ServerSettings:Eras:<Era>:CustomizationNameDataPath` - optional override directory for AC15 customization display names.
- `ServerSettings:Eras:<Era>:EnableShop` and `ActiveShopSeasonId` - controls Green, Blue, and Yellow item-shop availability.
- `ServerSettings:Eras:<Era>:EnableDonChallenge` and `ActiveDonChallengeBundleId` - controls Red and White Don Challenge availability.

#### AC15 Data Symlink Example

From a source checkout:

```powershell
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\Host\wwwroot\data\green\data'
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\Host\wwwroot\data\blue\data'
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\Host\wwwroot\data\yellow\data'
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\Host\wwwroot\data\red\data'
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\Host\wwwroot\data\white\data'
```

From an extracted release folder:

```powershell
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\wwwroot\data\green\data'
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\wwwroot\data\blue\data'
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\wwwroot\data\yellow\data'
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\wwwroot\data\red\data'
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\wwwroot\data\white\data'
```

PowerShell may need Administrator privileges unless Windows Developer Mode allows unprivileged symlink creation.

## Configuration

Server configuration lives under [Host/Configurations](./Host/Configurations/). Runtime data lives under [Host/wwwroot/data](./Host/wwwroot/data/) in a source checkout and under `wwwroot/data` next to a published executable.

The most important server settings are:

### Local or Server mode

Set `"Mode"` in [ServerSettings.json](./Host/Configurations/ServerSettings.json):

- **`Local`** (the default): no accounts or logins. Everyone who can open the web UI can edit any profile and use the admin pages. Good for a home cabinet.
- **`Server`**: players log in with their access code and a password, and only admins can use the admin pages. Account rules (who may register, deleting users, free profile editing, the access-code limit) are changed by admins in the web UI.

There is nothing else to configure for logins: the server generates its own signing key on first start.

To switch modes, change `"Mode"` and restart the server. Players and scores carry over both ways. Going from Local to Server shows the setup page if there's no admin yet; going from Server to Local keeps accounts and passwords but ignores them.

### Web UI admins (Server mode)

The first time the server starts in Server mode without an admin, it prints a **setup code** in its console, for example `K7QF-2M9X`. Open the web UI: it goes to the setup page until an admin exists. Enter the code, then your card and a password, and that account becomes the admin. The card doesn't need to have played on this server yet. The code is printed again whenever the web UI is opened, and changes each start until an admin exists.

Admins can also be managed from the command line, run from the server folder:

```bash
TaikoLocalServer.exe admin promote <accessCode>       # make this card's user an admin
TaikoLocalServer.exe admin demote <accessCode>        # remove admin rights
TaikoLocalServer.exe admin set-password <accessCode>  # set the web UI password (prompts, input hidden)
TaikoLocalServer.exe admin reset-2fa <accessCode>     # turn off 2-step verification (lost phone and recovery codes)
```

These commands apply database migrations, make the change and exit; they don't start the server. To create your first admin, run `promote` and then `set-password` on your own card, and log in. On Windows, run them from an administrator terminal, like the server itself.

### Backing up

Everything players have (accounts, scores, profiles) is in one SQLite file, `wwwroot/taiko.db3` next to `TaikoLocalServer.exe`. Back it up with your own scheduled task or cron job. Either copy it while the server is stopped, or, while it runs, use SQLite's online backup: `sqlite3 wwwroot/taiko.db3 ".backup taiko-backup.db3"`. To restore, stop the server and put the copy back as `taiko.db3`.

### TaikoWebUI configuration

See [Host/README.md](./Host/README.md) for data file details.

The WebUI reads presentation settings from [TaikoWebUI/wwwroot/appsettings.json](./TaikoWebUI/wwwroot/appsettings.json). See [TaikoWebUI/README.md](./TaikoWebUI/README.md).

## For Developers

This solution uses Central Package Management. Package versions live in `Directory.Packages.props`; individual project files use versionless package references.

Shared MSBuild defaults live in `Directory.Build.props`, and the SDK band is pinned by `global.json`.

Useful commands from the repo root:

### Building the Web UI

`dotnet build` is all it takes. No Node.js needed.

- **Styles** (Tailwind, `TaikoWebUI/Styles/app.css` → `wwwroot/css/app.css`, committed). The first build downloads the pinned Tailwind CLI into `tools/tailwindcss/` and checks it against the release's published SHA-256. To build without downloading anything (CI, offline), pass `-p:WebUiToolRestore=false`; style changes then need `tools/tailwindcss/tailwindcss(.exe)` in place yourself.
- **don-chan's 3D view** (`wwwroot/js/don-viewer.js` and `don-normalize.js`) is plain JavaScript on three.js, which is committed in `wwwroot/lib/three` (0.184.0, no CDN: servers are often offline). There's no build step: edit and reload. With Node.js, `node --test TaikoWebUI/don-normalize.test.mjs` checks its material rules.

The web UI uses no component library: Tailwind v4 classes and small components in `TaikoWebUI/Components/Ui`.

### Tests

`dotnet test Tests/Tests.csproj` runs the xunit tests. Some read an older game's data and fail without it (`Host\wwwroot\data\<era>\data`). `Tests/StressSeed` is not a test: `dotnet run --project Tests/StressSeed` builds a large database for load testing.
