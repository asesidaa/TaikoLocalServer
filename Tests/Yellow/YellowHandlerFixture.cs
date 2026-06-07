using TaikoLocalServer.Application.Catalog.Yellow;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Yellow;

internal sealed class YellowHandlerFixture : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    private YellowHandlerFixture(SqliteConnection connection, TaikoDbContext context, IGameDataCatalog catalog)
    {
        this.connection = connection;
        Context = context;
        Catalog = catalog;
    }

    public TaikoDbContext Context { get; }

    public IGameDataCatalog Catalog { get; }

    public static async Task<YellowHandlerFixture> CreateAsync(IYellowCatalog? yellowCatalog = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TaikoDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new TaikoDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var catalog = new FileGameDataCatalog([yellowCatalog ?? new TestYellowCatalog()]);
        return new YellowHandlerFixture(connection, context, catalog);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await connection.DisposeAsync();
    }

    internal sealed class TestYellowCatalog : IYellowCatalog
    {
        private readonly IReadOnlyList<YellowMusicInfoEntry> musicInfoFileOrder;
        private readonly IReadOnlyList<YellowTaikojukuEntry> taikojukuFileOrder;

        public TestYellowCatalog(
            IReadOnlyList<YellowMusicInfoEntry>? musicInfoFileOrder = null,
            IReadOnlyList<YellowTaikojukuEntry>? taikojukuFileOrder = null,
            YellowItemShopCatalog? itemShopCatalog = null)
        {
            this.musicInfoFileOrder = musicInfoFileOrder ?? DefaultMusicInfoFileOrder;
            this.taikojukuFileOrder = taikojukuFileOrder ?? [];
            ItemShopCatalog = itemShopCatalog ?? YellowItemShopCatalog.Disabled;
            ItemShop = ItemShopCatalog.ActiveItemsByNo;
        }

        public GameEra Era => GameEra.Yellow;

        public uint SongHashVersion => 789;

        public IReadOnlyList<YellowMusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo, song => (IMusicInfoEntry)song);

        public IReadOnlyDictionary<uint, YellowMusicInfoEntry> YellowMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyList<YellowTaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

        public IReadOnlyDictionary<uint, YellowTaikojukuEntry> Taikojuku
            => TaikojukuFileOrder.ToDictionary(pack => pack.UniqueId);

        public YellowItemShopCatalog ItemShopCatalog { get; }

        public IReadOnlyDictionary<uint, YellowItemShopEntry> ItemShop { get; }

        public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; } = new Dictionary<uint, EventFolderData>();

        public IReadOnlyDictionary<uint, YellowTelopEntry> Telops { get; } = new Dictionary<uint, YellowTelopEntry>();

        public IReadOnlyDictionary<uint, YellowGachaEntry> Gachas { get; } = new Dictionary<uint, YellowGachaEntry>();

        public IReadOnlyDictionary<uint, YellowTournamentEntry> Tournaments { get; } =
            new Dictionary<uint, YellowTournamentEntry>();

        public YellowRecommendEntry Recommend { get; init; } = YellowRecommendEntry.Empty;

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
                [10] = new() { TitleId = 10, TitleName = "Yellow Title", TitleRarity = 0 }
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

        private static IReadOnlyList<YellowMusicInfoEntry> DefaultMusicInfoFileOrder { get; } =
        [
            new() { SongNo = 101, MusicId = "a", FileOrder = 0 },
            new() { SongNo = 102, MusicId = "b", FileOrder = 1 },
            new() { SongNo = 103, MusicId = "c", FileOrder = 2 }
        ];
    }
}
