# Task 3 — AI Battle Integration Tests

**Goal:** Add `Tests/Green/GreenAiBattlePlayResultTests.cs` with seven end-to-end tests against the `UpdatePlayResultCommandHandler` and the in-memory `GreenHandlerFixture`. These tests assert the new AI Battle behavior introduced in Task 2.

**Files:**
- Create: `Tests/Green/GreenAiBattlePlayResultTests.cs`

**Depends on:** Task 2 must be merged first.

**Acceptance Criteria:**
- [ ] `UpdatePlayResult_Green_AcceptsAiBattlePlay` — full happy path: result `1`; 1 `SongPlayDatumGreen` with `StageMode=3, IsShin=false, PlayMode=6`; 3 `GhostStageSectionDatumGreen` rows; 1 `GreenGhostWinnings`; 1 `GreenGhostTokens`; `UserSaveDataGreen` ghost columns set; `GhostPlayedSongFlag` bit set; `GhostReleaseInfoFlag` bit set.
- [ ] `UpdatePlayResult_Green_AiBattleShinRoutesToShinBest` — `StageMode=4` upserts into `SongBestDatumGreen` with `IsShin=true`.
- [ ] `UpdatePlayResult_Green_AiBattleIntermediateAiDifficultyDoesNotUpdateCrown` — `SdCertifiedLevelId=11`, `PlayResult=2 (Gold)` does not lift the pre-seeded `BestCrown=None`; score still updates.
- [ ] `UpdatePlayResult_Green_AiBattleCertifiedAiDifficultyUpdatesCrown` — `SdCertifiedLevelId=13`, `PlayResult=2 (Gold)` lifts `BestCrown` to `Gold`.
- [ ] `UpdatePlayResult_Green_AiBattleUraAlwaysUpdatesCrown` — `Level=5, SdCertifiedLevelId=7` (non-正規 AI) still lifts `BestCrown`.
- [ ] `UpdatePlayResult_Green_RejectsAiBattleStageModeTwoAndFive` — `StageMode=2` and `StageMode=5` both reject; nothing persisted.
- [ ] `UpdatePlayResult_Green_NonAiBattlePlayDoesNotTouchGhostFields` — `PlayMode=0, StageMode=0`, no `GhostUpdate*` fields; `GhostPlayedSongFlag` stays all zero, no `GreenGhostWinnings` / `GreenGhostTokens` rows added.

**Verify:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAiBattlePlayResult"` → all 7 tests pass. Also confirm `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"` stays green.

---

## Background

### Fixture pattern

Tests follow the existing `Tests/Green/GreenPlayResultHandlerTests.cs` style:

```csharp
await using var fixture = await GreenHandlerFixture.CreateAsync();
fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
await fixture.Context.SaveChangesAsync();

var handler = new UpdatePlayResultCommandHandler(
    fixture.Context,
    fixture.Catalog,
    NullLogger<UpdatePlayResultCommandHandler>.Instance);

var result = await handler.Handle(new UpdatePlayResultCommand(1, GameEra.Green, commonRequest),
    CancellationToken.None);
