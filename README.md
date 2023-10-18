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

- dan_data.json: This is used customize normal dans. TODO: Details
- gaiden_data.json: This is used to customize gaiden dans.
```json
[
  {
    "danId":20, // The danId of the gaiden dan, can be the same value as normal dans, but has to be unique in all gaidens
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
- locked_songs_data.json: This is used to customize locked songs.
```json
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
- qrcode_data.json: This is used to customize which qrcode's uniqueId is invoked when a qrcode request is received.
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
- shop_folder_data.json: This is used to customize the in-game shop folder content.
```json
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
```json
{
  "shopTokenId": -1, // The token id used in shop, a.k.a. don coin, can be from 1 to 11, 1=spring, 2=summer, 3=autumn, 4=winter, 5=spring(again), etc. By default, this is turned off by setting it to -1
  "kaTokenId": -1, // The token id of ka coins, can be 1000 or 1001, corresponding to reward entrys in reward.bin in the client's datatable. By default, this is turned off by setting it to -1
  "onePieceTokenId": 100100, // The token id representing onePiece collab mode's win count, should not be changed
  "soshinaTokenId": 100200 // The token id representing soshina collab mode's win count, should not be changed
}
```