using System.Text.Json.Serialization;

namespace TaikoLocalServer.Contracts.AdminApi.Ac15ProfileSettings;

public sealed record Ac15ProfileSettingsDto(
    string Era,
    uint Baid,
    Ac15ProfileIdentityDto Identity,
    Ac15CustomizationDto? Customization,
    Ac15ProfileOptionGroupsDto Options,
    Ac15ProfileCapabilitiesDto Capabilities,
    DateTime LastPlayDateTime);

public sealed record Ac15ProfileIdentityDto(
    string MyDonName,
    uint MyDonNameLanguage);

public sealed record Ac15ProfileOptionGroupsDto(
    Ac15NamePlateOptionsDto? NamePlate,
    Ac15FolderOptionsDto? Folder,
    Ac15SongSelectOptionsDto? SongSelect,
    Ac15TaikojukuOptionsDto? Taikojuku,
    Ac15TutorialOptionsDto? Tutorials,
    Ac15CustomizationBehaviorOptionsDto? CustomizationBehavior);

public sealed record Ac15NamePlateOptionsDto(
    bool DisplayDanOnNamePlate);

public sealed record Ac15FolderOptionsDto(
    bool ShowFolderCloseButton);

public sealed record Ac15SongSelectOptionsDto(
    uint? LocalRankingDifficulty,
    uint? DefaultSelectedAndSelfBestDifficulty);

public sealed record Ac15TaikojukuOptionsDto(
    uint FolderDan,
    IReadOnlyList<uint> SelectableFolderDans);

public sealed record Ac15TutorialOptionsDto(
    bool? DisableHowToPlayTutorial);

public sealed record Ac15CustomizationBehaviorOptionsDto(
    bool ApplyCostumeChangesFromPlayResults);

public sealed record Ac15CustomizationDto(
    IReadOnlyList<Ac15CostumeSlotDto> CostumeSlots,
    Ac15TitleSelectionDto? Title,
    Ac15ToneSelectionDto? Tone,
    Ac15CostumeColorsDto? Colors);

public sealed record Ac15CostumeSlotDto(
    string Slot,
    uint CurrentId,
    IReadOnlyList<uint> UnlockedIds);

public sealed record Ac15TitleSelectionDto(
    string TitleText,
    uint TitleId,
    IReadOnlyList<uint> UnlockedTitleIds);

public sealed record Ac15ToneSelectionDto(
    uint ToneId,
    IReadOnlyList<uint> UnlockedToneIds);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15CostumeColorsDto(
    uint BodyColor,
    uint FaceColor,
    uint LimbColor);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15ProfileSettingsUpdateDto(
    Ac15ProfileIdentityDto Identity,
    Ac15CustomizationUpdateDto? Customization,
    Ac15ProfileOptionGroupsUpdateDto Options);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15ProfileOptionGroupsUpdateDto(
    Ac15NamePlateOptionsDto? NamePlate,
    Ac15FolderOptionsDto? Folder,
    Ac15SongSelectOptionsDto? SongSelect,
    Ac15TaikojukuFolderDanUpdateDto? Taikojuku,
    Ac15TutorialOptionsDto? Tutorials,
    Ac15CustomizationBehaviorOptionsDto? CustomizationBehavior);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15TaikojukuFolderDanUpdateDto(
    uint FolderDan);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15CustomizationUpdateDto(
    IReadOnlyList<Ac15CostumeSlotUpdateDto>? CostumeSlots,
    Ac15TitleSelectionUpdateDto? Title,
    Ac15ToneSelectionUpdateDto? Tone,
    Ac15CostumeColorsDto? Colors);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15CostumeSlotUpdateDto(
    string Slot,
    uint CurrentId,
    IReadOnlyList<uint>? UnlockedIds);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15TitleSelectionUpdateDto(
    string TitleText,
    uint TitleId,
    IReadOnlyList<uint>? UnlockedTitleIds);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record Ac15ToneSelectionUpdateDto(
    uint ToneId,
    IReadOnlyList<uint>? UnlockedToneIds);

public sealed record Ac15ProfileCapabilitiesDto(
    IReadOnlyList<string> CostumeSlots,
    bool SupportsTitle,
    bool SupportsTitlePlate,
    bool SupportsTone,
    bool SupportsColors,
    bool SupportsDisplayDanOnNamePlate,
    bool SupportsFolderCloseButton,
    bool SupportsAutoCostume,
    bool SupportsHowToPlayTutorialFlag,
    bool SupportsLocalRankingDifficulty,
    bool SupportsDefaultSelectedSelfBestDifficulty,
    bool SupportsTaikojukuFolderDan);
