namespace TaikoWebUI.Pages.Dialogs;

public partial class ResetPasswordConfirmDialog
{
    
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public User User { get; set; } = new();

    private void Cancel() => MudDialog.Cancel();
    
    private async Task ResetPassword()
    {
        var request = new SetPasswordRequest
        {
            Baid = User.Baid,
            Password = "",
            Salt = ""
        };
        var responseMessage = await Client.PostAsJsonAsync("api/Credentials", request);

        if (!responseMessage.IsSuccessStatusCode)
        {
            Snackbar.Add("Something went wrong when resetting password!", Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }
        
        Snackbar.Add("Reset password success!", Severity.Success);
        MudDialog.Close(DialogResult.Ok(true));
    }
}