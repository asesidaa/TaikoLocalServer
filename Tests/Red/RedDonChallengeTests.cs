using TaikoLocalServer.Application.Ac15.DonChallenge;
using TaikoLocalServer.Application;
using TaikoLocalServer.Tests.Ac15;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RedRewardCardCheckController = TaikoLocalServer.Adapters.GameProtocol.Red.Controllers.RewardCardCheckController;
using RedRewardExecutionController = TaikoLocalServer.Adapters.GameProtocol.Red.Controllers.RewardExecutionController;
using RedWire = TaikoLocalServer.Adapters.GameProtocol.Red.Wire;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedDonChallengeTests
{
    [Fact]
    public async Task UpdatePlayResult_Red_MatchedStagePersistsDonChallengeProgressOnlyInRedTables()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog());
        AddUser(fixture, protocolChallengeVisible: false, includeOtherEraSaves: true);
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult>
        {
            CreateStage(songNo: 101)
        };

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var raw = Assert.Single(await fixture.Context.RedDonChallengeRawFacts.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal("red-2016-07", raw.BundleId);
        Assert.Equal(1001u, raw.TaskId);
        Assert.Equal(1u, raw.Slot);
        Assert.Equal(1001u, raw.CompeId);
        Assert.Equal(1u, raw.TrackNo);
        Assert.Equal(101u, raw.SongNo);
        Assert.Equal(765432u, raw.HighScore);
        Assert.True(raw.Completed);

        var progress = Assert.Single(await fixture.Context.RedDonChallengeProgress.Where(row => row.Baid == 1).ToListAsync());
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
    public async Task UpdatePlayResult_Red_CompeBucketsDoNotDriveDonChallengeProgress()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog());
        AddUser(fixture, protocolChallengeVisible: true);
        var handler = CreateHandler(fixture);
        var nonMatchingSongWithMatchingCompeIds = new List<Ac15StageResult>
        {
            CreateStage(
                songNo: 999,
                challengeIds: [new Ac15CompeIdFact(1001, 1)],
                userCompeIds: [new Ac15CompeIdFact(1001, 1)],
                bngCompeIds: [new Ac15CompeIdFact(1001, 1)])
        };

        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: nonMatchingSongWithMatchingCompeIds),
            CancellationToken.None);

        Assert.Empty(await fixture.Context.RedDonChallengeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedDonChallengeProgress.ToListAsync());

        var matchingSongWithoutCompeIds = new List<Ac15StageResult>
        {
            CreateStage(songNo: 101)
        };

        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120100",
            stages: matchingSongWithoutCompeIds),
            CancellationToken.None);

        Assert.Single(await fixture.Context.RedDonChallengeRawFacts.ToListAsync());
        Assert.Single(await fixture.Context.RedDonChallengeProgress.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_DisabledOrNoActiveChallengeCatalogDoesNotCreateChallengeRows()
    {
        await using var disabledFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(enabled: false));
        AddUser(disabledFixture, protocolChallengeVisible: true);
        await RunMatchedChallengeAsync(disabledFixture);

        Assert.Empty(await disabledFixture.Context.RedDonChallengeRawFacts.ToListAsync());
        Assert.Empty(await disabledFixture.Context.RedDonChallengeProgress.ToListAsync());

        await using var inactiveFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(activeBundleId: null));
        AddUser(inactiveFixture, protocolChallengeVisible: true);
        await RunMatchedChallengeAsync(inactiveFixture);

        Assert.Empty(await inactiveFixture.Context.RedDonChallengeRawFacts.ToListAsync());
        Assert.Empty(await inactiveFixture.Context.RedDonChallengeProgress.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_ProtocolChallengeVisibilityFlagDoesNotGateDonChallengeRows()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog());
        AddUser(fixture, protocolChallengeVisible: false);

        await RunMatchedChallengeAsync(fixture);

        Assert.Single(await fixture.Context.RedDonChallengeRawFacts.ToListAsync());
        Assert.Single(await fixture.Context.RedDonChallengeProgress.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_DifficultyLimitedChallengeIgnoresLowerDifficulty()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rule: new Ac15DonChallengeRule(
            Ac15DonChallengeRuleKind.Clear,
            EligibleSongNoes: [101],
            MinimumLevel: 3)));
        AddUser(fixture, protocolChallengeVisible: true);
        var handler = CreateHandler(fixture);

        var normalStage = new List<Ac15StageResult>
        {
            CreateStage(
                songNo: 101,
                level: 2)
        };
        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: normalStage),
            CancellationToken.None);

        Assert.Empty(await fixture.Context.RedDonChallengeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedDonChallengeProgress.ToListAsync());

        var hardStage = new List<Ac15StageResult>
        {
            CreateStage(
                songNo: 101,
                level: 3)
        };
        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120100",
            stages: hardStage),
            CancellationToken.None);

        var progress = Assert.Single(await fixture.Context.RedDonChallengeProgress.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(3u, progress.Level);
        Assert.True(progress.Completed);
    }

    [Fact]
    public async Task UpdatePlayResult_Red_TokkunChallengeFactsDoNotCreateChallengeOrNormalRows()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog());
        AddUser(fixture, protocolChallengeVisible: true);
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult>
        {
            CreateStage(songNo: 101)
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
                    TokkunJumpCnt: 0))),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.RedDonChallengeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedDonChallengeProgress.ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataRed.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataRed.ToListAsync());
        Assert.Empty(await fixture.Context.RedFavoriteSongs.ToListAsync());
        Assert.Single(await fixture.Context.RedRecentSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataRed.ToListAsync());
        Assert.Equal(3u, await fixture.Context.UserSaveDataRed.Where(row => row.Baid == 1).Select(row => row.TokkunTutorialFlg).SingleAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_UnmatchedSongDoesNotCreateChallengeRowsEvenWhenCompeBucketsMatch()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog());
        AddUser(fixture, protocolChallengeVisible: true);
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult>
        {
            CreateStage(
                songNo: 999,
                challengeIds: [new Ac15CompeIdFact(1001, 1)],
                userCompeIds: [new Ac15CompeIdFact(1001, 1)],
                bngCompeIds: [new Ac15CompeIdFact(1001, 1)])
        };

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.RedDonChallengeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedDonChallengeProgress.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_ClearRequiredSongCountAccumulatesDistinctMatchedSongs()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rule: new Ac15DonChallengeRule(
            Ac15DonChallengeRuleKind.Clear,
            RequiredSongCount: 2,
            EligibleSongNoes: [101, 102, 103]),
            includeDefaultTaskTracks: false));
        AddUser(fixture, protocolChallengeVisible: true);
        var handler = CreateHandler(fixture);

        var firstStage = new List<Ac15StageResult>
        {
            CreateStage(songNo: 101)
        };
        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: firstStage),
            CancellationToken.None);

        var firstProgress = await fixture.Context.RedDonChallengeProgress.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1u, firstProgress.ProgressValue);
        Assert.False(firstProgress.Completed);

        var secondStage = new List<Ac15StageResult>
        {
            CreateStage(songNo: 102)
        };
        await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120100",
            stages: secondStage),
            CancellationToken.None);

        var progress = await fixture.Context.RedDonChallengeProgress.SingleAsync(row => row.Baid == 1);
        Assert.Equal(2u, progress.ProgressValue);
        Assert.True(progress.Completed);
        Assert.Equal(2, await fixture.Context.RedDonChallengeRawFacts.CountAsync(row => row.Baid == 1));
    }

    [Fact]
    public async Task UpdatePlayResult_Red_CompletedChallengeGrantsConfiguredSongRewardOnlyInRedSave()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rewards: [CreateReward(songs: [102])]));
        AddUser(fixture, protocolChallengeVisible: true, includeOtherEraSaves: true);
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult>
        {
            CreateStage(songNo: 101)
        };

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages),
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
        AddUser(fixture, protocolChallengeVisible: true);
        var handler = CreateHandler(fixture);
        var stages = Enumerable.Range(1, 10)
            .Select(index => CreateStage(
                songNo: (uint)(100 + index)))
            .ToList();

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var save = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.True(BitIsSet(save.TitleFlg, 10));
    }

    [Fact]
    public async Task UpdatePlayResult_Red_ReplayedChallengeRewardGrantIsIdempotent()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rewards: [CreateReward(songs: [102], titles: [10])]));
        AddUser(fixture, protocolChallengeVisible: true);
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult>
        {
            CreateStage(songNo: 101)
        };
        var command = Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages);

        await handler.Handle(command, CancellationToken.None);
        var firstSave = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        var firstRelease = firstSave.ReleaseSongFlg.ToArray();
        var firstTitles = firstSave.TitleFlg.ToArray();

        await handler.Handle(command, CancellationToken.None);

        var secondSave = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.Equal(firstRelease, secondSave.ReleaseSongFlg);
        Assert.Equal(firstTitles, secondSave.TitleFlg);
        Assert.Single(await fixture.Context.RedDonChallengeProgress.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_DisabledOrNoActiveChallengeCatalogDoesNotGrantRewards()
    {
        await using var disabledFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(enabled: false, rewards: [CreateReward(songs: [102], titles: [10])]));
        AddUser(disabledFixture, protocolChallengeVisible: true);
        await RunMatchedChallengeAsync(disabledFixture);
        var disabledSave = await disabledFixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.False(BitIsSet(disabledSave.ReleaseSongFlg, 102));
        Assert.False(BitIsSet(disabledSave.TitleFlg, 10));

        await using var inactiveFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            activeBundleId: null,
            rewards: [CreateReward(songs: [102], titles: [10])]));
        AddUser(inactiveFixture, protocolChallengeVisible: true);
        await RunMatchedChallengeAsync(inactiveFixture);
        var inactiveSave = await inactiveFixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.False(BitIsSet(inactiveSave.ReleaseSongFlg, 102));
        Assert.False(BitIsSet(inactiveSave.TitleFlg, 10));
    }

    [Fact]
    public async Task RewardCompatibilityRoutesDoNotGrantDonChallengeRewards()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(rewards: [CreateReward(songs: [102], titles: [10])]));
        AddUser(fixture, protocolChallengeVisible: true);
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
        Assert.Empty(await fixture.Context.RedDonChallengeRawFacts.ToListAsync());
        Assert.Empty(await fixture.Context.RedDonChallengeProgress.ToListAsync());
    }

    [Fact]
    public async Task UserDataQuery_Red_LocksActiveUnearnedChallengeRewardSongs()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            startsAt: null,
            endsAt: null,
            rewards: [CreateReward(songs: [102])]));
        AddUser(fixture, protocolChallengeVisible: true);
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
        AddUser(fixture, protocolChallengeVisible: true);
        var save = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, [102], Ac15EraProfiles.Red.Limits.SongFlagBytes);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateUserDataHandler(fixture);

        var response = await handler.Handle(new Ac15UserDataQuery(1, GameEra.Red), CancellationToken.None);

        Assert.True(BitIsSet(response.SongFlags.ReleaseSongFlg, 102));
    }

    [Fact]
    public async Task UserDataQuery_Red_DisabledOrInactiveChallengeDoesNotLockRewardSongs()
    {
        await using var disabledFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(enabled: false, rewards: [CreateReward(songs: [102])]));
        AddUser(disabledFixture, protocolChallengeVisible: true);
        var disabledResponse = await CreateUserDataHandler(disabledFixture).Handle(new Ac15UserDataQuery(1, GameEra.Red), CancellationToken.None);
        Assert.True(BitIsSet(disabledResponse.SongFlags.ReleaseSongFlg, 102));

        await using var inactiveFixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            activeBundleId: null,
            rewards: [CreateReward(songs: [102])]));
        AddUser(inactiveFixture, protocolChallengeVisible: true);
        var inactiveResponse = await CreateUserDataHandler(inactiveFixture).Handle(new Ac15UserDataQuery(1, GameEra.Red), CancellationToken.None);
        Assert.True(BitIsSet(inactiveResponse.SongFlags.ReleaseSongFlg, 102));

    }

    [Fact]
    public async Task UserDataQuery_Red_ProtocolChallengeVisibilityFlagDoesNotControlDonChallengeRewardLocks()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            startsAt: null,
            endsAt: null,
            rewards: [CreateReward(songs: [102])]));
        AddUser(fixture, protocolChallengeVisible: false);

        var response = await CreateUserDataHandler(fixture).Handle(new Ac15UserDataQuery(1, GameEra.Red), CancellationToken.None);

        Assert.False(BitIsSet(response.SongFlags.ReleaseSongFlg, 102));
    }

    private static async Task RunMatchedChallengeAsync(RedHandlerFixture fixture)
    {
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult>
        {
            CreateStage(songNo: 101)
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20160720120000",
            stages: stages),
            CancellationToken.None);

        Assert.Equal(1u, result);
    }

    private static void AddUser(
        RedHandlerFixture fixture,
        bool protocolChallengeVisible,
        bool includeOtherEraSaves = false)
    {
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.IsChallengeCompe = protocolChallengeVisible;
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
        Ac15DonChallengeRule? rule = null,
        Ac15DonChallengeRule? communityRule = null,
        IReadOnlyList<Ac15DonChallengeReward>? rewards = null,
        bool includeDefaultTaskTracks = true)
    {
        var taskRule = rule ?? new Ac15DonChallengeRule(
            Ac15DonChallengeRuleKind.Clear,
            RequiredSongCount: 1,
            EligibleSongNoes: [101]);
        return new RedHandlerFixture.TestRedCatalog
        {
            DonChallenge = new Ac15DonChallengeCatalog(
                enabled,
                activeBundleId,
                [
                    new Ac15DonChallengeMonthlyBundle(
                        "red-2016-07",
                        startsAt is null ? null : DateTimeOffset.Parse(startsAt),
                        endsAt is null ? null : DateTimeOffset.Parse(endsAt),
                        Enumerable.Range(1, 10)
                            .Select(index => new Ac15DonChallengeTask(
                                (uint)(1000 + index),
                                (uint)index,
                                $"Task {index}",
                                index == 1
                                    ? taskRule
                                    : includeDefaultTaskTracks
                                        ? new Ac15DonChallengeRule(
                                            Ac15DonChallengeRuleKind.Clear,
                                            RequiredSongCount: 1,
                                            EligibleSongNoes: [(uint)(100 + index)])
                                        : new Ac15DonChallengeRule(Ac15DonChallengeRuleKind.Clear)))
                            .ToArray(),
                        communityRule is null
                            ? null
                            : new Ac15DonChallengeCommunityTask(
                                1090,
                                "Community Task",
                                communityRule,
                                "Community task"),
                        rewards ?? [])
                ])
        };
    }

    private static Ac15DonChallengeReward CreateReward(
        uint threshold = 1,
        IReadOnlyList<uint>? songs = null,
        IReadOnlyList<uint>? titles = null)
        => new(threshold, songs ?? [], titles ?? []);

    private static Ac15StageResult CreateStage(
        uint songNo,
        List<Ac15CompeIdFact>? challengeIds = null,
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
            ChallengeIds = challengeIds ?? [],
            UserCompeIds = userCompeIds ?? [],
            BngCompeIds = bngCompeIds ?? []
        };

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
