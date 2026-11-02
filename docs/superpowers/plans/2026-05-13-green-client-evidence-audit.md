# Green Client Evidence Audit Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace "needs more client evidence" items in `docs/green-protocol-field-audit.md` with IDA-backed Green client constraints, source citations, and follow-up guard/test recommendations.

**Architecture:** Keep this as a read-only reverse-engineering/documentation pass. Open one IDA database session at a time, collect artifacts under `.tools/green-field-evidence/`, save safe IDA metadata only for names/comments, and write findings under `docs/green-client-evidence/` before updating the main audit report.

**Tech Stack:** ida-cli `AgentSession`, local IDA database `H:\taiko\EBOOT.ELF.i64` or fallback `H:\TaikoLocalServer\.tools\ida-snap\EBOOT.ELF.codex.i64`, PowerShell, Markdown.

---

## Ground Rules

- Use the `ida-cli` skill before any IDA work.
- Do not patch the binary.
- Do not implement server code changes in this evidence session.
- Spawn agents serially if using subagents. Only one worker may open/modify the IDB at a time.
- Prefer the original IDB `H:\taiko\EBOOT.ELF.i64`; if locked or unavailable, use `.tools/ida-snap/EBOOT.ELF.codex.i64`.
- It is safe to save IDA metadata for function/global names and comments when evidence is clear.
- Every evidence item must distinguish:
  - proven by IDA,
  - inferred from server source,
  - not found.

## Source Map

- `docs/green-protocol-field-audit.md`: current audit and "Needs More Client Evidence" list.
- `Adapters.GameProtocol.Green/Wire/Game.cs`: generated Green protobuf fields and field names.
- `Adapters.GameProtocol.Green/Controllers/*.cs`: Green route names and response/request types.
- `Application/Handlers/*.Green.cs`: server behavior to compare against client constraints.
- `Application/Common/GreenProtocolBytes.cs`: source-known bitset sizes.
- `.tools/ida-snap/EBOOT.ELF.codex.i64`: fallback IDA DB with Taikojuku metadata already added.
- `.tools/green-field-evidence/`: new artifact directory for JSON and Markdown evidence from this plan.
- `docs/green-client-evidence/`: new human-readable evidence reports.

## Task 1: Evidence Workspace and Reusable IDA Probe

**Files:**
- Create: `.tools/green-field-evidence/ida_probe.py`
- Create: `.tools/green-field-evidence/README.md`
- Create: `docs/green-client-evidence/README.md`

- [ ] **Step 1: Create artifact directories**

Run:

```powershell
New-Item -ItemType Directory -Force -Path '.tools\green-field-evidence','docs\green-client-evidence'
```

Expected: both directories exist.

- [ ] **Step 2: Create `.tools/green-field-evidence/ida_probe.py`**

Create this exact script:

```python
import argparse
import json
from pathlib import Path

from ida_cli.agent_bridge import AgentSession


def ida_string_literal(value: str) -> str:
    return json.dumps(value)


def result_to_file(output_dir: Path, name: str, value) -> Path:
    output_dir.mkdir(parents=True, exist_ok=True)
    path = output_dir / f"{name}.json"
    path.write_text(json.dumps(value, indent=2, ensure_ascii=False), encoding="utf-8")
    return path


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--db", required=True)
    parser.add_argument("--name", required=True)
    parser.add_argument("--needles", nargs="+", required=True)
    parser.add_argument("--out", default=".tools/green-field-evidence")
    parser.add_argument("--decompile-xrefs", action="store_true")
    args = parser.parse_args()

    output_dir = Path(args.out)
    needles_json = json.dumps([needle.lower() for needle in args.needles])

    with AgentSession.start(args.db, require_ida=True) as ida:
        backend = ida.probe_backend(require_ida=True)
        result_to_file(output_dir, f"{args.name}_backend", backend)

        hits = ida.result(
            f"""
needles = {needles_json}
items = ai.strings()
hits = []
for item in items:
    text = item.get("string", "") if isinstance(item, dict) else str(item)
    lower = text.lower()
    if any(needle in lower for needle in needles):
        hits.append(item)
__result__ = hits
""",
            request_id=f"{args.name}.strings",
            timeout_s=120,
        )
        result_to_file(output_dir, f"{args.name}_strings", hits)

        if args.decompile_xrefs:
            xref_pack = ida.result(
                f"""
needles = {needles_json}
items = ai.strings()
packs = []
for item in items:
    text = item.get("string", "") if isinstance(item, dict) else str(item)
    lower = text.lower()
    if not any(needle in lower for needle in needles):
        continue
    ea = item.get("ea") or item.get("address") or item.get("addr")
    if ea is None:
        packs.append({{"string": item, "xrefs": []}})
        continue
    xrefs = ai.xrefs_to(ea)
    contexts = []
    for xref in xrefs[:20]:
        frm = xref.get("from") if isinstance(xref, dict) else None
        if frm is None:
            contexts.append({{"xref": xref, "context": None}})
            continue
        try:
            contexts.append({{"xref": xref, "context": ai.context_pack(frm, disasm_limit=32, include_decompile=True)}})
        except Exception as exc:
            contexts.append({{"xref": xref, "error": str(exc)}})
    packs.append({{"string": item, "xrefs": contexts}})
__result__ = packs
""",
                request_id=f"{args.name}.xrefs",
                timeout_s=300,
            )
            result_to_file(output_dir, f"{args.name}_xrefs", xref_pack)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
```

