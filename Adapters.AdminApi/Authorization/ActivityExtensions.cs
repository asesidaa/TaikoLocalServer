using TaikoLocalServer.Contracts.AdminApi.Authorization;

namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

public static class ActivityExtensions
{
    /// <summary>Records an admin action in Access › Activity. Saved with the caller's next SaveChanges.</summary>
    public static void LogActivity(this ControllerBase controller, ITaikoDbContext context, string action, uint? targetBaid, string detail = "")
    {
        var loginRequired = controller.HttpContext.RequestServices.GetRequiredService<IServerAccess>().LoginRequired;
        context.Activity.Add(new ActivityEntry
        {
            Time = DateTime.UtcNow,
            ActorBaid = loginRequired ? controller.User.GetBaid() : null,
            Action = action,
            TargetBaid = targetBaid,
            Detail = detail,
        });
    }

    /// <summary>
    /// Records a change to <paramref name="baid"/>'s own data (profile, cards, favourites) when someone else made it: an
    /// admin, usually from the admin view. A player's changes to their own data aren't logged.
    /// </summary>
    public static void LogIfSomeoneElse(this ControllerBase controller, ITaikoDbContext context, string action, uint baid, string detail = "")
    {
        if (controller.HttpContext.RequestServices.GetRequiredService<IServerAccess>().LoginRequired && controller.User.GetBaid() != baid)
            controller.LogActivity(context, action, baid, detail);
    }

    /// <summary>"•••• 1234": access codes in the log and in running text never show in full.</summary>
    public static string MaskAccessCode(string accessCode) =>
        "•••• " + (accessCode.Length > 4 ? accessCode[^4..] : accessCode);
}