```

`SongNo = 101` is in the default test catalog (see `GreenHandlerFixture.TestGreenCatalog.DefaultMusicInfoFileOrder`).

### Default save data quirks worth knowing

`UserSaveDataGreenExtensions.CreateDefaultGreenSaveData` initializes:

- `GhostPlayedSongFlag = new byte[128]` (all zero, 1024 bits).
- `GhostReleaseInfoFlag = new byte[16]` (all zero, 128 bits).
- `TitleFlg = new byte[128]` (all zero, 1024 bits).

After a test play, these should reflect the bits we expect to have flipped.

### Required namespaces

The `Tests` project's `GlobalUsings.cs` already imports `Microsoft.Extensions.Logging.Abstractions`, `TaikoLocalServer.Application.Common`, `TaikoLocalServer.Application.Dtos`, `TaikoLocalServer.Application.Handlers`, `TaikoLocalServer.Domain.Entities`, `TaikoLocalServer.Domain.Enums`, `Xunit`, and EF Core. No additional `using` lines are needed at the top of the new test file.

---

## Steps

- [ ] **Step 1: Create the test file**

Create `Tests/Green/GreenAiBattlePlayResultTests.cs` with the following full content:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class GreenAiBattlePlayResultTests
{
    [Fact]
    public async Task UpdatePlayResult_Green_AcceptsAiBattlePlay()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260518020644",
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = 3,
                        PlayResult = 1,
                        PlayScore = 266580,
                        GoodCnt = 49, OkCnt = 13, NgCnt = 2, PoundCnt = 47, ComboCnt = 51, HitCnt = 109,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        StarLevel = 1,
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 5,
                            ArySectionData =
                            [
                                new() { IsWin = true,  GoodCnt = 13, OkCnt = 5, NgCnt = 2, PoundCnt = 0 },
                                new() { IsWin = true,  GoodCnt = 19, OkCnt = 4, NgCnt = 0, PoundCnt = 0 },
                                new() { IsWin = false, GoodCnt = 17, OkCnt = 4, NgCnt = 0, PoundCnt = 47 }
                            ]
                        }
                    }
                ],
                GhostReleaseData = new CommonPlayResultData.UpdateGhostInfoData
                {
                    ReleaseInfoId = [1],
                    AryTokendata =
                    [
                        new() { TokenId = 9, TokenValue = 42 }
                    ]
                },
                GhostUpdatePerfData = new CommonPlayResultData.UpdateGhostPerfData
                {
                    InputMedian = -12,
                    InputVariance = 1668
                },
                GhostUpdateRankData = new CommonPlayResultData.UpdateGhostRankData
                {
                    RankId = 1,
                    WinPoint = 7,
                    CertifiedLevelId = 5,
                    AryWinningsData =
                    [
                        new() { LevelId = 5, Winnings = 3 }
                    ]
                }
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var play = Assert.Single(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(3u, play.StageMode);
        Assert.False(play.IsShin);
        Assert.Equal(6u, play.PlayMode);

        var sections = await fixture.Context.GhostStageSectionDataGreen
            .Where(row => row.PlayId == play.Id)
            .OrderBy(row => row.SectionNo)
            .ToListAsync();
        Assert.Equal(3, sections.Count);
        Assert.Equal(0u, sections[0].SectionNo);
        Assert.True(sections[0].IsWin);
        Assert.Equal(47u, sections[2].PoundCount);

        var winnings = Assert.Single(await fixture.Context.GreenGhostWinnings.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(5u, winnings.LevelId);
        Assert.Equal(3u, winnings.Winnings);

        var token = Assert.Single(await fixture.Context.GreenGhostTokens.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(9u, token.TokenId);
        Assert.Equal(42u, token.TokenValue);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(-12, save!.GhostInputMedian);
        Assert.Equal(1668u, save.GhostInputVariance);
        Assert.Equal(1u, save.GhostRankId);
        Assert.Equal(7u, save.GhostWinPoint);
        Assert.Equal(5u, save.GhostCertifiedLevelId);
        Assert.Equal(3u, save.GhostTotalWinnings);
        Assert.True(BitIsSet(save.GhostPlayedSongFlag, 101));
        Assert.True(BitIsSet(save.GhostReleaseInfoFlag, 1));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiBattleShinRoutesToShinBest()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = 4,
                        PlayResult = 1,
                        PlayScore = 555555,
                        GoodCnt = 30, OkCnt = 5, NgCnt = 1, PoundCnt = 10, ComboCnt = 40, HitCnt = 46,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 5,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var shinBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, true);
        var normalBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, false);
        Assert.NotNull(shinBest);
        Assert.Null(normalBest);
        Assert.Equal(555555u, shinBest!.BestScore);

        var play = Assert.Single(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(4u, play.StageMode);
        Assert.True(play.IsShin);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiBattleIntermediateAiDifficultyDoesNotUpdateCrown()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Normal,
            IsShin = false,
            BestScore = 100000,
            BestCrown = CrownType.None
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = 3,
                        PlayResult = 2,
                        PlayScore = 200000,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 11,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, false);
        Assert.NotNull(best);
        Assert.Equal(CrownType.None, best!.BestCrown);
        Assert.Equal(200000u, best.BestScore);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiBattleCertifiedAiDifficultyUpdatesCrown()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Normal,
            IsShin = false,
            BestScore = 100000,
            BestCrown = CrownType.None
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = 3,
                        PlayResult = 2,
                        PlayScore = 200000,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 13,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, false);
        Assert.NotNull(best);
        Assert.Equal(CrownType.Gold, best!.BestCrown);
        Assert.Equal(200000u, best.BestScore);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiBattleUraAlwaysUpdatesCrown()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 5,
                        StageMode = 3,
                        PlayResult = 1,
                        PlayScore = 300000,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 7,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.UraOni, false);
        Assert.NotNull(best);
        Assert.Equal(CrownType.Clear, best!.BestCrown);
    }

    [Theory]
    [InlineData(2u)]
    [InlineData(5u)]
    public async Task UpdatePlayResult_Green_RejectsAiBattleStageModeTwoAndFive(uint stageMode)
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = stageMode,
                        PlayResult = 1,
                        PlayScore = 100000,
                        OptionFlg = [0], ToneFlg = new byte[16]
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(0u, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_NonAiBattlePlayDoesNotTouchGhostFields()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 0,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 100000,
                        OptionFlg = [0], ToneFlg = new byte[16]
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.All(save!.GhostPlayedSongFlag, b => Assert.Equal(0, b));
        Assert.All(save.GhostReleaseInfoFlag, b => Assert.Equal(0, b));
        Assert.Equal(0, save.GhostInputMedian);
        Assert.Equal(0u, save.GhostInputVariance);
        Assert.Equal(0u, save.GhostRankId);
        Assert.Equal(0u, save.GhostTotalWinnings);
        Assert.Empty(await fixture.Context.GreenGhostWinnings.ToListAsync());
        Assert.Empty(await fixture.Context.GreenGhostTokens.ToListAsync());
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
```

