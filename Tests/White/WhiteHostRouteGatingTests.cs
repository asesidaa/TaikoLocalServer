using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Shared;
using TaikoLocalServer.Domain.Enums;
using WhiteAdapter = TaikoLocalServer.Adapters.GameProtocol.White.DependencyInjection;

namespace TaikoLocalServer.Tests.White;

public sealed class WhiteHostRouteGatingTests
{
    private static readonly string[] ApprovedWhiteRoutes =
    [
        "/v07r00/chassis/baidcheck.php",
        "/v07r00/chassis/bookkeeping.php",
        "/v07r00/chassis/crownsdata.php",
        "/v07r00/chassis/getfolder.php",
        "/v07r00/chassis/gettelop.php",
        "/v07r00/chassis/heartbeat.php",
        "/v07r00/chassis/initialdatacheck.php",
        "/v07r00/chassis/mydonentry.php",
        "/v07r00/chassis/playresult.php",
        "/v07r00/chassis/recommend.php",
        "/v07r00/chassis/selfbest.php",
        "/v07r00/chassis/taikojuku.php",
        "/v07r00/chassis/tournamentcheck.php",
        "/v07r00/chassis/userdata.php"
    ];

    [Fact]
    public void EnabledWhiteApplicationPartExposesOnlyApprovedWhiteRoutes()
    {
        var routes = DiscoverWhiteRoutesFromHostSettings(whiteEnabled: true);

        Assert.Equal(ApprovedWhiteRoutes, routes);
        Assert.All(routes, route => Assert.StartsWith("/v07r00/chassis/", route, StringComparison.Ordinal));
        Assert.DoesNotContain("/v07r00/chassis/startupauth.php", routes);
        Assert.DoesNotContain("/v07r00/chassis/verupauth.php", routes);
        Assert.DoesNotContain("/v07r00/chassis/verupcomplete.php", routes);
        Assert.DoesNotContain("/v07r00/chassis/rewardexecution.php", routes);
        Assert.DoesNotContain("/v07r00/chassis/rewardcardcheck.php", routes);
        Assert.DoesNotContain("/v07r00/chassis/challengecompe.php", routes);
    }

    [Fact]
    public void DisabledWhiteApplicationPartRemovesWhiteRouteExposure()
    {
        var routes = DiscoverWhiteRoutesFromHostSettings(whiteEnabled: false);

        Assert.Empty(routes);
    }

    private static string[] DiscoverWhiteRoutesFromHostSettings(bool whiteEnabled)
    {
        var enabledEras = ReadEnabledErasFromHostSettings(whiteEnabled);
        var services = new ServiceCollection();
        services.AddLogging();
        services
            .AddControllers()
            .ConfigureApplicationPartManager(apm =>
            {
                apm.ApplicationParts.Add(new AssemblyPart(typeof(WhiteAdapter).Assembly));
                GameProtocolApplicationParts.RemoveDisabledGameProtocolApplicationParts(apm, enabledEras);
            });

        using var provider = services.BuildServiceProvider();
        var actionProvider = provider.GetRequiredService<IActionDescriptorCollectionProvider>();

        return actionProvider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Where(action => action.ControllerTypeInfo.Assembly == typeof(WhiteAdapter).Assembly)
            .Select(action => action.AttributeRouteInfo?.Template)
            .Where(route => !string.IsNullOrWhiteSpace(route))
            .Select(route => NormalizeRouteTemplate(route!))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string NormalizeRouteTemplate(string route)
        => route.StartsWith("/", StringComparison.Ordinal) ? route : $"/{route}";

    private static HashSet<GameEra> ReadEnabledErasFromHostSettings(bool whiteEnabled)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ServerSettings:Eras:White:Enabled"] = whiteEnabled.ToString()
            })
            .Build();

        return GameProtocolApplicationParts.ReadEnabledEras(configuration.GetSection("ServerSettings"));
    }
}