- [ ] **Step 3: Create evidence READMEs**

Create `.tools/green-field-evidence/README.md`:

```markdown
# Green Field Evidence Artifacts

Generated JSON artifacts from ida-cli evidence runs. These files are working artifacts, not final conclusions. Summaries belong in `docs/green-client-evidence/`.
```

Create `docs/green-client-evidence/README.md`:

```markdown
# Green Client Evidence

This directory records IDA-backed constraints for Green protocol fields that were marked "needs more client evidence" in `docs/green-protocol-field-audit.md`.

Each report must state whether a constraint is IDA-proven, source-inferred, or not found.
```

- [ ] **Step 4: Smoke-test the probe without saving IDA changes**

Run with the fallback DB first:

```powershell
python .tools\green-field-evidence\ida_probe.py --db '.tools\ida-snap\EBOOT.ELF.codex.i64' --name smoke --needles userdata.php disp_taikojuku_dan --out '.tools\green-field-evidence' --decompile-xrefs
```

Expected: JSON files appear under `.tools/green-field-evidence/` and include backend information plus string hits or empty hit lists.

- [ ] **Step 5: Commit the evidence harness**

Run:

```powershell
git add -- .tools/green-field-evidence/ida_probe.py .tools/green-field-evidence/README.md docs/green-client-evidence/README.md
git commit -m "Add Green client evidence audit harness"
```

## Task 2: Version and Update Semantics

**Files:**
- Create: `docs/green-client-evidence/01-version-update-fields.md`
- Update: `docs/green-protocol-field-audit.md`
- Optional IDA metadata: comments on handlers found for version fields.

- [ ] **Step 1: Collect endpoint and field string evidence**

Run:

```powershell
python .tools\green-field-evidence\ida_probe.py --db '.tools\ida-snap\EBOOT.ELF.codex.i64' --name version_update --needles initialdatacheck.php taikojuku.php getitemshopinfo.php gettelop.php getfolder.php tournamentcheck.php song_hash_ver verup_no season_id start_datetime end_datetime --out '.tools\green-field-evidence' --decompile-xrefs
```

Expected: `version_update_strings.json` and `version_update_xrefs.json` are written.

- [ ] **Step 2: Inspect server fields for comparison**

Run:

```powershell
rg -n "SongHashVer|song_hash_ver|VerupNo|verup_no|SeasonId|StartDatetime|EndDatetime|IsItemshop|IsDanplay" Adapters.GameProtocol.Green Application Tests -g "*.cs"
```

Expected: output identifies generated fields, mappers, and handlers that set version/update fields.

- [ ] **Step 3: Write `docs/green-client-evidence/01-version-update-fields.md`**

Use this exact structure, and include one filled row for each listed endpoint/field. Every cell in the committed file must contain a concrete result: `IDA-proven`, `source-inferred`, or `not found`, plus the relevant address/function/source citation.

