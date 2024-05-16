namespace TaikoWebUI.Pages;

public partial class Users
{
    private List<User>? users;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (AuthService.IsAdmin || !AuthService.LoginRequired)
        {
            users = await Client.GetFromJsonAsync<List<User>>("api/Users");
        }
    }
}