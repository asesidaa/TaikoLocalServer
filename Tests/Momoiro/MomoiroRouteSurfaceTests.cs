using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using TaikoLocalServer.Adapters.GameProtocol.Momoiro;
using TaikoLocalServer.Adapters.GameProtocol.Shared;
using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroRouteSurfaceTests
{
    private static readonly string[] ExpectedGameRoutes =
    [
        "/v04r00/chassis/baidcheck.php",
        "/v04r00/chassis/bookkeeping.php",
        "/v04r00/chassis/defaultsong.php",
        "/v04r00/chassis/gettelop.php",
        "/v04r00/chassis/heartbeat.php",
        "/v04r00/chassis/mydonentry.php",
        "/v04r00/chassis/playresult.php",
        "/v04r00/chassis/recommend.php",
        "/v04r00/chassis/selfbest.php",
        "/v04r00/chassis/songhash.php",
        "/v04r00/chassis/telopcheck.php",
        "/v04r00/chassis/userdata.php"
    ];

    private static readonly string[] ProtoOnlyRouteFragments =
    [
        "shoppingresult.php",
        "bestscore.php",
        "communicationlog.php",
        "mainichisong.php"
    ];

    private static readonly string[] SharedStartupRoutes =
    [
        "/v01r00/chassis/startupauth.php",
        "/v01r00/chassis/verupauth.php",
        "/v01r00/chassis/verupcomplete.php"
    ];

    [Fact]
    public void EnabledMomoiro_DiscoveredActionSurface_ExposesSuppliedGameRoutes()
    {
        var actions = DiscoverControllerActions(new HashSet<GameEra> { GameEra.Momoiro });

        var momoiroRoutes = actions
            .Where(IsMomoiroAction)
            .Select(action => action.RouteTemplate)
            .Where(route => route.StartsWith(MomoiroRoutePrefixes.Game, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(ExpectedGameRoutes, momoiroRoutes);
    }

    [Fact]
    public void DisabledMomoiro_DiscoveredActionSurface_HasNoMomoiroGameRoutes()
    {
        var actions = DiscoverControllerActions(new HashSet<GameEra>());

        Assert.DoesNotContain(actions, IsMomoiroAction);
    }

    [Fact]
    public void EnabledMomoiro_DiscoveredActionSurface_ExcludesProtoOnlyRouteFamilies()
    {
        var actions = DiscoverControllerActions(new HashSet<GameEra> { GameEra.Momoiro });
        var momoiroRoutes = actions
            .Where(IsMomoiroAction)
            .Select(action => action.RouteTemplate)
            .ToArray();

        foreach (var routeFragment in ProtoOnlyRouteFragments)
        {
            Assert.DoesNotContain(
                momoiroRoutes,
                route => route.Contains(routeFragment, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void StartupAndVersion_DiscoveredActionSurface_RemainsShared()
    {
        var actions = DiscoverControllerActions(new HashSet<GameEra> { GameEra.Momoiro });

        foreach (var route in SharedStartupRoutes)
        {
            var matchingActions = actions
                .Where(action => string.Equals(action.RouteTemplate, route, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Assert.NotEmpty(matchingActions);
            Assert.All(matchingActions, action => Assert.Same(SharedAssembly, action.ControllerTypeInfo.Assembly));
            Assert.DoesNotContain(matchingActions, IsMomoiroAction);
        }
    }

    private static IReadOnlyList<DiscoveredControllerAction> DiscoverControllerActions(IReadOnlySet<GameEra> enabledEras)
    {
        var manager = new ApplicationPartManager();
        manager.ApplicationParts.Add(new AssemblyPart(MomoiroAssembly));
        manager.ApplicationParts.Add(new AssemblyPart(SharedAssembly));
        manager.FeatureProviders.Add(new ControllerFeatureProvider());

        GameProtocolApplicationParts.RemoveDisabledGameProtocolApplicationParts(manager, enabledEras);

        var feature = new ControllerFeature();
        manager.PopulateFeature(feature);

        return feature.Controllers
            .SelectMany(controller => DiscoverControllerActions(controller))
            .ToArray();
    }

    private static IEnumerable<DiscoveredControllerAction> DiscoverControllerActions(TypeInfo controller)
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
                    yield return new DiscoveredControllerAction(controller, route);
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

    private static bool IsMomoiroAction(DiscoveredControllerAction action)
        => action.ControllerTypeInfo.Assembly == MomoiroAssembly;

    private static void RemoveDuplicatePart(ApplicationPartManager apm, string? assemblyName)
    {
        var duplicate = apm.ApplicationParts.FirstOrDefault(part =>
            part is AssemblyPart assemblyPart
            && assemblyPart.Assembly.GetName().Name == assemblyName);

        if (duplicate is not null)
        {
            apm.ApplicationParts.Remove(duplicate);
        }
    }

    private static readonly System.Reflection.Assembly MomoiroAssembly = typeof(MomoiroAdapterMarker).Assembly;
    private static readonly System.Reflection.Assembly SharedAssembly = typeof(StartupAuthController).Assembly;

    private sealed record DiscoveredControllerAction(TypeInfo ControllerTypeInfo, string RouteTemplate);
}
