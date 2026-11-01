
namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        return new User
        {
            Baid = userDatum.Baid,
            AccessCodes = cardEntries.Select(card => card.AccessCode).ToList(),
            IsAdmin = userDatum.IsAdmin
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

        foreach (var user in userEntries)
        {
            List<List<uint>> costumeUnlockData =
                [user.UnlockedKigurumi, user.UnlockedHead, user.UnlockedBody, user.UnlockedFace, user.UnlockedPuchi];

            var unlockedTitle = user.TitleFlgArray.ToList();

            for (var i = 0; i < 5; i++)
            {
                if (!costumeUnlockData[i].Contains(0))
                {
                    costumeUnlockData[i].Add(0);
                }
            }

            var userSetting = new UserSetting
            {
                Baid = user.Baid,
                AchievementDisplayDifficulty = user.AchievementDisplayDifficulty,
                IsDisplayAchievement = user.DisplayAchievement,
                IsDisplayDanOnNamePlate = user.DisplayDan,
                DifficultySettingCourse = user.DifficultySettingCourse,
                DifficultySettingStar = user.DifficultySettingStar,
                DifficultySettingSort = user.DifficultySettingSort,
                IsVoiceOn = user.IsVoiceOn,
                IsSkipOn = user.IsSkipOn,
                NotesPosition = user.NotesPosition,
                PlaySetting = PlaySettingConverter.ShortToPlaySetting(user.OptionSetting),
                ToneId = user.SelectedToneId,
                MyDonName = user.MyDonName,
                MyDonNameLanguage = user.MyDonNameLanguage,
                Title = user.Title,
                TitlePlateId = user.TitlePlateId,
                Kigurumi = user.CurrentKigurumi,
                Head = user.CurrentHead,
                Body = user.CurrentBody,
                Face = user.CurrentFace,
                Puchi = user.CurrentPuchi,
                UnlockedKigurumi = costumeUnlockData[0],
                UnlockedHead = costumeUnlockData[1],
                UnlockedBody = costumeUnlockData[2],
                UnlockedFace = costumeUnlockData[3],
                UnlockedPuchi = costumeUnlockData[4],
                UnlockedTitle = unlockedTitle,
                BodyColor = user.ColorBody,
                FaceColor = user.ColorFace,
                LimbColor = user.ColorLimb,
                LastPlayDateTime = user.LastPlayDatetime
            };

            users.Add(new User
            {
                Baid = user.Baid,
                AccessCodes = cardEntries.Where(card => card.Baid == user.Baid).Select(card => card.AccessCode).ToList(),
                IsAdmin = user.IsAdmin,
                UserSetting = userSetting
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
