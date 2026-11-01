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
            var version = path.StartsWithSegments("/v12r08_ww") ? "WW 39.06" : path.StartsWithSegments("/v12r00_cn") ? "CHN" : "";
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
