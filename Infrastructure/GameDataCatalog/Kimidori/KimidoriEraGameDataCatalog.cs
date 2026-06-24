using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Kimidori;

public sealed class KimidoriEraGameDataCatalog(
    ILogger<KimidoriEraGameDataCatalog> logger,
    IOptions<ServerSettings>? serverSettings = null,
    INijiiroCatalog? nijiiroCatalog = null) : IKimidoriCatalog
{
    public const string EventFolderFileName = "kimidori_event_folder_data.json";
    public const string TelopFileName = "kimidori_telop_data.json";
    public const string MovieFileName = "kimidori_movie_data.json";
    public const string CostumeFileName = "kimidori_costume_data.json";
    public const string TitleFileName = "kimidori_title_data.json";
    public const string NeiroFileName = "kimidori_neiro_data.json";

    private uint songHashVersion;
    private IReadOnlyList<ushort> songHashTable = [];
    private IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15MusicInfoEntry> musicInfos = new Dictionary<uint, Ac15MusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<Ac15TaikojukuEntry> daniFileOrder = [];
    private IReadOnlyDictionary<uint, EventFolderData> eventFolders = new Dictionary<uint, EventFolderData>();
    private IReadOnlyDictionary<uint, Ac15TelopEntry> telops = new Dictionary<uint, Ac15TelopEntry>();
    private IReadOnlyList<MovieData> movies = [];
    private IReadOnlyList<Ac15PresentItem> presents = [];
    private IReadOnlyList<Ac15SpecialBaidEntry> specialBaids = [];
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

    public GameEra Era => GameEra.Kimidori;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<ushort> SongHashTable => songHashTable;

    public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> KimidoriMusicInfos => musicInfos;

    public IReadOnlyList<Ac15TaikojukuEntry> DaniFileOrder => daniFileOrder;

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
        KimidoriRequiredDataFiles.ThrowIfMissing();

        var kimidoriSettings = GetKimidoriSettings();
        await Ac15CustomizationCatalogSupport.EnsureExtractedAsync(
            GameEra.Kimidori,
            kimidoriSettings,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            logger,
            cancellationToken);

        var musicInfo = await Ac15MusicInfoLoader.LoadFromFileAsync(KimidoriGameDataPaths.MusicInfoXml, cancellationToken);
        var stars = await Ac15TuningLoader.LoadFromFileAsync(
            KimidoriGameDataPaths.TuningBin,
            nameof(GameEra.Kimidori),
            cancellationToken);
        var loadedDaniFileOrder = await Ac15TaikojukuLoader.LoadFromFileAsync(
            KimidoriGameDataPaths.MusicMedleyInfoXml,
            cancellationToken);
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
            .Where(entry => !stars.ContainsKey(entry.MusicId))
            .Where(entry => !IsDaniMedleyMusicInfoEntry(entry))
            .Select(entry => entry.MusicId)
            .ToList();
        if (missingTuning.Count > 0)
        {
            logger.LogWarning(
                "Kimidori: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
                missingTuning.Count,
                string.Join(", ", missingTuning.Take(5)));
        }

        songHashVersion = musicInfo.SongHashVersion;
        songHashTable = Ac15SongHashCodec.BuildTable(enrichedEntries.Select(entry => entry.SongNo));
        musicInfoFileOrder = enrichedEntries;
        musicInfos = enrichedEntries.ToDictionary(entry => entry.SongNo);
        sharedMusicInfos = musicInfos.ToDictionary(
            pair => pair.Key,
            pair => (IMusicInfoEntry)pair.Value);
        daniFileOrder = loadedDaniFileOrder;

        eventFolders = await Ac15EventFolderLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Kimidori), EventFolderFileName),
            new HashSet<uint>(musicInfos.Keys),
            nameof(GameEra.Kimidori),
            cancellationToken);
        telops = await Ac15TelopLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Kimidori), TelopFileName),
            cancellationToken);
        movies = await Ac15MovieLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Kimidori), MovieFileName),
            KimidoriGameDataPaths.MovieDirectory,
            nameof(GameEra.Kimidori),
            logger,
            cancellationToken);
        presents = File.Exists(KimidoriGameDataPaths.PresentXml)
            ? await Ac15PresentLoader.LoadFromFileAsync(KimidoriGameDataPaths.PresentXml, cancellationToken)
            : [];
        specialBaids = File.Exists(KimidoriGameDataPaths.SpecialBaidXml)
            ? await Ac15SpecialBaidLoader.LoadFromFileAsync(KimidoriGameDataPaths.SpecialBaidXml, cancellationToken)
            : [];
        var kimidoriCustomization = await Ac15CustomizationCatalogSupport.LoadEraCatalogAsync(
            GameEra.Kimidori,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            cancellationToken);
        var sharedNames = await Ac15CustomizationCatalogSupport.LoadCustomizationNamesAsync(
            kimidoriSettings,
            cancellationToken);
        var customizationCatalog = Ac15CustomizationCatalogComposer.Compose(
            kimidoriCustomization.Costumes,
            kimidoriCustomization.Titles,
            kimidoriCustomization.Neiros,
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
            "Loaded Kimidori catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {DaniCount} Dani courses, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones, {MovieCount} attract movies, {PresentCount} present rows, {SpecialBaidCount} special BAID rows",
            musicInfoFileOrder.Count,
            songHashVersion,
            daniFileOrder.Count,
            stars.Count,
            costumeList.Count,
            titleDictionary.Count,
            neiroDictionary.Count,
            movies.Count,
            presents.Count,
            specialBaids.Count);
    }

    private EraSettings GetKimidoriSettings()
    {
        return serverSettings?.Value.Eras.TryGetValue(nameof(GameEra.Kimidori), out var settings) == true
            ? settings
            : new EraSettings
            {
                Enabled = true,
                AutoExtractCatalog = false,
                EnableShop = false,
                GameDataPath = "wwwroot/data/kimidori/data"
            };
    }

    private static bool IsDaniMedleyMusicInfoEntry(Ac15MusicInfoEntry entry)
        => entry.MusicId.StartsWith("medley", StringComparison.OrdinalIgnoreCase)
           && string.Equals(entry.PartsSet, "dojo", StringComparison.OrdinalIgnoreCase);
}
