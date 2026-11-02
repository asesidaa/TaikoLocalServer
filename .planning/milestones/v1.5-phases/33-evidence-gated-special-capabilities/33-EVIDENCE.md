# Phase 33 Evidence: Murasaki Special Capabilities

## Route Evidence

Current targeted Murasaki route evidence supports the implemented `/v06r00/chassis` endpoints from earlier phases and does not prove these proto-only request families as cabinet routes:

| Surface | Current Evidence | Phase 33 Decision |
|---------|------------------|-------------------|
| `bestscore.php` | `BestScoreRequest` and `BestScoreResponse` exist in `proto/murasaki/taiko.proto`, but the focused Murasaki route-string pass did not find `chassis/bestscore.php`. | Do not implement. Do not fake global rankings from self-best rows. |
| `songhash.php` | `SonghashRequest` and `SonghashResponse` exist in proto inventory, but the focused Murasaki route-string pass did not find `chassis/songhash.php`. | Do not implement. Existing default/mainichi song byte readback remains separate. |
| `shoppingresult.php` | `ShoppingResultRequest` and `ShoppingResultResponse` exist in proto inventory, but the focused Murasaki route-string pass did not find `chassis/shoppingresult.php`. | Do not implement. Do not add Yellow shop, Banacoin, wallet, coupon, or transaction state. |
| Challenge arrays | Murasaki userdata and playresult wire shapes contain embedded challenge fields, but there is no proven standalone Murasaki ChallengeCompe route/readback contract. | Preserve as ignored upload facts. Do not copy Red `challengecompe.php` or White Don Challenge behavior. |

## Byte Limits

Murasaki keeps the established AC15 byte families through `Ac15EraProfiles.Murasaki`:

| Field Family | Limit | Source |
|--------------|-------|--------|
| Song/release/default/mainichi flags | 128 bytes | `BlueProtocolBytes.SongFlagBytes` through common AC15 limits |
| Tone flags | 16 bytes | `BlueProtocolBytes.ToneFlagBytes` through common AC15 limits |
| Title flags | 128 bytes | `BlueProtocolBytes.TitleFlagBytes` through common AC15 limits |
| Costume flags | 32 bytes per slot | `BlueProtocolBytes.CostumeFlagBytes` through common AC15 limits |
| Normal Dan flags | 18 bytes | `BlueProtocolBytes.DanFlagBytes` through common AC15 limits |
| Extra Dan flags | 36 bytes | `BlueProtocolBytes.DanExtraFlagBytes` through common AC15 limits |
| `content_info` | 32 bytes | `BlueProtocolBytes.ContentInfoBytes` through common AC15 limits |
| Crown packed payload | 1280 bytes for 1024 song entries | `BlueProtocolBytes.CrownInflatedBytes` through common AC15 limits |
| `default_option_setting` | 2 bytes | Shared AC15 userdata default/normalization path |

Upload-side opaque byte fields stay non-authoritative. The server maps bytes it accepts for normal play, but Phase 33 does not add validation or new state semantics from `reserved`, `content_info`, option flags, or challenge arrays alone.

## Closeout Rule

If later cabinet logs, IDA proof, or captures show the client actually requests one of the proto-only route families, add a new scoped phase or follow-up task for that specific route. Do not widen Phase 33 by inferring behavior from message names.
