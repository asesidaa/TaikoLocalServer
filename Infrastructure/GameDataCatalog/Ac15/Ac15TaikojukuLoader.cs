using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public sealed class Ac15TaikojukuLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

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

    public static async Task<IReadOnlyList<Ac15TaikojukuEntry>> LoadFromFileAsync(
        string path,
        string verupPath,
        string eraName,
        CancellationToken cancellationToken)
    {
        var entries = await LoadFromFileAsync(path, cancellationToken);
        return await ApplyVerupSidecarAsync(entries, verupPath, eraName, cancellationToken);
    }

    private static async Task<IReadOnlyList<Ac15TaikojukuEntry>> ApplyVerupSidecarAsync(
        IReadOnlyList<Ac15TaikojukuEntry> entries,
        string verupPath,
        string eraName,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(verupPath))
        {
            return entries;
        }

        RawTaikojukuVerup raw;
        try
        {
            await using var stream = File.OpenRead(verupPath);
            raw = await JsonSerializer.DeserializeAsync<RawTaikojukuVerup>(stream, JsonOptions, cancellationToken)
                  ?? new RawTaikojukuVerup();
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"{eraName} taikojuku verup data is malformed: {verupPath}", ex);
        }

        var challengeLevels = entries
            .Select(entry => entry.ChallengeLevel)
            .ToHashSet();
        var overrides = new Dictionary<uint, uint>();
        foreach (var pack in raw.Packs ?? [])
        {
            if (pack.ChallengeLevel is not { } challengeLevel || challengeLevel == 0)
            {
                throw new InvalidDataException($"{eraName} taikojuku verup data contains a pack with missing or zero challengeLevel.");
            }

            if (pack.VerupNo is not { } verupNo)
            {
                throw new InvalidDataException($"{eraName} taikojuku verup data for challengeLevel {challengeLevel} is missing verupNo.");
            }

            if (!challengeLevels.Contains(challengeLevel))
            {
                throw new InvalidDataException($"{eraName} taikojuku verup data references unknown challengeLevel {challengeLevel}.");
            }

            if (!overrides.TryAdd(challengeLevel, verupNo))
            {
                throw new InvalidDataException($"{eraName} taikojuku verup data contains duplicate challengeLevel {challengeLevel}.");
            }
        }

        var defaultVerupNo = raw.DefaultVerupNo ?? 0;
        return entries
            .Select(entry => CopyWithVerupNo(
                entry,
                overrides.TryGetValue(entry.ChallengeLevel, out var overrideVerupNo)
                    ? overrideVerupNo
                    : defaultVerupNo))
            .ToArray();
    }

    private static Ac15TaikojukuEntry CopyWithVerupNo(Ac15TaikojukuEntry entry, uint verupNo) => new()
    {
        UniqueId = entry.UniqueId,
        DanLevel = entry.DanLevel,
        ChallengeLevel = entry.ChallengeLevel,
        Name = entry.Name,
        Difficulty = entry.Difficulty,
        VerupNo = verupNo,
        Conditions = entry.Conditions,
        ExcellentConditions = entry.ExcellentConditions,
        Songs = entry.Songs
    };

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

    private sealed class RawTaikojukuVerup
    {
        [JsonPropertyName("defaultVerupNo")]
        public uint? DefaultVerupNo { get; set; }

        [JsonPropertyName("packs")]
        public RawTaikojukuVerupPack[]? Packs { get; set; }
    }

    private sealed class RawTaikojukuVerupPack
    {
        [JsonPropertyName("challengeLevel")]
        public uint? ChallengeLevel { get; set; }

        [JsonPropertyName("verupNo")]
        public uint? VerupNo { get; set; }
    }
}
