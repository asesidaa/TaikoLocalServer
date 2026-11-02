using System.Text;
using TaikoLocalServer.Application.ServerData;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenEventFolderLoaderTests
{
    [Fact]
    public async Task LoadFromFile_MissingFileReturnsEmptyDictionary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var folders = await GreenEventFolderLoader.LoadFromFileAsync(
            path,
            KnownSongs(101),
            CancellationToken.None);

        Assert.Empty(folders);
    }

    [Fact]
    public async Task LoadFromFile_ReadsFolderRowsAndAllowsZeroVerupNo()
    {
        var path = await WriteTempJsonAsync("""
            [
              { "folderId": 1, "verupNo": 0, "songNo": [101, 102] },
              { "folderId": 11, "verupNo": 3, "songNo": [103] }
            ]
            """);
        try
        {
            var folders = await GreenEventFolderLoader.LoadFromFileAsync(
                path,
                KnownSongs(101, 102, 103),
                CancellationToken.None);

            Assert.Equal(2, folders.Count);
            Assert.Equal(0u, folders[1].VerupNo);
            Assert.Equal(new uint[] { 101, 102 }, folders[1].SongNoes);
            Assert.Equal(3u, folders[11].VerupNo);
            Assert.Equal(new uint[] { 103 }, folders[11].SongNoes);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadFromFile_DefaultGreenEventFolderDataLoads()
    {
        var path = FindDefaultEventFolderDataPath();

        Assert.NotNull(path);

        var folders = await GreenEventFolderLoader.LoadFromFileAsync(
            path,
            KnownSongs(Enumerable.Range(0, 1024).Select(id => (uint)id).ToArray()),
            CancellationToken.None);

        Assert.Equal(9, folders.Count);
        Assert.Contains(1u, folders.Keys);
        Assert.Contains(8u, folders.Keys);
        Assert.Contains(11u, folders.Keys);
        Assert.Equal(38, folders[1].SongNoes!.Length);
        Assert.Equal(16, folders[11].SongNoes!.Length);
        Assert.Equal(877u, folders[1].SongNoes![0]);
    }

    [Theory]
    [InlineData("""[{ "verupNo": 1, "songNo": [101] }]""", "folderId")]
    [InlineData("""[{ "folderId": 0, "verupNo": 1, "songNo": [101] }]""", "folderId")]
    [InlineData("""[{ "folderId": 16, "verupNo": 1, "songNo": [101] }]""", "folderId")]
    [InlineData("""[{ "folderId": 1, "songNo": [101] }]""", "verupNo")]
    [InlineData("""[{ "folderId": 1, "verupNo": 1 }]""", "songNo")]
    [InlineData("""[{ "folderId": 1, "verupNo": 1, "songNo": [] }]""", "songNo")]
    [InlineData("""[{ "folderId": 1, "verupNo": 1, "songNo": [1024] }]""", "1024")]
    [InlineData("""[{ "folderId": 1, "verupNo": 1, "songNo": [999] }]""", "unknown")]
    public async Task LoadFromFile_RejectsInvalidRows(string json, string failureText)
    {
        var path = await WriteTempJsonAsync(json);
        try
        {
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
                GreenEventFolderLoader.LoadFromFileAsync(
                    path,
                    KnownSongs(101),
                    CancellationToken.None));

            Assert.Contains(failureText, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task LoadFromFile_RejectsDuplicateFolderIds()
    {
        var path = await WriteTempJsonAsync("""
            [
              { "folderId": 1, "verupNo": 1, "songNo": [101] },
              { "folderId": 1, "verupNo": 2, "songNo": [102] }
            ]
            """);
        try
        {
            var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
                GreenEventFolderLoader.LoadFromFileAsync(
                    path,
                    KnownSongs(101, 102),
                    CancellationToken.None));

            Assert.Contains("duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static HashSet<uint> KnownSongs(params uint[] songIds)
        => new(songIds);

    private static async Task<string> WriteTempJsonAsync(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, json, Encoding.UTF8);
        return path;
    }

    private static string? FindDefaultEventFolderDataPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var path = Path.Combine(
                directory.FullName,
                "Host",
                "wwwroot",
                "data",
                "green",
                GreenEventFolderLoader.FileName);

            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
