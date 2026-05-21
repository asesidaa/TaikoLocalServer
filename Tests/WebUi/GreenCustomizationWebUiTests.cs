namespace TaikoLocalServer.Tests.WebUi;

public sealed class GreenCustomizationWebUiTests
{
    [Fact]
    public void UserCard_OffersGreenProfileCustomizationRoute()
    {
        var markup = ReadWebUiFile("Components", "UserCard.razor");

        Assert.Contains("WebUiEra.UserRoute(User.Baid, \"Green\", \"Profile\")", markup);
    }

    [Fact]
    public void Profile_GatesNijiiroAchievementControlsOutsideGreen()
    {
        var markup = ReadWebUiFile("Pages", "Profile.razor");

        AssertLabelGuardedByIfNotGreen(markup, "Achievement Panel Difficulty");
        AssertLabelGuardedByIfNotGreen(markup, "Display Achievement Panel");
    }

    [Fact]
    public void Profile_RendersGreenDanDisplaySwitchWithoutMovingNijiiroSwitch()
    {
        var markup = ReadWebUiFile("Pages", "Profile.razor");
        var normalized = NormalizeLineEndings(markup);

        Assert.Contains("@if (!IsGreen)", normalized);
        Assert.Contains("else\n                                {\n                                    <MudSwitch @bind-Value=\"@response.IsDisplayDanOnNamePlate\"", normalized);
        AssertLabelGuardedByIfNotGreen(markup, "Display Dan Rank on Name Plate");
    }

    [Fact]
    public void Profile_ShowsTaikoPreviewAndColorSwatchesForGreen()
    {
        var markup = ReadWebUiFile("Pages", "Profile.razor");
        var normalized = NormalizeLineEndings(markup);

        Assert.Contains("<PlayerPreview Setting=\"@response\" />", markup);
        Assert.DoesNotContain("@if (!IsGreen)\n                                    {\n                                        <PlayerPreview", normalized);
        Assert.DoesNotContain("ShowSwatches=\"@(!IsGreen)\"", markup);
        Assert.Contains("Colors=\"@TaikoCustomizationVisuals.CostumeColors\"", markup);
    }

    [Fact]
    public void Profile_UsesSharedPlayerPreviewForAllEras()
    {
        var markup = ReadWebUiFile("Pages", "Profile.razor");
        var code = ReadWebUiFile("Pages", "Profile.razor.cs");

        Assert.Contains("<PlayerPreview Setting=\"@response\" />", markup);
        Assert.DoesNotContain("response.Kigurumi == 0", markup);
        Assert.DoesNotContain("CostumeOrDefault(", markup);
        Assert.DoesNotContain("private static readonly string[] CostumeColors", code);
        Assert.DoesNotContain("CostumeColorFilters", code);
    }

    [Fact]
    public void PlayerPreview_OwnsSharedNijiiroCostumeAndNameplateMarkup()
    {
        var markup = ReadWebUiFile("Shared", "Customize", "PlayerPreview.razor");

        Assert.Contains("Setting.Kigurumi == 0", markup);
        Assert.Contains("Setting.BodyColor", markup);
        Assert.Contains("Setting.FaceColor", markup);
        Assert.Contains("Setting.LimbColor", markup);
        Assert.Contains("nameplate_dan.webp", markup);
    }

    [Fact]
    public void SharedCustomizationVisuals_OwnColorPaletteAndPreviewFilters()
    {
        var code = ReadWebUiFile("Shared", "Customize", "TaikoCustomizationVisuals.cs");

        Assert.Contains("public static IReadOnlyList<string> CostumeColors", code);
        Assert.Contains("GetCostumeColorFilter(uint colorId)", code);
    }

