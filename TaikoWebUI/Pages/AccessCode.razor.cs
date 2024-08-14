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

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        if (AuthService.LoginRequired && !AuthService.IsLoggedIn)
        {
            await AuthService.LoginWithAuthToken();
        }
        
        await InitializeUser();

        userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

        BreadcrumbsStateContainer.breadcrumbs.Clear();
        if (AuthService.IsLoggedIn && !AuthService.IsAdmin) BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        else BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Access Codes"], href: $"/Users/{Baid}/AccessCode", disabled: false));
        BreadcrumbsStateContainer.NotifyStateChanged();
    }

    private async Task InitializeUser()
    {
        User = await Client.GetFromJsonAsync<User>($"api/Users/{Baid}");
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
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Not Logged In Error"],
                    Localizer["Dialog OK"]);
                break;
            case 1:
                await DialogService.ShowMessageBox(
                    Localizer["Success"],
                    (MarkupString)
                    (string)Localizer["Access Code Bound Success"],
                    Localizer["Dialog OK"]);
                await InitializeUser();
                NavigationManager.NavigateTo(NavigationManager.Uri);
                break;
            case 2:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Access Code Upper Limit Error"],
                    Localizer["Dialog OK"]);
                break;
            case 3:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Access Code Already Bound Error"],
                    Localizer["Dialog OK"]);
                break;
            case 4:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Access Code Empty Error"],
                    Localizer["Dialog OK"]);
                break;
            case 5:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Access Code Not Admin Error"],
                    Localizer["Dialog OK"]);
                break;
        }
    }
}