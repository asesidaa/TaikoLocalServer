namespace TaikoWebUI.Pages;

public partial class Profile
{
    [Parameter]
    public int Baid { get; set; }

    private UserSetting? response;

    private bool isSavingOptions;

    private readonly string[] speedStrings =
    {
        "1.0", "1.1", "1.2", "1.3", "1.4",
        "1.5", "1.6", "1.7", "1.8", "1.9",
        "2.0", "2.5", "3.0", "3.5", "4.0"
    };

    private readonly string[] notePositionStrings = { "-5", "-4", "-3", "-2", "-1", "0", "+1", "+2", "+3", "+4", "+5" };

    private readonly string[] toneStrings =
    {
        "Taiko", "Festival", "Dogs & Cats", "Deluxe",
        "Drumset", "Tambourine", "Don Wada", "Clapping",
        "Conga", "8-Bit", "Heave-ho", "Mecha",
        "Bujain", "Rap", "Hosogai", "Akemi",
        "Synth Drum", "Shuriken", "Bubble Pop", "Electric Guitar"
    };

    private readonly string[] titlePlateStrings =
    {
        "Wood", "Rainbow", "Gold", "Purple",
        "AI 1", "AI 2", "AI 3", "AI 4"
    };

    private List<BreadcrumbItem> breadcrumbs = new()
    {
        new BreadcrumbItem("Cards", href: "/Cards"),
    };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        isSavingOptions = false;
        response = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

        breadcrumbs.Add(new BreadcrumbItem($"Card: {Baid}", href: null, disabled: true));
        breadcrumbs.Add(new BreadcrumbItem("Profile", href: $"/Cards/{Baid}/Profile", disabled: false));
    }

    private async Task SaveOptions()
    {
        isSavingOptions = true;
        await Client.PostAsJsonAsync($"api/UserSettings/{Baid}", response);
        isSavingOptions = false;
    }

}