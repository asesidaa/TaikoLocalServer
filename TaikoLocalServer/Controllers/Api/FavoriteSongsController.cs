using SharedProject.Models.Requests;

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
    public async Task<IActionResult> UpdateFavoriteSong(SetFavoriteRequest request)
    {
        var user = await userDatumService.GetFirstUserDatumOrNull(request.Baid);

        if (user is null) return NotFound();

        await userDatumService.UpdateFavoriteSong(request.Baid, request.SongId, request.IsFavorite);
        return NoContent();
    }
}