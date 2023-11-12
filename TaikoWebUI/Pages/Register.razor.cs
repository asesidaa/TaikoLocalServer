namespace TaikoWebUI.Pages;

public partial class Register
{
    private string accessCode = "";
    private string confirmPassword = "";
    private string password = "";
    private MudForm registerForm = default!;

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
            var result = await LoginService.Register(accessCode, password, confirmPassword, response, Client);
            switch (result)
            {
                case 0:
                    await DialogService.ShowMessageBox(
                        "Error",
                        "Only admin can log in.",
                        "Ok");
                    NavigationManager.NavigateTo("/Users");
                    break;
                case 1:
                    await DialogService.ShowMessageBox(
                        "Success",
                        "Access code registered successfully.",
                        "Ok");
                    NavigationManager.NavigateTo("/Users");
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
                        "Access code not found.<br />Please play one game with this access code to register it.",
                        "Ok");
                    break;
                case 4:
                    await DialogService.ShowMessageBox(
                        "Error",
                        (MarkupString)
                        "Access code is already registered, please use set password to login.",
                        "Ok");
                    NavigationManager.NavigateTo("/Users");
                    break;
            }
        }
    }
}