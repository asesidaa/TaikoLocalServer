using TaikoLocalServer.Tests.Blue;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowRouteSkeletonTests
{
    private static readonly string[] ExpectedYellowGameRoutes =
    [
        "/v09r00/chassis/initialdatacheck.php",
        "/v09r00/chassis/tournamentcheck.php",
        "/v09r00/chassis/bookkeeping.php",
        "/v09r00/chassis/coinsetting.php",
        "/v09r00/chassis/gettelop.php",
        "/v09r00/chassis/getfolder.php",
        "/v09r00/chassis/taikojuku.php",
        "/v09r00/chassis/getitemshopinfo.php",
        "/v09r00/chassis/headclerk2.php",
        "/v09r00/chassis/playresult.php",
        "/v09r00/chassis/baidcheck.php",
        "/v09r00/chassis/mydonentry.php",
        "/v09r00/chassis/userdata.php",
        "/v09r00/chassis/challengecompe.php",
        "/v09r00/chassis/balancecheck.php",
        "/v09r00/chassis/banacoinpayment.php",
        "/v09r00/chassis/banacoinerrorlog.php",
        "/v09r00/chassis/getbanacoininfo.php",
        "/v09r00/chassis/crownsdata.php",
        "/v09r00/chassis/recommend.php",
        "/v09r00/chassis/selfbest.php",
        "/v09r00/chassis/heartbeat.php",
        "/v09r00/chassis/itempurchase.php",
        "/v09r00/chassis/rewardcardcheck.php",
        "/v09r00/chassis/rewardexecution.php"
    ];

    [Fact]
    public void YellowGameRoutes_AreOwnedByYellowAdapter()
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Yellow.DependencyInjection).Assembly)
            .Where(route => route.Template.StartsWith("/v09r00/chassis", StringComparison.OrdinalIgnoreCase))
            .Select(route => route.Template)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedYellowGameRoutes.Order(StringComparer.Ordinal).ToArray(), routes);
    }

    [Theory]
    [InlineData("/v09r00/chassis/getreitai.php")]
    [InlineData("/v09r00/chassis/startupauth.php")]
    [InlineData("/v09r00/chassis/verupauth.php")]
    [InlineData("/v09r00/chassis/verupcomplete.php")]
    [InlineData("/v09r00/chassis/battleuserdata.php")]
    public void YellowAdapter_DoesNotOwnExcludedSharedOrBattleRoutes(string routeTemplate)
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Yellow.DependencyInjection).Assembly)
            .Where(route => route.Template == routeTemplate)
            .ToList();

        Assert.Empty(routes);
    }

    [Fact]
    public void YellowAdapter_OwnsNoBattleRoutes()
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Yellow.DependencyInjection).Assembly)
            .Where(route => route.Template.Contains("battle", StringComparison.OrdinalIgnoreCase))
            .Select(route => route.Template)
            .ToArray();

        Assert.Empty(routes);
    }

    [Fact]
    public void YellowControllers_OnlyCatalogMetadataRoutesCallRuntimeBusinessBehavior()
    {
        var root = FindRepoRoot();
        var controllersRoot = Path.Combine(root, "Adapters.GameProtocol.Yellow", "Controllers");
        var allowedMediatorCount = 12;

        foreach (var file in Directory.EnumerateFiles(controllersRoot, "*.cs", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(file);
            Assert.Equal(allowedMediatorCount, source.Split("Mediator.Send", StringSplitOptions.None).Length - 1);
            Assert.DoesNotContain("SaveChanges", source, StringComparison.Ordinal);
            Assert.DoesNotContain("BlueBattle", source, StringComparison.Ordinal);
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
