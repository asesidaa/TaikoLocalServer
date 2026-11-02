using TaikoLocalServer.Application.Ac15.DonChallenge;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Red;

internal sealed class RedHandlerFixture : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    private RedHandlerFixture(SqliteConnection connection, TaikoDbContext context, IGameDataCatalog catalog)
    {
        this.connection = connection;
        Context = context;
        Catalog = catalog;
    }

    public TaikoDbContext Context { get; }

    public IGameDataCatalog Catalog { get; }

    public static async Task<RedHandlerFixture> CreateAsync(IRedCatalog? redCatalog = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TaikoDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new TaikoDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var catalog = new FileGameDataCatalog([redCatalog ?? new TestRedCatalog()]);
        return new RedHandlerFixture(connection, context, catalog);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await connection.DisposeAsync();
    }

    internal sealed class TestRedCatalog : IRedCatalog
    {
        private readonly IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder;
        private readonly IReadOnlyList<Ac15TaikojukuEntry> taikojukuFileOrder;

        public TestRedCatalog(
            IReadOnlyList<Ac15MusicInfoEntry>? musicInfoFileOrder = null,
            IReadOnlyList<Ac15TaikojukuEntry>? taikojukuFileOrder = null)
        {
            this.musicInfoFileOrder = musicInfoFileOrder ?? DefaultMusicInfoFileOrder;
            this.taikojukuFileOrder = taikojukuFileOrder ?? [];
        }

        public GameEra Era => GameEra.Red;

        public uint SongHashVersion => 808;

        public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo, song => (IMusicInfoEntry)song);

        public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> RedMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyList<Ac15TaikojukuEntry> TaikojukuFileOrder => taikojukuFileOrder;

        public IReadOnlyDictionary<uint, Ac15TaikojukuEntry> Taikojuku
            => TaikojukuFileOrder.ToDictionary(pack => pack.UniqueId);

        public IReadOnlyDictionary<uint, EventFolderData> EventFolders { get; } = new Dictionary<uint, EventFolderData>();

        public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; } = new Dictionary<uint, Ac15TelopEntry>();

        public IReadOnlyList<MovieData> Movies { get; init; } = [];

        public Ac15DonChallengeCatalog DonChallenge { get; init; } = Ac15DonChallengeCatalog.Disabled;

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
                [10] = new() { TitleId = 10, TitleName = "Red Title", TitleRarity = 0 }
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
    }
}
