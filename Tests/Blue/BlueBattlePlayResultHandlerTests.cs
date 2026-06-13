using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Tests.Ac15;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattlePlayResultHandlerTests
{
    [Fact]
    public async Task UpdatePlayResult_Blue_BattlePayloadPersistsBattleRowsAndAllowedPlaySummary()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(BattleCommand(
            1,
            playDatetime: "20260528120000",
            playMode: 6,
            release: CreateReleaseData(assignNextStageId: 44),
            stages: [CreateBattleStage(9999, 99, 99, battleStageId: 33)]),
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
        Assert.False(BitIsSet(npc.NpcCostumeFlg!, 30));
        Assert.True(BitIsSet(npc.NpcCostumeFlg!, 5));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 6));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 21));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 22));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 23));

        var token = await fixture.Context.BlueBattleTokenStates.SingleAsync(row => row.Baid == 1 && row.TokenId == 17);
        Assert.Equal(765u, token.TokenValue);

        await AssertBattleForbiddenNormalBlueStateEmptyAsync(fixture.Context);
        Assert.Empty(await fixture.Context.UserSaveDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_BattlePayloadAddsShopDonmedalsToActiveSeason()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync(CreateShopCatalog());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(BattleCommand(
            1,
            playDatetime: "20260528120000",
            getDonmedal: 50,
            release: CreateReleaseData(assignNextStageId: 12)),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var shopState = await fixture.Context.BlueShopSeasonStates.SingleAsync(row => row.Baid == 1 && row.SeasonId == 2);
        Assert.Equal(50u, shopState.TotalGetDonmedal);
        Assert.Equal(0u, shopState.TotalUseDonmedal);

        Assert.Empty(await fixture.Context.UserSaveDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.BlueRecentSongs.ToListAsync());
        Assert.Empty(await fixture.Context.BlueFavoriteSongs.ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.DanStageScoreDataBlue.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_BattlePayloadStoresRecentSongsWithoutFavoriteOrScoreRows()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(BattleCommand(
            1,
            playDatetime: "20260528120000",
            stages:
            [
                CreateBattleStage(201, 99, 99, battleStageId: 33),
                CreateBattleStage(202, 99, 99, battleStageId: 33)
            ]),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var recents = await fixture.Context.BlueRecentSongs
            .Where(row => row.Baid == 1)
            .OrderBy(row => row.SongNo)
            .ToListAsync();
        Assert.Collection(
            recents,
            recent =>
            {
                Assert.Equal(201u, recent.SongNo);
                Assert.Equal(new DateTime(2026, 5, 28, 12, 0, 0), recent.LastPlayed);
            },
            recent =>
            {
                Assert.Equal(202u, recent.SongNo);
                Assert.Equal(new DateTime(2026, 5, 28, 12, 0, 0), recent.LastPlayed);
            });

        Assert.Empty(await fixture.Context.UserSaveDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.BlueFavoriteSongs.ToListAsync());
        Assert.Empty(await fixture.Context.DanScoreDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.DanStageScoreDataBlue.ToListAsync());
        Assert.Empty(await fixture.Context.BlueShopSeasonStates.ToListAsync());
        Assert.Empty(await fixture.Context.BlueShopItemStates.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_BattlePayloadReadsBackPersistedNpcState()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var playResultHandler = CreateHandler(fixture);

        var result = await playResultHandler.Handle(BattleCommand(
            1,
            playDatetime: "20260528120000",
            playMode: 6,
            stages:
            [
                CreateBattleStage(
                    9999,
                    99,
                    99,
                    battleStageId: 33,
                    dpn: 34,
                    npcId: 0,
                    specialId1: 1,
                    specialId2: 1,
                    specialId3: 1)
            ]),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var battleUserDataHandler = new GetBattleUserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<GetBattleUserDataQueryHandler>.Instance);
        var common = await battleUserDataHandler.Handle(new GetBattleUserDataQuery(1), CancellationToken.None);

        var npc = Assert.Single(common.NpcDatas);
        Assert.Equal(0u, npc.NpcId);
        Assert.Equal("888", npc.TotalExp);
        Assert.Equal(34u, npc.MaxDpn);
        Assert.Equal(30u, npc.NpcCostumeId);
        Assert.True(BitIsSet(npc.NpcCostumeFlg, 30));
        Assert.Equal(1u, npc.LastSelectSpecial1);
        Assert.Equal(1u, npc.LastSelectSpecial2);
        Assert.Equal(1u, npc.LastSelectSpecial3);
        Assert.NotNull(npc.ReleaseSpecialFlg);
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, 1));
        Assert.True(BitIsSet(npc.ReleaseSpecialFlg!, BlueProtocolBytes.BattleNpcSpecialRowGateId));
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_BattlePayloadKeepsMaximumObservedDpn()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        await handler.Handle(BattleCommand(
                1,
                playDatetime: "20260528120000",
                playMode: 6,
                stages: [CreateBattleStage(9999, 99, 99, battleStageId: 33, dpn: 456)]),
            CancellationToken.None);
        await handler.Handle(BattleCommand(
                1,
                playDatetime: "20260528130000",
                playMode: 6,
                stages: [CreateBattleStage(9999, 99, 99, battleStageId: 33, dpn: 123)]),
            CancellationToken.None);

        var npc = await fixture.Context.BlueBattleNpcStates.SingleAsync(row => row.Baid == 1 && row.NpcId == 9);
        Assert.Equal(456u, npc.MaxDpn);

        var battleUserDataHandler = new GetBattleUserDataQueryHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<GetBattleUserDataQueryHandler>.Instance);
        var common = await battleUserDataHandler.Handle(new GetBattleUserDataQuery(1), CancellationToken.None);
        var persistedNpc = Assert.Single(common.NpcDatas);
        Assert.Equal(9u, persistedNpc.NpcId);
        Assert.Equal(456u, persistedNpc.MaxDpn);
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

        var stages = new List<Ac15StageResult>
        {
            CreateBattleStage(101, 1, 0, battleStageId: 7)
        };
        var result = await handler.Handle(BattleCommand(
            1,
            playDatetime: "20260528120000",
            playMode: (uint)PlayMode.DanMode,
            getDonmedal: 50,
            release: CreateReleaseData(assignNextStageId: 12),
            stages: stages),
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

        await AssertBattleForbiddenNormalBlueStateEmptyAsync(fixture.Context);
    }

    [Fact]
    public async Task UpdatePlayResult_Blue_ReleaseBattleDataStoresClientStateWithoutDerivedEffects()
    {
        await using var fixture = await BlueHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(BattleCommand(
            1,
            playDatetime: "20260528120000",
            release: new Ac15BlueBattleReleaseData
            {
                ReleaseInfoIds = [12],
                ReleaseBattleStageIds = [33],
                ReleaseNpcIds = [99],
                ReleaseNpcCostumeIds = [34],
                ReleaseNpcSpecialIds = [35],
                BattleTokenData = [new(TokenId: 3, TokenValue: 999)],
                AssignNextStageId = 33
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

        Assert.Empty(await fixture.Context.BlueBattleNpcStates.ToListAsync());
        Assert.Empty(await fixture.Context.BlueBattleStageResults.ToListAsync());
        await AssertBattleForbiddenNormalBlueStateEmptyAsync(fixture.Context);
        Assert.Empty(await fixture.Context.UserSaveDataBlue.ToListAsync());
    }

    private static BlueHandlerFixture.TestBlueCatalog CreateShopCatalog()
        => new(itemShopCatalog: new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 2,
            Seasons = new Dictionary<uint, BlueItemShopSeason>
            {
                [2] = new()
                {
                    SeasonId = 2,
                    VerupNo = 7,
                    Items =
                    [
                        new BlueItemShopEntry
                        {
                            ItemNo = 1,
                            ItemType = Ac15ShopItemType.Song,
                            ItemId = 101,
                            Price = 1300
                        }
                    ]
                }
            }
        });

    private static UpdatePlayResultCommandHandler CreateHandler(BlueHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

    private static UpdateAc15PlayResultCommand BattleCommand(
        uint baid,
        string playDatetime,
        uint playMode = 0,
        uint getDonmedal = 0,
        List<Ac15StageResult>? stages = null,
        Ac15BlueBattleReleaseData? release = null)
    {
        var battleStages = stages ?? [];
        return Ac15PlayResultTestFactory.Command(
            baid,
            GameEra.Blue,
            playMode: playMode,
            playDatetime: playDatetime,
            stages: battleStages,
            battle: new Ac15BlueBattlePlayResult(release, battleStages, getDonmedal));
    }

    private static Ac15StageResult CreateBattleStage(
        uint songNo,
        uint level,
        uint stageMode,
        uint battleStageId,
        uint dpn = 456,
        uint npcId = 9,
        uint specialId1 = 21,
        uint specialId2 = 22,
        uint specialId3 = 23)
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
            BlueBattleStage = new Ac15BlueBattleStageData
            {
                SupportLv = 3,
                BattleStageId = battleStageId,
                NpcData = new Ac15BlueBattleNpcData
                {
                    NpcId = npcId,
                    AcquiredExp = "77",
                    TotalExp = "888",
                    Dpn = dpn,
                    NpcCostumeId = 30,
                    SpecialId1 = specialId1,
                    SpecialId2 = specialId2,
                    SpecialId3 = specialId3,
                    BondsLv = 6
                },
                KillCnt = 5,
                BossLife = 12345,
                TotalDamage = 54321,
                CriticalCnt = 7,
                SpecialMoveCnt = 2
            }
        };

    private static Ac15BlueBattleReleaseData CreateReleaseData(uint assignNextStageId)
        => new()
        {
            ReleaseInfoIds = [101],
            ReleaseBattleStageIds = [2],
            ReleaseNpcIds = [4],
            ReleaseNpcCostumeIds = [5],
            ReleaseNpcSpecialIds = [6],
            BattleTokenData =
            [
                new(TokenId: 17, TokenValue: 765)
            ],
            AssignNextStageId = assignNextStageId
        };

    private static async Task AssertBattleForbiddenNormalBlueStateEmptyAsync(TaikoDbContext context)
    {
        Assert.Empty(await context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await context.SongBestDataBlue.ToListAsync());
        Assert.Empty(await context.BlueFavoriteSongs.ToListAsync());
        Assert.Empty(await context.DanScoreDataBlue.ToListAsync());
        Assert.Empty(await context.DanStageScoreDataBlue.ToListAsync());
        Assert.Empty(await context.BlueShopItemStates.ToListAsync());
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;
}
