using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Murasaki;

public sealed class MurasakiEraGameDataCatalog(
    ILogger<MurasakiEraGameDataCatalog> logger,
    IOptions<ServerSettings>? serverSettings = null,
    INijiiroCatalog? nijiiroCatalog = null) : IMurasakiCatalog
{
    public const string EventFolderFileName = "murasaki_event_folder_data.json";
    public const string TelopFileName = "murasaki_telop_data.json";
    public const string MovieFileName = "murasaki_movie_data.json";
    public const string TaikojukuVerupFileName = "murasaki_taikojuku_verup_data.json";
    public const string CostumeFileName = "murasaki_costume_data.json";
    public const string TitleFileName = "murasaki_title_data.json";
    public const string NeiroFileName = "murasaki_neiro_data.json";

    private uint songHashVersion;
    private IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15MusicInfoEntry> musicInfos = new Dictionary<uint, Ac15MusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<Ac15TaikojukuEntry> taikojukuFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15TaikojukuEntry> taikojuku = new Dictionary<uint, Ac15TaikojukuEntry>();
    private IReadOnlyDictionary<uint, EventFolderData> eventFolders = new Dictionary<uint, EventFolderData>();
    private IReadOnlyDictionary<uint, Ac15TelopEntry> telops = new Dictionary<uint, Ac15TelopEntry>();
    private IReadOnlyList<MovieData> movies = [];
    private IReadOnlyList<Ac15PresentItem> presents = [];
    private IReadOnlyList<Ac15SpecialBaidEntry> specialBaids = [];
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

    public GameEra Era => GameEra.Murasaki;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> MurasakiMusicInfos => musicInfos;

    public IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

    public IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku => taikojuku;

    public IReadOnlyDictionary<uint, EventFolderData> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops => telops;

    public IReadOnlyList<MovieData> Movies => movies;

    public IReadOnlyList<Ac15PresentItem> Presents => presents;

    public IReadOnlyList<Ac15SpecialBaidEntry> SpecialBaids => specialBaids;

    public IReadOnlyList<Costume> GetCostumeList() => costumeList;

    public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => titleDictionary;

    public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => neiroDictionary;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        MurasakiRequiredDataFiles.ThrowIfMissing();

        var murasakiSettings = GetMurasakiSettings();
        await Ac15CustomizationCatalogSupport.EnsureExtractedAsync(
            GameEra.Murasaki,
            murasakiSettings,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            logger,
            cancellationToken);

        var musicInfo = await Ac15MusicInfoLoader.LoadFromFileAsync(MurasakiGameDataPaths.MusicInfoXml, cancellationToken);
        var stars = await Ac15TuningLoader.LoadFromFileAsync(
            MurasakiGameDataPaths.TuningBin,
            nameof(GameEra.Murasaki),
            cancellationToken);
        var loadedTaikojukuFileOrder = await Ac15TaikojukuLoader.LoadFromFileAsync(
            MurasakiGameDataPaths.MusicMedleyInfoXml,
            Path.Combine(PathHelper.GetDataPath(GameEra.Murasaki), TaikojukuVerupFileName),
            nameof(GameEra.Murasaki),
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
                "Murasaki: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
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
            Path.Combine(PathHelper.GetDataPath(GameEra.Murasaki), EventFolderFileName),
            new HashSet<uint>(musicInfos.Keys),
            nameof(GameEra.Murasaki),
            cancellationToken);
        telops = await Ac15TelopLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Murasaki), TelopFileName),
            cancellationToken);
        movies = await Ac15MovieLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Murasaki), MovieFileName),
            MurasakiGameDataPaths.MovieDirectory,
            nameof(GameEra.Murasaki),
            logger,
            cancellationToken);
        presents = await Ac15PresentLoader.LoadFromFileAsync(
            MurasakiGameDataPaths.PresentXml,
            cancellationToken);
        specialBaids = await Ac15SpecialBaidLoader.LoadFromFileAsync(
            MurasakiGameDataPaths.SpecialBaidXml,
            cancellationToken);
        var murasakiCustomization = await Ac15CustomizationCatalogSupport.LoadEraCatalogAsync(
            GameEra.Murasaki,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            cancellationToken);
        var sharedNames = await Ac15CustomizationCatalogSupport.LoadCustomizationNamesAsync(
            murasakiSettings,
            cancellationToken);
        var customizationCatalog = Ac15CustomizationCatalogComposer.Compose(
            murasakiCustomization.Costumes,
            murasakiCustomization.Titles,
            murasakiCustomization.Neiros,
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
            "Loaded Murasaki catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones, {MovieCount} attract movies, {PresentCount} present rows, {SpecialBaidCount} special BAID rows",
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

    private EraSettings GetMurasakiSettings()
    {
        return serverSettings?.Value.Eras.TryGetValue(nameof(GameEra.Murasaki), out var settings) == true
            ? settings
            : new EraSettings
            {
                Enabled = true,
                AutoExtractCatalog = false,
                EnableShop = false,
                GameDataPath = "wwwroot/data/murasaki/data"
            };
    }
}
