using System.Reflection;
using TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;
using GreenStartupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.StartupAuthRequest;
using GreenStartupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Green.Wire.StartupAuthResponse;
using SharedStartupAuthRequest = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.StartupAuthRequest;
using SharedStartupAuthResponse = TaikoLocalServer.Adapters.GameProtocol.Shared.Wire.StartupAuthResponse;
using WwR08StartupAuthResponse = taiko.vsinterface.StartupAuthResponse;

namespace TaikoLocalServer.Tests.Green;

public sealed class StartupAuthRouteTests
{
    [Fact]
    public void StartupAuthRoute_IsOwnedBySharedProtocolAdapter()
    {
        var routes = FindPostRoutes(
                typeof(BaseProtocolController<>).Assembly,
                typeof(TaikoLocalServer.Adapters.GameProtocol.WwR08.DependencyInjection).Assembly,
                typeof(TaikoLocalServer.Adapters.GameProtocol.Green.DependencyInjection).Assembly)
            .Where(route => route.Template == "/v01r00/chassis/startupauth.php")
            .ToList();

        var route = Assert.Single(routes);
        Assert.Equal("TaikoLocalServer.Adapters.GameProtocol.Shared", route.AssemblyName);
    }

    [Fact]
    public void StartupAuthRoute_IsNotVersionedAsGreenGameRoute()
    {
        var routes = FindPostRoutes(
                typeof(BaseProtocolController<>).Assembly,
                typeof(TaikoLocalServer.Adapters.GameProtocol.WwR08.DependencyInjection).Assembly,
                typeof(TaikoLocalServer.Adapters.GameProtocol.Green.DependencyInjection).Assembly)
            .Where(route => route.Template == "/v11r01/chassis/startupauth.php")
            .ToList();

        Assert.Empty(routes);
    }

    [Fact]
    public void SharedStartupAuthRequest_ReadsGreenVsInterfacePayload()
    {
        var request = new GreenStartupAuthRequest
        {
            ChassisId = "chassis",
            HddVer = 123,
            ShopId = "shop"
        };
        request.AryOperationInfoes.Add(new GreenStartupAuthRequest.OperationData
        {
            KeyData = 7,
            ValueData = [1, 2, 3]
        });

        var shared = Deserialize<SharedStartupAuthRequest>(Serialize(request));

        Assert.Equal("chassis", shared.ChassisId);
        Assert.Equal(123u, shared.HddVer);
        Assert.Equal("shop", shared.ShopId);
        var operation = Assert.Single(shared.AryOperationInfoes);
        Assert.Equal(7u, operation.KeyData);
        Assert.Equal([1, 2, 3], operation.ValueData);
    }

    [Fact]
    public void SharedStartupAuthResponse_WritesVsInterfacePayload()
    {
        var response = new SharedStartupAuthResponse { Result = 1 };
        response.AryOperationInfoes.Add(new SharedStartupAuthResponse.OperationData
        {
            KeyData = 9,
            ValueData = [4, 5, 6]
        });

        var bytes = Serialize(response);
        var green = Deserialize<GreenStartupAuthResponse>(bytes);
        var wwR08 = Deserialize<WwR08StartupAuthResponse>(bytes);

        Assert.Equal(1u, green.Result);
        Assert.Equal(1u, wwR08.Result);
        Assert.Equal(9u, Assert.Single(green.AryOperationInfoes).KeyData);
        Assert.Equal(9u, Assert.Single(wwR08.AryOperationInfoes).KeyData);
        Assert.Equal([4, 5, 6], Assert.Single(green.AryOperationInfoes).ValueData);
        Assert.Equal([4, 5, 6], Assert.Single(wwR08.AryOperationInfoes).ValueData);
    }

    private static IEnumerable<RouteInfo> FindPostRoutes(params Assembly[] assemblies)
    {
        foreach (var type in assemblies.SelectMany(assembly => assembly.DefinedTypes))
        {
            if (!type.IsClass || type.IsAbstract || !type.Name.EndsWith("Controller", StringComparison.Ordinal))
            {
                continue;
            }

            var controllerRoutes = GetRouteTemplates(type).ToArray();
            if (controllerRoutes.Length == 0)
            {
                continue;
            }

            var postActions = type.DeclaredMethods.Where(HasHttpPostAttribute).ToArray();
            foreach (var action in postActions)
            {
                var actionRoutes = GetRouteTemplates(action).ToArray();
                foreach (var controllerRoute in controllerRoutes)
                {
                    if (actionRoutes.Length == 0)
                    {
                        yield return new RouteInfo(type.Assembly.GetName().Name ?? string.Empty, controllerRoute);
                        continue;
                    }

                    foreach (var actionRoute in actionRoutes)
                    {
                        yield return new RouteInfo(
                            type.Assembly.GetName().Name ?? string.Empty,
                            CombineRoutes(controllerRoute, actionRoute));
                    }
                }
            }
        }
    }

    private static IEnumerable<string> GetRouteTemplates(MemberInfo member)
    {
        foreach (var attribute in CustomAttributeData.GetCustomAttributes(member))
        {
            if (attribute.AttributeType.FullName != "Microsoft.AspNetCore.Mvc.RouteAttribute")
            {
                continue;
            }

            if (attribute.ConstructorArguments is [{ Value: string template }])
            {
                yield return template;
            }
        }
    }

    private static bool HasHttpPostAttribute(MemberInfo member)
    {
        return CustomAttributeData.GetCustomAttributes(member)
            .Any(attribute => attribute.AttributeType.FullName == "Microsoft.AspNetCore.Mvc.HttpPostAttribute");
    }

    private static string CombineRoutes(string controllerRoute, string actionRoute)
    {
        if (actionRoute.StartsWith('/'))
        {
            return actionRoute;
        }

        return $"{controllerRoute.TrimEnd('/')}/{actionRoute.TrimStart('/')}";
    }

    private static byte[] Serialize<T>(T value)
    {
        using var stream = new MemoryStream();
        ProtoBuf.Serializer.Serialize(stream, value);
        return stream.ToArray();
    }

    private static T Deserialize<T>(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return ProtoBuf.Serializer.Deserialize<T>(stream);
    }

    private sealed record RouteInfo(string AssemblyName, string Template);
}
