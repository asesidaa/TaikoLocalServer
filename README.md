# Taiko Local Server

This is a server for Taiko no Tatsujin Nijiiro ver 08.18

## Setup

### Prerequisite

- A working game, with dongle and card reader emulation. You can use [TaikoArcadeLoader](https://github.com/BroGamer4256/TaikoArcadeLoader) for these if you haven't.

### Quick Setup

With the newest release (from [this](https://github.com/BroGamer4256/TaikoArcadeLoader/tree/95d633850d89cb7099e98ffe74cd23632fe26e56) commit) of TaikoArcadeLoader, you no longer need to run AMAuthd, AMUpdater or reverse proxy.

1. Extract the server release anywhere

2. From the game files, copy `music_attribute.bin`, `music_order.bin`, `musicinfo.bin` and `wordlist.bin` to `wwwroot/data` folder.

3. (Optional) Instead of direct copy, extract the specified game files (using 7zip), rename them by adding the file extension `.json` and copy the jsons over.

4. (Optional) In `Certificates` folder, import `root.pfx` to trusted root store, `cert.pfx` to personal store. All the other import options can be kept default.

5. Visit http://localhost:5000 and (Optional, only if you have done step 4) https://localhost:10122, if  the web ui starts without errors, the config is fine.

6. Modify comfig.toml from TAL, edit the following line:

   ```toml
   server = "https://divamodarchive.com" # Change https://divamodarchive.com to your/server's ip, like 192.168.1.100
   ```
7. Open command prompt as admin, navigate to game root folder (where `init.ps1` is). Run `regsvr32 .\AMCUS\iauthdll.dll`. It should prompt about success (This only need to be done once)

8. Now the game should be able to connect.

### About certificates

If you want to change the certificate used by server, just replace `cert.pfx` in `Certificates` folder. The bundled certificate includes the following DNS names:

```
DNS Name=nbgi-amnet.jp
DNS Name=v402-front.mucha-prd.nbgi-amnet.jp
DNS Name=*.mucha-prd.nbgi-amnet.jp
DNS Name=localhost
DNS Name=vsapi.taiko-p.jp
```

You will need to modify hosts file to use them.
