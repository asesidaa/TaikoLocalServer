# 08 - Official Data Curation Workflow

**Goal:** Use subagents or offline tooling to produce candidate official season data without making OCR authoritative or adding runtime scraping.

**Files:**

- Create after validation: `Host/wwwroot/data/green/green_item_shop_data.json`
- Create after validation: `docs/green-client-evidence/07-green-item-shop-official-data.md`
- No runtime code changes in this stage.

## Acceptance Criteria

- [ ] Candidate data has all four official seasons or a documented subset.
- [ ] Runtime JSON contains only protocol data: season envelope plus item triples.
- [ ] Review report records source pages/images, name resolution, confidence, and unresolved rows.
- [ ] No official data is committed until human validation confirms ids/prices.

## Steps

- [ ] **Step 1: Dispatch one curation worker per season**

Use `superpowers:subagent-driven-development` or direct subagents during execution. Give each worker one page:

```text
Spring: https://taiko-ch.net/blog/?page_id=3177
Summer: https://taiko-ch.net/blog/?page_id=3354
Fall: https://taiko-ch.net/blog/?page_id=3645
Winter: https://taiko-ch.net/blog/?page_id=3824
```

Worker instructions:

```text
Extract the Green item shop rows from the official announcement page images.
Return candidate rows with display name, price, inferred item_type, item_id, and confidence.
Use local Green catalogs under Host/wwwroot/data/green for id resolution.
Do not edit runtime JSON.
Do not invent ids for unresolved names.
Preserve row order.
```

- [ ] **Step 2: Reconcile candidate rows in main session**

For each row, validate:

- song names map to Green `song_no` / unique id used by `musicinfo.xml`
- costume names map to the correct slot and id
- `item_type=4` is body and `item_type=5` is head
- price matches the announcement image
- row order matches the announcement image

Rows that cannot be resolved stay out of runtime JSON.

- [ ] **Step 3: Create review report**

Create `docs/green-client-evidence/07-green-item-shop-official-data.md` with this structure:

```markdown
# Green Official Item Shop Data Review

## Sources

- Spring: https://taiko-ch.net/blog/?page_id=3177
- Summer: https://taiko-ch.net/blog/?page_id=3354
- Fall: https://taiko-ch.net/blog/?page_id=3645
- Winter: https://taiko-ch.net/blog/?page_id=3824

## Resolution Rules

- Runtime JSON stores only season protocol fields and item triples.
- `item_no` is inferred by row order.
- `item_type=4` is body; `item_type=5` is head.
- OCR/subagent output is candidate data only.

## Seasons

### Spring

| Row | Name | Price | item_type | item_id | Evidence | Status |
|---:|---|---:|---:|---:|---|---|

### Summer

| Row | Name | Price | item_type | item_id | Evidence | Status |
|---:|---|---:|---:|---:|---|---|

### Fall

| Row | Name | Price | item_type | item_id | Evidence | Status |
|---:|---|---:|---:|---:|---|---|

### Winter

| Row | Name | Price | item_type | item_id | Evidence | Status |
|---:|---|---:|---:|---:|---|---|
```

Fill the tables with validated data. Keep unresolved rows with `Status = unresolved`.

- [ ] **Step 4: Create runtime JSON only after validation**

Create `Host/wwwroot/data/green/green_item_shop_data.json`:

```json
{
  "seasons": [
    {
      "season_id": 1,
      "verup_no": 1,
      "telop": "Spring reward shop",
      "start_datetime": "20190314000000",
      "end_datetime": "20190626075959",
      "afterstart_days": 7,
      "beforeclose_days": 7,
      "items": [
        { "item_type": 4, "item_id": 117, "item_price": 500 }
      ]
    }
  ]
}
```

Replace sample values with validated season data. The JSON must not include names, source URLs, confidence fields, or comments.

- [ ] **Step 5: Validate runtime JSON with loader tests**

Add a focused test to `Tests/Green/GreenItemShopLoaderTests.cs` only if the default file is committed:

```csharp
[Fact]
public async Task LoadFromFile_DefaultGreenItemShopDataLoads()
{
    var path = Path.Combine("Host", "wwwroot", "data", "green", GreenItemShopLoader.FileName);
    if (!File.Exists(path))
    {
        return;
    }

    var catalog = await GreenItemShopLoader.LoadFromFileAsync(
        path,
        new EraSettings { EnableShop = true, ActiveShopSeasonId = 1 },
        CancellationToken.None);

    Assert.True(catalog.IsEnabled);
    Assert.NotNull(catalog.ActiveSeason);
    Assert.NotEmpty(catalog.ActiveSeason.Items);
}
```

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~DefaultGreenItemShopDataLoads"
```

Expected: pass when the file exists.

- [ ] **Step 6: Commit curation outputs**

Only commit validated outputs:

```powershell
git status --short
git add -- docs/green-client-evidence/07-green-item-shop-official-data.md Host/wwwroot/data/green/green_item_shop_data.json Tests/Green/GreenItemShopLoaderTests.cs
git commit -m "Add curated Green item shop default data"
```

If no official data is validated in this implementation pass, skip this commit and leave the stage documented as future/offline work.

