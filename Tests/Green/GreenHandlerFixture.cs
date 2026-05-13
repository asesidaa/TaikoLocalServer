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
        public TestGreenCatalog(IReadOnlyDictionary<uint, GreenItemShopEntry>? itemShop = null)
        {
            ItemShop = itemShop ?? new Dictionary<uint, GreenItemShopEntry>();
        }

        public GameEra Era => GameEra.Green;

        public uint SongHashVersion => 123;

        public IReadOnlyList<GreenMusicInfoEntry> MusicInfoFileOrder { get; } =
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
            new() { SongNo = 120, MusicId = "t", FileOrder = 19 }
        ];

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo, song => (IMusicInfoEntry)song);

        public IReadOnlyDictionary<uint, GreenMusicInfoEntry> GreenMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyList<GreenTaikojukuEntry> TaikojukuFileOrder { get; } =
        [
            new()
            {
                UniqueId = 20001,
                ChallengeLevel = 1,
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

        public IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku
            => TaikojukuFileOrder.ToDictionary(pack => pack.UniqueId);

        public IReadOnlyDictionary<uint, GreenItemShopEntry> ItemShop { get; }

        public IReadOnlyDictionary<uint, GreenEventFolderEntry> EventFolders { get; } = new Dictionary<uint, GreenEventFolderEntry>();

        public IReadOnlyDictionary<uint, GreenTelopEntry> Telops { get; } = new Dictionary<uint, GreenTelopEntry>();

        public IReadOnlyDictionary<uint, GreenGachaEntry> Gachas { get; } = new Dictionary<uint, GreenGachaEntry>();

        public IReadOnlyDictionary<uint, GreenTournamentEntry> Tournaments { get; } = new Dictionary<uint, GreenTournamentEntry>();

        public Task InitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
