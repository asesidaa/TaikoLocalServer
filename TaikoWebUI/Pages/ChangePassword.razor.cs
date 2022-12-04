namespace TaikoWebUI.Pages;

public partial class ChangePassword
{
    private string cardNum = "";
    private MudForm changePasswordForm = default!;
    private string newPassword = "";
    private string oldPassword = "";
    private string confirmNewPassword = "";
    
    private DashboardResponse? response;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<DashboardResponse>("api/Dashboard");
    }
    
    private async Task OnChangePassword()
    {
        if (response != null)
        {
            var result = await LoginService.ChangePassword(cardNum, oldPassword, newPassword, confirmNewPassword, response, Client);
            switch (result)
            {
                case 0:
                    await DialogService.ShowMessageBox(
                        "Error",
                        "Only admin can log in.",
                        "Ok");
                    NavigationManager.NavigateTo("/Cards");
                    break;
                case 1:
                    await DialogService.ShowMessageBox(
                        "Success",
                        "Password changed successfully.",
                        "Ok");
                    NavigationManager.NavigateTo("/Cards");
                    break;
                case 2:
                    await DialogService.ShowMessageBox(
                        "Error",
                        "Confirm new password is not the same as new password.",
                        "Ok");
                    break;
                case 3:
                    await DialogService.ShowMessageBox(
                        "Error",
                        (MarkupString)
                        "Card number not found.<br />Please play one game with this card number to register it.",
                        "Ok");
                    break;
                case 4:
                    await DialogService.ShowMessageBox(
                        "Error",
                        (MarkupString)
                        "Old password is wrong!",
                        "Ok");
                    break;
            }
        }
    }
}