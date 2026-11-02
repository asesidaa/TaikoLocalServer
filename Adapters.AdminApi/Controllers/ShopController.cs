using System.Globalization;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// Green, Blue and Yellow's item shop: a player's seasons, Don medals and purchases (Play Data › Item shop), the
/// purchase counts per season and the live season (Server Management › Game data › Item shop).
/// </summary>
[ApiController]
[Route("api/{era}/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminServer)]
public class ShopController(ITaikoDbContext context, IGameDataCatalog catalog, IShopLiveSeasons liveSeasons)
    : BaseAdminController<ShopController>
{
    [HttpGet("{baid}")]
    public async Task<ActionResult<ShopResponse>> Get(string era, uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        if (!TryShop(era, out var gameEra, out var shop, out var refused))
            return refused;

        var (seasonRows, itemRows) = gameEra switch
        {
            GameEra.Green => await Rows(context.GreenShopSeasonStates, context.GreenShopItemStates, baid),
            GameEra.Blue => await Rows(context.BlueShopSeasonStates, context.BlueShopItemStates, baid),
            _ => await Rows(context.YellowShopSeasonStates, context.YellowShopItemStates, baid),
        };
        var medals = seasonRows.ToDictionary(s => s.SeasonId);
        var bought = itemRows.ToLookup(i => i.SeasonId);
        // Every season in the file, then any the player bought from that the file no longer has.
        var ids = shop.Seasons.Keys.Concat(medals.Keys).Concat(bought.Select(g => g.Key)).Distinct().Order();
        var seasons = ids.Select(id =>
        {
            shop.Seasons.TryGetValue(id, out var s);
            var mine = bought[id].ToList();
            var sold = (s?.Items ?? []).Select(item => new ShopItem(TypeName((uint)item.ItemType), item.ItemId, item.Price,
                mine.FirstOrDefault(b => b.ItemType == (uint)item.ItemType && b.ItemId == item.ItemId)?.PurchasedAt));
            var gone = mine.Where(b => s?.Items.Any(item => (uint)item.ItemType == b.ItemType && item.ItemId == b.ItemId) != true)
                .Select(b => new ShopItem(TypeName(b.ItemType), b.ItemId, b.ItemPrice, b.PurchasedAt));
            medals.TryGetValue(id, out var m);
            return new ShopSeason(id, s?.Telop ?? "", Date(s?.StartDatetime), Date(s?.EndDatetime),
                m?.TotalGetDonmedal ?? 0, m?.TotalUseDonmedal ?? 0, [.. sold, .. gone]);
        }).ToList();
        return new ShopResponse(gameEra.ToString(), shop.IsEnabled, shop.ActiveSeasonId, seasons);
    }

    /// <summary>How many players bought from each season, and how many items (the Item shop editor).</summary>
    [HttpGet("Sales")]
    [ApiScope(ApiScopes.AdminServer, ApiScopes.AdminServer)]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<ActionResult<IReadOnlyList<ShopSeasonSales>>> Sales(string era)
    {
        if (!TryShop(era, out var gameEra, out _, out var refused))
            return refused;
        var rows = gameEra switch
        {
            GameEra.Green => await Sales(context.GreenShopItemStates),
            GameEra.Blue => await Sales(context.BlueShopItemStates),
            _ => await Sales(context.YellowShopItemStates),
        };
        return Ok(rows);
    }

    /// <summary>Makes a season the one the game's cabinets show; they pick it up on their next start-up.</summary>
    [HttpPost("LiveSeason/{seasonId}")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> SetLive(string era, uint seasonId)
    {
        if (!TryShop(era, out var gameEra, out var shop, out var refused))
            return refused;
        if (!shop.Seasons.ContainsKey(seasonId))
            return BadRequest(new { message = $"{gameEra}'s item shop has no season {seasonId}." });

        await liveSeasons.SetAsync(gameEra, seasonId, HttpContext.RequestAborted);
        await catalog.ReloadAsync(gameEra, HttpContext.RequestAborted);
        this.LogActivity(context, "gamedata.shopseason", null, $"{gameEra}: season {seasonId}");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private bool TryShop(string era, out GameEra gameEra, out Application.Catalog.Ac15.Ac15ItemShopCatalog shop, out ActionResult refused)
    {
        shop = Application.Catalog.Ac15.Ac15ItemShopCatalog.Disabled;
        refused = null!;
        if (!EraRoute.TryParse(era, out gameEra) || gameEra is not (GameEra.Green or GameEra.Blue or GameEra.Yellow))
        {
            refused = BadRequest(new { message = $"{era} has no item shop." });
            return false;
        }
        if (!catalog.LoadResults.ContainsKey(gameEra))
        {
            refused = BadRequest(new { message = $"{gameEra} is off in ServerSettings.json." });
            return false;
        }
        shop = gameEra switch
        {
            GameEra.Green => catalog.Green().ItemShopCatalog,
            GameEra.Blue => catalog.Blue().ItemShopCatalog,
            _ => catalog.Yellow().ItemShopCatalog,
        };
        return true;
    }

    private async Task<(List<IAc15ShopSeasonState>, List<IAc15ShopItemState>)> Rows<TSeason, TItem>(
        IQueryable<TSeason> seasons, IQueryable<TItem> items, uint baid)
        where TSeason : class, IAc15ShopSeasonState where TItem : class, IAc15ShopItemState
    {
        var s = await seasons.AsNoTracking().Where(x => x.Baid == baid).ToListAsync(HttpContext.RequestAborted);
        var i = await items.AsNoTracking().Where(x => x.Baid == baid).ToListAsync(HttpContext.RequestAborted);
        return (s.Cast<IAc15ShopSeasonState>().ToList(), i.Cast<IAc15ShopItemState>().ToList());
    }

    private async Task<List<ShopSeasonSales>> Sales<TItem>(IQueryable<TItem> items) where TItem : class, IAc15ShopItemState =>
        await items.GroupBy(x => x.SeasonId)
            .Select(g => new ShopSeasonSales(g.Key, g.Select(x => x.Baid).Distinct().Count(), g.Count()))
            .ToListAsync(HttpContext.RequestAborted);

    private static string TypeName(uint type) => Enum.IsDefined((Ac15ShopItemType)type) ? ((Ac15ShopItemType)type).ToString().ToLowerInvariant() : type.ToString();

    private static DateTime? Date(string? value) =>
        DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
}
