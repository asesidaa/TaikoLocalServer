using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;
using TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowWaiWaiTests
{
    [Fact]
    public void YellowProtoAndGeneratedWire_RecordNoWaiWaiTutorialSurface()
    {
        var root = FindRepoRoot();
        var proto = File.ReadAllText(Path.Combine(root, "proto", "yellow", "yellow.proto"));
        var wire = File.ReadAllText(Path.Combine(root, "Adapters.GameProtocol.Yellow", "Wire", "Game.cs"));

        Assert.DoesNotContain("waiwai_tutorial_flg", proto, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WaiwaiTutorialFlg", wire, StringComparison.Ordinal);
        Assert.Null(typeof(PlayResultRequest).GetProperty("WaiwaiTutorialFlg"));
        Assert.Null(typeof(UserDataResponse).GetProperty("WaiwaiTutorialFlg"));
    }

    [Fact]
    public void YellowUserDataMapper_DoesNotInventWaiWaiTutorialReadback()
    {
        var root = FindRepoRoot();
        var mapper = File.ReadAllText(Path.Combine(
            root,
            "Adapters.GameProtocol.Yellow",
            "Mappers",
            "UserDataMappers.cs"));

        Assert.DoesNotContain("WaiwaiTutorialFlg", mapper, StringComparison.Ordinal);
        Assert.Null(typeof(UserDataResponse).GetProperty("WaiwaiTutorialFlg"));
    }

    [Fact]
    public void YellowPlayResultMapper_MapsOnlyProtocolBackedWaiWaiStageFacts()
    {
        var root = FindRepoRoot();
        var proto = File.ReadAllText(Path.Combine(root, "proto", "yellow", "yellow.proto"));
        var request = CreateWireRequest(1);
        request.AryStageInfoes.Add(new PlayResultRequest.StageData
        {
            SongNo = 101,
            Level = 1,
            StageMode = 0,
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
            SelectedFolderId = 9
        });

        var common = PlayResultMappers.Map(request);

        var stage = Assert.Single(common.AryStageInfoes);
        if (proto.Contains("waiwai_result", StringComparison.OrdinalIgnoreCase))
        {
            Assert.True(stage.WaiwaiResult.HasValue);
        }
        else
        {
            Assert.Null(stage.WaiwaiResult);
        }

        if (proto.Contains("waiwai_gauge", StringComparison.OrdinalIgnoreCase))
        {
            Assert.True(stage.WaiwaiGauge.HasValue);
        }
        else
        {
            Assert.Null(stage.WaiwaiGauge);
        }
    }

    [Fact]
    public async Task YellowPlayResult_DoesNotPersistUnbackedWaiWaiTutorialFlag()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        save.WaiwaiTutorialFlg = 3;
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Yellow,
            new CommonPlayResultData
            {
                Baid = 1,
                WaiwaiTutorialFlg = 11,
                AryStageInfoes = [CreateStage(101, 1, 0)]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var reloaded = await fixture.Context.UserSaveDataYellow.SingleAsync(row => row.Baid == 1);
        Assert.Equal(3u, reloaded.WaiwaiTutorialFlg);
    }

    [Fact]
    public async Task YellowPlayResult_PreservesWaiWaiFactsOnlyAsPlayHistory()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateHandler(fixture);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Yellow,
            new CommonPlayResultData
            {
                Baid = 1,
                AryStageInfoes =
                [
                    CreateStage(101, 1, 0, waiwaiResult: 4, waiwaiGauge: 88)
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var play = Assert.Single(await fixture.Context.SongPlayDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(4u, play.WaiwaiResult);
        Assert.Equal(88u, play.WaiwaiGauge);

        var best = await fixture.Context.SongBestDataYellow.SingleAsync(row => row.Baid == 1);
        Assert.Equal(765432u, best.BestScore);
        var save = await fixture.Context.UserSaveDataYellow.SingleAsync(row => row.Baid == 1);
        Assert.Equal(0u, save.WaiwaiTutorialFlg);
        Assert.Empty(await fixture.Context.DanScoreDataYellow.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.YellowShopItemStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueTokkunStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueBattleStageResults.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.BlueShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
        Assert.Empty(await fixture.Context.GreenShopSeasonStates.Where(row => row.Baid == 1).ToListAsync());
    }

    [Fact]
    public void YellowWaiWaiSourceGuards_ForbidSpecialModeOrClassifier()
    {
        var root = FindRepoRoot();
        var yellowSources = Directory.EnumerateFiles(Path.Combine(root, "Application"), "*.Yellow.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "Adapters.GameProtocol.Yellow"), "*.cs", SearchOption.AllDirectories))
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "Domain"), "*.cs", SearchOption.AllDirectories))
            .Where(file => !file.EndsWith(Path.Combine("Wire", "Game.cs"), StringComparison.OrdinalIgnoreCase))
            .Select(File.ReadAllText)
            .Aggregate(string.Empty, string.Concat);

        Assert.DoesNotContain("PlayMode.WaiWai", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("PlayMode.Waiwai", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("HandleYellowWaiWai", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("HandleYellowWaiwai", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("IsYellowWaiWai", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("IsYellowWaiwai", yellowSources, StringComparison.Ordinal);
    }

    [Fact]
    public void YellowWaiWaiSourceGuards_KeepStageFactsDiagnosticAndNonAuthoritative()
    {
        var root = FindRepoRoot();
        var handler = File.ReadAllText(Path.Combine(root, "Application", "Handlers", "UpdatePlayResultCommand.Yellow.cs"));
        var adapter = File.ReadAllText(Path.Combine(root, "Application", "Ac15", "YellowAc15NormalPlayAdapter.cs"));
        var loggerMethod = ExtractMethodSource(handler, "LogYellowWaiWaiStageFacts");

        Assert.DoesNotContain("saveData.WaiwaiTutorialFlg =", handler, StringComparison.Ordinal);
        Assert.Contains("LogInformation", loggerMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", loggerMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("context.", loggerMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("DanScoreDataYellow", loggerMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("YellowShop", loggerMethod, StringComparison.Ordinal);
        Assert.Contains("WaiwaiResult = row.WaiwaiResult", adapter, StringComparison.Ordinal);
        Assert.Contains("WaiwaiGauge = row.WaiwaiGauge", adapter, StringComparison.Ordinal);
    }

    private static UpdatePlayResultCommandHandler CreateHandler(YellowHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

    private static CommonPlayResultData.StageData CreateStage(
        uint songNo,
        uint level,
        uint stageMode,
        uint waiwaiResult = 0,
        uint waiwaiGauge = 0)
        => new()
        {
            SongNo = songNo,
            Level = level,
            StageMode = stageMode,
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
            IsPushed = true,
            IsFavorite = true,
            IsRecent = true,
            SelectedFolderId = 9,
            SoulGauge = 100,
            WaiwaiResult = waiwaiResult,
            WaiwaiGauge = waiwaiGauge
        };

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
            BonusDailyFlg = false,
            BonusWeeklyFlg = false,
            BonusMonthlyFlg = false,
            GenderType = 0,
            PlayerAge = 0,
            PlayMode = (uint)PlayMode.Normal,
            AreaCode = 1,
            Reserved = new byte[16]
        };

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

    private static string ExtractMethodSource(string source, string methodName)
    {
        var start = source.IndexOf($"private void {methodName}(", StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException($"Could not find method {methodName}.");
        }

        var brace = source.IndexOf('{', start);
        var depth = 0;
        for (var i = brace; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source[start..(i + 1)];
                }
            }
        }

        throw new InvalidOperationException($"Could not extract method {methodName}.");
    }
}
