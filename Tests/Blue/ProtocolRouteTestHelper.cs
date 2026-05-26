using System.Reflection;

namespace TaikoLocalServer.Tests.Blue;

internal static class ProtocolRouteTestHelper
{
    public static IEnumerable<RouteInfo> FindPostRoutes(params Assembly[] assemblies)
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
}

internal sealed record RouteInfo(string AssemblyName, string Template);
