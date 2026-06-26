namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroPlayResultHandlerTests
{
    [Fact]
    public async Task UpdatePlayResult_Momoiro_RedContractWritesNormalStateAndReadbackOnlyMomoiroRows()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(1, "11111111111111111111", "MOMO");
        await fixture.SeedMomoiroSaveAsync(1, totalGetDonpoint: 10, rewardPtn: 1, rewardProgress: 2);
        await fixture.SeedMomoiroFavoriteAsync(1, 300, 0);
        await fixture.SeedMomoiroFavoriteAsync(1, 101, 1);
        await SeedAdjacentStateAsync(fixture, 1);
        var unsupportedBefore = await fixture.CountSelectedAdjacentUnsupportedRowsAsync(1);
        var handler = CreateHandler(fixture);

        var profile = Ac15ProfileMutationFacts.Empty with
        {
            AreaCode = 12,
            GetDonpoint = 25,
            RewardPtn = 4,
            RewardProgress = 9,
            IsDevil = true,
            IsExplain = true,
            ReleaseSongNoes = [MomoiroHandlerFixture.HighSongNo]
        };

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            1,
            GameEra.Momoiro,
            playDatetime: "20260608120000",
            profile: profile,
            stages: [CreateStage(250, 1, 0)]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Equal(1, await fixture.CountMomoiroPlayRowsAsync(1));

        var best = await fixture.Context.SongBestDataMomoiro.FindAsync(1u, 250u, Difficulty.Easy, false);
        Assert.NotNull(best);
        Assert.Equal(765_432u, best!.BestScore);
        Assert.Equal(CrownType.Gold, best.BestCrown);

        var favorite = await fixture.Context.MomoiroFavoriteSongs.FindAsync(1u, 250u);
        Assert.NotNull(favorite);
        Assert.Equal(2, favorite!.DisplayOrder);
        Assert.Equal(1, await fixture.CountMomoiroRecentRowsAsync(1));

        var save = await fixture.Context.UserSaveDataMomoiro.SingleAsync(row => row.Baid == 1);
        Assert.Equal(35u, save.TotalGetDonpoint);
        Assert.Equal(0u, save.TotalUseDonpoint);
        Assert.Equal(4u, save.RewardPtn);
        Assert.Equal(9u, save.RewardProgress);
        Assert.Equal(12u, save.PrevAreaCode);
        Assert.True(save.IsDevil);
        Assert.True(save.IsExplain);
        Assert.True(BitIsSet(save.ReleaseSongFlg, MomoiroHandlerFixture.HighSongNo));
        Assert.Equal(1u, save.CategJpopCnt);
        Assert.Equal(1u, save.SongPushedCnt);
        Assert.Equal(1u, save.SongFavoriteCnt);
        Assert.Equal(1u, save.SongRecentCnt);

        var selfBest = await new GetSelfBestQueryHandler(
            fixture.Catalog,
            fixture.Context,
            NullLogger<GetSelfBestQueryHandler>.Instance)
            .Handle(new GetSelfBestQuery(1, GameEra.Momoiro, 1, [250]), CancellationToken.None);
        Assert.Equal(765_432u, selfBest.ArySelfbestScores.Single().SelfBestScore);

        var userData = await new UserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()))
            .Handle(new Ac15UserDataQuery(1, GameEra.Momoiro), CancellationToken.None);
        Assert.Equal([300u, 101u, 250u], userData.SongLists.AryFavoriteSongNoes);
        Assert.True(BitIsSetByOrdinal(userData.SongFlags.ReleaseSongFlg, MomoiroHandlerFixture.HighSongOrdinal));
        Assert.NotNull(userData.HashCrownFlg);
        Assert.Equal(
            Ac15ProtocolBytes.BuildCrownValue(
                Ac15CrownState.None,
                Ac15CrownState.None,
                Ac15CrownState.None,
                Ac15CrownState.FullCombo,
                Ac15CrownState.None),
            ReadTenBitValue(userData.HashCrownFlg!, 249));
        await AssertSelectedAdjacentUnsupportedRowsUnchangedAsync(fixture, 1, unsupportedBefore);
    }

    [Fact]
    public async Task UpdatePlayResult_Momoiro_RedContractDanPersistsBoundedRowsAndBaidReadbackWithoutTaikojukuState()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(2, "22222222222222222222", "DAN");
        await fixture.SeedMomoiroSaveAsync(2);
        var unsupportedBefore = await fixture.CountSelectedAdjacentUnsupportedRowsAsync(2);
        var handler = CreateHandler(fixture);
        var stages = new List<Ac15StageResult>
        {
            CreateStage(101, 1, 0, score: 100_000, playDan: 1, soulGauge: 55, comboCnt: 120, goodCnt: 100, okCnt: 20, ngCnt: 4),
            CreateStage(102, 1, 0, score: 200_000, playDan: 1, soulGauge: 88, comboCnt: 220, goodCnt: 180, okCnt: 30, ngCnt: 2)
        };

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            2,
            GameEra.Momoiro,
            playMode: (uint)PlayMode.DanMode,
            stages: stages,
            dani: new Ac15DaniPlayResult(
                DanResult: (uint)Ac15DanClearGrade.GoldClear,
                ComboCntTotal: 320,
                Stages: stages)),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Equal(1, await fixture.CountMomoiroDanRowsAsync(2));
        Assert.Equal(2, await fixture.CountMomoiroDanStageRowsAsync(2));

        var save = await fixture.Context.UserSaveDataMomoiro.SingleAsync(row => row.Baid == 2);
        Assert.Equal(1u, save.GotDanMax);
        Assert.NotEqual(0u, save.DispDanType);
        Assert.NotEqual(0u, save.DispTaikojukuDan);
        Assert.Equal(Ac15DanClearGrade.GoldClear, Ac15DanHelpers.GetPackedGrade(save.GotDanFlg, 0));

        var baid = await new BaidQueryHandler(
            fixture.Context,
            NullLogger<BaidQueryHandler>.Instance,
            fixture.Catalog)
            .Handle(new Ac15BaidQuery(GameEra.Momoiro, "22222222222222222222"), CancellationToken.None);
        Assert.NotNull(baid.DanStatus);
        Assert.Equal(1u, baid.DanStatus!.DispDanType);
        Assert.Equal(1u, baid.DanStatus.GotDanMax);
        Assert.Equal(Ac15DanClearGrade.GoldClear, Ac15DanHelpers.GetPackedGrade(baid.DanStatus.GotDanFlg, 0));
        await AssertSelectedAdjacentUnsupportedRowsUnchangedAsync(fixture, 2, unsupportedBefore);
    }

    [Fact]
    public async Task UpdatePlayResult_Momoiro_RedContractChallengeArraysAreAcceptedAndDropped()
    {
        await using var fixture = await MomoiroHandlerFixture.CreateAsync();
        await fixture.SeedSharedIdentityAsync(3, "33333333333333333333", "CHAL");
        await fixture.SeedMomoiroSaveAsync(3);
        var unsupportedBefore = await fixture.CountSelectedAdjacentUnsupportedRowsAsync(3);
        var handler = CreateHandler(fixture);
        var stage = CreateStage(101, 1, 0) with
        {
            ChallengeIds = [new Ac15CompeIdFact(42, 1)],
            UserCompeIds = [new Ac15CompeIdFact(43, 2)],
            BngCompeIds = [new Ac15CompeIdFact(44, 3)]
        };

        var result = await handler.Handle(Ac15PlayResultTestFactory.Command(
            3,
            GameEra.Momoiro,
            stages: [stage]),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Equal(1, await fixture.CountMomoiroPlayRowsAsync(3));
        Assert.Single(await fixture.Context.SongBestDataMomoiro.Where(row => row.Baid == 3).ToListAsync());
        await AssertSelectedAdjacentUnsupportedRowsUnchangedAsync(fixture, 3, unsupportedBefore);
    }

    private static UpdatePlayResultCommandHandler CreateHandler(MomoiroHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

    private static async Task SeedAdjacentStateAsync(MomoiroHandlerFixture fixture, uint baid)
    {
        fixture.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(baid));
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(baid));
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(baid));
        fixture.Context.UserSaveDataRed.Add(UserSaveDataRedExtensions.CreateDefaultRedSaveData(baid));
        fixture.Context.UserSaveDataWhite.Add(UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData(baid));
        fixture.Context.UserSaveDataMurasaki.Add(UserSaveDataMurasakiExtensions.CreateDefaultMurasakiSaveData(baid));
        fixture.Context.UserSaveDataKimidori.Add(UserSaveDataKimidoriExtensions.CreateDefaultKimidoriSaveData(baid));
        fixture.Context.SongPlayDataKimidori.Add(new SongPlayDatumKimidori
        {
            Baid = baid,
            SongId = 999,
            Difficulty = Difficulty.Easy,
            Crown = CrownType.Clear,
            OptionFlg = [],
            ToneFlg = [],
            PlayTime = DateTime.UnixEpoch
        });
        fixture.Context.DanScoreDataKimidori.Add(new DanScoreDatumKimidori
        {
            Baid = baid,
            DanId = 1,
            MedleyUniqueId = 99_001,
            ClearGrade = Ac15DanClearGrade.NormalClear
        });
        await fixture.Context.SaveChangesAsync();
    }

    private static async Task AssertSelectedAdjacentUnsupportedRowsUnchangedAsync(
        MomoiroHandlerFixture fixture,
        uint baid,
        IReadOnlyDictionary<string, int> before)
    {
        var after = await fixture.CountSelectedAdjacentUnsupportedRowsAsync(baid);
        Assert.Equal(before.OrderBy(pair => pair.Key), after.OrderBy(pair => pair.Key));
    }

    private static Ac15StageResult CreateStage(
        uint songNo,
        uint level,
        uint stageMode,
        uint score = 765_432,
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

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

    private static bool BitIsSetByOrdinal(byte[] source, int ordinal)
        => (source[ordinal >> 3] & (1 << (ordinal & 7))) != 0;

    private static ushort ReadTenBitValue(byte[] packed, int index)
    {
        ushort value = 0;
        var bitOffset = index * 10;
        for (var bit = 0; bit < 10; bit++)
        {
            var absoluteBit = bitOffset + bit;
            if ((packed[absoluteBit >> 3] & (1 << (absoluteBit & 7))) != 0)
            {
                value |= (ushort)(1 << bit);
            }
        }

        return value;
    }
}
