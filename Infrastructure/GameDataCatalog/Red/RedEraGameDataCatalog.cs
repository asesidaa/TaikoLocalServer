using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Ac15.DonChallenge;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Red;

public sealed class RedEraGameDataCatalog(
    ILogger<RedEraGameDataCatalog> logger,
    IOptions<ServerSettings>? serverSettings = null,
    INijiiroCatalog? nijiiroCatalog = null) : IRedCatalog
{
    public const string EventFolderFileName = "red_event_folder_data.json";
    public const string TelopFileName = "red_telop_data.json";
    public const string RecommendFileName = "red_recommend_songs.json";
    public const string MovieFileName = "red_movie_data.json";
    public const string TaikojukuVerupFileName = "red_taikojuku_verup_data.json";
    public const string CostumeFileName = "red_costume_data.json";
    public const string TitleFileName = "red_title_data.json";
    public const string NeiroFileName = "red_neiro_data.json";
    public const string DonChallengeFileName = "red_challenge_compe_data.json";

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
    private IReadOnlyList<Costume> costumeList = [];
    private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();
    private Ac15DonChallengeCatalog donChallenge = Ac15DonChallengeCatalog.Disabled;

    public GameEra Era => GameEra.Red;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> RedMusicInfos => musicInfos;

    public IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

    public IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku => taikojuku;

    public IReadOnlyDictionary<uint, EventFolderData> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops => telops;

    public Ac15RecommendEntry Recommend => recommend;

    public IReadOnlyList<MovieData> Movies => movies;

    public Ac15DonChallengeCatalog DonChallenge => donChallenge;

    public IReadOnlyList<Costume> GetCostumeList() => costumeList;

    public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => titleDictionary;

    public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => neiroDictionary;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        RedRequiredDataFiles.ThrowIfMissing();

        var redSettings = GetRedSettings();
        await Ac15CustomizationCatalogSupport.EnsureExtractedAsync(
            GameEra.Red,
            redSettings,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            logger,
            cancellationToken);

        var musicInfo = await Ac15MusicInfoLoader.LoadFromFileAsync(RedGameDataPaths.MusicInfoXml, cancellationToken);
        var stars = await Ac15TuningLoader.LoadFromFileAsync(
            RedGameDataPaths.TuningBin,
            nameof(GameEra.Red),
            cancellationToken);
        var loadedTaikojukuFileOrder = await Ac15TaikojukuLoader.LoadFromFileAsync(
            RedGameDataPaths.MusicMedleyInfoXml,
            Path.Combine(PathHelper.GetDataPath(GameEra.Red), TaikojukuVerupFileName),
            nameof(GameEra.Red),
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
                "Red: {Count} musicinfo entries have no tuning record; using star=0 (examples: {Examples})",
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
            Path.Combine(PathHelper.GetDataPath(GameEra.Red), EventFolderFileName),
            new HashSet<uint>(musicInfos.Keys),
            nameof(GameEra.Red),
            cancellationToken);
        telops = await Ac15TelopLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Red), TelopFileName),
            cancellationToken);
        recommend = await Ac15RecommendLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Red), RecommendFileName),
            new HashSet<uint>(musicInfos.Keys),
            cancellationToken);
        movies = await Ac15MovieLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Red), MovieFileName),
            RedGameDataPaths.MovieDirectory,
            nameof(GameEra.Red),
            logger,
            cancellationToken);
        donChallenge = await Ac15DonChallengeLoader.LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Red), DonChallengeFileName),
            redSettings.IsDonChallengeEnabled(),
            redSettings.GetActiveDonChallengeBundleId(),
            nameof(GameEra.Red),
            cancellationToken);
        var redCustomization = await Ac15CustomizationCatalogSupport.LoadEraCatalogAsync(
            GameEra.Red,
            CostumeFileName,
            TitleFileName,
            NeiroFileName,
            cancellationToken);
        var sharedNames = await Ac15CustomizationCatalogSupport.LoadCustomizationNamesAsync(redSettings, cancellationToken);
        var customizationCatalog = Ac15CustomizationCatalogComposer.Compose(
            redCustomization.Costumes,
            redCustomization.Titles,
            redCustomization.Neiros,
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
            "Loaded Red catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs, {StarCount} tuning star rows, {CostumeCount} costumes, {TitleCount} titles, {NeiroCount} tones, {MovieCount} attract movies, DonChallenge enabled={DonChallengeEnabled}",
            musicInfoFileOrder.Count,
            songHashVersion,
            taikojukuFileOrder.Count,
            stars.Count,
            costumeList.Count,
            titleDictionary.Count,
            neiroDictionary.Count,
            movies.Count,
            donChallenge.Enabled);
    }

    private EraSettings GetRedSettings()
    {
        return serverSettings?.Value.Eras.TryGetValue(nameof(GameEra.Red), out var settings) == true
            ? settings
            : new EraSettings
            {
                Enabled = true,
                AutoExtractCatalog = false,
                EnableShop = false,
                GameDataPath = "wwwroot/data/red/data"
            };
    }
}
