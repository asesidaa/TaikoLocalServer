# Phase 26 Research: White Collectable Data and Don Challenge Evidence

## Local Data Evidence

- `Host/wwwroot/data/white/data/config/ST7100-1/present.xml` contains ten `PresentItemData` rows with `index`, `type`, `itemNumber`, and `donPoint`.
- `Host/wwwroot/data/white/data/config/ST7100-1/spacialbaid.xml` contains two `SpecialBAID` `info` rows with BAID/access-code/comment fields.
- White catalog customization data is already bootstrapped through Phase 24 AC15 customization extraction and composition.

## Protocol Evidence

- White userdata has embedded `ary_challenge_stat`, `ary_user_compe_stat`, and `ary_bng_compe_stat` fields.
- White playresult stage rows have embedded `ary_challenge_id`, `ary_user_compe_id`, and `ary_bng_compe_id` fields.
- White proto has rewardcardcheck/rewardexecution DTOs, but Phase 23 route evidence did not approve those routes and no White route/runtime state contract proves stateful reward route behavior.

## Don Challenge Decision

White Don Challenge was not implemented as stateful runtime behavior in Phase 26.

This decision is superseded by the 2026-06-18 correction: White Don Challenge is now server-side, stage-derived progress using White-owned catalog/state/AdminApi/WebUI data. ChallengeCompe protocol request/readback remains a separate stub/absent cabinet surface and is not used to send Don Challenge to the game.

Evidence is insufficient for:

- active White challenge bundle data,
- standalone White `challengecompe.php` route ownership,
- task/rule schema,
- opt-in or visibility semantics,
- reward timing,
- userdata readback mutation,
- user-created or BNG/official bucket behavior.

## Deferred

- If future White logs, IDA route evidence, local data, or cabinet captures prove the missing contract, add a future White Challenge expansion phase rather than widening Phase 26.
