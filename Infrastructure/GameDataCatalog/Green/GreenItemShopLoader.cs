using System.Text.Json;
using System.Text.Json.Serialization;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenItemShopLoader
{
    public const string FileName = "green_item_shop_data.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public Task<GreenItemShopCatalog> LoadAsync(
        EraSettings greenSettings,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Green), FileName);
        return LoadFromFileAsync(path, greenSettings, cancellationToken);
    }

    public static async Task<GreenItemShopCatalog> LoadFromFileAsync(
        string path,
        EraSettings greenSettings,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!greenSettings.EnableShop)
        {
            return GreenItemShopCatalog.Disabled;
        }

        if (!File.Exists(path))
        {
            throw new InvalidDataException($"Green item shop is enabled but data file was not found: {path}");
        }

        if (greenSettings.ActiveShopSeasonId is not { } activeSeasonId)
        {
            throw new InvalidDataException("Green item shop is enabled but ActiveShopSeasonId is not configured.");
        }

        await using var stream = File.OpenRead(path);
        var raw = await JsonSerializer.DeserializeAsync<RawShopData>(stream, JsonOptions, cancellationToken)
                  ?? new RawShopData();

        var seasons = (raw.Seasons ?? [])
            .Select(MapSeason)
            .ToList();

        if (seasons.Count == 0)
        {
            throw new InvalidDataException("Green item shop data must contain at least one season when enabled.");
        }

        var duplicates = seasons
            .GroupBy(season => season.SeasonId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicates.Length > 0)
        {
            throw new InvalidDataException($"Green item shop data contains duplicate season_id values: {string.Join(", ", duplicates)}");
        }

        var bySeason = seasons.ToDictionary(season => season.SeasonId);
        if (!bySeason.ContainsKey(activeSeasonId))
        {
            throw new InvalidDataException($"Green item shop active season {activeSeasonId} was not found.");
        }

        return new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = activeSeasonId,
            Seasons = bySeason
        };
    }

    private static GreenItemShopSeason MapSeason(RawSeason raw)
    {
        if (raw.SeasonId == 0)
        {
            throw new InvalidDataException("Green item shop season_id must be nonzero.");
        }

        if (!IsDate(raw.StartDatetime) || !IsDate(raw.EndDatetime))
        {
            throw new InvalidDataException($"Green item shop season {raw.SeasonId} has invalid datetime fields.");
        }

        var rawItems = raw.Items ?? [];
        if (rawItems.Length == 0)
        {
            throw new InvalidDataException($"Green item shop season {raw.SeasonId} must contain at least one item.");
        }

        if (rawItems.Length > 64)
        {
            throw new InvalidDataException($"Green item shop season {raw.SeasonId} has {rawItems.Length} items; the client supports at most 64.");
        }

        var items = rawItems
            .Select((item, index) => MapItem(raw.SeasonId, item, (uint)index + 1))
            .ToArray();

        var duplicateItems = items
            .GroupBy(item => new { item.ItemType, item.ItemId })
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key.ItemType}:{group.Key.ItemId}")
            .ToArray();
        if (duplicateItems.Length > 0)
        {
            throw new InvalidDataException($"Green item shop season {raw.SeasonId} contains duplicate item identities: {string.Join(", ", duplicateItems)}");
        }

        return new GreenItemShopSeason
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

    private static GreenItemShopEntry MapItem(uint seasonId, RawItem raw, uint itemNo)
    {
        if (raw.ItemType is < 1 or > 7)
        {
            throw new InvalidDataException($"Green item shop season {seasonId} item {itemNo} has unsupported item_type {raw.ItemType}.");
        }

        if (raw.ItemId == 0)
        {
            throw new InvalidDataException($"Green item shop season {seasonId} item {itemNo} has item_id 0.");
        }

        if (raw.ItemPrice == 0)
        {
            throw new InvalidDataException($"Green item shop season {seasonId} item {itemNo} has item_price 0.");
        }

        return new GreenItemShopEntry
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
