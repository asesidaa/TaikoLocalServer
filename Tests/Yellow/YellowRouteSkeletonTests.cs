using TaikoLocalServer.Tests.Blue;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowRouteSkeletonTests
{
    private static readonly string[] ExpectedYellowGameRoutes =
    [
        "/v09r02/chassis/initialdatacheck.php",
        "/v09r02/chassis/tournamentcheck.php",
        "/v09r02/chassis/bookkeeping.php",
        "/v09r02/chassis/coinsetting.php",
        "/v09r02/chassis/gettelop.php",
        "/v09r02/chassis/getfolder.php",
        "/v09r02/chassis/taikojuku.php",
        "/v09r02/chassis/getitemshopinfo.php",
        "/v09r02/chassis/headclerk2.php",
        "/v09r02/chassis/playresult.php",
        "/v09r02/chassis/baidcheck.php",
        "/v09r02/chassis/mydonentry.php",
        "/v09r02/chassis/userdata.php",
        "/v09r02/chassis/challengecompe.php",
        "/v09r02/chassis/balancecheck.php",
        "/v09r02/chassis/banacoinpayment.php",
        "/v09r02/chassis/banacoinerrorlog.php",
        "/v09r02/chassis/getbanacoininfo.php",
        "/v09r02/chassis/crownsdata.php",
        "/v09r02/chassis/recommend.php",
        "/v09r02/chassis/selfbest.php",
        "/v09r02/chassis/heartbeat.php",
        "/v09r02/chassis/itempurchase.php",
        "/v09r02/chassis/rewardcardcheck.php",
        "/v09r02/chassis/rewardexecution.php"
    ];

    [Fact]
    public void YellowGameRoutes_AreOwnedByYellowAdapter()
    {
        var routes = ProtocolRouteTestHelper.FindPostRoutes(typeof(TaikoLocalServer.Adapters.GameProtocol.Yellow.DependencyInjection).Assembly)
            .Where(route => route.Template.StartsWith("/v09r02/chassis", StringComparison.OrdinalIgnoreCase))
            .Select(route => route.Template)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedYellowGameRoutes.Order(StringComparer.Ordinal).ToArray(), routes);
    }

    [Theory]
    [InlineData("/v09r02/chassis/getreitai.php")]
    [InlineData("/v09r02/chassis/startupauth.php")]
    [InlineData("/v09r02/chassis/verupauth.php")]
    [InlineData("/v09r02/chassis/verupcomplete.php")]
    [InlineData("/v09r02/chassis/battleuserdata.php")]
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

}
