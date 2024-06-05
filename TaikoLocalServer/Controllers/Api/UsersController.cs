using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Models.Responses;
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
    public async Task<ActionResult<UsersResponse>> GetUsers([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        if (page < 1)
        {
            return BadRequest( new { Message = "Page number cannot be less than 1." });
        }
        
        if (limit > 200)
        {
            return BadRequest( new { Message = "Limit cannot be greater than 200." });
        }
        
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return new UsersResponse();
            }

            if (!tokenInfo.Value.isAdmin)
            {
                return new UsersResponse();
            }
        }

        return await authService.GetUsersFromCards(page, limit);
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