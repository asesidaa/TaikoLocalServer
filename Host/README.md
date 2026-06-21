# Taiko Local Server

This is the solution for server.  
Server is implemented with ASP.NET Core 10. ORM is Entity Framework Core 10. Database is SQLite for easier setup.  
As the game uses protobuf, `protobuf-net` is used for serializing and deserializing the data.  

- [Taiko Local Server](#taiko-local-server)
  - [Data file layout (per-era)](#data-file-layout-per-era)
  - [AC15 Setup](#ac15-setup)
    - [Required files](#required-files)
    - [Symlink or copy AC15 game data](#symlink-or-copy-ac15-game-data)
    - [Customization catalogs](#customization-catalogs)
    - [AC15 optional JSON](#ac15-optional-json)
    - [AC15 item shop](#ac15-item-shop)
    - [AC15 Don Challenge](#ac15-don-challenge)
    - [Blue battle and Tokkun](#blue-battle-and-tokkun)
    - [White final and legacy routes](#white-final-and-legacy-routes)
    - [Murasaki routes and split metadata](#murasaki-routes-and-split-metadata)
  - [Datatable documentation](#datatable-documentation)
    - [dan\_data.json](#dan_datajson)
    - [event\_folder\_data.json](#event_folder_datajson)
    - [gaiden\_data.json](#gaiden_datajson)
    - [intro\_data.json](#intro_datajson)
    - [locked\_songs\_data.json](#locked_songs_datajson)
    - [movie\_data.json](#movie_datajson)
    - [qrcode\_data.json](#qrcode_datajson)
    - [shop\_folder\_data.json](#shop_folder_datajson)
    - [token\_data.json](#token_datajson)

## Data file layout (per-era)

`wwwroot/data/` is partitioned by game era. In a source checkout this tree
lives under `Host/wwwroot/data/`; in a published release it lives next to the
server executable under `wwwroot/data/`.

```text
wwwroot/data/
|-- nijiiro/                    Nijiiro-era operator-edited datatables
|   |-- dan_data.json
|   |-- event_folder_data.json
|   |-- movie_data.json
|   |-- intro_data.json
|   |-- locked_*_data.json
|   |-- shop_folder_data.json
|   |-- special_songs_data.json
|   |-- gaiden_data.json
|   `-- datatable/              Operator-supplied Nijiiro game binaries
|       |-- musicinfo.bin
|       |-- music_order.bin
|       |-- wordlist.bin
|       |-- don_cos_reward.bin
|       |-- shougou.bin
|       `-- neiro.bin
|-- green/                      Green AC15 server-owned JSON plus game data link/copy
|   |-- telop_data.json         Optional Green telops
|   |-- movie_data.json         Green attract movie permissions
|   |-- green_item_shop_data.json Green item-shop seasons and item rows
|   |-- green_event_folder_data.json Optional Green event folders
|   |-- green_taikojuku_verup_data.json Green Taikojuku verup metadata
|   |-- green_costume_data.json Generated or curated Green customization catalog
|   |-- green_title_data.json   Generated or curated Green title catalog
|   |-- green_neiro_data.json   Generated or curated Green tone catalog
|   `-- data/                   Green game USRDIR/data tree, usually symlinked
|       |-- config/S11100-1/
|       |   |-- musicinfo.xml
|       |   `-- musicmedleyinfo.xml
|       `-- fumen/
|           `-- tuning.bin
|-- blue/                       Blue AC15 server-owned JSON plus game data link/copy
|   |-- blue_event_folder_data.json Optional Blue event folders
|   |-- blue_telop_data.json        Optional Blue telops
|   |-- blue_movie_data.json        Blue attract movie permissions
|   |-- blue_item_shop_data.json    Blue item shop seasons and item rows
|   |-- blue_taikojuku_verup_data.json Blue Taikojuku verup metadata
|   |-- blue_costume_data.json      Generated or curated Blue customization catalog
|   |-- blue_title_data.json        Generated or curated Blue title catalog
|   |-- blue_neiro_data.json        Generated or curated Blue tone catalog
|   `-- data/                       Blue game USRDIR/data tree, usually symlinked
|       |-- config/S10100-1/
|       |   |-- musicinfo.xml
|       |   |-- musicmedleyinfo.xml
|       |   `-- battle/             Blue battle XML files
|       `-- fumen/
|           `-- tuning.bin
|-- yellow/                     Yellow AC15 server-owned JSON plus game data link/copy
|   |-- yellow_event_folder_data.json
|   |-- yellow_telop_data.json
|   |-- yellow_movie_data.json
|   |-- yellow_item_shop_data.json
|   |-- yellow_taikojuku_verup_data.json
|   `-- data/
|       |-- config/ST9100-1/
|       |   |-- musicinfo.xml
|       |   |-- musicmedleyinfo.xml
|       |   `-- defmusic.bin
|       `-- fumen/
|           `-- tuning.bin
|-- red/                        Red AC15 server-owned JSON plus game data link/copy
|   |-- red_event_folder_data.json
|   |-- red_telop_data.json
|   |-- red_movie_data.json
|   |-- red_taikojuku_verup_data.json
|   |-- red_don_challenge_data.json
|   `-- data/
|       |-- config/ST8100-1/
|       |   |-- musicinfo.xml
|       |   |-- musicmedleyinfo.xml
|       |   `-- defmusic.bin
|       `-- fumen/
|           `-- tuning.bin
|-- white/                      White AC15 server-owned JSON plus game data link/copy
|   |-- white_event_folder_data.json
|   |-- white_telop_data.json
|   |-- white_movie_data.json
|   |-- white_taikojuku_verup_data.json
|   |-- white_don_challenge_data.json
|   `-- data/
|       |-- config/ST7100-1/
|       |   |-- musicinfo.xml
|       |   |-- musicmedleyinfo.xml
|       |   |-- defmusic.bin
|       |   |-- present.xml
|       |   `-- spacialbaid.xml
|       `-- fumen/
|           `-- tuning.bin
|-- murasaki/                   Murasaki AC15 server-owned JSON plus game data link/copy
|   |-- murasaki_event_folder_data.json
|   |-- murasaki_telop_data.json
|   |-- murasaki_movie_data.json
|   |-- murasaki_taikojuku_verup_data.json
|   `-- data/
|       |-- config/ST6100-1/
|       |   |-- musicinfo.xml
|       |   |-- musicmedleyinfo.xml
|       |   |-- defmusic.bin
|       |   |-- present.xml
|       |   `-- spacialbaid.xml
|       `-- fumen/
|           `-- tuning.bin
`-- shared/                     Cross-era operator-edited tables and AC15 name overrides
    |-- token_data.json
    |-- qrcode_data.json
    |-- costume_name_data.json
    |-- title_name_data.json
    `-- neiro_name_data.json
```

Era availability is controlled by `Configurations/ServerSettings.json` under
`ServerSettings:Eras`. At least one of `Nijiiro`, `Green`, `Blue`, `Yellow`,
`Red`, `White`, or `Murasaki` must be enabled or the host refuses to start. Disabled-era
controller assemblies are removed from ASP.NET Core routing at startup.

## AC15 Setup

Green, Blue, Yellow, Red, White, and Murasaki AC15 use the same setup shape:

- Enable the era in `Configurations/ServerSettings.json`.
- Point `ServerSettings:Eras:<Era>:GameDataPath` at that era's `USRDIR/data`
  tree. The default value is `wwwroot/data/<era>/data`.
- Provide the server-owned JSON files under `wwwroot/data/<era>/`.
- Keep the game-owned `USRDIR/data` tree local, either copied or linked under
  `wwwroot/data/<era>/data`.

### Required files

When `ServerSettings:Eras:Green:Enabled` is `true`, Green startup requires:

- `wwwroot/data/green/data/config/S11100-1/musicinfo.xml`
- `wwwroot/data/green/data/config/S11100-1/musicmedleyinfo.xml`
- `wwwroot/data/green/data/fumen/tuning.bin`

When `ServerSettings:Eras:Blue:Enabled` is `true`, normal Blue startup requires:

- `wwwroot/data/blue/data/config/S10100-1/musicinfo.xml`
- `wwwroot/data/blue/data/config/S10100-1/musicmedleyinfo.xml`
- `wwwroot/data/blue/data/fumen/tuning.bin`

When `ServerSettings:Eras:Yellow:Enabled` is `true`, Yellow startup requires:

- `wwwroot/data/yellow/data/config/ST9100-1/musicinfo.xml`
- `wwwroot/data/yellow/data/config/ST9100-1/musicmedleyinfo.xml`
- `wwwroot/data/yellow/data/config/ST9100-1/defmusic.bin`
- `wwwroot/data/yellow/data/fumen/tuning.bin`

When `ServerSettings:Eras:Red:Enabled` is `true`, Red startup requires:

- `wwwroot/data/red/data/config/ST8100-1/musicinfo.xml`
- `wwwroot/data/red/data/config/ST8100-1/musicmedleyinfo.xml`
- `wwwroot/data/red/data/config/ST8100-1/defmusic.bin`
- `wwwroot/data/red/data/fumen/tuning.bin`

When `ServerSettings:Eras:White:Enabled` is `true`, White startup requires:

- `wwwroot/data/white/data/config/ST7100-1/musicinfo.xml`
- `wwwroot/data/white/data/config/ST7100-1/musicmedleyinfo.xml`
- `wwwroot/data/white/data/config/ST7100-1/defmusic.bin`
- `wwwroot/data/white/data/config/ST7100-1/present.xml`
- `wwwroot/data/white/data/config/ST7100-1/spacialbaid.xml`
- `wwwroot/data/white/data/fumen/tuning.bin`

When `ServerSettings:Eras:Murasaki:Enabled` is `true`, Murasaki startup requires:

- `wwwroot/data/murasaki/data/config/ST6100-1/musicinfo.xml`
- `wwwroot/data/murasaki/data/config/ST6100-1/musicmedleyinfo.xml`
- `wwwroot/data/murasaki/data/config/ST6100-1/defmusic.bin`
- `wwwroot/data/murasaki/data/config/ST6100-1/present.xml`
- `wwwroot/data/murasaki/data/config/ST6100-1/spacialbaid.xml`
- `wwwroot/data/murasaki/data/fumen/tuning.bin`

The build excludes `wwwroot/data/<era>/data/**` AC15 game-data trees from
publish output. Operator game data should not be committed or shipped by the
project.

### Symlink or copy AC15 game data

From a source checkout, run these from the repository root and point each link
at the matching AC15 dump:


```powershell
New-Item -ItemType SymbolicLink -Target 'path\to\green\USRDIR\data\' -Path '.\Host\wwwroot\data\green\data'
New-Item -ItemType SymbolicLink -Target 'path\to\blue\USRDIR\data\' -Path '.\Host\wwwroot\data\blue\data'
New-Item -ItemType SymbolicLink -Target 'path\to\yellow\USRDIR\data\' -Path '.\Host\wwwroot\data\yellow\data'
New-Item -ItemType SymbolicLink -Target 'path\to\red\USRDIR\data\' -Path '.\Host\wwwroot\data\red\data'
New-Item -ItemType SymbolicLink -Target 'path\to\white\USRDIR\data\' -Path '.\Host\wwwroot\data\white\data'
New-Item -ItemType SymbolicLink -Target 'path\to\murasaki\USRDIR\data\' -Path '.\Host\wwwroot\data\murasaki\data'
```

From an extracted release folder, run these from the folder containing
`TaikoLocalServer.exe`:

```powershell
New-Item -ItemType SymbolicLink -Target 'path\to\green\USRDIR\data\' -Path '.\wwwroot\data\green\data'
New-Item -ItemType SymbolicLink -Target 'path\to\blue\USRDIR\data\' -Path '.\wwwroot\data\blue\data'
New-Item -ItemType SymbolicLink -Target 'path\to\yellow\USRDIR\data\' -Path '.\wwwroot\data\yellow\data'
New-Item -ItemType SymbolicLink -Target 'path\to\red\USRDIR\data\' -Path '.\wwwroot\data\red\data'
New-Item -ItemType SymbolicLink -Target 'path\to\white\USRDIR\data\' -Path '.\wwwroot\data\white\data'
New-Item -ItemType SymbolicLink -Target 'path\to\murasaki\USRDIR\data\' -Path '.\wwwroot\data\murasaki\data'
```

If you prefer copying for a release instead of linking:

```powershell
New-Item -ItemType Directory -Force -Path '.\wwwroot\data\green' | Out-Null
Copy-Item -Recurse -Path 'path\to\green\USRDIR\data' -Destination '.\wwwroot\data\green\data'
New-Item -ItemType Directory -Force -Path '.\wwwroot\data\blue' | Out-Null
Copy-Item -Recurse -Path 'path\to\blue\USRDIR\data' -Destination '.\wwwroot\data\blue\data'
New-Item -ItemType Directory -Force -Path '.\wwwroot\data\yellow' | Out-Null
Copy-Item -Recurse -Path 'path\to\yellow\USRDIR\data' -Destination '.\wwwroot\data\yellow\data'
New-Item -ItemType Directory -Force -Path '.\wwwroot\data\red' | Out-Null
Copy-Item -Recurse -Path 'path\to\red\USRDIR\data' -Destination '.\wwwroot\data\red\data'
New-Item -ItemType Directory -Force -Path '.\wwwroot\data\white' | Out-Null
Copy-Item -Recurse -Path 'path\to\white\USRDIR\data' -Destination '.\wwwroot\data\white\data'
New-Item -ItemType Directory -Force -Path '.\wwwroot\data\murasaki' | Out-Null
Copy-Item -Recurse -Path 'path\to\murasaki\USRDIR\data' -Destination '.\wwwroot\data\murasaki\data'
```

PowerShell may need to run as Administrator, unless Windows Developer Mode
allows unprivileged symlink creation.

In Debug builds, if an AC15 source data folder exists under
`Host/wwwroot/data/<era>/data`, MSBuild creates a matching output junction under
`Host/bin/Debug/net10.0/wwwroot/data/<era>/data` so `dotnet run --project Host`
can use the source checkout data layout.

### Customization catalogs

AC15 eras compose customization names from generated era catalogs plus shared or
override name JSON. The shared name files live under
`wwwroot/data/shared/`:

- `costume_name_data.json`
- `title_name_data.json`
- `neiro_name_data.json`

Set `ServerSettings:Eras:<Era>:CustomizationNameDataPath` to an override
directory when an era needs different display names. If the setting is blank,
the shared name files are used.

With `ServerSettings:Eras:<Era>:AutoExtractCatalog` enabled, the server can
bootstrap missing generated AC15 customization catalogs from the configured
game-data path on first startup.

Green can also be extracted manually:

```powershell
dotnet run --project GreenCatalogExtractor -- extract --game-data Host/wwwroot/data/green/data --out Host/wwwroot/data/green
```

The extractor reads the game-data tree and writes only generated JSON files
under `wwwroot/data/green`. It does not modify the operator's game-data tree.
The server and WebUI do not need further changes when generated JSON gains
better names or slot types.

### AC15 optional JSON

AC15 optional sidecar file names are era-specific:

| Feature | Green file | Blue file | Yellow file | Red file | White file | Murasaki file | Missing-file behavior |
|---------|------------|-----------|-------------|----------|------------|---------------|-----------------------|
| Telops | `telop_data.json` | `blue_telop_data.json` | `yellow_telop_data.json` | `red_telop_data.json` | `white_telop_data.json` | `murasaki_telop_data.json` | No telops |
| Attract movies | `movie_data.json` | `blue_movie_data.json` | `yellow_movie_data.json` | `red_movie_data.json` | `white_movie_data.json` | `murasaki_movie_data.json` | Auto-discover nonzero `attract_cm_###.pam` files |
| Event folders | `green_event_folder_data.json` | `blue_event_folder_data.json` | `yellow_event_folder_data.json` | `red_event_folder_data.json` | `white_event_folder_data.json` | `murasaki_event_folder_data.json` | No event folders |
| Taikojuku verup | `green_taikojuku_verup_data.json` | `blue_taikojuku_verup_data.json` | `yellow_taikojuku_verup_data.json` | `red_taikojuku_verup_data.json` | `white_taikojuku_verup_data.json` | `murasaki_taikojuku_verup_data.json` | XML-loaded Taikojuku packs keep `verup_no = 0` |
| Item shop | `green_item_shop_data.json` | `blue_item_shop_data.json` | `yellow_item_shop_data.json` | none | none | none | Required only when shop is enabled |
| Don Challenge | none | none | none | `red_don_challenge_data.json` | `white_don_challenge_data.json` | none | Disabled unless enabled with an active bundle |

Taikojuku packs come from `musicmedleyinfo.xml`, but that original AC15 file
does not carry the protocol `verup_no`. Use the era sidecar to set one default
for all packs and optional overrides by Taikojuku `challengeLevel`:

```json
{
  "defaultVerupNo": 1,
  "packs": [
    { "challengeLevel": 21, "verupNo": 2 }
  ]
}
```

AC15 movie files use the same object shape:


```json
{
  "override_default": false,
  "movies": []
}
```

When `override_default` is `false`, the server ignores `movies` and sends all
discovered nonzero movie IDs with `enable_days = 999`. When
`override_default` is `true`, the server sends only the listed movies. An empty
`movies` array disables attract movies for that era.

Example override:

```json
{
  "override_default": true,
  "movies": [
    { "movie_id": 100, "enable_days": 999 },
    { "movie_id": 102, "enable_days": 999 }
  ]
}
```

ID `0` is ignored because `attract_cm_000.pam` is a fallback asset, not a
startup permission candidate. Duplicate nonzero IDs are rejected at startup.

### AC15 item shop

Green, Blue, and Yellow item-shop support is controlled by
`Configurations/ServerSettings.json`:

```json
{
  "ServerSettings": {
    "Eras": {
      "Green": {
        "EnableShop": true,
        "ActiveShopSeasonId": 2
      },
      "Blue": {
        "EnableShop": true,
        "ActiveShopSeasonId": 1
      },
      "Yellow": {
        "EnableShop": true,
        "ActiveShopSeasonId": 4
      }
    }
  }
}
```

When Green, Blue, or Yellow is enabled, `EnableShop` must be present or startup
fails options validation. When `EnableShop` is `false`, the server does not
advertise the item shop for that era. When `EnableShop` is `true`,
`ActiveShopSeasonId` must be present and match a season in the era's item-shop
JSON file.

The shop data file stores protocol data only:

```json
{
  "seasons": [
    {
      "season_id": 1,
      "verup_no": 1,
      "telop": "Spring reward shop",
      "start_datetime": "20190314000000",
      "end_datetime": "20190626075959",
      "afterstart_days": 7,
      "beforeclose_days": 7,
      "items": [
        { "item_type": 4, "item_id": 117, "item_price": 500 }
      ]
    }
  ]
}
```

`item_no` is inferred from 1-based row order. `item_type` accepts the shared
AC15 string enum form and numeric compatibility values at the loader edge.
Item rows must not contain names or source metadata.

The current Green implementation still provides a small deterministic starter
state for new users. New Green users receive starter song flags, best scores,
crowns, and the first Dan on first known-card login after registration.

### AC15 Don Challenge

Red and White Don Challenge support is controlled by
`Configurations/ServerSettings.json`:

```json
{
  "ServerSettings": {
    "Eras": {
      "Red": {
        "EnableDonChallenge": true,
        "ActiveDonChallengeBundleId": "red-2016-08"
      },
      "White": {
        "EnableDonChallenge": true,
        "ActiveDonChallengeBundleId": "white-2016-06"
      }
    }
  }
}
```

When `EnableDonChallenge` is `true`, `ActiveDonChallengeBundleId` must be set
and match a bundle in the era's Don Challenge JSON file. Don Challenge progress
is server-side and stage-derived from normal playresult stages; it is exposed to
operators through dedicated AdminApi/WebUI contracts. It is not a standalone
cabinet ChallengeCompe route/readback model.

### Blue battle and Tokkun

Blue adds these setup notes on top of the shared AC15 flow:

- Blue game endpoints are direct protobuf under `/v10r03/chassis/*`.
- Shared AC15 startup/version endpoints remain under `/v01r00/chassis/*`.
- `getbanacoininfo.php`, `banacoinpayment.php`, and `banacoinerrorlog.php`
  are stateless compatibility routes. They do not model real Banacoin wallet
  or payment state.
- Tokkun uploads are accepted through Blue `playresult.php` using
  `PlayMode.Tokkun = 3`. The server persists only protocol-backed Tokkun
  tutorial/history facts and reads back `tokkun_tutorial_flg` through
  `userdata.php`.
- Tokkun playresults must not write normal Blue score, crown, Dani, profile,
  favorite, recent, normal unlock, battle, or shop state.

Blue battle availability is data-driven. Keep the complete battle XML folder at
`wwwroot/data/blue/data/config/S10100-1/battle`:

- `battleadjsetting.xml`
- `battlenpcinfo.xml`
- `battlestageinfo.xml`
- `battlesupportinfo.xml`
- `battletokeninfo.xml`

If the battle XML set is missing or malformed, normal Blue data can still load
but battle availability remains off. Blue battle state is persisted in
Blue-owned database tables and read back through `battleuserdata.php`;
unresolved battle reward, token, boss, and stage-graph semantics remain
store-and-echo rather than server-calculated behavior.

### White final and legacy routes

White has two game-route families:

- `/v07r03/chassis/*` is the final-version route family and uses
  `Adapters.GameProtocol.White.Wire`.
- `/v07r00/chassis/*` is the legacy compatibility route family and uses
  `Adapters.GameProtocol.White.LegacyWire`.

Do not share generated DTOs or final-only fields across the two route families.
Final White support includes proven Tokkun, stateless Banacoin-adjacent routes,
difficulty panel state, and heartbeat Banacoin status fields. Legacy White
compatibility keeps the older wire shape.

### Murasaki routes and split metadata

Murasaki game endpoints are direct protobuf under `/v06r00/chassis/*`.
Shared AC15 startup/version endpoints remain under `/v01r00/chassis/*`.

Murasaki does not expose a White-style `initialdatacheck.php` route. Startup
metadata is split across Murasaki-owned route files such as `defaultsong.php`,
`mainichisong.php`, `foldercheck.php`, `getfolder.php`, `telopcheck.php`, and
`gettelop.php`.

Special or older-version surfaces such as `bestscore.php`, `songhash.php`,
`shoppingresult.php`, challenge arrays, `content_info`,
`default_option_setting`, and reserved byte payloads remain evidence-gated.

## Datatable documentation

The server sends a variety of information to the game that you can edit.  
You'll find a list of all the files bellow. Nijiiro-specific JSON files live
under `wwwroot/data/nijiiro/`; shared files live under `wwwroot/data/shared/`.

The documentation is updated to reflect 39.06 files but most of it is similar for CHN.

### dan_data.json

This is used to customize normal dans.
  
```json
[
  {
    "danId":1, // The danId of the dan, has to be unique in all dans in dan_data.json
    "verupNo":1, // Used to control whether the client should update to a new dan when offline cache files are still present
    "title":"5kyuu", // Title of the dan, for example, "5kyuu" = ５級, "9dan" = 九段, "14dan" = 達人, etc.
    "aryOdaiSong":[
      {
        "songNo":420, // The uniqueId of the first song
        "level":2, // The level of the first song, 1 = easy, 4 = oni, 5 = ura, etc.
        "isHiddenSongName":false // If set to true, the song name will be displayed as ??? in dani selection in-game
      },
      {
        "songNo":881, // The uniqueId of the second song
        "level":2,
        "isHiddenSongName":false
      },
      {
        "songNo":995, // The uniqueId of the third song
        "level":2,
        "isHiddenSongName":false
      }
    ],
    "aryOdaiBorder":[
      {
        "odaiType":1, // The odai type, 1 = soul gauge percentage, 2 = good count, 3 = ok count, 4 = bad count, 5 = combo count, 6 = renda count, 7 = score, 8 = hit count
        "borderType":1, // Controls whether this odai requirement is shared, 1 means all 3 songs share this same odai requirement, 2 means 3 songs have separate odai requirements, to see how to set separate odai requirements, see the next dan example
        "redBorderTotal":92, // The odai requirement to get a red pass for this dan
        "goldBorderTotal":95 // The odai requirement to get a gold pass for this dan
      },
      {
        "odaiType":8,
        "borderType":1,
        "redBorderTotal":884,
        "goldBorderTotal":936
      }
    ]
  },
  {
    "danId":14,
    "verupNo":1,
    "title":"9dan",
    "aryOdaiSong":[
      {
        "songNo":568,
        "level":4,
        "isHiddenSongName":false
      },
      {
        "songNo":117,
        "level":4,
        "isHiddenSongName":false
      },
      {
        "songNo":21,
        "level":4,
        "isHiddenSongName":false
      }
    ],
    "aryOdaiBorder":[
      {
        "odaiType":1,
        "borderType":1,
        "redBorderTotal":100,
        "goldBorderTotal":100
      },
      {
        "odaiType":2,
        "borderType":1,
        "redBorderTotal":2045,
        "goldBorderTotal":2100
      },
      {
        "odaiType":4,
        "borderType":1,
        "redBorderTotal":10,
        "goldBorderTotal":5
      },
      {
        "odaiType":6, // This is set to 6, which means this is the renda requirement odai
        "borderType":2, // This is set to 2, which means the 3 songs have individual odai requirements
        "redBorder_1":107, // This means to get a red pass, you have to get above or equal to 107 rendas in song 1
        "goldBorder_1":114, // This means to get a gold pass, you have to get above or equal to 114 rendas in song 1
        "redBorder_2":74, // This means to get a red pass, you have to get above or equal to 74 rendas in song 2
        "goldBorder_2":79, // This means to get a gold pass, you have to get above or equal to 79 rendas in song 2
        "redBorder_3":54, // This means to get a red pass, you have to get above or equal to 54 rendas in song 3
        "goldBorder_3":59 // This means to get a gold pass, you have to get above or equal to 59 rendas in song 3
      }
    ]
  }
]
```

### event_folder_data.json

This is used to populate event folders/genres

```json
[
    {
        "folderId": 1, // The folderId of the event folder
        //For 39.06, the list is the following:
        //1: Touhou Project Special
        //2: The Idolmaster Special
        //3: Highly Recommended Songs
        //4 -- UNUSED --
        //5: Studio Ghibli Feature
        //6: Yokai Watch Special
        //7: UUUM Creator Feature
        //8: Soshina's Playlist
        //9: Soshina's Recommended Playlist
        //10: Championship Songs
        //11: [Bonus] 2023 Championship Songs
        //12: #Compass Creator Feature
        //13: Winter Seasonal Songs Pack
        //14: World Popular Songs
        //15: Taiko no Tatsujin 20th Anniversary Songs
        //You can find the corresponding folderId in the wordlist by searching for keys called folder_eventX, where X is the folderId
        "verupNo": 1, // Used to control whether the client should update to a new event folder when offline cache files are still present
        "priority": 1, 
        "songNo": [] // The uniqueId of the songs to be added to this event folder, if left empty, this folder will not show up in-game
    },
    {
        "folderId": 2,
        "verupNo": 1,
        "priority": 1,
        "songNo": [
            478, 153, 200, 482, 511, 672, 675, 646, 644, 645, 676, 671, 479,
            707, 480, 481, 203, 204, 483, 205, 202, 241, 14, 387, 197, 281, 226,
            484, 543, 512, 709, 35
        ] // A populated event folder example
    }
]
```

### gaiden_data.json

This is used to customize gaiden dans.

```json
[
  {
    "danId":20, // The danId of the gaiden dan, can be the same value as a dan in dan_data.json, but has to be unique in all gaidens in gaiden_data.json
    "verupNo":1, // Used to control whether the client should update to a new dan when offline cache files are still present
    "title":"[JPN]=復活！ブルー十段,[ENG]=Blue 10Dan", // The title of the gaiden dan, which will be displayed when scanning the QR code and in dani select interface. Use language code to specify each language's entry. [JPN], [ENG], [CHN], [KOR], [CHS] are supported. Use comma to separate each language's entry.
    "aryOdaiSong":[ // Starting from here, it uses the same format as dan_data.json
      {
        "songNo":60,
        "level":5,
        "isHiddenSongName":false
      },
      {
        "songNo":55,
        "level":4,
        "isHiddenSongName":false
      },
      {
        "songNo":737,
        "level":4,
        "isHiddenSongName":false
      }
    ],
    "aryOdaiBorder":[
      {
        "odaiType":1,
        "borderType":1,
        "redBorderTotal":100,
        "goldBorderTotal":100
      },
      {
        "odaiType":3,
        "borderType":1,
        "redBorderTotal":90,
        "goldBorderTotal":60
      },
      {
        "odaiType":4,
        "borderType":1,
        "redBorderTotal":8,
        "goldBorderTotal":5
      }
    ]
  }
]
```

### intro_data.json

This is used to customize the song intro displayed before entering the game

```json
[
  {
    "setId":1, // The setId of the intro, has to be unique in all intros in intro_data.json
    "verupNo":1, // Used to control whether the client should update to a new intro when offline cache files are still present
    "mainSongNo":1115, // The uniqueId of the main song, which will be displayed at the top of the four other songs
    "subSongNo":[1022,7,1089,1059] // The uniqueId of the four other songs, which will be displayed below the main song, there has to be 4 songs exactly
  },
  {
    "setId":2,
    "verupNo":1,
    "mainSongNo":1102,
    "subSongNo":[1065,966,1008,916]
  }
]
```

### locked_songs_data.json

This is used to customize locked songs.

```json
{
  "songNo": [
    // Fill in the uniqueId of songs you wish to lock
    // Songs locked will not be visible in-game to players(including guest) without the corresponding entry in UnlockedSongIdList in UserData, but may still show up in the shop folder
    100,
    200,
    300
  ],
  "uraSongNo": [
    // Fill in the uniqueId of songs whose ura chart you wish to lock
  ]
}
```

### movie_data.json

For Nijiiro, this array controls which in-game movie is displayed before entering the game.
AC15 era movie files use the `override_default` object format documented in
[AC15 optional JSON](#ac15-optional-json).

```json
[
  {
    "movie_id": 0, // The movie id can be the following: 
    //1 = #Compass collab
    //2 = Idolish 7
    //3 = Touhou collab 2020,
    //4 = DENONBU collab
    //7 = DENONBU BuSho Film Festival
    //8 = iM@S 15th anniversary collab, 
    //9 = iM@S 15th anniversary collab (en), 
    //10 = ONE PIECE collab, 
    //12 = ONE PIECE collab 2 (special mode here), 
    //14 = Touhou collab 2021,
    //15 = Taiko no Tatsujin 20th anniversary Soshina collab, 
    //16 = Taiko no Tatsujin 20th anniversary Soshina collab (en), 
    //17 = Taiko no Tatsujin 20th anniversary Soshina collab (zh-tw), 
    //18 = Taiko no Tatsujin 20th anniversary Soshina collab (ko)
    //19 = Touhou collab 2021,
    //20 = Hololive collab
    //You can find the updated list for your game dump in the \Data\x64\movie folder, where the file names are the movie_ids.
    "enable_days": 0 // Simply set to 999 for the movie to always be displayed
  }
]
```

### qrcode_data.json

This is used to customize which qrcode's uniqueId is invoked when a qrcode request is received.

```json
[
  {
    "serial": "gaiden_blue_10dan", // QR serial data sent by TAL
    "id": 20 // The uniqueId of the qrcode the server sends back, corresponding to the uniqueId in qrcode_info.bin in the client's datatable
  },
  {
    "serial": "gaiden_white_10dan",
    "id": 21
  }
]
```

### shop_folder_data.json

This is used to customize the in-game shop folder content.

```json
[
  {
    "songNo": 100, // The uniqueId of the song
    "type": 0, // 0 indicates the shop is selling the song itself
    "price": 20 // How many don coins buying this song costs, the type of don coin is specified by token_data.json
  },
  {
    "songNo": 200,
    "type": 1, // And 1 indicates the shop is selling the song's ura chart
    "price": 20
  }
]
```

### token_data.json

This is used to customize in-game reward tokens.

```json
{
  "shopTokenId": -1, // The token id used in shop, a.k.a. don coin, can be from 1 to 11, 1=spring, 2=summer, 3=autumn, 4=winter, 5=spring(again), etc. By default, this is turned off by setting it to -1
  "kaTokenId": -1, // The token id of ka coins, can be 1000 or 1001, corresponding to reward entrys in reward.bin in the client's datatable. By default, this is turned off by setting it to -1
  "onePieceTokenId": 100100, // The token id representing onePiece collab mode's win count, should not be changed
  "soshinaTokenId": 100200 // The token id representing soshina collab mode's win count, should not be changed
}
```
