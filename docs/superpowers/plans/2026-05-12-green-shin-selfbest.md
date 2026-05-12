# Green Shin Self-Best Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stop the Green cabinet crash on `selfbest.php` by returning a valid `ary_shin_selfbest_score` list in parallel with normal self-best rows.

**Architecture:** Treat Green `ary_shin_selfbest_score` as the AC15 shin-uchi self-best array. The immediate fix is response-shape only: return one shin row for every requested song, with zero scores until separate shin-uchi persistence is proven. Do not change the Green song ID namespace; logs show Green requests `musicinfo.xml` `uniqueid` values, not `class_id`.

**Tech Stack:** ASP.NET Core, protobuf-net, Mediator handlers, EF Core 10, SQLite, xUnit.

---

## Evidence Summary

- `Host/Logs/log-20260512.txt` showed Green `selfbest.php` at `2026-05-12 22:57:36 +08:00` requesting `873, 729, 789, 877, 862, 837, 861, 824, 828, 836`.
- Those values match the first ten `uniqueid` values in `Host/wwwroot/data/green/datatable/musicinfo.xml`, in file order.
- They do not match `class_id` (`2..11` for those same entries). Keep using `uniqueid` as Green protocol `song_no`.
- Green wire schema has `SelfBestResponse.ArySelfbestScores` and `SelfBestResponse.AryShinSelfbestScores`.
- Current Green mapper fills only `ArySelfbestScores`; `AryShinSelfbestScores` remains empty.
- Current storage is not prepared for separate shin-uchi bests:
  - `Domain/Entities/SongBestDatumGreen.cs` has one row per `(Baid, SongId, Difficulty)`.
  - `Infrastructure/Persistence/TaikoDbContext.Green.cs` keys `SongBestDatum_Green` on `(Baid, SongId, Difficulty)`.
  - There is no `IsShin`, no separate score columns, and no separate DbSet/table.
  - `SongPlayDatumGreen` stores `OptionFlg`, but no known code decodes which bit means shin-uchi.

## File Structure

- Modify: `Application/Dtos/CommonSelfBestResponse.cs`
  - Add a common DTO list for Green shin self-best rows.
- Modify: `Application/Handlers/GetSelfBestQuery.Green.cs`
  - Build normal rows from stored Green bests.
  - Build parallel shin rows with the same requested `SongNo` values and zero scores.
- Modify: `Adapters.GameProtocol.Green/Mappers/SelfBestMappers.cs`
  - Map `CommonSelfBestResponse.AryShinSelfbestScores` to Green protobuf `AryShinSelfbestScores`.
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`
  - Verify the Green handler returns normal rows and parallel zero shin rows.
- Create: `Tests/Green/GreenSelfBestMapperTests.cs`
  - Verify the Green protobuf mapper populates both repeated fields.
- Do not modify Green schema/migrations for the immediate crash fix.

---

### Task 1: Add DTO Contract For Shin Self-Best

**Files:**
- Modify: `Application/Dtos/CommonSelfBestResponse.cs`
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`

- [ ] **Step 1: Write the failing handler test**

Add this test to `Tests/Green/GreenPlayResultHandlerTests.cs` after `GetSelfBest_Green_ReturnsSavedBest`:

```csharp
[Fact]
public async Task GetSelfBest_Green_ReturnsParallelZeroShinRows()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
    {
        Baid = 1,
        SongId = 101,
        Difficulty = Difficulty.Normal,
        BestScore = 765432,
        BestCrown = CrownType.Gold
    });
    await fixture.Context.SaveChangesAsync();

    var handler = new GetSelfBestQueryHandler(
        fixture.Catalog,
        fixture.Context,
        NullLogger<GetSelfBestQueryHandler>.Instance);

    var response = await handler.Handle(new GetSelfBestQuery(1, GameEra.Green, 1, [101, 102]), CancellationToken.None);

    Assert.Equal((uint)1, response.Result);
    Assert.Equal([101u, 102u], response.ArySelfbestScores.Select(row => row.SongNo).ToArray());
    Assert.Equal([101u, 102u], response.AryShinSelfbestScores.Select(row => row.SongNo).ToArray());
    Assert.Contains(response.ArySelfbestScores, row => row.SongNo == 101 && row.SelfBestScore == 765432);
    Assert.All(response.AryShinSelfbestScores, row =>
    {
        Assert.Equal((uint)0, row.SelfBestScore);
        Assert.Equal((uint)0, row.UraBestScore);
    });
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run:

```powershell
dotnet test --filter FullyQualifiedName~GreenPlayResultHandlerTests.GetSelfBest_Green_ReturnsParallelZeroShinRows
```

Expected: compile failure because `CommonSelfBestResponse` does not have `AryShinSelfbestScores`.

- [ ] **Step 3: Add the DTO list**

In `Application/Dtos/CommonSelfBestResponse.cs`, change the top-level DTO to:

```csharp
public class CommonSelfBestResponse
{
    public uint Result { get; set; }

