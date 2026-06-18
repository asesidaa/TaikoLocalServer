using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowInitialDataProtocolTests
{
    [Fact]
    public void CatalogSnapshotFactory_FromYellowMapsYellowCatalogRows()
    {
        var yellow = YellowCatalogWithOptionalRows();

        var snapshot = Ac15CatalogSnapshotFactory.FromYellow(yellow);

        Assert.Equal(987u, snapshot.SongHashVersion);
        Assert.Equal([101u, 102u], snapshot.SongNoesInFileOrder);
        Assert.Equal(44u, snapshot.EventFolders[44].FolderId);
        Assert.Equal(31u, snapshot.Telops[31].TelopId);
        Assert.Equal(102u, snapshot.RecommendSong);
        Assert.Equal([101u, 102u], snapshot.RecommendBestSongs);
        Assert.True(snapshot.ItemShopCatalog.IsEnabled);
        Assert.Equal(7u, snapshot.ItemShopCatalog.ActiveSeasonId);
        Assert.Equal(14u, snapshot.ItemShopCatalog.ActiveSeason!.VerupNo);
        Assert.Equal(1u, snapshot.ItemShopCatalog.ActiveSeason.Items[0].ItemNo);
        Assert.Equal(Ac15ShopItemType.Song, snapshot.ItemShopCatalog.ActiveSeason.Items[0].ItemType);
        Assert.Equal(1u, snapshot.TaikojukuPacks[0].ChallengeLevel);
        Assert.Equal(101u, snapshot.TaikojukuPacks[0].Songs[0].SongNo);
    }

    [Fact]
    public async Task YellowInitialDataHandler_ReturnsCatalogBackedYellowRows()
    {
        var handler = new GetInitialDataQueryHandler(
            new FakeGameDataCatalog(YellowCatalogWithOptionalRows()),
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Yellow), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(987u, response.SongHashVer);
        Assert.True(response.IsDanplay);
        Assert.True(response.IsItemshop);
        Assert.True((response.DefaultSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
        Assert.True((response.DefaultSongFlg[102 >> 3] & (1 << (102 & 7))) == 0);
        Assert.Equal(31u, Assert.Single(response.AryTelopDatas).InfoId);
        Assert.Equal(44u, Assert.Single(response.AryEventFolderDatas).InfoId);
        Assert.Equal(1u, Assert.Single(response.AryTaikojukuDatas).InfoId);
        Assert.Equal(7u, Assert.Single(response.AryItemShopDatas).InfoId);
        Assert.Empty(response.AryLegaltermsDatas);
        Assert.Null(response.IsBattleplay);
        Assert.Null(response.ReleaseBattleStageFlg);
        Assert.Null(response.ReleaseBattleSpecialFlg);
    }

    [Fact]
    public async Task YellowInitialDataHandler_OptionalCatalogsCanStayEmpty()
    {
        var handler = new GetInitialDataQueryHandler(
            new FakeGameDataCatalog(YellowCatalogWithoutOptionalRows()),
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Yellow), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.False(response.IsItemshop);
        Assert.Empty(response.AryTelopDatas);
        Assert.Empty(response.AryEventFolderDatas);
        Assert.Empty(response.AryItemShopDatas);
        Assert.Empty(response.AryLegaltermsDatas);
    }

    private static FakeYellowCatalog YellowCatalogWithOptionalRows() => new()
    {
        MusicInfoFileOrder =
        [
            new Ac15MusicInfoEntry { MusicId = "song101", SongNo = 101, FileOrder = 0 },
            new Ac15MusicInfoEntry { MusicId = "song102", SongNo = 102, FileOrder = 1 }
        ],
        SongHashVersion = 987,
        EventFolders = new Dictionary<uint, EventFolderData>
        {
            [44] = new() { FolderId = 44, VerupNo = 5 }
        },
        Telops = new Dictionary<uint, Ac15TelopEntry>
        {
            [31] = new() { TelopId = 31, VerupNo = 6, Message = "Yellow" }
        },
        Recommend = new Ac15RecommendEntry
        {
            RecommendSong = 102,
            RecommendBestSongs = [101, 102]
        },
        ItemShopCatalog = new Ac15ItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = 7,
            Seasons = new Dictionary<uint, Ac15ItemShopSeason>
            {
                [7] = new()
                {
                    SeasonId = 7,
                    VerupNo = 14,
                    Items =
                    [
                        new Ac15ItemShopEntry
                        {
                            ItemNo = 1,
                            ItemType = Ac15ShopItemType.Song,
                            ItemId = 102,
                            Price = 300
                        }
                    ]
                }
            }
        },
        TaikojukuFileOrder =
        [
            new Ac15TaikojukuEntry
            {
                UniqueId = 9001,
                DanLevel = 1,
                ChallengeLevel = 1,
                VerupNo = 8,
                Songs = [new Ac15TaikojukuSong { MusicId = "song101", SongNo = 101, Level = 2 }]
            }
        ]
    };

    private static FakeYellowCatalog YellowCatalogWithoutOptionalRows() => new()
    {
        MusicInfoFileOrder = [new Ac15MusicInfoEntry { MusicId = "song101", SongNo = 101, FileOrder = 0 }],
        SongHashVersion = 123,
        ItemShopCatalog = Ac15ItemShopCatalog.Disabled,
        Recommend = Ac15RecommendEntry.Empty
    };

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
