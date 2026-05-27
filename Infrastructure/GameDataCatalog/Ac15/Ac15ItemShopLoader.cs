using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

public static class Ac15ItemShopLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static async Task<Ac15ItemShopCatalog> LoadFromFileAsync(
        string path,
        bool isEnabled,
        uint? activeSeasonId,
        string eraName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!isEnabled)
        {
            return Ac15ItemShopCatalog.Disabled;
        }

        if (!File.Exists(path))
        {
            throw new InvalidDataException($"{eraName} item shop is enabled but data file was not found: {path}");
        }

        if (activeSeasonId is not { } configuredActiveSeasonId)
        {
            throw new InvalidDataException($"{eraName} item shop is enabled but ActiveShopSeasonId is not configured.");
        }

        await using var stream = File.OpenRead(path);
        var raw = await JsonSerializer.DeserializeAsync<RawShopData>(stream, JsonOptions, cancellationToken)
                  ?? new RawShopData();

        var seasons = (raw.Seasons ?? [])
            .Select(season => MapSeason(season, eraName))
            .ToList();

        if (seasons.Count == 0)
        {
            throw new InvalidDataException($"{eraName} item shop data must contain at least one season when enabled.");
        }

        var duplicates = seasons
            .GroupBy(season => season.SeasonId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicates.Length > 0)
        {
            throw new InvalidDataException($"{eraName} item shop data contains duplicate season_id values: {string.Join(", ", duplicates)}");
        }

        var bySeason = seasons.ToDictionary(season => season.SeasonId);
        if (!bySeason.ContainsKey(configuredActiveSeasonId))
        {
            throw new InvalidDataException($"{eraName} item shop active season {configuredActiveSeasonId} was not found.");
        }

        return new Ac15ItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = configuredActiveSeasonId,
            Seasons = bySeason
        };
    }

    private static Ac15ItemShopSeason MapSeason(RawSeason raw, string eraName)
    {
        if (raw.SeasonId == 0)
        {
            throw new InvalidDataException($"{eraName} item shop season_id must be nonzero.");
        }

        if (!IsDate(raw.StartDatetime) || !IsDate(raw.EndDatetime))
        {
            throw new InvalidDataException($"{eraName} item shop season {raw.SeasonId} has invalid datetime fields.");
        }

        var rawItems = raw.Items ?? [];
        if (rawItems.Length == 0)
        {
            throw new InvalidDataException($"{eraName} item shop season {raw.SeasonId} must contain at least one item.");
        }

        if (rawItems.Length > 64)
        {
            throw new InvalidDataException($"{eraName} item shop season {raw.SeasonId} has {rawItems.Length} items; the client supports at most 64.");
        }

        var items = rawItems
            .Select((item, index) => MapItem(raw.SeasonId, item, (uint)index + 1, eraName))
            .ToArray();

        var duplicateItems = items
            .GroupBy(item => new { item.ItemType, item.ItemId })
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key.ItemType}:{group.Key.ItemId}")
            .ToArray();
        if (duplicateItems.Length > 0)
        {
            throw new InvalidDataException($"{eraName} item shop season {raw.SeasonId} contains duplicate item identities: {string.Join(", ", duplicateItems)}");
        }

        return new Ac15ItemShopSeason
        {
            SeasonId = raw.SeasonId,
            VerupNo = raw.VerupNo,
            Telop = raw.Telop ?? string.Empty,
            StartDatetime = raw.StartDatetime ?? string.Empty,
            EndDatetime = raw.EndDatetime ?? string.Empty,
            AfterstartDays = raw.AfterstartDays,
            BeforecloseDays = raw.BeforecloseDays,
            Items = items
        };
    }

    private static Ac15ItemShopEntry MapItem(uint seasonId, RawItem raw, uint itemNo, string eraName)
    {
        if (raw.ItemType is < 1 or > 7)
        {
            throw new InvalidDataException($"{eraName} item shop season {seasonId} item {itemNo} has unsupported item_type {raw.ItemType}.");
        }

        if (raw.ItemId == 0)
        {
            throw new InvalidDataException($"{eraName} item shop season {seasonId} item {itemNo} has item_id 0.");
        }

        if (raw.ItemPrice == 0)
        {
            throw new InvalidDataException($"{eraName} item shop season {seasonId} item {itemNo} has item_price 0.");
        }

        return new Ac15ItemShopEntry
        {
            ItemNo = itemNo,
            ItemType = raw.ItemType,
            ItemId = raw.ItemId,
            Price = raw.ItemPrice
        };
    }

    private static bool IsDate(string? value)
        => value is { Length: 14 } && value.All(char.IsAsciiDigit);

    private sealed class RawShopData
    {
        [JsonPropertyName("seasons")]
        public RawSeason[]? Seasons { get; set; }
    }

    private sealed class RawSeason
    {
        [JsonPropertyName("season_id")]
        public uint SeasonId { get; set; }

        [JsonPropertyName("verup_no")]
        public uint VerupNo { get; set; }

        [JsonPropertyName("telop")]
        public string? Telop { get; set; }

        [JsonPropertyName("start_datetime")]
        public string? StartDatetime { get; set; }

        [JsonPropertyName("end_datetime")]
        public string? EndDatetime { get; set; }

        [JsonPropertyName("afterstart_days")]
        public uint AfterstartDays { get; set; }

        [JsonPropertyName("beforeclose_days")]
        public uint BeforecloseDays { get; set; }

        [JsonPropertyName("items")]
        public RawItem[]? Items { get; set; }
    }

    private sealed class RawItem
    {
        [JsonPropertyName("item_type")]
        public uint ItemType { get; set; }

        [JsonPropertyName("item_id")]
        public uint ItemId { get; set; }

        [JsonPropertyName("item_price")]
        public uint ItemPrice { get; set; }
    }
}
