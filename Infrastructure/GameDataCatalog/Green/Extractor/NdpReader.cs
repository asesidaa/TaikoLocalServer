using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

public sealed record NdpEntry(uint Id, string FileName, uint Offset, uint Size);

public static partial class NdpReader
{
    private const int EntryCountOffset = 0x40;
    private const int EntryTableOffset = 0x50;
    private const int MountedEntryTableOffset = 0x4E;

    public static IReadOnlyList<NdpEntry> Read(byte[] bytes)
    {
        if (bytes.Length < EntryTableOffset)
        {
            throw new InvalidDataException("NUT_PACK_TYPE1 file is shorter than the fixed header.");
        }

        var magic = Encoding.ASCII.GetString(bytes.AsSpan(0, "NUT_PACK_TYPE1".Length));
        if (magic != "NUT_PACK_TYPE1")
        {
            throw new InvalidDataException($"Unexpected NDP magic '{magic}'.");
        }

        return LooksLikeFixedTableFormat(bytes)
            ? ReadFixedTable(bytes)
            : ReadMountedStringTable(bytes);
    }

    public static IReadOnlyList<NdpEntry> ReadFile(string path)
        => Read(File.ReadAllBytes(path));

    private static IReadOnlyList<NdpEntry> ReadFixedTable(byte[] bytes)
    {
        var count = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(EntryCountOffset, 4));
        var cursor = EntryTableOffset;
        var entries = new List<NdpEntry>(checked((int)count));

        for (var index = 0; index < count; index++)
        {
            EnsureAvailable(bytes, cursor, 4);
            var nameLength = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(cursor, 4));
            cursor += 4;

            EnsureAvailable(bytes, cursor, checked((int)nameLength));
            var rawName = bytes.AsSpan(cursor, checked((int)nameLength));
            var nul = rawName.IndexOf((byte)0);
            var fileName = Encoding.ASCII.GetString(nul >= 0 ? rawName[..nul] : rawName);
            if (!LooksLikeNutFileName(Encoding.ASCII.GetBytes(fileName)))
            {
                throw new InvalidDataException($"NDP fixed entry has malformed filename '{fileName}'.");
            }

            cursor += checked((int)nameLength);
            cursor = Align4(cursor);

            EnsureAvailable(bytes, cursor, 8);
            var offset = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(cursor, 4));
            var size = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(cursor + 4, 4));
            cursor += 8;

            entries.Add(new NdpEntry(ParseId(fileName), fileName, offset, size));
        }

        return entries.OrderBy(entry => entry.Id).ThenBy(entry => entry.FileName, StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<NdpEntry> ReadMountedStringTable(byte[] bytes)
    {
        var count = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(EntryCountOffset, 2));
        var cursor = MountedEntryTableOffset;
        var entries = new List<NdpEntry>(count);

        while (entries.Count < count)
        {
            if (!TryFindMountedEntry(bytes, cursor, out var entryStart))
            {
                throw new InvalidDataException(
                    $"NDP compact entry table ended after {entries.Count} entries, but header declares {count}.");
            }

            var nameLength = bytes[entryStart];
            var nameStart = entryStart + 1;
            var fileName = Encoding.ASCII.GetString(bytes.AsSpan(nameStart, nameLength));
            var payloadStart = nameStart + nameLength + 1;
            EnsureAvailable(bytes, payloadStart, 8);
            var offset = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(payloadStart, 4));
            var size = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(payloadStart + 4, 4));

            entries.Add(new NdpEntry(ParseId(fileName), fileName, offset, size));
            cursor = payloadStart;
        }

        return entries.OrderBy(entry => entry.Id).ThenBy(entry => entry.FileName, StringComparer.Ordinal).ToArray();
    }

    private static bool LooksLikeFixedTableFormat(byte[] bytes)
    {
        var fixedCountLowWord = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(EntryCountOffset + 2, 2));
        if (fixedCountLowWord > 0)
        {
            return true;
        }

        var nameLength = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(EntryTableOffset, 4));
        if (nameLength == 0)
        {
            return false;
        }

        if (nameLength > bytes.Length - EntryTableOffset - 4)
        {
            return false;
        }

        var rawName = bytes.AsSpan(EntryTableOffset + 4, checked((int)nameLength));
        var nul = rawName.IndexOf((byte)0);
        return LooksLikeNutFileName(nul >= 0 ? rawName[..nul] : rawName);
    }

    private static bool TryFindMountedEntry(byte[] bytes, int start, out int entryStart)
    {
        for (var cursor = start; cursor < bytes.Length; cursor++)
        {
            var nameLength = bytes[cursor];
            var nameStart = cursor + 1;
            if (nameLength == 0 || nameStart + nameLength >= bytes.Length)
            {
                continue;
            }

            if (bytes[nameStart + nameLength] == 0
                && LooksLikeNutFileName(bytes.AsSpan(nameStart, nameLength)))
            {
                entryStart = cursor;
                return true;
            }
        }

        entryStart = -1;
        return false;
    }

    private static bool LooksLikeNutFileName(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < ".nut".Length)
        {
            return false;
        }

        for (var index = 0; index < bytes.Length; index++)
        {
            var value = bytes[index];
            if (value < 0x20 || value > 0x7E)
            {
                return false;
            }
        }

        return bytes.EndsWith(".nut"u8);
    }

    private static void EnsureAvailable(byte[] bytes, int offset, int length)
    {
        if (offset < 0 || length < 0 || offset + length > bytes.Length)
        {
            throw new InvalidDataException($"NDP entry table read exceeds file length at 0x{offset:X}.");
        }
    }

    private static int Align4(int value) => (value + 3) & ~3;

    private static uint ParseId(string fileName)
    {
        var match = LastNumberRegex().Matches(fileName).LastOrDefault();
        return match is null ? 0 : uint.Parse(match.Value);
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex LastNumberRegex();
}
