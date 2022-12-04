namespace TaikoWebUI.Pages;

public partial class Register
{
    private string cardNum = "";
    private MudForm registerForm = default!;
    private string password = "";
    private string confirmPassword = "";
    
    private DashboardResponse? response;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<DashboardResponse>("api/Dashboard");
    }
    
    private async Task OnRegister()
    {
        if (response != null)
        {
            var result = await LoginService.Register(cardNum, password, confirmPassword, response, Client);
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
                        "Card registered successfully.",
                        "Ok");
                    NavigationManager.NavigateTo("/Cards");
                    break;
                case 2:
                    await DialogService.ShowMessageBox(
                        "Error",
                        "Confirm password is not the same as password.",
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
                        "Card is already registered, please use set password to login.",
                        "Ok");
                    NavigationManager.NavigateTo("/Cards");
                    break;
            }
        }
    }
}