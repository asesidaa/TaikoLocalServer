using Microsoft.Extensions.Options;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController(ITaikoDbContext context, IOptions<AuthSettings> authOptions) : BaseAdminController<CardsController>
{
    private readonly AuthSettings authSettings = authOptions.Value;

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

        if (authSettings.AuthenticationRequired && !User.IsAdmin())
        {
            var existingCount = await context.Cards.CountAsync(c => c.Baid == baid, HttpContext.RequestAborted);
            if (existingCount >= authSettings.BoundAccessCodeUpperLimit)
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