    public uint Level { get; set; }

    public List<SelfBestData> ArySelfbestScores { get; set; } = [];

    public List<SelfBestData> AryShinSelfbestScores { get; set; } = [];

    public class SelfBestData
    {
        public uint SongNo        { get; set; }
        public uint SelfBestScore { get; set; }
        public uint UraBestScore  { get; set; }
        public uint SelfBestScoreRate { get; set; }
        public uint UraBestScoreRate  { get; set; }
    }
}
```

- [ ] **Step 4: Run the test again**

Run:

```powershell
dotnet test --filter FullyQualifiedName~GreenPlayResultHandlerTests.GetSelfBest_Green_ReturnsParallelZeroShinRows
```

Expected: test fails because `AryShinSelfbestScores` is empty.

- [ ] **Step 5: Commit**

```powershell
git add -- Application/Dtos/CommonSelfBestResponse.cs Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "test(green): cover shin selfbest response rows"
```

---

### Task 2: Return Parallel Shin Rows From Green Handler

**Files:**
- Modify: `Application/Handlers/GetSelfBestQuery.Green.cs`
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`

- [ ] **Step 1: Replace the Green handler body**

Replace `HandleGreen` in `Application/Handlers/GetSelfBestQuery.Green.cs` with:

```csharp
private partial async ValueTask<CommonSelfBestResponse> HandleGreen(
    GetSelfBestQuery request,
    CancellationToken cancellationToken)
{
    var difficulty = GreenPlayResultMapping.MapDifficulty(request.Difficulty);
    var requestedSongs = request.SongIdList ?? [];
    var requestedSet = requestedSongs.ToHashSet();
    var bestRows = await context.SongBestDataGreen
        .Where(row => row.Baid == request.Baid
            && row.Difficulty == difficulty
            && requestedSet.Contains(row.SongId))
        .ToDictionaryAsync(row => row.SongId, cancellationToken);

    var normalRows = requestedSongs.Select(songNo =>
    {
        bestRows.TryGetValue(songNo, out var best);
        return new CommonSelfBestResponse.SelfBestData
        {
            SongNo = songNo,
            SelfBestScore = best?.BestScore ?? 0,
            SelfBestScoreRate = best?.BestRate ?? 0
        };
    }).ToList();

    var shinRows = requestedSongs.Select(songNo => new CommonSelfBestResponse.SelfBestData
    {
        SongNo = songNo
    }).ToList();

    return new CommonSelfBestResponse
    {
        Result = 1,
        Level = request.Difficulty,
        ArySelfbestScores = normalRows,
        AryShinSelfbestScores = shinRows
    };
}
```

- [ ] **Step 2: Run focused tests**

Run:

```powershell
dotnet test --filter FullyQualifiedName~GreenPlayResultHandlerTests
```

Expected: all `GreenPlayResultHandlerTests` pass.

- [ ] **Step 3: Commit**

```powershell
git add -- Application/Handlers/GetSelfBestQuery.Green.cs
git commit -m "fix(green): return parallel shin selfbest rows"
```

---

### Task 3: Map Shin Rows To Green Protobuf

**Files:**
- Modify: `Adapters.GameProtocol.Green/Mappers/SelfBestMappers.cs`
- Create: `Tests/Green/GreenSelfBestMapperTests.cs`

- [ ] **Step 1: Write the failing mapper test**

Create `Tests/Green/GreenSelfBestMapperTests.cs`:

```csharp
using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenSelfBestMapperTests
{
    [Fact]
    public void Map_GreenSelfBest_FillsNormalAndShinRepeatedFields()
    {
        var common = new CommonSelfBestResponse
        {
            Result = 1,
            Level = 1,
            ArySelfbestScores =
            [
                new() { SongNo = 873, SelfBestScore = 234560, UraBestScore = 0 }
            ],
            AryShinSelfbestScores =
            [
                new() { SongNo = 873, SelfBestScore = 0, UraBestScore = 0 }
            ]
        };

        var response = SelfBestMappers.Map(common);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)1, response.Level);
        Assert.Single(response.ArySelfbestScores);
        Assert.Single(response.AryShinSelfbestScores);
        Assert.Equal((uint)873, response.ArySelfbestScores[0].SongNo);
        Assert.Equal((uint)873, response.AryShinSelfbestScores[0].SongNo);
        Assert.Equal((uint)234560, response.ArySelfbestScores[0].SelfBestScore);
        Assert.Equal((uint)0, response.AryShinSelfbestScores[0].SelfBestScore);
    }
}
```

- [ ] **Step 2: Run the mapper test to verify it fails**

Run:

```powershell
dotnet test --filter FullyQualifiedName~GreenSelfBestMapperTests
```

Expected: test fails because `AryShinSelfbestScores` is empty.

- [ ] **Step 3: Update the mapper**

In `Adapters.GameProtocol.Green/Mappers/SelfBestMappers.cs`, update `Map` to:

