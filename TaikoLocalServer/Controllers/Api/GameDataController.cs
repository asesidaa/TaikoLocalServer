using Microsoft.Extensions.Options;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class GameDataController(IGameDataCatalog gameDataService, IAuthService authService, 
    IOptions<AuthSettings> settings) : BaseController<UsersController>
{
    private readonly AuthSettings authSettings = settings.Value;
    
    [HttpGet("MusicDetails")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GetMusicDetails()
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }
        }
        
        return Ok(gameDataService.GetMusicDetailDictionary());
    }
    
    [HttpGet("Costumes")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GetCostumes()
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }
        }
        
        return Ok(gameDataService.GetCostumeList());
    }
    
    [HttpGet("Titles")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GetTitles()
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }
        }
        
        return Ok(gameDataService.GetTitleDictionary());
    }
    
    [HttpGet("LockedCostumes")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GetLockedCostumes()
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }
        }

        return Ok(gameDataService.GetLockedCostumeDataDictionary());
    }
    
    [HttpGet("LockedTitles")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GetLockedTitles()
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }
        }

        return Ok(gameDataService.GetLockedTitleDataDictionary());
    }
}