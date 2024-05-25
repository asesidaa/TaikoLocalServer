namespace TaikoWebUI.Pages;

public partial class Register
{
    private string accessCode = "";
    private string confirmPassword = "";
    private string password = "";
    private MudForm registerForm = default!;

    private MudDatePicker datePicker = new();
    private MudTimePicker timePicker = new();
    private DateTime? date = DateTime.Today;
    private TimeSpan? time = new TimeSpan(00, 45, 00);
    private string inviteCode = "";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        if (AuthService.LoginRequired && !AuthService.IsLoggedIn)
        {
            await AuthService.LoginWithAuthToken();
        }
    }

    private async Task OnRegister()
    {
        var inputDateTime = date!.Value.Date + time!.Value;
        var result = await AuthService.Register(accessCode, inputDateTime, password, confirmPassword, inviteCode);
        var options = new DialogOptions { DisableBackdropClick = true };
        switch (result)
        {
            case 0:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    "Only admin can register.",
                    Localizer["Dialog OK"], null, null, options);
                NavigationManager.NavigateTo("/");
                break;
            case 1:
                await DialogService.ShowMessageBox(
                    Localizer["Success"],
                    "Access code registered successfully.",
                    Localizer["Dialog OK"], null, null, options);
                NavigationManager.NavigateTo("/Login");
                break;
            case 2:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    "Confirm password is not the same as password.",
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
                    "Access code is already registered, please use set password to login.",
                    Localizer["Dialog OK"], null, null, options);
                NavigationManager.NavigateTo("/Login");
                break;
            case 5:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    "Wrong last play time.<br />If you have forgotten when you last played, please play another game with this access code.",
                    Localizer["Dialog OK"], null, null, options);
                break;
            case 6:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    Localizer["Unknown Error"],
                    Localizer["Dialog OK"], null, null, options);
                break;
        }
    }
}