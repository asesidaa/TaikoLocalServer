using TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

namespace TaikoLocalServer.Application.Ac15;

public enum Ac15ProfileSettingsResultStatus
{
    Success,
    BadRequest
}

public sealed record Ac15ProfileSettingsResult(
    Ac15ProfileSettingsResultStatus Status,
    Ac15ProfileSettingsDto? Setting,
    string? ErrorMessage)
{
    public static Ac15ProfileSettingsResult Success(Ac15ProfileSettingsDto? setting = null)
        => new(Ac15ProfileSettingsResultStatus.Success, setting, null);

    public static Ac15ProfileSettingsResult BadRequest(string message)
        => new(Ac15ProfileSettingsResultStatus.BadRequest, null, message);
}

public sealed record Ac15ProfileEditPolicy(bool AllowFreeProfileEditing);

public static class Ac15ProfileSettingsService
{
    private static readonly string[] KnownCostumeSlots = ["kigurumi", "head", "body", "face", "puchi"];

    public static async ValueTask<Ac15ProfileSettingsResult> GetAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        DbSet<TDanScore>? danScores,
        Ac15EraProfile profile,
        CancellationToken cancellationToken)
        where TSave : class, IAc15ProfileSettingsSaveData
        where TDanScore : class, IAc15DanScoreDatum
    {
        var capabilities = profile.ProfileCapabilities;
        var selectableTaikojukuDans = capabilities.SupportsTaikojukuFolderDan && danScores is not null
            ? await GetSelectableTaikojukuFolderDans(danScores, user.Baid, profile.Limits, cancellationToken)
            : [];
        var taikojukuDan = capabilities.SupportsTaikojukuFolderDan
            ? SelectTaikojukuFolderDan(selectableTaikojukuDans, saveData.DispTaikojukuDan, profile.Limits)
            : 0;

        var dto = new Ac15ProfileSettingsDto(
            Era: profile.Era.ToString(),
            Baid: user.Baid,
            Identity: new Ac15ProfileIdentityDto(user.MyDonName, user.MyDonNameLanguage),
            Customization: BuildCustomization(saveData, profile),
            Options: BuildOptions(saveData, capabilities, taikojukuDan, selectableTaikojukuDans),
            Capabilities: capabilities.ToDto(),
            LastPlayDateTime: saveData.LastPlayDatetime);

        return Ac15ProfileSettingsResult.Success(dto);
    }

    private static Ac15CustomizationDto? BuildCustomization<TSave>(TSave saveData, Ac15EraProfile profile)
        where TSave : IAc15ProfileSettingsSaveData
    {
        var capabilities = profile.ProfileCapabilities;
        if (capabilities.CostumeSlots.Count == 0
            && !capabilities.SupportsTitle
            && !capabilities.SupportsTone
            && !capabilities.SupportsColors)
        {
            return null;
        }

        var slots = capabilities.CostumeSlots
            .Where(slot => KnownCostumeSlots.Contains(slot, StringComparer.Ordinal))
            .Select(slot => new Ac15CostumeSlotDto(
                slot,
                GetCostume(saveData, slot),
                BitsetCodec.Decode(GetCostumeFlag(saveData, slot), profile.Limits.CostumeFlagBytes)))
            .ToArray();

        return new Ac15CustomizationDto(
            CostumeSlots: slots,
            Title: capabilities.SupportsTitle
                ? new Ac15TitleSelectionDto(
                    saveData.Title,
                    saveData.TitleplateId,
                    BitsetCodec.Decode(saveData.TitleFlg, profile.Limits.TitleFlagBytes))
                : null,
            Tone: capabilities.SupportsTone
                ? new Ac15ToneSelectionDto(
                    saveData.DefaultToneSetting,
                    BitsetCodec.Decode(saveData.ToneFlg, profile.Limits.ToneFlagBytes))
                : null,
            Colors: capabilities.SupportsColors
                ? new Ac15CostumeColorsDto(saveData.ColorBody, saveData.ColorFace, saveData.ColorLimb)
                : null);
    }

    private static Ac15ProfileOptionGroupsDto BuildOptions<TSave>(
        TSave saveData,
        Ac15ProfileCapabilities capabilities,
        uint taikojukuDan,
        IReadOnlyList<uint> selectableTaikojukuDans)
        where TSave : IAc15ProfileSettingsSaveData
        => new(
            NamePlate: capabilities.SupportsDisplayDanOnNamePlate
                ? new Ac15NamePlateOptionsDto(saveData.DispDanType != 0)
                : null,
            Folder: capabilities.SupportsFolderCloseButton
                ? new Ac15FolderOptionsDto(saveData.IsTojiru)
                : null,
            SongSelect: capabilities.SupportsLocalRankingDifficulty
                        || capabilities.SupportsDefaultSelectedSelfBestDifficulty
                ? new Ac15SongSelectOptionsDto(
                    capabilities.SupportsLocalRankingDifficulty ? SafeDisplayLevel(saveData.DispLevelChassis) : null,
                    capabilities.SupportsDefaultSelectedSelfBestDifficulty ? SafeDisplayLevel(saveData.DispLevelSelf) : null)
                : null,
            Taikojuku: capabilities.SupportsTaikojukuFolderDan
                ? new Ac15TaikojukuOptionsDto(taikojukuDan, selectableTaikojukuDans)
                : null,
            Tutorials: capabilities.SupportsHowToPlayTutorialFlag
                ? new Ac15TutorialOptionsDto(saveData.IsExplain)
                : null,
            CustomizationBehavior: capabilities.SupportsAutoCostume
                ? new Ac15CustomizationBehaviorOptionsDto(saveData.IsAutoCostumeOn)
                : null);

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

    private static uint GetCostume<TSave>(TSave saveData, string slot)
        where TSave : IAc15CustomizationSaveData
        => slot switch
        {
            "kigurumi" => saveData.Costume1,
            "head" => saveData.Costume2,
            "body" => saveData.Costume3,
            "face" => saveData.Costume4,
            "puchi" => saveData.Costume5,
            _ => 0
        };

    private static byte[] GetCostumeFlag<TSave>(TSave saveData, string slot)
        where TSave : IAc15CustomizationSaveData
        => slot switch
        {
            "kigurumi" => saveData.CostumeFlg1,
            "head" => saveData.CostumeFlg2,
            "body" => saveData.CostumeFlg3,
            "face" => saveData.CostumeFlg4,
            "puchi" => saveData.CostumeFlg5,
            _ => []
        };
}
