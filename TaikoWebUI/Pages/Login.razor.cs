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
            var result = await LoginService.Login(inputAccessCode, inputPassword, Client);
            var options = new DialogOptions { DisableBackdropClick = true };
            switch (result)
            {
                case 0:
                    await DialogService.ShowMessageBox(
                        Localizer["Error"],
                    "Only admin can log in.",
                        Localizer["Dialog OK"], null, null, options);
                    await loginForm.ResetAsync();
                    break;
                case 1:
                    NavigationManager.NavigateTo("/Users");
                    break;
                case 2:
                    await DialogService.ShowMessageBox(
                        Localizer["Error"],
                        "Wrong password!",
                        Localizer["Dialog OK"], null, null, options);
                    break;
                case 3:
                    await DialogService.ShowMessageBox(
                        Localizer["Error"],
                        (MarkupString)
                        "Access code not found.<br />Please play one game with this access code to register it.",
                        Localizer["Dialog OK"], null, null, options);
                    break;
                case 4:
                    await DialogService.ShowMessageBox(
                        Localizer["Error"],
                        (MarkupString)
                        "Access code not registered.<br />Please use register button to create a password first.",
                        Localizer["Dialog OK"], null, null, options);
                    break;
                case 5:
                    await DialogService.ShowMessageBox(
                        Localizer["Error"],
                        Localizer["Unknown Error"],
                        Localizer["Dialog OK"], null, null, options);
                    break;
            }
        }
    }
}