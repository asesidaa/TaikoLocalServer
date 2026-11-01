
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
