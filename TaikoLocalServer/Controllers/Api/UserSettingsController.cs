using SharedProject.Models.Responses;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("/api/[controller]")]
public class UserSettingsController : BaseController<UserSettingsController>
{
    private readonly TaikoDbContext context;

    public UserSettingsController(TaikoDbContext context)
    {
        this.context = context;
    }

    [HttpGet("{baid}")]
    public async Task<ActionResult<UserSettingResponse>> GetUserSettingResponse(uint baid)
    {
        var user = await context.UserData.FirstOrDefaultAsync(datum => datum.Baid == baid);

        if (user is null)
        {
            return NotFound();
        }

        var response = new UserSettingResponse
        {
            
        };
        return Ok(response);
    }
    
}