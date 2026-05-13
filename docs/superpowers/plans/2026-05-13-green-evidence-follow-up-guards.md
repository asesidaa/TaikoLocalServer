# Green Evidence Follow-Up Guards Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement only Green field guards supported by the client evidence audit and conservative source-inferred domains documented in `docs/green-client-evidence/`.

**Architecture:** Replace unsafe default serialization and broad request persistence with exact client-proven domains where available. Where IDA only proves schema strings, keep behavior conservative: omit unknown optional outputs, reject unknown persisted echoes, and document unresolved value domains.

**Tech Stack:** C#/.NET, xUnit, EF Core SQLite test fixture, MediatR handlers, protobuf-net generated Green wire types.

## Evidence Inputs

- `docs/green-client-evidence/01-version-update-fields.md`
- `docs/green-client-evidence/02-userdata-baid-fields.md`
- `docs/green-client-evidence/03-score-crown-counter-fields.md`
- `docs/green-client-evidence/04-ghost-fields.md`
- `docs/green-client-evidence/05-shop-reward-catalog-fields.md`
- `docs/green-client-evidence/06-evidence-summary.md`

## Task 1: Version and Optional Output Guards

- [ ] Add fail-fast validation that Green `song_hash_ver` is nonzero when `musicinfo.xml` loads.
- [ ] Make `userdata.php` `option_flg` presence-aware and omit empty/unknown bytes.
- [ ] Preserve/record presence for `difficulty_played_course` and `difficulty_played_star`; do not serialize persisted default `0` as known progress.
- [ ] Keep telop/folder/tournament/shop optional version/date fields omitted until catalog rows are implemented.

## Task 2: BAID Selected ID Normalization

- [ ] Normalize `disp_dan_type` and `got_dan_max` in `BaidQuery.Green.cs`.
- [ ] Normalize selected titleplate, default tone, Don colors, and current costume slots before mapping to Green wire fields.
- [ ] Add tests for invalid persisted BAID selected/default IDs.

## Task 3: SelfBest, Crown, and PlayResult Guards

- [ ] Reject invalid `selfbest.php` levels and out-of-catalog requested song IDs before echoing rows.
- [ ] Filter crown rows by Green catalog membership before packing `hash_crown_flg`.
- [ ] Add chart-aware `play_score` and hit/count consistency checks when chart note data is available.
- [ ] Keep Green crown response Dondaful downgraded to full combo until client bitset state `3` is proven.

## Task 4: Ghost Guards

- [ ] Validate or cap ghost release IDs, token IDs/values, rank IDs, certified level IDs, winnings rows, and section counts before persistence.
- [ ] Add `getghostscore.php` level guard for `0..4`.
- [ ] Filter ghost response rows to only safe persisted values.

## Task 5: Shop and Reward Guards

- [ ] Preserve optional presence for `item_type`, `item_id`, and `item_price` in `ItemPurchaseController.cs`.
- [ ] Keep catalog/type/id/price/affordability validation in `ItemPurchaseCommand.Green.cs`.
- [ ] Implement `GreenItemShopLoader` before advertising item shop rows.
- [ ] Replace reward "already unlocked" placeholder checks with catalog/eligibility validation when reward catalogs exist.
- [ ] Decide and implement `release_song_no` reward handling against Green catalog song IDs.

## Task 6: Verification

- [ ] Add focused xUnit coverage for each new guard.
- [ ] Run `dotnet test`.
- [ ] Run `git diff --check -- docs/green-protocol-field-audit.md docs/green-client-evidence docs/superpowers/plans`.
