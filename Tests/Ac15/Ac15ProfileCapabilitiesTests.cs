using System.Text.Json;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15ProfileCapabilitiesTests
{
    [Fact]
    public void CurrentImplementedAc15ProfilesExposeFullProfileSettingsCapabilities()
    {
        var profiles = new[] { Ac15EraProfiles.Blue, Ac15EraProfiles.Green, Ac15EraProfiles.Yellow, Ac15EraProfiles.Red };

        foreach (var profile in profiles)
        {
            Assert.Equal(["kigurumi", "head", "body", "face", "puchi"], profile.ProfileCapabilities.CostumeSlots);
            Assert.True(profile.ProfileCapabilities.SupportsTitle);
            Assert.True(profile.ProfileCapabilities.SupportsTitlePlate);
            Assert.True(profile.ProfileCapabilities.SupportsTone);
            Assert.True(profile.ProfileCapabilities.SupportsColors);
            Assert.True(profile.ProfileCapabilities.SupportsDisplayDanOnNamePlate);
            Assert.True(profile.ProfileCapabilities.SupportsFolderCloseButton);
            Assert.True(profile.ProfileCapabilities.SupportsAutoCostume);
            Assert.True(profile.ProfileCapabilities.SupportsHowToPlayTutorialFlag);
            Assert.True(profile.ProfileCapabilities.SupportsLocalRankingDifficulty);
            Assert.True(profile.ProfileCapabilities.SupportsDefaultSelectedSelfBestDifficulty);
            Assert.True(profile.ProfileCapabilities.SupportsTaikojukuFolderDan);
        }
    }

    [Fact]
    public void WhiteAndMurasakiExposeTitleCustomizationWithoutTitlePlateCustomization()
    {
        var profiles = new[] { Ac15EraProfiles.White, Ac15EraProfiles.Murasaki };

        foreach (var profile in profiles)
        {
            Assert.True(profile.ProfileCapabilities.SupportsTitle);
            Assert.False(profile.ProfileCapabilities.SupportsTitlePlate);

            var dto = profile.ProfileCapabilities.ToDto();

            Assert.True(dto.SupportsTitle);
            Assert.False(dto.SupportsTitlePlate);
        }
    }

    [Fact]
    public void CapabilityDtoCanRepresentTitleOnlyOlderEra()
    {
        var capabilities = new Ac15ProfileCapabilities(
            CostumeSlots: [],
            SupportsTitle: true,
            SupportsTitlePlate: false,
            SupportsTone: false,
            SupportsColors: false,
            SupportsDisplayDanOnNamePlate: true,
            SupportsFolderCloseButton: false,
            SupportsAutoCostume: false,
            SupportsHowToPlayTutorialFlag: false,
            SupportsLocalRankingDifficulty: false,
            SupportsDefaultSelectedSelfBestDifficulty: false,
            SupportsTaikojukuFolderDan: false);

        var dto = capabilities.ToDto();

        Assert.Empty(dto.CostumeSlots);
        Assert.True(dto.SupportsTitle);
        Assert.False(dto.SupportsTitlePlate);
        Assert.False(dto.SupportsTone);
        Assert.True(dto.SupportsDisplayDanOnNamePlate);
    }

    [Fact]
    public void UpdateDtoRejectsUnknownJsonFieldsLocally()
    {
        const string json = """
        {
          "identity": { "myDonName": "DON", "myDonNameLanguage": 0 },
          "customization": null,
          "options": {},
          "unexpectedField": 1
        }
        """;

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Ac15ProfileSettingsUpdateDto>(json, options));
    }
}
