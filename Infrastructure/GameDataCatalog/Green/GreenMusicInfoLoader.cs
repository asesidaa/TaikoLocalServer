using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed record GreenMusicInfoLoadResult(
    uint SongHashVersion,
    IReadOnlyList<GreenMusicInfoEntry> Entries);

public sealed class GreenMusicInfoLoader
{
    public Task<GreenMusicInfoLoadResult> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataTablePath(GameEra.Green), "musicinfo.xml");
        return LoadFromFileAsync(path, cancellationToken);
    }

    public static async Task<GreenMusicInfoLoadResult> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var root = document.Root ?? throw new InvalidDataException($"Missing root element in {path}");

        var musicInfo = root.Element("MusicInfo")
            ?? throw new InvalidDataException($"Missing MusicInfo in {path}");
        var header = musicInfo.Element("Header")
            ?? throw new InvalidDataException($"Missing MusicInfo/Header in {path}");
        var version = ParseUInt(header.Element("version")?.Value);

        var entries = musicInfo
            .Elements("Data")
            .Select((element, index) => new GreenMusicInfoEntry
            {
                MusicId = ReadString(element, "musicid"),
                SongNo = ReadUInt(element, "uniqueid"),
                NewRelease = ReadUInt(element, "newrelease"),
                IsSecret = ReadUInt(element, "secret") != 0,
                IsPapaMama = ReadUInt(element, "papamama") != 0,
                HasExtreme = ReadUInt(element, "hasextreme") != 0,
                PartsSet = ReadString(element, "partsset"),
                WaiwaiPartsSet = ReadString(element, "wai2partsset"),
                Title = ReadString(element, "musicname"),
                GenreName = ReadString(element, "genrename"),
                DemoPlay = ReadUInt(element, "demoplay"),
                Tags = element.Elements("tag").Select(tag => ParseUInt(tag.Value)).ToArray(),
                FileOrder = index
            })
            .ToArray();

        return new GreenMusicInfoLoadResult(version, entries);
    }

    private static string ReadString(XContainer element, string name)
        => element.Element(name)?.Value ?? string.Empty;

    private static uint ReadUInt(XContainer element, string name)
        => ParseUInt(element.Element(name)?.Value);

    private static uint ParseUInt(string? value)
        => uint.TryParse(value, out var parsed) ? parsed : 0;
}
