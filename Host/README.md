# Taiko Local Server

This is the solution for server.  
Server is implemented with ASP.NET Core 10. ORM is Entity Framework Core 10. Database is SQLite for easier setup.  
As the game uses protobuf, `protobuf-net` is used for serializing and deserializing the data.  

- [Taiko Local Server](#taiko-local-server)
  - [Data file layout (per-era)](#data-file-layout-per-era)
  - [Green AC15 Test Support](#green-ac15-test-support)
    - [Green game data symlink](#green-game-data-symlink)
    - [Green customization catalogs](#green-customization-catalogs)
    - [Green attract movies](#green-attract-movies)
    - [Green Cabinet Smoke Checklist](#green-cabinet-smoke-checklist)
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

`wwwroot/data/` is partitioned by game era:

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
|-- green/                      Green-era AC15 data
|   |-- recommend_songs.json    Operator-edited Green pushed/recommended songs
|   |-- movie_data.json         Green attract movie permissions (default discovery or explicit override)
|   |-- green_costume_data.json Generated Green customization catalog
|   |-- green_title_data.json   Generated Green title catalog
|   |-- green_neiro_data.json   Generated Green tone catalog
|   `-- data/                   Green game USRDIR/data tree, often symlinked
|       |-- config/S11100-1/
|       |   |-- musicinfo.xml
|       |   `-- musicmedleyinfo.xml
|       `-- fumen/
|           `-- tuning.bin
`-- shared/                     Cross-era operator-edited tables
    |-- token_data.json
    `-- qrcode_data.json
```

Era availability is controlled by `Configurations/ServerSettings.json` under
`ServerSettings:Eras`. Nijiiro is enabled by default. To allow Green cabinet
routes, set `ServerSettings:Eras:Green:Enabled` to `true`.

## Green AC15 Test Support

When `ServerSettings:Eras:Green:Enabled` is `true`, the server requires:

- `wwwroot/data/green/data/config/S11100-1/musicinfo.xml`
- `wwwroot/data/green/data/config/S11100-1/musicmedleyinfo.xml`
- `wwwroot/data/green/data/fumen/tuning.bin`

### Green game data symlink

For Green, the server reads files from the original game `USRDIR/data` layout. Instead of copying that whole folder into the repository, you can symlink it to `wwwroot/data/green/data`.

From a source checkout, run this from the repository root:

```powershell
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\Host\wwwroot\data\green\data'
```

From an extracted release folder, run this from the folder containing `TaikoLocalServer.exe`:

```powershell
New-Item -ItemType SymbolicLink -Target 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data\' -Path '.\wwwroot\data\green\data'
```

If you prefer copying for a release instead of linking:

```powershell
New-Item -ItemType Directory -Force -Path '.\wwwroot\data\green' | Out-Null
Copy-Item -Recurse -Path 'path\to\rpcs3\dev_hdd0\game\SCEEXE001\USRDIR\data' -Destination '.\wwwroot\data\green\data'
```

PowerShell may need to run as Administrator, unless Windows Developer Mode allows unprivileged symlink creation. The symlink target should contain `config\S11100-1\musicinfo.xml`, `config\S11100-1\musicmedleyinfo.xml`, and `fumen\tuning.bin`.

### Green customization catalogs

Green costume, title, and tone catalogs are generated from the symlinked or copied `USRDIR/data` tree. With `ServerSettings:Eras:Green:AutoExtractCatalog` enabled, the server runs the Phase 1 extractor on first startup when `green_costume_data.json`, `green_title_data.json`, or `green_neiro_data.json` is missing. To run it manually:

```powershell
dotnet run --project GreenCatalogExtractor -- extract --game-data Host/wwwroot/data/green/data --out Host/wwwroot/data/green
```

For slot mapping enrichment, run the IDA-backed stage in `docs/superpowers/plans/2026-05-17-green-customization/07-ida-slot-mapping-enrichment.md` against `.tools/ida-snap/EBOOT.ELF.codex.i64`, then rerun the extractor. The server and WebUI do not need further changes when the generated JSON gains better names or slot types.

The extractor reads the game-data tree and writes only the generated JSON files under `wwwroot/data/green`. It does not modify the operator's game-data tree. Names may be blank on Phase 1 output; the WebUI displays id labels such as `#004` until an enriched catalog is generated.

The current Green implementation intentionally unlocks a small deterministic song set for cabinet validation:

- no-card/default song flags: first 10 `uniqueid` values from `musicinfo.xml`
- logged-in user release flags: first 20 `uniqueid` values from `musicinfo.xml`

New Green users also receive deterministic fake best scores/crowns and receive the first fake Dan on their first known-card login after registration. This is test scaffolding for verifying Green bitset, self-best, crown, and Dan response formats.

### Green attract movies

Green startup auth sends attract movie permissions through `ary_movie_info`.
By default, Green auto-enables every nonzero `attract_cm_###.pam` file found
under `wwwroot/data/green/data/movie`.

`wwwroot/data/green/movie_data.json` controls whether discovery is used or
replaced:

```json
{
  "override_default": false,
  "movies": []
}
```

When `override_default` is `false`, the server ignores `movies` and sends all
discovered nonzero movie IDs with `enable_days = 999`. When
`override_default` is `true`, the server sends only the listed movies. An empty
`movies` array disables Green attract movies.

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

ID `0` is ignored because Green treats `attract_cm_000.pam` as a fallback
asset, not a startup permission candidate. Duplicate nonzero IDs are rejected
at startup.

### Green Cabinet Smoke Checklist

1. Start server with `ServerSettings:Eras:Green:Enabled = true`.
2. Boot cabinet and reach song select without card. Confirm 10 songs are visible.
3. Register a card through `mydonentry`.
4. Log in with that card. Confirm 20 songs are visible.
5. Confirm fake self-best/crown data appears for the seeded songs.
6. Log out and log in again. Confirm first fake Dan appears.
7. Change gameplay options, play a song, and log in again. Confirm option state did not reset.
8. Play a song and confirm `selfbest` and `crownsdata` reflect the upload.
9. Enter any ghost/AI-battle-like flow available on the cabinet. Confirm it does not crash and request logs show bounded byte previews.

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
Green uses `wwwroot/data/green/movie_data.json` with the `override_default`
object format documented in [Green attract movies](#green-attract-movies).

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
