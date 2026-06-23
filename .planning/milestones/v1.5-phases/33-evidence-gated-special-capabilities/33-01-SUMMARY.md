# Phase 33 Summary: Evidence-Gated Special Capabilities

## Completed

- Added `33-EVIDENCE.md` documenting why `bestscore.php`, `songhash.php`, and `shoppingresult.php` remain unimplemented proto-only surfaces without current route evidence.
- Recorded the accepted Murasaki byte limits for existing AC15 byte families, including 128-byte song/title flags, 16-byte tone flags, 32-byte costume/content fields, 18/36-byte Dan flags, 1280-byte crown payloads, and 2-byte `default_option_setting`.
- Added Murasaki regression coverage for byte-limit defaults.
- Added Murasaki regression coverage proving embedded challenge arrays on playresult do not create Red/White Don Challenge state or shop state.
- Left `bestscore.php`, `songhash.php`, `shoppingresult.php`, ChallengeCompe, Don Challenge, Yellow shop, Banacoin, and global-score persistence absent.

## Deferred

- Real global score, song-hash, shopping, or Don Challenge behavior remains future work until Murasaki route/cabinet/log/IDA evidence proves the route and state contract.
