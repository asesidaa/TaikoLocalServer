using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTaikojukuLoader
{
    public Task<IReadOnlyList<GreenTaikojukuEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(GreenGameDataPaths.MusicMedleyInfoXml, cancellationToken);
    }

    public static async Task<IReadOnlyList<GreenTaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var root = document.Root ?? throw new InvalidDataException($"Missing root element in {path}");

        return root.Elements("MusicMedleyInfoData")
            .Select(element => new GreenTaikojukuEntry
            {
                UniqueId = ReadUInt(element, "uniqueid"),
                DanLevel = ReadUInt(element, "challengelv"),
                Name = ReadString(element, "medleyname"),
                Difficulty = ReadUInt(element, "difficulty"),
                ChallengeLevel = ReadUInt(element, "challengelv"),
                Songs = element.Elements("Content")
                    .Select(content => new GreenTaikojukuSong
                    {
                        MusicId = ReadString(content, "musicid"),
                        SongNo = ReadUInt(content, "uniqueid"),
                        Level = ReadUInt(content, "difficulty"),
                        Notes = ReadUInt(content, "notes")
                    })
                    .ToArray()
            })
            .ToArray();
    }

    private static string ReadString(XContainer element, string name)
        => element.Element(name)?.Value ?? string.Empty;

    private static uint ReadUInt(XContainer element, string name)
        => uint.TryParse(element.Element(name)?.Value, out var parsed) ? parsed : 0;
}
