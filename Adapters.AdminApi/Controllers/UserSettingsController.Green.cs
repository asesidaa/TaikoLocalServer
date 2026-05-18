namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

public partial class UserSettingsController
{
    private async Task<ActionResult<UserSetting>> GetGreenUserSetting(uint baid)
    {
        var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateGreenSaveDataAsync(baid, HttpContext.RequestAborted);
        return Ok(await BuildGreenUserSetting(user, saveData));
    }

    private async Task<IActionResult> SaveGreenUserSetting(uint baid, UserSetting userSetting)
    {
        var user = await context.UserData.FindAsync(baid);
        if (user is null)
        {
            return NotFound();
        }

        var saveData = await context.GetOrCreateGreenSaveDataAsync(baid, HttpContext.RequestAborted);

        user.MyDonName = userSetting.MyDonName;
        user.MyDonNameLanguage = userSetting.MyDonNameLanguage;
        saveData.ColorBody = userSetting.BodyColor;
        saveData.ColorFace = userSetting.FaceColor;
        saveData.ColorLimb = userSetting.LimbColor;
        saveData.DispDanType = userSetting.IsDisplayDanOnNamePlate ? 1u : 0u;
        if (userSetting.GreenTaikojukuDan != 0)
        {
            saveData.DispTaikojukuDan = await GetGreenTaikojukuFolderDan(baid, userSetting.GreenTaikojukuDan);
        }

        if (ShouldEnforceUnlockedOnly())
        {
            var unlockedKigurumi = BitsetCodec.Decode(saveData.CostumeFlg1, GreenProtocolBytes.CostumeFlagBytes).ToHashSet();
            var unlockedHead = BitsetCodec.Decode(saveData.CostumeFlg2, GreenProtocolBytes.CostumeFlagBytes).ToHashSet();
            var unlockedBody = BitsetCodec.Decode(saveData.CostumeFlg3, GreenProtocolBytes.CostumeFlagBytes).ToHashSet();
            var unlockedFace = BitsetCodec.Decode(saveData.CostumeFlg4, GreenProtocolBytes.CostumeFlagBytes).ToHashSet();
            var unlockedPuchi = BitsetCodec.Decode(saveData.CostumeFlg5, GreenProtocolBytes.CostumeFlagBytes).ToHashSet();
            var unlockedTone = BitsetCodec.Decode(saveData.ToneFlg, GreenProtocolBytes.ToneFlagBytes).ToHashSet();
            var unlockedTitle = BitsetCodec.Decode(saveData.TitleFlg, GreenProtocolBytes.TitleFlagBytes).ToHashSet();

            saveData.TitleplateId = unlockedTitle.Contains(userSetting.TitlePlateId) ? userSetting.TitlePlateId : saveData.TitleplateId;
            saveData.Costume1 = unlockedKigurumi.Contains(userSetting.Kigurumi) ? userSetting.Kigurumi : saveData.Costume1;
            saveData.Costume2 = unlockedHead.Contains(userSetting.Head) ? userSetting.Head : saveData.Costume2;
            saveData.Costume3 = unlockedBody.Contains(userSetting.Body) ? userSetting.Body : saveData.Costume3;
            saveData.Costume4 = unlockedFace.Contains(userSetting.Face) ? userSetting.Face : saveData.Costume4;
            saveData.Costume5 = unlockedPuchi.Contains(userSetting.Puchi) ? userSetting.Puchi : saveData.Costume5;
            saveData.DefaultToneSetting = unlockedTone.Contains(userSetting.ToneId) ? userSetting.ToneId : saveData.DefaultToneSetting;
        }
        else
        {
            saveData.TitleplateId = userSetting.TitlePlateId;
            saveData.Costume1 = userSetting.Kigurumi;
            saveData.Costume2 = userSetting.Head;
            saveData.Costume3 = userSetting.Body;
            saveData.Costume4 = userSetting.Face;
            saveData.Costume5 = userSetting.Puchi;
            saveData.DefaultToneSetting = userSetting.ToneId;

            saveData.CostumeFlg1 = EncodeGreenCostumeUnlocks(userSetting.UnlockedKigurumi, saveData.Costume1);
            saveData.CostumeFlg2 = EncodeGreenCostumeUnlocks(userSetting.UnlockedHead, saveData.Costume2);
            saveData.CostumeFlg3 = EncodeGreenCostumeUnlocks(userSetting.UnlockedBody, saveData.Costume3);
            saveData.CostumeFlg4 = EncodeGreenCostumeUnlocks(userSetting.UnlockedFace, saveData.Costume4);
            saveData.CostumeFlg5 = EncodeGreenCostumeUnlocks(userSetting.UnlockedPuchi, saveData.Costume5);
            saveData.TitleFlg = BitsetCodec.Encode(
                SortedDistinctWithZero(userSetting.UnlockedTitle).Append(saveData.TitleplateId),
                GreenProtocolBytes.TitleFlagBytes);
            saveData.ToneFlg = BitsetCodec.Encode(
                SortedDistinctWithZero(userSetting.UnlockedTone).Append(saveData.DefaultToneSetting),
                GreenProtocolBytes.ToneFlagBytes);
        }

        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<UserSetting> BuildGreenUserSetting(UserDatum user, UserSaveDataGreen saveData)
    {
        var selectableTaikojukuDans = await GetGreenSelectableTaikojukuFolderDans(user.Baid);
        var taikojukuDan = SelectGreenTaikojukuFolderDan(selectableTaikojukuDans, saveData.DispTaikojukuDan);

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
            UnlockedKigurumi = BitsetCodec.Decode(saveData.CostumeFlg1, GreenProtocolBytes.CostumeFlagBytes),
            UnlockedHead = BitsetCodec.Decode(saveData.CostumeFlg2, GreenProtocolBytes.CostumeFlagBytes),
            UnlockedBody = BitsetCodec.Decode(saveData.CostumeFlg3, GreenProtocolBytes.CostumeFlagBytes),
            UnlockedFace = BitsetCodec.Decode(saveData.CostumeFlg4, GreenProtocolBytes.CostumeFlagBytes),
            UnlockedPuchi = BitsetCodec.Decode(saveData.CostumeFlg5, GreenProtocolBytes.CostumeFlagBytes),
            UnlockedTitle = BitsetCodec.Decode(saveData.TitleFlg, GreenProtocolBytes.TitleFlagBytes),
            UnlockedTone = BitsetCodec.Decode(saveData.ToneFlg, GreenProtocolBytes.ToneFlagBytes),
            BodyColor = saveData.ColorBody,
            FaceColor = saveData.ColorFace,
            LimbColor = saveData.ColorLimb,
            IsDisplayDanOnNamePlate = saveData.DispDanType != 0,
            GreenTaikojukuDan = taikojukuDan,
            GreenSelectableTaikojukuDans = selectableTaikojukuDans,
            LastPlayDateTime = saveData.LastPlayDatetime
        };
    }

