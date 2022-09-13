# Taiko Local Server

This is a server for Taiko no Tatsujin Nijiiro ver 08.18

## Setup

### Prerequisite

- A working game, with dongle and card reader emulation. You can use [TAL](https://github.com/BroGamer4256/TaikoArcadeLoader) for these if you haven't.

### Server setup

1. Download the server from release page, extract anywhere

2. Modify hosts, add the following entries:

   ```
   server.ip      tenporouter.loc
   server.ip      naominet.jp
   server.ip      v402-front.mucha-prd.nbgi-amnet.jp
   server.ip      vsapi.taiko-p.jp
   ```

   where `server.ip` is your computers ip (or the server's ip)

3. Setup Apache as reverse proxy. Notice the following assumes a windows install, the server also works on Linux, but the guide only covers windows. 

   1. Download [Apache](https://www.apachelounge.com/download/), extract anywhere

   2. Copy the content in Apache folder to Apache root folder (and replace)

   5. Open the certs folder Apache root folder, then click on the localhost.crt file and import it to trusted root store.
   
      If everything is correct, run bin/httpd.exe, a command prompt will open (and stay open, if it shut down, probably something is not setup correctly)
   
4. Now run the server, if everything is setup correctly, visit http://localhost:5000, you should be able to see the web ui up and running without errors.

5. Go to game folder, copy the config files (AMConfig.ini and WritableConfig.ini) in the AMCUS folder from server release to AMCUS folder and replace the original ones.

6. Open command prompt as admin, navigate to game root folder (where init.ps1 is). Run `regsvr32 .\AMCUS\iauthdll.dll`. It should prompt about success.

7. Run AMCUS/AMAuthd.exe, then run AMCUS/AMUpdater.exe. If the updater run and exits without issue, you are ready to run the game and connect to server.

8. Run the game, it should now connect to the server.

