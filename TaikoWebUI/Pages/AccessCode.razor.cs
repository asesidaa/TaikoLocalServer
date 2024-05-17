using TaikoWebUI.Pages.Dialogs;

namespace TaikoWebUI.Pages;

public partial class AccessCode
{
    [Parameter]
    public int Baid { get; set; }

    private string inputAccessCode = "";
    private MudForm bindAccessCodeForm = default!;

    private User? User { get; set; }
    
    private UserSetting? userSetting;

    private readonly List<BreadcrumbItem> breadcrumbs = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await InitializeUser();

        userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

        if (AuthService.IsLoggedIn && !AuthService.IsAdmin)
        {
            breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        }
        else
        {
            breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        }
        breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        breadcrumbs.Add(new BreadcrumbItem(Localizer["Access Codes"], href: $"/Users/{Baid}/AccessCode", disabled: false));
    }

    private async Task InitializeUser()
    {
        if (!AuthService.LoginRequired)
        {
            var users = await Client.GetFromJsonAsync<List<User>>("api/Users");
            if (users != null) User = users.FirstOrDefault(u => u.Baid == Baid);
        }
        else if (AuthService.IsLoggedIn)
        {
            User = await Client.GetFromJsonAsync<User>($"api/Users/{Baid}");
        }
    }

    private async Task DeleteAccessCode(string accessCode)
    {
        var parameters = new DialogParameters<AccessCodeDeleteConfirmDialog>
        {
            { x => x.User, User },
            { x => x.AccessCode, accessCode }
        };

        var dialog = await DialogService.ShowAsync<AccessCodeDeleteConfirmDialog>("Delete Access Code", parameters);
        var result = await dialog.Result;

        if (result.Canceled) return;

        await InitializeUser();
        NavigationManager.NavigateTo(NavigationManager.Uri);
    }

    private async Task OnBind()
    {
        if (User == null) return;
        var result = await AuthService.BindAccessCode(inputAccessCode.ToUpper().Trim(), User);
        switch (result)
        {
            case 0:
                await DialogService.ShowMessageBox(
                    "Error",
                    (MarkupString)
                    "Not logged in.<br />Please log in first and try again.",
                    "Ok");
                break;
            case 1:
                await DialogService.ShowMessageBox(
                    "Success",
                    "New access code bound successfully.",
                    "Ok");
                await InitializeUser();
                NavigationManager.NavigateTo(NavigationManager.Uri);
                break;
            case 2:
                await DialogService.ShowMessageBox(
                    "Error",
                    (MarkupString)
                    "Bound access code upper limit reached.<br />Please delete one access code first.",
                    "Ok");
                break;
            case 3:
                await DialogService.ShowMessageBox(
                    "Error",
                    (MarkupString)
                    "Access code already bound.<br />Please delete it from the bound user first.",
                    "Ok");
                break;
            case 4:
                await DialogService.ShowMessageBox(
                    "Error",
                    (MarkupString)
                    "Access code cannot be empty.<br />Please enter a valid access code.",
                    "Ok");
                break;
            case 5:
                await DialogService.ShowMessageBox(
                    "Error",
                    (MarkupString)
                    "You can't do that!<br />You need to be an admin to edit someone else's access codes.",
                    "Ok");
                break;
        }
    }
}