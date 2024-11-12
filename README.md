# Taiko Local Server

This is a server for Taiko no Tatsujin Nijiiro ver 39.06  
It is composed of two major components :

- [TaikoLocalServer](./TaikoLocalServer/): The server handling the game's requests
- [TaikoWebUI](./TaikoWebUI/): The frontend handling user profiles.

## Installation

### Prerequisite

- You need a working game install, with dongle and QR reader emulation.  
  You can use [TaikoArcadeLoader](https://github.com/esuo1198/TaikoArcadeLoader) to have these working (Teknoparrot will NOT work).

### Setup Steps

1. Extract the Server's release anywhere
2. From the game files (Data/x64/datatable), copy `music_order.bin`, `musicinfo.bin`, `wordlist.bin`, `don_cos_reward.bin`, `shougou.bin`,`neiro.bin` to [taikolocalserver/wwwroot/data/datatable](./TaikoLocalServer/wwwroot/data/datatable/)
3. (Optional) In `Certificates` folder, import `root.pfx` to trusted root store and `cert.pfx` to personal store. All the other import options can be kept default
4. Visit [http://localhost](http://localhost). If the WebUI starts without errors, the config is fine
5. Start your game! (First boot with the server will take a good minute, be patient!)

## Configuration

### TaikoLocalServer configuration

There are various json files under [taikolocalserver/wwwroot/data](./TaikoLocalServer/wwwroot/data/) that can be customized.  
Please refer to the [taikolocalserver readme file](./TaikoLocalServer/README.md) for documentation.

### TaikoWebUI configuration

The WebUI has a few settings you can change in [appsettings.json](./TaikoWebUI/wwwroot/appsettings.json)  
Please refer to the [taikowebui readme file](./TaikoWebUI/README.md) for documentation.
