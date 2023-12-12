# Taiko Local Server

This is a server for Taiko no Tatsujin Nijiiro ver CHN

## Setup

### Prerequisite

- A working game, with dongle and QR reader emulation. You can use [TaikoArcadeLoader](https://github.com/BroGamer4256/TaikoArcadeLoader) for these if you haven't.

### Setup Steps

1. Extract the server release anywhere

2. From the game files, copy `music_order.bin`, `musicinfo.bin`, `wordlist.bin`, `don_cos_reward.bin`, `shougou.bin`,`neiro.bin` to `wwwroot/data/datatable` folder.

3. (Optional) In `Certificates` folder, import `root.pfx` to trusted root store, `cert.pfx` to personal store. All the other import options can be kept default.

4. Visit http://localhost:5000 if the web ui starts without errors, the config is fine.
   
5. Now start the game and it should be able to connect.

## Data config

There are various data json files under wwwroot/data that can be customized.

- dan_data.json: This is used customize normal dans.
```
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
- event_folder_data.json: This is used to populate event folders/genres
```
[
    {
        "folderId": 1, // The folderId of the event folder, find corresponding folderId in wordlist.bin by searching keys called folder_eventX, where X is the folderId
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
- gaiden_data.json: This is used to customize gaiden dans.
```
[
  {
    "danId":20, // The danId of the gaiden dan, can be the same value as a dan in dan_data.json, but has to be unique in all gaidens in gaiden_data.json
    "verupNo":1, // Used to control whether the client should update to a new dan when offline cache files are still present
    "title":"[JPN]=復活！ブルー十段,[ENG]=Blue 10Dan", // The title of the gaiden dan, which will be displayed when scanning the QR code and in dani select interface. Use language code to specify each language's entry. [JPN], [CHS], [CHT], [KOR], [ENG] are supported. Use comma to separate each language's entry.
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
- intro_data.json: This is used to customize the song intro displayed before entering the game
```
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
- locked_songs_data.json: This is used to customize locked songs.
```
{
  "songNo": [
    // Fill in the uniqueId of songs you wish to lock
    // Songs locked will not be visible in-game to players(including guest) without the corresponding entry in UnlockedSongIdList in UserData, but may still show up in the shop folder
    100,
    200,
    300
  ]
}
```
- movie_data.json: This is used to control which in-game movie is displayed before entering the game
```
[
  {
    "movie_id": 0, // The movie id, 8 = iM@S 15th anniversary collab, 9 = iM@S 15th anniversary collab (en), 10 = ONE PIECE collab, 12 = ONE PIECE collab 2 (special mode here), 14 = Touhou collab 2021,15 = Taiko no Tatsujin 20th anniversary Soshina collab, 16 = Taiko no Tatsujin 20th anniversary Soshina collab (en), 17 = Taiko no Tatsujin 20th anniversary Soshina collab (zh-tw), 18 = Taiko no Tatsujin 20th anniversary Soshina collab (ko)
    "enable_days": 0 // Simply set to 999 for the movie to always be displayed
  }
]
```
- qrcode_data.json: This is used to customize which qrcode's uniqueId is invoked when a qrcode request is received.
```
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
- shop_folder_data.json: This is used to customize the in-game shop folder content.
```
[
  {
    "songNo": 100, // The uniqueId of the song
    "price": 20 // How many don coins buying this song costs, the type of don coin is specified by token_data.json
  },
  {
    "songNo": 200,
    "price": 20
  }
]
```
- token_data.json: This is used to customize in-game reward tokens.
```
{
  "shopTokenId": -1, // The token id used in shop, a.k.a. don coin, can be from 1 to 11, 1=spring, 2=summer, 3=autumn, 4=winter, 5=spring(again), etc. By default, this is turned off by setting it to -1
  "kaTokenId": -1, // The token id of ka coins, can be 1000 or 1001, corresponding to reward entrys in reward.bin in the client's datatable. By default, this is turned off by setting it to -1
  "onePieceTokenId": 100100, // The token id representing onePiece collab mode's win count, should not be changed
  "soshinaTokenId": 100200 // The token id representing soshina collab mode's win count, should not be changed
}
```

## TaikoWebUI appsettings.json config
This section is for configuring the TaikoWebUI appsettings.json file found under the ```wwwroot``` folder.
This file is used to configure the web UI.
```
{
  "WebUiSettings": {
    "LoginRequired": "false", // Whether a login is required to access personal profiles, default to false
    "AdminUserName": "admin", // The username of the admin account, if LoginRequired is set to false, this is ignored, admin account can always access all personal profiles
    "AdminPassword": "admin", // The password of the admin account, if LoginRequired is set to false, this is ignored
    "OnlyAdmin": "false" // Whether only the admin account can access personal profiles, if set to true, register will be unavailable and only admins can login, default to false
  }
}
```