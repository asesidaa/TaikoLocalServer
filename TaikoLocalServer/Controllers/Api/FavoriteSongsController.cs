using TaikoLocalServer.Services.Interfaces;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class FavoriteSongsController : BaseController<FavoriteSongsController>
{
    private readonly IUserDatumService userDatumService;

    public FavoriteSongsController(IUserDatumService userDatumService)
    {
        this.userDatumService = userDatumService;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateFavoriteSong(uint baid, uint songId, bool isFavorite)
    {
        var user = await userDatumService.GetFirstUserDatumOrNull(baid);

        if (user is null)
        {
            return NotFound();
        }

        await userDatumService.UpdateFavoriteSong(baid, songId, isFavorite);
        return NoContent();
    }
}