using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueEraGameDataCatalog(
    ILogger<BlueEraGameDataCatalog> logger,
    IOptions<ServerSettings>? serverSettings = null,
    INijiiroCatalog? nijiiroCatalog = null) : IBlueCatalog
{
    private uint songHashVersion;
    private IReadOnlyList<BlueMusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, BlueMusicInfoEntry> musicInfos = new Dictionary<uint, BlueMusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<BlueTaikojukuEntry> taikojukuFileOrder = [];
    private IReadOnlyDictionary<uint, BlueTaikojukuEntry> taikojuku = new Dictionary<uint, BlueTaikojukuEntry>();
    private BlueItemShopCatalog itemShopCatalog = BlueItemShopCatalog.Disabled;
    private IReadOnlyDictionary<uint, BlueItemShopEntry> itemShop = new Dictionary<uint, BlueItemShopEntry>();
    private BlueBattleCatalog battleCatalog = BlueBattleCatalog.Unavailable;
    private IReadOnlyDictionary<uint, EventFolderData> eventFolders = new Dictionary<uint, EventFolderData>();
    private IReadOnlyDictionary<uint, BlueTelopEntry> telops = new Dictionary<uint, BlueTelopEntry>();
    private IReadOnlyDictionary<uint, BlueGachaEntry> gachas = new Dictionary<uint, BlueGachaEntry>();
    private IReadOnlyDictionary<uint, BlueTournamentEntry> tournaments = new Dictionary<uint, BlueTournamentEntry>();
    private BlueRecommendEntry recommend = BlueRecommendEntry.Empty;
    private IReadOnlyList<MovieData> movies = [];
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

    public GameEra Era => GameEra.Blue;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<BlueMusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, BlueMusicInfoEntry> BlueMusicInfos => musicInfos;

    public IReadOnlyList<BlueTaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

    public IReadOnlyDictionary<uint, BlueTaikojukuEntry> Taikojuku => taikojuku;

    public BlueItemShopCatalog ItemShopCatalog => itemShopCatalog;

    public IReadOnlyDictionary<uint, BlueItemShopEntry> ItemShop => itemShop;

    public BlueBattleCatalog BattleCatalog => battleCatalog;

    public IReadOnlyDictionary<uint, EventFolderData> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, BlueTelopEntry> Telops => telops;

    public IReadOnlyDictionary<uint, BlueGachaEntry> Gachas => gachas;

    public IReadOnlyDictionary<uint, BlueTournamentEntry> Tournaments => tournaments;

    public BlueRecommendEntry Recommend => recommend;

    public IReadOnlyList<MovieData> Movies => movies;

    public IReadOnlyList<Costume> GetCostumeList() => costumeList;

    public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => titleDictionary;

    public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => neiroDictionary;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        BlueRequiredDataFiles.ThrowIfMissing();

        await BootstrapCustomizationCatalogAsync(cancellationToken);

        var musicInfo = await new BlueMusicInfoLoader().LoadAsync(cancellationToken);
        var stars = await new BlueTuningLoader().LoadAsync(cancellationToken);
        var loadedTaikojukuFileOrder = await new BlueTaikojukuLoader().LoadAsync(cancellationToken);
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
                "Blue: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
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

        var blueSettings = GetBlueSettings();
        itemShopCatalog = await new BlueItemShopLoader().LoadAsync(blueSettings, cancellationToken);
        itemShop = itemShopCatalog.ActiveItemsByNo;
        battleCatalog = await new BlueBattleDataLoader().LoadAsync(cancellationToken);
        eventFolders = await new BlueEventFolderLoader().LoadAsync(new HashSet<uint>(musicInfos.Keys), cancellationToken);
        telops = await new BlueTelopLoader().LoadAsync(cancellationToken);
        gachas = await new BlueGachaLoader().LoadAsync(cancellationToken);
        tournaments = await new BlueTournamentLoader().LoadAsync(cancellationToken);
        recommend = await new BlueRecommendLoader().LoadAsync(new HashSet<uint>(musicInfos.Keys), cancellationToken);
        movies = await new BlueMovieLoader().LoadAsync(logger, cancellationToken);
        var blueCostumes = await new BlueCostumeLoader().LoadAsync(cancellationToken);
        var blueTitles = await new BlueTitleLoader().LoadAsync(cancellationToken);
        var blueNeiros = await new BlueNeiroLoader().LoadAsync(cancellationToken);
        var sharedNames = await LoadCustomizationNamesAsync(cancellationToken);
        var customizationCatalog = Ac15CustomizationCatalogComposer.Compose(
            blueCostumes,
            blueTitles,
            blueNeiros,
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
            "Loaded Blue catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones, {MovieCount} attract movies, item_shop_enabled={ItemShopEnabled}, battle_raw_files={BattleRawFileCount}",
            musicInfoFileOrder.Count,
            songHashVersion,
            taikojukuFileOrder.Count,
            stars.Count,
            costumeList.Count,
            titleDictionary.Count,
            neiroDictionary.Count,
            movies.Count,
            itemShopCatalog.IsEnabled,
            battleCatalog.Files.Count(file => file.IsPresent && file.IsXmlParsed));
    }

    private EraSettings GetBlueSettings()
    {
        return serverSettings?.Value.Eras.TryGetValue(nameof(GameEra.Blue), out var settings) == true
            ? settings
            : new EraSettings { GameDataPath = "wwwroot/data/blue/data", EnableShop = false };
    }

    private async Task BootstrapCustomizationCatalogAsync(CancellationToken cancellationToken)
    {
        var outputDirectory = PathHelper.GetDataPath(GameEra.Blue);
        var required = new[]
        {
            Path.Combine(outputDirectory, BlueCostumeLoader.FileName),
            Path.Combine(outputDirectory, BlueTitleLoader.FileName),
            Path.Combine(outputDirectory, BlueNeiroLoader.FileName)
        };

        if (required.All(File.Exists))
        {
            return;
        }

        var blueSettings = GetBlueSettings();
        if (!blueSettings.AutoExtractCatalog)
        {
            logger.LogInformation("Blue customization catalog auto-extract is disabled; continuing with empty or partial customization catalogs.");
            return;
        }

        var gameDataPath = ResolveConfiguredPath(blueSettings.GameDataPath);
        if (!Directory.Exists(gameDataPath))
        {
            logger.LogWarning("Blue customization catalog JSON is missing and game data path does not exist: {Path}", gameDataPath);
            return;
        }

        try
        {
            logger.LogInformation("Blue customization catalog JSON is missing; running first-run extraction from {Path}", gameDataPath);
            var stagingDirectory = Path.Combine(
                Path.GetTempPath(),
                "TaikoLocalServer-BlueCatalog",
                Guid.NewGuid().ToString("N"));

            try
            {
                await BlueCustomizationCatalogExtractor.ExtractAsync(
                    gameDataPath,
                    stagingDirectory,
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
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Blue customization catalog extraction failed; continuing with empty or partial customization catalogs.");
        }
    }

    private async Task<Ac15CustomizationCatalog> LoadCustomizationNamesAsync(CancellationToken cancellationToken)
    {
        var loader = new Ac15CustomizationNameCatalogLoader();
        var sharedNames = await loader.LoadAsync(PathHelper.GetSharedDataPath(), cancellationToken);
        var blueSettings = GetBlueSettings();
        if (string.IsNullOrWhiteSpace(blueSettings.CustomizationNameDataPath))
        {
            return sharedNames;
        }

        var overridePath = ResolveConfiguredPath(blueSettings.CustomizationNameDataPath);
        var overrideNames = await loader.LoadAsync(overridePath, cancellationToken);

        return MergeCustomizationNameCatalogs(sharedNames, overrideNames);
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
