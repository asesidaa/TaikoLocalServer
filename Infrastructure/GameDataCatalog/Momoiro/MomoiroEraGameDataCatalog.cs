using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Momoiro;

public sealed class MomoiroEraGameDataCatalog(
    ILogger<MomoiroEraGameDataCatalog> logger,
    IOptions<ServerSettings>? serverSettings = null,
    INijiiroCatalog? nijiiroCatalog = null) : IMomoiroCatalog
{
    public const string TelopFileName = "momoiro_telop_data.json";
    public const string MovieFileName = "momoiro_movie_data.json";
    public const string CostumeFileName = "momoiro_costume_data.json";
    public const string TitleFileName = "momoiro_title_data.json";
    public const string NeiroFileName = "momoiro_neiro_data.json";

    private uint songHashVersion;
    private IReadOnlyList<ushort> songHashTable = [];
    private IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15MusicInfoEntry> musicInfos = new Dictionary<uint, Ac15MusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<Ac15TaikojukuEntry> daniFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15TelopEntry> telops = new Dictionary<uint, Ac15TelopEntry>();
    private IReadOnlyList<MovieData> movies = [];
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

    public GameEra Era => GameEra.Momoiro;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<ushort> SongHashTable => songHashTable;

    public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> MomoiroMusicInfos => musicInfos;

    public IReadOnlyList<Ac15TaikojukuEntry> DaniFileOrder => daniFileOrder;

    public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops => telops;

    public IReadOnlyList<MovieData> Movies => movies;

    public IReadOnlyList<Costume> GetCostumeList() => costumeList;

    public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => titleDictionary;

    public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => neiroDictionary;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        MomoiroRequiredDataFiles.ThrowIfMissing();

        var momoiroSettings = GetMomoiroSettings();
        await Ac15CustomizationCatalogSupport.EnsureExtractedAsync(
            GameEra.Momoiro,
            momoiroSettings,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            logger,
            cancellationToken);

        var musicInfo = await Ac15MusicInfoLoader.LoadFromFileAsync(MomoiroGameDataPaths.MusicInfoXml, cancellationToken);
        var stars = await Ac15TuningLoader.LoadFromFileAsync(
            MomoiroGameDataPaths.TuningBin,
            nameof(GameEra.Momoiro),
            cancellationToken);
        var loadedDaniFileOrder = await Ac15TaikojukuLoader.LoadFromFileAsync(
            MomoiroGameDataPaths.MusicMedleyInfoXml,
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
            .Where(entry => !IsMedleyMusicInfoEntry(entry))
            .Select(entry => entry.MusicId)
            .ToList();
        if (missingTuning.Count > 0)
        {
            logger.LogWarning(
                "Momoiro: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
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
        telops = await Ac15TelopLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Momoiro), TelopFileName),
            cancellationToken);
        movies = await Ac15MovieLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Momoiro), MovieFileName),
            MomoiroGameDataPaths.MovieDirectory,
            nameof(GameEra.Momoiro),
            logger,
            cancellationToken);
        var momoiroCustomization = await Ac15CustomizationCatalogSupport.LoadEraCatalogAsync(
            GameEra.Momoiro,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            cancellationToken);
        var sharedNames = await Ac15CustomizationCatalogSupport.LoadCustomizationNamesAsync(
            momoiroSettings,
            cancellationToken);
        var customizationCatalog = Ac15CustomizationCatalogComposer.Compose(
            momoiroCustomization.Costumes,
            momoiroCustomization.Titles,
            momoiroCustomization.Neiros,
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
            "Loaded Momoiro catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {DaniCount} Dani rows, {StarCount} tuning star rows, {TelopCount} telops, {MovieCount} attract movies, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones",
            musicInfoFileOrder.Count,
            songHashVersion,
            daniFileOrder.Count,
            stars.Count,
            telops.Count,
            movies.Count,
            costumeList.Count,
            titleDictionary.Count,
            neiroDictionary.Count);
    }

    private static bool IsMedleyMusicInfoEntry(Ac15MusicInfoEntry entry)
        => entry.MusicId.StartsWith("medley", StringComparison.OrdinalIgnoreCase);

    private EraSettings GetMomoiroSettings()
    {
        return serverSettings?.Value.Eras.TryGetValue(nameof(GameEra.Momoiro), out var settings) == true
            ? settings
            : new EraSettings
            {
                Enabled = true,
                AutoExtractCatalog = false,
                GameDataPath = "wwwroot/data/momoiro/data"
            };
    }
}
