using Microsoft.Extensions.Options;
using SharedProject.Models.Requests;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class FavoriteSongsController(IUserDatumService userDatumService, IAuthService authService, 
    IOptions<AuthSettings> settings) : BaseController<FavoriteSongsController>
{
    private readonly AuthSettings authSettings = settings.Value;
    
    [HttpPost]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> UpdateFavoriteSong(SetFavoriteRequest request)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }
            
            if (tokenInfo.Value.baid != request.Baid && !tokenInfo.Value.isAdmin)
            {
                return Forbid();
            }
        }
        
        var user = await userDatumService.GetFirstUserDatumOrNull(request.Baid);

        if (user is null)
        {
            return NotFound();
        }

        await userDatumService.UpdateFavoriteSong(request.Baid, request.SongId, request.IsFavorite);
        return NoContent();
    }

    [HttpGet("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> GetFavoriteSongs(uint baid)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }
            
            if (tokenInfo.Value.baid != baid && !tokenInfo.Value.isAdmin)
            {
                return Forbid();
            }
        }
        
        var user = await userDatumService.GetFirstUserDatumOrNull(baid);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user.FavoriteSongsArray);
    }
}