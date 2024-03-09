using Microsoft.Extensions.Options;
using TaikoWebUI.Pages.Dialogs;

namespace TaikoWebUI.Pages;

public partial class Login
{
    private string inputAccessCode = "";
    private MudForm loginForm = default!;
    private string inputPassword = "";
    private DashboardResponse? response;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<DashboardResponse>("api/Dashboard");
    }

    private async Task OnLogin()
    {
        if (response != null)
        {
            var result = LoginService.Login(inputAccessCode, inputPassword, response);
            var options = new DialogOptions() { DisableBackdropClick = true };
            switch (result)
            {
                case 0:
                    await DialogService.ShowMessageBox(
                        "Error",
                    "Only admin can log in.",
                        "Ok", null, null, options);
                    await loginForm.ResetAsync();
                    break;
                case 1:
                    NavigationManager.NavigateTo("/Users");
                    break;
                case 2:
                    await DialogService.ShowMessageBox(
                        "Error",
                        "Wrong password!",
                        "Ok", null, null, options);
                    break;
                case 3:
                    await DialogService.ShowMessageBox(
                        "Error",
                        (MarkupString)
                        "Access code not found.<br />Please play one game with this access code to register it.",
                        "Ok", null, null, options);
                    break;
                case 4:
                    await DialogService.ShowMessageBox(
                        "Error",
                        (MarkupString)
                        "Access code not registered.<br />Please use register button to create a password first.",
                        "Ok", null, null, options);
                    break;
            }
        }
    }
}