    private async Task<uint> GetGreenTaikojukuFolderDan(uint baid, uint requestedDan)
        => SelectGreenTaikojukuFolderDan(
            await GetGreenSelectableTaikojukuFolderDans(baid),
            requestedDan);

    private async Task<List<uint>> GetGreenSelectableTaikojukuFolderDans(uint baid)
    {
        var clearGrades = await context.DanScoreDataGreen
            .Where(row => row.Baid == baid && !row.IsExtra)
            .Select(row => new { row.DanId, row.ClearGrade })
            .ToListAsync(HttpContext.RequestAborted);
        var clearGradeMap = clearGrades.ToDictionary(row => row.DanId, row => row.ClearGrade);

        var selectable = new List<uint>();
        for (uint danId = GreenDanHelpers.MinNormalDanId; danId <= GreenDanHelpers.MaxNormalDanId; danId++)
        {
            if (!clearGradeMap.TryGetValue(danId, out var grade) || !GreenDanHelpers.IsClear(grade))
            {
                selectable.Add(danId);
            }
        }

        return selectable;
    }

    private static uint SelectGreenTaikojukuFolderDan(IReadOnlyList<uint> selectableDans, uint requestedDan)
    {
        if (selectableDans.Contains(requestedDan))
        {
            return requestedDan;
        }

        return selectableDans.FirstOrDefault(GreenDanHelpers.MinNormalDanId);
    }

    private static byte[] EncodeGreenCostumeUnlocks(IEnumerable<uint> requestedUnlocks, uint currentId)
    {
        return BitsetCodec.Encode(
            SortedDistinctWithZero(requestedUnlocks).Append(currentId),
            GreenProtocolBytes.CostumeFlagBytes);
    }
}
