using System.Text.Json;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

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
}
