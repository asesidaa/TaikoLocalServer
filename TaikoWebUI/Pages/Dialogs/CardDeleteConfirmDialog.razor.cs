namespace TaikoWebUI.Pages.Dialogs;

public partial class CardDeleteConfirmDialog
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public User User { get; set; } = new();

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task DeleteCard()
    {
        var responseMessage = await Client.DeleteAsync($"api/Cards/{User.AccessCode}");

        if (!responseMessage.IsSuccessStatusCode)
        {
            Snackbar.Add("Something went wrong when deleting card!", Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        Snackbar.Add("Delete success!", Severity.Success);
        MudDialog.Close(DialogResult.Ok(true));
    }
}