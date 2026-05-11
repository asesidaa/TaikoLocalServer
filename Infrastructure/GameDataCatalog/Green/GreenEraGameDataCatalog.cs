using Microsoft.Extensions.Logging;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenEraGameDataCatalog(ILogger<GreenEraGameDataCatalog> logger) : IGreenCatalog
{
    private IReadOnlyDictionary<uint, GreenMusicInfoEntry> musicInfos = new Dictionary<uint, GreenMusicInfoEntry>();
    private IReadOnlyDictionary<uint, IMusicInfoEntry> sharedMusicInfos = new Dictionary<uint, IMusicInfoEntry>();
    private IReadOnlyDictionary<uint, GreenTaikojukuEntry> taikojuku = new Dictionary<uint, GreenTaikojukuEntry>();
    private IReadOnlyDictionary<uint, GreenItemShopEntry> itemShop = new Dictionary<uint, GreenItemShopEntry>();
    private IReadOnlyDictionary<uint, GreenEventFolderEntry> eventFolders = new Dictionary<uint, GreenEventFolderEntry>();
    private IReadOnlyDictionary<uint, GreenTelopEntry> telops = new Dictionary<uint, GreenTelopEntry>();
    private IReadOnlyDictionary<uint, GreenGachaEntry> gachas = new Dictionary<uint, GreenGachaEntry>();
    private IReadOnlyDictionary<uint, GreenTournamentEntry> tournaments = new Dictionary<uint, GreenTournamentEntry>();

    public GameEra Era => GameEra.Green;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos => sharedMusicInfos;

    public IReadOnlyDictionary<uint, GreenMusicInfoEntry> GreenMusicInfos => musicInfos;

    public IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku => taikojuku;

    public IReadOnlyDictionary<uint, GreenItemShopEntry> ItemShop => itemShop;

    public IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders => eventFolders;

    public IReadOnlyDictionary<uint, GreenTelopEntry> Telops => telops;

    public IReadOnlyDictionary<uint, GreenGachaEntry> Gachas => gachas;

    public IReadOnlyDictionary<uint, GreenTournamentEntry> Tournaments => tournaments;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var greenDataPath = PathHelper.GetDataPath(GameEra.Green);
        if (!Directory.Exists(greenDataPath))
        {
            logger.LogWarning(
                "Green data path {Path} does not exist. Green endpoints will return empty data until binaries are dropped in.",
                greenDataPath);
            return;
        }

        musicInfos = await new GreenMusicInfoLoader().LoadAsync(cancellationToken);
        sharedMusicInfos = musicInfos.ToDictionary(
            pair => pair.Key,
            pair => (IMusicInfoEntry)pair.Value);
        taikojuku = await new GreenTaikojukuLoader().LoadAsync(cancellationToken);
        itemShop = await new GreenItemShopLoader().LoadAsync(cancellationToken);
        eventFolders = await new GreenEventFolderLoader().LoadAsync(cancellationToken);
        telops = await new GreenTelopLoader().LoadAsync(cancellationToken);
        gachas = await new GreenGachaLoader().LoadAsync(cancellationToken);
        tournaments = await new GreenTournamentLoader().LoadAsync(cancellationToken);
    }
}
