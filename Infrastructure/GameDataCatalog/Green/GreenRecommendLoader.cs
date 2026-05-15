using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenRecommendLoader
{
    public async Task<GreenRecommendEntry> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(Domain.Enums.GameEra.Green), "recommend_songs.json");
        return await LoadFromFileAsync(path, catalogSongIds, cancellationToken);
    }

    public static async Task<GreenRecommendEntry> LoadFromFileAsync(
        string path,
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return GreenRecommendEntry.Empty;
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

        return new GreenRecommendEntry
        {
            RecommendSong = single,
            RecommendBestSongs = list
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private sealed class RawRecommend
    {
        [JsonPropertyName("recommendSong")]
        public uint RecommendSong { get; set; }

        [JsonPropertyName("recommendBestSongs")]
        public uint[]? RecommendBestSongs { get; set; }
    }
}
