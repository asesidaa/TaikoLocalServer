using TaikoLocalServer.Filters;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class GameDataController(IGameDataCatalog catalog) : BaseController<GameDataController>
{
    [HttpGet("MusicDetails")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GetMusicDetails() => Ok(catalog.GetMusicDetailDictionary());

    [HttpGet("Costumes")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GetCostumes() => Ok(catalog.GetCostumeList());

    [HttpGet("Titles")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GetTitles() => Ok(catalog.GetTitleDictionary());

    [HttpGet("LockedCostumes")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GetLockedCostumes() => Ok(catalog.GetLockedCostumeDataDictionary());

    [HttpGet("LockedTitles")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public IActionResult GetLockedTitles() => Ok(catalog.GetLockedTitleDataDictionary());
}
