using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
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
    private IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15MusicInfoEntry> musicInfos = new Dictionary<uint, Ac15MusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<Ac15TaikojukuEntry> taikojukuFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15TaikojukuEntry> taikojuku = new Dictionary<uint, Ac15TaikojukuEntry>();
    private Ac15ItemShopCatalog itemShopCatalog = Ac15ItemShopCatalog.Disabled;
    private IReadOnlyDictionary<uint, Ac15ItemShopEntry> itemShop = new Dictionary<uint, Ac15ItemShopEntry>();
    private BlueBattleCatalog battleCatalog = BlueBattleCatalog.Unavailable;
    private IReadOnlyDictionary<uint, EventFolderData> eventFolders = new Dictionary<uint, EventFolderData>();
    private IReadOnlyDictionary<uint, Ac15TelopEntry> telops = new Dictionary<uint, Ac15TelopEntry>();
    private IReadOnlyDictionary<uint, Ac15GachaEntry> gachas = new Dictionary<uint, Ac15GachaEntry>();
    private IReadOnlyDictionary<uint, Ac15TournamentEntry> tournaments = new Dictionary<uint, Ac15TournamentEntry>();
    private Ac15RecommendEntry recommend = Ac15RecommendEntry.Empty;
    private IReadOnlyList<MovieData> movies = [];
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

    public GameEra Era => GameEra.Blue;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> BlueMusicInfos => musicInfos;

    public IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

    public IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku => taikojuku;

    public Ac15ItemShopCatalog ItemShopCatalog => itemShopCatalog;

    public IReadOnlyDictionary<uint, Ac15ItemShopEntry> ItemShop => itemShop;

    public BlueBattleCatalog BattleCatalog => battleCatalog;

    public IReadOnlyDictionary<uint, EventFolderData> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops => telops;

    public IReadOnlyDictionary<uint, Ac15GachaEntry> Gachas => gachas;

    public IReadOnlyDictionary<uint, Ac15TournamentEntry> Tournaments => tournaments;

    public Ac15RecommendEntry Recommend => recommend;

    public IReadOnlyList<MovieData> Movies => movies;

    public IReadOnlyList<Costume> GetCostumeList() => costumeList;

    public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => titleDictionary;

    public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => neiroDictionary;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        BlueRequiredDataFiles.ThrowIfMissing();
        var blueSettings = GetBlueSettings();

        await Ac15CustomizationCatalogSupport.EnsureExtractedAsync(
            GameEra.Blue,
            blueSettings,
            BlueCostumeLoader.FileName,
            BlueTitleLoader.FileName,
            BlueNeiroLoader.FileName,
            logger,
            cancellationToken);

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

        itemShopCatalog = await new BlueItemShopLoader().LoadAsync(blueSettings, cancellationToken);
        itemShop = itemShopCatalog.ActiveItemsByNo;
        battleCatalog = await new BlueBattleDataLoader().LoadAsync(cancellationToken);
        eventFolders = await new BlueEventFolderLoader().LoadAsync(new HashSet<uint>(musicInfos.Keys), cancellationToken);
        telops = await new BlueTelopLoader().LoadAsync(cancellationToken);
        gachas = await new BlueGachaLoader().LoadAsync(cancellationToken);
        tournaments = await new BlueTournamentLoader().LoadAsync(cancellationToken);
        recommend = await new BlueRecommendLoader().LoadAsync(new HashSet<uint>(musicInfos.Keys), cancellationToken);
        movies = await new BlueMovieLoader().LoadAsync(logger, cancellationToken);
        var blueCustomization = await Ac15CustomizationCatalogSupport.LoadEraCatalogAsync(
            GameEra.Blue,
            BlueCostumeLoader.FileName,
            BlueTitleLoader.FileName,
            BlueNeiroLoader.FileName,
            cancellationToken);
        var sharedNames = await Ac15CustomizationCatalogSupport.LoadCustomizationNamesAsync(
            blueSettings,
            cancellationToken);
        var customizationCatalog = Ac15CustomizationCatalogComposer.Compose(
            blueCustomization.Costumes,
            blueCustomization.Titles,
            blueCustomization.Neiros,
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
}
