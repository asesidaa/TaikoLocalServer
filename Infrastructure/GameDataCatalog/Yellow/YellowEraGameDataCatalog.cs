using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
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
    private IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15MusicInfoEntry> musicInfos = new Dictionary<uint, Ac15MusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<Ac15TaikojukuEntry> taikojukuFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15TaikojukuEntry> taikojuku = new Dictionary<uint, Ac15TaikojukuEntry>();
    private Ac15ItemShopCatalog itemShopCatalog = Ac15ItemShopCatalog.Disabled;
    private IReadOnlyDictionary<uint, Ac15ItemShopEntry> itemShop = new Dictionary<uint, Ac15ItemShopEntry>();
    private IReadOnlyDictionary<uint, EventFolderData> eventFolders = new Dictionary<uint, EventFolderData>();
    private IReadOnlyDictionary<uint, Ac15TelopEntry> telops = new Dictionary<uint, Ac15TelopEntry>();
    private IReadOnlyDictionary<uint, Ac15GachaEntry> gachas = new Dictionary<uint, Ac15GachaEntry>();
    private IReadOnlyDictionary<uint, Ac15TournamentEntry> tournaments = new Dictionary<uint, Ac15TournamentEntry>();
    private IReadOnlyList<MovieData> movies = [];
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

    public GameEra Era => GameEra.Yellow;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> YellowMusicInfos => musicInfos;

    public IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

    public IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku => taikojuku;

    public Ac15ItemShopCatalog ItemShopCatalog => itemShopCatalog;

    public IReadOnlyDictionary<uint, Ac15ItemShopEntry> ItemShop => itemShop;

    public IReadOnlyDictionary<uint, EventFolderData> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops => telops;

    public IReadOnlyDictionary<uint, Ac15GachaEntry> Gachas => gachas;

    public IReadOnlyDictionary<uint, Ac15TournamentEntry> Tournaments => tournaments;

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
