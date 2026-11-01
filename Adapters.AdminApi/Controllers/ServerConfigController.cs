using Microsoft.Extensions.Configuration;
using TaikoLocalServer.Domain;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>The operator's config files as the server read them at start, for Server Management › Status. Admin only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
public class ServerConfigController(IConfiguration config, IServerAccess access) : BaseAdminController<ServerConfigController>
{
    [HttpGet]
    public ActionResult<ServerConfigResponse> Get()
    {
        var s = config.GetSection("ServerSettings");
        // Only names, URLs and paths: the certificate password and the JWT key stay out.
        var endpoints = config.GetSection("Kestrel:Endpoints").GetChildren()
            .Select(e => new ServerEndpoint(e.Key, e["Url"] ?? "")).ToList();
        return Ok(new ServerConfigResponse(
            access.Mode,
            endpoints,
            config["Kestrel:Certificates:Default:Path"] ?? "",
            config["DbFileName"] ?? "",
            s["MuchaUrl"] ?? "",
            s["GameUrl"] ?? "",
            s.GetValue<bool>("EnableMoreSongs"),
            s.GetValue("MoreSongsSize", DomainConstants.MusicIdMaxExpanded),
            s["GamePath"] ?? "",
            s.GetValue<bool>("ApiTokens")));
    }
}
