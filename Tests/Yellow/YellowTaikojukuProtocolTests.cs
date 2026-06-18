using TaikoLocalServer.Application.Catalog.Ac15;
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
                new Ac15MusicInfoEntry { MusicId = "song101", SongNo = 101, FileOrder = 0 },
                new Ac15MusicInfoEntry { MusicId = "song102", SongNo = 102, FileOrder = 1 }
            ],
            TaikojukuFileOrder =
            [
                new Ac15TaikojukuEntry
                {
                    UniqueId = 9001,
                    ChallengeLevel = 1,
                    VerupNo = 44,
                    Songs =
                    [
                        new Ac15TaikojukuSong { MusicId = "song101", SongNo = 101, Level = 0 },
                        new Ac15TaikojukuSong { MusicId = "song102", SongNo = 102, Level = 3 }
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

        public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder { get; init; } = [];

        public uint SongHashVersion { get; init; }

        public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> YellowMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => YellowMusicInfos.ToDictionary(pair => pair.Key, pair => (IMusicInfoEntry)pair.Value);

        public IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder { get; init; } = [];

        public IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku
            => TaikojukuFileOrder.ToDictionary(entry => entry.ChallengeLevel);

        public Ac15ItemShopCatalog ItemShopCatalog { get; init; } = Ac15ItemShopCatalog.Disabled;

        public IReadOnlyDictionary<uint, Ac15ItemShopEntry> ItemShop => ItemShopCatalog.ActiveItemsByNo;

        public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; init; }
            = new Dictionary<uint, EventFolderData>();

        public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; init; }
            = new Dictionary<uint, Ac15TelopEntry>();

        public IReadOnlyDictionary<uint, Ac15GachaEntry> Gachas { get; init; }
            = new Dictionary<uint, Ac15GachaEntry>();

        public IReadOnlyDictionary<uint, Ac15TournamentEntry> Tournaments { get; init; }
            = new Dictionary<uint, Ac15TournamentEntry>();

        public Ac15RecommendEntry Recommend { get; init; } = Ac15RecommendEntry.Empty;

        public IReadOnlyList<MovieData> Movies { get; init; } = [];

        public IReadOnlyList<Costume> GetCostumeList() => [];

        public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => new Dictionary<uint, Title>();

        public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => new Dictionary<uint, Neiro>();

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
