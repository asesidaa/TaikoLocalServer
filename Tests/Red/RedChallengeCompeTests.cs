using TaikoLocalServer.Application.Ac15.ChallengeCompe;
using TaikoLocalServer.Tests.Ac15;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedChallengeCompeTests
{
    [Fact]
    public async Task UpdatePlayResult_Red_EnrolledMatchedChallengePersistsRawFactAndProgressOnlyInRedTables()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog());
        AddUser(fixture, enrolled: true, includeOtherEraSaves: true);
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult>
        {
            CreateStage(
                songNo: 101,
                challengeIds: [new Ac15CompeIdFact(1001, 1)],
                userCompeIds: [new Ac15CompeIdFact(7001, 1)],
                bngCompeIds: [new Ac15CompeIdFact(8001, 1)])
        };

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages,
            challenge: CreateChallenge(stages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var raw = Assert.Single(await fixture.Context.RedChallengeCompeRawFacts.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal("red-2016-07", raw.BundleId);
        Assert.Equal(1001u, raw.TaskId);
        Assert.Equal(1u, raw.Slot);
        Assert.Equal(1001u, raw.CompeId);
        Assert.Equal(1u, raw.TrackNo);
        Assert.Equal(101u, raw.SongNo);
        Assert.Equal(765432u, raw.HighScore);
        Assert.True(raw.Completed);

        var progress = Assert.Single(await fixture.Context.RedChallengeCompeProgress.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal("red-2016-07", progress.BundleId);
        Assert.Equal(1001u, progress.TaskId);
        Assert.Equal(1u, progress.ProgressValue);
        Assert.True(progress.Completed);
        Assert.Equal(new DateTime(2016, 7, 20, 12, 0, 0), progress.CompletedAt);

        Assert.Empty(await fixture.Context.SongPlayDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_DisabledOrNoActiveChallengeCatalogDoesNotCreateChallengeRows()
    {
        await using var disabledFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(enabled: false));
        AddUser(disabledFixture, enrolled: true);
        await RunMatchedChallengeAsync(disabledFixture);

        Assert.Empty(await disabledFixture.Context.RedChallengeCompeRawFacts.ToListAsync());
        Assert.Empty(await disabledFixture.Context.RedChallengeCompeProgress.ToListAsync());

        await using var inactiveFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(startsAt: "2016-08-01T00:00:00Z"));
        AddUser(inactiveFixture, enrolled: true);
        await RunMatchedChallengeAsync(inactiveFixture);

        Assert.Empty(await inactiveFixture.Context.RedChallengeCompeRawFacts.ToListAsync());
        Assert.Empty(await inactiveFixture.Context.RedChallengeCompeProgress.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_NotEnrolledDoesNotCreateChallengeRows()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog());
        AddUser(fixture, enrolled: false);

        await RunMatchedChallengeAsync(fixture);

        Assert.Empty(await fixture.Context.RedChallengeCompeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedChallengeCompeProgress.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_TokkunChallengeFactsDoNotCreateChallengeOrNormalRows()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog());
        AddUser(fixture, enrolled: true);
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult>
        {
            CreateStage(101, [new Ac15CompeIdFact(1001, 1)])
        };

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playMode: (uint)PlayMode.Tokkun,
            playDatetime: "20160720120000",
            stages: stages,
            tokkun: new Ac15TokkunPlayResult(
                TutorialFlg: 3,
                StageData: new Ac15TokkunStageData(
                    BanacoinDatetime: "20160720120100",
                    TokkunSongCnt: 1,
                    TookunSongnoes: [101],
                    TokkunSpeedchangeCnt: 0,
                    TokkunAutoplayCnt: 0,
                    TokkunJumpCnt: 0)),
            challenge: CreateChallenge(stages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.RedChallengeCompeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedChallengeCompeProgress.ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataRed.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataRed.ToListAsync());
        Assert.Empty(await fixture.Context.RedFavoriteSongs.ToListAsync());
        Assert.Empty(await fixture.Context.RedRecentSongs.ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataRed.ToListAsync());
        Assert.Equal(3u, await fixture.Context.UserSaveDataRed.Where(row => row.Baid == 1).Select(row => row.TokkunTutorialFlg).SingleAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_UnmatchedAndNonChallengeBucketsDoNotCreateChallengeRows()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog());
        AddUser(fixture, enrolled: true);
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult>
        {
            CreateStage(
                songNo: 101,
                challengeIds: [new Ac15CompeIdFact(9999, 1), new Ac15CompeIdFact(1001, 99)],
                userCompeIds: [new Ac15CompeIdFact(1001, 1)],
                bngCompeIds: [new Ac15CompeIdFact(1001, 1)])
        };

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages,
            challenge: CreateChallenge(stages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.RedChallengeCompeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedChallengeCompeProgress.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_SongSetRuleAccumulatesDistinctMatchedSongs()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rule: new Ac15ChallengeCompeRule(
            Ac15ChallengeCompeRuleKind.SongSetCount,
            Threshold: 2,
            SongNoes: [101, 102, 103])));
        AddUser(fixture, enrolled: true);
        var handler = CreateHandler(fixture);

        var firstStage = new List<Ac15StageResult> { CreateStage(101, [new Ac15CompeIdFact(1001, 1)]) };
        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: firstStage,
            challenge: CreateChallenge(firstStage)),
            CancellationToken.None);

        var firstProgress = await fixture.Context.RedChallengeCompeProgress.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1u, firstProgress.ProgressValue);
        Assert.False(firstProgress.Completed);

        var secondStage = new List<Ac15StageResult> { CreateStage(102, [new Ac15CompeIdFact(1001, 1)]) };
        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120100",
            stages: secondStage,
            challenge: CreateChallenge(secondStage)),
            CancellationToken.None);

        var progress = await fixture.Context.RedChallengeCompeProgress.SingleAsync(row => row.Baid == 1);
        Assert.Equal(2u, progress.ProgressValue);
        Assert.True(progress.Completed);
        Assert.Equal(2, await fixture.Context.RedChallengeCompeRawFacts.CountAsync(row => row.Baid == 1));
    }

    private static async Task RunMatchedChallengeAsync(RedHandlerFixture fixture)
    {
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult> { CreateStage(101, [new Ac15CompeIdFact(1001, 1)]) };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages,
            challenge: CreateChallenge(stages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
    }

    private static void AddUser(
        RedHandlerFixture fixture,
        bool enrolled,
        bool includeOtherEraSaves = false)
    {
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.IsChallengeCompe = enrolled;
        fixture.Context.UserSaveDataRed.Add(save);
        if (includeOtherEraSaves)
        {
            fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
            fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
            fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        }

        fixture.Context.SaveChanges();
    }

    private static RedHandlerFixture.TestRedCatalog CreateCatalog(
        bool enabled = true,
        string startsAt = "2016-07-01T00:00:00Z",
        Ac15ChallengeCompeRule? rule = null)
    {
        var taskRule = rule ?? new Ac15ChallengeCompeRule(Ac15ChallengeCompeRuleKind.Clear, null, []);
        return new RedHandlerFixture.TestRedCatalog
        {
            ChallengeCompe = new Ac15ChallengeCompeCatalog(
                enabled,
                [
                    new Ac15ChallengeCompeMonthlyBundle(
                        "red-2016-07",
                        DateTimeOffset.Parse(startsAt),
                        DateTimeOffset.Parse("2016-08-01T00:00:00Z"),
                        Enumerable.Range(1, 10)
                            .Select(index => new Ac15ChallengeCompeTask(
                                (uint)(1000 + index),
                                (uint)index,
                                $"Task {index}",
                                index == 1 ? taskRule : new Ac15ChallengeCompeRule(Ac15ChallengeCompeRuleKind.Clear, null, [])))
                            .ToArray(),
                        null,
                        [])
                ])
        };
    }

    private static Ac15StageResult CreateStage(
        uint songNo,
        List<Ac15CompeIdFact> challengeIds,
        List<Ac15CompeIdFact>? userCompeIds = null,
        List<Ac15CompeIdFact>? bngCompeIds = null)
        => new()
        {
            SongNo = songNo,
            Level = 1,
            StageMode = 0,
            PlayResult = 2,
            PlayScore = 765432,
            ScoreRate = 95,
            GoodCnt = 100,
            OkCnt = 20,
            NgCnt = 3,
            PoundCnt = 4,
            ComboCnt = 120,
            HitCnt = 123,
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            SelectedFolderId = 9,
            SoulGauge = 100,
            ChallengeIds = challengeIds,
            UserCompeIds = userCompeIds ?? [],
            BngCompeIds = bngCompeIds ?? []
        };

    private static Ac15RedChallengeCompeFacts CreateChallenge(List<Ac15StageResult> stages)
        => new(stages
            .Where(stage => stage.ChallengeIds.Count != 0 || stage.UserCompeIds.Count != 0 || stage.BngCompeIds.Count != 0)
            .Select(stage => new Ac15RedChallengeCompeStageFacts(stage.SongNo, stage.ChallengeIds, stage.UserCompeIds, stage.BngCompeIds))
            .ToList());

    private static UpdatePlayResultCommandHandler CreateHandler(RedHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);
}
