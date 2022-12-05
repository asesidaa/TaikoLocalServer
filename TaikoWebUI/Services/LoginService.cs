using Microsoft.Extensions.Options;
using TaikoWebUI.Settings;

namespace TaikoWebUI.Services;

public class LoginService
{
    private readonly string adminPassword;
    private readonly string adminUsername;
    public bool IsLoggedIn { get; private set; }
    public uint Baid { get; private set; }
    private int CardNum { get; set; }
    public bool IsAdmin { get; private set; }
    
    public bool LoginRequired { get; }
    public bool OnlyAdmin { get; }
    
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
            if (user.Password == "") return 4;
            if (ComputeHash(inputPassword) != user.Password) return 2;
            CardNum = int.Parse(user.AccessCode);
            Baid = user.Baid;
            IsLoggedIn = true;
            IsAdmin = false;
            return 1;
        }

        return 3;
    }

    public async Task<int> Register(string inputCardNum, string inputPassword, string inputConfirmPassword,
        DashboardResponse response, HttpClient client)
    {
        if (OnlyAdmin) return 0;
        foreach (var user in response.Users.Where(user => user.AccessCode == inputCardNum))
        {
            if (user.Password != "") return 4;
            if (inputPassword != inputConfirmPassword) return 2;
            var request = new SetPasswordRequest
            {
                AccessCode = user.AccessCode,
                Password = ComputeHash(inputPassword)
            };
            var responseMessage = await client.PostAsJsonAsync("api/Cards", request);
            return responseMessage.IsSuccessStatusCode ? 1 : 3;
        }

        return 3;
    }

    private string ComputeHash(string inputPassword)
    {
        var encDataByte = System.Text.Encoding.UTF8.GetBytes(inputPassword);
        var encodedData = Convert.ToBase64String(encDataByte);
        return encodedData;
    }

    public async Task<int> ChangePassword(string inputCardNum, string inputOldPassword, string inputNewPassword,
        string inputConfirmNewPassword, DashboardResponse response, HttpClient client)
    {
        if (OnlyAdmin) return 0;
        foreach (var user in response.Users.Where(user => user.AccessCode == inputCardNum))
        {
            if (user.Password != ComputeHash(inputOldPassword)) return 4;
            if (inputNewPassword != inputConfirmNewPassword) return 2;
            var request = new SetPasswordRequest
            {
                AccessCode = user.AccessCode,
                Password = ComputeHash(inputNewPassword)
            };
            var responseMessage = await client.PostAsJsonAsync("api/Cards", request);
            return responseMessage.IsSuccessStatusCode ? 1 : 3;
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

    public int GetBaid()
    {
        return checked((int)Baid);
    }

    public string GetCardNum()
    {
        if (IsAdmin) return "Admin";
        return CardNum == 0 ? "Not logged in" : CardNum.ToString();
    }
}