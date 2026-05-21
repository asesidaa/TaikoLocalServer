using Microsoft.Extensions.Options;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(ITaikoDbContext context, IOptions<AuthSettings> authOptions) : BaseAdminController<UsersController>
{
    private readonly AuthSettings authSettings = authOptions.Value;


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
        var userEntriesQuery = context.UserData.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerCaseSearchTerm = searchTerm.ToLower();
            userEntriesQuery = userEntriesQuery.Where(user => user.Baid.ToString() == lowerCaseSearchTerm
                                                              || user.MyDonName.ToLower().Contains(lowerCaseSearchTerm)
                                                              || context.Cards.Any(card => card.Baid == user.Baid && card.AccessCode.ToLower().Contains(lowerCaseSearchTerm)));
        }

        var totalUsers = await userEntriesQuery.CountAsync(HttpContext.RequestAborted);
        var totalPages = totalUsers / limit;
        if (totalUsers % limit > 0)
        {
            totalPages++;
        }

        var userEntries = await userEntriesQuery
            .OrderBy(user => user.Baid)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(HttpContext.RequestAborted);

        // Batch-load cards + save data for the page only; previously this re-pulled every Card row + N FindAsyncs.
        var pagedBaids = userEntries.Select(u => u.Baid).ToList();
        var cardEntries = await context.Cards
            .Where(card => pagedBaids.Contains(card.Baid))
            .AsNoTracking()
            .ToListAsync(HttpContext.RequestAborted);
        var existingSaveData = await context.UserSaveDataNijiiro
            .Where(s => pagedBaids.Contains(s.Baid))
            .ToDictionaryAsync(s => s.Baid, HttpContext.RequestAborted);

        foreach (var user in userEntries)
        {
            if (!existingSaveData.TryGetValue(user.Baid, out var saveData))
            {
                saveData = await context.GetOrCreateNijiiroSaveDataAsync(user.Baid, HttpContext.RequestAborted);
                existingSaveData[user.Baid] = saveData;
            }
            List<List<uint>> costumeUnlockData =
                [saveData.UnlockedKigurumi, saveData.UnlockedHead, saveData.UnlockedBody, saveData.UnlockedFace, saveData.UnlockedPuchi];

            var unlockedTitle = saveData.TitleFlgArray.ToList();

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
                AchievementDisplayDifficulty = saveData.AchievementDisplayDifficulty,
                IsDisplayAchievement = saveData.DisplayAchievement,
                IsDisplayDanOnNamePlate = saveData.DisplayDan,
                DifficultySettingCourse = saveData.DifficultySettingCourse,
                DifficultySettingStar = saveData.DifficultySettingStar,
                DifficultySettingSort = saveData.DifficultySettingSort,
                IsVoiceOn = saveData.IsVoiceOn,
                IsSkipOn = saveData.IsSkipOn,
                NotesPosition = saveData.NotesPosition,
                PlaySetting = PlaySettingConverter.ShortToPlaySetting(saveData.OptionSetting),
                ToneId = saveData.SelectedToneId,
                MyDonName = user.MyDonName,
                MyDonNameLanguage = user.MyDonNameLanguage,
                Title = saveData.Title,
                TitlePlateId = saveData.TitlePlateId,
                Kigurumi = saveData.CurrentKigurumi,
                Head = saveData.CurrentHead,
                Body = saveData.CurrentBody,
                Face = saveData.CurrentFace,
                Puchi = saveData.CurrentPuchi,
                UnlockedKigurumi = costumeUnlockData[0],
                UnlockedHead = costumeUnlockData[1],
                UnlockedBody = costumeUnlockData[2],
                UnlockedFace = costumeUnlockData[3],
                UnlockedPuchi = costumeUnlockData[4],
                UnlockedTitle = unlockedTitle,
                BodyColor = saveData.ColorBody,
                FaceColor = saveData.ColorFace,
                LimbColor = saveData.ColorLimb,
                LastPlayDateTime = saveData.LastPlayDatetime
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

        if (authSettings.AuthenticationRequired && !authSettings.AllowUserDelete && !User.IsAdmin())
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
}
