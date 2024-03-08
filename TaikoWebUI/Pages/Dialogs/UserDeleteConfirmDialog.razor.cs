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
        var responseMessage = await Client.DeleteAsync($"api/Users/{User.Baid}");

        if (!responseMessage.IsSuccessStatusCode)
        {
            Snackbar.Add("Something went wrong when deleting user!", Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        Snackbar.Add("Delete success!", Severity.Success);
        MudDialog.Close(DialogResult.Ok(true));
    }
}