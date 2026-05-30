using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

namespace TaikoLocalServer.Tests.Blue;

[Collection("Blue runtime catalog tests")]
public sealed class BlueBattleCatalogLoaderTests
{
    private static readonly string[] ExpectedFileNames =
    [
        "battleadjsetting.xml",
        "battlenpcinfo.xml",
        "battlestageinfo.xml",
        "battlesupportinfo.xml",
        "battletokeninfo.xml"
    ];

    [Fact]
    public async Task LoadFromDirectoryAsync_InventoriesFiveKnownBattleXmlFilesWhenPresent()
    {
        using var scope = BattleDataScope.WithFiles();

        var catalog = await BlueBattleDataLoader.LoadFromDirectoryAsync(scope.BattleRoot, CancellationToken.None);

        Assert.True(catalog.IsRawDataAvailable);
        Assert.True(catalog.EnablesBattleAdvertisement);
        Assert.Equal(ExpectedFileNames, catalog.Files.Select(file => file.FileName).Order(StringComparer.Ordinal));
        Assert.All(catalog.Files, file =>
        {
            Assert.True(file.IsPresent);
            Assert.True(file.IsXmlParsed);
            Assert.Null(file.ParseError);
            Assert.True(file.ElementCount > 0);
        });
        Assert.Equal(2, catalog.Files.Single(file => file.FileName == "battlestageinfo.xml").RowCount);
        Assert.Equal(2, catalog.Files.Single(file => file.FileName == "battletokeninfo.xml").RowCount);
        Assert.Equal([1u, 33u], catalog.ReleaseBattleStageIds);
        Assert.Equal([1u, 10u], catalog.ReleaseBattleSpecialIds);
        Assert.Equal(3u, catalog.BattleBondsLvCap);
    }

    [Fact]
    public async Task LoadFromDirectoryAsync_MissingBattleXmlYieldsRawUnavailableCatalog()
    {
        using var scope = BattleDataScope.Missing();

        var catalog = await BlueBattleDataLoader.LoadFromDirectoryAsync(scope.BattleRoot, CancellationToken.None);

        Assert.False(catalog.IsRawDataAvailable);
        Assert.False(catalog.EnablesBattleAdvertisement);
        Assert.Equal(ExpectedFileNames, catalog.Files.Select(file => file.FileName).Order(StringComparer.Ordinal));
        Assert.All(catalog.Files, file =>
        {
            Assert.False(file.IsPresent);
            Assert.False(file.IsXmlParsed);
            Assert.Equal(0, file.ElementCount);
            Assert.Equal(0, file.RowCount);
        });
    }

    [Fact]
    public void BlueCatalogWiring_ExposesRawBattleCatalogWithoutAdvertisementDefaults()
    {
        var catalog = new BlueHandlerFixture.TestBlueCatalog();

        Assert.Same(BlueBattleCatalog.Unavailable, catalog.BattleCatalog);
        Assert.False(catalog.BattleCatalog.IsRawDataAvailable);
        Assert.False(catalog.BattleCatalog.EnablesBattleAdvertisement);
    }

    [Fact]
    public void ProductionBattleCatalogConsumers_DoNotDeriveRuntimeSemantics()
    {
        var root = FindRepoRoot();
        var files = new[]
        {
            Path.Combine(root, "Application", "Handlers", "GetInitialDataQuery.Blue.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Mappers", "BattleUserDataMappers.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Controllers", "BattleUserDataController.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Mappers", "PlayResultMappers.cs"),
            Path.Combine(root, "Application", "Handlers", "UpdatePlayResultCommand.Blue.cs")
        };
        var forbidden = new[]
        {
            "AssignStageId",
            "LastBossLife",
            "LastBattleStageId",
            "AssignNextStageId",
            "battlestageinfo",
            "battletokeninfo",
            "battlenpcinfo",
            "0xFE",
            "0x07"
        };

        foreach (var file in files.Where(File.Exists))
        {
            var source = File.ReadAllText(file);
            if (!source.Contains("BattleCatalog", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var token in forbidden)
            {
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
            }

            Assert.DoesNotContain("stage 33", source, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("reward", source, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }

    private sealed class BattleDataScope : IDisposable
    {
        private readonly string root;

        private BattleDataScope(bool writeFiles)
        {
            root = Path.Combine(Path.GetTempPath(), "TaikoLocalServer-BlueBattleCatalog", Guid.NewGuid().ToString("N"));
            BattleRoot = Path.Combine(root, "battle");
            if (writeFiles)
            {
                Directory.CreateDirectory(BattleRoot);
                File.WriteAllText(Path.Combine(BattleRoot, "battleadjsetting.xml"), "<root><adjustedsetting id=\"1\" /></root>");
                File.WriteAllText(Path.Combine(BattleRoot, "battlenpcinfo.xml"), "<root><npcinfo><id>1</id><requred_exp>10</requred_exp><requred_exp>20</requred_exp><requred_exp>30</requred_exp></npcinfo></root>");
                File.WriteAllText(Path.Combine(BattleRoot, "battlestageinfo.xml"), "<root><stageinfo id=\"1\" /><stageinfo id=\"33\" /></root>");
                File.WriteAllText(Path.Combine(BattleRoot, "battlesupportinfo.xml"), "<root><supportinfo musicid=\"a\" /></root>");
                File.WriteAllText(Path.Combine(BattleRoot, "battletokeninfo.xml"), "<root><tokeninfo><id>1</id><rewardtbl><reward><id>1</id></reward><reward><id>10</id></reward></rewardtbl></tokeninfo></root>");
            }
        }

        public string BattleRoot { get; }

        public static BattleDataScope WithFiles() => new(writeFiles: true);

        public static BattleDataScope Missing() => new(writeFiles: false);

        public void Dispose()
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
