using System.Text.Json;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoWebUI.Shared.Customize;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenCustomizationContractTests
{
    [Fact]
    public void UserSetting_CarriesUnlockedToneIds()
    {
        var setting = new UserSetting
        {
            ToneId = 4,
            UnlockedTone = [0, 4, 7]
        };

        Assert.Equal(new List<uint> { 0, 4, 7 }, setting.UnlockedTone);
    }

    [Fact]
    public void GreenCatalogDtos_CarryOptionalSourceProvenance()
    {
        var costume = new Costume
        {
            CostumeId = 7,
            CostumeType = "unknown",
            CostumeName = string.Empty,
            Source = "ndp"
        };
        var title = new Title
        {
            TitleId = 131,
            TitleName = string.Empty,
            Source = "rewardtitlefiltering"
        };
        var neiro = new Neiro
        {
            NeiroId = 4,
            NeiroName = string.Empty,
            Source = "ndp"
        };

        var json = JsonSerializer.Serialize(new { costume, title, neiro });

        Assert.Contains("ndp", json);
        Assert.Contains("rewardtitlefiltering", json);
        Assert.Contains("\"neiroId\":4", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TitlePickerCatalog_UsesTitleIdsForGreenTitleSelection()
    {
        var titles = new Dictionary<uint, Title>
        {
            [10] = new() { TitleId = 10, TitleName = "Green Title", TitleRarity = 0 },
            [11] = new() { TitleId = 11, TitleName = "Other Green Title", TitleRarity = 0 }
        };

        var ids = TitlePickerCatalog.GetSelectableIds(titles, currentId: 10, TitleSelectionMode.TitleId);

        Assert.Equal(new uint[] { 10, 11 }, ids);
    }

    [Fact]
    public void TitlePickerCatalog_ResolvesGreenSelectedTitleTextFromCatalog()
    {
        var titles = new Dictionary<uint, Title>
        {
            [10] = new() { TitleId = 10, TitleName = "Green Title", TitleRarity = 0 }
        };

        var title = TitlePickerCatalog.ResolveSelectedTitleText(titles, selectedId: 10, fallback: "Previous");

        Assert.Equal("Green Title", title);
    }
}
