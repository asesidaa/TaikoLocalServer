using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>Access › Overview's checklist: what needs fixing on this server, worst first. Admin only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminServer)]
public class ChecklistController(IConfiguration config, IHostEnvironment host, IServerAccess access, ITaikoDbContext context)
    : BaseAdminController<ChecklistController>
{
    [HttpGet]
    public async Task<List<ChecklistItem>> Get()
    {
        var items = new List<ChecklistItem> { Certificate() };
        var rules = access.Rules;

        if (access.LoginRequired)
        {
            items.Add(rules.SignUpMode == SignUpMode.Open
                ? new(ChecklistLevel.Warning, "Sign-up is open",
                    "Anyone who knows a card's access code (it's printed on the card) can create its account. Pick Last play time or Invite code only below.")
                : new(ChecklistLevel.Ok, "Sign-up needs proof", $"Sign-up mode: {rules.SignUpMode}."));
            items.Add(new(ChecklistLevel.Ok, "Failed sign-ins are limited",
                $"An account locks for {rules.LockoutMinutes} min after {rules.LockoutAttempts} wrong passwords, and each IP gets 10 tries a minute."));
        }

        var now = DateTime.UtcNow;
        var adminTokens = await context.ApiTokens.CountAsync(t => t.RevokedAt == null && (t.ExpiresAt == null || t.ExpiresAt > now)
                                                                  && t.Scopes.Contains("admin:"), HttpContext.RequestAborted);
        if (config.GetValue<bool>("ServerSettings:ApiTokens") && adminTokens > 0)
            items.Add(new(ChecklistLevel.Warning, $"{adminTokens} API token(s) can change things",
                "Tokens with admin:users or admin:server scopes act as their admin. Revoke any you don't use (API tokens tab)."));

        return items.OrderBy(i => i.Level).ToList();
    }

    // Cabinets refuse an expired certificate, so that's a problem; close to expiry or self-signed is a warning.
    private ChecklistItem Certificate()
    {
        var path = config["Kestrel:Certificates:Default:Path"];
        if (string.IsNullOrEmpty(path))
            return new(ChecklistLevel.Warning, "No certificate configured", "Kestrel:Certificates:Default in Kestrel.json has no Path; cabinets need HTTPS.");
        var full = Path.IsPathRooted(path) ? path : Path.Combine(host.ContentRootPath, path);
        try
        {
            using var cert = X509CertificateLoader.LoadPkcs12FromFile(full, config["Kestrel:Certificates:Default:Password"]);
            var days = (cert.NotAfter.ToUniversalTime() - DateTime.UtcNow).TotalDays;
            if (days < 0)
                return new(ChecklistLevel.Problem, "The certificate has expired",
                    $"It expired on {cert.NotAfter:d}. Cabinets refuse it: replace {path} and restart the server.");
            if (days < 90)
                return new(ChecklistLevel.Warning, "The certificate expires soon", $"It expires on {cert.NotAfter:d}. Replace {path} before then.");
            return new(ChecklistLevel.Ok, "Certificate is valid",
                $"Until {cert.NotAfter:d}{(cert.Subject == cert.Issuer ? " (self-signed, as cabinets on a local server expect)" : "")}.");
        }
        catch (Exception e) when (e is CryptographicException or IOException or UnauthorizedAccessException)
        {
            return new(ChecklistLevel.Problem, "The certificate can't be read", $"{path}: {e.Message}");
        }
    }
}
