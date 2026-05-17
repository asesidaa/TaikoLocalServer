using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor.Output;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenCustomizationCatalogLoaderTests
{
    [Fact]
    public async Task CostumeLoader_MissingFileReturnsEmptyList()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var items = await GreenCostumeLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Empty(items);
    }

    [Fact]
    public async Task TitleLoader_ReadsEnvelopeAsDictionary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new GreenCatalogEnvelope<Title>
        {
            Items =
            [
                new Title { TitleId = 132, TitleName = "B" },
                new Title { TitleId = 131, TitleName = "A" }
            ]
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var items = await GreenTitleLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Equal(new uint[] { 131, 132 }, items.Keys.OrderBy(id => id));
        File.Delete(path);
    }

    [Fact]
    public async Task NeiroLoader_ReadsEnvelopeAsDictionary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new GreenCatalogEnvelope<Neiro>
        {
            Items =
            [
                new Neiro { NeiroId = 4, NeiroName = "Tone" }
            ]
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var items = await GreenNeiroLoader.LoadFromFileAsync(path, CancellationToken.None);

        Assert.Equal("Tone", items[4].NeiroName);
        File.Delete(path);
    }

    [Fact]
    public async Task CatalogInitialize_MalformedRewardTitleFilteringXmlDoesNotThrow()
    {
        CopyGreenRuntimeCatalogFilesToProcessRoot();
        DeleteGreenCustomizationFilesFromProcessRoot();

        var gameDataRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(gameDataRoot, "nutdata", "cos_name"));
            Directory.CreateDirectory(Path.Combine(gameDataRoot, "nutdata", "title_name"));
            Directory.CreateDirectory(Path.Combine(gameDataRoot, "nutdata", "tone_name"));
            Directory.CreateDirectory(Path.Combine(gameDataRoot, "config", "S11100-1"));

            await File.WriteAllBytesAsync(Path.Combine(gameDataRoot, "nutdata", "cos_name", "nutdatapack.ndp"), BuildNdp(("cos_name_001.nut", 0, 1)));
            await File.WriteAllBytesAsync(Path.Combine(gameDataRoot, "nutdata", "title_name", "nutdatapack.ndp"), BuildNdp(("title_name_131.nut", 0, 1)));
            await File.WriteAllBytesAsync(Path.Combine(gameDataRoot, "nutdata", "tone_name", "nutdatapack.ndp"), BuildNdp(("tone_name_004.nut", 0, 1)));
            await File.WriteAllTextAsync(Path.Combine(gameDataRoot, "config", "S11100-1", "rewardtitlefiltering.xml"), "<boost_serialization>");

            var settings = Options.Create(new ServerSettings
            {
                Eras = new Dictionary<string, EraSettings>
                {
                    [nameof(GameEra.Green)] = new()
                    {
                        Enabled = true,
                        AutoExtractCatalog = true,
                        GameDataPath = gameDataRoot
                    }
                }
            });
            var catalog = new GreenEraGameDataCatalog(NullLogger<GreenEraGameDataCatalog>.Instance, settings);

            await catalog.InitializeAsync(CancellationToken.None);

            Assert.NotEmpty(catalog.MusicInfoFileOrder);
            Assert.Empty(catalog.GetCostumeList());
            Assert.Empty(catalog.GetTitleDictionary());
            Assert.Empty(catalog.GetNeiroDictionary());
        }
        finally
        {
            if (Directory.Exists(gameDataRoot))
            {
                Directory.Delete(gameDataRoot, recursive: true);
            }

            DeleteGreenCustomizationFilesFromProcessRoot();
        }
    }

    private static void CopyGreenRuntimeCatalogFilesToProcessRoot()
    {
        var repoRoot = FindRepoRoot();
        var sourceRoot = Path.Combine(repoRoot, "Host", "wwwroot", "data", "green", "data");
        var targetRoot = Path.Combine(GetProcessGreenDataPath(), "data");

        Copy(
            Path.Combine(sourceRoot, "config", "S11100-1", "musicinfo.xml"),
            Path.Combine(targetRoot, "config", "S11100-1", "musicinfo.xml"));
        Copy(
            Path.Combine(sourceRoot, "config", "S11100-1", "musicmedleyinfo.xml"),
            Path.Combine(targetRoot, "config", "S11100-1", "musicmedleyinfo.xml"));
        Copy(
            Path.Combine(sourceRoot, "fumen", "tuning.bin"),
            Path.Combine(targetRoot, "fumen", "tuning.bin"));
    }

    private static void DeleteGreenCustomizationFilesFromProcessRoot()
    {
        var dataPath = GetProcessGreenDataPath();
        File.Delete(Path.Combine(dataPath, GreenCatalogExtractor.CostumeFileName));
        File.Delete(Path.Combine(dataPath, GreenCatalogExtractor.TitleFileName));
        File.Delete(Path.Combine(dataPath, GreenCatalogExtractor.NeiroFileName));
    }

    private static string GetProcessGreenDataPath()
    {
        var processDirectory = Path.GetDirectoryName(Environment.ProcessPath)
            ?? throw new ApplicationException("Cannot resolve process directory.");
        return Path.Combine(processDirectory, "wwwroot", "data", "green");
    }

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

    private static byte[] BuildNdp(params (string Name, uint Offset, uint Size)[] entries)
    {
        var bytes = new byte[0x50 + entries.Sum(entry => 4 + Align4(Encoding.ASCII.GetByteCount(entry.Name) + 1) + 8)];
        Encoding.ASCII.GetBytes("NUT_PACK_TYPE1").CopyTo(bytes, 0);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(0x40), (uint)entries.Length);
        var cursor = 0x50;

        foreach (var entry in entries)
        {
            var nameBytes = Encoding.ASCII.GetBytes(entry.Name);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor), (uint)(nameBytes.Length + 1));
            cursor += 4;
            nameBytes.CopyTo(bytes.AsSpan(cursor));
            cursor += nameBytes.Length + 1;
            cursor = Align4(cursor);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor), entry.Offset);
            BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(cursor + 4), entry.Size);
            cursor += 8;
        }

        return bytes;
    }

    private static int Align4(int value) => (value + 3) & ~3;
}
