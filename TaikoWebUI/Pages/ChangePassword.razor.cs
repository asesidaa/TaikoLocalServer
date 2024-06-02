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
                    "Only admin can log in.",
                    Localizer["Dialog OK"]);
                NavigationManager.NavigateTo("/Users");
                break;
            case 1:
                await DialogService.ShowMessageBox(
                    Localizer["Success"],
                    "Password changed successfully.",
                    Localizer["Dialog OK"]);
                NavigationManager.NavigateTo("/Users");
                break;
            case 2:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    "Confirm new password is not the same as new password.",
                    Localizer["Dialog OK"]);
                break;
            case 3:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    "Card number not found.<br />Please play one game with this card number to register it.",
                    Localizer["Dialog OK"]);
                break;
            case 4:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    "Old password is wrong!",
                    Localizer["Dialog OK"]);
                break;
            case 5:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    "Card number not registered.<br />Please use register button to create a password first.",
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