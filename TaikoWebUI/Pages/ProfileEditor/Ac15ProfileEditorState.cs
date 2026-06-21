using TaikoWebUI.Shared.Customize;

namespace TaikoWebUI.Pages.ProfileEditor;

public sealed class Ac15ProfileEditorState
{
    private Ac15ProfileEditorState(Ac15ProfileSettingsDto source)
    {
        Source = source;
        MyDonName = source.Identity.MyDonName;
        MyDonNameLanguage = source.Identity.MyDonNameLanguage;
        DisplayDanOnNamePlate = source.Options.NamePlate?.DisplayDanOnNamePlate ?? false;
        ShowFolderCloseButton = source.Options.Folder?.ShowFolderCloseButton ?? false;
        LocalRankingDifficulty = source.Options.SongSelect?.LocalRankingDifficulty ?? 0;
        DefaultSelectedAndSelfBestDifficulty = source.Options.SongSelect?.DefaultSelectedAndSelfBestDifficulty ?? 0;
        TaikojukuFolderDan = source.Options.Taikojuku?.FolderDan ?? 0;
        SelectableTaikojukuDans = source.Options.Taikojuku?.SelectableFolderDans ?? [];
        DisableHowToPlayTutorial = source.Options.Tutorials?.DisableHowToPlayTutorial ?? false;
        ApplyCostumeChangesFromPlayResults = source.Options.CustomizationBehavior?.ApplyCostumeChangesFromPlayResults ?? false;
        CostumeSlots = source.Customization?.CostumeSlots
            .Where(slot => source.Capabilities.CostumeSlots.Contains(slot.Slot, StringComparer.Ordinal))
            .Select(slot => new Ac15CostumeSlotEditorState(
                slot.Slot,
                new CostumePickerValue(slot.CurrentId, slot.UnlockedIds)))
            .ToList() ?? [];
        Title = source.Customization?.Title is { } title
            ? new TitlePickerValue(title.TitleText, title.TitleId, title.UnlockedTitleIds)
            : null;
        Tone = source.Customization?.Tone is { } tone
            ? new NeiroPickerValue(tone.ToneId, tone.UnlockedToneIds)
            : null;
        Colors = source.Customization?.Colors is { } colors
            ? new ColorPickerValue(colors.BodyColor, colors.FaceColor, colors.LimbColor)
            : null;
    }

    public Ac15ProfileSettingsDto Source { get; }
    public string MyDonName { get; set; }
    public uint MyDonNameLanguage { get; set; }
    public bool DisplayDanOnNamePlate { get; set; }
    public bool ShowFolderCloseButton { get; set; }
    public uint LocalRankingDifficulty { get; set; }
    public uint DefaultSelectedAndSelfBestDifficulty { get; set; }
    public uint TaikojukuFolderDan { get; set; }
    public IReadOnlyList<uint> SelectableTaikojukuDans { get; }
    public bool DisableHowToPlayTutorial { get; set; }
    public bool ApplyCostumeChangesFromPlayResults { get; set; }
    public List<Ac15CostumeSlotEditorState> CostumeSlots { get; }
    public TitlePickerValue? Title { get; set; }
    public NeiroPickerValue? Tone { get; set; }
    public ColorPickerValue? Colors { get; set; }

    public bool ShowNamePlateOptions => Source.Options.NamePlate is not null;
    public bool ShowFolderOptions => Source.Options.Folder is not null;
    public bool ShowSongSelectOptions => Source.Options.SongSelect is not null;
    public bool ShowTaikojukuOptions => Source.Options.Taikojuku is not null;
    public bool ShowTutorialOptions => Source.Options.Tutorials is not null;
    public bool ShowCustomizationBehaviorOptions => Source.Options.CustomizationBehavior is not null;
    public bool ShowCustomization => Source.Customization is not null
        && (CostumeSlots.Count > 0 || ShowTitle || ShowTone || ShowColors);
    public bool ShowTitle => Source.Capabilities.SupportsTitle && Source.Customization?.Title is not null;
    public bool ShowTitlePlate => Source.Capabilities.SupportsTitlePlate && Source.Customization?.Title is not null;
    public bool ShowTone => Source.Capabilities.SupportsTone && Source.Customization?.Tone is not null;
    public bool ShowColors => Source.Capabilities.SupportsColors && Source.Customization?.Colors is not null;

