﻿using TaikoWebUI.Pages.Dialogs;

namespace TaikoWebUI.Pages;

public partial class Users
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

    private async Task DeleteUser(User user)
    {
        var parameters = new DialogParameters
        {
            ["user"] = user
        };

        var dialog = DialogService.Show<UserDeleteConfirmDialog>("Delete User", parameters);
        var result = await dialog.Result;

        if (result.Canceled) return;

        response = await Client.GetFromJsonAsync<DashboardResponse>("api/Dashboard");
        OnLogout();
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
                    await loginForm.ResetAsync();
                    break;
                case 1:
                    NavigationManager.NavigateTo("/Users");
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
        NavigationManager.NavigateTo("/Users");
    }
    
    private Task ShowQrCode(User user)
    {
        var parameters = new DialogParameters
        {
            ["user"] = user
        };

        DialogService.Show<UserQrCodeDialog>("QR Code", parameters);

        return Task.CompletedTask;
    }
}