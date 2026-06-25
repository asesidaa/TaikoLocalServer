using System.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Infrastructure;

namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroCatalogLoaderTests
{
    [Fact]
    public async Task CatalogInitialize_LoadsRootLevelMomoiroDataThroughRegisteredGameDataCatalog()
    {
        CopyMomoiroCatalogFilesToProcessRoot();
        await using var provider = BuildMomoiroProvider();
        var catalog = provider.GetRequiredService<IGameDataCatalog>();

        await catalog.InitializeAsync(CancellationToken.None);

        var momoiro = catalog.For(GameEra.Momoiro);
        Assert.Equal(GameEra.Momoiro, momoiro.Era);
        Assert.Equal(380, momoiro.MusicInfos.Count);

        var songHashVersion = GetRequiredPropertyValue<uint>(momoiro, "SongHashVersion");
        Assert.Equal(538_116_869u, songHashVersion);

        var songHashTable = GetRequiredPropertyValue<IReadOnlyList<ushort>>(momoiro, "SongHashTable");
        Assert.Equal(380, songHashTable.Count);
        Assert.Equal(760, Ac15SongHashCodec.EncodeTable(songHashTable).Length);

        var musicInfoFileOrder = GetRequiredPropertyValue<IReadOnlyList<Ac15MusicInfoEntry>>(momoiro, "MusicInfoFileOrder");
        Assert.Equal(380, musicInfoFileOrder.Count);
    }

    [Fact]
    public async Task CatalogInitialize_RequiresRootLevelMusicMedleyDefMusicAndTuningInputs()
    {
        CopyMomoiroCatalogFilesToProcessRoot(skipDefMusic: true);
        await using var provider = BuildMomoiroProvider();
        var catalog = provider.GetRequiredService<IGameDataCatalog>();
        var momoiro = catalog.For(GameEra.Momoiro);

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(
            () => momoiro.InitializeAsync(CancellationToken.None));

        Assert.Contains("defmusic.bin", ex.FileName ?? ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ServiceProvider BuildMomoiroProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(BuildConfiguration(), new HashSet<GameEra> { GameEra.Momoiro });
        return services.BuildServiceProvider();
    }

    private static IConfigurationRoot BuildConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DbFileName"] = $"momoiro-catalog-test-{Guid.NewGuid():N}.db",
                ["AuthSettings:JwtIssuer"] = "test",
                ["AuthSettings:JwtAudience"] = "test",
                ["AuthSettings:JwtKey"] = "0123456789abcdef0123456789abcdef",
                ["ServerSettings:Eras:Momoiro:Enabled"] = "true",
                ["ServerSettings:Eras:Momoiro:AutoExtractCatalog"] = "false",
                ["ServerSettings:Eras:Momoiro:GameDataPath"] = ProcessMomoiroDataRoot()
            })
            .Build();

    private static T GetRequiredPropertyValue<T>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        var value = property.GetValue(instance);
        return Assert.IsAssignableFrom<T>(value);
    }

    private static void CopyMomoiroCatalogFilesToProcessRoot(bool skipDefMusic = false)
    {
        var repoRoot = FindRepoRoot();
        var targetRoot = ProcessMomoiroRoot();
        if (Directory.Exists(targetRoot))
        {
            Directory.Delete(targetRoot, recursive: true);
        }

        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "momoiro", "data", "musicinfo.xml"),
            Path.Combine(targetRoot, "data", "musicinfo.xml"));
        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "momoiro", "data", "musicmedleyinfo.xml"),
            Path.Combine(targetRoot, "data", "musicmedleyinfo.xml"));
        if (!skipDefMusic)
        {
            Copy(
                Path.Combine(repoRoot, "Host", "wwwroot", "data", "momoiro", "data", "defmusic.bin"),
                Path.Combine(targetRoot, "data", "defmusic.bin"));
        }

        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "momoiro", "data", "fumen", "tuning.bin"),
            Path.Combine(targetRoot, "data", "fumen", "tuning.bin"));
    }

    private static string ProcessMomoiroRoot()
        => Path.Combine(
            Path.GetDirectoryName(Environment.ProcessPath)
                ?? throw new ApplicationException("Cannot resolve process directory."),
            "wwwroot",
            "data",
            "momoiro");

    private static string ProcessMomoiroDataRoot()
        => Path.Combine(ProcessMomoiroRoot(), "data");

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static void Copy(string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)
            ?? throw new ApplicationException($"Cannot resolve directory for {destination}."));
        File.Copy(source, destination, overwrite: true);
    }
}
