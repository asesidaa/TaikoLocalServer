using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
[ApiScope(ApiScopes.ReadProfiles, ApiScopes.AdminUsers)]
public class UserSettingsController(ITaikoDbContext context, IServerAccess access, IGameDataCatalog catalog) : BaseAdminController<UserSettingsController>
{

    [HttpGet]
    [Authorize(Policy = AuthPolicies.Admin)]
    public async Task<ActionResult<List<UserSetting>>> GetAllUserSetting()
    {
        var users = await context.UserData.Include(d => d.Tokens).ToListAsync();
        var profiles = await context.UserWebProfiles.ToDictionaryAsync(p => p.Baid);

        var response = new List<UserSetting>();
        foreach (var user in users)
        {
            response.Add(BuildUserSetting(user, profiles.GetValueOrDefault(user.Baid)));
        }

        return Ok(response);
    }


    [HttpGet("{baid}")]
    public async Task<ActionResult<UserSetting>> GetUserSetting(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(BuildUserSetting(user, await context.UserWebProfiles.FindAsync(baid)));
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
        var merged = JsonSerializer.SerializeToNode(BuildUserSetting(user, await context.UserWebProfiles.FindAsync(baid)), options)!.AsObject();
        foreach (var (key, value) in changes)
        {
            // Match the stored property case-insensitively, as deserialization would.
            var name = merged.Select(p => p.Key).FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase)) ?? key;
            merged[name] = value?.DeepClone();
        }
        var userSetting = merged.Deserialize<UserSetting>(options);
        return userSetting is null ? BadRequest() : await SaveUserSetting(baid, userSetting);
    }

    [HttpPost("{baid}")]
    public async Task<IActionResult> SaveUserSetting(uint baid, UserSetting userSetting)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;

        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var enforceUnlockedOnly = access.LoginRequired
                                  && !access.Rules.AllowFreeProfileEditing
                                  && !User.IsAdmin();
        // Custom titles off: free text is refused (the current title is kept); a game title in any language is fine.
        if (access.LoginRequired && !access.Rules.AllowCustomTitles && !User.IsAdmin() && userSetting.Title != user.Title
            && !catalog.GetTitleDictionary().Values.Any(t => userSetting.Title == t.TitleName || userSetting.Title == t.TitleNameEN
                                                            || userSetting.Title == t.TitleNameCN || userSetting.Title == t.TitleNameKO))
            userSetting.Title = user.Title;

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

        if (enforceUnlockedOnly)
        {
            // Costume / title fields can only contain values the user has already unlocked.
            // Ignore the request payload's *unlocked* lists entirely (those are server-owned),
            // and clamp equipped IDs + the current title to the persisted unlocked sets.
            var unlockedKigurumi = user.UnlockedKigurumi.ToHashSet();
            var unlockedHead = user.UnlockedHead.ToHashSet();
            var unlockedBody = user.UnlockedBody.ToHashSet();
            var unlockedFace = user.UnlockedFace.ToHashSet();
            var unlockedPuchi = user.UnlockedPuchi.ToHashSet();
            var unlockedTitle = user.TitleFlgArray.ToHashSet();

            user.Title = unlockedTitle.Contains(userSetting.TitlePlateId) ? userSetting.Title : user.Title;
            user.TitlePlateId = unlockedTitle.Contains(userSetting.TitlePlateId) ? userSetting.TitlePlateId : user.TitlePlateId;
            user.CurrentKigurumi = unlockedKigurumi.Contains(userSetting.Kigurumi) ? userSetting.Kigurumi : user.CurrentKigurumi;
            user.CurrentHead = unlockedHead.Contains(userSetting.Head) ? userSetting.Head : user.CurrentHead;
            user.CurrentBody = unlockedBody.Contains(userSetting.Body) ? userSetting.Body : user.CurrentBody;
            user.CurrentFace = unlockedFace.Contains(userSetting.Face) ? userSetting.Face : user.CurrentFace;
            user.CurrentPuchi = unlockedPuchi.Contains(userSetting.Puchi) ? userSetting.Puchi : user.CurrentPuchi;
            // Body colors are independent of unlock state; users can always pick from the palette.
            user.ColorBody = userSetting.BodyColor;
            user.ColorFace = userSetting.FaceColor;
            user.ColorLimb = userSetting.LimbColor;
        }
        else
        {
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
        }

        // If a locked tone is selected, unlock it
        var toneFlg = user.ToneFlgArray;
        toneFlg = toneFlg.Append(0u).Append(userSetting.ToneId).Distinct().ToList();
        user.ToneFlgArray = toneFlg;

        // WebUI-only card settings live in UserWebProfiles.
        var profile = await context.UserWebProfiles.FindAsync(baid);
        if (profile is not null)
        {
            profile.CardColor = userSetting.CardColor < 63 ? userSetting.CardColor : profile.CardColor;
            profile.CardPattern = Enum.IsDefined(userSetting.CardPattern) ? userSetting.CardPattern : profile.CardPattern;
            profile.CardFade = userSetting.CardFade is >= -1 and < 63 ? userSetting.CardFade : profile.CardFade;
            profile.CardAnimation = Enum.IsDefined(userSetting.CardAnimation) ? userSetting.CardAnimation : profile.CardAnimation;
        }

        this.LogIfSomeoneElse(context, "profile.edit", baid);
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    internal static UserSetting BuildUserSetting(UserDatum user, UserWebProfile? profile)
    {
        profile ??= new UserWebProfile();
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
            LastPlayDateTime = user.LastPlayDatetime,
            CardColor = profile.CardColor,
            CardPattern = profile.CardPattern,
            CardFade = profile.CardFade,
            CardAnimation = profile.CardAnimation,
        };
    }
}
