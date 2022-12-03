using TaikoWebUI.Pages.Dialogs;

namespace TaikoWebUI.Pages;

public partial class Cards
{
    private string cardNum = "";
    private MudForm loginForm = default!;
    private string password = "";
    private DashboardResponse? response;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<DashboardResponse>("api/Dashboard");
    }

    private async Task DeleteCard(User user)
    {
        var parameters = new DialogParameters
        {
            ["user"] = user
        };

        var dialog = DialogService.Show<CardDeleteConfirmDialog>("Delete Card", parameters);
        var result = await dialog.Result;

        if (result.Cancelled) return;

        response = await Client.GetFromJsonAsync<DashboardResponse>("api/Dashboard");
    }

    private async Task OnLogin()
    {
        if (response != null && LoginService.Login(cardNum, password, response))
            NavigationManager.NavigateTo("/Cards");
        else
            await DialogService.ShowMessageBox(
                "Error",
                (MarkupString)"Card number not found.<br />Please play one game with this card number to register it.",
                "Ok");
        loginForm.Reset();
    }

    private void OnLogout()
    {
        LoginService.Logout();
        NavigationManager.NavigateTo("/Cards");
    }
}