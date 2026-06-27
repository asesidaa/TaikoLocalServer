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
        private readonly IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder;
        private readonly IReadOnlyList<Ac15TaikojukuEntry> taikojukuFileOrder;

        public TestBlueCatalog(
            IReadOnlyDictionary<uint, EventFolderData>? eventFolders = null,
            IReadOnlyDictionary<uint, Ac15TelopEntry>? telops = null,
            IReadOnlyList<Ac15MusicInfoEntry>? musicInfoFileOrder = null,
            IReadOnlyList<Ac15TaikojukuEntry>? taikojukuFileOrder = null,
            Ac15ItemShopCatalog? itemShopCatalog = null,
            BlueBattleCatalog? battleCatalog = null)
        {
            EventFolders = eventFolders ?? new Dictionary<uint, EventFolderData>();
            Telops = telops ?? new Dictionary<uint, Ac15TelopEntry>();
            this.musicInfoFileOrder = musicInfoFileOrder ?? DefaultMusicInfoFileOrder;
            this.taikojukuFileOrder = taikojukuFileOrder ?? DefaultTaikojukuFileOrder;
            ItemShopCatalog = itemShopCatalog ?? Ac15ItemShopCatalog.Disabled;
            ItemShop = ItemShopCatalog.ActiveItemsByNo;
            BattleCatalog = battleCatalog ?? BlueBattleCatalog.Unavailable;
        }

        public GameEra Era => GameEra.Blue;

        public uint SongHashVersion => 456;

        public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo, song => (IMusicInfoEntry)song);

        public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> BlueMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

        public IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku
            => TaikojukuFileOrder.ToDictionary(pack => pack.UniqueId);

        public Ac15ItemShopCatalog ItemShopCatalog { get; }

        public IReadOnlyDictionary<uint, Ac15ItemShopEntry> ItemShop { get; }

        public BlueBattleCatalog BattleCatalog { get; }

        public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; }

        public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; }

        public IReadOnlyDictionary<uint, Ac15GachaEntry> Gachas { get; } = new Dictionary<uint, Ac15GachaEntry>();

        public IReadOnlyDictionary<uint, Ac15TournamentEntry> Tournaments { get; } =
            new Dictionary<uint, Ac15TournamentEntry>();

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

        private static IReadOnlyList<Ac15MusicInfoEntry> DefaultMusicInfoFileOrder { get; } =
        [
            new() { SongNo = 101, MusicId = "a", FileOrder = 0 },
            new() { SongNo = 102, MusicId = "b", FileOrder = 1 },
            new() { SongNo = 103, MusicId = "c", FileOrder = 2 }
        ];

        private static IReadOnlyList<Ac15TaikojukuEntry> DefaultTaikojukuFileOrder { get; } =
        [
            new()
            {
                UniqueId = 20001,
                ChallengeLevel = 1,
                VerupNo = 0,
                Songs =
                [
                    new() { SongNo = 101, Level = Difficulty.Easy },
                    new() { SongNo = 102, Level = Difficulty.Easy },
                    new() { SongNo = 103, Level = Difficulty.Easy }
                ]
            }
        ];
    }
}
