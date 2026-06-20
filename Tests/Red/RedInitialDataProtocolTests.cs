using TaikoLocalServer.Application.Ac15.DonChallenge;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedInitialDataProtocolTests
{
    [Fact]
    public void RedProfile_UsesAc15LimitsAndDisablesItemShop()
    {
        var profile = Ac15EraProfiles.Red;

        Assert.Equal(GameEra.Red, profile.Era);
        Assert.True(profile.Features.Taikojuku);
        Assert.True(profile.Features.Dani);
        Assert.False(profile.Features.ItemShop);
        Assert.Equal(128, profile.Limits.SongFlagBytes);
        Assert.Equal(16, profile.Limits.ToneFlagBytes);
        Assert.Equal(128, profile.Limits.TitleFlagBytes);
        Assert.Equal(1024, profile.Limits.CrownSongCount);
        Assert.Equal(10, profile.Limits.MaxSongsPerTaikojukuPack);
        Assert.False(profile.WirePlacement.HasInitialDataItemShopRows);
        Assert.True(profile.WirePlacement.HasInitialDataLegalTermsRows);
        Assert.True(profile.WirePlacement.HasTokkunTutorialFlagInUserData);
    }

    [Fact]
    public void CatalogSnapshotFactory_FromRedMapsCatalogRowsWithoutItemShop()
    {
        var red = RedCatalogWithOptionalRows();

        var snapshot = Ac15CatalogSnapshotFactory.FromRed(red);

        Assert.Equal(810u, snapshot.SongHashVersion);
        Assert.Equal([101u, 102u], snapshot.SongNoesInFileOrder);
        Assert.Equal(44u, snapshot.EventFolders[44].FolderId);
        Assert.Equal(31u, snapshot.Telops[31].TelopId);
        Assert.False(snapshot.ItemShopCatalog.IsEnabled);
        Assert.Equal(1u, snapshot.TaikojukuPacks[0].ChallengeLevel);
        Assert.Equal(101u, snapshot.TaikojukuPacks[0].Songs[0].SongNo);
    }

    [Fact]
    public async Task RedInitialDataHandler_ReturnsCatalogBackedRowsWithoutShop()
    {
        var handler = new GetInitialDataQueryHandler(
            new FakeGameDataCatalog(RedCatalogWithOptionalRows()),
            NullLogger<GetInitialDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

        var response = await handler.Handle(new GetInitialDataQuery(GameEra.Red), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(810u, response.SongHashVer);
        Assert.True(response.IsDanplay);
        Assert.False(response.IsItemshop);
        Assert.True((response.DefaultSongFlg[101 >> 3] & (1 << (101 & 7))) != 0);
        Assert.True((response.DefaultSongFlg[102 >> 3] & (1 << (102 & 7))) != 0);
        Assert.Equal(31u, Assert.Single(response.AryTelopDatas).InfoId);
        Assert.Equal(44u, Assert.Single(response.AryEventFolderDatas).InfoId);
        Assert.Equal(1u, Assert.Single(response.AryTaikojukuDatas).InfoId);
        Assert.Empty(response.AryItemShopDatas);
        Assert.Empty(response.AryLegaltermsDatas);
    }

    [Fact]
    public async Task RedTaikojukuHandler_ReturnsRequestedPacksFromRedCatalog()
    {
        var handler = new GetTaikojukuQueryHandler(
            new FakeGameDataCatalog(RedCatalogWithOptionalRows()),
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(
            new GetTaikojukuQuery(GameEra.Red, [1]),
            CancellationToken.None);

        Assert.Equal(1u, response.Result);
        var pack = Assert.Single(response.Packs);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal(8u, pack.VerupNo);
        Assert.Equal([101u], pack.Songs.Select(song => song.SongNo).ToArray());
        Assert.Equal([2u], pack.Songs.Select(song => song.Level).ToArray());
    }

    private static FakeRedCatalog RedCatalogWithOptionalRows() => new()
    {
        MusicInfoFileOrder =
        [
            new Ac15MusicInfoEntry { MusicId = "song101", SongNo = 101, FileOrder = 0 },
            new Ac15MusicInfoEntry { MusicId = "song102", SongNo = 102, FileOrder = 1 }
        ],
        SongHashVersion = 810,
        EventFolders = new Dictionary<uint, EventFolderData>
        {
            [44] = new() { FolderId = 44, VerupNo = 5 }
        },
        Telops = new Dictionary<uint, Ac15TelopEntry>
        {
            [31] = new() { TelopId = 31, VerupNo = 6, Message = "Red" }
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

    private sealed class FakeGameDataCatalog(IRedCatalog red) : IGameDataCatalog
    {
        public IEraGameDataCatalog For(GameEra era)
            => era == GameEra.Red ? red : throw new InvalidOperationException($"Unexpected era {era}");

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeRedCatalog : IRedCatalog
    {
        public GameEra Era => GameEra.Red;

        public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder { get; init; } = [];

        public uint SongHashVersion { get; init; }

        public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> RedMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => RedMusicInfos.ToDictionary(pair => pair.Key, pair => (IMusicInfoEntry)pair.Value);

        public IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder { get; init; } = [];

        public IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku
            => TaikojukuFileOrder.ToDictionary(entry => entry.ChallengeLevel);

        public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; init; }
            = new Dictionary<uint, EventFolderData>();

        public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; init; }
            = new Dictionary<uint, Ac15TelopEntry>();

        public IReadOnlyList<MovieData> Movies { get; init; } = [];

        public Ac15DonChallengeCatalog DonChallenge { get; init; } = Ac15DonChallengeCatalog.Disabled;

        public IReadOnlyList<Costume> GetCostumeList() => [];

        public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => new Dictionary<uint, Title>();

        public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => new Dictionary<uint, Neiro>();

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
