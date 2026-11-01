namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>The server-side avatar renderer (Server Management › Status): its status, and the admin's on/off switch.</summary>
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
}
