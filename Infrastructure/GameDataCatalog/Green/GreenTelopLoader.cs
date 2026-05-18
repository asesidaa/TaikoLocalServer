using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTelopLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public Task<IReadOnlyDictionary<uint, GreenTelopEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(Domain.Enums.GameEra.Green), "telop_data.json");
        return LoadFromFileAsync(path, cancellationToken);
    }

    public static async Task<IReadOnlyDictionary<uint, GreenTelopEntry>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<uint, GreenTelopEntry>();
        }

        await using var stream = File.OpenRead(path);
        var raw = await JsonSerializer.DeserializeAsync<RawTelop[]>(stream, JsonOptions, cancellationToken)
                  ?? [];

        return raw
            .Where(entry => entry.TelopId > 0)
            .GroupBy(entry => entry.TelopId)
            .ToDictionary(group => group.Key, group => Map(group.First()));
    }

    private static GreenTelopEntry Map(RawTelop raw) => new()
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
