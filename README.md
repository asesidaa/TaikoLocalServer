# Taiko Local Server

This is a server for Taiko no Tatsujin Nijiiro ver 08.18

## Setup

### Prerequisite

- A working game, with dongle and card reader emulation. You can use [TAL](https://github.com/BroGamer4256/TaikoArcadeLoader) for these if you haven't.

### Server setup

1. Download the server from release page, extract anywhere

2. From game's `Data\x64\datatable` folder, find `music_attribute.bin`, `musicinfo.bin`, `music_order.bin` and `wordlist.bin`, decompress them, add `.json` prefix to them.
   The result is `music_attribute.json`, `musicinfo.json`, `music_order.json` and `wordlist.json`. Put `music_attribute.json` under `wwwroot`, the others under` wwwroot/data` folder in server.

3. Modify hosts, add the following entries:

   ```
   server.ip      tenporouter.loc
   server.ip      naominet.jp
   server.ip      v402-front.mucha-prd.nbgi-amnet.jp
   server.ip      vsapi.taiko-p.jp
   ```

   where `server.ip` is your computers ip (or the server's ip)

4. Setup Apache as reverse proxy. Notice the following assumes a windows install, the server also works on Linux, but the guide only covers windows. 

   1. Download [Apache](https://www.apachelounge.com/download/), extract anywhere

   2. **Copy the content in Apache folder to installed Apache root folder (and replace)**

   3. Open `conf/httpd.conf`, find this line, modify it to your Apache install (extracted) full path

      ```htaccess
      # For example, if your Apache is extracted to C:\users\username\Apache24, then this should be "c:/users/username/Apache24"
      Define SRVROOT "d:/Projects/Apache24"
      ```

      

   4. Open the certs folder Apache root folder, then click on the localhost.crt file and import it to trusted root store.

      If everything is correct, run bin/httpd.exe, a command prompt will open (and stay open, if it shut down, probably something is not setup correctly)

5. Now run the server, if everything is setup correctly, visit http://localhost:5000, you should be able to see the web ui up and running without errors. (If you encounter errors in web ui for the first time, try visit https://naominet.jp:10122/)

6. Go to game folder, copy the config files (AMConfig.ini and WritableConfig.ini) in the AMCUS folder from server release to AMCUS folder and replace the original ones.

7. Open command prompt as admin, navigate to game root folder (where init.ps1 is). Run `regsvr32 .\AMCUS\iauthdll.dll`. It should prompt about success.

8. Run AMCUS/AMAuthd.exe, then run AMCUS/AMUpdater.exe. If the updater run and exits without issue, you are ready to run the game and connect to server.

9. Run the game, it should now connect to the server.

### Run the server on another computer

If you want to run the server on another computer, the procedure is almost identical. 

Before you open browser, in `wwwroot/appsettings.json`, change `BaseUrl` to `https://naominet.jp:10122` then instead of visit localhost, visit the server using domain name to test.

Also note that now the cetificate also need to be imported on client computer, or web ui may not work. If you don't need https, change `BaseUrl` to `http://server.ip:80`, and visit on client. The game does not care about certificate.

