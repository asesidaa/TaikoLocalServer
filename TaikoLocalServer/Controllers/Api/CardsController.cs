using GameDatabase.Entities;
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
    public async Task<IActionResult> BindAccessCode(BindAccessCodeRequest request)
    {
        var accessCode = request.AccessCode;
        var baid = request.Baid;
        var existingCard = await cardService.GetCardByAccessCode(accessCode);
        if (existingCard is not null)
        {
            return BadRequest("Access code already exists");
        }
        var newCard = new Card
        {
          Baid  = baid,
          AccessCode = accessCode
        };
        await cardService.AddCard(newCard);
        return NoContent();
    }
}