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

   2. Modify conf/httpd.conf:

      ```htaccess
      # example: c:/Apache24
      Define SRVROOT "full:/path/to/Apache/folder" 
      
      # In loadmodule section, find the following lines and uncomment (remove the # at the beginning), they are by default commented out
      LoadModule proxy_module modules/mod_proxy.so
      LoadModule proxy_http_module modules/mod_proxy_http.so
      LoadModule ssl_module modules/mod_ssl.so
      LoadModule headers_module modules/mod_headers.so
      
      # Scroll down to bottom, find this line and uncomment
      Include conf/extra/httpd-vhosts.conf
      ```

   3. Modify conf/extra/httpd-vhosts.conf, add the following

      ```htaccess
      <VirtualHost *:80>
          ProxyPreserveHost On
          ProxyPass / http://127.0.0.1:5000/
          ProxyPassReverse / http://127.0.0.1:5000/
          ServerName naominet.jp
          RequestHeader set "X-Forwarded-Proto" expr=%{REQUEST_SCHEME}
      </VirtualHost>
      
      Listen 10122
      
      Listen 54430
      Listen 54431
      
      <VirtualHost *:10122 *:54430 *:54431>
          ProxyPreserveHost     On
          ProxyPass             / http://127.0.0.1:5000/
          ProxyPassReverse      / http://127.0.0.1:5000/
          SSLEngine             on
          SSLProtocol           all -SSLv3
          SSLCertificateFile    certs/localhost.crt
          SSLCertificateKeyFile certs/localhost.key
          ServerName            vsapi.taiko-p.jp
          RequestHeader set "X-Forwarded-Proto" expr=%{REQUEST_SCHEME}
      </VirtualHost>
      ```

   4. Modify conf/openssl.cnf, change the start of file to 

      ```
      #
      # OpenSSL example configuration file.
      # This is mostly being used for generation of certificate requests.
      #
      
      # Note that you can include other files from the main configuration
      # file using the .include directive.
      #.include filename
      
      # This definition stops the following lines choking if HOME isn't
      # defined.
      HOME                    = .
      openssl_conf = default_conf
      # Extra OBJECT IDENTIFIER info:
      #oid_file               = $ENV::HOME/.oid
      oid_section             = new_oids
      
      [default_conf]
      ssl_conf = ssl_sect
      
      [ssl_sect]
      system_default = system_default_sect
      
      [system_default_sect]
      CipherString = DEFAULT@SECLEVEL=1
      
      # To use this configuration file with the "-extfile" option of the
      # "openssl x509" utility, name here the section containing the
      # X.509v3 extensions to use:
      # extensions		=
      # (Alternatively, use a configuration file that has only
      # X.509v3 extensions in its main [= default] section.)
      ```

   5. Copy the cert folder from server release to Apache root folder, then click on the localhost.crt file and import it to trusted root store.

      If everything is correct, run bin/httpd.exe, a command prompt will open (and stay open, if it shut down, probably something is not setup correctly)

4. Now run the server, if everything is setup correctly, visit http://localhost:5000, you should be able to see the web ui up and running without errors.

5. Go to game folder, copy the config files (AMConfig.ini and WritableConfig.ini) in the config folder from server release to AMCUS folder and replace the original ones.

6. Open command prompt as admin, navigate to game root folder (where init.ps1 is). Run `regsvr32 .\AMCUS\iauthdll.dll`. It should prompt about success.

7. Run AMCUS/AMAuthd.exe, then run AMCUS/AMUpdater.exe. If the updater run and exits without issue, you are ready to run the game and connect to server.

8. Run the game, it should now connect to the server.

