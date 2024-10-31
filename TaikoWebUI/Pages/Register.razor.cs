using System.ComponentModel.DataAnnotations;

namespace TaikoWebUI.Pages;

public partial class Register
{
    [Required]
    private string AccessCode { get; set; } = "";

    [Required]
    private string Password { get; set; } = "";

    [Required]
    [Compare(nameof(Password))]
    private string ConfirmPassword { get; set; } = "";

    private MudForm registerForm = default!;

    private MudDatePicker datePicker = new();
    private MudTimePicker timePicker = new();
    private DateTime? date = DateTime.Today;
    private TimeSpan? time = new TimeSpan(00, 45, 00);
    private string inviteCode = "";
    bool debounce = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        BreadcrumbsStateContainer.breadcrumbs.Clear();
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Register"], href: "/Register"));
        BreadcrumbsStateContainer.NotifyStateChanged();
    }

    private async Task OnRegister()
    {
        debounce = true;
        var inputDateTime = date!.Value.Date + time!.Value;
        var result = await AuthService.Register(AccessCode, inputDateTime, Password, ConfirmPassword, inviteCode);
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
                    (string)Localizer["Register Wrong Last Play Time Error"],
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

    internal void HandleKeyDown(KeyboardEventArgs args)
    {
        if (debounce) { debounce = !debounce; return; }
        switch (args.Key)
        {
            case "Enter":
                _ = Submit();
                break;
        }
    }

    private async Task Submit()
    {
        if (registerForm != null)
        {
            await registerForm.Validate();
            if (registerForm.IsValid)
            {
                await OnRegister();
            }
        }
    }
}