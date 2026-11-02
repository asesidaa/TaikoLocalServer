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

1. Extract the server's release anywhere.
2. Open [Configurations/ServerSettings.json](./Host/Configurations/ServerSettings.json) and set:
   - `"Mode"`: `"Local"` (the default, no logins) or `"Server"` (accounts and admins). See [Local or Server mode](#local-or-server-mode).
   - `"GamePath"`: your game install, the folder that contains `Data`, for example `"C:/Games/Taiko no Tatsujin Nijiiro Version"`. The web UI uses it to copy the game files the server needs. If the game is on another PC, leave it empty and set `"AllowRemoteGameDir": true` instead (see step 5).
3. (Optional) In the `Certificates` folder, import `root.pfx` into the trusted root store and `cert.pfx` into the personal store. The other import options can stay at their defaults.
4. Start the server: `TaikoLocalServer.exe` on Windows, which asks for administrator rights, or `./TaikoLocalServer` on Linux, as root or with permission to bind low ports. It listens on ports 80 and 443 as well as the game's ports. Then open [http://localhost](http://localhost).
   - **Server mode:** the web UI opens on a setup page until an admin exists. Enter the **setup code** printed in the server's console, then your card's access code and a password for the admin account (see [Web UI admins](#web-ui-admins-server-mode)).
   - **Local mode:** the web UI opens straight away.
5. Add the game files. In Server mode this is the setup's **Game files** step. Afterwards, and in Local mode, use **Admin › Server Management › Game Data**; until it's done, admins see a "Game files are missing" banner. Press **Extract game files**: this runs `extract` (`extract.exe` on Windows, next to the server) on your `GamePath`. It copies the datatables the server needs and converts the don-chan models, textures and animations for the web UI's 3D preview (about 175 MB). It writes only into the server's `wwwroot` folder, never into your game folder. Run it again after a game update; files that are already up to date are skipped.

   **The game is on another PC** (for example the server runs on a Linux box): with `"AllowRemoteGameDir": true`, open the web UI on the game PC and press **Choose game folder** instead. Pick the game folder (the one that contains `Data`, so `Data_mods` comes along). The browser sends only the files extract reads (about 600 MB of a 15 GB install), and the server runs extract on them, then deletes them.

   Or run extract yourself on the game PC, then copy its `wwwroot` output into the server's `wwwroot` folder:
   ```bash
   extract.exe --game "C:\Games\Taiko no Tatsujin Nijiiro Version"
   ```
   If it reports anything as `MISSING` or `FAILED`, check that the path points at a complete install. See [tools/extract](./tools/extract/README.md) for details.
6. Start your game! (The first boot with the server takes a good minute, be patient!)

## Configuration

### TaikoLocalServer configuration

There are various json files under [Host/wwwroot/data](./Host/wwwroot/data/) that can be customized.  
Please refer to the [Host README file](./Host/README.md) for documentation.

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

### Building the Web UI

`dotnet build` is all it takes. No Node.js needed.

- **Styles** (Tailwind, `TaikoWebUI/Styles/app.css` → `wwwroot/css/app.css`, committed). The first build downloads the pinned Tailwind CLI into `tools/tailwindcss/` and checks it against the release's published SHA-256. To build without downloading anything (CI, offline), pass `-p:WebUiToolRestore=false`; style changes then need `tools/tailwindcss/tailwindcss(.exe)` in place yourself.
- **don-chan's 3D view** (`wwwroot/js/don-viewer.js` and `don-normalize.js`) is plain JavaScript on three.js, which is committed in `wwwroot/lib/three` (0.184.0, no CDN: servers are often offline). There's no build step: edit and reload. With Node.js, `node --test TaikoWebUI/don-normalize.test.mjs` checks its material rules.

The admin UI uses **MudBlazor 9.x**. If you customize `TaikoWebUI/`, refer to the [MudBlazor v9 docs](https://mudblazor.com/) — components and parameters changed across the 7→8→9 majors compared to older forks.
