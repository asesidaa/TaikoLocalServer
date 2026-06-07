using TaikoLocalServer.Tests.Blue;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowNoBattleSourceGuardTests
{
    private static readonly string[] ForbiddenBattleProtocolTokens =
    [
        "BattleUserDataRequest",
        "BattleUserDataResponse",
        "is_battleplay",
        "release_battle_stage_flg",
        "release_battle_special_flg",
        "battle_bonds_lv_cap",
        "BattleStageData",
        "ReleaseBattleData",
        "ary_battletokendata",
        "assign_next_stage_id"
    ];

    private static readonly string[] ForbiddenGeneratedBattleTokens =
    [
        "BattleUserDataRequest",
        "BattleUserDataResponse",
        "IsBattleplay",
        "ReleaseBattleStageFlg",
        "ReleaseBattleSpecialFlg",
        "BattleBondsLvCap",
        "BattleStageData",
        "ReleaseBattleData",
        "AryBattletokendata",
        "AssignNextStageId"
    ];

    private static readonly string[] ForbiddenYellowAdapterReferences =
    [
        "TaikoLocalServer.Adapters.GameProtocol.Blue",
        "BlueBattle",
        "HandleBlueBattle",
        "BlueBattleStageResult",
        "BlueBattleUserState",
        "BlueBattleNpcState",
        "BlueBattleTokenState",
        "GameEra.Blue",
        "battleuserdata.php"
    ];

    private static readonly string[] ForbiddenYellowPersistenceTokens =
    [
        "YellowBattle",
        "BattleUserDataYellow",
        "YellowBattleStage",
        "YellowBattleNpc",
        "YellowBattleToken"
    ];

    [Fact]
    public void YellowProto_DoesNotExposeBlueBattleMessagesOrFields()
    {
        var root = FindRepoRoot();
        var yellowProto = File.ReadAllText(Path.Combine(root, "proto", "yellow", "yellow.proto"));
        var blueProto = File.ReadAllText(Path.Combine(root, "proto", "blue", "taiko.proto"));

        foreach (var token in ForbiddenBattleProtocolTokens)
        {
            Assert.DoesNotContain(token, yellowProto, StringComparison.Ordinal);
            Assert.Contains(token, blueProto, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void YellowGeneratedWire_DoesNotExposeBlueBattleMessagesOrFields()
    {
        var root = FindRepoRoot();
        var yellowWire = File.ReadAllText(Path.Combine(
            root,
            "Adapters.GameProtocol.Yellow",
            "Wire",
            "Game.cs"));

        foreach (var token in ForbiddenGeneratedBattleTokens)
        {
            Assert.DoesNotContain(token, yellowWire, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void YellowRoutes_DoNotExposeBattleEndpoints()
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Yellow.DependencyInjection).Assembly)
            .Where(route => route.Template.Contains("battle", StringComparison.OrdinalIgnoreCase))
            .Select(route => route.Template)
            .ToArray();

        Assert.Empty(routes);
        Assert.DoesNotContain(
            routes,
            route => route.EndsWith("battleuserdata.php", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void YellowAdapter_DoesNotReferenceBlueBattleFallbackOrBlueProtocol()
    {
        var root = FindRepoRoot();
        var yellowAdapterRoot = Path.Combine(root, "Adapters.GameProtocol.Yellow");

        foreach (var file in Directory.EnumerateFiles(yellowAdapterRoot, "*.cs", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(file);
            foreach (var token in ForbiddenYellowAdapterReferences)
            {
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void YellowOwnedApplicationFiles_DoNotReferenceBlueBattleFallback()
    {
        var root = FindRepoRoot();
        var applicationRoot = Path.Combine(root, "Application");
        var yellowOwnedFiles = Directory.EnumerateFiles(applicationRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => Path.GetFileName(path).Contains("Yellow", StringComparison.OrdinalIgnoreCase)
                           || path.Contains($"{Path.DirectorySeparatorChar}Yellow{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var file in yellowOwnedFiles)
        {
            var source = File.ReadAllText(file);
            foreach (var token in ForbiddenYellowAdapterReferences)
            {
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void DomainInfrastructureAndMigrations_DoNotContainYellowBattlePersistence()
    {
        var root = FindRepoRoot();
        var searchedRoots = new[]
        {
            Path.Combine(root, "Domain", "Entities"),
            Path.Combine(root, "Infrastructure", "Persistence")
        };

        foreach (var file in searchedRoots.SelectMany(path => Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories)))
        {
            var source = File.ReadAllText(file);
            foreach (var token in ForbiddenYellowPersistenceTokens)
            {
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
            }
        }
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
