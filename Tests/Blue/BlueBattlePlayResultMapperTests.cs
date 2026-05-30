using TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Blue.Wire;

namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattlePlayResultMapperTests
{
    [Fact]
    public void Map_TopLevelReleaseBattleData_ClassifiesBattlePlayResult()
    {
        var request = CreateRequest();
        request.AryReleaseBattledata = CreateReleaseBattleData();
        request.AryStageInfoes.Add(CreateStage(101, 1, 0));

        var common = PlayResultMappers.Map(request);

        Assert.True(common.IsBattlePlayResult);
        Assert.NotNull(common.BattleReleaseData);
        Assert.Equal(19u, common.BattleReleaseData.AssignNextStageId);
    }

    [Fact]
    public void Map_StageBattleData_ClassifiesBattlePlayResult()
    {
        var request = CreateRequest();
        request.AryStageInfoes.Add(CreateStage(101, 1, 7, CreateBattleStageData()));

        var common = PlayResultMappers.Map(request);

        Assert.True(common.IsBattlePlayResult);
        var stage = Assert.Single(common.AryStageInfoes);
        Assert.NotNull(stage.BattleStageData);
        Assert.Equal(7u, stage.StageMode);
        Assert.Equal(12u, stage.BattleStageData.BattleStageId);
    }

    [Fact]
    public void Map_BattleSections_PreservesRawClientReportedValues()
    {
        var request = CreateRequest();
        request.PlayMode = 6;
        request.AryReleaseBattledata = CreateReleaseBattleData();
        request.AryStageInfoes.Add(CreateStage(101, 4, 8, CreateBattleStageData()));

        var common = PlayResultMappers.Map(request);

        Assert.True(common.IsBattlePlayResult);
        Assert.Equal(6u, common.PlayMode);

        var stage = Assert.Single(common.AryStageInfoes);
        Assert.Equal(8u, stage.StageMode);
        var battleStage = Assert.NotNull(stage.BattleStageData);
        Assert.Equal(3u, battleStage.SupportLv);
        Assert.Equal(12u, battleStage.BattleStageId);
        Assert.Equal(5u, battleStage.KillCnt);
        Assert.Equal(12345u, battleStage.BossLife);
        Assert.Equal(54321u, battleStage.TotalDamage);
        Assert.Equal(7u, battleStage.CriticalCnt);
        Assert.Equal(2u, battleStage.SpecialMoveCnt);

        var npc = Assert.NotNull(battleStage.NpcData);
        Assert.Equal(9u, npc.NpcId);
        Assert.Equal("77", npc.AcquiredExp);
        Assert.Equal("888", npc.TotalExp);
        Assert.Equal(456u, npc.Dpn);
        Assert.Equal(33u, npc.NpcCostumeId);
        Assert.Equal(21u, npc.SpecialId1);
        Assert.Equal(22u, npc.SpecialId2);
        Assert.Equal(23u, npc.SpecialId3);
        Assert.Equal(6u, npc.BondsLv);

        var release = Assert.NotNull(common.BattleReleaseData);
        Assert.Equal([101u, 102u], release.ReleaseInfoIds);
        Assert.Equal([2u, 3u], release.ReleaseBattleStageIds);
        Assert.Equal([4u], release.ReleaseNpcIds);
        Assert.Equal([5u], release.ReleaseNpcCostumeIds);
        Assert.Equal([6u], release.ReleaseNpcSpecialIds);
        Assert.Equal(19u, release.AssignNextStageId);
        var token = Assert.Single(release.BattleTokenData);
        Assert.Equal(17u, token.TokenId);
        Assert.Equal(765u, token.TokenValue);
    }

    [Fact]
    public void MapperSources_DoNotReferenceGreenAiBattleTruth()
    {
        var root = FindRepoRoot();
        var files = new[]
        {
            Path.Combine(root, "Application", "Dtos", "CommonPlayResultData.BlueBattle.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Mappers", "PlayResultMappers.cs"),
            Path.Combine(root, "Tests", "Blue", "BlueBattlePlayResultMapperTests.cs")
        };
        var forbidden = new[]
        {
            "GreenAiBattle",
            "GreenStageModeInterpreter",
            "GreenGhost",
            "GreenGhostTokens",
            "GreenGhostWinnings",
            "Adapters.GameProtocol.Green",
            "StageMode == 3",
            "StageMode == 4"
        };

        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            foreach (var token in forbidden)
            {
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
            }
        }
    }

    private static PlayResultRequest CreateRequest() => new()
    {
        Baid = 1,
        ChassisId = "268410000000",
        ShopId = "JPN0JPN0123",
        PlayDatetime = "20260528120000",
        IsRight = false,
        CardType = 1,
        IsTwoPlayers = false,
        BonusDailyFlg = false,
        BonusWeeklyFlg = false,
        BonusMonthlyFlg = false,
        GetDonmedal = 0,
        GetKatsumedal = 0,
        GenderType = 0,
        PlayerAge = 0,
        PlayMode = 0,
        AreaCode = 1,
        Reserved = new byte[16]
    };

    private static PlayResultRequest.StageData CreateStage(
        uint songNo,
        uint level,
        uint stageMode,
        PlayResultRequest.StageData.BattleStageData? battleStageData = null)
        => new()
        {
            SongNo = songNo,
            Level = level,
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
            IsPapamama = false,
            StageMode = stageMode,
            SelectedFolderId = 9,
            SoulGauge = 100,
            WaiwaiResult = 0,
            WaiwaiGauge = 0,
            AryBattlestagedata = battleStageData
        };

    private static PlayResultRequest.StageData.BattleStageData CreateBattleStageData() => new()
    {
        SupportLv = 3,
        BattleStageId = 12,
        NpcData = new PlayResultRequest.StageData.BattleStageData.BattleNpcData
        {
            NpcId = 9,
            AcquiredExp = "77",
            TotalExp = "888",
            Dpn = 456,
            NpcCostumeId = 33,
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
    };

    private static PlayResultRequest.ReleaseBattleData CreateReleaseBattleData()
    {
        var release = new PlayResultRequest.ReleaseBattleData
        {
            ReleaseInfoIds = [101, 102],
            ReleaseBattleStageIds = [2, 3],
            ReleaseNpcIds = [4],
            ReleaseNpcCostumeIds = [5],
            ReleaseNpcSpecialIds = [6],
            AssignNextStageId = 19
        };
        release.AryBattletokendatas.Add(new PlayResultRequest.ReleaseBattleData.BattleTokenData
        {
            TokenId = 17,
            TokenValue = 765
        });

        return release;
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }
}
