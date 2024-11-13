# Taiko Local Server

This is the solution for server.  
Server is implemented with ASP.NET Core 8. ORM is Entity Framework Core 8. Database is SQLite for easier setup.  
As the game uses protobuf, `protobuf-net` is used for serializing and deserializing the data.  

- [Taiko Local Server](#taiko-local-server)
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

## Datatable documentation

The server sends a variety of information to the game that you can edit.  
You'll find a list of all the files bellow!

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

This is used to control which in-game movie is displayed before entering the game

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
