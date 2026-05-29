using Microsoft.Extensions.Options;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class UserSettingsController(
    ITaikoDbContext context,
    IOptions<AuthSettings> authOptions) : BaseAdminController<UserSettingsController>
{
    private readonly ITaikoDbContext context = context;
    private readonly AuthSettings authSettings = authOptions.Value;

    [HttpGet]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<ActionResult<List<UserSetting>>> GetAllUserSetting()
    {
        var users = await context.UserData.Include(d => d.Tokens).ToListAsync();

        var response = new List<UserSetting>();
        foreach (var user in users)
        {
            var saveData = await context.GetOrCreateNijiiroSaveDataAsync(user.Baid, HttpContext.RequestAborted);
            response.Add(BuildNijiiroUserSetting(user, saveData));
        }

        return Ok(response);
    }

    [HttpGet("{baid}")]
    public Task<ActionResult<UserSetting>> GetUserSetting(uint baid)
        => GetUserSetting(nameof(GameEra.Nijiiro), baid);

    [HttpGet("/api/{era}/[controller]/{baid}")]
    public async Task<ActionResult<UserSetting>> GetUserSetting(string era, uint baid)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        return gameEra switch
        {
            GameEra.Nijiiro => await GetNijiiroUserSetting(baid),
            GameEra.Green => await GetGreenUserSetting(baid),
            GameEra.Blue => await GetBlueUserSetting(baid),
            _ => EraRoute.BadEra(era)
        };
    }

    [HttpPost("{baid}")]
    public Task<IActionResult> SaveUserSetting(uint baid, UserSetting userSetting)
        => SaveUserSetting(nameof(GameEra.Nijiiro), baid, userSetting);

    [HttpPost("/api/{era}/[controller]/{baid}")]
    public async Task<IActionResult> SaveUserSetting(string era, uint baid, UserSetting userSetting)
    {
        if (!EraRoute.TryParse(era, out var gameEra))
            return EraRoute.BadEra(era);

        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        return gameEra switch
        {
            GameEra.Nijiiro => await SaveNijiiroUserSetting(baid, userSetting),
            GameEra.Green => await SaveGreenUserSetting(baid, userSetting),
            GameEra.Blue => await SaveBlueUserSetting(baid, userSetting),
            _ => EraRoute.BadEra(era)
        };
    }

    private bool ShouldEnforceUnlockedOnly()
        => authSettings.AuthenticationRequired
           && !authSettings.AllowFreeProfileEditing
           && !User.IsAdmin();

    private static List<uint> SortedDistinctWithZero(IEnumerable<uint> ids)
    {
        var result = ids.ToHashSet();
        result.Add(0);
        return result.OrderBy(id => id).ToList();
    }
}
