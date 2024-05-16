using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace TaikoWebUI.Pages.Dialogs;

public partial class AccessCodeDeleteConfirmDialog
{

    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public User User { get; set; } = new();

    [Parameter]
    public string AccessCode { get; set; } = "";
    
    [Inject]
    public ILocalStorageService LocalStorage { get; set; } = null!;
    
    [Inject]
    public AuthService AuthService { get; set; } = null!;


    private void Cancel() => MudDialog.Cancel();

    private async Task DeleteAccessCode()
    {
        if (User.AccessCodes.Count == 1)
        {
            Snackbar.Add("Cannot delete last access code!", Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        var cardResponseMessage = await Client.DeleteAsync($"api/Cards/{AccessCode}");

        if (!cardResponseMessage.IsSuccessStatusCode)
        {
            Snackbar.Add("Something went wrong when deleting access code!", Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        Snackbar.Add("Delete success!", Severity.Success);
        MudDialog.Close(DialogResult.Ok(true));
    }
}