using TaikoLocalServer.Adapters.GameProtocol.White.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.White.Wire;

namespace TaikoLocalServer.Tests.White;

public sealed class WhiteRuntimeHandlerTests
{
    [Fact]
    public async Task AddMyDonEntry_White_CreatesSharedIdentityAndWhiteSaveOnly()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.White, "12345678901234567890", "WHITE", 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(1u, response.Baid);
        Assert.Equal("WHITE", response.MydonName);
        Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
        Assert.NotNull(await fixture.Context.UserData.FindAsync(1u));
        Assert.NotNull(await fixture.Context.Credentials.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataWhite.FindAsync(1u));
        Assert.Empty(await fixture.Context.UserSaveDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataRed.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataNijiiro.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UserDataQuery_White_ReadsWhiteRewardFavoritesRecentWithoutChallengeStats()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 3, MyDonName = "DON" });
        var save = UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData(3);
        save.TotalGetDonpoint = 120;
        save.TotalUseDonpoint = 30;
        save.RewardProgress = 8;
        save.DifficultyTutorialFlg = 2;
        save.IsDevil = true;
        save.IsExplain = true;
        fixture.Context.UserSaveDataWhite.Add(save);
        fixture.Context.WhiteFavoriteSongs.Add(new WhiteFavoriteSongs { Baid = 3, SongNo = 101 });
        fixture.Context.WhiteRecentSongs.Add(new WhiteRecentSongs { Baid = 3, SongNo = 102, LastPlayed = DateTime.UtcNow });
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new Ac15UserDataQuery(3, GameEra.White), CancellationToken.None);
        var wire = AssembleWhiteUserDataResponse(response);

        Assert.Equal(1u, response.Result);
        Assert.NotNull(response.Reward);
        Assert.Equal(120u, response.Reward.TotalGetDonpoint);
        Assert.Equal(30u, response.Reward.TotalUseDonpoint);
        Assert.Equal(8u, response.Reward.RewardProgress);
        Assert.Equal(2u, response.Tutorial!.DifficultyTutorialFlg);
        Assert.Null(response.Tutorial.TokkunTutorialFlg);
        Assert.Equal([101u], response.SongLists.AryFavoriteSongNoes);
        Assert.Equal([102u], response.SongLists.AryRecentSongNoes);
        Assert.True(response.ModeFlags!.IsDevil);
        Assert.True(response.ModeFlags.IsExplain);
        Assert.Equal(120u, wire.TotalGetDonpoint);
        Assert.Equal(30u, wire.TotalUseDonpoint);
        Assert.Equal(8u, wire.RewardProgress);
        Assert.Empty(wire.AryChallengeStats);
        Assert.Empty(wire.AryUserCompeStats);
        Assert.Empty(wire.AryBngCompeStats);
    }

    [Fact]
    public async Task UpdatePlayResult_White_SavesNormalPlayRewardAndOnlyWhiteRows()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataWhite.Add(UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData(1));
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        fixture.Context.UserSaveDataRed.Add(UserSaveDataRedExtensions.CreateDefaultRedSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var profile = Ac15ProfileMutationFacts.Empty with
        {
            GetDonpoint = 25,
            RewardPtn = 4,
            RewardProgress = 9,
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
            GameEra.White,
            playDatetime: "20260608120000",
            profile: profile,
            stages: [CreateStage(101, 1, 0)]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = Assert.Single(await fixture.Context.SongPlayDataWhite.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(101u, play.SongId);
        Assert.Equal(Difficulty.Easy, play.Difficulty);
        Assert.Equal(CrownType.Gold, play.Crown);
        Assert.True(play.IsFavorite);
        Assert.True(play.IsRecent);

        var best = await fixture.Context.SongBestDataWhite.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.NotNull(best);
        Assert.Equal(765432u, best!.BestScore);
        Assert.Single(await fixture.Context.WhiteFavoriteSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());
        Assert.Single(await fixture.Context.WhiteRecentSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());

        var save = await fixture.Context.UserSaveDataWhite.SingleAsync(row => row.Baid == 1);
        Assert.Equal(25u, save.TotalGetDonpoint);
        Assert.Equal(0u, save.TotalUseDonpoint);
        Assert.Equal(4u, save.RewardPtn);
        Assert.Equal(9u, save.RewardProgress);
        Assert.Equal(new DateTime(2026, 6, 8, 12, 0, 0), save.LastPlayDatetime);
        Assert.Equal(12u, save.PrevAreaCode);
        Assert.Equal(1u, save.Costume1);
        Assert.True(BitIsSet(save.ReleaseSongFlg, 104));
        Assert.True(BitIsSet(save.ToneFlg, 4));
        Assert.True(BitIsSet(save.CostumeFlg1, 1));
        Assert.True(BitIsSet(save.TitleFlg, 10));

        Assert.Empty(await fixture.Context.SongPlayDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataRed.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.RedDonChallengeRawFacts.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.RedDonChallengeProgress.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_White_DaniCreatesWhiteDanRows()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync(CreateDanCatalog(1));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataWhite.Add(UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var danStages = new List<Ac15StageResult>
        {
            CreateStage(101, 1, 0, score: 100000, playDan: 1, soulGauge: 55, comboCnt: 120, goodCnt: 100, okCnt: 20, ngCnt: 4),
            CreateStage(102, 1, 0, score: 200000, playDan: 1, soulGauge: 88, comboCnt: 220, goodCnt: 180, okCnt: 30, ngCnt: 2)
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.White,
            playMode: (uint)PlayMode.DanMode,
            stages: danStages,
            dani: new Ac15DaniPlayResult(
                DanResult: (uint)Ac15DanClearGrade.GoldClear,
                ComboCntTotal: 320,
                Stages: danStages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var dan = Assert.Single(await fixture.Context.DanScoreDataWhite
            .Include(row => row.DanStageScoreData)
            .Where(row => row.Baid == 1)
            .ToListAsync());
        Assert.Equal(1u, dan.DanId);
        Assert.False(dan.IsExtra);
        Assert.Equal(20001u, dan.MedleyUniqueId);
        Assert.Equal(Ac15DanClearGrade.GoldClear, dan.ClearGrade);
        Assert.Equal(2u, dan.ArrivalSongCount);
        Assert.Equal(88u, dan.SoulGaugeTotal);
        Assert.Equal(320u, dan.ComboCountTotal);

        var stages = dan.DanStageScoreData.OrderBy(row => row.StageIndex).ToArray();
        Assert.Equal([101u, 102u], stages.Select(stage => stage.SongNumber).ToArray());
        var save = await fixture.Context.UserSaveDataWhite.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1u, save.GotDanMax);
        Assert.Equal(2u, save.DispTaikojukuDan);
        Assert.Equal(Ac15DanClearGrade.GoldClear, Ac15DanHelpers.GetPackedGrade(save.GotDanFlg, 0));
        Assert.True(BitIsSet(save.CostumeFlg1, 36));
        Assert.Equal(36u, save.Costume1);
        Assert.Empty(await fixture.Context.DanScoreDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataRed.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public void PlayResultMapper_White_MapsNormalWirePayloadAndOmitsUnsupportedModeSections()
    {
        var request = CreateWireRequest(1);
        request.GetDonpoint = 10;
        request.RewardPtn = 4;
        request.RewardProgress = 9;
        request.GetToneNoes = [4];
        request.GetCostumeNo1s = [1];
        request.GetTitleNoes = [10];
        request.ReleaseSongNoes = [104];
        request.AryCurrentCostume = new PlayResultRequest.CostumeData
        {
            Costume1 = 1,
            Costume2 = 0,
            Costume3 = 0,
            Costume4 = 0,
            Costume5 = 0
        };
        var stage = CreateWireStage(101);
        stage.AryChallengeIds.Add(new PlayResultRequest.StageData.ResultcompeData { CompeId = 5, TrackNo = 1 });
        request.AryStageInfoes.Add(stage);

        var envelope = PlayResultMappers.Map(request);

        Assert.Equal(1u, envelope.Metadata.Baid);
        Assert.Equal("268410000000", envelope.Metadata.ChassisId);
        Assert.Equal("JPN0JPN0123", envelope.Metadata.ShopId);
        Assert.Equal("20260608120000", envelope.Metadata.PlayDatetime);
        Assert.Equal(10u, envelope.Profile.GetDonpoint);
        Assert.Equal(4u, envelope.Profile.RewardPtn);
        Assert.Equal(9u, envelope.Profile.RewardProgress);
        Assert.Equal([104u], envelope.Profile.ReleaseSongNoes);
        Assert.Equal([4u], envelope.Profile.GetToneNoes);
        Assert.Equal([1u], envelope.Profile.GetCostumeNo1s);
        Assert.Equal([10u], envelope.Profile.GetTitleNoes);
        Assert.True(envelope.Profile.HasAryCurrentCostume);
        Assert.False(envelope.Profile.HasDifficultyPlayedCourse);
        Assert.False(envelope.Profile.HasDifficultyPlayedStar);
        Assert.Equal(0u, envelope.Profile.GetDonmedal);
        Assert.Equal(0u, envelope.Profile.GetKatsumedal);
        Assert.Null(envelope.Tokkun);
        Assert.Null(envelope.BlueBattle);
        Assert.Null(envelope.GreenGhost);
        Assert.Null(envelope.DonChallenge);

        var mappedStage = Assert.Single(envelope.Normal!.Stages);
        Assert.Equal(101u, mappedStage.SongNo);
        Assert.Equal(765432u, mappedStage.PlayScore);
        Assert.Single(mappedStage.ChallengeIds);
    }

    private static UpdatePlayResultCommandHandler CreateHandler(WhiteHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

    private static UserDataResponse AssembleWhiteUserDataResponse(Ac15UserDataResponse common)
    {
        var response = new UserDataResponse
        {
            Result = common.Result
        };

        UserDataMappers.Apply(common.SongFlags, response);
        UserDataMappers.Apply(common.SongLists, response);
        UserDataMappers.Apply(common.Recommendations, response);
        UserDataMappers.Apply(common.Counters, response);
        UserDataMappers.Apply(common.Display, response);

        if (common.ModeFlags is { } modeFlags)
        {
            UserDataMappers.Apply(modeFlags, response);
        }

        if (common.Tutorial is { } tutorial)
        {
            UserDataMappers.Apply(tutorial, response);
        }

        if (common.Reward is { } reward)
        {
            UserDataMappers.Apply(reward, response);
        }

        return response;
    }

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
            Level = level,
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

    private static WhiteHandlerFixture.TestWhiteCatalog CreateDanCatalog(params uint[] challengeLevels)
        => new(taikojukuFileOrder: challengeLevels
            .Select((dan, index) => new Ac15TaikojukuEntry
            {
                UniqueId = 20001u + (uint)index,
                ChallengeLevel = dan,
                DanLevel = dan,
                Name = $"Dan {dan}",
                Songs =
                [
                    new Ac15TaikojukuSong { SongNo = 101, Level = 1 },
                    new Ac15TaikojukuSong { SongNo = 102, Level = 1 }
                ]
            })
            .ToArray());

    private static PlayResultRequest CreateWireRequest(uint baid)
        => new()
        {
            Baid = baid,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123",
            PlayDatetime = "20260608120000",
            IsRight = false,
            CardType = 1,
            IsTwoPlayers = false,
            GenderType = 0,
            PlayerAge = 0,
            PlayMode = (uint)PlayMode.Normal,
            AreaCode = 1,
            Reserved = []
        };

    private static PlayResultRequest.StageData CreateWireStage(uint songNo)
        => new()
        {
            SongNo = songNo,
            Level = 1,
            PlayResult = 2,
            PlayScore = 765432,
            GoodCnt = 100,
            OkCnt = 20,
            NgCnt = 3,
            PoundCnt = 4,
            ComboCnt = 120,
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            IsPapamama = false,
            PlayDan = 0,
            SoulGauge = 100,
            HitCnt = 123,
            SelectedFolderId = 9
        };

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
