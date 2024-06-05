namespace TaikoWebUI.Pages;

public partial class Login
{
    private string inputAccessCode = "";
    private MudForm loginForm = default!;
    private string inputPassword = "";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        if (AuthService.LoginRequired && !AuthService.IsLoggedIn)
        {
            await AuthService.LoginWithAuthToken();
        }
    }

    private async Task OnLogin()
    {
        var result = await AuthService.Login(inputAccessCode, inputPassword);
        var options = new DialogOptions { DisableBackdropClick = true };
        switch (result)
        {
            case 0:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                (MarkupString)
                (string)Localizer["Login Only Admin Error"],
                    Localizer["Dialog OK"], null, null, options);
                await loginForm.ResetAsync();
                break;
            case 1:
                NavigationManager.NavigateTo("/Users");
                break;
            case 2:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Login Wrong Password Error"],
                    Localizer["Dialog OK"], null, null, options);
                break;
            case 3:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Unknown Access Code Error"],
                    Localizer["Dialog OK"], null, null, options);
                break;
            case 4:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Access Code Not Registered Error"],
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