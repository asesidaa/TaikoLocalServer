namespace TaikoWebUI.Pages;

public partial class ChangePassword
{
    private string cardNum = "";
    private MudForm changePasswordForm = default!;
    private string confirmNewPassword = "";
    private string newPassword = "";
    private string oldPassword = "";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        if (AuthService.LoginRequired && !AuthService.IsLoggedIn)
        {
            await AuthService.LoginWithAuthToken();
        }
    }

    private async Task OnChangePassword()
    {
        var result = await AuthService.ChangePassword(cardNum, oldPassword, newPassword, confirmNewPassword);
        switch (result)
        {
            case 0:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Login Only Admin Error"],
                    Localizer["Dialog OK"]);
                NavigationManager.NavigateTo("/Users");
                break;
            case 1:
                await DialogService.ShowMessageBox(
                    Localizer["Success"],
                    (MarkupString)
                    (string)Localizer["Change Password Success"],
                    Localizer["Dialog OK"]);
                NavigationManager.NavigateTo("/Users");
                break;
            case 2:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Change Password Different Confirm Password Error"],
                    Localizer["Dialog OK"]);
                break;
            case 3:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Unknown Access Code Error"],
                    Localizer["Dialog OK"]);
                break;
            case 4:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Change Password Wrong Current Password Error"],
                    Localizer["Dialog OK"]);
                break;
            case 5:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Access Code Not Registered Error"],
                    Localizer["Dialog OK"]);
                break;
            case 6:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    Localizer["Unknown Error"],
                    Localizer["Dialog OK"]);
                break;
        }
    }
}