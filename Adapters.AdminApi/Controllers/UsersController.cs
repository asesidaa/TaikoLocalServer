
using Microsoft.AspNetCore.RateLimiting;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadProfiles, ApiScopes.AdminUsers)]
public class UsersController(ITaikoDbContext context, IServerAccess access) : BaseAdminController<UsersController>
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

    [HttpGet]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<ActionResult<UsersResponse>> GetUsers([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? searchTerm = null)
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
        var cardEntries = await context.Cards.ToListAsync();
        var userEntriesQuery = context.UserData.AsQueryable();

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

        var registered = await context.Credentials.Where(c => pageBaids.Contains(c.Baid) && c.Password != "")
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
    /// The avatar the WebUI uploaded for costume <paramref name="v"/>. Any other key 404s, so a costume changed in
    /// game shows the fallback until the WebUI renders it again; the URL changes with the key, so it caches forever.
    /// </summary>
    [HttpGet("{baid}/Avatar")]
    [ApiScope(ApiScopes.ReadAvatars)]
    public async Task<IActionResult> GetAvatar(uint baid, [FromQuery] string v)
    {
        var avatar = await context.UserAvatars.FindAsync(baid);
        if (avatar is null || avatar.Key != v)
        {
            return NotFound();
        }

        Response.Headers.CacheControl = "private, max-age=31536000, immutable";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(avatar.Image, avatar.ContentType);
    }

    /// <summary>Stores the avatar (raw WebP or PNG body, at most <see cref="UserAvatar.MaxBytes"/>) for costume <paramref name="key"/>.</summary>
    [HttpPut("{baid}/Avatar")]
    [RequestSizeLimit(UserAvatar.MaxBytes)]
    [EnableRateLimiting(DependencyInjection.AssetsRateLimitPolicy)]
    public async Task<IActionResult> PutAvatar(uint baid, [FromQuery] string key)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        if (key.Length is 0 or > UserAvatar.KeyMaxLength || !key.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'))
        {
            return BadRequest();
        }

        using var body = new MemoryStream();
        await Request.Body.CopyToAsync(body, HttpContext.RequestAborted);
        var image = body.ToArray();
        // Only the two formats the WebUI sends, by their magic bytes (never the client's Content-Type).
        var contentType = image.AsSpan().StartsWith((byte[])[0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A]) ? "image/png"
            : image.Length > 12 && image.AsSpan(0, 4).SequenceEqual("RIFF"u8) && image.AsSpan(8, 4).SequenceEqual("WEBP"u8) ? "image/webp"
            : null;
        if (contentType is null)
        {
            return BadRequest();
        }

        if (!await context.UserData.AnyAsync(u => u.Baid == baid, HttpContext.RequestAborted))
        {
            return NotFound();
        }

        var avatar = await context.UserAvatars.FindAsync(baid);
        if (avatar is null)
        {
            avatar = new UserAvatar { Baid = baid };
            context.UserAvatars.Add(avatar);
        }

        avatar.Image = image;
        avatar.ContentType = contentType;
        avatar.Key = key;
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpDelete("{baid}")]
    public async Task<IActionResult> DeleteUser(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        if (access.LoginRequired && !access.Rules.AllowUserDelete && !User.IsAdmin())
            return Forbid();

        var userDatum = await context.UserData.FindAsync(baid);
        if (userDatum == null)
        {
            return NotFound();
        }

        context.UserData.Remove(userDatum);
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
