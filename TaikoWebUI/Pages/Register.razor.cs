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
                    (MarkupString)
                    (string)Localizer["Register Only Admin Error"],
                    Localizer["Dialog OK"], null, null, options);
                NavigationManager.NavigateTo("/");
                break;
            case 1:
                await DialogService.ShowMessageBox(
                    Localizer["Success"],
                    (MarkupString)
                    (string)Localizer["Register Success"],
                    Localizer["Dialog OK"], null, null, options);
                NavigationManager.NavigateTo("/Login");
                break;
            case 2:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string)Localizer["Register Different Confirm Password Error"],
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
                    (string)Localizer["Register Already Registered Error"],
                    Localizer["Dialog OK"], null, null, options);
                NavigationManager.NavigateTo("/Login");
                break;
            case 5:
                await DialogService.ShowMessageBox(
                    Localizer["Error"],
                    (MarkupString)
                    (string) Localizer["Register Wrong Last Play Time Error"],
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