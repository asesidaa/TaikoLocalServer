

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadProfiles, ApiScopes.AdminUsers)]
public class UsersController(ITaikoDbContext context, IServerAccess access, IAvatarRenderer avatars) : BaseAdminController<UsersController>
{


    [HttpGet("{baid}")]
    public async Task<ActionResult<User?>> GetUser(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var userDatum = await context.UserData.FindAsync(baid);
        if (userDatum == null)
        {
            return NotFound();
        }

        var cardEntries = await context.Cards.Where(card => card.Baid == baid).ToListAsync();
        var profile = await context.UserWebProfiles.FindAsync(baid);
        return new User
        {
            Baid = userDatum.Baid,
            AccessCodes = cardEntries.Select(card => card.AccessCode).ToList(),
            IsAdmin = userDatum.IsAdmin,
            // The sidebar shows the name and avatar from this.
            UserSetting = UserSettingsController.BuildUserSetting(userDatum, profile),
        };
    }

    /// <summary>The Web UI theme they last picked; nulls when they never did.</summary>
    [HttpGet("{baid}/Theme")]
    public async Task<ActionResult<UiTheme>> GetTheme(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var profile = await context.UserWebProfiles.FindAsync([baid], HttpContext.RequestAborted);
        return new UiTheme { Dark = profile?.DarkMode, Accent = profile?.Accent, Language = profile?.Language, Quality3D = profile?.Quality3D };
    }

    [HttpPut("{baid}/Theme")]
    public async Task<IActionResult> PutTheme(uint baid, UiTheme theme)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        if (theme.Accent is { } a && (a.Length > 20 || !a.All(char.IsAsciiLetterLower)))
            return BadRequest(new { message = "Unknown accent." });
        if (theme.Language is { } l && (l.Length is 0 or > 16 || !l.All(c => char.IsAsciiLetter(c) || c == '-')))
            return BadRequest(new { message = "Unknown language." });
        if (theme.Quality3D is { } q && !Enum.IsDefined(q))
            return BadRequest(new { message = "Unknown quality." });
        var profile = await context.UserWebProfiles.FindAsync([baid], HttpContext.RequestAborted);
        if (profile is null) return NotFound();
        profile.DarkMode = theme.Dark ?? profile.DarkMode;
        profile.Accent = theme.Accent ?? profile.Accent;
        profile.Language = theme.Language ?? profile.Language;
        profile.Quality3D = theme.Quality3D ?? profile.Quality3D;
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<ActionResult<UsersResponse>> GetUsers([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? searchTerm = null, [FromQuery] string? filter = null)
    {
        if (page < 1)
        {
            return BadRequest(new { Message = "Page number cannot be less than 1." });
        }

        if (limit > 200)
        {
            return BadRequest(new { Message = "Limit cannot be greater than 200." });
        }

        var users = new List<User>();
        var userEntriesQuery = filter switch
        {
            null or "" or "all" => context.UserData.AsQueryable(),
            "registered" => context.UserData.Where(u => context.Credentials.Any(c => c.Baid == u.Baid && c.Password != "")),
            "unregistered" => context.UserData.Where(u => !context.Credentials.Any(c => c.Baid == u.Baid && c.Password != "")),
            "admins" => context.UserData.Where(u => u.IsAdmin),
            _ => null,
        };
        if (userEntriesQuery is null)
            return BadRequest(new { Message = "Unknown filter." });

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerCaseSearchTerm = searchTerm.ToLower();
            userEntriesQuery = userEntriesQuery.Where(user => user.Baid.ToString() == lowerCaseSearchTerm
                                                              || user.MyDonName.ToLower().Contains(lowerCaseSearchTerm)
                                                              || context.Cards.Any(card => card.Baid == user.Baid && card.AccessCode.ToLower().Contains(lowerCaseSearchTerm)));
        }

        var totalUsers = await userEntriesQuery.CountAsync();
        var totalPages = totalUsers / limit;
        if (totalUsers % limit > 0)
        {
            totalPages++;
        }

        var userEntries = await userEntriesQuery
            .OrderBy(user => user.Baid)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
        var pageBaids = userEntries.Select(user => user.Baid).ToList();
        var profiles = await context.UserWebProfiles.Where(p => pageBaids.Contains(p.Baid)).ToDictionaryAsync(p => p.Baid);
        var cardEntries = await context.Cards.Where(card => pageBaids.Contains(card.Baid)).ToListAsync(HttpContext.RequestAborted);

        var registered = await context.Credentials.Where(c => pageBaids.Contains(c.Baid) && c.Password != "")
            .Select(c => c.Baid).ToListAsync(HttpContext.RequestAborted);
        var twoFactor = await context.Credentials.Where(c => pageBaids.Contains(c.Baid) && c.TotpSecret != null)
            .Select(c => c.Baid).ToListAsync(HttpContext.RequestAborted);
        var plays = await context.SongPlayData.Where(p => pageBaids.Contains(p.Baid)).GroupBy(p => p.Baid)
            .Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(g => g.Key, g => g.Count, HttpContext.RequestAborted);

        foreach (var user in userEntries)
        {
            users.Add(new User
            {
                Baid = user.Baid,
                AccessCodes = cardEntries.Where(card => card.Baid == user.Baid).Select(card => card.AccessCode).ToList(),
                IsAdmin = user.IsAdmin,
                IsRegistered = registered.Contains(user.Baid),
                TwoFactor = twoFactor.Contains(user.Baid),
                Plays = plays.GetValueOrDefault(user.Baid),
                UserSetting = UserSettingsController.BuildUserSetting(user, profiles.GetValueOrDefault(user.Baid))
            });
        }

        return new UsersResponse
        {
            Users = users,
            Page = page,
            TotalPages = totalPages,
            TotalUsers = totalUsers
        };
    }

