using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Application;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Momoiro;

internal sealed class MomoiroHandlerFixture : IAsyncDisposable
{
    public const uint HighSongNo = 777;
    public const int HighSongOrdinal = 379;

    private readonly SqliteConnection connection;

    private MomoiroHandlerFixture(SqliteConnection connection, TaikoDbContext context, IGameDataCatalog catalog)
    {
        this.connection = connection;
        Context = context;
        Catalog = catalog;
    }

    public TaikoDbContext Context { get; }

    public IGameDataCatalog Catalog { get; }

    public static async Task<MomoiroHandlerFixture> CreateAsync(IMomoiroCatalog? momoiroCatalog = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TaikoDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new TaikoDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var catalog = new FileGameDataCatalog([momoiroCatalog ?? new TestMomoiroCatalog()]);
        var fixture = new MomoiroHandlerFixture(connection, context, catalog);
        await fixture.EnsureMomoiroReadbackTablesAsync();
        return fixture;
    }

    public ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddSingleton(Context);
        services.AddSingleton<ITaikoDbContext>(Context);
        services.AddSingleton(Catalog);
        services.AddSingleton(Options.Create(new ServerSettings()));
        return services.BuildServiceProvider();
    }

    public async Task EnsureMomoiroReadbackTablesAsync()
    {
        await Context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS UserSaveData_Momoiro (
                Baid INTEGER NOT NULL PRIMARY KEY,
                Title TEXT NOT NULL,
                TitleplateId INTEGER NOT NULL,
                ColorBody INTEGER NOT NULL,
                ColorFace INTEGER NOT NULL,
                ColorLimb INTEGER NOT NULL,
                Costume1 INTEGER NOT NULL,
                Costume2 INTEGER NOT NULL,
                Costume3 INTEGER NOT NULL,
                Costume4 INTEGER NOT NULL,
                Costume5 INTEGER NOT NULL,
                CostumeFlg1 BLOB NOT NULL,
                CostumeFlg2 BLOB NOT NULL,
                CostumeFlg3 BLOB NOT NULL,
                CostumeFlg4 BLOB NOT NULL,
                CostumeFlg5 BLOB NOT NULL,
                ToneFlg BLOB NOT NULL,
                TitleFlg BLOB NOT NULL,
                ReleaseSongFlg BLOB NOT NULL,
                OptionFlg BLOB NOT NULL,
                DefaultOptionSetting BLOB NOT NULL,
                DefaultShinSetting INTEGER NOT NULL,
                DefaultToneSetting INTEGER NOT NULL,
                DispDanType INTEGER NOT NULL,
                GotDanMax INTEGER NOT NULL,
                GotDanFlg BLOB NOT NULL,
                GotDanExtraFlg BLOB NOT NULL,
                DispTaikojukuDan INTEGER NOT NULL,
                TotalGetDonpoint INTEGER NOT NULL,
                TotalUseDonpoint INTEGER NOT NULL,
                RewardPtn INTEGER NOT NULL,
                RewardProgress INTEGER NOT NULL,
                DifficultyTutorialFlg INTEGER NOT NULL,
                IsAutoCostumeOn INTEGER NOT NULL,
                CategJpopCnt INTEGER NOT NULL,
                CategAnimeCnt INTEGER NOT NULL,
                CategDoyoCnt INTEGER NOT NULL,
                CategVarietyCnt INTEGER NOT NULL,
                CategClassicCnt INTEGER NOT NULL,
                CategGameCnt INTEGER NOT NULL,
                CategNamcoCnt INTEGER NOT NULL,
                CategVocaloidCnt INTEGER NOT NULL,
                SongPushedCnt INTEGER NOT NULL,
                SongFavoriteCnt INTEGER NOT NULL,
                SongRecentCnt INTEGER NOT NULL,
                TotalCreditCnt INTEGER NOT NULL,
                PrevAreaCode INTEGER NOT NULL,
                ConsecAreaCnt INTEGER NOT NULL,
                DispLevelTotal INTEGER NOT NULL,
                DispLevelChassis INTEGER NOT NULL,
                DispLevelSelf INTEGER NOT NULL,
                IsDevil INTEGER NOT NULL,
                DispScoreType INTEGER NOT NULL,
                DifficultyPlayedCourse INTEGER NOT NULL,
                DifficultyPlayedStar INTEGER NOT NULL,
                IsTojiru INTEGER NOT NULL,
                IsExplain INTEGER NOT NULL,
                LastPlayDatetime datetime NOT NULL
            );
            """);

        await Context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS SongBestDatum_Momoiro (
                Baid INTEGER NOT NULL,
                SongId INTEGER NOT NULL,
                Difficulty INTEGER NOT NULL,
                IsShin INTEGER NOT NULL,
                BestScore INTEGER NOT NULL,
                BestRate INTEGER NOT NULL,
                BestCrown INTEGER NOT NULL,
                PRIMARY KEY (Baid, SongId, Difficulty, IsShin)
            );
            """);

        await Context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS MomoiroFavoriteSongs (
                Baid INTEGER NOT NULL,
                SongNo INTEGER NOT NULL,
                DisplayOrder INTEGER NOT NULL,
                PRIMARY KEY (Baid, SongNo)
            );
            """);

        await Context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS MomoiroRecentSongs (
                Baid INTEGER NOT NULL,
                SongNo INTEGER NOT NULL,
                LastPlayed datetime NOT NULL,
                PRIMARY KEY (Baid, SongNo)
            );
            """);

        await Context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS SongPlayDatum_Momoiro (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                Baid INTEGER NOT NULL,
                SongId INTEGER NOT NULL,
                Difficulty INTEGER NOT NULL,
                Crown INTEGER NOT NULL,
                Score INTEGER NOT NULL,
                ScoreRate INTEGER NOT NULL,
                GoodCount INTEGER NOT NULL,
                OkCount INTEGER NOT NULL,
                MissCount INTEGER NOT NULL,
                ComboCount INTEGER NOT NULL,
                HitCount INTEGER NOT NULL,
                PoundCount INTEGER NOT NULL,
                StarLevel INTEGER NOT NULL,
                OptionFlg BLOB NOT NULL,
                ToneFlg BLOB NOT NULL,
                PlayMode INTEGER NOT NULL,
                StageMode INTEGER NOT NULL,
                IsShin INTEGER NOT NULL,
                MusicCategory INTEGER NOT NULL,
                SelectedFolderId INTEGER NOT NULL,
                IsFavorite INTEGER NOT NULL,
                IsRecent INTEGER NOT NULL,
                IsPapamama INTEGER NOT NULL,
                IsPushed INTEGER NOT NULL,
                SoulGauge INTEGER NOT NULL,
                PlayDan INTEGER NOT NULL,
                WaiwaiResult INTEGER NOT NULL,
                WaiwaiGauge INTEGER NOT NULL,
                PlayTime datetime NOT NULL
            );
            """);

        await Context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS DanScoreDatum_Momoiro (
                Baid INTEGER NOT NULL,
                DanId INTEGER NOT NULL,
                IsExtra INTEGER NOT NULL,
                MedleyUniqueId INTEGER NOT NULL,
                ArrivalSongCount INTEGER NOT NULL,
                SoulGaugeTotal INTEGER NOT NULL,
                ComboCountTotal INTEGER NOT NULL,
                ClearGrade INTEGER NOT NULL DEFAULT 0,
                PRIMARY KEY (Baid, DanId, IsExtra)
            );
            """);

        await Context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS DanStageScoreDatum_Momoiro (
                Baid INTEGER NOT NULL,
                DanId INTEGER NOT NULL,
                IsExtra INTEGER NOT NULL,
                StageIndex INTEGER NOT NULL,
                SongNumber INTEGER NOT NULL,
                PlayScore INTEGER NOT NULL,
                GoodCount INTEGER NOT NULL,
                OkCount INTEGER NOT NULL,
                BadCount INTEGER NOT NULL,
                DrumrollCount INTEGER NOT NULL,
                TotalHitCount INTEGER NOT NULL,
                ComboCount INTEGER NOT NULL,
                HighScore INTEGER NOT NULL,
                PRIMARY KEY (Baid, DanId, IsExtra, StageIndex),
                FOREIGN KEY (Baid, DanId, IsExtra)
                    REFERENCES DanScoreDatum_Momoiro (Baid, DanId, IsExtra)
                    ON DELETE CASCADE
            );
            """);
    }

    public async Task SeedSharedIdentityAsync(uint baid, string accessCode, string myDonName = "MOMO")
    {
        if (await Context.UserData.FindAsync(baid) is null)
        {
            Context.UserData.Add(new UserDatum
            {
                Baid = baid,
                MyDonName = myDonName,
                MyDonNameLanguage = 0
            });
        }

        if (await Context.Cards.FindAsync(accessCode) is null)
        {
            Context.Cards.Add(new Card { AccessCode = accessCode, Baid = baid });
        }

        if (await Context.Credentials.FindAsync(baid) is null)
        {
            Context.Credentials.Add(new Credential { Baid = baid, Password = string.Empty, Salt = string.Empty });
        }

        await Context.SaveChangesAsync();
    }

    public async Task SeedMomoiroSaveAsync(
        uint baid,
        IEnumerable<uint>? releaseSongNoes = null,
        bool isDevil = false,
        bool isExplain = false,
        uint rewardPtn = 0,
        uint rewardProgress = 0,
        uint totalGetDonpoint = 0,
        uint totalUseDonpoint = 0,
        uint dispLevelSelf = 0)
    {
        await EnsureMomoiroReadbackTablesAsync();
        var limits = Ac15EraProfiles.Momoiro.Limits;
        var releaseFlags = Ac15ProtocolBytes.CreateFixedBitset(releaseSongNoes ?? [], limits.SongFlagBytes);
        await InsertOrReplaceAsync(
            "UserSaveData_Momoiro",
            new Dictionary<string, object?>
            {
                ["Baid"] = baid,
                ["Title"] = "Momoiro Title",
                ["TitleplateId"] = 0u,
                ["ColorBody"] = 1u,
                ["ColorFace"] = 0u,
                ["ColorLimb"] = 3u,
                ["Costume1"] = 0u,
                ["Costume2"] = 0u,
                ["Costume3"] = 0u,
                ["Costume4"] = 0u,
                ["Costume5"] = 0u,
                ["CostumeFlg1"] = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
                ["CostumeFlg2"] = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
                ["CostumeFlg3"] = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
                ["CostumeFlg4"] = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
                ["CostumeFlg5"] = Ac15ProtocolBytes.CreateFixedBitset([0], limits.CostumeFlagBytes),
                ["ToneFlg"] = Ac15ProtocolBytes.CreateFixedBitset([0], limits.ToneFlagBytes),
                ["TitleFlg"] = new byte[limits.TitleFlagBytes],
                ["ReleaseSongFlg"] = releaseFlags,
                ["OptionFlg"] = new byte[limits.SongFlagBytes],
                ["DefaultOptionSetting"] = new byte[2],
                ["DefaultShinSetting"] = false,
                ["DefaultToneSetting"] = 0u,
                ["DispDanType"] = 0u,
                ["GotDanMax"] = 0u,
                ["GotDanFlg"] = new byte[limits.DanFlagBytes],
                ["GotDanExtraFlg"] = new byte[limits.DanExtraFlagBytes],
                ["DispTaikojukuDan"] = 0u,
                ["TotalGetDonpoint"] = totalGetDonpoint,
                ["TotalUseDonpoint"] = totalUseDonpoint,
                ["RewardPtn"] = rewardPtn,
                ["RewardProgress"] = rewardProgress,
                ["DifficultyTutorialFlg"] = 0u,
                ["IsAutoCostumeOn"] = true,
                ["CategJpopCnt"] = 0u,
                ["CategAnimeCnt"] = 0u,
                ["CategDoyoCnt"] = 0u,
                ["CategVarietyCnt"] = 0u,
                ["CategClassicCnt"] = 0u,
                ["CategGameCnt"] = 0u,
                ["CategNamcoCnt"] = 0u,
                ["CategVocaloidCnt"] = 0u,
                ["SongPushedCnt"] = 0u,
                ["SongFavoriteCnt"] = 0u,
                ["SongRecentCnt"] = 0u,
                ["TotalCreditCnt"] = 0u,
                ["PrevAreaCode"] = 0u,
                ["ConsecAreaCnt"] = 0u,
                ["DispLevelTotal"] = 0u,
                ["DispLevelChassis"] = 0u,
                ["DispLevelSelf"] = dispLevelSelf,
                ["IsDevil"] = isDevil,
                ["DispScoreType"] = 0u,
                ["DifficultyPlayedCourse"] = 0u,
                ["DifficultyPlayedStar"] = 0u,
                ["IsTojiru"] = true,
                ["IsExplain"] = isExplain,
                ["LastPlayDatetime"] = DateTime.UnixEpoch
            });
    }

    public Task SeedMomoiroBestAsync(
        uint baid,
        uint songId,
        Difficulty difficulty,
        bool isShin,
        uint bestScore,
        uint bestRate,
        CrownType bestCrown)
        => InsertOrReplaceAsync(
            "SongBestDatum_Momoiro",
            new Dictionary<string, object?>
            {
                ["Baid"] = baid,
                ["SongId"] = songId,
                ["Difficulty"] = (uint)difficulty,
                ["IsShin"] = isShin,
                ["BestScore"] = bestScore,
                ["BestRate"] = bestRate,
                ["BestCrown"] = (uint)bestCrown
            });

    public Task SeedMomoiroFavoriteAsync(uint baid, uint songNo, int displayOrder)
        => InsertOrReplaceAsync(
            "MomoiroFavoriteSongs",
            new Dictionary<string, object?>
            {
                ["Baid"] = baid,
                ["SongNo"] = songNo,
                ["DisplayOrder"] = displayOrder
            });

    public Task SeedMomoiroRecentAsync(uint baid, uint songNo, DateTime lastPlayed)
        => InsertOrReplaceAsync(
            "MomoiroRecentSongs",
            new Dictionary<string, object?>
            {
                ["Baid"] = baid,
                ["SongNo"] = songNo,
                ["LastPlayed"] = lastPlayed
            });

    public Task SeedMomoiroPlayAsync(
        uint baid,
        uint songId,
        Difficulty difficulty = Difficulty.Easy,
        CrownType crown = CrownType.Gold,
        uint score = 765_432,
        DateTime? playTime = null)
        => InsertAsync(
            "SongPlayDatum_Momoiro",
            new Dictionary<string, object?>
            {
                ["Baid"] = baid,
                ["SongId"] = songId,
                ["Difficulty"] = (uint)difficulty,
                ["Crown"] = (uint)crown,
                ["Score"] = score,
                ["ScoreRate"] = 95u,
                ["GoodCount"] = 100u,
                ["OkCount"] = 20u,
                ["MissCount"] = 3u,
                ["ComboCount"] = 120u,
                ["HitCount"] = 123u,
                ["PoundCount"] = 4u,
                ["StarLevel"] = 0u,
                ["OptionFlg"] = new byte[] { 1, 2, 3 },
                ["ToneFlg"] = new byte[] { 4 },
                ["PlayMode"] = 0u,
                ["StageMode"] = 0u,
                ["IsShin"] = false,
                ["MusicCategory"] = 1u,
                ["SelectedFolderId"] = 9u,
                ["IsFavorite"] = true,
                ["IsRecent"] = true,
                ["IsPapamama"] = false,
                ["IsPushed"] = true,
                ["SoulGauge"] = 100u,
                ["PlayDan"] = 0u,
                ["WaiwaiResult"] = 0u,
                ["WaiwaiGauge"] = 0u,
                ["PlayTime"] = playTime ?? new DateTime(2026, 6, 8, 12, 0, 0)
            });

    public Task SeedMomoiroDanScoreAsync(
        uint baid,
        uint danId,
        uint medleyUniqueId = 20_001,
        Ac15DanClearGrade clearGrade = Ac15DanClearGrade.GoldClear,
        uint arrivalSongCount = 2,
        uint soulGaugeTotal = 88,
        uint comboCountTotal = 320,
        bool isExtra = false)
        => InsertOrReplaceAsync(
            "DanScoreDatum_Momoiro",
            new Dictionary<string, object?>
            {
                ["Baid"] = baid,
                ["DanId"] = danId,
                ["IsExtra"] = isExtra,
                ["MedleyUniqueId"] = medleyUniqueId,
                ["ArrivalSongCount"] = arrivalSongCount,
                ["SoulGaugeTotal"] = soulGaugeTotal,
                ["ComboCountTotal"] = comboCountTotal,
                ["ClearGrade"] = (uint)clearGrade
            });

    public Task SeedMomoiroDanStageScoreAsync(
        uint baid,
        uint danId,
        uint stageIndex,
        uint songNumber,
        uint playScore,
        bool isExtra = false)
        => InsertOrReplaceAsync(
            "DanStageScoreDatum_Momoiro",
            new Dictionary<string, object?>
            {
                ["Baid"] = baid,
                ["DanId"] = danId,
                ["IsExtra"] = isExtra,
                ["StageIndex"] = stageIndex,
                ["SongNumber"] = songNumber,
                ["PlayScore"] = playScore,
                ["GoodCount"] = 100u,
                ["OkCount"] = 20u,
                ["BadCount"] = 3u,
                ["DrumrollCount"] = 4u,
                ["TotalHitCount"] = 123u,
                ["ComboCount"] = 120u,
                ["HighScore"] = playScore
            });

    public Task<bool> MomoiroSaveExistsAsync(uint baid)
        => RowExistsAsync("UserSaveData_Momoiro", baid);

    public Task<int> CountMomoiroPlayRowsAsync(uint baid)
        => CountRowsAsync("SongPlayDatum_Momoiro", baid);

    public Task<int> CountMomoiroDanRowsAsync(uint baid)
        => CountRowsAsync("DanScoreDatum_Momoiro", baid);

    public Task<int> CountMomoiroDanStageRowsAsync(uint baid)
        => CountRowsAsync("DanStageScoreDatum_Momoiro", baid);

    public Task<int> CountMomoiroFavoriteRowsAsync(uint baid)
        => CountRowsAsync("MomoiroFavoriteSongs", baid);

    public Task<int> CountMomoiroRecentRowsAsync(uint baid)
        => CountRowsAsync("MomoiroRecentSongs", baid);

    public async Task<IReadOnlyDictionary<string, int>> CountSelectedAdjacentUnsupportedRowsAsync(uint baid)
    {
        var tableNames = new[]
        {
            "SongPlayDatum_Green",
            "SongPlayDatum_Blue",
            "SongPlayDatum_Yellow",
            "SongPlayDatum_Red",
            "SongPlayDatum_White",
            "SongPlayDatum_Murasaki",
            "SongPlayDatum_Kimidori",
            "DanScoreDatum_Green",
            "DanScoreDatum_Blue",
            "DanScoreDatum_Yellow",
            "DanScoreDatum_Red",
            "DanScoreDatum_White",
            "DanScoreDatum_Murasaki",
            "DanScoreDatum_Kimidori",
            "BlueTokkunStageResults",
            "YellowTokkunStageResults",
            "WhiteTokkunStageResults",
            "BlueShopSeasonStates",
            "GreenShopSeasonStates",
            "YellowShopSeasonStates",
            "RedDonChallengeRawFacts",
            "RedDonChallengeProgress",
            "WhiteDonChallengeRawFacts",
            "WhiteDonChallengeProgress"
        };

        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var tableName in tableNames)
        {
            counts[tableName] = await CountRowsAsync(tableName, baid);
        }

        return counts;
    }

    public async Task<int> CountRowsAsync(string tableName, uint baid)
    {
        await using var command = Context.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE Baid = @baid";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@baid";
        parameter.Value = baid;
        command.Parameters.Add(parameter);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await connection.DisposeAsync();
    }

    private async Task<bool> RowExistsAsync(string tableName, uint baid)
        => await CountRowsAsync(tableName, baid) > 0;

    private async Task InsertOrReplaceAsync(string tableName, IReadOnlyDictionary<string, object?> values)
        => await InsertAsync(tableName, values, replace: true);

    private async Task InsertAsync(string tableName, IReadOnlyDictionary<string, object?> values)
        => await InsertAsync(tableName, values, replace: false);

    private async Task InsertAsync(string tableName, IReadOnlyDictionary<string, object?> values, bool replace)
    {
        await EnsureMomoiroReadbackTablesAsync();
        await using var command = Context.Database.GetDbConnection().CreateCommand();
        var columns = values.Keys.ToArray();
        var parameterNames = columns.Select((_, index) => $"@p{index}").ToArray();
        command.CommandText =
            $"{(replace ? "INSERT OR REPLACE" : "INSERT")} INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameterNames)})";

        for (var i = 0; i < columns.Length; i++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterNames[i];
            parameter.Value = values[columns[i]] ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        await command.ExecuteNonQueryAsync();
    }

    internal sealed class TestMomoiroCatalog : IMomoiroCatalog
    {
        private readonly IReadOnlyList<Ac15MusicInfoEntry> musicInfoFileOrder;

        public TestMomoiroCatalog(
            IReadOnlyList<Ac15MusicInfoEntry>? musicInfoFileOrder = null,
            IReadOnlyList<Ac15TaikojukuEntry>? daniFileOrder = null)
        {
            this.musicInfoFileOrder = musicInfoFileOrder ?? DefaultMusicInfoFileOrder;
            DaniFileOrder = daniFileOrder ?? DefaultDaniFileOrder;
        }

        public GameEra Era => GameEra.Momoiro;

        public uint SongHashVersion => 538_116_869;

        public IReadOnlyList<Ac15MusicInfoEntry> MusicInfoFileOrder => musicInfoFileOrder;

        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo, song => (IMusicInfoEntry)song);

        public IReadOnlyList<ushort> SongHashTable
            => MusicInfoFileOrder.Select(song => checked((ushort)song.SongNo)).ToArray();

        public IReadOnlyDictionary<uint, Ac15MusicInfoEntry> MomoiroMusicInfos
            => MusicInfoFileOrder.ToDictionary(song => song.SongNo);

        public IReadOnlyDictionary<uint, Ac15TelopEntry> Telops { get; init; } =
            new Dictionary<uint, Ac15TelopEntry>();

        public IReadOnlyList<Ac15TaikojukuEntry> DaniFileOrder { get; }

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
                [10] = new() { TitleId = 10, TitleName = "Momoiro Title", TitleRarity = 0 }
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
            Enumerable.Range(1, 379)
                .Select(songNo => new Ac15MusicInfoEntry
                {
                    SongNo = (uint)songNo,
                    MusicId = $"m{songNo:D3}",
                    FileOrder = songNo - 1
                })
                .Append(new Ac15MusicInfoEntry
                {
                    SongNo = HighSongNo,
                    MusicId = "high",
                    FileOrder = HighSongOrdinal
                })
                .ToArray();

        private static IReadOnlyList<Ac15TaikojukuEntry> DefaultDaniFileOrder { get; } =
        [
            new()
            {
                UniqueId = 20_001,
                ChallengeLevel = 1,
                DanLevel = 1,
                VerupNo = 1,
                Name = "Momoiro Dan 1",
                Songs =
                [
                    new Ac15TaikojukuSong { SongNo = 101, Level = 1 },
                    new Ac15TaikojukuSong { SongNo = 102, Level = 1 }
                ]
            }
        ];
    }
}
