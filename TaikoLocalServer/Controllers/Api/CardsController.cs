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
    public async Task<IActionResult> DeleteUser(uint baid)
    {
        var result = await cardService.DeleteCard(baid);

        return result ? NoContent() : NotFound();
    }
}