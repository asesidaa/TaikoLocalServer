using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Yellow;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowEraGameDataCatalog(
    ILogger<YellowEraGameDataCatalog> logger,
    IOptions<ServerSettings>? serverSettings = null,
    INijiiroCatalog? nijiiroCatalog = null) : IYellowCatalog
{
    public const string CostumeFileName = "yellow_costume_data.json";
    public const string TitleFileName = "yellow_title_data.json";
    public const string NeiroFileName = "yellow_neiro_data.json";

    private uint songHashVersion;
    private IReadOnlyList<YellowMusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, YellowMusicInfoEntry> musicInfos = new Dictionary<uint, YellowMusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<YellowTaikojukuEntry> taikojukuFileOrder = [];
    private IReadOnlyDictionary<uint, YellowTaikojukuEntry> taikojuku = new Dictionary<uint, YellowTaikojukuEntry>();
    private YellowItemShopCatalog itemShopCatalog = YellowItemShopCatalog.Disabled;
    private IReadOnlyDictionary<uint, YellowItemShopEntry> itemShop = new Dictionary<uint, YellowItemShopEntry>();
    private IReadOnlyDictionary<uint, EventFolderData> eventFolders = new Dictionary<uint, EventFolderData>();
    private IReadOnlyDictionary<uint, YellowTelopEntry> telops = new Dictionary<uint, YellowTelopEntry>();
    private IReadOnlyDictionary<uint, YellowGachaEntry> gachas = new Dictionary<uint, YellowGachaEntry>();
    private IReadOnlyDictionary<uint, YellowTournamentEntry> tournaments = new Dictionary<uint, YellowTournamentEntry>();
    private YellowRecommendEntry recommend = YellowRecommendEntry.Empty;
    private IReadOnlyList<MovieData> movies = [];
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

    public GameEra Era => GameEra.Yellow;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<YellowMusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, YellowMusicInfoEntry> YellowMusicInfos => musicInfos;

    public IReadOnlyList<YellowTaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

    public IReadOnlyDictionary<uint, YellowTaikojukuEntry> Taikojuku => taikojuku;

    public YellowItemShopCatalog ItemShopCatalog => itemShopCatalog;

    public IReadOnlyDictionary<uint, YellowItemShopEntry> ItemShop => itemShop;

    public IReadOnlyDictionary<uint, EventFolderData> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, YellowTelopEntry> Telops => telops;

    public IReadOnlyDictionary<uint, YellowGachaEntry> Gachas => gachas;

    public IReadOnlyDictionary<uint, YellowTournamentEntry> Tournaments => tournaments;

    public YellowRecommendEntry Recommend => recommend;

    public IReadOnlyList<MovieData> Movies => movies;

    public IReadOnlyList<Costume> GetCostumeList() => costumeList;

    public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => titleDictionary;

    public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => neiroDictionary;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        YellowRequiredDataFiles.ThrowIfMissing();
        var yellowSettings = GetYellowSettings();

        await Ac15CustomizationCatalogSupport.EnsureExtractedAsync(
            GameEra.Yellow,
            yellowSettings,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            logger,
            cancellationToken);

        var musicInfo = await new YellowMusicInfoLoader().LoadAsync(cancellationToken);
        var stars = await new YellowTuningLoader().LoadAsync(cancellationToken);
        var loadedTaikojukuFileOrder = await new YellowTaikojukuLoader().LoadAsync(cancellationToken);
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
                "Yellow: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
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

        itemShopCatalog = await new YellowItemShopLoader().LoadAsync(yellowSettings, cancellationToken);
        itemShop = itemShopCatalog.ActiveItemsByNo;
        eventFolders = await new YellowEventFolderLoader().LoadAsync(
            new HashSet<uint>(musicInfos.Keys),
            cancellationToken);
        telops = await new YellowTelopLoader().LoadAsync(cancellationToken);
        gachas = await new YellowGachaLoader().LoadAsync(cancellationToken);
        tournaments = await new YellowTournamentLoader().LoadAsync(cancellationToken);
        recommend = await new YellowRecommendLoader().LoadAsync(
            new HashSet<uint>(musicInfos.Keys),
            cancellationToken);
        movies = await new YellowMovieLoader().LoadAsync(logger, cancellationToken);
        var yellowCustomization = await Ac15CustomizationCatalogSupport.LoadEraCatalogAsync(
            GameEra.Yellow,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            cancellationToken);
        var sharedNames = await Ac15CustomizationCatalogSupport.LoadCustomizationNamesAsync(
            yellowSettings,
            cancellationToken);
        var customizationCatalog = Ac15CustomizationCatalogComposer.Compose(
            yellowCustomization.Costumes,
            yellowCustomization.Titles,
            yellowCustomization.Neiros,
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
            "Loaded Yellow catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones, {MovieCount} attract movies, item_shop_enabled={ItemShopEnabled}",
            musicInfoFileOrder.Count,
            songHashVersion,
            taikojukuFileOrder.Count,
            stars.Count,
            costumeList.Count,
            titleDictionary.Count,
            neiroDictionary.Count,
            movies.Count,
            itemShopCatalog.IsEnabled);
    }

    private EraSettings GetYellowSettings()
    {
        return serverSettings?.Value.Eras.TryGetValue(nameof(GameEra.Yellow), out var settings) == true
            ? settings
            : new EraSettings
            {
                Enabled = true,
                AutoExtractCatalog = false,
                EnableShop = false,
                GameDataPath = "wwwroot/data/yellow/data"
            };
    }
}
