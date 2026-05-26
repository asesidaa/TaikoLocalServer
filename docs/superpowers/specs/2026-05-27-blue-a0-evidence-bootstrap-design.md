# Blue A0 Evidence And Bootstrap Spec

**Date:** 2026-05-27
**Status:** A0 evidence result; not an implementation plan
**Scope:** Confirm Blue bootstrap transport, route ownership, and local data
layout before writing Blue adapter code.

## Evidence Inputs

- Blue IDB: `.tools/blue/EBOOT.ELF.i64`
- Local protos: `proto/blue/taiko.proto`, `proto/blue/vsinterface.proto`
- Existing shared startup route:
  `Adapters.GameProtocol.Shared/Controllers/StartupAuthController.cs`
- Existing missing-content-type workaround: `Host/Program.cs`
- Local Blue data symlink: `Host/wwwroot/data/blue/data`

## Confirmed Routes

The Blue binary builds two base URLs in `sub_2D5A28`:

- Game endpoints use `https://%s:%s@%s:%d/v10r03`.
- Startup/version endpoints use `https://%s:%s@%s:%d/v01r00`.

The request path suffixes are still `chassis/*.php`. The binary appends them
with `"%s/%s"`, so server routes should include the leading version prefix:

- `/v01r00/chassis/startupauth.php`
- `/v01r00/chassis/verupauth.php`
- `/v01r00/chassis/verupcomplete.php`
- `/v10r03/chassis/initialdatacheck.php`
- `/v10r03/chassis/tournamentcheck.php`
- `/v10r03/chassis/bookkeeping.php`
- `/v10r03/chassis/coinsetting.php`
- `/v10r03/chassis/gettelop.php`
- `/v10r03/chassis/getfolder.php`
- `/v10r03/chassis/taikojuku.php`
- `/v10r03/chassis/getitemshopinfo.php`
- `/v10r03/chassis/headclerk2.php`
- `/v10r03/chassis/playresult.php`
- `/v10r03/chassis/banacoinerrorlog.php`
- `/v10r03/chassis/baidcheck.php`
- `/v10r03/chassis/mydonentry.php`
- `/v10r03/chassis/userdata.php`
- `/v10r03/chassis/challengecompe.php`
- `/v10r03/chassis/balancecheck.php`
- `/v10r03/chassis/banacoinpayment.php`
- `/v10r03/chassis/crownsdata.php`
- `/v10r03/chassis/recommend.php`
- `/v10r03/chassis/selfbest.php`
- `/v10r03/chassis/heartbeat.php`
- `/v10r03/chassis/itempurchase.php`
- `/v10r03/chassis/battleuserdata.php`
- `/v10r03/chassis/rewardcardcheck.php`
- `/v10r03/chassis/rewardexecution.php`

`GetreitaiRequest`/`GetreitaiResponse` and
`GetbanacoininfoRequest`/`GetbanacoininfoResponse` strings exist in the binary,
but no `chassis/getreitai.php` or `chassis/getbanacoininfo.php` route string
was found in this A0 pass. Do not treat those as cabinet-called routes until
logs or additional IDA evidence prove a path.

## Startup Route Ownership

`proto/blue/vsinterface.proto` and `proto/green/vsinterface.proto` are byte-for-
byte identical in the repo. Blue startup/verup should reuse the existing shared
`/v01r00` route and shared wire shape unless cabinet logs prove a Blue-specific
behavior.

## Request Content Type

The traced Blue request path uses
`network::HttpRequestServiceProxy::sendRequest(const char*, const unsigned char*, std::size_t, network::Receiver*)`.
The traced call sites pass URL, payload pointer, payload size, and receiver; they
do not pass a content-type argument.

The IDB contains generic HTTP header strings, including `Content-Type`, but this
A0 pass found no `application/protobuf` or `application/x-protobuf` string and
no route-specific content-type setup for the Blue protocol sends. Treat Blue as
requiring the same missing-content-type server workaround as Green:

- Add `/v10r03/chassis` to the protobuf content-type assumption in `Host/Program.cs`.
- Keep `/v01r00/chassis` shared for startup/verup.

Cabinet logs should still confirm the actual incoming headers during A1/A8.

## Request Framing

Representative normal endpoints such as BAID and crowns serialize the request
object directly with the protobuf serializer helper and pass the serialized byte
count to `sendRequest`.

`playresult.php` is not Green-style gzip-wrapped payload transport. The Blue
binary stores a serialized `PlayResultRequest` in a delayed ring buffer in
`sub_2DE860`; the first four bytes are an internal ring-buffer length prefix.
`sub_2DCB64` sends from `entry + 4` with exactly that stored length, so the
four-byte prefix is not part of the HTTP request body.

No per-endpoint request gzip, zlib, raw-deflate, or 32-byte request header was
found in the traced Blue send paths. A0 assumption:

- Blue game requests are direct protobuf request bodies.
- Blue `playresult.php` is direct `PlayResultRequest` protobuf.
- Any compression still needs to be considered field-specific on responses or
  byte fields, not as a request transport wrapper.

## Local Blue Data Layout

Current local data:

- `Host/wwwroot/data/blue/.gitkeep` exists.
- `Host/wwwroot/data/blue/data` is a directory symlink to
  `H:\RPCS3\rpcs3-blue\dev_hdd0\game\SCEEXE001\USRDIR\data`.
- `Host/.gitignore` ignores `wwwroot/data/*/data/*` and also has a Blue data
  symlink ignore entry.

The symlink target has the original `USRDIR/data` style layout:

- Top-level directories include `config`, `content`, `don3d`, `font`, `fumen`,
  `libsmart`, `lumendata`, `module`, `movie`, `nutdata`, `shader`, `sound`,
  and `usio`.
- `config` contains `common` and `S10100-1`.
- `config/S10100-1` contains normal catalog inputs such as `musicinfo.xml`,
  `musicmedleyinfo.xml`, `defmusic.bin`, `present.xml`, `spacialbaid.xml`,
  `waiwaiconfig.xml`, plus `battle` and `waiwaicollabo` directories.
- `fumen` contains `tuning.bin` and `tuning_ext.bin`.

The IDB also references `/cache/S10100-1/`, matching the release directory name
observed in the local data tree.

## Unknowns And Deferred Checks

- Actual cabinet request headers still need log confirmation, especially absence
  of `Content-Type`.
- Response body compression was not exhaustively traced in A0.
- Route strings for `getreitai.php` and `getbanacoininfo.php` were not found,
  despite proto message types existing.
- Battle mode route ownership is confirmed only at the path level
  (`battleuserdata.php`); battle semantics remain Track B.
- Banacoin endpoints exist at the route level, but Banacoin/payment behavior is
  still out of scope for normal Blue support.

## A1 Design Constraints

- Add Blue as route prefix `/v10r03/chassis` for game endpoints.
- Reuse the shared `/v01r00/chassis/startupauth.php` route and Blue/Green
  `vsinterface.proto` shape for startup auth.
- Extend the missing-content-type workaround to `/v10r03/chassis`.
- Implement Blue normal endpoints as direct protobuf request/response
  controllers unless a later cabinet trace proves endpoint-specific framing.
- Do not scaffold `getreitai.php` or `getbanacoininfo.php` as cabinet-called
  routes from proto alone.
- Keep Blue runtime game data under `Host/wwwroot/data/blue/data`, with
  release-specific catalog files under `config/S10100-1`.
