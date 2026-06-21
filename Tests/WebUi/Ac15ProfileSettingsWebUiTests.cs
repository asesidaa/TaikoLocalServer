using System.Net;
using TaikoWebUI.Pages.ProfileEditor;
using TaikoWebUI.Services;
using TaikoWebUI.Shared.Customize;

namespace TaikoLocalServer.Tests.WebUi;

public sealed class Ac15ProfileSettingsWebUiTests
{
    [Fact]
    public void EditorState_UsesReturnedGroupsAndSlots()
    {
        var dto = CreateAc15Dto(
            customization: new Ac15CustomizationDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotDto("head", 8, [0, 8])
                ],
                Title: null,
                Tone: null,
                Colors: null),
            options: new Ac15ProfileOptionGroupsDto(
                NamePlate: new Ac15NamePlateOptionsDto(true),
                Folder: null,
                SongSelect: null,
                Taikojuku: null,
                Tutorials: null,
                CustomizationBehavior: null));

        var state = Ac15ProfileEditorState.From(dto);

        Assert.True(state.ShowNamePlateOptions);
        Assert.False(state.ShowFolderOptions);
        Assert.Equal(["head"], state.CostumeSlots.Select(slot => slot.Slot).ToArray());
        var update = state.ToUpdateDto(includeUnlockLists: false);
        Assert.NotNull(update.Options.NamePlate);
        Assert.Null(update.Options.Folder);
        Assert.Single(update.Customization!.CostumeSlots!);
        Assert.Null(update.Customization.CostumeSlots![0].UnlockedIds);
    }

    [Fact]
    public void PlayerPreviewModel_FromAc15DefaultsUnsupportedSlotsToZero()
    {
        var dto = CreateAc15Dto(
            customization: new Ac15CustomizationDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotDto("kigurumi", 12, [0, 12])
                ],
                Title: new Ac15TitleSelectionDto("Title", 10, [10]),
                Tone: null,
                Colors: new Ac15CostumeColorsDto(2, 3, 4)),
            options: new Ac15ProfileOptionGroupsDto(
                NamePlate: new Ac15NamePlateOptionsDto(true),
                Folder: null,
                SongSelect: null,
                Taikojuku: null,
                Tutorials: null,
                CustomizationBehavior: null));

        var preview = PlayerPreviewModel.FromAc15(dto);

        Assert.Equal("DON", preview.MyDonName);
        Assert.Equal("Title", preview.Title);
        Assert.Equal(10u, preview.TitlePlateId);
        Assert.Equal(12u, preview.Kigurumi);
        Assert.Equal(0u, preview.Head);
        Assert.Equal(2u, preview.BodyColor);
        Assert.True(preview.IsDisplayDanOnNamePlate);
    }

    [Fact]
    public void EditorState_KeepsTitleCustomizationWhenTitlePlateUnsupported()
    {
        var dto = CreateAc15Dto(
            customization: new Ac15CustomizationDto(
                CostumeSlots: [],
                Title: new Ac15TitleSelectionDto("White Title", 10, [10]),
                Tone: null,
                Colors: null),
            options: new Ac15ProfileOptionGroupsDto(
                NamePlate: null,
                Folder: null,
                SongSelect: null,
                Taikojuku: null,
                Tutorials: null,
                CustomizationBehavior: null),
            capabilities: Ac15ProfileCapabilities.CurrentWithoutTitlePlate.ToDto());

        var state = Ac15ProfileEditorState.From(dto);

        Assert.True(state.ShowCustomization);
        Assert.True(state.ShowTitle);
        Assert.False(state.ShowTitlePlate);
        var update = state.ToUpdateDto(includeUnlockLists: true);
        Assert.Equal("White Title", update.Customization!.Title!.TitleText);
        Assert.Equal(10u, update.Customization.Title.TitleId);
    }

    [Fact]
    public void TitlePickerCatalog_UsesModeSpecificSelectionLabels()
    {
        Assert.Equal("Title", TitlePickerCatalog.GetSelectionLabelKey(TitleSelectionMode.TitleId));
        Assert.Equal("Title Plate", TitlePickerCatalog.GetSelectionLabelKey(TitleSelectionMode.TitlePlate));
    }

    [Fact]
    public async Task GetProfileDisplayNameAsync_UsesAc15RouteForAc15AndUserSettingsForNijiiro()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var blueName = await client.GetProfileDisplayNameAsync("Blue", 1);
        var nijiiroName = await client.GetProfileDisplayNameAsync("Nijiiro", 2);

        Assert.Equal("BLUE", blueName);
        Assert.Equal("NIJIIRO", nijiiroName);
        Assert.Equal(
            [
                "api/Blue/Ac15ProfileSettings/1",
                "api/Nijiiro/UserSettings/2"
            ],
            handler.RequestPaths);
    }

    [Fact]
    public async Task BreadcrumbDisplayNameHelperPreservesEraSpecificRoutes()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        await client.GetProfileDisplayNameAsync("Red", 99);
        await client.GetProfileDisplayNameAsync("White", 98);
        await client.GetProfileDisplayNameAsync("Murasaki", 97);
        await client.GetProfileDisplayNameAsync("Green", 100);
        await client.GetProfileDisplayNameAsync("Nijiiro", 101);

        Assert.Equal(
            [
                "api/Red/Ac15ProfileSettings/99",
                "api/White/Ac15ProfileSettings/98",
                "api/Murasaki/Ac15ProfileSettings/97",
                "api/Green/Ac15ProfileSettings/100",
                "api/Nijiiro/UserSettings/101"
            ],
            handler.RequestPaths);
    }

    private static Ac15ProfileSettingsDto CreateAc15Dto(
        Ac15CustomizationDto? customization,
        Ac15ProfileOptionGroupsDto options,
        Ac15ProfileCapabilitiesDto? capabilities = null)
        => new(
            Era: "Blue",
            Baid: 1,
            Identity: new Ac15ProfileIdentityDto("DON", 0),
            Customization: customization,
            Options: options,
            Capabilities: capabilities ?? Ac15ProfileCapabilities.CurrentFull.ToDto(),
            LastPlayDateTime: DateTime.UnixEpoch);

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<string> RequestPaths { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.PathAndQuery.TrimStart('/') ?? string.Empty;
            RequestPaths.Add(path);
            var content = path switch
            {
                "api/Blue/Ac15ProfileSettings/1" => """
                {
                  "era": "Blue",
                  "baid": 1,
                  "identity": { "myDonName": "BLUE", "myDonNameLanguage": 0 },
                  "customization": null,
                  "options": {},
                  "capabilities": {
                    "costumeSlots": [],
                    "supportsTitle": false,
                    "supportsTitlePlate": false,
                    "supportsTone": false,
                    "supportsColors": false,
                    "supportsDisplayDanOnNamePlate": false,
                    "supportsFolderCloseButton": false,
                    "supportsAutoCostume": false,
                    "supportsHowToPlayTutorialFlag": false,
                    "supportsLocalRankingDifficulty": false,
                    "supportsDefaultSelectedSelfBestDifficulty": false,
                    "supportsTaikojukuFolderDan": false
                  },
                  "lastPlayDateTime": "1970-01-01T00:00:00Z"
                }
                """,
                "api/Nijiiro/UserSettings/2" => """{ "myDonName": "NIJIIRO" }""",
                "api/Red/Ac15ProfileSettings/99" => """
                {
                  "era": "Red",
                  "baid": 99,
                  "identity": { "myDonName": "RED", "myDonNameLanguage": 0 },
                  "customization": null,
                  "options": {},
                  "capabilities": {
                    "costumeSlots": [],
                    "supportsTitle": false,
                    "supportsTitlePlate": false,
                    "supportsTone": false,
                    "supportsColors": false,
                    "supportsDisplayDanOnNamePlate": false,
                    "supportsFolderCloseButton": false,
                    "supportsAutoCostume": false,
                    "supportsHowToPlayTutorialFlag": false,
                    "supportsLocalRankingDifficulty": false,
                    "supportsDefaultSelectedSelfBestDifficulty": false,
                    "supportsTaikojukuFolderDan": false
                  },
                  "lastPlayDateTime": "1970-01-01T00:00:00Z"
                }
                """,
                "api/White/Ac15ProfileSettings/98" => """
                {
                  "era": "White",
                  "baid": 98,
                  "identity": { "myDonName": "WHITE", "myDonNameLanguage": 0 },
                  "customization": null,
                  "options": {},
                  "capabilities": {
                    "costumeSlots": [],
                    "supportsTitle": false,
                    "supportsTitlePlate": false,
                    "supportsTone": false,
                    "supportsColors": false,
                    "supportsDisplayDanOnNamePlate": false,
                    "supportsFolderCloseButton": false,
                    "supportsAutoCostume": false,
                    "supportsHowToPlayTutorialFlag": false,
                    "supportsLocalRankingDifficulty": false,
                    "supportsDefaultSelectedSelfBestDifficulty": false,
                    "supportsTaikojukuFolderDan": false
                  },
                  "lastPlayDateTime": "1970-01-01T00:00:00Z"
                }
                """,
                "api/Murasaki/Ac15ProfileSettings/97" => """
                {
                  "era": "Murasaki",
                  "baid": 97,
                  "identity": { "myDonName": "MURASAKI", "myDonNameLanguage": 0 },
                  "customization": null,
                  "options": {},
                  "capabilities": {
                    "costumeSlots": [],
                    "supportsTitle": false,
                    "supportsTitlePlate": false,
                    "supportsTone": false,
                    "supportsColors": false,
                    "supportsDisplayDanOnNamePlate": false,
                    "supportsFolderCloseButton": false,
                    "supportsAutoCostume": false,
                    "supportsHowToPlayTutorialFlag": false,
                    "supportsLocalRankingDifficulty": false,
                    "supportsDefaultSelectedSelfBestDifficulty": false,
                    "supportsTaikojukuFolderDan": false
                  },
                  "lastPlayDateTime": "1970-01-01T00:00:00Z"
                }
                """,
                "api/Green/Ac15ProfileSettings/100" => """
                {
                  "era": "Green",
                  "baid": 100,
                  "identity": { "myDonName": "GREEN", "myDonNameLanguage": 0 },
                  "customization": null,
                  "options": {},
                  "capabilities": {
                    "costumeSlots": [],
                    "supportsTitle": false,
                    "supportsTitlePlate": false,
                    "supportsTone": false,
                    "supportsColors": false,
                    "supportsDisplayDanOnNamePlate": false,
                    "supportsFolderCloseButton": false,
                    "supportsAutoCostume": false,
                    "supportsHowToPlayTutorialFlag": false,
                    "supportsLocalRankingDifficulty": false,
                    "supportsDefaultSelectedSelfBestDifficulty": false,
                    "supportsTaikojukuFolderDan": false
                  },
                  "lastPlayDateTime": "1970-01-01T00:00:00Z"
                }
                """,
                "api/Nijiiro/UserSettings/101" => """{ "myDonName": "NIJIIRO" }""",
                _ => "{}"
            };

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }
}
