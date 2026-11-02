using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadProfiles, ApiScopes.AdminUsers)]
public partial class UserSettingsController(
    ITaikoDbContext context,
    IServerAccess access,
    IGameDataCatalog catalog) : BaseAdminController<UserSettingsController>
{
    private readonly ITaikoDbContext context = context;
    [HttpGet]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<ActionResult<List<UserSetting>>> GetAllUserSetting()
    {
        var users = await context.UserData.Include(d => d.Tokens).ToListAsync();
        var profiles = await context.UserWebProfiles.ToDictionaryAsync(p => p.Baid);
        var response = new List<UserSetting>();
        foreach (var user in users)
        {
            var saveData = await context.GetOrCreateNijiiroSaveDataAsync(user.Baid, HttpContext.RequestAborted);
            response.Add(BuildNijiiroUserSetting(user, saveData, profiles.GetValueOrDefault(user.Baid)));
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
            _ => EraRoute.BadEra(era)
        };
    }

    /// <summary>
    /// Saves only the fields in the body (a partial UserSetting), merged over the stored settings and then saved
    /// like a full POST. Two open pages each send just what they changed, so neither undoes the other's edits.
    /// </summary>
    [HttpPatch("{baid}")]
    public async Task<IActionResult> PatchUserSetting(uint baid, [FromBody] JsonObject changes,
        [FromServices] IOptions<JsonOptions> jsonOptions)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        var options = jsonOptions.Value.JsonSerializerOptions;
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(baid, HttpContext.RequestAborted);
        var merged = JsonSerializer.SerializeToNode(BuildNijiiroUserSetting(user, saveData, await context.UserWebProfiles.FindAsync(baid)), options)!.AsObject();
        foreach (var (key, value) in changes)
        {
            // Match the stored property case-insensitively, as deserialization would.
            var name = merged.Select(p => p.Key).FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase)) ?? key;
            merged[name] = value?.DeepClone();
        }
        var userSetting = merged.Deserialize<UserSetting>(options);
        return userSetting is null ? BadRequest() : await SaveNijiiroUserSetting(baid, userSetting);
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
            _ => EraRoute.BadEra(era)
        };
    }

    private bool ShouldEnforceUnlockedOnly()
        => access.LoginRequired
           && !access.Rules.AllowFreeProfileEditing
           && !User.IsAdmin();

    private static List<uint> SortedDistinctWithZero(IEnumerable<uint> ids)
    {
        var result = ids.ToHashSet();
        result.Add(0);
        return result.OrderBy(id => id).ToList();
    }
}
