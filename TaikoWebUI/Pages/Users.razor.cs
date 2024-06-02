namespace TaikoWebUI.Pages;

public partial class Users
{
    // Tuple of User and UserSetting
    private List<(User, UserSetting)>? usersWithSettings;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (AuthService.LoginRequired && !AuthService.IsLoggedIn)
        {
            await AuthService.LoginWithAuthToken();
        }
        
        if (AuthService.IsAdmin || !AuthService.LoginRequired)
        {
            var users = await Client.GetFromJsonAsync<List<User>>("api/Users");
            var userSettings = await Client.GetFromJsonAsync<List<UserSetting>>("api/UserSettings");
            if (users != null && userSettings != null)
            {
                // Combine User and UserSetting with the same Baid
                usersWithSettings = users.Join(userSettings,
                    user => user.Baid,
                    setting => setting.Baid,
                    (user, setting) => (user, setting))
                    .ToList();
            }
        }
    }
}