using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace TaikoWebUI.Pages.Dialogs;

public partial class UserDeleteConfirmDialog
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public User User { get; set; } = new();

    [Inject]
    public ILocalStorageService LocalStorage { get; set; } = null!;
    
    [Inject]
    public AuthService AuthService { get; set; } = null!;

    private void Cancel() => MudDialog.Cancel();

    private async Task DeleteUser()
    {
        var responseMessage = await Client.DeleteAsync($"api/Users/{User.Baid}");

        if (!responseMessage.IsSuccessStatusCode)
        {
            Snackbar.Add(Localizer["Unknown Error"], Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        Snackbar.Add(Localizer["Delete User Success"], Severity.Success);
        MudDialog.Close(DialogResult.Ok(true));
    }
}