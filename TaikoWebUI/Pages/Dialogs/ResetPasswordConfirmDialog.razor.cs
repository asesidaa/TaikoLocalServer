namespace TaikoWebUI.Pages.Dialogs;

public partial class ResetPasswordConfirmDialog
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public User User { get; set; } = new();

    private void Cancel() => MudDialog.Cancel();

    private async Task ResetPassword()
    {
        var request = new ResetPasswordRequest
        {
            Baid = User.Baid
        };
        
        var responseMessage = await Client.PostAsJsonAsync("api/Auth/ResetPassword", request);
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