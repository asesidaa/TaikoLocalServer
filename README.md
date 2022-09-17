# Taiko Local Server

This is a server for Taiko no Tatsujin Nijiiro ver 08.18

## Setup

### Prerequisite

- A working game, with dongle and card reader emulation. You can use [TaikoArcadeLoader](https://github.com/BroGamer4256/TaikoArcadeLoader) for these if you haven't.

### Tools

- [TaikoArcadeLoader](https://github.com/BroGamer4256/TaikoArcadeLoader): A loader for the game with hardware emulation and other fixes.
- [TaikoReverseProxy](https://github.com/shiibe/TaikoReverseProxy): A no-setup bundled proxy server, use as a user-friendly alternative to Apache.

### Quick Setup

With the newest release (>=2.00) of TaikoArcadeLoader, you no longer need to run AMAuthd or AMUpdater.

1. Download the latest release of [TaikoArcadeLoader](https://github.com/BroGamer4256/TaikoArcadeLoader) and install it. Make sure to setup TAL using the config.toml and set the `server` parameter to your local IP address. 
2. Download the latest release of [TaikoReverseProxy](https://github.com/shiibe/TaikoReverseProxy).
3. In the `Data\x64\datatable` folder of the game, find the following files:

    ```
    music_attribute.bin
    musicinfo.bin
    music_order.bin
    wordlist.bin
    ```

    Extract them (you can use [7zip](https://www.7-zip.org)) and rename the extracted file like so:

    ```
    music_attribute -> music_attribute.json
    musicinfo       -> musicinfo.json
    music_order     -> music_order.json
    wordlist        -> wordlist.json
    ```

    Then put these in TaikoLocalServer's `wwwroot/data` folder.
    

4. Modify hosts, add the following entries (this step can be done automatically with TaikoReverseProxy, check the config for it):

   ```
   server.ip      tenporouter.loc
   server.ip      naominet.jp
   server.ip      v402-front.mucha-prd.nbgi-amnet.jp
   server.ip      vsapi.taiko-p.jp
   ```

   where `server.ip` is your computer's ip (or the server's ip)

5. Run TaikoReverseProxy and TaikoLocalServer, then run the game. You can access the WebUI by going to `https://naominet.jp:10122/` in your browser.


### Server setup (for TAL<2.00 or other loaders)

1. Download the server from release page, extract anywhere

2. From game's `Data\x64\datatable` folder, find `music_attribute.bin`, `musicinfo.bin`, `music_order.bin` and `wordlist.bin`, decompress them, add `.json` prefix to them.
   The result is `music_attribute.json`, `musicinfo.json`, `music_order.json` and `wordlist.json`. Put the json files under` wwwroot/data` folder in server.

3. Modify hosts, add the following entries:

   ```
   server.ip      tenporouter.loc
   server.ip      naominet.jp
   server.ip      v402-front.mucha-prd.nbgi-amnet.jp
   server.ip      vsapi.taiko-p.jp
   ```

   where `server.ip` is your computers ip (or the server's ip)

4. Setup [TaikoReverseProxy](https://github.com/shiibe/TaikoReverseProxy) or [Apache](#apache-setup-optional) as reverse proxy.

5. Now run the server, if everything is setup correctly, visit http://localhost:5000, you should be able to see the web ui up and running without errors. (If you encounter errors in web ui for the first time, try visit https://naominet.jp:10122/)

6. Go to game folder, copy the config files (AMConfig.ini and WritableConfig.ini) in the AMCUS folder from server release to AMCUS folder and replace the original ones.

7. Open command prompt as admin, navigate to game root folder (where init.ps1 is). Run `regsvr32 .\AMCUS\iauthdll.dll`. It should prompt about success.

8. Run AMCUS/AMAuthd.exe, then run AMCUS/AMUpdater.exe. If the updater run and exits without issue, you are ready to run the game and connect to server.

9. Run the game, it should now connect to the server.

### Run the server on another computer

If you want to run the server on another computer, the procedure is almost identical. 

Before you open browser, in `wwwroot/appsettings.json`, change `BaseUrl` to `https://naominet.jp:10122` then instead of visit localhost, visit the server using domain name to test.

Also note that now the cetificate also need to be imported on client computer, or web ui may not work. If you don't need https, change `BaseUrl` to `http://server.ip:80`, and visit on client. The game does not care about certificate.

### Apache Setup (Optional)
Notice the following assumes a windows install, the server also works on Linux, but the guide only covers windows.

1. Download [Apache](https://www.apachelounge.com/download/), extract anywhere

2. Copy the content in release rar's Apache folder to installed Apache root folder (and replace, which includes httpd.conf and httpd-vhosts.conf, if no prompt to replace files, you are extracting to wrong folder)

3. Open `conf/httpd.conf` (under installed Apache folder), find this line (line 37 by default), modify it to your Apache install (extracted) full path

```htaccess
# For example, if your Apache is extracted to C:\users\username\Apache24, then this should be "c:/users/username/Apache24"
Define SRVROOT "d:/Projects/Apache24"
```

4. Open the certs folder Apache root folder, then click on the localhost.crt file and import it to trusted root store.

If everything is correct, run bin/httpd.exe, a command prompt will open (and stay open, if it shut down, probably something is not setup correctly)
