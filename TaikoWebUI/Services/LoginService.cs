using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TaikoWebUI.Settings;
using Blazored.LocalStorage;

namespace TaikoWebUI.Services;

public class LoginService
{
	public event EventHandler? LoginStatusChanged;
    public delegate void LoginStatusChangedEventHandler(object? sender, EventArgs e);
    public bool LoginRequired { get; }
    public bool OnlyAdmin { get; }
    private readonly int boundAccessCodeUpperLimit;
    public bool RegisterWithLastPlayTime { get; }
    public bool AllowUserDelete { get; }
    public bool AllowFreeProfileEditing { get; }
    public bool IsLoggedIn { get; private set; }
    private User LoggedInUser { get; set; } = new();
    public bool IsAdmin { get; private set; }
    private readonly ILocalStorageService localStorage;
    
    public LoginService(IOptions<WebUiSettings> settings, ILocalStorageService localStorage)
    {
        this.localStorage = localStorage;
        IsLoggedIn = false;
        IsAdmin = false;
        var webUiSettings = settings.Value;
        LoginRequired = webUiSettings.LoginRequired;
        OnlyAdmin = webUiSettings.OnlyAdmin;
        boundAccessCodeUpperLimit = webUiSettings.BoundAccessCodeUpperLimit;
        RegisterWithLastPlayTime = webUiSettings.RegisterWithLastPlayTime;
        AllowUserDelete = webUiSettings.AllowUserDelete;
        AllowFreeProfileEditing = webUiSettings.AllowFreeProfileEditing;
    }
    
    protected virtual void OnLoginStatusChanged()
    {
        LoginStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<int> Login(string inputAccessCode, string inputPassword, HttpClient client)
    {
        // strip spaces or dashes from card number
        inputAccessCode = inputAccessCode.Replace(" ", "").Replace("-", "").Replace(":", "");

        var request = new LoginRequest
        {
            AccessCode = inputAccessCode,
            Password = inputPassword
        };
        
        var responseMessage = await client.PostAsJsonAsync("api/Auth/Login", request);

        if (!responseMessage.IsSuccessStatusCode)
        {
            // Unauthorized, extract specific error message as json
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
            // Unknown error message
            if (responseJson is null) return 5;
            var errorMessage = responseJson["message"];
            return errorMessage switch
            {
                "Access Code Not Found" => 3,
                "User Not Registered" => 4,
                "Invalid Password" => 2,
                _ => 5
            };
        }
        else
        {
            // Authorized, store Jwt token
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
            if (responseJson is null) return 5;
            
            var authToken = responseJson["authToken"];
            await localStorage.SetItemAsync("authToken", authToken);
            
            return await LoginWithAuthToken(authToken, client) == false ? 5 : 1;
        }
    }

    public async Task<bool> LoginWithAuthToken(string authToken, HttpClient client)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(authToken);
        
        // Check whether token is expired
        if (jwtSecurityToken.ValidTo < DateTime.UtcNow) return false;
        
        var baid = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;
        var isAdmin = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value == "Admin";
        
        var response = await client.GetFromJsonAsync<DashboardResponse>("api/Dashboard");

        var user = response?.Users.FirstOrDefault(u => u.Baid == uint.Parse(baid));
        if (user is null) return false;
        
        IsLoggedIn = true;
        IsAdmin = isAdmin;
        LoggedInUser = user;
        OnLoginStatusChanged();
        return true;
    }

    public async Task<int> Register(string inputCardNum, DateTime inputDateTime, string inputPassword,
        string inputConfirmPassword,
        DashboardResponse response, HttpClient client, string inviteCode)
    {
        if (OnlyAdmin) return 0;

        if (inputPassword != inputConfirmPassword) return 2;

        // strip spaces or dashes from card number
        inputCardNum = inputCardNum.Replace(" ", "").Replace("-", "").Replace(":", "");

        var request = new RegisterRequest
        {
            AccessCode = inputCardNum,
            Password = inputPassword,
            RegisterWithLastPlayTime = RegisterWithLastPlayTime,
            LastPlayDateTime = inputDateTime,
            InviteCode = inviteCode
        };

        var responseMessage = await client.PostAsJsonAsync("api/Auth/Register", request);

        if (responseMessage.IsSuccessStatusCode) return 1;
        
        // Unauthorized, extract specific error message as json
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
        // Unknown error message
        if (responseJson is null) return 6;
        var errorMessage = responseJson["message"];
        return errorMessage switch
        {
            "Access Code Not Found" => 3,
            "User Already Registered" => 4,
            "Wrong Last Play Time" => 5,
            _ => 6
        };
    }
    
    public async Task<int> ChangePassword(string inputAccessCode, string inputOldPassword, string inputNewPassword,
        string inputConfirmNewPassword, DashboardResponse response, HttpClient client)
    {
        if (OnlyAdmin) return 0;
        
        if (inputNewPassword != inputConfirmNewPassword) return 2;
        
        var request = new ChangePasswordRequest
        {
            AccessCode = inputAccessCode,
            OldPassword = inputOldPassword,
            NewPassword = inputNewPassword
        };
        
        var responseMessage = await client.PostAsJsonAsync("api/Auth/ChangePassword", request);
        
        if (responseMessage.IsSuccessStatusCode) return 1;
        
        // Unauthorized, extract specific error message as json
        var responseContent = await responseMessage.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
        // Unknown error message
        if (responseJson is null) return 6;
        var errorMessage = responseJson["message"];
        return errorMessage switch
        {
            "Access Code Not Found" => 3,
            "User Not Registered" => 5,
            "Wrong Old Password" => 4,
            _ => 6
        };
    }

    public async Task Logout()
    {
        IsLoggedIn = false;
        LoggedInUser = new User();
        IsAdmin = false;
        
        // Clear JWT token
        await localStorage.RemoveItemAsync("authToken");
        OnLoginStatusChanged();
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

    public async Task<int> BindAccessCode(string inputAccessCode, User user, HttpClient client)
    {
        if (inputAccessCode.Trim() == "") return 4; /*Empty access code*/
        if (!IsLoggedIn && LoginRequired) return 0; /*User not connected and login is required*/
        if (LoginRequired && !IsAdmin && user.Baid != GetLoggedInUser().Baid) return 5; /*User not admin trying to update someone elses Access Codes*/
        if (user.AccessCodes.Count >= boundAccessCodeUpperLimit) return 2; /*Limit of codes has been reached*/
        var request = new BindAccessCodeRequest
        {
            AccessCode = inputAccessCode,
            Baid = user.Baid
        };
        var responseMessage = await client.PostAsJsonAsync("api/Cards", request);
        return responseMessage.IsSuccessStatusCode ? 1 : 3;
    }
}