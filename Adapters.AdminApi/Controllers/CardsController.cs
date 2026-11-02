
namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadProfiles, ApiScopes.AdminUsers)]
public class CardsController(ITaikoDbContext context, IServerAccess access) : BaseAdminController<CardsController>
{
    /// <summary>The card's QR code for a cabinet's reader (BNTTCNID + access code), as a PNG data: URI.</summary>
    [HttpGet("{accessCode}/Qr")]
    public async Task<IActionResult> Qr(string accessCode)
    {
        var card = await context.Cards.FindAsync([accessCode], HttpContext.RequestAborted);
        if (card is null) return NotFound();
        if (this.AuthorizeOwnerOrAdmin(card.Baid) is { } forbid)
            return forbid;
        return Ok(new { qrCode = QrImage.DataUri("BNTTCNID" + card.AccessCode, QRCoder.QRCodeGenerator.ECCLevel.Q, 10) });
    }

    /// <summary>A player's cards for Settings › Your cards: code, name and when a cabinet last read it.</summary>
    [HttpGet("Of/{baid}")]
    public async Task<ActionResult<List<CardItem>>> Of(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        return (await context.Cards.Where(c => c.Baid == baid).ToListAsync(HttpContext.RequestAborted))
            .Select(c => new CardItem(c.AccessCode, c.Name, c.LastScanAt is { } t ? DateTime.SpecifyKind(t, DateTimeKind.Utc) : null)).ToList();
    }

    [HttpPut("{accessCode}/Name")]
    public async Task<IActionResult> Rename(string accessCode, RenameCardRequest request)
    {
        var card = await context.Cards.FindAsync([accessCode], HttpContext.RequestAborted);
        if (card is null) return NotFound();
        if (this.AuthorizeOwnerOrAdmin(card.Baid) is { } forbid)
            return forbid;
        var name = request.Name.Trim();
        if (name.Length > 20) return BadRequest(new { message = "At most 20 characters." });
        card.Name = name;
        this.LogIfSomeoneElse(context, "card.rename", card.Baid, ActivityExtensions.MaskAccessCode(card.AccessCode));
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpDelete("{accessCode}")]
    public async Task<IActionResult> DeleteAccessCode(string accessCode)
    {
        var card = await context.Cards.FindAsync(accessCode);
        if (card == null)
        {
            return NotFound();
        }

        if (this.AuthorizeOwnerOrAdmin(card.Baid) is { } forbid)
            return forbid;
        // A player can't remove their last card: they couldn't play as themselves any more.
        if (access.LoginRequired && !User.IsAdmin() && await context.Cards.CountAsync(c => c.Baid == card.Baid, HttpContext.RequestAborted) <= 1)
            return BadRequest(new { message = "Last Card" });

        context.Cards.Remove(card);
        this.LogIfSomeoneElse(context, "card.remove", card.Baid, ActivityExtensions.MaskAccessCode(card.AccessCode));
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    /// <summary>
    /// Move an access code to another user. Scores already played stay with the old user. Not the user's last code
    /// (that would orphan them; merging users is the tool for that), and not past the target's access-code limit.
    /// </summary>
    [HttpPost("{accessCode}/Move")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> MoveAccessCode(string accessCode, MoveAccessCodeRequest request)
    {
        var card = await context.Cards.FindAsync(accessCode);
        if (card is null)
            return NotFound();
        if (card.Baid == request.ToBaid)
            return NoContent();
        if (!await context.UserData.AnyAsync(u => u.Baid == request.ToBaid, HttpContext.RequestAborted))
            return NotFound(new { message = "Target user not found" });
        if (await context.Cards.CountAsync(c => c.Baid == card.Baid, HttpContext.RequestAborted) <= 1)
            return Conflict(new { message = "Last Access Code" });
        if (await context.Cards.CountAsync(c => c.Baid == request.ToBaid, HttpContext.RequestAborted) >= access.Rules.AccessCodeLimit)
            return Conflict(new { message = "Access Code Limit Reached" });

        var from = card.Baid;
        card.Baid = request.ToBaid;
        this.LogActivity(context, "card.move", request.ToBaid, $"{ActivityExtensions.MaskAccessCode(accessCode)} · from Baid {from}");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPost("BindAccessCode")]
    public async Task<IActionResult> BindAccessCode(BindAccessCodeRequest bindAccessCodeRequest)
    {
        if (this.AuthorizeOwnerOrAdmin(bindAccessCodeRequest.Baid) is { } forbid)
            return forbid;

        var accessCode = bindAccessCodeRequest.AccessCode;
        var baid = bindAccessCodeRequest.Baid;

        if (access.LoginRequired && !User.IsAdmin())
        {
            var existingCount = await context.Cards.CountAsync(c => c.Baid == baid, HttpContext.RequestAborted);
            if (existingCount >= access.Rules.AccessCodeLimit)
                return Conflict(new { message = "Access Code Limit Reached" });
        }

        if (accessCode.Length != 20 || !accessCode.All(char.IsAsciiDigit))
            return BadRequest(new { message = "Not 20 Digits" });
        var existingCard = await context.Cards.FindAsync(accessCode);
        if (existingCard is not null)
            return Conflict(new { message = existingCard.Baid == baid ? "Already Yours" : "Another Player" });
        var ban = await context.BannedCards.FindAsync([accessCode], HttpContext.RequestAborted);
        if (ban is not null && (ban.Until is null || ban.Until > DateTime.UtcNow))
            return Conflict(new { message = "Card Banned" });

        var newCard = new Card
        {
            Baid = baid,
            AccessCode = accessCode
        };
        context.Cards.Add(newCard);
        this.LogIfSomeoneElse(context, "card.bind", baid, ActivityExtensions.MaskAccessCode(accessCode));
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
