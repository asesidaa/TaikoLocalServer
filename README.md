# Taiko Local Server

This is a server for Taiko no Tatsujin Nijiiro ver CHN

## Setup

### Prerequisite

- A working game, with dongle and QR reader emulation. You can use [TaikoArcadeLoader](https://github.com/BroGamer4256/TaikoArcadeLoader) for these if you haven't.

### Setup Steps

1. Extract the server release anywhere

2. From the game files, copy `music_order.bin`, `musicinfo.bin`, `wordlist.bin`, `don_cos_reward.bin`, `shougou.bin` to `wwwroot/data` folder.

3. (Optional) In `Certificates` folder, import `root.pfx` to trusted root store, `cert.pfx` to personal store. All the other import options can be kept default.

4. Visit http://localhost:5000 and (Optional, only if you have done step 4) https://localhost:10122, if  the web ui starts without errors, the config is fine.
   
5. Now the game should be able to connect.

## Data config

There are various data json files under wwwroot/data that can be customized.

- dan_data.json: This is used customize normal dans. TODO: Details
- TODO: More details on the files
