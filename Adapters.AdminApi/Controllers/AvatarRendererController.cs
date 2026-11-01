namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>The server-side avatar renderer (Server Management › Settings): its status, the admin's on/off switch, and the stored avatars.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
public class AvatarRendererController(IAvatarRenderer renderer, ITaikoDbContext context) : BaseAdminController<AvatarRendererController>
{
    [HttpGet]
    public AvatarRendererStatus Get() => renderer.Status;

    public sealed record SetRequest(bool Enabled);

    [HttpPut]
    public async Task<AvatarRendererStatus> Put(SetRequest request)
    {
        await renderer.SetEnabledAsync(request.Enabled, HttpContext.RequestAborted);
        this.LogActivity(context, "server.avatars", null, request.Enabled ? "on" : "off");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return renderer.Status;
    }

    /// <summary>"Enable 3D Renderer": player cards show the 3D model, or (off) still images the renderer draws.</summary>
    [HttpPut("Cards3D")]
    public async Task<AvatarRendererStatus> PutCards3D(SetRequest request)
    {
        await renderer.SetCards3DAsync(request.Enabled, HttpContext.RequestAborted);
        this.LogActivity(context, "server.avatars", null, request.Enabled ? "3d cards" : "still cards");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return renderer.Status;
    }

    /// <summary>Every stored avatar, without the images.</summary>
    [HttpGet("Avatars")]
    public Task<List<StoredAvatar>> Avatars() =>
        context.UserAvatars.OrderBy(a => a.Baid).Select(a => new StoredAvatar(a.Baid,
                context.UserData.Where(u => u.Baid == a.Baid).Select(u => u.MyDonName).FirstOrDefault() ?? "", a.Key, a.ContentType, a.Image.Length))
            .ToListAsync(HttpContext.RequestAborted);

    /// <summary>Deletes every stored avatar (and still image); each is drawn again the next time it's shown.</summary>
    [HttpDelete("Avatars")]
    public async Task<int> Clear()
    {
        var deleted = await context.UserAvatars.ExecuteDeleteAsync(HttpContext.RequestAborted);
        await context.UserStills.ExecuteDeleteAsync(HttpContext.RequestAborted);
        this.LogActivity(context, "server.avatars", null, $"cleared {deleted}");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return deleted;
    }
}
