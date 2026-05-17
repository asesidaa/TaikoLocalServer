namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/{era}/customization")]
[Authorize]
public class CustomizationCatalogController(IGameDataCatalog catalog) : BaseAdminController<CustomizationCatalogController>
{
    [HttpGet("costumes")]
    public IActionResult GetCostumes(string era)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(catalog.Nijiiro().GetCostumeList()),
            GameEra.Green => Ok(catalog.Green().GetCostumeList()),
            _ => EraRoute.BadEra(era)
        };
    }

    [HttpGet("titles")]
    public IActionResult GetTitles(string era)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(catalog.Nijiiro().GetTitleDictionary()),
            GameEra.Green => Ok(catalog.Green().GetTitleDictionary()),
            _ => EraRoute.BadEra(era)
        };
    }

    [HttpGet("neiros")]
    public IActionResult GetNeiros(string era)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        return gameEra switch
        {
            GameEra.Nijiiro => Ok(catalog.Nijiiro().GetNeiroDictionary()),
            GameEra.Green => Ok(catalog.Green().GetNeiroDictionary()),
            _ => EraRoute.BadEra(era)
        };
    }
}
