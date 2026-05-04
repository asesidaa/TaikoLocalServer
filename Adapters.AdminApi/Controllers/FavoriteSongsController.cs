using Microsoft.Extensions.Options;
using SharedProject.Models.Requests;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoriteSongsController(
    ITaikoDbContext context,
    IJwtTokenService jwtTokens,
    IOptions<AuthSettings> settings) : BaseAdminController<FavoriteSongsController>
{
    private readonly AuthSettings authSettings = settings.Value;

    [HttpPost]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> UpdateFavoriteSong(SetFavoriteRequest request)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }

            if (tokenInfo.Value.Baid != request.Baid && !tokenInfo.Value.IsAdmin)
            {
                return Forbid();
            }
        }

        var user = await context.UserData.FindAsync(request.Baid);
        if (user is null)
        {
            return NotFound();
        }

        var favoriteSet = new HashSet<uint>(user.FavoriteSongsArray);
        if (request.IsFavorite)
        {
            favoriteSet.Add(request.SongId);
        }
        else
        {
            favoriteSet.Remove(request.SongId);
        }

        user.FavoriteSongsArray = favoriteSet.ToList();
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpGet("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> GetFavoriteSongs(uint baid)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }

            if (tokenInfo.Value.Baid != baid && !tokenInfo.Value.IsAdmin)
            {
                return Forbid();
            }
        }

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user.FavoriteSongsArray);
    }
}
