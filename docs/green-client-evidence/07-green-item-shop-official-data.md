# Green Official Item Shop Data Review

## Sources

- Spring: https://taiko-ch.net/blog/?page_id=3177
- Summer: https://taiko-ch.net/blog/?page_id=3354
- Fall: https://taiko-ch.net/blog/?page_id=3645
- Winter: https://taiko-ch.net/blog/?page_id=3824

## Resolution Rules

- Runtime JSON stores only season protocol fields and item triples.
- `item_no` is inferred by row order.
- `item_type=1` is song, `item_type=4` is body, and `item_type=5` is head.
- OCR/subagent output is candidate data only; rows below were reconciled into the numeric runtime JSON.
- Season start dates came from the official announcement header images. End dates in the runtime JSON are non-overlapping season boundaries.

## Subagent Outputs

Four read-only curation workers produced the rows below, one worker per official announcement page. The agent outputs were not meant to edit runtime JSON directly; this report records the reconciled result.

## Seasons

### Spring

| Row | Name | Price | item_type | item_id | Evidence | Status |
|---:|---|---:|---:|---:|---|---|
| 1 | こ・き・く・くる・くる・くれ・こ！ | 1300 | 1 | 799 | Official Spring page image; Green `musicinfo.xml` uniqueid 799 | resolved |
| 2 | PAC-MAN CHAMPIONSHIP EDITION 2 | 1500 | 1 | 797 | Official Spring page image; Green `musicinfo.xml` uniqueid 797 | resolved |
| 3 | トラストゲーム | 1500 | 1 | 800 | Official Spring page image; Green `musicinfo.xml` uniqueid 800 | resolved |
| 4 | あの日出会えたキセキ | 1500 | 1 | 798 | Official Spring page image; Green `musicinfo.xml` uniqueid 798 | resolved |

### Summer

| Row | Name | Price | item_type | item_id | Evidence | Status |
|---:|---|---:|---:|---:|---|---|
| 1 | 王冠 | 500 | 5 | 71 | Official Summer page image; Green costume catalog head id 71 | resolved |
| 2 | ドドドド | 500 | 4 | 116 | Official Summer page image; Green costume catalog body id 116 | resolved |
| 3 | 皇帝 | 500 | 5 | 139 | Official Summer page image; Green costume catalog head id 139 | resolved |
| 4 | 皇帝 | 500 | 4 | 150 | Official Summer page image; Green costume catalog body id 150 | resolved |
| 5 | Heat Haze Shadow 2 | 1500 | 1 | 822 | Official Summer page image; Green `musicinfo.xml` uniqueid 822 | resolved |
| 6 | 和有るど経りて維持・序 | 1300 | 1 | 821 | Official Summer page image; Green `musicinfo.xml` uniqueid 821 | resolved |
| 7 | もしもし神様 | 1300 | 1 | 823 | Official Summer page image; Green `musicinfo.xml` uniqueid 823 | resolved |
| 8 | デッド・オア・ダイ | 1500 | 1 | 820 | Official Summer page image; Green `musicinfo.xml` uniqueid 820 | resolved |

### Fall

| Row | Name | Price | item_type | item_id | Evidence | Status |
|---:|---|---:|---:|---:|---|---|
| 1 | コンパス | 500 | 5 | 53 | Official Fall page image; Green costume catalog head id 53 | resolved |
| 2 | 女子高校生に大人気 | 500 | 4 | 132 | Official Fall page image; Green costume catalog body id 132 | resolved |
| 3 | 剣道 | 500 | 5 | 51 | Official Fall page image; Green costume catalog head id 51 | resolved |
| 4 | クレープやたい | 500 | 4 | 101 | Official Fall page image; Green costume catalog body id 101 | resolved |
| 5 | タベテモタベテモ | 1300 | 1 | 855 | Official Fall page image; Green `musicinfo.xml` uniqueid 855 | resolved |
| 6 | 音虫をつかまえろ！ | 1300 | 1 | 852 | Official Fall page image; Green `musicinfo.xml` uniqueid 852 | resolved |
| 7 | 気焔万丈神楽 | 1500 | 1 | 851 | Official Fall page image; Green `musicinfo.xml` uniqueid 851 | resolved |
| 8 | コネクトカラーズ | 1500 | 1 | 850 | Official Fall page image; Green `musicinfo.xml` uniqueid 850 | resolved |

### Winter

| Row | Name | Price | item_type | item_id | Evidence | Status |
|---:|---|---:|---:|---:|---|---|
| 1 | 電柱 | 500 | 5 | 117 | Official Winter page image; Green costume catalog head id 117 | resolved |
| 2 | おかしの家 | 500 | 4 | 117 | Official Winter page image; Green costume catalog body id 117 | resolved |
| 3 | まほうつかい | 500 | 5 | 135 | Official Winter page image; Green costume catalog head id 135 | resolved |
| 4 | まほうつかい | 500 | 4 | 146 | Official Winter page image; Green costume catalog body id 146 | resolved |
| 5 | 流浪の琥珀姫 | 1300 | 1 | 865 | Official Winter page image; Green `musicinfo.xml` uniqueid 865 | resolved |
| 6 | TD - 28619029byte remix - | 1300 | 1 | 864 | Official Winter page image; Green `musicinfo.xml` uniqueid 864 | resolved |
| 7 | UFO Swingin' | 1500 | 1 | 858 | Official Winter page image; Green `musicinfo.xml` uniqueid 858 | resolved |
| 8 | 氷竜 ～Kooryu～ | 1500 | 1 | 866 | Official Winter page image; Green `musicinfo.xml` uniqueid 866 | resolved |
