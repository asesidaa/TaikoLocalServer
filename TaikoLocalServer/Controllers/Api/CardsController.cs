using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SharedProject.Models.Requests;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CardsController(IAuthService authService, IOptions<AuthSettings> settings) : BaseController<CardsController>
{
    private readonly AuthSettings authSettings = settings.Value;
    
    [HttpDelete("{accessCode}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> DeleteAccessCode(string accessCode)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }
            
            var card = await authService.GetCardByAccessCode(accessCode);
            if (card == null)
            {
                return Unauthorized();
            }

            if (card.Baid != tokenInfo.Value.baid && !tokenInfo.Value.isAdmin)
            {
                return Forbid();
            }
        }
        
        var result = await authService.DeleteCard(accessCode);

        return result ? NoContent() : NotFound();
    }

    [HttpPost("BindAccessCode")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> BindAccessCode(BindAccessCodeRequest bindAccessCodeRequest)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = authService.ExtractTokenInfo(HttpContext);
            if (tokenInfo == null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.isAdmin && tokenInfo.Value.baid != bindAccessCodeRequest.Baid)
            {
                return Forbid();
            }
        }
        
        var accessCode = bindAccessCodeRequest.AccessCode;
        var baid = bindAccessCodeRequest.Baid;
        var existingCard = await authService.GetCardByAccessCode(accessCode);
        if (existingCard is not null)
        {
            return BadRequest("Access code already exists");
        }

        var newCard = new Card
        {
            Baid = baid,
            AccessCode = accessCode
        };
        await authService.AddCard(newCard);
        return NoContent();
    }
}