```markdown
# Version and Update Field Evidence

## Scope

Fields: `song_hash_ver`, `verup_no`, `season_id`, `start_datetime`, `end_datetime`.

Endpoints: `initialdatacheck.php`, `taikojuku.php`, `getitemshopinfo.php`, `gettelop.php`, `getfolder.php`, `tournamentcheck.php`.

## Evidence Table

| Endpoint | Field | IDA evidence | Constraint | Server behavior at audit baseline | Recommendation |
|---|---|---|---|---|---|

Required rows:

- `initialdatacheck.php` / `song_hash_ver`
- `taikojuku.php` / `verup_no`
- `getitemshopinfo.php` / `season_id` and dates
- `gettelop.php` / `verup_no` and dates
- `getfolder.php` / `verup_no`
- `tournamentcheck.php` / `song_hash_ver`

## Artifact Files

- `.tools/green-field-evidence/version_update_strings.json`
- `.tools/green-field-evidence/version_update_xrefs.json`
```

Fill every blank cell with a concrete result: `IDA-proven`, `source-inferred`, or `not found`.

- [ ] **Step 4: Update the main audit report**

In `docs/green-protocol-field-audit.md`, update the `song_hash_ver`, `verup_no fields`, and `Shop dates and item rows` rows in "Needs More Client Evidence" with either:

```markdown
Evidence collected in `docs/green-client-evidence/01-version-update-fields.md`; follow-up implementation required.
```

or a short final constraint if the evidence is conclusive.

- [ ] **Step 5: Commit version/update evidence**

Run:

```powershell
git add -- .tools/green-field-evidence/version_update_*.json docs/green-client-evidence/01-version-update-fields.md docs/green-protocol-field-audit.md
git commit -m "Document Green version and update field evidence"
```

## Task 3: UserData and BAID Optional Fields

**Files:**
- Create: `docs/green-client-evidence/02-userdata-baid-fields.md`
- Update: `docs/green-protocol-field-audit.md`
- Optional IDA metadata: comments near confirmed userdata/baid response handlers.

- [ ] **Step 1: Collect userdata/BAID evidence**

Run:

```powershell
python .tools\green-field-evidence\ida_probe.py --db '.tools\ida-snap\EBOOT.ELF.codex.i64' --name userdata_baid --needles userdata.php baidcheck.php option_flg disp_dan_type got_dan_max got_dan_flg got_danextra_flg tone_flg title_flg costume_flg default_tone_setting titleplate_id color_face color_body color_limb difficulty_played_course difficulty_played_star --out '.tools\green-field-evidence' --decompile-xrefs
```

Expected: `userdata_baid_strings.json` and `userdata_baid_xrefs.json` are written.

- [ ] **Step 2: Inspect generated and mapper fields**

Run:

```powershell
rg -n "OptionFlg|DispDanType|GotDanMax|GotDanFlg|GotDanextraFlg|ToneFlg|TitleFlg|CostumeFlg|DefaultToneSetting|TitleplateId|ColorFace|ColorBody|ColorLimb|DifficultyPlayed" Adapters.GameProtocol.Green Application Tests -g "*.cs"
```

Expected: output maps each field to generated wire, mapper, handler, and tests.

- [ ] **Step 3: Write `docs/green-client-evidence/02-userdata-baid-fields.md`**

Use this exact structure, and include one filled row for each listed endpoint/field. Every cell in the committed file must contain a concrete result: `IDA-proven`, `source-inferred`, or `not found`, plus the relevant address/function/source citation.