```csharp
public static SelfBestResponse Map(CommonSelfBestResponse common)
{
    var response = new SelfBestResponse
    {
        Result = common.Result,
        Level = common.Level
    };

    response.ArySelfbestScores.AddRange(common.ArySelfbestScores.Select(row => new SelfBestResponse.SelfBestData
    {
        SongNo = row.SongNo,
        SelfBestScore = row.SelfBestScore,
        UraBestScore = row.UraBestScore
    }));

    response.AryShinSelfbestScores.AddRange(common.AryShinSelfbestScores.Select(row => new SelfBestResponse.SelfBestData
    {
        SongNo = row.SongNo,
        SelfBestScore = row.SelfBestScore,
        UraBestScore = row.UraBestScore
    }));

    return response;
}
```

- [ ] **Step 4: Run focused tests**

Run:

```powershell
dotnet test --filter "FullyQualifiedName~GreenSelfBestMapperTests|FullyQualifiedName~GreenPlayResultHandlerTests"
```

Expected: all selected tests pass.

- [ ] **Step 5: Commit**

```powershell
git add -- Adapters.GameProtocol.Green/Mappers/SelfBestMappers.cs Tests/Green/GreenSelfBestMapperTests.cs
git commit -m "fix(green): map shin selfbest protobuf rows"
```

---

### Task 4: Verify Green Self-Best End To End

**Files:**
- No code changes expected.

- [ ] **Step 1: Run Green tests**

Run:

```powershell
dotnet test --filter FullyQualifiedName~Green
```

Expected: all Green tests pass.

- [ ] **Step 2: Run full test suite**

Run:

```powershell
dotnet test
```

Expected: all tests pass.

- [ ] **Step 3: Build**

Run:

```powershell
dotnet build
```

Expected: build succeeds with no new errors.

- [ ] **Step 4: Manual smoke check with cabinet**

Start Host the same way the current Green smoke test was run. Use a card with Green save data and navigate until the cabinet calls:

```text
/v11r01/chassis/selfbest.php
```

Expected server log shape:

```text
Green SelfBest request:
  Level           : 1
  ArySongNoes     :
    [0]: 873
    [1]: 729
    ...
HTTP POST /v11r01/chassis/selfbest.php responded 200
```

Expected cabinet behavior: no crash after the response.

- [ ] **Step 5: Commit verification note if this repo tracks smoke docs**

If `docs/superpowers/plans/2026-05-12-green-real-support/08-build-smoke-and-docs.md` is being used as the smoke checklist, add one line under the self-best smoke section:

```markdown
- Green `selfbest.php` returned parallel normal and `ary_shin_selfbest_score` rows; cabinet did not crash.
```

Then commit:

```powershell
git add -- docs/superpowers/plans/2026-05-12-green-real-support/08-build-smoke-and-docs.md
git commit -m "docs(green): record shin selfbest smoke result"
```

---

## Separate Shin Storage Follow-Up

Do not add schema for separate shin-uchi storage in the immediate crash fix. The current code cannot reliably distinguish normal and shin-uchi plays yet.

Required evidence before implementing separate storage:

- Capture a Green `PlayResultDataRequest` for a play made with normal scoring.
- Capture a Green `PlayResultDataRequest` for the same song/difficulty made with shin-uchi enabled.
- Compare at least:
  - top-level fields in `PlayResultDataRequest`
  - `StageData.OptionFlg`
  - `UserDataResponse.DefaultShinSetting`
  - `PlayScore` scale

If the marker is confirmed, implement storage in a separate branch with these concrete changes:

- Add `public bool IsShin { get; set; }` to `SongBestDatumGreen`.
- Add `public bool IsShin { get; set; }` to `SongPlayDatumGreen`.
- Change `SongBestDatum_Green` primary key to `(Baid, SongId, Difficulty, IsShin)`.
- Add an EF migration such as `AddGreenShinBestStorage`.
- Add `CommonPlayResultData.StageData.IsShin`.
- Add a `GreenShinModeDetector` that maps the confirmed marker to `IsShin`.
- Update `UpdatePlayResultCommand.Green.cs` to upsert either normal or shin best rows.
- Update `GetSelfBestQuery.Green.cs` to read normal rows into `ArySelfbestScores` and shin rows into `AryShinSelfbestScores`.
- Keep `CrownsDataController` using normal rows only unless cabinet evidence proves crowns also have a separate shin-uchi crown source.

Do not infer the shin-uchi marker from score range alone. A high normal score and a low shin-uchi score can overlap enough to corrupt persistence.

---

## Final Verification Checklist

- [ ] `dotnet test --filter FullyQualifiedName~Green` passes.
- [ ] `dotnet test` passes.
- [ ] `dotnet build` passes.
- [ ] Green `selfbest.php` logs show requested `uniqueid` values.
- [ ] Cabinet no longer crashes after Green self-best response.
- [ ] No EF migration was added for the immediate response-shape fix.
- [ ] Separate shin-uchi storage remains documented as not ready until the play-result marker is captured.
