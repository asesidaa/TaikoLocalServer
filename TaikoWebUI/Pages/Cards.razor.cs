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
        if (response != null)
        {
            var result = LoginService.Login(cardNum, password, response);
            switch (result)
            {
                case 0:
                    await DialogService.ShowMessageBox(
                        "Error",
                        "Only admin can log in.",
                        "Ok");
                    loginForm.Reset();
                    break;
                case 1:
                    NavigationManager.NavigateTo("/Cards");
                    break;
                case 2:
                    await DialogService.ShowMessageBox(
                        "Error",
                        "Wrong password!",
                        "Ok");
                    break;
                case 3:
                    await DialogService.ShowMessageBox(
                        "Error",
                        (MarkupString)
                        "Card number not found.<br />Please play one game with this card number to register it.",
                        "Ok");
                    break;
                case 4:
                    await DialogService.ShowMessageBox(
                        "Error",
                        (MarkupString)
                        "Card number not registered.<br />Please use register button to create a password first.",
                        "Ok");
                    break;
            }
        }
    }

    private void OnLogout()
    {
        LoginService.Logout();
        NavigationManager.NavigateTo("/Cards");
    }
}