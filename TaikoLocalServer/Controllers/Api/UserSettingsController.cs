using Microsoft.Extensions.Options;
using SharedProject.Models;
using SharedProject.Utils;
using TaikoLocalServer.Filters;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("/api/[controller]")]
public class UserSettingsController(
    ITaikoDbContext context,
    IJwtTokenService jwtTokens,
    IOptions<AuthSettings> settings) : BaseController<UserSettingsController>
{
    private readonly AuthSettings authSettings = settings.Value;

    [HttpGet]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<List<UserSetting>>> GetAllUserSetting()
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }

            if (!tokenInfo.Value.IsAdmin)
            {
                return Forbid();
            }
        }

        var users = await context.UserData.Include(d => d.Tokens).ToListAsync();

        var response = new List<UserSetting>();
        foreach (var user in users)
        {
            response.Add(BuildUserSetting(user));
        }

        return Ok(response);
    }


    [HttpGet("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<ActionResult<UserSetting>> GetUserSetting(uint baid)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }

            if (tokenInfo.Value.Baid != baid && !tokenInfo.Value.IsAdmin)
            {
                return Forbid();
            }
        }

        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(BuildUserSetting(user));
    }

    [HttpPost("{baid}")]
    [ServiceFilter(typeof(AuthorizeIfRequiredAttribute))]
    public async Task<IActionResult> SaveUserSetting(uint baid, UserSetting userSetting)
    {
        if (authSettings.AuthenticationRequired)
        {
            var tokenInfo = jwtTokens.ExtractTokenInfo(HttpContext);
            if (tokenInfo is null)
            {
                return Unauthorized();
            }

            if (tokenInfo.Value.Baid != baid && !tokenInfo.Value.IsAdmin)
            {
                return Forbid();
            }
        }

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        user.IsSkipOn = userSetting.IsSkipOn;
        user.IsVoiceOn = userSetting.IsVoiceOn;
        user.DisplayAchievement = userSetting.IsDisplayAchievement;
        user.DisplayDan = userSetting.IsDisplayDanOnNamePlate;
        user.DisplaySouUchi = userSetting.IsDisplaySouUchi;
        user.DifficultySettingCourse = userSetting.DifficultySettingCourse;
        user.DifficultySettingStar = userSetting.DifficultySettingStar;
        user.DifficultySettingSort = userSetting.DifficultySettingSort;
        user.NotesPosition = userSetting.NotesPosition;
        user.SelectedToneId = userSetting.ToneId;
        user.AchievementDisplayDifficulty = userSetting.AchievementDisplayDifficulty;
        user.OptionSetting = PlaySettingConverter.PlaySettingToShort(userSetting.PlaySetting);
        user.MyDonName = userSetting.MyDonName;
        user.MyDonNameLanguage = userSetting.MyDonNameLanguage;
        user.Title = userSetting.Title;
        user.TitlePlateId = userSetting.TitlePlateId;
        user.ColorBody = userSetting.BodyColor;
        user.ColorFace = userSetting.FaceColor;
        user.ColorLimb = userSetting.LimbColor;
        user.CurrentKigurumi = userSetting.Kigurumi;
        user.CurrentHead = userSetting.Head;
        user.CurrentBody = userSetting.Body;
        user.CurrentFace = userSetting.Face;
        user.CurrentPuchi = userSetting.Puchi;

        // If a locked tone is selected, unlock it
        var toneFlg = user.ToneFlgArray;
        toneFlg = toneFlg.Append(0u).Append(userSetting.ToneId).Distinct().ToList();
        user.ToneFlgArray = toneFlg;

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private static UserSetting BuildUserSetting(UserDatum user)
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

        return new UserSetting
        {
            Baid = user.Baid,
            AchievementDisplayDifficulty = user.AchievementDisplayDifficulty,
            IsDisplayAchievement = user.DisplayAchievement,
            IsDisplayDanOnNamePlate = user.DisplayDan,
            IsDisplaySouUchi = user.DisplaySouUchi,
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
    }
}
