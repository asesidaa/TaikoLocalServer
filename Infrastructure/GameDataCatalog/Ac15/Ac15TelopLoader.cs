using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public static class Ac15TelopLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static async Task<IReadOnlyDictionary<uint, Ac15TelopEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<uint, Ac15TelopEntry>();
        }

        await using var stream = File.OpenRead(path);
        var raw = await JsonSerializer.DeserializeAsync<RawTelop[]>(stream, JsonOptions, cancellationToken)
                  ?? [];

        return raw
            .Where(entry => entry.TelopId > 0)
            .GroupBy(entry => entry.TelopId)
            .ToDictionary(group => group.Key, group => Map(group.First()));
    }

    private static Ac15TelopEntry Map(RawTelop raw) => new()
    {
        TelopId = raw.TelopId,
        VerupNo = raw.VerupNo,
        StartDatetime = raw.StartDatetime ?? string.Empty,
        EndDatetime = raw.EndDatetime ?? string.Empty,
        Message = raw.Telop ?? string.Empty
    };

    private sealed class RawTelop
    {
        [JsonPropertyName("telopId")]
        public uint TelopId { get; set; }

        [JsonPropertyName("verupNo")]
        public uint VerupNo { get; set; }

        [JsonPropertyName("startDatetime")]
        public string? StartDatetime { get; set; }

        [JsonPropertyName("endDatetime")]
        public string? EndDatetime { get; set; }

        [JsonPropertyName("telop")]
        public string? Telop { get; set; }
    }
}
