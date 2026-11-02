namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

internal sealed class Ac15CustomizationNameCatalogLoader
{
    public const string CostumeFileName = "costume_name_data.json";
    public const string TitleFileName = "title_name_data.json";
    public const string NeiroFileName = "neiro_name_data.json";

    public async Task<Ac15CustomizationCatalog> LoadAsync(
        string directory,
        CancellationToken cancellationToken)
    {
        var costumes = await Ac15CustomizationCatalogLoader.LoadListAsync<Costume>(
            Path.Combine(directory, CostumeFileName),
            cancellationToken);
        var titles = await LoadDictionaryAsync<Title>(
            Path.Combine(directory, TitleFileName),
            title => title.TitleId,
            cancellationToken);
        var neiros = await LoadDictionaryAsync<Neiro>(
            Path.Combine(directory, NeiroFileName),
            neiro => neiro.NeiroId,
            cancellationToken);

        return new Ac15CustomizationCatalog(costumes, titles, neiros);
    }

    private static async Task<IReadOnlyDictionary<uint, T>> LoadDictionaryAsync<T>(
        string path,
        Func<T, uint> keySelector,
        CancellationToken cancellationToken)
    {
        var items = await Ac15CustomizationCatalogLoader.LoadListAsync<T>(path, cancellationToken);
        return items
            .GroupBy(keySelector)
            .ToDictionary(group => group.Key, group => group.First());
    }
}
