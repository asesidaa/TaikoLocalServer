namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattleSourceGuardTests
{
    private static readonly string[] Phase05BattleProductionFiles =
    [
        Path.Combine("Application", "Abstractions", "IBlueCatalog.cs"),
        Path.Combine("Application", "Abstractions", "ITaikoDbContext.Blue.cs"),
        Path.Combine("Application", "Catalog", "Blue", "BlueBattleCatalog.cs"),
        Path.Combine("Application", "Common", "BlueBattleStateExtensions.cs"),
        Path.Combine("Application", "Dtos", "CommonBattleUserDataResponse.cs"),
        Path.Combine("Application", "Dtos", "CommonInitialDataCheckResponse.Blue.cs"),
        Path.Combine("Application", "Dtos", "CommonPlayResultData.BlueBattle.cs"),
        Path.Combine("Application", "Handlers", "GetBattleUserDataQuery.Blue.cs"),
        Path.Combine("Application", "Handlers", "GetInitialDataQuery.Blue.cs"),
        Path.Combine("Application", "Handlers", "UpdatePlayResultCommand.Blue.cs"),
        Path.Combine("Application", "Handlers", "UpdatePlayResultCommand.BlueBattle.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Controllers", "BattleUserDataController.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Controllers", "InitialDataCheckController.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Controllers", "PlayResultController.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Mappers", "BattleUserDataMappers.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Mappers", "InitialDataMappers.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Mappers", "PlayResultMappers.cs"),
        Path.Combine("Domain", "Entities", "BlueBattleUserState.cs"),
        Path.Combine("Domain", "Entities", "BlueBattleNpcState.cs"),
        Path.Combine("Domain", "Entities", "BlueBattleTokenState.cs"),
        Path.Combine("Domain", "Entities", "BlueBattleStageResult.cs"),
        Path.Combine("Domain", "Entities", "BlueBattleReleaseState.cs"),
        Path.Combine("Infrastructure", "GameDataCatalog", "Blue", "BlueBattleDataLoader.cs"),
        Path.Combine("Infrastructure", "GameDataCatalog", "Blue", "BlueEraGameDataCatalog.cs"),
        Path.Combine("Infrastructure", "GameDataCatalog", "Blue", "BlueGameDataPaths.cs"),
        Path.Combine("Infrastructure", "Persistence", "TaikoDbContext.Blue.cs"),
        Path.Combine("Infrastructure", "Persistence", "Migrations", "20260530185853_AddBlueBattleState.cs")
    ];

    private static readonly string[] BattleStateMutationFiles =
    [
        Path.Combine("Application", "Common", "BlueBattleStateExtensions.cs"),
        Path.Combine("Application", "Dtos", "CommonBattleUserDataResponse.cs"),
        Path.Combine("Application", "Dtos", "CommonPlayResultData.BlueBattle.cs"),
        Path.Combine("Application", "Handlers", "GetBattleUserDataQuery.Blue.cs"),
        Path.Combine("Application", "Handlers", "UpdatePlayResultCommand.BlueBattle.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Mappers", "BattleUserDataMappers.cs")
    ];

    private static readonly string[] RuntimeConsumerFiles =
    [
        Path.Combine("Application", "Handlers", "GetBattleUserDataQuery.Blue.cs"),
        Path.Combine("Application", "Handlers", "GetInitialDataQuery.Blue.cs"),
        Path.Combine("Application", "Handlers", "UpdatePlayResultCommand.Blue.cs"),
        Path.Combine("Application", "Handlers", "UpdatePlayResultCommand.BlueBattle.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Controllers", "BattleUserDataController.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Controllers", "InitialDataCheckController.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Controllers", "PlayResultController.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Mappers", "BattleUserDataMappers.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Mappers", "InitialDataMappers.cs"),
        Path.Combine("Adapters.GameProtocol.Blue", "Mappers", "PlayResultMappers.cs")
    ];

    private static readonly string[] ForbiddenCrossEraBattleTruth =
    [
        "GreenAiBattle",
        "GreenStageModeInterpreter",
        "GreenGhost",
        "GhostStageData",
        "Adapters.GameProtocol.Green",
        "UserSaveDataGreen",
        "SongPlayDataGreen",
        "SongBestDataGreen",
        "DanScoreDataGreen",
        "GreenGhostTokens",
        "GreenGhostWinnings",
        "AiScoreDatum",
        "UpdateAiBattleData",
        "FunctionIdAiBattleAvailable",
        "GameEra.Green",
        "GameEra.Nijiiro"
    ];

    private static readonly string[] ForbiddenNormalBlueBattleEffects =
    [
        "ApplyUnlockBits",
        "SaveBlueStageAsync",
        "UpsertBestAsync",
        "UpsertBlueFavoriteAndRecentAsync",
        "SaveBlueDanAsync",
        "BlueProfileCounters.ApplyStage",
        "ReleaseSongNoes",
        "GetToneNoes",
        "GetCostumeNo",
        "GetTitleNoes",
        "BlueShopSeasonStates",
        "BlueShopItemStates",
        "SongPlayDataBlue",
        "SongBestDataBlue",
        "DanScoreDataBlue",
        "DanStageScoreDataBlue",
        "BlueRecentSongs",
        "BlueFavoriteSongs"
    ];

    [Fact]
    public void BlueBattleProductionCode_DoesNotDependOnGreenAiBattleOrOtherEraTruth()
    {
        var root = FindRepoRoot();

        foreach (var file in ExistingFiles(root, Phase05BattleProductionFiles))
        {
            var source = File.ReadAllText(file);
            foreach (var forbidden in ForbiddenCrossEraBattleTruth)
            {
                Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void BlueBattleStateMutationCode_DoesNotWriteNormalBlueScoreShopDaniOrUnlockState()
    {
        var root = FindRepoRoot();

        foreach (var file in ExistingFiles(root, BattleStateMutationFiles))
        {
            var source = File.ReadAllText(file);
            foreach (var forbidden in ForbiddenNormalBlueBattleEffects)
            {
                Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void BlueBattleRuntimeConsumers_DoNotDeriveBehaviorFromXmlRowCountsOrUnapprovedConstants()
    {
        var root = FindRepoRoot();
        var forbidden = new[]
        {
            "RowCount",
            "ElementCount",
            "stage 33",
            "Stage 33",
            "BattleBondsLvCap = 65",
            "BattleBondsLvCap=65",
            "0xFE",
            "0x07",
            "reward type",
            "token reward",
            "threshold",
            "boss-life default",
            "boss life default",
            "normal unlock"
        };

        foreach (var file in ExistingFiles(root, RuntimeConsumerFiles))
        {
            var source = File.ReadAllText(file);
            foreach (var token in forbidden)
            {
                Assert.DoesNotContain(token, source, StringComparison.OrdinalIgnoreCase);
            }

            Assert.DoesNotMatch(@"(?<![A-Za-z0-9_])33(?![A-Za-z0-9_])", source);
            Assert.DoesNotMatch(@"(?<![A-Za-z0-9_])65(?![A-Za-z0-9_])", source);
        }
    }

    [Fact]
    public void DataDerivedInitialDataFields_AreBackedByExactResolutionApprovals()
    {
        var root = FindRepoRoot();
        var rows = ReadResolutionRows(root);
        AssertResolutionApproves(
            rows,
            "InitialdatacheckResponse.release_battle_stage_flg",
            "APPROVED_BY_USER_DATA_DERIVED_INITIALDATA",
            "parsed `battlestageinfo.xml` stage IDs");
        AssertResolutionApproves(
            rows,
            "InitialdatacheckResponse.release_battle_special_flg",
            "APPROVED_BY_USER_DATA_DERIVED_INITIALDATA",
            "parsed battle token reward IDs");
        AssertResolutionApproves(
            rows,
            "InitialdatacheckResponse.battle_bonds_lv_cap",
            "APPROVED_BY_USER_DATA_DERIVED_INITIALDATA",
            "parsed NPC progression cap");

        var initialDataSource = File.ReadAllText(Path.Combine(
            root,
            "Application",
            "Handlers",
            "GetInitialDataQuery.Blue.cs"));
        Assert.Contains("battle.ReleaseBattleStageIds", initialDataSource, StringComparison.Ordinal);
        Assert.Contains("battle.ReleaseBattleSpecialIds", initialDataSource, StringComparison.Ordinal);
        Assert.Contains("battle.BattleBondsLvCap", initialDataSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BattleBondsLvCap = 65", initialDataSource, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"(?<![A-Za-z0-9_])33(?![A-Za-z0-9_])", initialDataSource);
    }

    private static void AssertResolutionApproves(
        IReadOnlyList<Dictionary<string, string>> rows,
        string rowName,
        string expectedStatus,
        string expectedRuntimeUse)
    {
        var row = Assert.Single(rows, candidate =>
            candidate["Phase 4 Missing-Evidence Row"].Contains(rowName, StringComparison.Ordinal));

        Assert.Contains(expectedStatus, row["Phase 5 Status"], StringComparison.Ordinal);
        Assert.Contains(expectedRuntimeUse, row["Runtime Use"], StringComparison.Ordinal);
        Assert.Contains("Latest user decision on 2026-05-31", row["Evidence Source"], StringComparison.Ordinal);
    }

    private static IEnumerable<string> ExistingFiles(string root, IEnumerable<string> relativePaths)
    {
        foreach (var relativePath in relativePaths)
        {
            var path = Path.Combine(root, relativePath);
            if (File.Exists(path))
            {
                yield return path;
            }
        }
    }

    private static IReadOnlyList<Dictionary<string, string>> ReadResolutionRows(string root)
    {
        var path = Path.Combine(root, ".planning", "phases", "05-blue-battle-runtime-support", "05-RESOLUTION.md");
        var lines = File.ReadAllLines(path);
        for (var i = 0; i < lines.Length - 1; i++)
        {
            if (!lines[i].Contains("Phase 4 Missing-Evidence Row", StringComparison.Ordinal))
            {
                continue;
            }

            var headers = SplitCells(lines[i]);
            var rows = new List<Dictionary<string, string>>();
            for (var rowIndex = i + 2; rowIndex < lines.Length; rowIndex++)
            {
                var line = lines[rowIndex];
                if (!line.StartsWith('|') || !line.EndsWith('|'))
                {
                    break;
                }

                var cells = SplitCells(line);
                Assert.Equal(headers.Length, cells.Length);
                rows.Add(headers.Zip(cells).ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal));
            }

            return rows;
        }

        throw new InvalidOperationException($"Could not find resolution matrix in {path}.");
    }

    private static string[] SplitCells(string line)
        => line.Trim().Trim('|').Split('|', StringSplitOptions.TrimEntries);

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
