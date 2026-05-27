using System.Collections.Immutable;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Green;

public sealed class EventFolderLookupSharedTests
{
    [Fact]
    public async Task GetFolder_NijiiroStillReturnsKnownRequestedFoldersAndOmitsUnknownIds()
    {
        var folders = new Dictionary<uint, EventFolderData>
        {
            [4] = new()
            {
                FolderId = 4,
                VerupNo = 2,
                SongNoes = [101]
            },
            [9] = new()
            {
                FolderId = 9,
                VerupNo = 5,
                SongNoes = [102, 103]
            }
        }.ToImmutableDictionary();
        var catalog = new FileGameDataCatalog([new TestNijiiroCatalog(folders)]);
        var handler = new GetFolderQueryHandler(
            NullLogger<GetFolderQueryHandler>.Instance,
            catalog);

        var response = await handler.Handle(new GetFolderQuery(GameEra.Nijiiro, [9, 99, 4]), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Collection(
            response.AryEventfolderDatas,
            row =>
            {
                Assert.Equal(9u, row.FolderId);
                Assert.Equal(5u, row.VerupNo);
                Assert.Equal(new uint[] { 102, 103 }, row.SongNoes);
            },
            row =>
            {
                Assert.Equal(4u, row.FolderId);
                Assert.Equal(2u, row.VerupNo);
                Assert.Equal(new uint[] { 101 }, row.SongNoes);
            });
    }

    private sealed class TestNijiiroCatalog(ImmutableDictionary<uint, EventFolderData> eventFolders) : INijiiroCatalog
    {
        public GameEra Era => GameEra.Nijiiro;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos { get; } = new Dictionary<uint, IMusicInfoEntry>();

        public List<uint> GetMusicList() => [];

        public List<uint> GetMusicWithUraList() => [];

        public ImmutableDictionary<uint, SongIntroductionData> GetSongIntroductionDictionary()
            => ImmutableDictionary<uint, SongIntroductionData>.Empty;

        public ImmutableDictionary<uint, MovieData> GetMovieDataDictionary()
            => ImmutableDictionary<uint, MovieData>.Empty;

        public ImmutableDictionary<uint, EventFolderData> GetEventFolderDictionary()
            => eventFolders;

        public ImmutableDictionary<uint, DanData> GetCommonDanDataDictionary()
            => ImmutableDictionary<uint, DanData>.Empty;

        public ImmutableDictionary<uint, DanData> GetCommonGaidenDataDictionary()
            => ImmutableDictionary<uint, DanData>.Empty;

        public List<ShopFolderData> GetShopFolderList() => [];

        public uint GetShopFolderVerup() => 1;

        public Dictionary<string, int> GetTokenDataDictionary() => [];

        public List<uint> GetLockedSongsList() => [];

        public List<uint> GetTimeLimitedSongsList() => [];

        public List<uint> GetLockedUraSongsList() => [];

        public Dictionary<uint, MusicDetail> GetMusicDetailDictionary() => [];

        public List<Costume> GetCostumeList() => [];

        public Dictionary<uint, Title> GetTitleDictionary() => [];

        public Dictionary<uint, Neiro> GetNeiroDictionary() => [];

        public Dictionary<string, List<uint>> GetLockedCostumeDataDictionary() => [];

        public Dictionary<string, List<uint>> GetLockedTitleDataDictionary() => [];

        public List<int> GetCostumeFlagArraySizes() => [];

        public int GetTitleFlagArraySize() => 0;

        public int GetToneFlagArraySize() => 0;

        public ImmutableDictionary<string, uint> GetQRCodeDataDictionary()
            => ImmutableDictionary<string, uint>.Empty;

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
