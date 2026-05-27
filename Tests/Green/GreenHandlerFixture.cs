using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Green;

internal sealed class GreenHandlerFixture : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    private GreenHandlerFixture(SqliteConnection connection, TaikoDbContext context, IGameDataCatalog catalog)
    {
        this.connection = connection;
        Context = context;
        Catalog = catalog;
    }

    public TaikoDbContext Context { get; }

    public IGameDataCatalog Catalog { get; }

    public static async Task<GreenHandlerFixture> CreateAsync(IGreenCatalog? greenCatalog = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TaikoDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new TaikoDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var catalog = new FileGameDataCatalog([greenCatalog ?? new TestGreenCatalog()]);
        return new GreenHandlerFixture(connection, context, catalog);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await connection.DisposeAsync();
    }

    internal sealed class TestGreenCatalog : IGreenCatalog
    {
        private readonly IReadOnlyList<GreenMusicInfoEntry> musicInfoFileOrder;
        private readonly IReadOnlyList<GreenTaikojukuEntry> taikojukuFileOrder;

        public TestGreenCatalog(
            IReadOnlyDictionary<uint, GreenItemShopEntry>? itemShop = null,
            GreenItemShopCatalog? itemShopCatalog = null,
            IReadOnlyDictionary<uint, EventFolderData>? eventFolders = null,
            IReadOnlyList<GreenMusicInfoEntry>? musicInfoFileOrder = null,
            IReadOnlyList<GreenTaikojukuEntry>? taikojukuFileOrder = null)
        {
            ItemShopCatalog = itemShopCatalog ?? GreenItemShopCatalog.Disabled;
            ItemShop = itemShop ?? ItemShopCatalog.ActiveItemsByNo;
            EventFolders = eventFolders ?? new Dictionary<uint, EventFolderData>();
            this.musicInfoFileOrder = musicInfoFileOrder ?? DefaultMusicInfoFileOrder;
            this.taikojukuFileOrder = taikojukuFileOrder ?? DefaultTaikojukuFileOrder;
        }

        public GameEra Era => GameEra.Green;

        public uint SongHashVersion => 123;

        public IReadOnlyList<GreenMusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

        private static IReadOnlyList<GreenMusicInfoEntry> DefaultMusicInfoFileOrder { get; } =
        [
            new() { SongNo = 101, MusicId = "a", FileOrder = 0 },
            new() { SongNo = 102, MusicId = "b", FileOrder = 1 },
            new() { SongNo = 103, MusicId = "c", FileOrder = 2 },
            new() { SongNo = 104, MusicId = "d", FileOrder = 3 },
            new() { SongNo = 105, MusicId = "e", FileOrder = 4, HasExtreme = true },
            new() { SongNo = 106, MusicId = "f", FileOrder = 5 },
            new() { SongNo = 107, MusicId = "g", FileOrder = 6 },
            new() { SongNo = 108, MusicId = "h", FileOrder = 7 },
            new() { SongNo = 109, MusicId = "i", FileOrder = 8 },
            new() { SongNo = 110, MusicId = "j", FileOrder = 9 },
            new() { SongNo = 111, MusicId = "k", FileOrder = 10 },
            new() { SongNo = 112, MusicId = "l", FileOrder = 11 },
            new() { SongNo = 113, MusicId = "m", FileOrder = 12 },
            new() { SongNo = 114, MusicId = "n", FileOrder = 13 },
            new() { SongNo = 115, MusicId = "o", FileOrder = 14 },
            new() { SongNo = 116, MusicId = "p", FileOrder = 15 },
            new() { SongNo = 117, MusicId = "q", FileOrder = 16 },
            new() { SongNo = 118, MusicId = "r", FileOrder = 17 },
            new() { SongNo = 119, MusicId = "s", FileOrder = 18 },
            new() { SongNo = 120, MusicId = "t", FileOrder = 19 },
            new() { SongNo = 121, MusicId = "u", FileOrder = 20 }
        ];

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo, song => (IMusicInfoEntry)song);

        public IReadOnlyDictionary<uint, GreenMusicInfoEntry> GreenMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        private static IReadOnlyList<GreenTaikojukuEntry> DefaultTaikojukuFileOrder { get; } =
        [
            new()
            {
                UniqueId = 20001,
                ChallengeLevel = 1,
                Conditions = new GreenTaikojukuConditions
                {
                    SoulGauge = 90,
                    TotalHitCount = 420
                },
                ExcellentConditions = new GreenTaikojukuConditions
                {
                    SoulGauge = 95,
                    TotalHitCount = 460
                },
                Songs =
                [
                    new() { SongNo = 101, Level = 0 },
                    new() { SongNo = 102, Level = 0 },
                    new() { SongNo = 103, Level = 0 }
                ]
            },
            new()
            {
                UniqueId = 20026,
                ChallengeLevel = 101,
                Songs =
                [
                    new() { SongNo = 104, Level = 1 },
                    new() { SongNo = 105, Level = 1 },
                    new() { SongNo = 106, Level = 1 }
                ]
            }
        ];

        public IReadOnlyList<GreenTaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

        public IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku
            => TaikojukuFileOrder.ToDictionary(pack => pack.UniqueId);

        public GreenItemShopCatalog ItemShopCatalog { get; }

        public IReadOnlyDictionary<uint, GreenItemShopEntry> ItemShop { get; }

        public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

        public IReadOnlyDictionary<uint, GreenTelopEntry> Telops { get; init; } = new Dictionary<uint, GreenTelopEntry>();

        public IReadOnlyDictionary<uint, GreenGachaEntry> Gachas { get; } = new Dictionary<uint, GreenGachaEntry>();

        public IReadOnlyDictionary<uint, GreenTournamentEntry> Tournaments { get; } = new Dictionary<uint, GreenTournamentEntry>();

        public GreenRecommendEntry Recommend { get; init; } = GreenRecommendEntry.Empty;

        public IReadOnlyList<MovieData> Movies { get; init; } = [];

        public IReadOnlyList<Costume> CostumeList { get; init; } =
        [
            new() { CostumeId = 0, CostumeType = "kigurumi" },
            new() { CostumeId = 1, CostumeType = "head" },
            new() { CostumeId = 2, CostumeType = "body" },
            new() { CostumeId = 3, CostumeType = "face" },
            new() { CostumeId = 4, CostumeType = "puchi" }
        ];

        public IReadOnlyDictionary<uint, Title> TitleDictionary { get; init; } =
            new Dictionary<uint, Title>
            {
                [10] = new() { TitleId = 10, TitleName = "Green Title", TitleRarity = 0 }
            };

        public IReadOnlyDictionary<uint, Neiro> NeiroDictionary { get; init; } =
            new Dictionary<uint, Neiro>
            {
                [0] = new() { NeiroId = 0, NeiroName = "Taiko" },
                [4] = new() { NeiroId = 4, NeiroName = "Tone 4" }
            };

        public IReadOnlyList<Costume> GetCostumeList() => CostumeList;

        public IReadOnlyDictionary<uint, Title> GetTitleDictionary() => TitleDictionary;

        public IReadOnlyDictionary<uint, Neiro> GetNeiroDictionary() => NeiroDictionary;

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
