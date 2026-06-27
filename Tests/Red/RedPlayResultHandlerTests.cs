using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Tests.Ac15;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedPlayResultHandlerTests
{
    [Fact]
    public async Task UpdatePlayResult_Red_SavesNormalPlayDonPointsAndOnlyRedRows()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataRed.Add(UserSaveDataRedExtensions.CreateDefaultRedSaveData(1));
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var profile = Ac15ProfileMutationFacts.Empty with
        {
            GetDonpoint = 25,
            RewardPtn = 4,
            RewardProgress = 9,
            DifficultyTutorialFlg = 2,
            IsDevil = true,
            IsExplain = true,
            DifficultyPlayedCourse = 4,
            DifficultyPlayedStar = 8,
            HasDifficultyPlayedCourse = true,
            HasDifficultyPlayedStar = true,
            ReleaseSongNoes = [104],
            GetToneNoes = [4],
            GetCostumeNo1s = [1],
            GetTitleNoes = [10],
            HasAryCurrentCostume = true,
            AryCurrentCostume = new Ac15CostumeFacts(1, 0, 0, 0, 0),
            AreaCode = 12
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playDatetime: "20260608120000",
            profile: profile,
            stages: [CreateStage(101, 1, 0)]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = Assert.Single(await fixture.Context.SongPlayDataRed.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(101u, play.SongId);
        Assert.Equal(Difficulty.Easy, play.Difficulty);
        Assert.Equal(CrownType.Gold, play.Crown);
        Assert.True(play.IsFavorite);
        Assert.True(play.IsRecent);

        var best = await fixture.Context.SongBestDataRed.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.NotNull(best);
        Assert.Equal(765432u, best!.BestScore);
        Assert.Single(await fixture.Context.RedFavoriteSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());
        Assert.Single(await fixture.Context.RedRecentSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());

        var save = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.Equal(25u, save.TotalGetDonpoint);
        Assert.Equal(0u, save.TotalUseDonpoint);
        Assert.Equal(4u, save.RewardPtn);
        Assert.Equal(9u, save.RewardProgress);
        Assert.Equal(2u, save.DifficultyTutorialFlg);
        Assert.True(save.IsDevil);
        Assert.True(save.IsExplain);
        Assert.Equal(4u, save.DifficultyPlayedCourse);
        Assert.Equal(8u, save.DifficultyPlayedStar);
        Assert.Equal(new DateTime(2026, 6, 8, 12, 0, 0), save.LastPlayDatetime);
        Assert.Equal(12u, save.PrevAreaCode);
        Assert.True(BitIsSet(save.ReleaseSongFlg, 104));
        Assert.True(BitIsSet(save.ToneFlg, 4));
        Assert.True(BitIsSet(save.CostumeFlg1, 1));
        Assert.True(BitIsSet(save.TitleFlg, 10));

        Assert.Empty(await fixture.Context.SongPlayDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_NormalUploadWithDefaultTokkunTutorialFlagStillSavesNormalPlay()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var saveData = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        saveData.TokkunTutorialFlg = 5;
        fixture.Context.UserSaveDataRed.Add(saveData);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playMode: (uint)PlayMode.Normal,
            profile: Ac15ProfileMutationFacts.Empty with
            {
                GetDonpoint = 1050,
                DifficultyTutorialFlg = 1,
                DifficultyPlayedCourse = 1,
                DifficultyPlayedStar = 5,
                HasDifficultyPlayedCourse = true,
                HasDifficultyPlayedStar = true
            },
            stages: [CreateStage(574, 1, 0, score: 261880)],
            tokkun: new Ac15TokkunPlayResult(TutorialFlg: 0, StageData: null)),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var play = Assert.Single(await fixture.Context.SongPlayDataRed.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(574u, play.SongId);
        Assert.Equal(261880u, play.Score);

        var best = await fixture.Context.SongBestDataRed.FindAsync(1u, 574u, Difficulty.Easy, false);
        Assert.NotNull(best);
        Assert.Equal(261880u, best!.BestScore);
        Assert.Single(await fixture.Context.RedRecentSongs.Where(row => row.Baid == 1 && row.SongNo == 574).ToListAsync());

        var reloaded = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1050u, reloaded.TotalGetDonpoint);
        Assert.Equal(5u, reloaded.TokkunTutorialFlg);
        Assert.Equal(1u, reloaded.DifficultyTutorialFlg);
        Assert.Equal(1u, reloaded.DifficultyPlayedCourse);
        Assert.Equal(5u, reloaded.DifficultyPlayedStar);
    }

    [Fact]
    public async Task UpdatePlayResult_Red_DaniCreatesRedBestAndStageRows()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateDanCatalog(1));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataRed.Add(UserSaveDataRedExtensions.CreateDefaultRedSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var danStages = new List<Ac15StageResult>
        {
            CreateStage(101, 1, 0, score: 100000, playDan: 1, soulGauge: 55, comboCnt: 120, goodCnt: 100, okCnt: 20, ngCnt: 4),
            CreateStage(102, 1, 0, score: 200000, playDan: 1, soulGauge: 88, comboCnt: 220, goodCnt: 180, okCnt: 30, ngCnt: 2)
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playMode: (uint)PlayMode.DanMode,
            playDatetime: "20260608120000",
            stages: danStages,
            dani: new Ac15DaniPlayResult(
                DanResult: (uint)Ac15DanClearGrade.GoldClear,
                ComboCntTotal: 0,
                Stages: danStages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var dan = Assert.Single(await fixture.Context.DanScoreDataRed
            .Include(row => row.DanStageScoreData)
            .Where(row => row.Baid == 1)
            .ToListAsync());
        Assert.Equal(1u, dan.DanId);
        Assert.Equal(Ac15DanClearGrade.GoldClear, dan.ClearGrade);
        Assert.Equal(2u, dan.ArrivalSongCount);
        Assert.Equal(88u, dan.SoulGaugeTotal);

        var stages = dan.DanStageScoreData.OrderBy(row => row.StageIndex).ToArray();
        Assert.Equal([101u, 102u], stages.Select(stage => stage.SongNumber).ToArray());
        var save = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1u, save.GotDanMax);
        Assert.Equal(2u, save.DispTaikojukuDan);
        Assert.Equal(Ac15DanClearGrade.GoldClear, Ac15DanHelpers.GetPackedGrade(save.GotDanFlg, 0));
        Assert.True(BitIsSet(save.CostumeFlg1, 36));
    }

    [Fact]
    public async Task UpdatePlayResult_Red_TokkunUpdatesTutorialAndRecentsOnly()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var saveData = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        saveData.TotalGetDonpoint = 5;
        fixture.Context.UserSaveDataRed.Add(saveData);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playMode: (uint)PlayMode.Tokkun,
            tokkun: new Ac15TokkunPlayResult(
                TutorialFlg: 7,
                StageData: new Ac15TokkunStageData(
                    BanacoinDatetime: "20260608120100",
                    TokkunSongCnt: 2,
                    TookunSongnoes: [101, 101],
                    TokkunSpeedchangeCnt: 3,
                    TokkunAutoplayCnt: 4,
                    TokkunJumpCnt: 5)),
            profile: Ac15ProfileMutationFacts.Empty with
            {
                GetDonpoint = 50,
                ReleaseSongNoes = [104],
                GetToneNoes = [4]
            },
            stages: [CreateStage(101, 1, 0)]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var reloaded = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.Equal(5u, reloaded.TotalGetDonpoint);
        Assert.Equal(7u, reloaded.TokkunTutorialFlg);
        Assert.False(BitIsSet(reloaded.ReleaseSongFlg, 104));
        Assert.False(BitIsSet(reloaded.ToneFlg, 4));
        Assert.Empty(await fixture.Context.SongPlayDataRed.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataRed.ToListAsync());
        Assert.Empty(await fixture.Context.RedFavoriteSongs.ToListAsync());
        var recent = Assert.Single(await fixture.Context.RedRecentSongs.ToListAsync());
        Assert.Equal(101u, recent.SongNo);
        Assert.Empty(await fixture.Context.DanScoreDataRed.ToListAsync());
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Red_TokkunZeroTutorialFlagDoesNotClearExistingState()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var saveData = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        saveData.TokkunTutorialFlg = 7;
        fixture.Context.UserSaveDataRed.Add(saveData);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playMode: (uint)PlayMode.Tokkun,
            tokkun: new Ac15TokkunPlayResult(TutorialFlg: 0, StageData: null)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var reloaded = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.Equal(7u, reloaded.TokkunTutorialFlg);
    }

    [Fact]
    public async Task UpdatePlayResult_Red_TokkunZeroTutorialFlagStoresZeroWhenUnset()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataRed.Add(UserSaveDataRedExtensions.CreateDefaultRedSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playMode: (uint)PlayMode.Tokkun,
            tokkun: new Ac15TokkunPlayResult(TutorialFlg: 0, StageData: null)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var reloaded = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.Equal(0u, reloaded.TokkunTutorialFlg);
    }

    [Fact]
    public async Task UpdatePlayResult_Red_NormalUploadWithTokkunStageFactsStillSavesNormalPlay()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var saveData = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        saveData.TotalGetDonpoint = 5;
        fixture.Context.UserSaveDataRed.Add(saveData);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Red,
            playMode: (uint)PlayMode.Normal,
            tokkun: new Ac15TokkunPlayResult(
                TutorialFlg: 7,
                StageData: new Ac15TokkunStageData(
                    BanacoinDatetime: "20260608120100",
                    TokkunSongCnt: 2,
                    TookunSongnoes: [101, 101],
                    TokkunSpeedchangeCnt: 3,
                    TokkunAutoplayCnt: 4,
                    TokkunJumpCnt: 5)),
            profile: Ac15ProfileMutationFacts.Empty with
            {
                GetDonpoint = 50,
                ReleaseSongNoes = [104],
                GetToneNoes = [4]
            },
            stages: [CreateStage(101, 1, 0)]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var reloaded = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        Assert.Equal(55u, reloaded.TotalGetDonpoint);
        Assert.Null(reloaded.TokkunTutorialFlg);
        Assert.True(BitIsSet(reloaded.ReleaseSongFlg, 104));
        Assert.True(BitIsSet(reloaded.ToneFlg, 4));
        Assert.Single(await fixture.Context.SongPlayDataRed.ToListAsync());
        Assert.Single(await fixture.Context.SongBestDataRed.ToListAsync());
        Assert.Single(await fixture.Context.RedFavoriteSongs.ToListAsync());
        Assert.Single(await fixture.Context.RedRecentSongs.ToListAsync());
    }

    private static UpdatePlayResultCommandHandler CreateHandler(RedHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

    private static Ac15StageResult CreateStage(
        uint songNo,
        uint level,
        uint stageMode,
        uint score = 765432,
        uint playDan = 0,
        uint soulGauge = 100,
        uint comboCnt = 120,
        uint goodCnt = 100,
        uint okCnt = 20,
        uint ngCnt = 3)
        => new()
        {
            SongNo = songNo,
            Level = Ac15Difficulty.FromProtocol(level),
            StageMode = stageMode,
            PlayResult = 2,
            PlayScore = score,
            ScoreRate = 95,
            GoodCnt = goodCnt,
            OkCnt = okCnt,
            NgCnt = ngCnt,
            PoundCnt = 4,
            ComboCnt = comboCnt,
            HitCnt = 123,
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            SelectedFolderId = 9,
            PlayDan = playDan == 0 ? null : playDan,
            SoulGauge = soulGauge
        };

    private static RedHandlerFixture.TestRedCatalog CreateDanCatalog(params uint[] challengeLevels)
        => new(taikojukuFileOrder: challengeLevels
            .Select((dan, index) => new Ac15TaikojukuEntry
            {
                UniqueId = 20001u + (uint)index,
                ChallengeLevel = dan,
                DanLevel = dan,
                Name = $"Dan {dan}",
                Songs =
                [
                    new Ac15TaikojukuSong { SongNo = 101, Level = Difficulty.Easy },
                    new Ac15TaikojukuSong { SongNo = 102, Level = Difficulty.Easy }
                ]
            })
            .ToArray());

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
