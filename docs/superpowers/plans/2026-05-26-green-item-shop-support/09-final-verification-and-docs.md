# 09 - Final Verification And Docs

**Goal:** Verify the full Green item shop feature, document settings/data shape, and leave the worktree ready for review.

**Files:**

- Modify: `Host/README.md`
- Optional modify: `Host/Configurations/ServerSettings.json` only if the project wants a disabled example value.

## Acceptance Criteria

- [ ] Green shop settings and JSON shape are documented.
- [ ] Full Green test filter passes.
- [ ] Host build passes.
- [ ] EF migration script includes `GreenShopSeasonStates` and `GreenShopItemStates`.
- [ ] Worktree has no accidental unrelated staged files.

## Steps

- [ ] **Step 1: Document Green shop settings**

In `Host/README.md`, under Green data layout, add:

````markdown
### Green item shop

Green item shop support is controlled by `Configurations/ServerSettings.json`:

```json
{
  "ServerSettings": {
    "Eras": {
      "Green": {
        "EnableShop": true,
        "ActiveShopSeasonId": 1
      }
    }
  }
}
```

When `EnableShop` is `false`, the server keeps the current default unlock behavior and does not advertise the shop. When `EnableShop` is `true`, `ActiveShopSeasonId` must match a season in `wwwroot/data/green/green_item_shop_data.json`.

The shop data file stores protocol data only:

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

`item_no` is inferred from 1-based row order. Item rows must not contain names or source metadata. Official announcement pages list names in images, so name-to-id resolution is an offline curation step.
````

- [ ] **Step 2: Run focused shop tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenItemShop"
```

Expected: exit `0`.

- [ ] **Step 3: Run full Green tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
```

Expected: exit `0`.

- [ ] **Step 4: Build Host**

Run:

```powershell
dotnet build Host/Host.csproj
```

Expected: exit `0`.

- [ ] **Step 5: Inspect migration script**

Run:

```powershell
dotnet ef migrations script --project Infrastructure --startup-project Host
```

Expected: output includes create-table SQL for:

```text
GreenShopSeasonStates
GreenShopItemStates
```

Expected: no unrelated table rebuilds other than EF/SQLite operations caused by the new shop tables.

- [ ] **Step 6: Check worktree and staged files**

Run:

```powershell
git status --short
git diff --check
```

Expected:

- no whitespace errors from `git diff --check`
- only intentional files are modified
- no unrelated pre-existing files are staged accidentally

- [ ] **Step 7: Commit final docs**

```powershell
git add -- Host/README.md
git commit -m "Document Green item shop configuration"
```

If `Host/Configurations/ServerSettings.json` was edited only to include disabled example keys, include it in the commit:

```powershell
git add -- Host/README.md Host/Configurations/ServerSettings.json
git commit -m "Document Green item shop configuration"
```

- [ ] **Step 8: Final status report**

Run:

```powershell
git status --short
```

Report:

- commits created
- focused test result
- full Green test result
- Host build result
- migration script inspection result
- any skipped official data curation rows or manual validation still needed
