namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController<UsersController>
{
    private readonly IUserDatumService userDatumService;

    public UsersController(IUserDatumService userDatumService)
    {
        this.userDatumService = userDatumService;
    }

    [HttpDelete("{baid}")]
    public async Task<IActionResult> DeleteUser(uint baid)
    {
        var result = await userDatumService.DeleteUser(baid);

        return result ? NoContent() : NotFound();
    }
}