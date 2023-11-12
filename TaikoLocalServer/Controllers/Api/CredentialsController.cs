using SharedProject.Models.Requests;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CredentialsController : BaseController<CredentialsController>
{
    private readonly ICredentialService credentialService;

    public CredentialsController(ICredentialService credentialService)
    {
        this.credentialService = credentialService;
    }
    
    [HttpPost]
    public async Task<IActionResult> UpdatePassword(SetPasswordRequest request)
    {
        var baid = request.Baid;
        var password = request.Password;
        var salt = request.Salt;
        var result = await credentialService.UpdatePassword(baid, password, salt);
        return result ? NoContent() : NotFound();
    }
}