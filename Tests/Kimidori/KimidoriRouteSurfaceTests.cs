using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using TaikoLocalServer.Adapters.GameProtocol.Kimidori;
using TaikoLocalServer.Adapters.GameProtocol.Shared;

namespace TaikoLocalServer.Tests.Kimidori;

public sealed class KimidoriRouteSurfaceTests
{
    private static readonly string[] ExpectedFinalRoutes =
    [
        "/v05r06/chassis/baidcheck.php",
        "/v05r06/chassis/bookkeeping.php",
        "/v05r06/chassis/crownsdata.php",
        "/v05r06/chassis/defaultsong.php",
        "/v05r06/chassis/foldercheck.php",
        "/v05r06/chassis/getfolder.php",
        "/v05r06/chassis/gettelop.php",
        "/v05r06/chassis/heartbeat.php",
        "/v05r06/chassis/mainichisong.php",
        "/v05r06/chassis/mydonentry.php",
        "/v05r06/chassis/playresult.php",
        "/v05r06/chassis/recommend.php",
        "/v05r06/chassis/selfbest.php",
        "/v05r06/chassis/songhash.php",
        "/v05r06/chassis/startupauth.php",
        "/v05r06/chassis/taikojuku.php",
        "/v05r06/chassis/telopcheck.php",
        "/v05r06/chassis/userdata.php",
        "/v05r06/chassis/verupauth.php",
        "/v05r06/chassis/verupcomplete.php"
    ];

    private static readonly string[] ExpectedCompatibilityRoutes =
    [
        "/v05r00/chassis/baidcheck.php",
        "/v05r00/chassis/bestscore.php",
        "/v05r00/chassis/bookkeeping.php",
        "/v05r00/chassis/communicationlog.php",
        "/v05r00/chassis/crownsdata.php",
        "/v05r00/chassis/defaultsong.php",
        "/v05r00/chassis/foldercheck.php",
        "/v05r00/chassis/getfolder.php",
        "/v05r00/chassis/gettelop.php",
        "/v05r00/chassis/heartbeat.php",
        "/v05r00/chassis/mainichisong.php",
        "/v05r00/chassis/mydonentry.php",
        "/v05r00/chassis/playresult.php",
        "/v05r00/chassis/recommend.php",
        "/v05r00/chassis/selfbest.php",
        "/v05r00/chassis/shoppingresult.php",
        "/v05r00/chassis/songhash.php",
        "/v05r00/chassis/telopcheck.php",
        "/v05r00/chassis/userdata.php"
    ];

    private static readonly string[] FinalExcludedRouteFragments =
    [
        "bestscore.php",
        "communicationlog.php",
        "headclerk2.php",
        "shoppingresult.php"
    ];

    [Fact]
    public void EnabledKimidori_DiscoveredActionSurface_ExposesFinalRoutes()
    {
        var routes = DiscoverKimidoriRoutes(new HashSet<GameEra> { GameEra.Kimidori })
            .Where(route => route.StartsWith(KimidoriRoutePrefixes.Final, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedFinalRoutes, routes);
    }

    [Fact]
    public void EnabledKimidori_DiscoveredActionSurface_PreservesCompatibilityRoutes()
    {
        var routes = DiscoverKimidoriRoutes(new HashSet<GameEra> { GameEra.Kimidori })
            .Where(route => route.StartsWith(KimidoriRoutePrefixes.Compatibility, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedCompatibilityRoutes, routes);
        Assert.DoesNotContain(KimidoriRoutePrefixes.Compatibility + "/taikojuku.php", routes);
    }

    [Fact]
    public void EnabledKimidori_DiscoveredActionSurface_ExcludesFinalProtoOnlyRoutes()
    {
        var finalRoutes = DiscoverKimidoriRoutes(new HashSet<GameEra> { GameEra.Kimidori })
            .Where(route => route.StartsWith(KimidoriRoutePrefixes.Final, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var routeFragment in FinalExcludedRouteFragments)
        {
            Assert.DoesNotContain(
                finalRoutes,
                route => route.Contains(routeFragment, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void DisabledKimidori_DiscoveredActionSurface_HasNoKimidoriRoutes()
    {
        var routes = DiscoverKimidoriRoutes(new HashSet<GameEra>());

        Assert.Empty(routes);
    }

    private static IReadOnlyList<string> DiscoverKimidoriRoutes(IReadOnlySet<GameEra> enabledEras)
    {
        var manager = new ApplicationPartManager();
        manager.ApplicationParts.Add(new AssemblyPart(KimidoriAssembly));
        manager.FeatureProviders.Add(new ControllerFeatureProvider());

        GameProtocolApplicationParts.RemoveDisabledGameProtocolApplicationParts(manager, enabledEras);

        var feature = new ControllerFeature();
        manager.PopulateFeature(feature);

        return feature.Controllers
            .Where(controller => controller.Assembly == KimidoriAssembly)
            .SelectMany(DiscoverControllerActions)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<string> DiscoverControllerActions(TypeInfo controller)
    {
        var controllerRoutes = RouteTemplates(controller.GetCustomAttributes(inherit: true));

        foreach (var method in controller.DeclaredMethods.Where(method => method.IsPublic && !method.IsStatic))
        {
            var methodRoutes = RouteTemplates(method.GetCustomAttributes(inherit: true));

            foreach (var controllerRoute in controllerRoutes)
            foreach (var methodRoute in methodRoutes)
            {
                var route = CombineRoutes(controllerRoute, methodRoute);
                if (!string.IsNullOrWhiteSpace(route))
                {
                    yield return route;
                }
            }
        }
    }

    private static string[] RouteTemplates(IEnumerable<object> attributes)
    {
        var templates = attributes
            .OfType<IRouteTemplateProvider>()
            .Select(attribute => attribute.Template ?? string.Empty)
            .ToArray();

        return templates.Length == 0 ? [string.Empty] : templates;
    }

    private static string CombineRoutes(string controllerRoute, string methodRoute)
    {
        if (string.IsNullOrWhiteSpace(controllerRoute))
        {
            return methodRoute;
        }

        if (string.IsNullOrWhiteSpace(methodRoute))
        {
            return controllerRoute;
        }

        return $"{controllerRoute.TrimEnd('/')}/{methodRoute.TrimStart('/')}";
    }

    private static readonly Assembly KimidoriAssembly = typeof(KimidoriAdapterMarker).Assembly;
}
