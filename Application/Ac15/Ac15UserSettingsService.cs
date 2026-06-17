using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15UserSettingsResult(bool IsSuccess, UserSetting? Setting, string? ErrorMessage)
{
    public static Ac15UserSettingsResult Success(UserSetting? setting = null) => new(true, setting, null);
    public static Ac15UserSettingsResult Error(string message) => new(false, null, message);
}

public static class Ac15UserSettingsService
{
    public static async ValueTask<Ac15UserSettingsResult> GetAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        DbSet<TDanScore> danScores,
        Ac15UserSettingsAccess<TSave> access,
        Ac15ProtocolLimits limits,
        CancellationToken cancellationToken)
        where TDanScore : class, IAc15DanScoreDatum
    {
        var selectableTaikojukuDans = await GetSelectableTaikojukuFolderDans(danScores, user.Baid, limits, cancellationToken);
        var taikojukuDan = SelectTaikojukuFolderDan(
            selectableTaikojukuDans,
            access.GetDispTaikojukuDan(saveData),
            limits);

        return Ac15UserSettingsResult.Success(new UserSetting
        {
            Baid = user.Baid,
            ToneId = access.GetToneId(saveData),
            MyDonName = user.MyDonName,
            MyDonNameLanguage = user.MyDonNameLanguage,
            Title = access.GetTitle(saveData),
            TitlePlateId = access.GetTitlePlateId(saveData),
            Kigurumi = access.GetCostume1(saveData),
            Head = access.GetCostume2(saveData),
            Body = access.GetCostume3(saveData),
            Face = access.GetCostume4(saveData),
            Puchi = access.GetCostume5(saveData),
            UnlockedKigurumi = BitsetCodec.Decode(access.GetCostumeFlg1(saveData), access.CostumeFlagBytes),
            UnlockedHead = BitsetCodec.Decode(access.GetCostumeFlg2(saveData), access.CostumeFlagBytes),
            UnlockedBody = BitsetCodec.Decode(access.GetCostumeFlg3(saveData), access.CostumeFlagBytes),
            UnlockedFace = BitsetCodec.Decode(access.GetCostumeFlg4(saveData), access.CostumeFlagBytes),
            UnlockedPuchi = BitsetCodec.Decode(access.GetCostumeFlg5(saveData), access.CostumeFlagBytes),
            UnlockedTitle = BitsetCodec.Decode(access.GetTitleFlg(saveData), access.TitleFlagBytes),
            UnlockedTone = BitsetCodec.Decode(access.GetToneFlg(saveData), access.ToneFlagBytes),
            BodyColor = access.GetColorBody(saveData),
            FaceColor = access.GetColorFace(saveData),
            LimbColor = access.GetColorLimb(saveData),
            IsDisplayDanOnNamePlate = access.GetDispDanType(saveData) != 0,
            GreenTaikojukuDan = taikojukuDan,
            GreenSelectableTaikojukuDans = selectableTaikojukuDans,
            GreenIsTojiru = access.GetIsTojiru(saveData),
            GreenIsAutoCostumeOn = access.GetIsAutoCostumeOn(saveData),
            GreenDispLevelChassis = SafeDisplayLevel(access.GetDispLevelChassis(saveData)),
            GreenDispLevelSelf = SafeDisplayLevel(access.GetDispLevelSelf(saveData)),
            LastPlayDateTime = access.GetLastPlayDatetime(saveData)
        });
    }

    public static async ValueTask<Ac15UserSettingsResult> SaveAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        UserSetting request,
        DbSet<TDanScore> danScores,
        Ac15UserSettingsAccess<TSave> access,
        Ac15ProtocolLimits limits,
        CancellationToken cancellationToken)
        where TDanScore : class, IAc15DanScoreDatum
    {
        if (request.GreenDispLevelChassis > 4)
        {
            return Ac15UserSettingsResult.Error("GreenDispLevelChassis must be between 0 and 4.");
        }

        if (request.GreenDispLevelSelf > 4)
        {
            return Ac15UserSettingsResult.Error("Default selected and self best difficulty must be between 0 and 4.");
        }

        user.MyDonName = request.MyDonName;
        user.MyDonNameLanguage = request.MyDonNameLanguage;
        access.SetTitle(saveData, request.Title);
        access.SetTitlePlateId(saveData, request.TitlePlateId);
        access.SetColorBody(saveData, request.BodyColor);
        access.SetColorFace(saveData, request.FaceColor);
        access.SetColorLimb(saveData, request.LimbColor);
        access.SetCostume1(saveData, request.Kigurumi);
        access.SetCostume2(saveData, request.Head);
        access.SetCostume3(saveData, request.Body);
        access.SetCostume4(saveData, request.Face);
        access.SetCostume5(saveData, request.Puchi);
        access.SetToneId(saveData, request.ToneId);
        access.SetDispDanType(saveData, request.IsDisplayDanOnNamePlate ? 1u : 0u);
        access.SetIsTojiru(saveData, request.GreenIsTojiru);
        access.SetIsAutoCostumeOn(saveData, request.GreenIsAutoCostumeOn);
        access.SetDispLevelChassis(saveData, request.GreenDispLevelChassis);
        access.SetDispLevelSelf(saveData, request.GreenDispLevelSelf);

        if (request.GreenTaikojukuDan != 0)
        {
            var selectableTaikojukuDans = await GetSelectableTaikojukuFolderDans(
                danScores,
                user.Baid,
                limits,
                cancellationToken);
            access.SetDispTaikojukuDan(
                saveData,
                SelectTaikojukuFolderDan(selectableTaikojukuDans, request.GreenTaikojukuDan, limits));
        }

        access.SetCostumeFlg1(saveData, EncodeUnlocks(request.UnlockedKigurumi, access.GetCostume1(saveData), access.CostumeFlagBytes));
        access.SetCostumeFlg2(saveData, EncodeUnlocks(request.UnlockedHead, access.GetCostume2(saveData), access.CostumeFlagBytes));
        access.SetCostumeFlg3(saveData, EncodeUnlocks(request.UnlockedBody, access.GetCostume3(saveData), access.CostumeFlagBytes));
        access.SetCostumeFlg4(saveData, EncodeUnlocks(request.UnlockedFace, access.GetCostume4(saveData), access.CostumeFlagBytes));
        access.SetCostumeFlg5(saveData, EncodeUnlocks(request.UnlockedPuchi, access.GetCostume5(saveData), access.CostumeFlagBytes));
        access.SetTitleFlg(saveData, EncodeUnlocks(request.UnlockedTitle, access.GetTitlePlateId(saveData), access.TitleFlagBytes));
        access.SetToneFlg(saveData, EncodeUnlocks(request.UnlockedTone, access.GetToneId(saveData), access.ToneFlagBytes));

        return Ac15UserSettingsResult.Success();
    }

    private static async Task<List<uint>> GetSelectableTaikojukuFolderDans<TDanScore>(
        DbSet<TDanScore> danScores,
        uint baid,
        Ac15ProtocolLimits limits,
        CancellationToken cancellationToken)
        where TDanScore : class, IAc15DanScoreDatum
    {
        var clearGrades = await danScores
            .Where(row => row.Baid == baid && !row.IsExtra)
            .Select(row => new { row.DanId, row.ClearGrade })
            .ToListAsync(cancellationToken);
        var clearGradeMap = clearGrades.ToDictionary(row => row.DanId, row => row.ClearGrade);

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

    private static uint SelectTaikojukuFolderDan(
        IReadOnlyList<uint> selectableDans,
        uint requestedDan,
        Ac15ProtocolLimits limits)
        => selectableDans.Contains(requestedDan)
            ? requestedDan
            : selectableDans.FirstOrDefault(limits.MinNormalDanId);

    private static uint SafeDisplayLevel(uint value)
        => value <= 4 ? value : 0u;

    private static byte[] EncodeUnlocks(IEnumerable<uint> requestedUnlocks, uint currentId, int bytes)
    {
        var ids = requestedUnlocks.ToHashSet();
        ids.Add(0);
        ids.Add(currentId);
        return BitsetCodec.Encode(ids.OrderBy(id => id).ToList(), bytes);
    }
}
