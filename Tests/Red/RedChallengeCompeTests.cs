using TaikoLocalServer.Application.Ac15.ChallengeCompe;
using TaikoLocalServer.Application;
using TaikoLocalServer.Tests.Ac15;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RedChallengeCompeController = TaikoLocalServer.Adapters.GameProtocol.Red.Controllers.ChallengeCompeController;
using RedRewardCardCheckController = TaikoLocalServer.Adapters.GameProtocol.Red.Controllers.RewardCardCheckController;
using RedRewardExecutionController = TaikoLocalServer.Adapters.GameProtocol.Red.Controllers.RewardExecutionController;
using RedWire = TaikoLocalServer.Adapters.GameProtocol.Red.Wire;

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

        await using var inactiveFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(activeBundleId: null));
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
    public async Task UpdatePlayResult_Red_DifficultyLimitedChallengeIgnoresLowerDifficulty()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rule: new Ac15ChallengeCompeRule(
            Ac15ChallengeCompeRuleKind.Clear,
            EligibleSongNoes: [101],
            MinimumLevel: 3)));
        AddUser(fixture, enrolled: true);
        var handler = CreateHandler(fixture);

        var normalStage = new List<Ac15StageResult> { CreateStage(101, [new Ac15CompeIdFact(1001, 1)], level: 2) };
        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: normalStage,
            challenge: CreateChallenge(normalStage)),
            CancellationToken.None);

        Assert.Empty(await fixture.Context.RedChallengeCompeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedChallengeCompeProgress.ToListAsync());

        var hardStage = new List<Ac15StageResult> { CreateStage(101, [new Ac15CompeIdFact(1001, 1)], level: 3) };
        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120100",
            stages: hardStage,
            challenge: CreateChallenge(hardStage)),
            CancellationToken.None);

        var progress = Assert.Single(await fixture.Context.RedChallengeCompeProgress.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(3u, progress.Level);
        Assert.True(progress.Completed);
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
    public async Task UpdatePlayResult_Red_ClearRequiredSongCountAccumulatesDistinctMatchedSongs()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rule: new Ac15ChallengeCompeRule(
            Ac15ChallengeCompeRuleKind.Clear,
            RequiredSongCount: 2,
            EligibleSongNoes: [101, 102, 103])));
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

        var secondStage = new List<Ac15StageResult> { CreateStage(102, [new Ac15CompeIdFact(1001, 6)]) };
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

    [Fact]
    public async Task UpdatePlayResult_Red_CompletedChallengeGrantsConfiguredSongRewardOnlyInRedSave()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rewards: [CreateReward(songs: [102])]));
        AddUser(fixture, enrolled: true, includeOtherEraSaves: true);
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
        var redSave = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.True(BitIsSet(redSave.ReleaseSongFlg, 102));

        var blueSave = await fixture.Context.UserSaveDataBlue.SingleAsync(row => row.Baid == 1);
        var greenSave = await fixture.Context.UserSaveDataGreen.SingleAsync(row => row.Baid == 1);
        var yellowSave = await fixture.Context.UserSaveDataYellow.SingleAsync(row => row.Baid == 1);
        Assert.False(BitIsSet(blueSave.ReleaseSongFlg, 102));
        Assert.False(BitIsSet(greenSave.TitleFlg, 10));
        Assert.False(BitIsSet(yellowSave.ReleaseSongFlg, 102));
    }

    [Fact]
    public async Task UpdatePlayResult_Red_CompletedTenTasksGrantsConfiguredTitleReward()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rewards: [CreateReward(threshold: 10, titles: [10])]));
        AddUser(fixture, enrolled: true);
        var handler = CreateHandler(fixture);
        var stages = Enumerable.Range(1, 10)
            .Select(index => CreateStage((uint)(100 + index), [new Ac15CompeIdFact((uint)(1000 + index), 1)]))
            .ToList();

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages,
            challenge: CreateChallenge(stages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var save = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.True(BitIsSet(save.TitleFlg, 10));
    }

    [Fact]
    public async Task UpdatePlayResult_Red_ReplayedChallengeRewardGrantIsIdempotent()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rewards: [CreateReward(songs: [102], titles: [10])]));
        AddUser(fixture, enrolled: true);
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult> { CreateStage(101, [new Ac15CompeIdFact(1001, 1)]) };
        var command = Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages,
            challenge: CreateChallenge(stages));

        await handler.Handle(command, CancellationToken.None);
        var firstSave = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        var firstRelease = firstSave.ReleaseSongFlg.ToArray();
        var firstTitles = firstSave.TitleFlg.ToArray();

        await handler.Handle(command, CancellationToken.None);

        var secondSave = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.Equal(firstRelease, secondSave.ReleaseSongFlg);
        Assert.Equal(firstTitles, secondSave.TitleFlg);
        Assert.Single(await fixture.Context.RedChallengeCompeProgress.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_DisabledOrNoActiveChallengeCatalogDoesNotGrantRewards()
    {
        await using var disabledFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(enabled: false, rewards: [CreateReward(songs: [102], titles: [10])]));
        AddUser(disabledFixture, enrolled: true);
        await RunMatchedChallengeAsync(disabledFixture);
        var disabledSave = await disabledFixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.False(BitIsSet(disabledSave.ReleaseSongFlg, 102));
        Assert.False(BitIsSet(disabledSave.TitleFlg, 10));

        await using var inactiveFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            activeBundleId: null,
            rewards: [CreateReward(songs: [102], titles: [10])]));
        AddUser(inactiveFixture, enrolled: true);
        await RunMatchedChallengeAsync(inactiveFixture);
        var inactiveSave = await inactiveFixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.False(BitIsSet(inactiveSave.ReleaseSongFlg, 102));
        Assert.False(BitIsSet(inactiveSave.TitleFlg, 10));
    }

    [Fact]
    public async Task RewardCompatibilityRoutesDoNotGrantChallengeCompeRewards()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rewards: [CreateReward(songs: [102], titles: [10])]));
        AddUser(fixture, enrolled: true);
        var rewardCard = new RedRewardCardCheckController
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };
        var rewardExecution = new RedRewardExecutionController
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() }
        };

        var cardResult = rewardCard.RewardCardCheck(new RedWire.RewardcardcheckRequest
        {
            AccessCode = "12345678901234567890",
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            CountryId = "JPN"
        });
        var executionResult = rewardExecution.RewardExecution(new RedWire.RewardexecutionRequest
        {
            Baid = 1,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            ReleaseSongNoes = [102],
            GetTitleNoes = [10]
        });

        Assert.Equal(1u, Assert.IsType<RedWire.RewardcardcheckResponse>(Assert.IsType<OkObjectResult>(cardResult).Value).Result);
        Assert.Equal(1u, Assert.IsType<RedWire.RewardexecutionResponse>(Assert.IsType<OkObjectResult>(executionResult).Value).Result);
        var save = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.False(BitIsSet(save.ReleaseSongFlg, 102));
        Assert.False(BitIsSet(save.TitleFlg, 10));
        Assert.Empty(await fixture.Context.RedChallengeCompeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedChallengeCompeProgress.ToListAsync());
    }

    [Fact]
    public async Task UserDataQuery_Red_LocksActiveUnearnedChallengeRewardSongs()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            startsAt: null,
            endsAt: null,
            rewards: [CreateReward(songs: [102])]));
        AddUser(fixture, enrolled: true);
        var handler = CreateUserDataHandler(fixture);

        var response = await handler.Handle(new Ac15UserDataQuery(1, GameEra.Red), CancellationToken.None);

        Assert.True(BitIsSet(response.SongFlags.ReleaseSongFlg, 101));
        Assert.False(BitIsSet(response.SongFlags.ReleaseSongFlg, 102));
    }

    [Fact]
    public async Task UserDataQuery_Red_EarnedChallengeRewardSongIsNotLocked()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            startsAt: null,
            endsAt: null,
            rewards: [CreateReward(songs: [102])]));
        AddUser(fixture, enrolled: true);
        var save = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, [102], Ac15EraProfiles.Red.Limits.SongFlagBytes);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateUserDataHandler(fixture);

        var response = await handler.Handle(new Ac15UserDataQuery(1, GameEra.Red), CancellationToken.None);

        Assert.True(BitIsSet(response.SongFlags.ReleaseSongFlg, 102));
    }

    [Fact]
    public async Task UserDataQuery_Red_DisabledInactiveOrNotOptedInChallengeDoesNotLockRewardSongs()
    {
        await using var disabledFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(enabled: false, rewards: [CreateReward(songs: [102])]));
        AddUser(disabledFixture, enrolled: true);
        var disabledResponse = await CreateUserDataHandler(disabledFixture).Handle(new Ac15UserDataQuery(1, GameEra.Red), CancellationToken.None);
        Assert.True(BitIsSet(disabledResponse.SongFlags.ReleaseSongFlg, 102));

        await using var inactiveFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            activeBundleId: null,
            rewards: [CreateReward(songs: [102])]));
        AddUser(inactiveFixture, enrolled: true);
        var inactiveResponse = await CreateUserDataHandler(inactiveFixture).Handle(new Ac15UserDataQuery(1, GameEra.Red), CancellationToken.None);
        Assert.True(BitIsSet(inactiveResponse.SongFlags.ReleaseSongFlg, 102));

        await using var notEnrolledFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            startsAt: null,
            endsAt: null,
            rewards: [CreateReward(songs: [102])]));
        AddUser(notEnrolledFixture, enrolled: false);
        var notEnrolledResponse = await CreateUserDataHandler(notEnrolledFixture).Handle(new Ac15UserDataQuery(1, GameEra.Red), CancellationToken.None);
        Assert.True(BitIsSet(notEnrolledResponse.SongFlags.ReleaseSongFlg, 102));
    }

    [Fact]
    public async Task GetChallengeCompeQuery_Red_ReturnsActiveProgressAndEmptyUnsupportedBucketsWithoutMutation()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            startsAt: null,
            endsAt: null,
            includeDefaultTaskTracks: false));
        AddUser(fixture, enrolled: true);
        var save = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        var releaseBefore = save.ReleaseSongFlg.ToArray();
        var titleBefore = save.TitleFlg.ToArray();
        fixture.Context.RedChallengeCompeProgress.Add(new RedChallengeCompeProgress
        {
            Baid = 1,
            BundleId = "red-2016-07",
            TaskId = 1001,
            Slot = 1,
            CompeId = 1001,
            TrackNo = 1,
            SongNo = 101,
            Level = 1,
            OptionFlg = [1, 2, 3],
            StageMode = 0,
            HighScore = 765432,
            ProgressValue = 1,
            Completed = false,
            UpdatedAt = new DateTime(2016, 7, 20, 12, 0, 0)
        });
        fixture.Context.SongBestDataRed.Add(new SongBestDatumRed
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Easy,
            BestScore = 800000,
            BestRate = 95,
            BestCrown = CrownType.Clear
        });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateChallengeCompeHandler(fixture);

        var response = await handler.Handle(new GetChallengeCompeQuery(GameEra.Red, 1), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        var challenge = Assert.Single(response.AryChallengeStat);
        Assert.Equal(1001u, challenge.CompeId);
        Assert.Equal(5, challenge.AryTrackStat.Count);
        var track = challenge.AryTrackStat.Single(track => track.Level == 1);
        Assert.Equal(101u, track.SongNo);
        Assert.Equal(1u, track.Level);
        Assert.Empty(track.OptionFlg);
        Assert.Equal(0u, track.StageMode);
        Assert.Equal(800000u, track.HighScore);
        Assert.Empty(response.AryUserCompeStat);
        Assert.Empty(response.AryBngCompeStat);

        var after = await fixture.Context.UserSaveDataRed.AsNoTracking().SingleAsync(row => row.Baid == 1);
        Assert.True(after.IsChallengeCompe);
        Assert.Equal(releaseBefore, after.ReleaseSongFlg);
        Assert.Equal(titleBefore, after.TitleFlg);
    }

    [Fact]
    public async Task GetChallengeCompeQuery_Red_ReturnsConfiguredActiveTracksBeforeAnyProgress()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            startsAt: null,
            endsAt: null,
            includeDefaultTaskTracks: false,
            rule: new Ac15ChallengeCompeRule(
                Ac15ChallengeCompeRuleKind.Clear,
                RequiredSongCount: 1,
                EligibleSongNoes: [101])));
        AddUser(fixture, enrolled: true);

        var response = await CreateChallengeCompeHandler(fixture)
            .Handle(new GetChallengeCompeQuery(GameEra.Red, 1), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        var challenge = Assert.Single(response.AryChallengeStat);
        Assert.Equal(1001u, challenge.CompeId);
        Assert.Equal([1u, 2u, 3u, 4u, 5u], challenge.AryTrackStat.Select(track => track.Level).ToArray());
        Assert.All(challenge.AryTrackStat, track =>
        {
            Assert.Equal(101u, track.SongNo);
            Assert.Empty(track.OptionFlg);
            Assert.Equal(0u, track.StageMode);
            Assert.Equal(0u, track.HighScore);
        });
        Assert.Empty(response.AryUserCompeStat);
        Assert.Empty(response.AryBngCompeStat);
        Assert.Empty(await fixture.Context.RedChallengeCompeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedChallengeCompeProgress.ToListAsync());
    }

    [Fact]
    public async Task GetChallengeCompeQuery_Red_DoesNotOptInOrEchoRawFactsWithoutSavedProgress()
    {
        await using var notEnrolledFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(startsAt: null, endsAt: null));
        AddUser(notEnrolledFixture, enrolled: false);
        var notEnrolledResponse = await CreateChallengeCompeHandler(notEnrolledFixture)
            .Handle(new GetChallengeCompeQuery(GameEra.Red, 1), CancellationToken.None);

        Assert.Empty(notEnrolledResponse.AryChallengeStat);
        Assert.False(await notEnrolledFixture.Context.UserSaveDataRed
            .Where(row => row.Baid == 1)
            .Select(row => row.IsChallengeCompe)
            .SingleAsync());

        await using var rawOnlyFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            startsAt: null,
            endsAt: null,
            includeDefaultTaskTracks: false,
            rule: new Ac15ChallengeCompeRule(Ac15ChallengeCompeRuleKind.Clear)));
        AddUser(rawOnlyFixture, enrolled: true);
        rawOnlyFixture.Context.RedChallengeCompeRawFacts.Add(new RedChallengeCompeRawFact
        {
            Baid = 1,
            BundleId = "red-2016-07",
            TaskId = 1001,
            Slot = 1,
            CompeId = 1001,
            TrackNo = 1,
            SongNo = 101,
            Level = 1,
            OptionFlg = [9],
            StageMode = 0,
            HighScore = 999999,
            PlayResult = 2,
            ProgressValue = 1,
            Completed = true,
            PlayTime = new DateTime(2016, 7, 20, 12, 0, 0),
            CreatedAt = new DateTime(2016, 7, 20, 12, 0, 0)
        });
        await rawOnlyFixture.Context.SaveChangesAsync();

        var rawOnlyResponse = await CreateChallengeCompeHandler(rawOnlyFixture)
            .Handle(new GetChallengeCompeQuery(GameEra.Red, 1), CancellationToken.None);

        Assert.Empty(rawOnlyResponse.AryChallengeStat);
        Assert.Empty(rawOnlyResponse.AryUserCompeStat);
        Assert.Empty(rawOnlyResponse.AryBngCompeStat);
    }

    [Fact]
    public async Task ChallengeCompeController_Red_ReturnsStatefulProgressThroughWireMapper()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            startsAt: null,
            endsAt: null,
            includeDefaultTaskTracks: false,
            rule: new Ac15ChallengeCompeRule(Ac15ChallengeCompeRuleKind.Clear)));
        AddUser(fixture, enrolled: true);
        fixture.Context.RedChallengeCompeProgress.Add(new RedChallengeCompeProgress
        {
            Baid = 1,
            BundleId = "red-2016-07",
            TaskId = 1001,
            Slot = 1,
            CompeId = 1001,
            TrackNo = 1,
            SongNo = 101,
            Level = 1,
            OptionFlg = [1, 2, 3],
            StageMode = 0,
            HighScore = 765432,
            ProgressValue = 1,
            Completed = true,
            UpdatedAt = new DateTime(2016, 7, 20, 12, 0, 0),
            CompletedAt = new DateTime(2016, 7, 20, 12, 0, 0)
        });
        await fixture.Context.SaveChangesAsync();
        var controller = new RedChallengeCompeController
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext(CreateServices(fixture)) }
        };

        var result = await controller.ChallengeCompe(new RedWire.ChallengeCompeRequest
        {
            Baid = 1,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123"
        });

        var response = Assert.IsType<RedWire.ChallengeCompeResponse>(Assert.IsType<OkObjectResult>(result).Value);
        var challenge = Assert.Single(response.AryChallengeStats);
        Assert.Equal(1001u, challenge.CompeId);
        var track = Assert.Single(challenge.AryTrackStats);
        Assert.Equal(101u, track.SongNo);
        Assert.Equal(1u, track.Level);
        Assert.Equal([1, 2, 3], track.OptionFlg);
        Assert.Equal(0u, track.StageMode);
        Assert.Equal(765432u, track.HighScore);
        Assert.Empty(response.AryUserCompeStats);
        Assert.Empty(response.AryBngCompeStats);
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
        string? startsAt = "2016-07-01T00:00:00Z",
        string? endsAt = "2016-08-01T00:00:00Z",
        string? activeBundleId = "red-2016-07",
        Ac15ChallengeCompeRule? rule = null,
        IReadOnlyList<Ac15ChallengeCompeReward>? rewards = null,
        bool includeDefaultTaskTracks = true)
    {
        var taskRule = rule ?? new Ac15ChallengeCompeRule(
            Ac15ChallengeCompeRuleKind.Clear,
            RequiredSongCount: 1,
            EligibleSongNoes: [101]);
        return new RedHandlerFixture.TestRedCatalog
        {
            ChallengeCompe = new Ac15ChallengeCompeCatalog(
                enabled,
                activeBundleId,
                [
                    new Ac15ChallengeCompeMonthlyBundle(
                        "red-2016-07",
                        startsAt is null ? null : DateTimeOffset.Parse(startsAt),
                        endsAt is null ? null : DateTimeOffset.Parse(endsAt),
                        Enumerable.Range(1, 10)
                            .Select(index => new Ac15ChallengeCompeTask(
                                (uint)(1000 + index),
                                (uint)index,
                                $"Task {index}",
                                index == 1
                                    ? taskRule
                                    : includeDefaultTaskTracks
                                        ? new Ac15ChallengeCompeRule(
                                            Ac15ChallengeCompeRuleKind.Clear,
                                            RequiredSongCount: 1,
                                            EligibleSongNoes: [(uint)(100 + index)])
                                        : new Ac15ChallengeCompeRule(Ac15ChallengeCompeRuleKind.Clear)))
                            .ToArray(),
                        null,
                        rewards ?? [])
                ])
        };
    }

    private static Ac15ChallengeCompeReward CreateReward(
        uint threshold = 1,
        IReadOnlyList<uint>? songs = null,
        IReadOnlyList<uint>? titles = null)
        => new(threshold, songs ?? [], titles ?? []);

    private static Ac15StageResult CreateStage(
        uint songNo,
        List<Ac15CompeIdFact> challengeIds,
        List<Ac15CompeIdFact>? userCompeIds = null,
        List<Ac15CompeIdFact>? bngCompeIds = null,
        uint level = 1)
        => new()
        {
            SongNo = songNo,
            Level = level,
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

    private static UserDataQueryHandler CreateUserDataHandler(RedHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

    private static GetChallengeCompeQueryHandler CreateChallengeCompeHandler(RedHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<GetChallengeCompeQueryHandler>.Instance);

    private static ServiceProvider CreateServices(RedHandlerFixture fixture)
        => new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddScoped<ITaikoDbContext>(_ => fixture.Context)
            .AddScoped<IGameDataCatalog>(_ => fixture.Catalog)
            .BuildServiceProvider();

    private static DefaultHttpContext CreateHttpContext(IServiceProvider? services = null)
    {
        services ??= new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        return new DefaultHttpContext { RequestServices = services };
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
