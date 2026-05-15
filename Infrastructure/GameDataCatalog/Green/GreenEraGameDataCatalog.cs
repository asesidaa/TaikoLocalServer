using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenEraGameDataCatalog(ILogger<GreenEraGameDataCatalog> logger) : IGreenCatalog
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

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        GreenRequiredDataFiles.ThrowIfMissing();

        var musicInfo = await new GreenMusicInfoLoader().LoadAsync(cancellationToken);
        songHashVersion = musicInfo.SongHashVersion;
        musicInfoFileOrder = musicInfo.Entries;
        musicInfos = musicInfo.Entries.ToDictionary(entry => entry.SongNo);
        sharedMusicInfos = musicInfos.ToDictionary(
            pair => pair.Key,
            pair => (IMusicInfoEntry)pair.Value);
        taikojukuFileOrder = await new GreenTaikojukuLoader().LoadAsync(cancellationToken);
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

        logger.LogInformation(
            "Loaded Green catalog: {SongCount} songs, song_hash_ver={SongHashVersion}, {TaikojukuCount} taikojuku packs",
            musicInfoFileOrder.Count,
            songHashVersion,
            taikojukuFileOrder.Count);
    }
}
