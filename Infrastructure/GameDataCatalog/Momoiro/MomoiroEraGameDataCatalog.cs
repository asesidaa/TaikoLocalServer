using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Momoiro;

public sealed class MomoiroEraGameDataCatalog(
    ILogger<MomoiroEraGameDataCatalog> logger) : IMomoiroCatalog
{
    public const string TelopFileName = "momoiro_telop_data.json";
    public const string MovieFileName = "momoiro_movie_data.json";

    private uint songHashVersion;
    private IReadOnlyList<ushort> songHashTable = [];
    private IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15MusicInfoEntry> musicInfos = new Dictionary<uint, Ac15MusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyList<Ac15TaikojukuEntry> daniFileOrder = [];
    private IReadOnlyDictionary<uint, Ac15TelopEntry> telops = new Dictionary<uint, Ac15TelopEntry>();
    private IReadOnlyList<MovieData> movies = [];

    public GameEra Era => GameEra.Momoiro;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public uint SongHashVersion => songHashVersion;

    public IReadOnlyList<ushort> SongHashTable => songHashTable;

    public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

    public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> MomoiroMusicInfos => musicInfos;

    public IReadOnlyList<Ac15TaikojukuEntry> DaniFileOrder => daniFileOrder;

    public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops => telops;

    public IReadOnlyList<MovieData> Movies => movies;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        MomoiroRequiredDataFiles.ThrowIfMissing();

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

        logger.LogInformation(
            "Loaded Momoiro catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {DaniCount} Dani rows, {StarCount} tuning star rows, {TelopCount} telops, {MovieCount} attract movies",
            musicInfoFileOrder.Count,
            songHashVersion,
            daniFileOrder.Count,
            stars.Count,
            telops.Count,
            movies.Count);
    }

    private static bool IsMedleyMusicInfoEntry(Ac15MusicInfoEntry entry)
        => entry.MusicId.StartsWith("medley", StringComparison.OrdinalIgnoreCase);
}
