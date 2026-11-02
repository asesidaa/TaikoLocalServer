using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public sealed class Ac15MusicInfoLoader
{
    public static async Task<Ac15MusicInfoLoadResult> LoadFromFileAsync(
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
            .Select(MapEntry)
            .ToArray();

        return new Ac15MusicInfoLoadResult(version, entries);
    }

    private static Ac15MusicInfoEntry MapEntry(XElement element, int index)
    {
        var genreName = ReadString(element, "genrename");
        return new Ac15MusicInfoEntry
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
            GenreName = genreName,
            CategoryId = Ac15MusicMetadata.MapGenreNameToCategoryId(genreName),
            DemoPlay = ReadUInt(element, "demoplay"),
            Tags = element.Elements("tag").Select(tag => ParseUInt(tag.Value)).ToArray(),
            FileOrder = index
        };
    }

    private static string ReadString(XContainer element, string name)
        => element.Element(name)?.Value ?? string.Empty;

    private static uint ReadUInt(XContainer element, string name)
        => ParseUInt(element.Element(name)?.Value);

    private static uint ParseUInt(string? value)
        => uint.TryParse(value, out var parsed) ? parsed : 0;
}
