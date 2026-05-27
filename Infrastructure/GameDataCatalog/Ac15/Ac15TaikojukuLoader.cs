using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public sealed class Ac15TaikojukuLoader
{
    public static async Task<IReadOnlyList<Ac15TaikojukuEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var root = document.Root ?? throw new InvalidDataException($"Missing root element in {path}");

        return root.Elements("MusicMedleyInfoData")
            .Select(element => new Ac15TaikojukuEntry
            {
                UniqueId = ReadUInt(element, "uniqueid"),
                DanLevel = ReadUInt(element, "challengelv"),
                Name = ReadString(element, "medleyname"),
                Difficulty = ReadUInt(element, "difficulty"),
                ChallengeLevel = ReadUInt(element, "challengelv"),
                Conditions = ReadConditions(element.Element("Conditions")),
                ExcellentConditions = ReadConditions(element.Element("ExcellentConditions")),
                Songs = element.Elements("Content")
                    .Select(content => new Ac15TaikojukuSong
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

    private static Ac15TaikojukuConditions ReadConditions(XContainer? element)
    {
        if (element is null)
        {
            return Ac15TaikojukuConditions.Empty;
        }

        return new Ac15TaikojukuConditions
        {
            SoulGauge = ReadUInt(element, "tamashii") / 100,
            GoodCount = ReadUInt(element, "hit_ryo"),
            OkCount = ReadUInt(element, "hit_ka"),
            BadCount = ReadUInt(element, "hit_fuka"),
            ComboCount = ReadUInt(element, "combo"),
            TotalHitCount = ReadUInt(element, "hits"),
            Score = ReadUInt(element, "score"),
            DrumrollCount = ReadUInt(element, "renda")
        };
    }
}
