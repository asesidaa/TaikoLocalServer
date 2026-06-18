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
            GameEra.Blue => Ok(catalog.Blue().GetCostumeList()),
            GameEra.Yellow => Ok(catalog.Yellow().GetCostumeList()),
            GameEra.Red => Ok(catalog.Red().GetCostumeList()),
            GameEra.White => Ok(catalog.White().GetCostumeList()),
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
            GameEra.Blue => Ok(catalog.Blue().GetTitleDictionary()),
            GameEra.Yellow => Ok(catalog.Yellow().GetTitleDictionary()),
            GameEra.Red => Ok(catalog.Red().GetTitleDictionary()),
            GameEra.White => Ok(catalog.White().GetTitleDictionary()),
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
            GameEra.Blue => Ok(catalog.Blue().GetNeiroDictionary()),
            GameEra.Yellow => Ok(catalog.Yellow().GetNeiroDictionary()),
            GameEra.Red => Ok(catalog.Red().GetNeiroDictionary()),
            GameEra.White => Ok(catalog.White().GetNeiroDictionary()),
            _ => EraRoute.BadEra(era)
        };
    }
}