```markdown
# UserData and BAID Field Evidence

## Scope

Endpoints: `userdata.php`, `baidcheck.php`.

Fields: `option_flg`, `disp_dan_type`, `got_dan_max`, `got_dan_flg`, `got_danextra_flg`, costume/tone/title flags, default selected IDs, titleplate, Don colors, difficulty played fields.

## Evidence Table

| Endpoint | Field | IDA evidence | Constraint | 0 valid? | Omit when unknown? | Server behavior at audit baseline | Recommendation |
|---|---|---|---|---|---|---|---|

Required rows:

- `userdata.php` / `option_flg`
- `userdata.php` / `difficulty_played_course`
- `userdata.php` / `difficulty_played_star`
- `baidcheck.php` / `disp_dan_type`
- `baidcheck.php` / `got_dan_max`
- `baidcheck.php` / `got_dan_flg`
- `baidcheck.php` / `got_danextra_flg`
- `baidcheck.php` / costume, tone, and title selected IDs
- `baidcheck.php` / Don colors

## Artifact Files

- `.tools/green-field-evidence/userdata_baid_strings.json`
- `.tools/green-field-evidence/userdata_baid_xrefs.json`
```

Fill every blank cell with a concrete result.

- [ ] **Step 4: Update the main audit report**

Update the related rows in `docs/green-protocol-field-audit.md`:

- `disp_dan_type`, `got_dan_max`
- Costume/tone/title catalog IDs
- `option_flg`
- Difficulty played fields if conclusive

- [ ] **Step 5: Commit userdata/BAID evidence**

Run:

```powershell
git add -- .tools/green-field-evidence/userdata_baid_*.json docs/green-client-evidence/02-userdata-baid-fields.md docs/green-protocol-field-audit.md
git commit -m "Document Green userdata and BAID field evidence"
```

## Task 4: Score, Crown, and Counter Evidence

**Files:**
- Create: `docs/green-client-evidence/03-score-crown-counter-fields.md`
- Update: `docs/green-protocol-field-audit.md`
- Optional IDA metadata: names/comments for crown unpack/score handlers.

- [ ] **Step 1: Collect score/crown field evidence**

Run:

```powershell
python .tools\green-field-evidence\ida_probe.py --db '.tools\ida-snap\EBOOT.ELF.codex.i64' --name score_crown --needles selfbest.php crownsdata.php playresult.php ary_selfbest_score ary_shin_selfbest_score self_best_score ura_best_score hash_crown_flg play_score play_result good_cnt ok_cnt ng_cnt pound_cnt combo_cnt hit_cnt --out '.tools\green-field-evidence' --decompile-xrefs
```

Expected: `score_crown_strings.json` and `score_crown_xrefs.json` are written.

- [ ] **Step 2: Inspect server score/crown mapping**

Run:

```powershell
rg -n "GreenCrown|Crown|SelfBest|BestScore|PlayScore|GoodCnt|OkCnt|NgCnt|PoundCnt|ComboCnt|HitCnt|MapCrown|MapGreenCrownState" Application Adapters.GameProtocol.Green Tests -g "*.cs"
```

Expected: output identifies all source mapping and tests for crown/selfbest data.

- [ ] **Step 3: Write `docs/green-client-evidence/03-score-crown-counter-fields.md`**

Use this exact structure, and include one filled row for each listed field. Every cell in the committed file must contain a concrete result: `IDA-proven`, `source-inferred`, or `not found`, plus the relevant address/function/source citation.

```markdown
# Score, Crown, and Counter Field Evidence

## Scope

Endpoints: `selfbest.php`, `crownsdata.php`, request-side `playresult.php`.

## Evidence Table

| Field | IDA evidence | Constraint | 0 valid? | Max/list length | Server behavior at audit baseline | Recommendation |
|---|---|---|---|---|---|---|

Required rows:

- `selfbest.level`
- `selfbest.ary_selfbest_score[].song_no`
- `selfbest.ura_best_score`
- `crownsdata.hash_crown_flg`
- crown state value `3`
- `playresult.stage.play_score`
- hit/count fields

## Artifact Files

- `.tools/green-field-evidence/score_crown_strings.json`
- `.tools/green-field-evidence/score_crown_xrefs.json`
```

Fill every blank cell with a concrete result.

- [ ] **Step 4: Update the main audit report**

Update the rows for:

- Score/counter maxima
- Crown state `3`
- Any selfbest Ura/Shin row behavior proven by the client

- [ ] **Step 5: Commit score/crown evidence**

Run:

```powershell
git add -- .tools/green-field-evidence/score_crown_*.json docs/green-client-evidence/03-score-crown-counter-fields.md docs/green-protocol-field-audit.md
git commit -m "Document Green score and crown field evidence"
```

