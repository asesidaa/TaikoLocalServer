namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CardsController : BaseController<CardsController>
{
    private readonly TaikoDbContext context;

    public CardsController(TaikoDbContext context)
    {
        this.context = context;
    }

    [HttpDelete("{accessCode}")]
    public async Task<IActionResult> DeleteUser(string accessCode)
    {
        var card = await context.Cards.FindAsync(accessCode);

        if (card is null)
        {
            return NotFound();
        }

        context.Cards.Remove(card);
        await context.SaveChangesAsync();

        return NoContent();
    }
    
}