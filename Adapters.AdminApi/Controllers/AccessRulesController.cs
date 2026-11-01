namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>Server-mode access rules (sign-up, user delete, free editing, access-code limit), admin only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
public class AccessRulesController(IServerAccess access) : BaseAdminController<AccessRulesController>
{
    [HttpGet]
    public ActionResult<AccessRulesSettings> Get()
    {
        var r = access.Rules;
        return Ok(new AccessRulesSettings
        {
            OnlyAdmin = r.OnlyAdmin, RegisterWithLastPlayTime = r.RegisterWithLastPlayTime,
            AllowUserDelete = r.AllowUserDelete, AllowFreeProfileEditing = r.AllowFreeProfileEditing, AccessCodeLimit = r.AccessCodeLimit,
        });
    }

    [HttpPut]
    public async Task<IActionResult> Put(AccessRulesSettings settings)
    {
        if (settings.AccessCodeLimit < 1)
            return BadRequest(new { message = "The access-code limit must be at least 1." });

        await access.UpdateRulesAsync(new AccessRules
        {
            OnlyAdmin = settings.OnlyAdmin, RegisterWithLastPlayTime = settings.RegisterWithLastPlayTime,
            AllowUserDelete = settings.AllowUserDelete, AllowFreeProfileEditing = settings.AllowFreeProfileEditing,
            AccessCodeLimit = settings.AccessCodeLimit,
        }, HttpContext.RequestAborted);
        return NoContent();
    }
}
