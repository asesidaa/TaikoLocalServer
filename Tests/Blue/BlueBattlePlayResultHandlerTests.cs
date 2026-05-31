namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattlePlayResultHandlerTests
{
    [Fact]
    public async Task UpdatePlayResult_Blue_BattlePayloadPersistsBlueBattleRowsOnly()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260528120000",
                PlayMode = 6,
                IsBattlePlayResult = true,
                BattleReleaseData = CreateReleaseData(assignNextStageId: 44),
                AryStageInfoes = [CreateBattleStage(9999, 99, 99, battleStageId: 33)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var stage = await fixture.Context.BlueBattleStageResults.SingleAsync(row => row.Baid == 1);
        Assert.Equal(9999u, stage.SongNo);
        Assert.Equal(99u, stage.Level);
        Assert.Equal(99u, stage.StageMode);
        Assert.Equal(33u, stage.BattleStageId);
        Assert.Equal(9u, stage.NpcId);
        Assert.Equal(12345u, stage.BossLife);
        Assert.Equal(888u, stage.TotalExp);
        Assert.Equal(77u, stage.AcquiredExp);
        Assert.Equal(456u, stage.Dpn);

        var userState = await fixture.Context.BlueBattleUserStates.SingleAsync(row => row.Baid == 1);
        Assert.True(BitIsSet(userState.ReleaseInfoFlg!, 101));
        Assert.True(BitIsSet(userState.ReleaseBattleStageFlg!, 2));
        Assert.Equal(33u, userState.LastBattleStageId);
        Assert.Equal(12345u, userState.LastBossLife);
        Assert.Equal(9u, userState.LastNpcId);
        Assert.Equal(44u, userState.AssignStageId);

        var npc = await fixture.Context.BlueBattleNpcStates.SingleAsync(row => row.Baid == 1 && row.NpcId == 9);
        Assert.Equal(888u, npc.TotalExp);
        Assert.Equal(456u, npc.MaxDpn);
        Assert.Equal(6u, npc.BondsLevel);
        Assert.Equal(30u, npc.NpcCostumeId);
        Assert.Equal(21u, npc.SelectedSpecialId1);
        Assert.Equal(22u, npc.SelectedSpecialId2);
        Assert.Equal(23u, npc.SelectedSpecialId3);
        Assert.True(BitIsSet(npc.NpcCostumeFlg!, 30));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 21));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 22));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 23));

        var token = await fixture.Context.BlueBattleTokenStates.SingleAsync(row => row.Baid == 1 && row.TokenId == 17);
        Assert.Equal(765u, token.TokenValue);

        var release = await fixture.Context.BlueBattleReleaseStates.SingleAsync(row => row.Baid == 1);
        Assert.Equal(101u, release.ReleaseInfoId);
        Assert.Equal(2u, release.ReleaseBattleStageId);
        Assert.Equal(4u, release.ReleaseNpcId);
        Assert.Equal(5u, release.ReleaseNpcCostumeId);
        Assert.Equal(6u, release.ReleaseNpcSpecialId);
        Assert.Equal(44u, release.AssignNextStageId);
        Assert.Equal(17u, release.TokenId);
        Assert.Equal(765u, release.TokenValue);

        await AssertNormalBlueStateEmptyAsync(fixture.Context);
        Assert.Empty(await fixture.Context.UserSaveDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_BattlePayloadEchoesNpcSelectedSpecialsThroughBattleUserData()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var playResultHandler = CreateHandler(fixture);

        var result = await playResultHandler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260528120000",
                PlayMode = 6,
                IsBattlePlayResult = true,
                AryStageInfoes = [CreateBattleStage(9999, 99, 99, battleStageId: 33)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var battleUserDataHandler = new GetBattleUserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<GetBattleUserDataQueryHandler>.Instance);
        var common = await battleUserDataHandler.Handle(new GetBattleUserDataQuery(1), CancellationToken.None);

        var npc = Assert.Single(common.NpcDatas);
        Assert.Equal(9u, npc.NpcId);
        Assert.Equal("888", npc.TotalExp);
        Assert.Equal(456u, npc.MaxDpn);
        Assert.Equal(30u, npc.NpcCostumeId);
        Assert.Equal([0, 0, 0, 64], npc.NpcCostumeFlg);
        Assert.Equal(21u, npc.LastSelectSpecial1);
        Assert.Equal(22u, npc.LastSelectSpecial2);
        Assert.Equal(23u, npc.LastSelectSpecial3);
        Assert.NotNull(npc.ReleaseSpecialFlg);
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 21));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 22));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 23));
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_BattlePayloadKeepsMaximumObservedDpn()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        await handler.Handle(new UpdatePlayResultCommand(
                1,
                GameEra.Blue,
                new CommonPlayResultData
                {
                    Baid = 1,
                    PlayDatetime = "20260528120000",
                    PlayMode = 6,
                    IsBattlePlayResult = true,
                    AryStageInfoes = [CreateBattleStage(9999, 99, 99, battleStageId: 33, dpn: 456)]
                }),
            CancellationToken.None);
        await handler.Handle(new UpdatePlayResultCommand(
                1,
                GameEra.Blue,
                new CommonPlayResultData
                {
                    Baid = 1,
                    PlayDatetime = "20260528130000",
                    PlayMode = 6,
                    IsBattlePlayResult = true,
                    AryStageInfoes = [CreateBattleStage(9999, 99, 99, battleStageId: 33, dpn: 123)]
                }),
            CancellationToken.None);

        var npc = await fixture.Context.BlueBattleNpcStates.SingleAsync(row => row.Baid == 1 && row.NpcId == 9);
        Assert.Equal(456u, npc.MaxDpn);

        var battleUserDataHandler = new GetBattleUserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<GetBattleUserDataQueryHandler>.Instance);
        var common = await battleUserDataHandler.Handle(new GetBattleUserDataQuery(1), CancellationToken.None);
        Assert.Equal(456u, Assert.Single(common.NpcDatas).MaxDpn);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_BattlePayloadLeavesExistingNormalStateUnchanged()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        var saveData = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        saveData.TotalGetDonmedal = 5;
        saveData.TotalGetKatsumedal = 7;
        saveData.CategJpopCnt = 3;
        saveData.SongPushedCnt = 4;
        saveData.LastPlayDatetime = new DateTime(2026, 5, 1, 8, 0, 0);
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataBlue.Add(saveData);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260528120000",
                PlayMode = (uint)PlayMode.DanMode,
                GetDonmedal = 50,
                GetKatsumedal = 60,
                ReleaseSongNoes = [104],
                GetToneNoes = [4],
                GetCostumeNo1s = [1],
                GetTitleNoes = [10],
                DanResult = 2,
                ComboCntTotal = 300,
                IsBattlePlayResult = true,
                BattleReleaseData = CreateReleaseData(assignNextStageId: 12),
                AryStageInfoes = [CreateBattleStage(101, 1, 0, battleStageId: 7)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var reloaded = await fixture.Context.UserSaveDataBlue.SingleAsync(row => row.Baid == 1);
        Assert.Equal(5u, reloaded.TotalGetDonmedal);
        Assert.Equal(7u, reloaded.TotalGetKatsumedal);
        Assert.Equal(3u, reloaded.CategJpopCnt);
        Assert.Equal(4u, reloaded.SongPushedCnt);
        Assert.Equal(new DateTime(2026, 5, 1, 8, 0, 0), reloaded.LastPlayDatetime);
        Assert.False(BitIsSet(reloaded.ReleaseSongFlg, 104));
        Assert.False(BitIsSet(reloaded.ToneFlg, 4));
        Assert.False(BitIsSet(reloaded.CostumeFlg1, 1));
        Assert.False(BitIsSet(reloaded.TitleFlg, 10));

        await AssertNormalBlueStateEmptyAsync(fixture.Context);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_ReleaseBattleDataStoresClientStateWithoutDerivedEffects()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Blue,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260528120000",
                IsBattlePlayResult = true,
                BattleReleaseData = new CommonPlayResultData.BattleReleaseDataDto
                {
                    ReleaseInfoIds = [12],
                    ReleaseBattleStageIds = [33],
                    ReleaseNpcIds = [99],
                    ReleaseNpcCostumeIds = [34],
                    ReleaseNpcSpecialIds = [35],
                    BattleTokenData =
                    [
                        new CommonPlayResultData.BattleTokenData
                        {
                            TokenId = 3,
                            TokenValue = 999
                        }
                    ],
                    AssignNextStageId = 33
                },
                AryStageInfoes = []
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var userState = await fixture.Context.BlueBattleUserStates.SingleAsync(row => row.Baid == 1);
        Assert.True(BitIsSet(userState.ReleaseInfoFlg!, 12));
        Assert.True(BitIsSet(userState.ReleaseBattleStageFlg!, 33));
        Assert.Equal(33u, userState.AssignStageId);
        Assert.Null(userState.LastBattleStageId);
        Assert.Null(userState.LastBossLife);
        Assert.Null(userState.LastNpcId);

        var token = await fixture.Context.BlueBattleTokenStates.SingleAsync(row => row.Baid == 1 && row.TokenId == 3);
        Assert.Equal(999u, token.TokenValue);
        var release = await fixture.Context.BlueBattleReleaseStates.SingleAsync(row => row.Baid == 1);
        Assert.Equal(99u, release.ReleaseNpcId);
        Assert.Equal(999u, release.TokenValue);

        Assert.Empty(await fixture.Context.BlueBattleStageResults.ToListAsync());
        await AssertNormalBlueStateEmptyAsync(fixture.Context);
        Assert.Empty(await fixture.Context.UserSaveDataBlue.ToListAsync());
    }

    private static UpdatePlayResultCommandHandler CreateHandler(BlueHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

    private static CommonPlayResultData.StageData CreateBattleStage(
        uint songNo,
        uint level,
        uint stageMode,
        uint battleStageId,
        uint dpn = 456)
        => new()
        {
            SongNo = songNo,
            Level = level,
            StageMode = stageMode,
            PlayResult = 2,
            PlayScore = 765432,
            GoodCnt = 100,
            OkCnt = 20,
            NgCnt = 3,
            PoundCnt = 4,
            ComboCnt = 120,
            HitCnt = 123,
            OptionFlg = [1, 2, 3],
            ToneFlg = [4],
            MusicCateg = 1,
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            SoulGauge = 100,
            BattleStageData = new CommonPlayResultData.BattleStageData
            {
                SupportLv = 3,
                BattleStageId = battleStageId,
                NpcData = new CommonPlayResultData.BattleNpcData
                {
                    NpcId = 9,
                    AcquiredExp = "77",
                    TotalExp = "888",
                    Dpn = dpn,
                    NpcCostumeId = 30,
                    SpecialId1 = 21,
                    SpecialId2 = 22,
                    SpecialId3 = 23,
                    BondsLv = 6
                },
                KillCnt = 5,
                BossLife = 12345,
                TotalDamage = 54321,
                CriticalCnt = 7,
                SpecialMoveCnt = 2
            }
        };

    private static CommonPlayResultData.BattleReleaseDataDto CreateReleaseData(uint assignNextStageId)
        => new()
        {
            ReleaseInfoIds = [101],
            ReleaseBattleStageIds = [2],
            ReleaseNpcIds = [4],
            ReleaseNpcCostumeIds = [5],
            ReleaseNpcSpecialIds = [6],
            BattleTokenData =
            [
                new CommonPlayResultData.BattleTokenData
                {
                    TokenId = 17,
                    TokenValue = 765
                }
            ],
            AssignNextStageId = assignNextStageId
        };

    private static async Task AssertNormalBlueStateEmptyAsync(TaikoDbContext context)
    {
        Assert.Empty(await context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await context.SongBestDataBlue.ToListAsync());
        Assert.Empty(await context.BlueRecentSongs.ToListAsync());
        Assert.Empty(await context.BlueFavoriteSongs.ToListAsync());
        Assert.Empty(await context.DanScoreDataBlue.ToListAsync());
        Assert.Empty(await context.DanStageScoreDataBlue.ToListAsync());
        Assert.Empty(await context.BlueShopSeasonStates.ToListAsync());
        Assert.Empty(await context.BlueShopItemStates.ToListAsync());
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
