using Microsoft.Extensions.Options;
using TaikoWebUI.Settings;

namespace TaikoWebUI.Services;

public class LoginService
{
    private readonly string adminPassword;
    private readonly string adminUsername;

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
    }

    public bool LoginRequired { get; }
    public bool IsLoggedIn { get; private set; }
    public int Baid { get; private set; }
    private int CardNum { get; set; }
    public bool IsAdmin { get; private set; }

    public bool Login(string inputCardNum, string inputPassword, DashboardResponse response)
    {
        if (inputCardNum == adminUsername && inputPassword == adminPassword)
        {
            CardNum = 0;
            Baid = 0;
            IsLoggedIn = true;
            IsAdmin = true;
            return true;
        }

        if (!int.TryParse(inputCardNum, out var n) || n >= response.Users.Count) return false;
        CardNum = n;
        Baid = n;
        IsLoggedIn = true;
        IsAdmin = false;
        return true;
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