    /// <summary>
    /// The avatar (drawn by the server's renderer) for costume <paramref name="v"/>. Any other key
    /// 404s and queues a render.
    /// </summary>
    [HttpGet("{baid}/Avatar")]
    [ApiScope(ApiScopes.ReadAvatars)]
    public async Task<IActionResult> GetAvatar(uint baid, [FromQuery] string v)
    {
        var avatar = await context.UserAvatars.FindAsync(baid);
        if (avatar is null || avatar.Key != v)
        {
            // A costume changed (in game too) or a new player: the server renders it, if that's on.
            avatars.Enqueue(baid);
            return NotFound();
        }

        return Cached(avatar.Key) ?? File(avatar.Image, avatar.ContentType);
    }

    // Revalidated on every load (no-cache + the key as ETag), not cached for good: a browser's copy still asks, so an
    // image that's gone from the server (cleared, or never stored) is a miss here and queues a render.
    private IActionResult? Cached(string key)
    {
        var etag = $"\"{key}\"";
        Response.Headers.CacheControl = "private, no-cache";
        Response.Headers.ETag = etag;
        Response.Headers.XContentTypeOptions = "nosniff";
        return Request.Headers.IfNoneMatch == etag ? StatusCode(304) : null;
    }

    /// <summary>
    /// The 2D view's full-body don-chan for costume <paramref name="v"/> (AvatarKeys.StillKey), drawn by the server's
    /// renderer. A miss asks the renderer for one and is a 404 until it's ready.
    /// </summary>
    [HttpGet("{baid}/Still")]
    [ApiScope(ApiScopes.ReadAvatars)]
    public async Task<IActionResult> GetStill(uint baid, [FromQuery] string v)
    {
        var still = await context.UserStills.FindAsync(baid);
        if (still is null || still.Key != v)
        {
            avatars.Enqueue(baid);
            return NotFound();
        }

        return Cached(still.Key) ?? File(still.Image, still.ContentType);
    }

    [HttpDelete("{baid}")]
    public async Task<IActionResult> DeleteUser(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        // Players delete their own account with their password (api/Auth/DeleteAccount), not here.
        if (access.LoginRequired && !User.IsAdmin())
            return Forbid();

        var userDatum = await context.UserData.FindAsync(baid);
        if (userDatum == null)
        {
            return NotFound();
        }

        UserRemoval.Remove(context, userDatum);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    /// <summary>Make a user an admin, or remove the role. Not on yourself, so the server can't lose its last admin by accident.</summary>
    [HttpPut("{baid}/Admin")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> SetAdmin(uint baid, SetAdminRequest request)
    {
        if (access.LoginRequired && User.GetBaid() == baid)
            return BadRequest(new { message = "You can't change your own admin role." });

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
            return NotFound();
        if (user.IsAdmin == request.IsAdmin)
            return NoContent();

        user.IsAdmin = request.IsAdmin;
        this.LogActivity(context, request.IsAdmin ? "user.promote" : "user.demote", baid);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    /// <summary>Remove one best score (a song + difficulty) and the plays behind it.</summary>
    [HttpDelete("{baid}/Scores/{songId}/{difficulty}")]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<IActionResult> RemoveScore(uint baid, uint songId, Difficulty difficulty)
    {
        var best = await context.SongBestData.FindAsync(baid, songId, difficulty);
        if (best is null)
            return NotFound();

        var plays = await context.SongPlayData
            .Where(p => p.Baid == baid && p.SongId == songId && p.Difficulty == difficulty)
            .ToListAsync(HttpContext.RequestAborted);
        context.SongBestData.Remove(best);
        context.SongPlayData.RemoveRange(plays);
        this.LogActivity(context, "score.remove", baid, $"#{songId} {difficulty} · {best.BestScore:N0} · {plays.Count} {(plays.Count == 1 ? "play" : "plays")}");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
