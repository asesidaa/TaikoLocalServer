using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public static class Ac15EventFolderLoader
{
    private const uint MaxProtocolFolderId = 15;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static async Task<IReadOnlyDictionary<uint, EventFolderData>> LoadFromFileAsync(
        string path,
        IReadOnlySet<uint> catalogSongIds,
        string eraName,
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
            throw new InvalidDataException($"{eraName} event folder data is malformed: {path}", ex);
        }

        var byId = new Dictionary<uint, EventFolderData>();
        for (var index = 0; index < raw.Length; index++)
        {
            var folder = Map(raw[index], index, catalogSongIds, eraName);
            if (!byId.TryAdd(folder.FolderId, folder))
            {
                throw new InvalidDataException($"{eraName} event folder data contains duplicate folderId {folder.FolderId}.");
            }
        }

        return byId;
    }

    private static EventFolderData Map(
        RawEventFolder raw,
        int index,
        IReadOnlySet<uint> catalogSongIds,
        string eraName)
    {
        var rowNumber = index + 1;
        if (raw.FolderId is not { } folderId)
        {
            throw new InvalidDataException($"{eraName} event folder row {rowNumber} is missing folderId.");
        }

        if (folderId is 0 or > MaxProtocolFolderId)
        {
            throw new InvalidDataException($"{eraName} event folder row {rowNumber} has unsupported folderId {folderId}.");
        }

        if (raw.VerupNo is not { } verupNo)
        {
            throw new InvalidDataException($"{eraName} event folder row {rowNumber} is missing verupNo.");
        }

        if (raw.SongNoes is null)
        {
            throw new InvalidDataException($"{eraName} event folder row {rowNumber} is missing songNo.");
        }

        if (raw.SongNoes.Length == 0)
        {
            throw new InvalidDataException($"{eraName} event folder row {rowNumber} has empty songNo.");
        }

        var maxSongId = (uint)(GreenProtocolBytes.SongFlagBytes * 8);
        foreach (var songNo in raw.SongNoes)
        {
            if (songNo >= maxSongId)
            {
                throw new InvalidDataException($"{eraName} event folder {folderId} contains songNo {songNo}, but the limit is {maxSongId - 1}.");
            }

            if (!catalogSongIds.Contains(songNo))
            {
                throw new InvalidDataException($"{eraName} event folder {folderId} contains unknown songNo {songNo}.");
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
