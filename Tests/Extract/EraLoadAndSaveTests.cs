namespace TaikoLocalServer.Tests.Extract;

// FileGameDataCatalog keeps a game that fails to load from stopping the rest; EraDataFiles puts back a saved file that
// breaks its game (Infrastructure/GameDataCatalog/EraDataFiles.cs).
public sealed class EraLoadAndSaveTests
{
    private sealed class FakeEra(GameEra era, Func<Task> load) : IEraGameDataCatalog
    {
        public GameEra Era => era;
        public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos { get; } = new Dictionary<uint, IMusicInfoEntry>();
        public Task InitializeAsync(CancellationToken cancellationToken) => load();
    }

    [Fact]
    public async Task OneGameFailing_OthersStillLoad()
    {
        var catalog = new FileGameDataCatalog([
            new FakeEra(GameEra.Red, () => throw new FileNotFoundException("musicinfo.xml is missing")),
            new FakeEra(GameEra.Blue, () => Task.CompletedTask)]);

        await catalog.InitializeAsync();

        Assert.Equal("musicinfo.xml is missing", catalog.LoadResults[GameEra.Red].Error);
        Assert.Null(catalog.LoadResults[GameEra.Blue].Error);
    }

    [Fact]
    public async Task SaveThatBreaksTheGame_IsPutBack()
    {
        var dir = PathHelper.GetDataPath(GameEra.Momoiro);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "momoiro_telop_test_data.json");
        await File.WriteAllTextAsync(path, "[]");
        try
        {
            // The game "fails" whenever this file isn't an empty list.
            var catalog = new FileGameDataCatalog([new FakeEra(GameEra.Momoiro, async () =>
            {
                if ((await File.ReadAllTextAsync(path)).Trim() != "[]") throw new InvalidDataException("bad telop");
            })]);
            await catalog.InitializeAsync();

            Assert.Equal("bad telop", await EraDataFiles.SaveAsync(catalog, GameEra.Momoiro, "momoiro_telop_test_data.json", "[1]", default));
            Assert.Equal("[]", await File.ReadAllTextAsync(path));
            Assert.Null(catalog.LoadResults[GameEra.Momoiro].Error);

            Assert.NotNull(await EraDataFiles.SaveAsync(catalog, GameEra.Momoiro, "../taiko.db3", "[]", default));
            Assert.NotNull(await EraDataFiles.SaveAsync(catalog, GameEra.Momoiro, "momoiro_telop_test_data.json", "{oops", default));
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".bak");
        }
    }
}
