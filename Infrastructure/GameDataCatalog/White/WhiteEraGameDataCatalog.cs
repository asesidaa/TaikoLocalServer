using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.White;

public sealed class WhiteEraGameDataCatalog(
    ILogger<WhiteEraGameDataCatalog> logger,
    IOptions<ServerSettings>? serverSettings = null,
    INijiiroCatalog? nijiiroCatalog = null) : IWhiteCatalog
{
    public const string EventFolderFileName = "white_event_folder_data.json";
    public const string TelopFileName = "white_telop_data.json";
    public const string RecommendFileName = "white_recommend_songs.json";
    public const string MovieFileName = "white_movie_data.json";
    public const string TaikojukuVerupFileName = "white_taikojuku_verup_data.json";
    public const string CostumeFileName = "white_costume_data.json";
    public const string TitleFileName = "white_title_data.json";
    public const string NeiroFileName = "white_neiro_data.json";

    private uint songHashVersion;
    private IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15MusicInfoEntry> musicInfos = new Dictionary<uint, Ac15MusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<Ac15TaikojukuEntry> taikojukuFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15TaikojukuEntry> taikojuku = new Dictionary<uint, Ac15TaikojukuEntry>();
    private IReadOnlyDictionary<uint, EventFolderData> eventFolders = new Dictionary<uint, EventFolderData>();
    private IReadOnlyDictionary<uint, Ac15TelopEntry> telops = new Dictionary<uint, Ac15TelopEntry>();
    private Ac15RecommendEntry recommend = Ac15RecommendEntry.Empty;
    private IReadOnlyList<MovieData> movies = [];
    private IReadOnlyList<Ac15PresentItem> presents = [];
    private IReadOnlyList<Ac15SpecialBaidEntry> specialBaids = [];
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

    public GameEra Era => GameEra.White;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> WhiteMusicInfos => musicInfos;

    public IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

    public IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku => taikojuku;

    public IReadOnlyDictionary<uint, EventFolderData> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops => telops;

    public Ac15RecommendEntry Recommend => recommend;

    public IReadOnlyList<MovieData> Movies => movies;

    public IReadOnlyList<Ac15PresentItem> Presents => presents;

    public IReadOnlyList<Ac15SpecialBaidEntry> SpecialBaids => specialBaids;

    public IReadOnlyList<Costume> GetCostumeList() => costumeList;

    public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => titleDictionary;

    public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => neiroDictionary;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        WhiteRequiredDataFiles.ThrowIfMissing();

        var whiteSettings = GetWhiteSettings();
        await Ac15CustomizationCatalogSupport.EnsureExtractedAsync(
            GameEra.White,
            whiteSettings,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            logger,
            cancellationToken);

        var musicInfo = await Ac15MusicInfoLoader.LoadFromFileAsync(WhiteGameDataPaths.MusicInfoXml, cancellationToken);
        var stars = await Ac15TuningLoader.LoadFromFileAsync(
            WhiteGameDataPaths.TuningBin,
            nameof(GameEra.White),
            cancellationToken);
        var loadedTaikojukuFileOrder = await Ac15TaikojukuLoader.LoadFromFileAsync(
            WhiteGameDataPaths.MusicMedleyInfoXml,
            Path.Combine(PathHelper.GetDataPath(GameEra.White), TaikojukuVerupFileName),
            nameof(GameEra.White),
            cancellationToken);
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
                "White: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
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

        eventFolders = await Ac15EventFolderLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.White), EventFolderFileName),
            new HashSet<uint>(musicInfos.Keys),
            nameof(GameEra.White),
            cancellationToken);
        telops = await Ac15TelopLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.White), TelopFileName),
            cancellationToken);
        recommend = await Ac15RecommendLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.White), RecommendFileName),
            new HashSet<uint>(musicInfos.Keys),
            cancellationToken);
        movies = await Ac15MovieLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.White), MovieFileName),
            WhiteGameDataPaths.MovieDirectory,
            nameof(GameEra.White),
            logger,
            cancellationToken);
        presents = await Ac15PresentLoader.LoadFromFileAsync(
            WhiteGameDataPaths.PresentXml,
            cancellationToken);
        specialBaids = await Ac15SpecialBaidLoader.LoadFromFileAsync(
            WhiteGameDataPaths.SpecialBaidXml,
            cancellationToken);
        var whiteCustomization = await Ac15CustomizationCatalogSupport.LoadEraCatalogAsync(
            GameEra.White,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            cancellationToken);
        var sharedNames = await Ac15CustomizationCatalogSupport.LoadCustomizationNamesAsync(
            whiteSettings,
            cancellationToken);
        var customizationCatalog = Ac15CustomizationCatalogComposer.Compose(
            whiteCustomization.Costumes,
            whiteCustomization.Titles,
            whiteCustomization.Neiros,
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
            "Loaded White catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones, {MovieCount} attract movies, {PresentCount} present rows, {SpecialBaidCount} special BAID rows",
            musicInfoFileOrder.Count,
            songHashVersion,
            taikojukuFileOrder.Count,
            stars.Count,
            costumeList.Count,
            titleDictionary.Count,
            neiroDictionary.Count,
            movies.Count,
            presents.Count,
            specialBaids.Count);
    }

    private EraSettings GetWhiteSettings()
    {
        return serverSettings?.Value.Eras.TryGetValue(nameof(GameEra.White), out var settings) == true
            ? settings
            : new EraSettings
            {
                Enabled = true,
                AutoExtractCatalog = false,
                EnableShop = false,
                GameDataPath = "wwwroot/data/white/data"
            };
    }
}
