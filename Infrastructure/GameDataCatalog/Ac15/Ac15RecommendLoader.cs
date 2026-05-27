using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public static class Ac15RecommendLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<Ac15RecommendEntry> LoadFromFileAsync(
        string path,
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return Ac15RecommendEntry.Empty;
        }

        await using var stream = File.OpenRead(path);
        var raw = await JsonSerializer.DeserializeAsync<RawRecommend>(stream, JsonOptions, cancellationToken)
                  ?? new RawRecommend();

        var maxBits = (uint)(GreenProtocolBytes.SongFlagBytes * 8);
        bool IsValid(uint id) => id > 0 && id < maxBits && catalogSongIds.Contains(id);

        var single = IsValid(raw.RecommendSong) ? raw.RecommendSong : 0u;
        var list = (raw.RecommendBestSongs ?? [])
            .Where(IsValid)
            .ToArray();

        return new Ac15RecommendEntry
        {
            RecommendSong = single,
            RecommendBestSongs = list
        };
    }

    private sealed class RawRecommend
    {
        [JsonPropertyName("recommendSong")]
        public uint RecommendSong { get; set; }

        [JsonPropertyName("recommendBestSongs")]
        public uint[]? RecommendBestSongs { get; set; }
    }
}
