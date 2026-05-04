using Microsoft.Extensions.Options;
using SharedProject.Models.Requests;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController(
    ITaikoDbContext context,
    IJwtTokenService jwtTokens,
    IOptions<AuthSettings> settings) : BaseAdminController<CardsController>
{
    private readonly AuthSettings authSettings = settings.Value;

    [HttpDelete("{accessCode}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> DeleteAccessCode(string accessCode)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            var existingCard = await context.Cards.FindAsync(accessCode);
            if (existingCard == null)
            {
                return Unauthorized();
            }

            if (existingCard.Baid != tokenInfo.Value.Baid && !tokenInfo.Value.IsAdmin)
            {
                return Forbid();
            }
        }

        var card = await context.Cards.FindAsync(accessCode);
        if (card == null)
        {
            return NotFound();
        }

        context.Cards.Remove(card);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPost("BindAccessCode")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> BindAccessCode(BindAccessCodeRequest bindAccessCodeRequest)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.IsAdmin && tokenInfo.Value.Baid != bindAccessCodeRequest.Baid)
            {
                return Forbid();
            }
        }

        var accessCode = bindAccessCodeRequest.AccessCode;
        var baid = bindAccessCodeRequest.Baid;
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
