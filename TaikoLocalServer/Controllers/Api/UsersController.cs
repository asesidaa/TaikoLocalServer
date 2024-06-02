using Microsoft.Extensions.Options;
using SharedProject.Models;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserDatumService userDatumService, IAuthService authService, 
    IOptions<AuthSettings> settings) : BaseController<UsersController>
{
    private readonly AuthSettings authSettings = settings.Value;
    
    [HttpGet("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<User?> GetUser(uint baid)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return null;
            }

            if (!tokenInfo.Value.isAdmin && tokenInfo.Value.baid != baid)
            {
                return null;
            }
        }

        var user = await authService.GetUserByBaid(baid);
        return user;
    }
    
    [HttpGet]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IEnumerable<User>> GetUsers()
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Array.Empty<User>();
            }

            if (!tokenInfo.Value.isAdmin)
            {
                return Array.Empty<User>();
            }
        }

        return await authService.GetUsersFromCards();
    }
    
    [HttpDelete("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> DeleteUser(uint baid)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.isAdmin && tokenInfo.Value.baid != baid)
            {
                return Forbid();
            }
        }
        
        var result = await userDatumService.DeleteUser(baid);

        return result ? NoContent() : NotFound();
    }
}