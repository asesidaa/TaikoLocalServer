using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Murasaki;

public sealed class MurasakiRuntimeHandlerTests
{
    [Fact]
    public async Task AddMyDonEntry_Murasaki_CreatesSharedIdentityAndMurasakiSaveOnly()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync();
        var handler = new AddMyDonEntryCommandHandler(
            fixture.Context,
            NullLogger<AddMyDonEntryCommandHandler>.Instance);

        var response = await handler.Handle(
            new AddMyDonEntryCommand(GameEra.Murasaki, "12345678901234567890", "MURA", 0),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(1u, response.Baid);
        Assert.Equal("MURA", response.MydonName);
        Assert.NotNull(await fixture.Context.Cards.FindAsync("12345678901234567890"));
        Assert.NotNull(await fixture.Context.UserData.FindAsync(1u));
        Assert.NotNull(await fixture.Context.Credentials.FindAsync(1u));
        Assert.NotNull(await fixture.Context.UserSaveDataMurasaki.FindAsync(1u));
        Assert.Empty(await fixture.Context.UserSaveDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataRed.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataWhite.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.UserSaveDataNijiiro.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UserDataQuery_Murasaki_ReadsMurasakiRewardFavoritesAndRecentOnly()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 3, MyDonName = "DON" });
        var save = UserSaveDataMurasakiExtensions.CreateDefaultMurasakiSaveData(3);
        save.TotalGetDonpoint = 120;
        save.TotalUseDonpoint = 30;
        save.RewardProgress = 8;
        save.DifficultyTutorialFlg = 2;
        save.IsDevil = true;
        save.IsExplain = true;
        fixture.Context.UserSaveDataMurasaki.Add(save);
        fixture.Context.MurasakiFavoriteSongs.Add(new MurasakiFavoriteSongs { Baid = 3, SongNo = 101 });
        fixture.Context.MurasakiRecentSongs.Add(new MurasakiRecentSongs
        {
            Baid = 3,
            SongNo = 102,
            LastPlayed = new DateTime(2026, 6, 21, 12, 0, 0)
        });
        await fixture.Context.SaveChangesAsync();
        var handler = new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new Ac15UserDataQuery(3, GameEra.Murasaki), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.NotNull(response.Reward);
        Assert.Equal(120u, response.Reward.TotalGetDonpoint);
        Assert.Equal(30u, response.Reward.TotalUseDonpoint);
        Assert.Equal(8u, response.Reward.RewardProgress);
        Assert.NotNull(response.Tutorial);
        Assert.Null(response.Tutorial.TokkunTutorialFlg);
        Assert.Equal(2u, response.Tutorial.DifficultyTutorialFlg);
        Assert.Equal([101u], response.SongLists.AryFavoriteSongNoes);
        Assert.Equal([102u], response.SongLists.AryRecentSongNoes);
        Assert.True(response.ModeFlags!.IsDevil);
        Assert.True(response.ModeFlags.IsExplain);
    }

    [Fact]
    public async Task GetInitialDataQuery_Murasaki_ProvidesSplitMetadataCatalogRowsWithoutItemShopOrLegalTerms()
    {
        var catalog = new MurasakiHandlerFixture.TestMurasakiCatalog(taikojukuFileOrder:
        [
            new Ac15TaikojukuEntry
            {
                UniqueId = 20001,
                ChallengeLevel = 1,
                DanLevel = 1,
                VerupNo = 5,
                Name = "Dan 1",
                Songs = [new Ac15TaikojukuSong { SongNo = 101, Level = 1 }]
            }
        ])
        {
            EventFolders = new Dictionary<uint, EventFolderData>
            {
                [7] = new() { FolderId = 7, VerupNo = 2, SongNoes = [101, 102] }
            },
            Telops = new Dictionary<uint, Ac15TelopEntry>
            {
                [9] = new() { TelopId = 9, VerupNo = 3, Message = "Telop" }
            }
        };
        await using var fixture = await MurasakiHandlerFixture.CreateAsync(catalog);
        var handler = new GetInitialDataQueryHandler(
            fixture.Catalog,
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Murasaki), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(606u, response.SongHashVer);
        Assert.True(BitIsSet(response.DefaultSongFlg, 101));
        Assert.True(BitIsSet(response.DefaultSongFlg, 104));
        Assert.False(response.IsItemshop);
        Assert.Empty(response.AryItemShopDatas);
        Assert.Empty(response.AryLegaltermsDatas);
        Assert.Equal([(7u, 2u)], response.AryEventFolderDatas.Select(row => (row.InfoId, row.VerupNo)).ToArray());
        Assert.Equal([(9u, 3u)], response.AryTelopDatas.Select(row => (row.InfoId, row.VerupNo)).ToArray());
        Assert.Equal([(1u, 5u)], response.AryTaikojukuDatas.Select(row => (row.InfoId, row.VerupNo)).ToArray());
    }

    [Fact]
    public async Task UpdatePlayResult_Murasaki_SavesNormalPlayRewardAndOnlyMurasakiRows()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataMurasaki.Add(UserSaveDataMurasakiExtensions.CreateDefaultMurasakiSaveData(1));
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        fixture.Context.UserSaveDataRed.Add(UserSaveDataRedExtensions.CreateDefaultRedSaveData(1));
        fixture.Context.UserSaveDataWhite.Add(UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var profile = Ac15ProfileMutationFacts.Empty with
        {
            GetDonpoint = 25,
            RewardPtn = 4,
            RewardProgress = 9,
            IsDevil = true,
            IsExplain = true,
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
            GameEra.Murasaki,
            playDatetime: "20260608120000",
            profile: profile,
            stages: [CreateStage(101, 1, 0)]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = Assert.Single(await fixture.Context.SongPlayDataMurasaki.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(101u, play.SongId);
        Assert.Equal(Difficulty.Easy, play.Difficulty);
        Assert.Equal(CrownType.Gold, play.Crown);
        Assert.True(play.IsFavorite);
        Assert.True(play.IsRecent);

        var best = await fixture.Context.SongBestDataMurasaki.FindAsync(1u, 101u, Difficulty.Easy, false);
        Assert.NotNull(best);
        Assert.Equal(765432u, best!.BestScore);
        Assert.Single(await fixture.Context.MurasakiFavoriteSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());
        Assert.Single(await fixture.Context.MurasakiRecentSongs.Where(row => row.Baid == 1 && row.SongNo == 101).ToListAsync());

        var save = await fixture.Context.UserSaveDataMurasaki.SingleAsync(row => row.Baid == 1);
        Assert.Equal(25u, save.TotalGetDonpoint);
        Assert.Equal(0u, save.TotalUseDonpoint);
        Assert.Equal(4u, save.RewardPtn);
        Assert.Equal(9u, save.RewardProgress);
        Assert.True(save.IsDevil);
        Assert.True(save.IsExplain);
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
        Assert.Empty(await fixture.Context.SongPlayDataWhite.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.WhiteTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.RedDonChallengeRawFacts.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.RedDonChallengeProgress.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.WhiteDonChallengeRawFacts.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.WhiteDonChallengeProgress.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Murasaki_DaniCreatesMurasakiDanRows()
    {
        await using var fixture = await MurasakiHandlerFixture.CreateAsync(CreateDanCatalog(1));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataMurasaki.Add(UserSaveDataMurasakiExtensions.CreateDefaultMurasakiSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var danStages = new List<Ac15StageResult>
        {
            CreateStage(101, 1, 0, score: 100000, playDan: 1, soulGauge: 55, comboCnt: 120, goodCnt: 100, okCnt: 20, ngCnt: 4),
            CreateStage(102, 1, 0, score: 200000, playDan: 1, soulGauge: 88, comboCnt: 220, goodCnt: 180, okCnt: 30, ngCnt: 2)
        };
        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Murasaki,
            playMode: (uint)PlayMode.DanMode,
            stages: danStages,
            dani: new Ac15DaniPlayResult(
                DanResult: (uint)Ac15DanClearGrade.GoldClear,
                ComboCntTotal: 320,
                Stages: danStages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var dan = Assert.Single(await fixture.Context.DanScoreDataMurasaki
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
        var save = await fixture.Context.UserSaveDataMurasaki.SingleAsync(row => row.Baid == 1);
        Assert.Equal(1u, save.GotDanMax);
        Assert.Equal(2u, save.DispTaikojukuDan);
        Assert.Equal(Ac15DanClearGrade.GoldClear, Ac15DanHelpers.GetPackedGrade(save.GotDanFlg, 0));
        Assert.True(BitIsSet(save.CostumeFlg1, 36));
        Assert.Equal(36u, save.Costume1);
        Assert.Empty(await fixture.Context.DanScoreDataBlue.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataRed.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataWhite.Where(row => row.Baid == 1).ToListAsync());
    }

    private static UpdatePlayResultCommandHandler CreateHandler(MurasakiHandlerFixture fixture)
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

    private static MurasakiHandlerFixture.TestMurasakiCatalog CreateDanCatalog(params uint[] challengeLevels)
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

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