## Task 5: Ghost, Token, Rank, and Section Evidence

**Files:**
- Create: `docs/green-client-evidence/04-ghost-fields.md`
- Update: `docs/green-protocol-field-audit.md`
- Optional IDA metadata: names/comments for ghost response/request handlers.

- [ ] **Step 1: Collect ghost field evidence**

Run:

```powershell
python .tools\green-field-evidence\ida_probe.py --db '.tools\ida-snap\EBOOT.ELF.codex.i64' --name ghost --needles getghostdata.php getghostscore.php ghost_release_data ghost_update_perfdata ghost_update_rank release_info_flag played_song_flag ary_token_data rank_id win_point certified_level_id ary_winnings_data ary_best_section_data sd_certified_level_id --out '.tools\green-field-evidence' --decompile-xrefs
```

Expected: `ghost_strings.json` and `ghost_xrefs.json` are written.

- [ ] **Step 2: Inspect server ghost mapping**

Run:

```powershell
rg -n "Ghost|Token|Winnings|Rank|Certified|Section|ReleaseInfo" Application Adapters.GameProtocol.Green Tests -g "*.cs"
```

Expected: output identifies ghost DTOs, mappers, handlers, and tests.

- [ ] **Step 3: Write `docs/green-client-evidence/04-ghost-fields.md`**

Use this exact structure, and include one filled row for each listed field. Every cell in the committed file must contain a concrete result: `IDA-proven`, `source-inferred`, or `not found`, plus the relevant address/function/source citation.

```markdown
# Ghost Field Evidence

## Scope

Endpoints: `getghostdata.php`, `getghostscore.php`, request-side ghost fields in `playresult.php`.

## Evidence Table

| Field | IDA evidence | Constraint | 0 valid? | Required length/count | Server behavior at audit baseline | Recommendation |
|---|---|---|---|---|---|---|

Required rows:

- `release_info_flag`
- `played_song_flag`
- token IDs and values
- rank IDs
- certified level IDs
- winnings level IDs
- ghost score sections

## Artifact Files

- `.tools/green-field-evidence/ghost_strings.json`
- `.tools/green-field-evidence/ghost_xrefs.json`
```

Fill every blank cell with a concrete result.

- [ ] **Step 4: Update the main audit report**

Update `Ghost ranks/tokens/sections` in `docs/green-protocol-field-audit.md`.

- [ ] **Step 5: Commit ghost evidence**

Run:

```powershell
git add -- .tools/green-field-evidence/ghost_*.json docs/green-client-evidence/04-ghost-fields.md docs/green-protocol-field-audit.md
git commit -m "Document Green ghost field evidence"
```

## Task 6: Shop, Reward, Catalog ID Evidence

**Files:**
- Create: `docs/green-client-evidence/05-shop-reward-catalog-fields.md`
- Update: `docs/green-protocol-field-audit.md`
- Optional IDA metadata: names/comments for shop/reward handlers.

- [ ] **Step 1: Collect shop/reward/catalog evidence**

Run:

```powershell
python .tools\green-field-evidence\ida_probe.py --db '.tools\ida-snap\EBOOT.ELF.codex.i64' --name shop_reward --needles getitemshopinfo.php itempurchase.php rewardexecution.php rewardcardcheck.php item_no item_type item_id item_price release_song_no get_tone_no get_costume_no get_title_no titleplate_id default_tone_setting --out '.tools\green-field-evidence' --decompile-xrefs
```

Expected: `shop_reward_strings.json` and `shop_reward_xrefs.json` are written.

- [ ] **Step 2: Inspect server shop/reward mapping**

Run:

```powershell
rg -n "ItemShop|ItemPurchase|RewardExecution|RewardCard|GetTone|GetCostume|GetTitle|Titleplate|DefaultTone|release_song" Application Adapters.GameProtocol.Green Tests -g "*.cs"
```

Expected: output identifies shop/reward DTOs, mappers, handlers, and tests.

- [ ] **Step 3: Write `docs/green-client-evidence/05-shop-reward-catalog-fields.md`**

