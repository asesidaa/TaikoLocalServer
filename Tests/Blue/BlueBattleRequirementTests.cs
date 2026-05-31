namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattleRequirementTests
{
    private static readonly string[] RuntimeRequirements =
    [
        "BTL-01",
        "BTL-02",
        "BTL-03",
        "BTL-04",
        "BTL-05",
        "BTL-06"
    ];

    [Fact]
    public void BlueBattleRuntimeRequirements_AreMarkedCompleteAndTraceableToPhase05()
    {
        var requirements = File.ReadAllText(FindRepoFile(".planning", "REQUIREMENTS.md"));

        foreach (var requirement in RuntimeRequirements)
        {
            Assert.Contains($"- [x] **{requirement}**", requirements, StringComparison.Ordinal);
            Assert.Contains($"| {requirement} | Phase 5 | Complete |", requirements, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void BlueBattleRuntimeRequirements_MapToFocusedRegressionCoverage()
    {
        var root = FindRepoRoot();

        AssertFileContains(
            root,
            Path.Combine("Tests", "Blue", "BlueBattlePersistenceTests.cs"),
            "BlueBattleUserStates",
            "SelectedSpecialId1",
            "SelectedSpecialId2",
            "SelectedSpecialId3",
            "CreatingBlueBattleState_DoesNotCreateNormalBlueOrGreenAiBattleRows",
            "UnresolvedBlueBattleFields_RemainNullOrAbsentUntilClientValuesAreStored");
        AssertFileContains(
            root,
            Path.Combine("Tests", "Blue", "BlueBattleUserDataTests.cs"),
            "Handle_PersistedCompleteNpcRows_EmitsNpcDatasWithSelectedSpecials",
            "BattleUserDataController_UsesMediatorQueryAndMapperInsteadOfLocalSuccessConstruction",
            "ShouldSerializeReleaseBattleStageFlg",
            "NpcDatas",
            "AryTokenDatas");
        AssertFileContains(
            root,
            Path.Combine("Tests", "Blue", "BlueInitialDataTests.cs"),
            "InitialData_Blue_AdvertisesParsedBattleCatalogValues",
            "ReleaseBattleStageFlg",
            "BattleBondsLvCap");
        AssertFileContains(
            root,
            Path.Combine("Tests", "Blue", "BlueBattlePlayResultMapperTests.cs"),
            "Map_BattleSections_PreservesRawClientReportedValues",
            "MapperSources_DoNotReferenceCrossEraBattleTruth");
        AssertFileContains(
            root,
            Path.Combine("Tests", "Blue", "BlueBattlePlayResultHandlerTests.cs"),
            "UpdatePlayResult_Blue_BattlePayloadEchoesNpcSelectedSpecialsThroughBattleUserData",
            "UpdatePlayResult_Blue_BattlePayloadLeavesExistingNormalStateUnchanged",
            "UpdatePlayResult_Blue_ReleaseBattleDataStoresClientStateWithoutDerivedEffects",
            "AssertNormalBlueStateEmptyAsync");
        AssertFileContains(
            root,
            Path.Combine("Tests", "Blue", "BlueBattleSourceGuardTests.cs"),
            "BlueBattleProductionCode_DoesNotDependOnGreenAiBattleOrOtherEraTruth",
            "DataDerivedInitialDataFields_AreBackedByExactResolutionApprovals");
    }

    [Fact]
    public void Btl05BlockedAndApprovedEffects_RemainBoundToResolutionRows()
    {
        var rows = ReadResolutionRows();

        AssertRowContains(rows, 18, "APPROVED_BY_USER_CLIENT_STATE_STORE_ECHO", "store/echo client-reported typed battle state only");
        AssertRowContains(rows, 19, "PROVEN_RELEASE_INFO_BITSET_DIFF", "battle-owned release info bitset");
        AssertRowContains(rows, 20, "PARTIAL_PROVEN_RELEASE_BITSET_DIFFS", "No normal unlock mirrors");
        AssertRowContains(rows, 21, "APPROVED_BY_USER_CLIENT_TOKEN_STORE_ECHO", "Do not grant, spend, threshold-check, or interpret token rewards");
        AssertRowContains(rows, 22, "APPROVED_BY_USER_ASSIGN_STAGE_ECHO", "Do not compute stage graph transitions");
        AssertRowContains(rows, 23, "DEFER_STAGE_EX_IMPLEMENTATION", "blocked except catalog/raw observation");
        AssertRowContains(rows, 24, "APPROVED_BY_USER_NO_SERVER_REWARD_TYPE_EFFECTS", "Do not implement reward type effects");
        AssertRowContains(rows, 26, "APPROVED_BY_USER_CLIENT_STATE_STORE_ECHO", "Do not compute boss completion");
    }

    [Fact]
    public void RuntimeVerificationRecord_ListsAutomatedGatesAndCabinetSmokeHandoff()
    {
        var verification = File.ReadAllText(FindRepoFile(
            ".planning",
            "phases",
            "05-blue-battle-runtime-support",
            "05-BATTLE-RUNTIME-VERIFICATION.md"));

        Assert.Contains("dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleSourceGuardTests", verification, StringComparison.Ordinal);
        Assert.Contains("dotnet test Tests/Tests.csproj --filter FullyQualifiedName~BlueBattleRequirementTests", verification, StringComparison.Ordinal);
        Assert.Contains("dotnet test Tests/Tests.csproj --filter BlueBattle", verification, StringComparison.Ordinal);
        Assert.Contains("dotnet test Tests/Tests.csproj", verification, StringComparison.Ordinal);
        Assert.Contains("dotnet build Host/Host.csproj -o \"$env:TEMP\\TaikoLocalServer-host-build", verification, StringComparison.Ordinal);
        Assert.Contains("FULL-01", verification, StringComparison.Ordinal);
        Assert.Contains("Cabinet/RPCS3 battle smoke was not performed in Phase 05", verification, StringComparison.Ordinal);
        Assert.Contains("2026-05-31 data-derived unlock-all decision", verification, StringComparison.Ordinal);
    }

    private static void AssertFileContains(string root, string relativePath, params string[] snippets)
    {
        var path = Path.Combine(root, relativePath);
        var source = File.ReadAllText(path);
        foreach (var snippet in snippets)
        {
            Assert.Contains(snippet, source, StringComparison.Ordinal);
        }
    }

    private static void AssertRowContains(
        IReadOnlyList<Dictionary<string, string>> rows,
        int rowNumber,
        params string[] snippets)
    {
        var row = Assert.Single(rows, candidate => candidate["#"] == rowNumber.ToString());
        var joined = string.Join(" ", row.Values);
        foreach (var snippet in snippets)
        {
            Assert.Contains(snippet, joined, StringComparison.Ordinal);
        }
    }

    private static IReadOnlyList<Dictionary<string, string>> ReadResolutionRows()
    {
        var path = FindRepoFile(".planning", "phases", "05-blue-battle-runtime-support", "05-RESOLUTION.md");
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

    private static string FindRepoFile(params string[] pathParts)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"Could not find {Path.Combine(pathParts)}.");
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
