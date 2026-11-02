namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenCustomizationNameCatalogLoader
{
    public const string CostumeFileName = "costume_name_data.json";
    public const string TitleFileName = "title_name_data.json";
    public const string NeiroFileName = "neiro_name_data.json";

    public async Task<GreenCustomizationCatalog> LoadAsync(
        string directory,
        CancellationToken cancellationToken)
    {
        var costumes = await GreenCustomizationCatalogLoader.LoadListAsync<Costume>(
            Path.Combine(directory, CostumeFileName),
            cancellationToken);
        var titles = await GreenTitleLoader.LoadFromFileAsync(
            Path.Combine(directory, TitleFileName),
            cancellationToken);
        var neiros = await GreenNeiroLoader.LoadFromFileAsync(
            Path.Combine(directory, NeiroFileName),
            cancellationToken);

        return new GreenCustomizationCatalog(costumes, titles, neiros);
    }
}