Use this exact structure, and include one filled row for each listed field. Every cell in the committed file must contain a concrete result: `IDA-proven`, `source-inferred`, or `not found`, plus the relevant address/function/source citation.

```markdown
# Shop, Reward, and Catalog ID Evidence

## Scope

Endpoints: `getitemshopinfo.php`, `itempurchase.php`, `rewardexecution.php`, `rewardcardcheck.php`, and related BAID/userdata selected IDs.

## Evidence Table

| Field | IDA evidence | Constraint | 0 valid? | Catalog namespace | Server behavior at audit baseline | Recommendation |
|---|---|---|---|---|---|---|

Required rows:

- shop `item_no`
- shop `item_type`
- shop `item_id`
- shop `item_price`
- reward `release_song_no`
- reward tone IDs
- reward costume IDs
- reward title IDs
- selected tone/titleplate IDs

## Artifact Files

- `.tools/green-field-evidence/shop_reward_strings.json`
- `.tools/green-field-evidence/shop_reward_xrefs.json`
```

Fill every blank cell with a concrete result.

- [ ] **Step 4: Update the main audit report**

Update rows for:

- Costume/tone/title catalog IDs
- Shop dates and item rows
- Reward arrays
- Selected titleplate/default tone IDs

- [ ] **Step 5: Commit shop/reward evidence**

Run:

```powershell
git add -- .tools/green-field-evidence/shop_reward_*.json docs/green-client-evidence/05-shop-reward-catalog-fields.md docs/green-protocol-field-audit.md
git commit -m "Document Green shop and reward catalog evidence"
```

## Task 7: Final Evidence Synthesis

**Files:**
- Create: `docs/green-client-evidence/06-evidence-summary.md`
- Update: `docs/green-protocol-field-audit.md`

- [ ] **Step 1: Write final evidence summary**

Create `docs/green-client-evidence/06-evidence-summary.md`:

```markdown
# Green Client Evidence Summary

## Conclusive Constraints

| Area | Constraint | Evidence file | Follow-up guard/test |
|---|---|---|---|

## Still Not Found

| Area | What was searched | Why still open | Safe interim behavior |
|---|---|---|---|

## IDA Metadata Changes

| Address | Change | Reason |
|---|---|---|
```

Fill each table with results from Tasks 2-6.

- [ ] **Step 2: Update main audit report status**

In `docs/green-protocol-field-audit.md`:

- Move any resolved "Needs More Client Evidence" item into a conclusive section or link to the evidence file.
- Leave unresolved items in "Needs More Client Evidence" with the exact searches performed and the safe interim behavior.
- Add links to all `docs/green-client-evidence/*.md` reports.

- [ ] **Step 3: Run documentation checks**

Run:

```powershell
git diff --check -- docs/green-protocol-field-audit.md docs/green-client-evidence .tools/green-field-evidence
```

Expected: no whitespace errors.

- [ ] **Step 4: Commit final synthesis**

Run:

```powershell
git add -- docs/green-protocol-field-audit.md docs/green-client-evidence .tools/green-field-evidence
git commit -m "Summarize Green client field evidence"
```

## Task 8: Handoff for Follow-Up Implementation

**Files:**
- Create: `docs/superpowers/plans/YYYY-MM-DD-green-evidence-follow-up-guards.md`

- [ ] **Step 1: Create a follow-up implementation plan**

After evidence is complete, create a new implementation plan for only the conclusive constraints. Use this command to list evidence files:

```powershell
Get-ChildItem docs\green-client-evidence -Filter '*.md' | Select-Object -ExpandProperty FullName
```

Expected: all evidence reports are listed.

The follow-up plan must include:

```markdown
# Green Evidence Follow-Up Guards Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement only Green field guards proven by the client evidence audit.

**Architecture:** Replace conservative interim guards with exact client-proven domains where evidence is conclusive. Leave unresolved fields conservative and documented.

**Tech Stack:** C#/.NET, xUnit, EF Core SQLite test fixture, MediatR handlers, protobuf-net generated Green wire types.
```

- [ ] **Step 2: Commit the follow-up plan**

Run:

```powershell
git add -- docs/superpowers/plans
git commit -m "Plan Green evidence follow-up guards"
```
