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
            Snackbar.Add(Localizer["Access Code Delete Last Access Code Error"], Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        var cardResponseMessage = await Client.DeleteAsync($"api/Cards/{AccessCode}");

        if (!cardResponseMessage.IsSuccessStatusCode)
        {
            Snackbar.Add(Localizer["Unknown Error"], Severity.Error);
            MudDialog.Close(DialogResult.Ok(false));
            return;
        }

        Snackbar.Add(Localizer["Access Code Delete Success"], Severity.Success);
        MudDialog.Close(DialogResult.Ok(true));
    }
}