    [Fact]
    public void Profile_RendersGreenTaikojukuFolderDanAsGreenOnlySetting()
    {
        var markup = ReadWebUiFile("Pages", "Profile.razor");

        Assert.Contains("@if (IsGreen && response.GreenSelectableTaikojukuDans.Count > 0)", markup);
        Assert.Contains("@bind-Value=\"@response.GreenTaikojukuDan\"", markup);
        Assert.Contains("GreenSelectableTaikojukuDans", markup);
    }

    [Fact]
    public void CostumePicker_CachesOrderedCatalogInsteadOfSortingDuringRender()
    {
        var markup = ReadWebUiFile("Shared", "Customize", "CostumePicker.razor");

        Assert.DoesNotContain("private IReadOnlyList<Costume> OrderedCatalog => Catalog", markup);
        Assert.Contains("OnParametersSet", markup);
        Assert.Contains("orderedCatalog", markup);
        Assert.Contains("unlockedIds", markup);
    }

    [Fact]
    public void CostumePicker_VirtualizesUnlockPanelCheckboxList()
    {
        var markup = ReadWebUiFile("Shared", "Customize", "CostumePicker.razor");

        Assert.Contains("using Microsoft.AspNetCore.Components.Web.Virtualization", markup);
        Assert.Contains("<Virtualize Items=\"orderedCatalog\"", markup);
        Assert.Contains("picker-unlock-list", markup);
    }

    [Fact]
    public void TitlePicker_CachesOrderedTitlesAndPlateIdsInOnParametersSet()
    {
        var markup = ReadWebUiFile("Shared", "Customize", "TitlePicker.razor");

        Assert.DoesNotContain("private IReadOnlyList<Title> OrderedTitles => Catalog.Values", markup);
        Assert.DoesNotContain("private IReadOnlyList<uint> PlateIds => TitlePickerCatalog", markup);
        Assert.Contains("protected override void OnParametersSet()", markup);
        Assert.Contains("orderedTitles =", markup);
        Assert.Contains("plateIds =", markup);
        Assert.Contains("unlockedTitleIds =", markup);
    }

    [Fact]
    public void TitlePicker_LazyRendersAndVirtualizesUnlockPanel()
    {
        var markup = ReadWebUiFile("Shared", "Customize", "TitlePicker.razor");

        Assert.Contains("using Microsoft.AspNetCore.Components.Web.Virtualization", markup);
        Assert.Contains("Expanded=\"@unlockPanelExpanded\"", markup);
        Assert.Contains("ExpandedChanged=\"SetUnlockPanelExpanded\"", markup);
        Assert.Contains("@if (unlockPanelExpanded)", markup);
        Assert.Contains("<Virtualize Items=\"orderedTitles\"", markup);
    }

    [Fact]
    public void TitlePicker_UsesDialogActivatorInsteadOfMudSelect()
    {
        var markup = ReadWebUiFile("Shared", "Customize", "TitlePicker.razor");

        Assert.DoesNotContain("<MudSelectItem Value=\"@plate\"", markup);
        Assert.DoesNotContain("<MudSelect T=\"uint\"", markup);
        Assert.Contains("ShowAsync<IdPickerDialog>", markup);
        Assert.Contains("FormatPlateOption(Value.TitlePlateId)", markup);
        Assert.Contains("FormatPlateOption(uint plate)", markup);
    }

    [Fact]
    public void Profile_DoesNotLockTitleTextEditingForGreen()
    {
        var markup = ReadWebUiFile("Pages", "Profile.razor");
        var code = ReadWebUiFile("Pages", "Profile.razor.cs");

        Assert.DoesNotContain("ReadOnlyTitleText=\"@(IsGreen", markup);
        Assert.Contains("ReadOnlyTitleText=\"false\"", markup);
        Assert.Contains("response.Title = titleValue.Title;", code);
        Assert.DoesNotContain("response.Title = IsGreen", code);
    }

