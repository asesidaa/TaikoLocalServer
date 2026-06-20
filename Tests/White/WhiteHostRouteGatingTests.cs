using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.White;
using TaikoLocalServer.Adapters.GameProtocol.Shared;
using TaikoLocalServer.Domain.Enums;
using WhiteAdapter = TaikoLocalServer.Adapters.GameProtocol.White.DependencyInjection;

namespace TaikoLocalServer.Tests.White;

public sealed class WhiteHostRouteGatingTests
{
    private static readonly string[] ApprovedWhitePrefixes =
    [
        WhiteRoutePrefixes.Compatibility,
        WhiteRoutePrefixes.Final
    ];

    private static readonly string[] ApprovedWhiteSuffixes =
    [
        "baidcheck.php",
        "balancecheck.php",
        "banacoinerrorlog.php",
        "banacoinpayment.php",
        "bookkeeping.php",
        "crownsdata.php",
        "getfolder.php",
        "getbanacoininfo.php",
        "gettelop.php",
        "heartbeat.php",
        "initialdatacheck.php",
        "mydonentry.php",
        "playresult.php",
        "recommend.php",
        "selfbest.php",
        "taikojuku.php",
        "tournamentcheck.php",
        "userdata.php"
    ];

    private static readonly string[] ApprovedWhiteRoutes = ApprovedWhitePrefixes
        .SelectMany(prefix => ApprovedWhiteSuffixes.Select(suffix => $"{prefix}/{suffix}"))
        .Order(StringComparer.Ordinal)
        .ToArray();

    [Fact]
    public void EnabledWhiteApplicationPartExposesOnlyApprovedWhiteRoutes()
    {
        var routes = DiscoverWhiteRoutesFromHostSettings(whiteEnabled: true);

        Assert.Equal(ApprovedWhiteRoutes, routes);
        Assert.All(routes, route => Assert.Contains(ApprovedWhitePrefixes, prefix => route.StartsWith($"{prefix}/", StringComparison.Ordinal)));
        foreach (var prefix in ApprovedWhitePrefixes)
        {
            Assert.DoesNotContain($"{prefix}/startupauth.php", routes);
            Assert.DoesNotContain($"{prefix}/verupauth.php", routes);
            Assert.DoesNotContain($"{prefix}/verupcomplete.php", routes);
            Assert.DoesNotContain($"{prefix}/rewardexecution.php", routes);
            Assert.DoesNotContain($"{prefix}/rewardcardcheck.php", routes);
            Assert.DoesNotContain($"{prefix}/challengecompe.php", routes);
        }
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