- [ ] **Step 2: Build and run the new tests**

Run:

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAiBattlePlayResult"
```

Expected: 7 facts + 2 theory rows = 8 tests, all pass.

If any fails: STOP and investigate. Likely failure modes:

| Symptom | Likely cause |
| --- | --- |
| `AcceptsAiBattlePlay` fails with `result == 0` | Task 2's StageMode predicate not applied — re-check `02-handler-edits.md` Step 2. |
| `AcceptsAiBattlePlay` fails with `play.PlayMode != 6` | `SaveStageAsync` already persists `PlayMode = playMode` so this should never fail — verify the test sets `PlayMode = 6` on the request. |
| `AcceptsAiBattlePlay` fails with `BitIsSet(save.GhostPlayedSongFlag, 101)` returning false | `ApplyGhostPlayedSongBits` not called from `HandleGreen` — re-check Step 5 of `02-handler-edits.md`. |
| `AiBattleShinRoutesToShinBest` fails with `shinBest is null` | `isShin` not routed through the interpreter — re-check `02-handler-edits.md` Step 3. |
| `AiBattleIntermediateAiDifficultyDoesNotUpdateCrown` fails with `BestCrown == Gold` | Crown gate missing — re-check Step 4 of `02-handler-edits.md`. The pre-seeded row is hit by the "existing is not null" branch; `allowCrownUpdate` must gate the assignment there. |
| `AiBattleCertifiedAiDifficultyUpdatesCrown` fails with `BestCrown == None` | `IsCertifiedLevel(13)` returning false — re-check Task 1's helper. |
| `AiBattleUraAlwaysUpdatesCrown` fails | `stage.Level == 5` check missing from the OR-chain in Step 3 of `02-handler-edits.md`. |
| `NonAiBattlePlayDoesNotTouchGhostFields` fails because winnings or tokens were inserted | `ApplyGhostUpdates` is over-eager — but it gates on `playResultData.GhostReleaseData / Perf / RankData` being non-null, so the test's null-by-default request must work. If it fails, something on the request was inadvertently populated. |

- [ ] **Step 3: Run the full Green test suite to confirm no cross-test regression**

Run:

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
```

Expected: every Green test passes (existing + new).

- [ ] **Step 4: Commit**

```bash
git -C H:/TaikoLocalServer status --short
git -C H:/TaikoLocalServer add Tests/Green/GreenAiBattlePlayResultTests.cs
git -C H:/TaikoLocalServer commit -m "$(cat <<'EOF'
Cover Green AI Battle play-result behavior

End-to-end tests through GreenHandlerFixture: accept AI Battle plays
with full ghost section / rank / winnings / token / perf persistence,
route Shin AI Battle to the Shin best-score row, gate crown updates
by SdCertifiedLevelId, exempt Ura plays from the crown gate, reject
unknown StageMode values, and confirm normal plays don't touch any
ghost fields.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
EOF
)"
```
