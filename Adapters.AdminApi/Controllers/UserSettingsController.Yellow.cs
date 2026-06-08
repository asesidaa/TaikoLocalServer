using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class UserSettingsController
{
    private async Task<ActionResult<UserSetting>> GetYellowUserSetting(uint baid)
    {
        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateYellowSaveDataAsync(baid, HttpContext.RequestAborted);
        return Ok(await BuildYellowUserSetting(user, saveData));
    }

    private async Task<IActionResult> SaveYellowUserSetting(uint baid, UserSetting userSetting)
    {
        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateYellowSaveDataAsync(baid, HttpContext.RequestAborted);
        if (userSetting.GreenDispLevelChassis > 4)
        {
            return BadRequest("GreenDispLevelChassis must be between 0 and 4.");
        }

        if (userSetting.GreenDispLevelSelf > 4)
        {
            return BadRequest("GreenDispLevelSelf must be between 0 and 4.");
        }

        user.MyDonName = userSetting.MyDonName;
        user.MyDonNameLanguage = userSetting.MyDonNameLanguage;
        saveData.Title = userSetting.Title;
        saveData.TitleplateId = userSetting.TitlePlateId;
        saveData.ColorBody = userSetting.BodyColor;
        saveData.ColorFace = userSetting.FaceColor;
        saveData.ColorLimb = userSetting.LimbColor;
        saveData.Costume1 = userSetting.Kigurumi;
        saveData.Costume2 = userSetting.Head;
        saveData.Costume3 = userSetting.Body;
        saveData.Costume4 = userSetting.Face;
        saveData.Costume5 = userSetting.Puchi;
        saveData.DefaultToneSetting = userSetting.ToneId;
        saveData.DispDanType = userSetting.IsDisplayDanOnNamePlate ? 1u : 0u;
        saveData.IsTojiru = userSetting.GreenIsTojiru;
        saveData.IsAutoCostumeOn = userSetting.GreenIsAutoCostumeOn;
        saveData.DispLevelChassis = userSetting.GreenDispLevelChassis;
        saveData.DispLevelSelf = userSetting.GreenDispLevelSelf;
        if (userSetting.GreenTaikojukuDan != 0)
        {
            saveData.DispTaikojukuDan = await GetYellowTaikojukuFolderDan(baid, userSetting.GreenTaikojukuDan);
        }

        var limits = Ac15EraProfiles.Yellow.Limits;
        saveData.CostumeFlg1 = EncodeYellowCostumeUnlocks(userSetting.UnlockedKigurumi, saveData.Costume1);
        saveData.CostumeFlg2 = EncodeYellowCostumeUnlocks(userSetting.UnlockedHead, saveData.Costume2);
        saveData.CostumeFlg3 = EncodeYellowCostumeUnlocks(userSetting.UnlockedBody, saveData.Costume3);
        saveData.CostumeFlg4 = EncodeYellowCostumeUnlocks(userSetting.UnlockedFace, saveData.Costume4);
        saveData.CostumeFlg5 = EncodeYellowCostumeUnlocks(userSetting.UnlockedPuchi, saveData.Costume5);
        saveData.TitleFlg = BitsetCodec.Encode(
            SortedDistinctWithZero(userSetting.UnlockedTitle).Append(saveData.TitleplateId),
            limits.TitleFlagBytes);
        saveData.ToneFlg = BitsetCodec.Encode(
            SortedDistinctWithZero(userSetting.UnlockedTone).Append(saveData.DefaultToneSetting),
            limits.ToneFlagBytes);

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<UserSetting> BuildYellowUserSetting(UserDatum user, UserSaveDataYellow saveData)
    {
        var selectableTaikojukuDans = await GetYellowSelectableTaikojukuFolderDans(user.Baid);
        var taikojukuDan = SelectYellowTaikojukuFolderDan(selectableTaikojukuDans, saveData.DispTaikojukuDan);
        var limits = Ac15EraProfiles.Yellow.Limits;

        return new UserSetting
        {
            Baid = user.Baid,
            ToneId = saveData.DefaultToneSetting,
            MyDonName = user.MyDonName,
            MyDonNameLanguage = user.MyDonNameLanguage,
            Title = saveData.Title,
            TitlePlateId = saveData.TitleplateId,
            Kigurumi = saveData.Costume1,
            Head = saveData.Costume2,
            Body = saveData.Costume3,
            Face = saveData.Costume4,
            Puchi = saveData.Costume5,
            UnlockedKigurumi = BitsetCodec.Decode(saveData.CostumeFlg1, limits.CostumeFlagBytes),
            UnlockedHead = BitsetCodec.Decode(saveData.CostumeFlg2, limits.CostumeFlagBytes),
            UnlockedBody = BitsetCodec.Decode(saveData.CostumeFlg3, limits.CostumeFlagBytes),
            UnlockedFace = BitsetCodec.Decode(saveData.CostumeFlg4, limits.CostumeFlagBytes),
            UnlockedPuchi = BitsetCodec.Decode(saveData.CostumeFlg5, limits.CostumeFlagBytes),
            UnlockedTitle = BitsetCodec.Decode(saveData.TitleFlg, limits.TitleFlagBytes),
            UnlockedTone = BitsetCodec.Decode(saveData.ToneFlg, limits.ToneFlagBytes),
            BodyColor = saveData.ColorBody,
            FaceColor = saveData.ColorFace,
            LimbColor = saveData.ColorLimb,
            IsDisplayDanOnNamePlate = saveData.DispDanType != 0,
            GreenTaikojukuDan = taikojukuDan,
            GreenSelectableTaikojukuDans = selectableTaikojukuDans,
            GreenIsTojiru = saveData.IsTojiru,
            GreenIsAutoCostumeOn = saveData.IsAutoCostumeOn,
            GreenDispLevelChassis = GetSafeYellowDispLevelChassis(saveData.DispLevelChassis),
            GreenDispLevelSelf = GetSafeYellowDispLevelSelf(saveData.DispLevelSelf),
            LastPlayDateTime = saveData.LastPlayDatetime
        };
    }

    private async Task<uint> GetYellowTaikojukuFolderDan(uint baid, uint requestedDan)
        => SelectYellowTaikojukuFolderDan(
            await GetYellowSelectableTaikojukuFolderDans(baid),
            requestedDan);

    private async Task<List<uint>> GetYellowSelectableTaikojukuFolderDans(uint baid)
    {
        var clearGrades = await context.DanScoreDataYellow
            .Where(row => row.Baid == baid && !row.IsExtra)
            .Select(row => new { row.DanId, row.ClearGrade })
            .ToListAsync(HttpContext.RequestAborted);
        var clearGradeMap = clearGrades.ToDictionary(row => row.DanId, row => row.ClearGrade);

        var selectable = new List<uint>();
        for (uint danId = YellowDanHelpers.MinNormalDanId; danId <= YellowDanHelpers.MaxNormalDanId; danId++)
        {
            if (!clearGradeMap.TryGetValue(danId, out var grade) || !YellowDanHelpers.IsClear(grade))
            {
                selectable.Add(danId);
            }
        }

        return selectable;
    }

    private static uint SelectYellowTaikojukuFolderDan(IReadOnlyList<uint> selectableDans, uint requestedDan)
    {
        if (selectableDans.Contains(requestedDan))
        {
            return requestedDan;
        }

        return selectableDans.FirstOrDefault(YellowDanHelpers.MinNormalDanId);
    }

    private static uint GetSafeYellowDispLevelChassis(uint value)
        => value <= 4 ? value : 0u;

    private static uint GetSafeYellowDispLevelSelf(uint value)
        => value <= 4 ? value : 0u;

    private static byte[] EncodeYellowCostumeUnlocks(IEnumerable<uint> requestedUnlocks, uint currentId)
    {
        return BitsetCodec.Encode(
            SortedDistinctWithZero(requestedUnlocks).Append(currentId),
            Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes);
    }
}
