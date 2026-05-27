using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Common;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenEventFolderLoader
{
    public const string FileName = "green_event_folder_data.json";

    private const uint MaxProtocolFolderId = 15;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public Task<IReadOnlyDictionary<uint, EventFolderData>> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Green), FileName);
        return LoadFromFileAsync(path, catalogSongIds, cancellationToken);
    }

    public static async Task<IReadOnlyDictionary<uint, EventFolderData>> LoadFromFileAsync(
        string path,
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<uint, EventFolderData>();
        }

        RawEventFolder[] raw;
        try
        {
            await using var stream = File.OpenRead(path);
            raw = await JsonSerializer.DeserializeAsync<RawEventFolder[]>(stream, JsonOptions, cancellationToken)
                  ?? [];
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Green event folder data is malformed: {path}", ex);
        }

        var byId = new Dictionary<uint, EventFolderData>();
        for (var index = 0; index < raw.Length; index++)
        {
            var folder = Map(raw[index], index, catalogSongIds);
            if (!byId.TryAdd(folder.FolderId, folder))
            {
                throw new InvalidDataException($"Green event folder data contains duplicate folderId {folder.FolderId}.");
            }
        }

        return byId;
    }

    private static EventFolderData Map(
        RawEventFolder raw,
        int index,
        IReadOnlySet<uint> catalogSongIds)
    {
        var rowNumber = index + 1;
        if (raw.FolderId is not { } folderId)
        {
            throw new InvalidDataException($"Green event folder row {rowNumber} is missing folderId.");
        }

        if (folderId is 0 or > MaxProtocolFolderId)
        {
            throw new InvalidDataException($"Green event folder row {rowNumber} has unsupported folderId {folderId}.");
        }

        if (raw.VerupNo is not { } verupNo)
        {
            throw new InvalidDataException($"Green event folder row {rowNumber} is missing verupNo.");
        }

        if (raw.SongNoes is null)
        {
            throw new InvalidDataException($"Green event folder row {rowNumber} is missing songNo.");
        }

        if (raw.SongNoes.Length == 0)
        {
            throw new InvalidDataException($"Green event folder row {rowNumber} has empty songNo.");
        }

        var maxSongId = (uint)(GreenProtocolBytes.SongFlagBytes * 8);
        foreach (var songNo in raw.SongNoes)
        {
            if (songNo >= maxSongId)
            {
                throw new InvalidDataException($"Green event folder {folderId} contains songNo {songNo}, but the limit is {maxSongId - 1}.");
            }

            if (!catalogSongIds.Contains(songNo))
            {
                throw new InvalidDataException($"Green event folder {folderId} contains unknown songNo {songNo}.");
            }
        }

        return new EventFolderData
        {
            FolderId = folderId,
            VerupNo = verupNo,
            SongNoes = raw.SongNoes
        };
    }

    private sealed class RawEventFolder
    {
        [JsonPropertyName("folderId")]
        public uint? FolderId { get; set; }

        [JsonPropertyName("verupNo")]
        public uint? VerupNo { get; set; }

        [JsonPropertyName("songNo")]
        public uint[]? SongNoes { get; set; }
    }
}
