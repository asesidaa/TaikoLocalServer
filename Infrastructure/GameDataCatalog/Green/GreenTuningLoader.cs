using System.Buffers.Binary;
using System.Text;
using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTuningLoader
{
    private const uint ExpectedSongCount = 0x4BA;
    private const int RecordCount = (int)ExpectedSongCount;
    private const int HeaderSize = 4;
    private const int RecordSize = 2_316;
    private const int StringTableOffset = HeaderSize + RecordCount * RecordSize;
    private const int Player0CellOffset = 0x10;
    private const int DifficultyCellStride = 0x80;
    private const string ExPrefix = "ex_";

    public Task<IReadOnlyDictionary<string, GreenStarSet>> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(GreenGameDataPaths.TuningBin, cancellationToken);
    }

    public static async Task<IReadOnlyDictionary<string, GreenStarSet>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        return Parse(bytes, path);
    }

    private static IReadOnlyDictionary<string, GreenStarSet> Parse(byte[] bytes, string path)
    {
        ValidateHeader(bytes, path);

        var baseRecords = new Dictionary<string, TuningCourseStars>(StringComparer.Ordinal);
        var exRecords = new Dictionary<string, byte>(StringComparer.Ordinal);

        for (var index = 0; index < RecordCount; index++)
        {
            var recordOffset = HeaderSize + index * RecordSize;
            var musicIdOffset = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(recordOffset, 4));
            var musicId = ReadMusicId(bytes, musicIdOffset, path, index);
            if (musicId.StartsWith(ExPrefix, StringComparison.Ordinal))
            {
                exRecords[musicId[ExPrefix.Length..]] = ReadStar(bytes, recordOffset, 3, path, index);
            }
            else
            {
                var stars = new TuningCourseStars(
                    ReadStar(bytes, recordOffset, 0, path, index),
                    ReadStar(bytes, recordOffset, 1, path, index),
                    ReadStar(bytes, recordOffset, 2, path, index),
                    ReadStar(bytes, recordOffset, 3, path, index));

                baseRecords[musicId] = stars;
            }
        }

        var result = new Dictionary<string, GreenStarSet>(baseRecords.Count, StringComparer.Ordinal);
        foreach (var pair in baseRecords)
        {
            var baseStars = pair.Value;
            result[pair.Key] = new GreenStarSet(
                baseStars.Easy,
                baseStars.Normal,
                baseStars.Hard,
                baseStars.Oni,
                exRecords.TryGetValue(pair.Key, out var uraStar) ? uraStar : (byte)0);
        }

        return result;
    }

    private static void ValidateHeader(byte[] bytes, string path)
    {
        var songCount = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(0, 4));
        if (songCount != ExpectedSongCount)
        {
            throw new InvalidDataException(
                $"Invalid Green tuning.bin song_count at 0x0000 in {path}: expected 0x{ExpectedSongCount:X8} ({ExpectedSongCount}), actual 0x{songCount:X8} ({songCount}).");
        }
    }

    private static string ReadMusicId(byte[] bytes, uint musicIdOffset, string path, int recordIndex)
    {
        var stringTableLength = bytes.Length - StringTableOffset;
        if (musicIdOffset >= (uint)stringTableLength)
        {
            throw new InvalidDataException(
                $"Invalid Green tuning.bin musicid offset in record {recordIndex} of {path}: string table offset 0x{musicIdOffset:X8} is outside the {stringTableLength}-byte string table.");
        }

        var absoluteOffset = StringTableOffset + (int)musicIdOffset;
        var end = Array.IndexOf(bytes, (byte)0, absoluteOffset);
        if (end < 0)
        {
            throw new InvalidDataException(
                $"Invalid Green tuning.bin musicid in record {recordIndex} of {path}: missing null terminator after file offset 0x{absoluteOffset:X8}.");
        }

        return Encoding.ASCII.GetString(bytes, absoluteOffset, end - absoluteOffset);
    }

    private static byte ReadStar(byte[] bytes, int recordOffset, int difficultyIndex, string path, int recordIndex)
    {
        var offset = recordOffset + Player0CellOffset + difficultyIndex * DifficultyCellStride;
        var value = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));
        if (value > byte.MaxValue)
        {
            throw new InvalidDataException(
                $"Invalid Green tuning.bin star value in record {recordIndex}, difficulty {difficultyIndex}, offset 0x{offset:X8} of {path}: {value} does not fit in a byte.");
        }

        return (byte)value;
    }

    private readonly record struct TuningCourseStars(
        byte Easy,
        byte Normal,
        byte Hard,
        byte Oni);
}
