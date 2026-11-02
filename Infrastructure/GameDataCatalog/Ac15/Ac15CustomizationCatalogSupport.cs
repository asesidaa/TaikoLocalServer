using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

internal static class Ac15CustomizationCatalogSupport
{
    public static async Task EnsureExtractedAsync(
        GameEra era,
        EraSettings eraSettings,
        string costumeFileName,
        string titleFileName,
        string neiroFileName,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var outputDirectory = PathHelper.GetDataPath(era);
        var required = new[]
        {
            Path.Combine(outputDirectory, costumeFileName),
            Path.Combine(outputDirectory, titleFileName),
            Path.Combine(outputDirectory, neiroFileName)
        };

        var refreshExistingCatalogs = await ShouldRefreshEmptyTitleCatalogAsync(eraSettings, required[1], cancellationToken);
        if (required.All(File.Exists) && !refreshExistingCatalogs)
        {
            return;
        }

        if (!eraSettings.AutoExtractCatalog)
        {
            logger.LogInformation("{Era} customization catalog auto-extract is disabled; continuing with empty or partial customization catalogs.", era);
            return;
        }

        var gameDataPath = ResolveConfiguredPath(eraSettings.GameDataPath);
        if (!Directory.Exists(gameDataPath))
        {
            logger.LogWarning("{Era} customization catalog JSON is missing and game data path does not exist: {Path}", era, gameDataPath);
            return;
        }

        try
        {
            logger.LogInformation("{Era} customization catalog JSON is missing; running first-run extraction from {Path}", era, gameDataPath);
            var stagingDirectory = Path.Combine(
                Path.GetTempPath(),
                $"TaikoLocalServer-{era}Catalog",
                Guid.NewGuid().ToString("N"));

            try
            {
                await Ac15CustomizationCatalogExtractor.ExtractAsync(
                    gameDataPath,
                    stagingDirectory,
                    costumeFileName,
                    titleFileName,
                    neiroFileName,
                    cancellationToken);

                Directory.CreateDirectory(outputDirectory);
                foreach (var path in required.Where(path => refreshExistingCatalogs || !File.Exists(path)))
                {
                    var sourcePath = Path.Combine(stagingDirectory, Path.GetFileName(path));
                    PublishStagedFile(sourcePath, path, refreshExistingCatalogs);
                }
            }
            finally
            {
                if (Directory.Exists(stagingDirectory))
                {
                    Directory.Delete(stagingDirectory, recursive: true);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "{Era} customization catalog extraction failed; continuing with empty or partial customization catalogs.", era);
        }
    }

    public static async Task<Ac15CustomizationCatalog> LoadEraCatalogAsync(
        GameEra era,
        string costumeFileName,
        string titleFileName,
        string neiroFileName,
        CancellationToken cancellationToken)
    {
        var dataPath = PathHelper.GetDataPath(era);
        var costumes = await Ac15CustomizationCatalogLoader.LoadListAsync<Costume>(
            Path.Combine(dataPath, costumeFileName),
            cancellationToken);
        var titles = await LoadDictionaryAsync<Title>(
            Path.Combine(dataPath, titleFileName),
            title => title.TitleId,
            cancellationToken);
        var neiros = await LoadDictionaryAsync<Neiro>(
            Path.Combine(dataPath, neiroFileName),
            neiro => neiro.NeiroId,
            cancellationToken);

        return new Ac15CustomizationCatalog(costumes, titles, neiros);
    }

    public static async Task<Ac15CustomizationCatalog> LoadCustomizationNamesAsync(
        EraSettings eraSettings,
        CancellationToken cancellationToken)
    {
        var loader = new Ac15CustomizationNameCatalogLoader();
        var sharedNames = await loader.LoadAsync(PathHelper.GetSharedDataPath(), cancellationToken);
        if (string.IsNullOrWhiteSpace(eraSettings.CustomizationNameDataPath))
        {
            return sharedNames;
        }

        var overridePath = ResolveConfiguredPath(eraSettings.CustomizationNameDataPath);
        var overrideNames = await loader.LoadAsync(overridePath, cancellationToken);

        return MergeCustomizationNameCatalogs(sharedNames, overrideNames);
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

    private static Ac15CustomizationCatalog MergeCustomizationNameCatalogs(
        Ac15CustomizationCatalog sharedNames,
        Ac15CustomizationCatalog overrideNames)
    {
        return new Ac15CustomizationCatalog(
            MergeCostumeNameCatalogs(sharedNames.Costumes, overrideNames.Costumes),
            MergeNameDictionaries(sharedNames.Titles, overrideNames.Titles),
            MergeNameDictionaries(sharedNames.Neiros, overrideNames.Neiros));
    }

    private static IReadOnlyList<Costume> MergeCostumeNameCatalogs(
        IReadOnlyList<Costume> sharedNames,
        IReadOnlyList<Costume> overrideNames)
    {
        var result = sharedNames
            .GroupBy(costume => new CostumeKey(costume.CostumeType.ToLowerInvariant(), costume.CostumeId))
            .ToDictionary(group => group.Key, group => group.First());

        foreach (var costume in overrideNames)
        {
            result[new CostumeKey(costume.CostumeType.ToLowerInvariant(), costume.CostumeId)] = costume;
        }

        return result.Values
            .OrderBy(costume => costume.CostumeType)
            .ThenBy(costume => costume.CostumeId)
            .ToList();
    }

    private static IReadOnlyDictionary<uint, T> MergeNameDictionaries<T>(
        IReadOnlyDictionary<uint, T> sharedNames,
        IReadOnlyDictionary<uint, T> overrideNames)
    {
        var result = sharedNames.ToDictionary();
        foreach (var (id, item) in overrideNames)
        {
            result[id] = item;
        }

        return result;
    }

    private static void PublishStagedFile(string sourcePath, string destinationPath, bool overwrite)
    {
        var tempPath = Path.Combine(
            Path.GetDirectoryName(destinationPath)
                ?? throw new InvalidOperationException($"Could not resolve directory for {destinationPath}."),
            $"{Path.GetFileName(destinationPath)}.{Guid.NewGuid():N}.tmp");

        File.Copy(sourcePath, tempPath, overwrite: false);
        try
        {
            File.Move(tempPath, destinationPath, overwrite);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static string ResolveConfiguredPath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        var root = Directory.GetParent(PathHelper.GetRootPath())?.FullName
                   ?? throw new InvalidOperationException("Could not resolve server root.");
        return Path.GetFullPath(Path.Combine(root, configuredPath));
    }

    private static async Task<bool> ShouldRefreshEmptyTitleCatalogAsync(
        EraSettings eraSettings,
        string titlePath,
        CancellationToken cancellationToken)
    {
        if (!eraSettings.AutoExtractCatalog || !File.Exists(titlePath))
        {
            return false;
        }

        var titles = await Ac15CustomizationCatalogLoader.LoadListAsync<Title>(titlePath, cancellationToken);
        if (titles.Count > 0)
        {
            return false;
        }

        var gameDataPath = ResolveConfiguredPath(eraSettings.GameDataPath);
        return HasNameCatalogSource(gameDataPath, "title_name");
    }

    private static bool HasNameCatalogSource(string gameDataPath, string catalogDirectoryName)
    {
        var nutdataRoot = Path.Combine(gameDataPath, "nutdata");
        return Directory.Exists(nutdataRoot)
               && Directory.EnumerateFiles(nutdataRoot, "*", SearchOption.AllDirectories)
                   .Any(path => string.Equals(
                       Path.GetFileName(Path.GetDirectoryName(path)),
                       catalogDirectoryName,
                       StringComparison.OrdinalIgnoreCase));
    }

    private readonly record struct CostumeKey(string CostumeType, uint CostumeId);
}
