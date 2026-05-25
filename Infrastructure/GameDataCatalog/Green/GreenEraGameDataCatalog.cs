using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenEraGameDataCatalog(
    ILogger<GreenEraGameDataCatalog> logger,
    IOptions<ServerSettings>? serverSettings = null,
    INijiiroCatalog? nijiiroCatalog = null) : IGreenCatalog
{
    private uint songHashVersion;
    private IReadOnlyList<GreenMusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, GreenMusicInfoEntry> musicInfos = new Dictionary<uint, GreenMusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<GreenTaikojukuEntry> taikojukuFileOrder = [];
    private IReadOnlyDictionary<uint, GreenTaikojukuEntry> taikojuku = new Dictionary<uint, GreenTaikojukuEntry>();
    private GreenItemShopCatalog itemShopCatalog = GreenItemShopCatalog.Disabled;
    private IReadOnlyDictionary<uint, GreenItemShopEntry> itemShop = new Dictionary<uint, GreenItemShopEntry>();
    private IReadOnlyDictionary<uint, GreenEventFolderEntry> eventFolders = new Dictionary<uint, GreenEventFolderEntry>();
    private IReadOnlyDictionary<uint, GreenTelopEntry> telops = new Dictionary<uint, GreenTelopEntry>();
    private IReadOnlyDictionary<uint, GreenGachaEntry> gachas = new Dictionary<uint, GreenGachaEntry>();
    private IReadOnlyDictionary<uint, GreenTournamentEntry> tournaments = new Dictionary<uint, GreenTournamentEntry>();
    private GreenRecommendEntry recommend = GreenRecommendEntry.Empty;
    private IReadOnlyList<MovieData> movies = [];
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

    public GameEra Era => GameEra.Green;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<GreenMusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, GreenMusicInfoEntry> GreenMusicInfos => musicInfos;

    public IReadOnlyList<GreenTaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

    public IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku => taikojuku;

    public GreenItemShopCatalog ItemShopCatalog => itemShopCatalog;

    public IReadOnlyDictionary<uint, GreenItemShopEntry> ItemShop => itemShop;

    public IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, GreenTelopEntry> Telops => telops;

    public IReadOnlyDictionary<uint, GreenGachaEntry> Gachas => gachas;

    public IReadOnlyDictionary<uint, GreenTournamentEntry> Tournaments => tournaments;

    public GreenRecommendEntry Recommend => recommend;

    public IReadOnlyList<MovieData> Movies => movies;

    public IReadOnlyList<Costume> GetCostumeList() => costumeList;

    public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => titleDictionary;

    public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => neiroDictionary;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        GreenRequiredDataFiles.ThrowIfMissing();

        await BootstrapCustomizationCatalogAsync(cancellationToken);

        var musicInfo = await new GreenMusicInfoLoader().LoadAsync(cancellationToken);
        var stars = await new GreenTuningLoader().LoadAsync(cancellationToken);
        var loadedTaikojukuFileOrder = await new GreenTaikojukuLoader().LoadAsync(cancellationToken);
        var taikojukuUniqueIds = loadedTaikojukuFileOrder
            .Select(entry => entry.UniqueId)
            .ToHashSet();
        var enrichedEntries = musicInfo.Entries
            .Select(entry => stars.TryGetValue(entry.MusicId, out var set)
                ? entry with
                {
                    StarEasy = set.Easy,
                    StarNormal = set.Normal,
                    StarHard = set.Hard,
                    StarOni = set.Oni,
                    StarUra = set.Ura
                }
                : entry)
            .ToArray();

        var missingTuning = enrichedEntries
            .Where(entry => !stars.ContainsKey(entry.MusicId)
                && !taikojukuUniqueIds.Contains(entry.SongNo))
            .Select(entry => entry.MusicId)
            .ToList();
        if (missingTuning.Count > 0)
        {
            logger.LogWarning(
                "Green: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
                missingTuning.Count,
                string.Join(", ", missingTuning.Take(5)));
        }

        songHashVersion = musicInfo.SongHashVersion;
        musicInfoFileOrder = enrichedEntries;
        musicInfos = enrichedEntries.ToDictionary(entry => entry.SongNo);
        sharedMusicInfos = musicInfos.ToDictionary(
            pair => pair.Key,
            pair => (IMusicInfoEntry)pair.Value);
        taikojukuFileOrder = loadedTaikojukuFileOrder;
        taikojuku = taikojukuFileOrder
            .GroupBy(entry => entry.UniqueId)
            .ToDictionary(group => group.Key, group => group.First());
        var greenSettings = GetGreenSettings();
        itemShopCatalog = await new GreenItemShopLoader().LoadAsync(greenSettings, cancellationToken);
        itemShop = itemShopCatalog.ActiveItemsByNo;
        eventFolders = await new GreenEventFolderLoader().LoadAsync(cancellationToken);
        telops = await new GreenTelopLoader().LoadAsync(cancellationToken);
        gachas = await new GreenGachaLoader().LoadAsync(cancellationToken);
        tournaments = await new GreenTournamentLoader().LoadAsync(cancellationToken);
        recommend = await new GreenRecommendLoader().LoadAsync(
            new HashSet<uint>(musicInfos.Keys),
            cancellationToken);
        movies = await new GreenMovieLoader().LoadAsync(logger, cancellationToken);
        var greenCostumes = await new GreenCostumeLoader().LoadAsync(cancellationToken);
        var greenTitles = await new GreenTitleLoader().LoadAsync(cancellationToken);
        var greenNeiros = await new GreenNeiroLoader().LoadAsync(cancellationToken);
        var sharedNames = await LoadCustomizationNamesAsync(cancellationToken);
        var customizationCatalog = GreenCustomizationCatalogComposer.Compose(
            greenCostumes,
            greenTitles,
            greenNeiros,
            sharedNames.Costumes,
            sharedNames.Titles,
            sharedNames.Neiros,
            nijiiroCatalog?.GetCostumeList(),
            nijiiroCatalog?.GetTitleDictionary(),
            nijiiroCatalog?.GetNeiroDictionary());
        costumeList = customizationCatalog.Costumes;
        titleDictionary = customizationCatalog.Titles;
        neiroDictionary = customizationCatalog.Neiros;

        logger.LogInformation(
            "Loaded Green catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones, {MovieCount} attract movies",
            musicInfoFileOrder.Count,
            songHashVersion,
            taikojukuFileOrder.Count,
            stars.Count,
            costumeList.Count,
            titleDictionary.Count,
            neiroDictionary.Count,
            movies.Count);
    }

    private async Task BootstrapCustomizationCatalogAsync(CancellationToken cancellationToken)
    {
        var outputDirectory = PathHelper.GetDataPath(GameEra.Green);
        var required = new[]
        {
            Path.Combine(outputDirectory, GreenCatalogExtractor.CostumeFileName),
            Path.Combine(outputDirectory, GreenCatalogExtractor.TitleFileName),
            Path.Combine(outputDirectory, GreenCatalogExtractor.NeiroFileName)
        };

        if (required.All(File.Exists))
        {
            return;
        }

        var greenSettings = GetGreenSettings();
        if (!greenSettings.AutoExtractCatalog)
        {
            logger.LogInformation("Green customization catalog auto-extract is disabled; continuing with empty or partial customization catalogs.");
            return;
        }

        var gameDataPath = ResolveConfiguredPath(greenSettings.GameDataPath);
        if (!Directory.Exists(gameDataPath))
        {
            logger.LogWarning("Green customization catalog JSON is missing and game data path does not exist: {Path}", gameDataPath);
            return;
        }

        try
        {
            logger.LogInformation("Green customization catalog JSON is missing; running first-run Phase 1 extraction from {Path}", gameDataPath);
            var stagingDirectory = Path.Combine(
                Path.GetTempPath(),
                "TaikoLocalServer-GreenCatalog",
                Guid.NewGuid().ToString("N"));

            try
            {
                await GreenCatalogExtractor.ExtractAsync(
                    new GreenExtractorOptions(gameDataPath, stagingDirectory),
                    cancellationToken);

                _ = await GreenCostumeLoader.LoadFromFileAsync(
                    Path.Combine(stagingDirectory, GreenCatalogExtractor.CostumeFileName),
                    cancellationToken);
                _ = await GreenTitleLoader.LoadFromFileAsync(
                    Path.Combine(stagingDirectory, GreenCatalogExtractor.TitleFileName),
                    cancellationToken);
                _ = await GreenNeiroLoader.LoadFromFileAsync(
                    Path.Combine(stagingDirectory, GreenCatalogExtractor.NeiroFileName),
                    cancellationToken);

                Directory.CreateDirectory(outputDirectory);
                foreach (var path in required.Where(path => !File.Exists(path)))
                {
                    var sourcePath = Path.Combine(stagingDirectory, Path.GetFileName(path));
                    PublishStagedFile(sourcePath, path);
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
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or System.Xml.XmlException)
        {
            logger.LogWarning(ex, "Green customization catalog extraction failed; continuing with empty or partial customization catalogs.");
        }
    }

    private static void PublishStagedFile(string sourcePath, string destinationPath)
    {
        var tempPath = Path.Combine(
            Path.GetDirectoryName(destinationPath)
                ?? throw new InvalidOperationException($"Could not resolve directory for {destinationPath}."),
            $"{Path.GetFileName(destinationPath)}.{Guid.NewGuid():N}.tmp");

        File.Copy(sourcePath, tempPath, overwrite: false);
        try
        {
            File.Move(tempPath, destinationPath, overwrite: false);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private EraSettings GetGreenSettings()
    {
        return serverSettings?.Value.Eras.TryGetValue(nameof(GameEra.Green), out var settings) == true
            ? settings
            : new EraSettings();
    }

    private async Task<GreenCustomizationCatalog> LoadCustomizationNamesAsync(CancellationToken cancellationToken)
    {
        var loader = new GreenCustomizationNameCatalogLoader();
        var sharedNames = await loader.LoadAsync(PathHelper.GetSharedDataPath(), cancellationToken);
        var greenSettings = GetGreenSettings();
        if (string.IsNullOrWhiteSpace(greenSettings.CustomizationNameDataPath))
        {
            return sharedNames;
        }

        var overridePath = ResolveConfiguredPath(greenSettings.CustomizationNameDataPath);
        var overrideNames = await loader.LoadAsync(overridePath, cancellationToken);

        return MergeCustomizationNameCatalogs(sharedNames, overrideNames);
    }

    private static GreenCustomizationCatalog MergeCustomizationNameCatalogs(
        GreenCustomizationCatalog sharedNames,
        GreenCustomizationCatalog overrideNames)
    {
        return new GreenCustomizationCatalog(
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

    private readonly record struct CostumeKey(string CostumeType, uint CostumeId);
}
