using TaikoLocalServer.Application.Catalog.Yellow;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowTaikojukuProtocolTests
{
    [Fact]
    public async Task YellowTaikojukuHandler_ReturnsRequestedPacksFromYellowCatalog()
    {
        var catalog = new FakeYellowCatalog
        {
            MusicInfoFileOrder =
            [
                new YellowMusicInfoEntry { MusicId = "song101", SongNo = 101, FileOrder = 0 },
                new YellowMusicInfoEntry { MusicId = "song102", SongNo = 102, FileOrder = 1 }
            ],
            TaikojukuFileOrder =
            [
                new YellowTaikojukuEntry
                {
                    UniqueId = 9001,
                    ChallengeLevel = 1,
                    VerupNo = 44,
                    Songs =
                    [
                        new YellowTaikojukuSong { MusicId = "song101", SongNo = 101, Level = 0 },
                        new YellowTaikojukuSong { MusicId = "song102", SongNo = 102, Level = 3 }
                    ]
                }
            ]
        };
        var handler = new GetTaikojukuQueryHandler(
            new FakeGameDataCatalog(catalog),
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(
            new GetTaikojukuQuery(GameEra.Yellow, [1]),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        var pack = Assert.Single(response.Packs);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal(44u, pack.VerupNo);
        Assert.Equal([101u, 102u], pack.Songs.Select(song => song.SongNo).ToArray());
        Assert.Equal([0u, 3u], pack.Songs.Select(song => song.Level).ToArray());
    }

    private sealed class FakeGameDataCatalog(IYellowCatalog yellow) : IGameDataCatalog
    {
        public IEraGameDataCatalog For(GameEra era)
            => era == GameEra.Yellow ? yellow : throw new InvalidOperationException($"Unexpected era {era}");

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeYellowCatalog : IYellowCatalog
    {
        public GameEra Era => GameEra.Yellow;

        public IReadOnlyList<YellowMusicInfoEntry> MusicInfoFileOrder { get; init; } = [];

        public uint SongHashVersion { get; init; }

        public IReadOnlyDictionary<uint, YellowMusicInfoEntry> YellowMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => YellowMusicInfos.ToDictionary(pair => pair.Key, pair => (IMusicInfoEntry)pair.Value);

        public IReadOnlyList<YellowTaikojukuEntry> TaikojukuFileOrder { get; init; } = [];

        public IReadOnlyDictionary<uint, YellowTaikojukuEntry> Taikojuku
            => TaikojukuFileOrder.ToDictionary(entry => entry.ChallengeLevel);

        public YellowItemShopCatalog ItemShopCatalog { get; init; } = YellowItemShopCatalog.Disabled;

        public IReadOnlyDictionary<uint, YellowItemShopEntry> ItemShop => ItemShopCatalog.ActiveItemsByNo;

        public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; init; }
            = new Dictionary<uint, EventFolderData>();

        public IReadOnlyDictionary<uint, YellowTelopEntry> Telops { get; init; }
            = new Dictionary<uint, YellowTelopEntry>();

        public IReadOnlyDictionary<uint, YellowGachaEntry> Gachas { get; init; }
            = new Dictionary<uint, YellowGachaEntry>();

        public IReadOnlyDictionary<uint, YellowTournamentEntry> Tournaments { get; init; }
            = new Dictionary<uint, YellowTournamentEntry>();

        public YellowRecommendEntry Recommend { get; init; } = YellowRecommendEntry.Empty;

        public IReadOnlyList<MovieData> Movies { get; init; } = [];

        public IReadOnlyList<Costume> GetCostumeList() => [];

        public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => new Dictionary<uint, Title>();

        public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => new Dictionary<uint, Neiro>();

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
