using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueRewardShopDataParser
{
    private const string BoostSignature = "serialization::archive";
    private const int SignatureLengthOffset = 0x00;
    private const int SignatureOffset = 0x04;
    private const int SignatureTerminatorOffset = 0x1A;
    private const int ArchiveHeaderOffset = 0x1B;
    private const int SeasonIdOffset = 0x20;
    private const int VerupNoBcdOffset = 0x29;
    private const int StartDatetimeOffset = 0x3A;
    private const int EndDatetimeOffset = 0x42;
    private const int RepeatStartDatetimeOffset = 0x4A;
    private const int AfterstartDatetimeOffset = 0x52;
    private const int BeforecloseDatetimeOffset = 0x5A;
    private const int RepeatEndDatetimeOffset = 0x62;
    private const int ItemCountOffset = 0x6A;
    private const int ItemPaddingOffset = 0x6E;
    private const int ItemOffset = 0x73;
    private const int ItemSize = 0x10;
    private const long BoostPtimeToDateTimeTickScale = 10;

    private static readonly byte[] ExpectedArchiveHeader = [0x0A, 0x04, 0x04, 0x04, 0x08];

    public async Task<BlueRewardShopData> ParseFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var bytes = new byte[stream.Length];
        await stream.ReadExactlyAsync(bytes, cancellationToken);

        return Parse(bytes);
    }

    public BlueRewardShopData Parse(ReadOnlySpan<byte> source)
    {
        ValidateHeader(source);

        var seasonId = ReadUInt32(source, SeasonIdOffset);
        if (seasonId == 0)
        {
            throw new InvalidDataException("Blue reward shop season_id must be nonzero.");
        }

        var start = ReadBoostPtime(source, StartDatetimeOffset);
        var end = ReadBoostPtime(source, EndDatetimeOffset);
        var repeatStart = ReadBoostPtime(source, RepeatStartDatetimeOffset);
        var afterstart = ReadBoostPtime(source, AfterstartDatetimeOffset);
        var beforeclose = ReadBoostPtime(source, BeforecloseDatetimeOffset);
        var repeatEnd = ReadBoostPtime(source, RepeatEndDatetimeOffset);

        if (start != repeatStart || end != repeatEnd)
        {
            throw new InvalidDataException("Blue reward shop season datetime envelope is inconsistent.");
        }

        if (afterstart < start || beforeclose > end || end <= start)
        {
            throw new InvalidDataException("Blue reward shop season datetime envelope is out of order.");
        }

        var itemCount = ReadUInt32(source, ItemCountOffset);
        if (itemCount == 0)
        {
            throw new InvalidDataException("Blue reward shop must contain at least one item.");
        }

        if (!source.Slice(ItemPaddingOffset, ItemOffset - ItemPaddingOffset).ToArray().All(value => value == 0))
        {
            throw new InvalidDataException("Blue reward shop item table padding has an unexpected value.");
        }

        var expectedLength = checked(ItemOffset + (int)itemCount * ItemSize);
        if (source.Length != expectedLength)
        {
            throw new InvalidDataException($"Blue reward shop item table length is invalid. Expected {expectedLength} bytes but found {source.Length}.");
        }

        var items = new List<BlueRewardShopItem>((int)itemCount);
        for (uint index = 0; index < itemCount; index++)
        {
            items.Add(ReadItem(source, index, seasonId));
        }

        var duplicateItems = items
            .GroupBy(item => new { item.ItemType, item.ItemId })
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key.ItemType}:{group.Key.ItemId}")
            .ToArray();
        if (duplicateItems.Length > 0)
        {
            throw new InvalidDataException($"Blue reward shop season {seasonId} contains duplicate item identities: {string.Join(", ", duplicateItems)}");
        }

        var afterstartDays = checked((uint)(afterstart - start).TotalDays);
        var beforecloseDays = checked((uint)(end - beforeclose).TotalDays);

        return new BlueRewardShopData(
            [
                new BlueRewardShopSeason(
                    seasonId,
                    ReadBcdUInt32(source, VerupNoBcdOffset, 4),
                    string.Empty,
                    FormatDateTime(start),
                    FormatDateTime(end),
                    afterstartDays,
                    beforecloseDays,
                    items)
            ]);
    }

    private static void ValidateHeader(ReadOnlySpan<byte> source)
    {
        if (source.Length < ItemOffset)
        {
            throw new InvalidDataException("Blue reward shop data is too short.");
        }

        var signatureLength = ReadUInt32(source, SignatureLengthOffset);
        if (signatureLength != BoostSignature.Length)
        {
            throw new InvalidDataException("Blue reward shop data has an invalid Boost signature length.");
        }

        var signature = Encoding.ASCII.GetString(source.Slice(SignatureOffset, BoostSignature.Length));
        if (!string.Equals(signature, BoostSignature, StringComparison.Ordinal))
        {
            throw new InvalidDataException("Blue reward shop data does not contain a Boost serialization marker.");
        }

        if (source[SignatureTerminatorOffset] != 0)
        {
            throw new InvalidDataException("Blue reward shop Boost signature is not null-terminated.");
        }

        if (!source.Slice(ArchiveHeaderOffset, ExpectedArchiveHeader.Length).SequenceEqual(ExpectedArchiveHeader))
        {
            throw new InvalidDataException("Blue reward shop Boost archive header is unsupported.");
        }
    }

    private static BlueRewardShopItem ReadItem(ReadOnlySpan<byte> source, uint index, uint seasonId)
    {
        var offset = checked(ItemOffset + (int)index * ItemSize);
        var itemNo = ReadUInt32(source, offset);
        var catalogMarker = ReadUInt32(source, offset + 4);
        var reserved = ReadUInt16(source, offset + 8);
        var itemType = source[offset + 10];
        var itemId = source[offset + 11];
        var price = ReadUInt32(source, offset + 12);

        var expectedItemNo = index + 1;
        if (itemNo != expectedItemNo)
        {
            throw new InvalidDataException($"Blue reward shop season {seasonId} item row {expectedItemNo} has item_no {itemNo}.");
        }

        if (catalogMarker != 1)
        {
            throw new InvalidDataException($"Blue reward shop season {seasonId} item {itemNo} has unsupported catalog marker {catalogMarker}.");
        }

        if (reserved != 0)
        {
            throw new InvalidDataException($"Blue reward shop season {seasonId} item {itemNo} has unsupported row marker {reserved}.");
        }

        if (itemType is < 1 or > 7)
        {
            throw new InvalidDataException($"Blue reward shop season {seasonId} item {itemNo} has unsupported item_type {itemType}.");
        }

        if (itemId == 0)
        {
            throw new InvalidDataException($"Blue reward shop season {seasonId} item {itemNo} has item_id 0.");
        }

        if (price == 0)
        {
            throw new InvalidDataException($"Blue reward shop season {seasonId} item {itemNo} has item_price 0.");
        }

        return new BlueRewardShopItem(
            itemNo,
            itemType,
            itemId,
            price,
            ResolveCatalogDomain(itemType));
    }

    private static BlueRewardShopCatalogDomain ResolveCatalogDomain(uint itemType)
        => itemType switch
        {
            1 => BlueRewardShopCatalogDomain.Song,
            2 => BlueRewardShopCatalogDomain.Tone,
            3 => BlueRewardShopCatalogDomain.Kigurumi,
            4 => BlueRewardShopCatalogDomain.Body,
            5 => BlueRewardShopCatalogDomain.Head,
            6 => BlueRewardShopCatalogDomain.Face,
            7 => BlueRewardShopCatalogDomain.Puchi,
            _ => throw new InvalidDataException($"Blue reward shop item_type {itemType} is unsupported.")
        };

    private static DateTime ReadBoostPtime(ReadOnlySpan<byte> source, int offset)
    {
        var raw = BinaryPrimitives.ReadUInt64BigEndian(source.Slice(offset, sizeof(ulong)));
        if (raw > long.MaxValue / BoostPtimeToDateTimeTickScale)
        {
            throw new InvalidDataException("Blue reward shop datetime value is out of range.");
        }

        return new DateTime(checked((long)raw * BoostPtimeToDateTimeTickScale), DateTimeKind.Unspecified);
    }

    private static string FormatDateTime(DateTime value)
        => value.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

    private static uint ReadBcdUInt32(ReadOnlySpan<byte> source, int offset, int byteCount)
    {
        uint value = 0;
        for (var index = 0; index < byteCount; index++)
        {
            var current = source[offset + index];
            var high = current >> 4;
            var low = current & 0x0F;
            if (high > 9 || low > 9)
            {
                throw new InvalidDataException("Blue reward shop BCD field contains a non-decimal digit.");
            }

            value = value * 100 + (uint)(high * 10 + low);
        }

        return value;
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> source, int offset)
        => BinaryPrimitives.ReadUInt32BigEndian(source.Slice(offset, sizeof(uint)));

    private static ushort ReadUInt16(ReadOnlySpan<byte> source, int offset)
        => BinaryPrimitives.ReadUInt16BigEndian(source.Slice(offset, sizeof(ushort)));

    public sealed record BlueRewardShopData(IReadOnlyList<BlueRewardShopSeason> Seasons);

    public sealed record BlueRewardShopSeason(
        uint SeasonId,
        uint VerupNo,
        string Telop,
        string StartDatetime,
        string EndDatetime,
        uint AfterstartDays,
        uint BeforecloseDays,
        IReadOnlyList<BlueRewardShopItem> Items);

    public sealed record BlueRewardShopItem(
        uint ItemNo,
        uint ItemType,
        uint ItemId,
        uint Price,
        BlueRewardShopCatalogDomain CatalogDomain);

    public enum BlueRewardShopCatalogDomain
    {
        Song = 1,
        Tone = 2,
        Kigurumi = 3,
        Body = 4,
        Head = 5,
        Face = 6,
        Puchi = 7
    }
}