    [Fact]
    public void Profile_RefreshesFittedNameplateTitleWhenTitlePickerChanges()
    {
        var markup = ReadWebUiFile("Pages", "Profile.razor");
        var code = ReadWebUiFile("Pages", "Profile.razor.cs");

        Assert.Contains("ValueChanged=\"HandleTitleChanged\"", markup);
        Assert.Contains("private async Task HandleTitleChanged(TitlePickerValue value)", code);
        Assert.Contains("titleValue = value;", code);
        Assert.Contains("ApplyCustomizationValues();", code);
        Assert.Contains("await UpdateTitle();", code);
    }

    [Fact]
    public void Profile_CustomizationCatalogsAreNotUnlockGated()
    {
        var code = ReadWebUiFile("Pages", "Profile.razor.cs");

        Assert.DoesNotContain("catalogById.Keys.Intersect(unlockedIds)", code);
        Assert.DoesNotContain("neirosById.Keys.Intersect(unlockedIds)", code);
        Assert.DoesNotContain("TitleCanBeShown", code);
        Assert.DoesNotContain("lockedCostumeDataDictionary", code);
        Assert.DoesNotContain("lockedTitleDataDictionary", code);
    }

    [Fact]
    public void CostumePicker_UsesDialogActivatorInsteadOfMudSelect()
    {
        var markup = ReadWebUiFile("Shared", "Customize", "CostumePicker.razor");

        Assert.DoesNotContain("<MudSelect T=\"uint\"", markup);
        Assert.DoesNotContain("<MudSelectItem Value=\"@costume.CostumeId\"", markup);
        Assert.Contains("ShowAsync<IdPickerDialog>", markup);
        Assert.Contains("DisplayNameById(Value.CurrentId)", markup);
        Assert.Contains("orderedIds", markup);
        Assert.Contains("costumeById", markup);
    }

    [Fact]
    public void IdPickerDialog_VirtualizesFilteredIdsWithSearch()
    {
        var markup = ReadWebUiFile("Shared", "Customize", "IdPickerDialog.razor");

        Assert.Contains("using Microsoft.AspNetCore.Components.Web.Virtualization", markup);
        Assert.Contains("<Virtualize Items=\"filteredIds\"", markup);
        Assert.Contains("ItemSize=\"48\"", markup);
        Assert.Contains("OnSearchChanged", markup);
        Assert.Contains("MudDialog.Close(DialogResult.Ok(id))", markup);
        Assert.Contains("RadioButtonChecked", markup);
        Assert.Contains("id-picker-dialog-row-selected", markup);
    }

    [Fact]
    public void NeiroPicker_CachesOrderedNeirosInOnParametersSet()
    {
        var markup = ReadWebUiFile("Shared", "Customize", "NeiroPicker.razor");

        Assert.DoesNotContain("private IReadOnlyList<Neiro> OrderedNeiros => Catalog.Values", markup);
        Assert.Contains("protected override void OnParametersSet()", markup);
        Assert.Contains("orderedNeiros =", markup);
    }

    private static void AssertLabelGuardedByIfNotGreen(string markup, string label)
    {
        var index = markup.IndexOf(label, StringComparison.Ordinal);
        Assert.True(index >= 0, $"Could not find '{label}' in Profile.razor.");

        var prefixStart = Math.Max(0, index - 2000);
        var prefix = markup[prefixStart..index];
        Assert.Contains("@if (!IsGreen)", prefix);
    }

    private static void AssertLabelNotGuardedByIfNotGreen(string markup, string label)
    {
        var index = markup.IndexOf(label, StringComparison.Ordinal);
        Assert.True(index >= 0, $"Could not find '{label}' in Profile.razor.");

        var prefixStart = Math.Max(0, index - 2000);
        var prefix = markup[prefixStart..index];
        Assert.DoesNotContain("@if (!IsGreen)", prefix);
    }

    private static string ReadWebUiFile(params string[] pathParts)
        => File.ReadAllText(Path.Combine([FindRepoRoot(), "TaikoWebUI", .. pathParts]));

    private static string NormalizeLineEndings(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
