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

    public static async ValueTask<Ac15ProfileSettingsResult> SaveAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        DbSet<TDanScore>? danScores,
        Ac15EraProfile profile,
        Ac15ProfileSettingsUpdateDto request,
        Ac15ProfileEditPolicy policy,
        CancellationToken cancellationToken)
        where TSave : class, IAc15ProfileSettingsSaveData
        where TDanScore : class, IAc15DanScoreDatum
    {
        _ = policy;

        var validation = await ValidateUpdateAsync(
            request,
            profile,
            danScores,
            user.Baid,
            cancellationToken);
        if (validation is not null)
        {
            return Ac15ProfileSettingsResult.BadRequest(validation);
        }

        user.MyDonName = request.Identity.MyDonName;
        user.MyDonNameLanguage = request.Identity.MyDonNameLanguage;

        if (request.Customization is { } customization)
        {
            ApplyCustomization(saveData, profile, customization);
        }

        ApplyOptions(saveData, request.Options);
        return Ac15ProfileSettingsResult.Success();
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

    private static async Task<string?> ValidateUpdateAsync<TDanScore>(
        Ac15ProfileSettingsUpdateDto request,
        Ac15EraProfile profile,
        DbSet<TDanScore>? danScores,
        uint baid,
        CancellationToken cancellationToken)
        where TDanScore : class, IAc15DanScoreDatum
    {
        if (request.Identity is null)
        {
            return "Identity is required.";
        }

        if (request.Options is null)
        {
            return "Options are required.";
        }

        var capabilities = profile.ProfileCapabilities;
        if (request.Options.NamePlate is not null && !capabilities.SupportsDisplayDanOnNamePlate)
        {
            return $"NamePlate options are not supported by {profile.Era}.";
        }

        if (request.Options.Folder is not null && !capabilities.SupportsFolderCloseButton)
        {
            return $"Folder options are not supported by {profile.Era}.";
        }

        if (request.Options.CustomizationBehavior is not null && !capabilities.SupportsAutoCostume)
        {
            return $"CustomizationBehavior options are not supported by {profile.Era}.";
        }

        if (request.Options.Tutorials is not null && !capabilities.SupportsHowToPlayTutorialFlag)
        {
            return $"Tutorial options are not supported by {profile.Era}.";
        }

        if (request.Options.SongSelect is { } songSelect)
        {
            if (!capabilities.SupportsLocalRankingDifficulty && songSelect.LocalRankingDifficulty is not null)
            {
                return $"Local ranking difficulty is not supported by {profile.Era}.";
            }

            if (!capabilities.SupportsDefaultSelectedSelfBestDifficulty
                && songSelect.DefaultSelectedAndSelfBestDifficulty is not null)
            {
                return $"Default selected and self best difficulty is not supported by {profile.Era}.";
            }

            if (songSelect.LocalRankingDifficulty is > 4)
            {
                return "Local ranking difficulty must be between 0 and 4.";
            }

            if (songSelect.DefaultSelectedAndSelfBestDifficulty is > 4)
            {
                return "Default selected and self best difficulty must be between 0 and 4.";
            }
        }

        if (request.Options.Taikojuku is { } taikojuku)
        {
            if (!capabilities.SupportsTaikojukuFolderDan || danScores is null)
            {
                return $"Taikojuku folder Dan is not supported by {profile.Era}.";
            }

            var selectableDans = await GetSelectableTaikojukuFolderDans(danScores, baid, profile.Limits, cancellationToken);
            if (!selectableDans.Contains(taikojuku.FolderDan))
            {
                return $"Taikojuku folder Dan {taikojuku.FolderDan} is not selectable for {profile.Era}.";
            }
        }

        if (request.Customization is { } customization)
        {
            foreach (var slot in customization.CostumeSlots ?? [])
            {
                if (!capabilities.CostumeSlots.Contains(slot.Slot, StringComparer.Ordinal))
                {
                    return $"Customization slot '{slot.Slot}' is not supported by {profile.Era}.";
                }
            }

            if (customization.Title is not null && !capabilities.SupportsTitle)
            {
                return $"Title customization is not supported by {profile.Era}.";
            }

            if (customization.Tone is not null && !capabilities.SupportsTone)
            {
                return $"Tone customization is not supported by {profile.Era}.";
            }

            if (customization.Colors is not null && !capabilities.SupportsColors)
            {
                return $"Costume colors are not supported by {profile.Era}.";
            }
        }

        return null;
    }

    private static void ApplyCustomization<TSave>(
        TSave saveData,
        Ac15EraProfile profile,
        Ac15CustomizationUpdateDto customization)
        where TSave : IAc15ProfileSettingsSaveData
    {
        foreach (var slot in customization.CostumeSlots ?? [])
        {
            SetCostume(saveData, slot.Slot, slot.CurrentId);
            if (slot.UnlockedIds is not null)
            {
                SetCostumeFlag(
                    saveData,
                    slot.Slot,
                    EncodeUnlocks(slot.UnlockedIds, slot.CurrentId, profile.Limits.CostumeFlagBytes));
            }
        }

        if (customization.Title is { } title)
        {
            saveData.Title = title.TitleText;
            saveData.TitleplateId = title.TitleId;
            if (title.UnlockedTitleIds is not null)
            {
                saveData.TitleFlg = EncodeUnlocks(title.UnlockedTitleIds, title.TitleId, profile.Limits.TitleFlagBytes);
            }
        }

        if (customization.Tone is { } tone)
        {
            saveData.DefaultToneSetting = tone.ToneId;
            if (tone.UnlockedToneIds is not null)
            {
                saveData.ToneFlg = EncodeUnlocks(tone.UnlockedToneIds, tone.ToneId, profile.Limits.ToneFlagBytes);
            }
        }

        if (customization.Colors is { } colors)
        {
            saveData.ColorBody = colors.BodyColor;
            saveData.ColorFace = colors.FaceColor;
            saveData.ColorLimb = colors.LimbColor;
        }
    }

    private static void ApplyOptions<TSave>(TSave saveData, Ac15ProfileOptionGroupsUpdateDto options)
        where TSave : IAc15ProfileSettingsSaveData
    {
        if (options.NamePlate is { } namePlate)
        {
            saveData.DispDanType = namePlate.DisplayDanOnNamePlate ? 1u : 0u;
        }

        if (options.Folder is { } folder)
        {
            saveData.IsTojiru = folder.ShowFolderCloseButton;
        }

        if (options.CustomizationBehavior is { } customizationBehavior)
        {
            saveData.IsAutoCostumeOn = customizationBehavior.ApplyCostumeChangesFromPlayResults;
        }

        if (options.Tutorials is { DisableHowToPlayTutorial: { } disabled })
        {
            saveData.IsExplain = disabled;
        }

        if (options.SongSelect is { } songSelect)
        {
            if (songSelect.LocalRankingDifficulty is { } localRankingDifficulty)
            {
                saveData.DispLevelChassis = localRankingDifficulty;
            }

            if (songSelect.DefaultSelectedAndSelfBestDifficulty is { } defaultSelectedDifficulty)
            {
                saveData.DispLevelSelf = defaultSelectedDifficulty;
            }
        }

        if (options.Taikojuku is { } taikojuku)
        {
            saveData.DispTaikojukuDan = taikojuku.FolderDan;
        }
    }

    private static byte[] EncodeUnlocks(IEnumerable<uint> requestedUnlocks, uint currentId, int bytes)
    {
        var ids = requestedUnlocks.ToHashSet();
        ids.Add(0);
        ids.Add(currentId);
        return BitsetCodec.Encode(ids.OrderBy(id => id).ToList(), bytes);
    }

    private static void SetCostume<TSave>(TSave saveData, string slot, uint value)
        where TSave : IAc15CustomizationSaveData
    {
        switch (slot)
        {
            case "kigurumi":
                saveData.Costume1 = value;
                break;
            case "head":
                saveData.Costume2 = value;
                break;
            case "body":
                saveData.Costume3 = value;
                break;
            case "face":
                saveData.Costume4 = value;
                break;
            case "puchi":
                saveData.Costume5 = value;
                break;
        }
    }

    private static void SetCostumeFlag<TSave>(TSave saveData, string slot, byte[] value)
        where TSave : IAc15CustomizationSaveData
    {
        switch (slot)
        {
            case "kigurumi":
                saveData.CostumeFlg1 = value;
                break;
            case "head":
                saveData.CostumeFlg2 = value;
                break;
            case "body":
                saveData.CostumeFlg3 = value;
                break;
            case "face":
                saveData.CostumeFlg4 = value;
                break;
            case "puchi":
                saveData.CostumeFlg5 = value;
                break;
        }
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