    public static Ac15ProfileEditorState From(Ac15ProfileSettingsDto source)
        => new(source);

    public Ac15ProfileSettingsUpdateDto ToUpdateDto(bool includeUnlockLists)
        => new(
            Identity: new Ac15ProfileIdentityDto(MyDonName, MyDonNameLanguage),
            Customization: ShowCustomization
                ? new Ac15CustomizationUpdateDto(
                    CostumeSlots: CostumeSlots
                        .Select(slot => new Ac15CostumeSlotUpdateDto(
                            slot.Slot,
                            slot.Value.CurrentId,
                            includeUnlockLists ? slot.Value.UnlockedIds : null))
                        .ToArray(),
                    Title: ShowTitle && Title is not null
                        ? new Ac15TitleSelectionUpdateDto(
                            Title.Title,
                            Title.TitlePlateId,
                            includeUnlockLists ? Title.UnlockedTitleIds : null)
                        : null,
                    Tone: ShowTone && Tone is not null
                        ? new Ac15ToneSelectionUpdateDto(
                            Tone.CurrentId,
                            includeUnlockLists ? Tone.UnlockedIds : null)
                        : null,
                    Colors: ShowColors && Colors is not null
                        ? new Ac15CostumeColorsDto(Colors.BodyColor, Colors.FaceColor, Colors.LimbColor)
                        : null)
                : null,
            Options: new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: ShowNamePlateOptions ? new Ac15NamePlateOptionsDto(DisplayDanOnNamePlate) : null,
                Folder: ShowFolderOptions ? new Ac15FolderOptionsDto(ShowFolderCloseButton) : null,
                SongSelect: ShowSongSelectOptions
                    ? new Ac15SongSelectOptionsDto(
                        Source.Options.SongSelect!.LocalRankingDifficulty is null ? null : LocalRankingDifficulty,
                        Source.Options.SongSelect.DefaultSelectedAndSelfBestDifficulty is null ? null : DefaultSelectedAndSelfBestDifficulty)
                    : null,
                Taikojuku: ShowTaikojukuOptions ? new Ac15TaikojukuFolderDanUpdateDto(TaikojukuFolderDan) : null,
                Tutorials: ShowTutorialOptions ? new Ac15TutorialOptionsDto(DisableHowToPlayTutorial) : null,
                CustomizationBehavior: ShowCustomizationBehaviorOptions
                    ? new Ac15CustomizationBehaviorOptionsDto(ApplyCostumeChangesFromPlayResults)
                    : null));

    public PlayerPreviewModel ToPreviewModel()
        => PlayerPreviewModel.FromAc15(Source with
        {
            Identity = new Ac15ProfileIdentityDto(MyDonName, MyDonNameLanguage),
            Customization = Source.Customization is null
                ? null
                : new Ac15CustomizationDto(
                    CostumeSlots
                        .Select(slot => new Ac15CostumeSlotDto(slot.Slot, slot.Value.CurrentId, slot.Value.UnlockedIds))
                        .ToArray(),
                    Title is null ? null : new Ac15TitleSelectionDto(Title.Title, Title.TitlePlateId, Title.UnlockedTitleIds),
                    Tone is null ? null : new Ac15ToneSelectionDto(Tone.CurrentId, Tone.UnlockedIds),
                    Colors is null ? null : new Ac15CostumeColorsDto(Colors.BodyColor, Colors.FaceColor, Colors.LimbColor)),
            Options = Source.Options with
            {
                NamePlate = ShowNamePlateOptions ? new Ac15NamePlateOptionsDto(DisplayDanOnNamePlate) : null
            }
        });
}

public sealed record Ac15CostumeSlotEditorState(
    string Slot,
    CostumePickerValue Value);
