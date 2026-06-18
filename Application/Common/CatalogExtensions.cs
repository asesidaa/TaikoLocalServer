using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Common;

public static class CatalogExtensions
{
    public static INijiiroCatalog Nijiiro(this IGameDataCatalog catalog)
        => (INijiiroCatalog)catalog.For(GameEra.Nijiiro);

    public static IGreenCatalog Green(this IGameDataCatalog catalog)
        => (IGreenCatalog)catalog.For(GameEra.Green);

    public static IBlueCatalog Blue(this IGameDataCatalog catalog)
        => (IBlueCatalog)catalog.For(GameEra.Blue);

    public static IYellowCatalog Yellow(this IGameDataCatalog catalog)
        => (IYellowCatalog)catalog.For(GameEra.Yellow);

    public static IRedCatalog Red(this IGameDataCatalog catalog)
        => (IRedCatalog)catalog.For(GameEra.Red);

    public static IWhiteCatalog White(this IGameDataCatalog catalog)
        => (IWhiteCatalog)catalog.For(GameEra.White);
}
