namespace TaikoLocalServer.Adapters.AdminApi.Authorization;

/// <summary>Deleting a user: an admin (UsersController) or the player themselves (AuthController.DeleteAccount).</summary>
public static class UserRemoval
{
    /// <summary>The user and everything that cascades from them, plus what doesn't: their sessions and trusted browsers.</summary>
    public static void Remove(ITaikoDbContext context, UserDatum user)
    {
        context.UserData.Remove(user);
        // Not cascaded: without this the deleted user's tokens stay valid, and would carry over to a new user who
        // got the same Baid.
        context.WebSessions.RemoveRange(context.WebSessions.Where(s => s.Baid == user.Baid));
        context.TrustedBrowsers.RemoveRange(context.TrustedBrowsers.Where(t => t.Baid == user.Baid));
    }
}
