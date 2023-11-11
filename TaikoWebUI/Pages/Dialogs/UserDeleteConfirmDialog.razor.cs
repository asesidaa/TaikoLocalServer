namespace TaikoWebUI.Pages.Dialogs;

public partial class UserDeleteConfirmDialog
{
    
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public User User { get; set; } = new();

    private void Cancel() => MudDialog.Cancel();
    
    private async Task DeleteUser()
    {
        var credentialResponseMessage = await Client.DeleteAsync($"api/Credentials/{User.Baid}");

        if (!credentialResponseMessage.IsSuccessStatusCode)
        {
            Snackbar.Add("Something went wrong when deleting user credentials!", Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }
        
        var cardResponseMessage = await Client.DeleteAsync($"api/Cards/{User.Baid}");

        if (!cardResponseMessage.IsSuccessStatusCode)
        {
            Snackbar.Add("Something went wrong when deleting user!", Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        Snackbar.Add("Delete success!", Severity.Success);
        MudDialog.Close(DialogResult.Ok(true));
    }
}