namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

internal static class EraRoute
{
    public static bool TryParse(string era, out GameEra gameEra)
    {
        return Enum.TryParse(era, ignoreCase: true, out gameEra)
               && Enum.IsDefined(gameEra);
    }

    public static BadRequestObjectResult BadEra(string era)
    {
        return new BadRequestObjectResult($"Unsupported game era '{era}'.");
    }
}
