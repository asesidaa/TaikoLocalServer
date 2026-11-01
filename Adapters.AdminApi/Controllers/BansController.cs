namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// Access › Banned cards: cards the server refuses at the cabinet and at Web UI sign-in. The account and scores stay.
/// Admin only; works in Local mode too (cabinets exist without accounts).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
[ApiScope(ApiScopes.AdminUsers, ApiScopes.AdminUsers)]
public class BansController(ITaikoDbContext context) : BaseAdminController<BansController>
{
    [HttpGet]
    public async Task<List<BanItem>> Get()
    {
        var bans = await context.BannedCards.OrderByDescending(b => b.BannedAt).ToListAsync(HttpContext.RequestAborted);
        var codes = bans.Select(b => b.AccessCode).ToList();
        var owners = await context.Cards.Where(c => codes.Contains(c.AccessCode)).ToDictionaryAsync(c => c.AccessCode, c => c.Baid, HttpContext.RequestAborted);
        var ids = owners.Values.Concat(bans.Select(b => b.BannedBy).OfType<uint>()).Distinct().ToList();
        var names = await context.UserData.Where(u => ids.Contains(u.Baid)).ToDictionaryAsync(u => u.Baid, u => u.MyDonName, HttpContext.RequestAborted);
        return bans.Select(b =>
        {
            uint? owner = owners.TryGetValue(b.AccessCode, out var o) ? o : null;
            return new BanItem(b.AccessCode, ActivityExtensions.MaskAccessCode(b.AccessCode), owner, owner is { } x ? names.GetValueOrDefault(x) : null,
                b.Reason, b.BannedBy, b.BannedBy is { } by ? names.GetValueOrDefault(by) : null,
                Utc(b.BannedAt), b.Until is { } u ? Utc(u) : null, b.RefusedCount);
        }).ToList();
    }

    /// <summary>Bans (or re-bans, replacing the reason and length) a card. It needn't be on this server yet.</summary>
    [HttpPost]
    public async Task<IActionResult> Ban(BanRequest request)
    {
        var code = new string(request.AccessCode.Where(char.IsAsciiDigit).ToArray());
        if (code.Length != 20) return BadRequest(new { message = "Access codes are 20 digits." });
        if (request.Days is <= 0) return BadRequest(new { message = "A ban lasts at least a day." });

        var now = DateTime.UtcNow;
        var ban = await context.BannedCards.FindAsync([code], HttpContext.RequestAborted);
        if (ban is null) context.BannedCards.Add(ban = new BannedCard { AccessCode = code });
        ban.Reason = request.Reason.Trim() is { Length: > 200 } long_ ? long_[..200] : request.Reason.Trim();
        ban.BannedBy = User.GetBaid();
        ban.BannedAt = now;
        ban.Until = request.Days is { } days ? now.AddDays(days) : null;

        var owner = await context.Cards.FindAsync([code], HttpContext.RequestAborted);
        this.LogActivity(context, "card.ban", owner?.Baid, $"{ActivityExtensions.MaskAccessCode(code)} · {(request.Days is { } d ? $"{d} days" : "permanently")}");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    /// <summary>Unbans, or removes an expired ban from the list.</summary>
    [HttpDelete("{accessCode}")]
    public async Task<IActionResult> Unban(string accessCode)
    {
        var ban = await context.BannedCards.FindAsync([accessCode], HttpContext.RequestAborted);
        if (ban is null) return NotFound();
        context.BannedCards.Remove(ban);
        var owner = await context.Cards.FindAsync([accessCode], HttpContext.RequestAborted);
        this.LogActivity(context, "card.unban", owner?.Baid, ActivityExtensions.MaskAccessCode(accessCode));
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static DateTime Utc(DateTime t) => DateTime.SpecifyKind(t, DateTimeKind.Utc);
}
