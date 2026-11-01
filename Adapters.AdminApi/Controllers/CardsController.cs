
namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController(ITaikoDbContext context, IServerAccess access) : BaseAdminController<CardsController>
{

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

        context.Cards.Remove(card);
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

        var existingCard = await context.Cards.FindAsync(accessCode);
        if (existingCard is not null)
        {
            return BadRequest("Access code already exists");
        }

        var newCard = new Card
        {
            Baid = baid,
            AccessCode = accessCode
        };
        context.Cards.Add(newCard);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
