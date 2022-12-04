using SharedProject.Models.Requests;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CardsController : BaseController<CardsController>
{
    private readonly ICardService cardService;

    public CardsController(ICardService cardService)
    {
        this.cardService = cardService;
    }

    [HttpDelete("{accessCode}")]
    public async Task<IActionResult> DeleteUser(string accessCode)
    {
        var result = await cardService.DeleteCard(accessCode);

        return result ? NoContent() : NotFound();
    }
    
    [HttpPost]
    public async Task<IActionResult> UpdatePassword(SetPasswordRequest request)
    {
        var accessCode = request.AccessCode;
        var password = request.Password;
        var result = await cardService.UpdatePassword(accessCode, password);

        return result ? NoContent() : NotFound();
    }
}