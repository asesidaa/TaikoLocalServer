using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class UserSettingsController
{
    private async Task<ActionResult<UserSetting>> GetBlueUserSetting(uint baid)
    {
        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateBlueSaveDataAsync(baid, HttpContext.RequestAborted);
        return Ok(await BuildBlueUserSetting(user, saveData));
    }

    private async Task<IActionResult> SaveBlueUserSetting(uint baid, UserSetting userSetting)
    {
        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateBlueSaveDataAsync(baid, HttpContext.RequestAborted);
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
            saveData.DispTaikojukuDan = await GetBlueTaikojukuFolderDan(baid, userSetting.GreenTaikojukuDan);
        }

        saveData.CostumeFlg1 = EncodeBlueCostumeUnlocks(userSetting.UnlockedKigurumi, saveData.Costume1);
        saveData.CostumeFlg2 = EncodeBlueCostumeUnlocks(userSetting.UnlockedHead, saveData.Costume2);
        saveData.CostumeFlg3 = EncodeBlueCostumeUnlocks(userSetting.UnlockedBody, saveData.Costume3);
        saveData.CostumeFlg4 = EncodeBlueCostumeUnlocks(userSetting.UnlockedFace, saveData.Costume4);
        saveData.CostumeFlg5 = EncodeBlueCostumeUnlocks(userSetting.UnlockedPuchi, saveData.Costume5);
        saveData.TitleFlg = BitsetCodec.Encode(
            SortedDistinctWithZero(userSetting.UnlockedTitle).Append(saveData.TitleplateId),
            BlueProtocolBytes.TitleFlagBytes);
        saveData.ToneFlg = BitsetCodec.Encode(
            SortedDistinctWithZero(userSetting.UnlockedTone).Append(saveData.DefaultToneSetting),
            BlueProtocolBytes.ToneFlagBytes);

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<UserSetting> BuildBlueUserSetting(UserDatum user, UserSaveDataBlue saveData)
    {
        var selectableTaikojukuDans = await GetBlueSelectableTaikojukuFolderDans(user.Baid);
        var taikojukuDan = SelectBlueTaikojukuFolderDan(selectableTaikojukuDans, saveData.DispTaikojukuDan);

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
            UnlockedKigurumi = BitsetCodec.Decode(saveData.CostumeFlg1, BlueProtocolBytes.CostumeFlagBytes),
            UnlockedHead = BitsetCodec.Decode(saveData.CostumeFlg2, BlueProtocolBytes.CostumeFlagBytes),
            UnlockedBody = BitsetCodec.Decode(saveData.CostumeFlg3, BlueProtocolBytes.CostumeFlagBytes),
            UnlockedFace = BitsetCodec.Decode(saveData.CostumeFlg4, BlueProtocolBytes.CostumeFlagBytes),
            UnlockedPuchi = BitsetCodec.Decode(saveData.CostumeFlg5, BlueProtocolBytes.CostumeFlagBytes),
            UnlockedTitle = BitsetCodec.Decode(saveData.TitleFlg, BlueProtocolBytes.TitleFlagBytes),
            UnlockedTone = BitsetCodec.Decode(saveData.ToneFlg, BlueProtocolBytes.ToneFlagBytes),
            BodyColor = saveData.ColorBody,
            FaceColor = saveData.ColorFace,
            LimbColor = saveData.ColorLimb,
            IsDisplayDanOnNamePlate = saveData.DispDanType != 0,
            GreenTaikojukuDan = taikojukuDan,
            GreenSelectableTaikojukuDans = selectableTaikojukuDans,
            GreenIsTojiru = saveData.IsTojiru,
            GreenIsAutoCostumeOn = saveData.IsAutoCostumeOn,
            GreenDispLevelChassis = GetSafeBlueDispLevelChassis(saveData.DispLevelChassis),
            GreenDispLevelSelf = GetSafeBlueDispLevelSelf(saveData.DispLevelSelf),
            LastPlayDateTime = saveData.LastPlayDatetime
        };
    }

    private async Task<uint> GetBlueTaikojukuFolderDan(uint baid, uint requestedDan)
        => SelectBlueTaikojukuFolderDan(
            await GetBlueSelectableTaikojukuFolderDans(baid),
            requestedDan);

    private async Task<List<uint>> GetBlueSelectableTaikojukuFolderDans(uint baid)
    {
        var clearGrades = await context.DanScoreDataBlue
            .Where(row => row.Baid == baid && !row.IsExtra)
            .Select(row => new { row.DanId, row.ClearGrade })
            .ToListAsync(HttpContext.RequestAborted);
        var clearGradeMap = clearGrades.ToDictionary(row => row.DanId, row => row.ClearGrade);
        var limits = Ac15EraProfiles.Blue.Limits;

        var selectable = new List<uint>();
        for (var danId = limits.MinNormalDanId; danId <= limits.MaxNormalDanId; danId++)
        {
            if (!clearGradeMap.TryGetValue(danId, out var grade) || !Ac15DanHelpers.IsClear(grade))
            {
                selectable.Add(danId);
            }
        }

        return selectable;
    }

    private static uint SelectBlueTaikojukuFolderDan(IReadOnlyList<uint> selectableDans, uint requestedDan)
    {
        if (selectableDans.Contains(requestedDan))
        {
            return requestedDan;
        }

        return selectableDans.FirstOrDefault(Ac15EraProfiles.Blue.Limits.MinNormalDanId);
    }

    private static uint GetSafeBlueDispLevelChassis(uint value)
        => value <= 4 ? value : 0u;

    private static uint GetSafeBlueDispLevelSelf(uint value)
        => value <= 4 ? value : 0u;

    private static byte[] EncodeBlueCostumeUnlocks(IEnumerable<uint> requestedUnlocks, uint currentId)
    {
        return BitsetCodec.Encode(
            SortedDistinctWithZero(requestedUnlocks).Append(currentId),
            BlueProtocolBytes.CostumeFlagBytes);
    }
}
