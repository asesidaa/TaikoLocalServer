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
    IOptions<ServerSettings>? serverSettings = null) : IGreenCatalog
{
    private uint songHashVersion;
    private IReadOnlyList<GreenMusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, GreenMusicInfoEntry> musicInfos = new Dictionary<uint, GreenMusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<GreenTaikojukuEntry> taikojukuFileOrder = [];
    private IReadOnlyDictionary<uint, GreenTaikojukuEntry> taikojuku = new Dictionary<uint, GreenTaikojukuEntry>();
    private IReadOnlyDictionary<uint, GreenItemShopEntry> itemShop = new Dictionary<uint, GreenItemShopEntry>();
    private IReadOnlyDictionary<uint, GreenEventFolderEntry> eventFolders = new Dictionary<uint, GreenEventFolderEntry>();
    private IReadOnlyDictionary<uint, GreenTelopEntry> telops = new Dictionary<uint, GreenTelopEntry>();
    private IReadOnlyDictionary<uint, GreenGachaEntry> gachas = new Dictionary<uint, GreenGachaEntry>();
    private IReadOnlyDictionary<uint, GreenTournamentEntry> tournaments = new Dictionary<uint, GreenTournamentEntry>();
    private GreenRecommendEntry recommend = GreenRecommendEntry.Empty;
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

    public IReadOnlyDictionary<uint, GreenItemShopEntry> ItemShop => itemShop;

    public IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, GreenTelopEntry> Telops => telops;

    public IReadOnlyDictionary<uint, GreenGachaEntry> Gachas => gachas;

    public IReadOnlyDictionary<uint, GreenTournamentEntry> Tournaments => tournaments;

    public GreenRecommendEntry Recommend => recommend;

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
        itemShop = await new GreenItemShopLoader().LoadAsync(cancellationToken);
        eventFolders = await new GreenEventFolderLoader().LoadAsync(cancellationToken);
        telops = await new GreenTelopLoader().LoadAsync(cancellationToken);
        gachas = await new GreenGachaLoader().LoadAsync(cancellationToken);
        tournaments = await new GreenTournamentLoader().LoadAsync(cancellationToken);
        recommend = await new GreenRecommendLoader().LoadAsync(
            new HashSet<uint>(musicInfos.Keys),
            cancellationToken);
        costumeList = await new GreenCostumeLoader().LoadAsync(cancellationToken);
        titleDictionary = await new GreenTitleLoader().LoadAsync(cancellationToken);
        neiroDictionary = await new GreenNeiroLoader().LoadAsync(cancellationToken);

        logger.LogInformation(
            "Loaded Green catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones",
            musicInfoFileOrder.Count,
            songHashVersion,
            taikojukuFileOrder.Count,
            stars.Count,
            costumeList.Count,
            titleDictionary.Count,
            neiroDictionary.Count);
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
            await GreenCatalogExtractor.ExtractAsync(
                new GreenExtractorOptions(gameDataPath, outputDirectory),
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Green customization catalog extraction failed; continuing with empty or partial customization catalogs.");
        }
    }

    private EraSettings GetGreenSettings()
    {
        return serverSettings?.Value.Eras.TryGetValue(nameof(GameEra.Green), out var settings) == true
            ? settings
            : new EraSettings();
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
}
