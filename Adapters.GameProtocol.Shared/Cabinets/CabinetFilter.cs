using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Cabinets;

/// <summary>
/// Every game request carries the cabinet's chassis ID: record the cabinet, and refuse a blocked (or, when only approved
/// cabinets may connect, an unapproved) one before the controller runs. Requests without one (AllNet, admin API) pass.
/// </summary>
public sealed class CabinetFilter(ICabinetRegistry cabinets, IServerAccess access, ILogger<CabinetFilter> logger) : IAsyncActionFilter
{
    private static readonly ConcurrentDictionary<Type, (PropertyInfo Chassis, PropertyInfo? Shop)?> Properties = new();

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null || Lookup(argument.GetType()) is not { } props) continue;
            var chassis = props.Chassis.GetValue(argument) as string ?? "";
            var shop = props.Shop?.GetValue(argument)?.ToString() ?? "";
            var path = context.HttpContext.Request.Path;
            var version = VersionOf(path);
            // Offline: the heartbeat itself tells the cabinet (NG); everything else is refused.
            var offline = access.Status.At(DateTime.UtcNow) == ServerState.Offline && !path.Value!.Contains("heartbeat");
            if (!cabinets.Seen(chassis, shop, version) || offline)
            {
                if (!offline) logger.LogWarning("Refused cabinet {ChassisId} ({Path})", chassis, path);
                // The cabinet shows its network error: in Taiko.exe 39.06 a failed request (or an empty answer, whose
                // result is 0) sets GameConnectStatus kError with kServerAccess, shown as connectserver_error.
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }
            break;
        }
        await next();
    }

    // The game's name comes first: the admin UI reads a cabinet's game from it (anything else is Nijiiro).
    private static readonly (string Prefix, string Version)[] Versions =
    [
        ("/v12r08_ww", "WW 39.06"), ("/v12r00_cn", "CHN"),
        ("/v11r01", "Green 11.01"), ("/v10r03", "Blue 10.03"), ("/v09r02", "Yellow 09.02"),
        ("/v08r01", "Red 08.01"), ("/v08r00_tw", "Red 08.00 TW"), ("/v07r03", "White 07.03"), ("/v07r00", "White 07.00"),
        ("/v06r01", "Murasaki 06.01"), ("/v06r00", "Murasaki 06.00"), ("/v05r06", "Kimidori 05.06"), ("/v05r00", "Kimidori 05.00"),
        ("/v04r02", "Momoiro 04.02"), ("/v04r00", "Momoiro 04.00"),
    ];

    public static string VersionOf(PathString path) =>
        Versions.FirstOrDefault(v => path.StartsWithSegments(v.Prefix)).Version ?? "";

    private static (PropertyInfo Chassis, PropertyInfo? Shop)? Lookup(Type type) => Properties.GetOrAdd(type, t =>
        t.GetProperty("ChassisId") is { PropertyType: var pt } chassis && pt == typeof(string)
            ? (chassis, t.GetProperty("ShopId"))
            : null);

    /// <summary>Registers the filter once, however many game-protocol adapters ask.</summary>
    public static IServiceCollection AddOnce(IServiceCollection services)
    {
        if (services.Any(s => s.ServiceType == typeof(CabinetFilter))) return services;
        services.AddScoped<CabinetFilter>();
        services.Configure<MvcOptions>(o => o.Filters.AddService<CabinetFilter>());
        return services;
    }
}
