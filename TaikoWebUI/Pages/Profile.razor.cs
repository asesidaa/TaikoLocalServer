using TaikoWebUI.Pages.Dialogs;

namespace TaikoWebUI.Pages;

public partial class Profile
{
    private static readonly string[] CostumeColors =
    {
        "#F84828", "#68C0C0", "#DC1500", "#F8F0E0", "#009687", "#00BF87",
        "#00FF9A", "#66FFC2", "#FFFFFF", "#690000", "#FF0000", "#FF6666",
        "#FFB3B3", "#00BCC2", "#00F7FF", "#66FAFF", "#B3FDFF", "#E4E4E4",
        "#993800", "#FF5E00", "#FF9E78", "#FFCFB3", "#005199", "#0088FF",
        "#66B8FF", "#B3DBFF", "#B9B9B9", "#B37700", "#FFAA00", "#FFCC66",
        "#FFE2B3", "#000C80", "#0019FF", "#6675FF", "#B3BAFF", "#858585",
        "#B39B00", "#FFDD00", "#FFFF00", "#FFFF71", "#2B0080", "#5500FF",
        "#9966FF", "#CCB3FF", "#505050", "#38A100", "#78C900", "#B3FF00",
        "#DCFF8A", "#610080", "#C400FF", "#DC66FF", "#EDB3FF", "#232323",
        "#006600", "#00B800", "#00FF00", "#8AFF9E", "#990059", "#FF0095",
        "#FF66BF", "#FFB3DF", "#000000"
    };

    private static readonly string[] SpeedStrings =
    {
        "1.0", "1.1", "1.2", "1.3", "1.4",
        "1.5", "1.6", "1.7", "1.8", "1.9",
        "2.0", "2.5", "3.0", "3.5", "4.0"
    };

    private static readonly string[] NotePositionStrings =
        { "-5", "-4", "-3", "-2", "-1", "0", "+1", "+2", "+3", "+4", "+5" };

    private static readonly string[] ToneStrings =
    {
        "Taiko", "Festival", "Dogs & Cats", "Deluxe",
        "Drumset", "Tambourine", "Don Wada", "Clapping",
        "Conga", "8-Bit", "Heave-ho", "Mecha",
        "Bujain", "Rap", "Hosogai", "Akemi",
        "Synth Drum", "Shuriken", "Bubble Pop", "Electric Guitar"
    };

    private static readonly string[] TitlePlateStrings =
    {
        "Wood", "Rainbow", "Gold", "Purple",
        "AI 1", "AI 2", "AI 3", "AI 4"
    };

    private readonly List<BreadcrumbItem> breadcrumbs = new()
    {
        new BreadcrumbItem("Cards", "/Cards")
    };

    private bool isSavingOptions;

    private UserSetting? response;

    [Parameter] public int Baid { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        isSavingOptions = false;
        response = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

        breadcrumbs.Add(new BreadcrumbItem($"Card: {Baid}", null, true));
        breadcrumbs.Add(new BreadcrumbItem("Profile", $"/Cards/{Baid}/Profile"));
    }

    private async Task SaveOptions()
    {
        isSavingOptions = true;
        await Client.PostAsJsonAsync($"api/UserSettings/{Baid}", response);
        isSavingOptions = false;
    }

    private async Task OpenChooseTitleDialog()
    {
        var options = new DialogOptions
        {
            //CloseButton = false,
            CloseOnEscapeKey = false,
            DisableBackdropClick = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };
        var parameters = new DialogParameters
        {
            ["UserSetting"] = response
        };
        var dialog = DialogService.Show<ChooseTitleDialog>("Player Titles", parameters, options);
        var result = await dialog.Result;
        if (!result.Cancelled) StateHasChanged();
    }
}