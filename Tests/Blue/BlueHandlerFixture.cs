using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Blue;

internal sealed class BlueHandlerFixture : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    private BlueHandlerFixture(SqliteConnection connection, TaikoDbContext context, IGameDataCatalog catalog)
    {
        this.connection = connection;
        Context = context;
        Catalog = catalog;
    }

    public TaikoDbContext Context { get; }

    public IGameDataCatalog Catalog { get; }

    public static async Task<BlueHandlerFixture> CreateAsync(IBlueCatalog? blueCatalog = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TaikoDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new TaikoDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var catalog = new FileGameDataCatalog([blueCatalog ?? new TestBlueCatalog()]);
        return new BlueHandlerFixture(connection, context, catalog);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await connection.DisposeAsync();
    }

    internal sealed class TestBlueCatalog : IBlueCatalog
    {
        private readonly IReadOnlyList<BlueMusicInfoEntry> musicInfoFileOrder;
        private readonly IReadOnlyList<BlueTaikojukuEntry> taikojukuFileOrder;

        public TestBlueCatalog(
            IReadOnlyDictionary<uint, EventFolderData>? eventFolders = null,
            IReadOnlyDictionary<uint, BlueTelopEntry>? telops = null,
            IReadOnlyList<BlueMusicInfoEntry>? musicInfoFileOrder = null,
            IReadOnlyList<BlueTaikojukuEntry>? taikojukuFileOrder = null,
            BlueItemShopCatalog? itemShopCatalog = null,
            BlueRecommendEntry? recommend = null)
        {
            EventFolders = eventFolders ?? new Dictionary<uint, EventFolderData>();
            Telops = telops ?? new Dictionary<uint, BlueTelopEntry>();
            this.musicInfoFileOrder = musicInfoFileOrder ?? DefaultMusicInfoFileOrder;
            this.taikojukuFileOrder = taikojukuFileOrder ?? DefaultTaikojukuFileOrder;
            ItemShopCatalog = itemShopCatalog ?? BlueItemShopCatalog.Disabled;
            ItemShop = ItemShopCatalog.ActiveItemsByNo;
            Recommend = recommend ?? BlueRecommendEntry.Empty;
        }

        public GameEra Era => GameEra.Blue;

        public uint SongHashVersion => 456;

        public IReadOnlyList<BlueMusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo, song => (IMusicInfoEntry)song);

        public IReadOnlyDictionary<uint, BlueMusicInfoEntry> BlueMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyList<BlueTaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

        public IReadOnlyDictionary<uint, BlueTaikojukuEntry> Taikojuku
            => TaikojukuFileOrder.ToDictionary(pack => pack.UniqueId);

        public BlueItemShopCatalog ItemShopCatalog { get; }

        public IReadOnlyDictionary<uint, BlueItemShopEntry> ItemShop { get; }

        public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

        public IReadOnlyDictionary<uint, BlueTelopEntry> Telops { get; }

        public IReadOnlyDictionary<uint, BlueGachaEntry> Gachas { get; } = new Dictionary<uint, BlueGachaEntry>();

        public IReadOnlyDictionary<uint, BlueTournamentEntry> Tournaments { get; } =
            new Dictionary<uint, BlueTournamentEntry>();

        public BlueRecommendEntry Recommend { get; }

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
                [10] = new() { TitleId = 10, TitleName = "Blue Title", TitleRarity = 0 }
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

        private static IReadOnlyList<BlueMusicInfoEntry> DefaultMusicInfoFileOrder { get; } =
        [
            new() { SongNo = 101, MusicId = "a", FileOrder = 0 },
            new() { SongNo = 102, MusicId = "b", FileOrder = 1 },
            new() { SongNo = 103, MusicId = "c", FileOrder = 2 }
        ];

        private static IReadOnlyList<BlueTaikojukuEntry> DefaultTaikojukuFileOrder { get; } =
        [
            new()
            {
                UniqueId = 20001,
                ChallengeLevel = 1,
                VerupNo = 0,
                Songs =
                [
                    new() { SongNo = 101, Level = 0 },
                    new() { SongNo = 102, Level = 0 },
                    new() { SongNo = 103, Level = 0 }
                ]
            }
        ];
    }
}
