using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TaikoWebUI.Settings;

namespace TaikoWebUI.Services;

public class LoginService
{
    private readonly string adminPassword;
    private readonly string adminUsername;
    public bool LoginRequired { get; }
    public bool OnlyAdmin { get; }
    private int BoundAccessCodeUpperLimit;

    public LoginService(IOptions<WebUiSettings> settings)
    {
        IsLoggedIn = false;
        IsAdmin = false;
        var webUiSettings = settings.Value;
        adminUsername = webUiSettings.AdminUsername;
        adminPassword = webUiSettings.AdminPassword;
        LoginRequired = webUiSettings.LoginRequired;
        OnlyAdmin = webUiSettings.OnlyAdmin;
        BoundAccessCodeUpperLimit = webUiSettings.BoundAccessCodeUpperLimit;
    }

    public bool IsLoggedIn { get; private set; }
    private User LoggedInUser { get; set; } = new();
    public bool IsAdmin { get; private set; }

    public int Login(string inputCardNum, string inputPassword, DashboardResponse response)
    {
        if (inputCardNum == adminUsername && inputPassword == adminPassword)
        {
            IsLoggedIn = true;
            IsAdmin = true;
            return 1;
        }

        if (OnlyAdmin) return 0;

        foreach (var user in response.Users.Where(user => user.AccessCodes.Contains(inputCardNum)))
        {
            foreach (var userCredential in response.UserCredentials.Where(userCredential => userCredential.Baid == user.Baid))
            {
                if (userCredential.Password == "") return 4;
                if (ComputeHash(inputPassword, userCredential.Salt) != userCredential.Password) return 2;
                IsLoggedIn = true;
                LoggedInUser = user;
                IsAdmin = false;
                return 1;
            }
        }
        return 3;
    }

    public async Task<int> Register(string inputCardNum, string inputPassword, string inputConfirmPassword,
        DashboardResponse response, HttpClient client)
    {
        if (OnlyAdmin) return 0;
        foreach (var user in response.Users.Where(user => user.AccessCodes.Contains(inputCardNum)))
        {
            foreach (var userCredential in response.UserCredentials.Where(userCredential => userCredential.Baid == user.Baid))
            {
                if (userCredential.Password != "") return 4;
                if (inputPassword != inputConfirmPassword) return 2;
                var salt = CreateSalt();
                var request = new SetPasswordRequest
                {
                    Baid = user.Baid,
                    Password = ComputeHash(inputPassword, salt),
                    Salt = salt
                };
                var responseMessage = await client.PostAsJsonAsync("api/Credentials", request);
                return responseMessage.IsSuccessStatusCode ? 1 : 3;
            }
        }

        return 3;
    }

    private static string CreateSalt()
    {
        //Generate a cryptographic random number.
        var randomNumber = new byte[32];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var salt = Convert.ToBase64String(randomNumber);

        // Return a Base64 string representation of the random number.
        return salt;
    }

    private static string ComputeHash(string inputPassword, string salt)
    {
        var encDataByte = Encoding.UTF8.GetBytes(inputPassword + salt);
        var encodedData = Convert.ToBase64String(encDataByte);
        encDataByte = Encoding.UTF8.GetBytes(encodedData);
        encodedData = Convert.ToBase64String(encDataByte);
        encDataByte = Encoding.UTF8.GetBytes(encodedData);
        encodedData = Convert.ToBase64String(encDataByte);
        encDataByte = Encoding.UTF8.GetBytes(encodedData);
        encodedData = Convert.ToBase64String(encDataByte);
        return encodedData;
    }

    public async Task<int> ChangePassword(string inputCardNum, string inputOldPassword, string inputNewPassword,
        string inputConfirmNewPassword, DashboardResponse response, HttpClient client)
    {
        if (OnlyAdmin) return 0;
        foreach (var user in response.Users.Where(user => user.AccessCodes.Contains(inputCardNum)))
        {
            foreach (var userCredential in response.UserCredentials.Where(userCredential => userCredential.Baid == user.Baid))
            {
                if (userCredential.Password != ComputeHash(inputOldPassword, userCredential.Salt)) return 4;
                if (inputNewPassword != inputConfirmNewPassword) return 2;
                var request = new SetPasswordRequest
                {
                    Baid = user.Baid,
                    Password = ComputeHash(inputNewPassword, userCredential.Salt),
                    Salt = userCredential.Salt
                };
                var responseMessage = await client.PostAsJsonAsync("api/Credentials", request);
                return responseMessage.IsSuccessStatusCode ? 1 : 3;
            }
        }

        return 3;
    }

    public void Logout()
    {
        IsLoggedIn = false;
        LoggedInUser = new User();
        IsAdmin = false;
    }

    public User GetLoggedInUser()
    {
        return LoggedInUser;
    }
    
    public void ResetLoggedInUser(DashboardResponse? response)
    {
        if (response is null) return;
        var baid = LoggedInUser.Baid;
        var newLoggedInUser = response.Users.FirstOrDefault(u => u.Baid == baid);
        if (newLoggedInUser is null) return;
        LoggedInUser = newLoggedInUser;
    }
    
    public async Task<int> BindAccessCode(string inputAccessCode, HttpClient client)
    {
        if (!IsLoggedIn) return 0;
        if (LoggedInUser.AccessCodes.Count >= BoundAccessCodeUpperLimit) return 2;
        var request = new BindAccessCodeRequest
        {
            AccessCode = inputAccessCode,
            Baid = LoggedInUser.Baid
        };
        var responseMessage = await client.PostAsJsonAsync("api/Cards", request);
        return responseMessage.IsSuccessStatusCode ? 1 : 3;
    }
}