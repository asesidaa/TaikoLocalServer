using Microsoft.Extensions.Options;
using OneOf.Types;
using TaikoWebUI.Settings;

namespace TaikoWebUI.Services;

public class LoginService
{
    private readonly string adminPassword;
    private readonly string adminUsername;
    public bool LoginRequired { get; }
    public bool IsLoggedIn { get; private set; }
    public uint Baid { get; private set; }
    private int CardNum { get; set; }
    public bool IsAdmin { get; private set; }
    private bool OnlyAdmin { get; set; }
    
    public LoginService(IOptions<WebUiSettings> settings)
    {
        IsLoggedIn = false;
        Baid = 0;
        CardNum = 0;
        IsAdmin = false;
        var webUiSettings = settings.Value;
        adminUsername = webUiSettings.AdminUsername;
        adminPassword = webUiSettings.AdminPassword;
        LoginRequired = webUiSettings.LoginRequired;
        OnlyAdmin = webUiSettings.OnlyAdmin;
    }

    public int Login(string inputCardNum, string inputPassword, DashboardResponse response)
    {
        if (inputCardNum == adminUsername && inputPassword == adminPassword)
        {
            CardNum = 0;
            Baid = 0;
            IsLoggedIn = true;
            IsAdmin = true;
            return 1;
        }

        if (OnlyAdmin) return 0;

        foreach (var user in response.Users.Where(user => user.AccessCode == inputCardNum))
        {
            if (inputPassword != user.Password) return 2;
            CardNum = int.Parse(user.AccessCode);
            Baid = user.Baid;
            IsLoggedIn = true;
            IsAdmin = false;
            return 1;
        }
        return 3;
    }

    public void Logout()
    {
        IsLoggedIn = false;
        IsAdmin = false;
        Baid = 0;
        CardNum = 0;
    }

    public string GetCardNum()
    {
        if (IsAdmin) return "Admin";
        return CardNum == 0 ? "Not logged in" : CardNum.ToString();
    }
}