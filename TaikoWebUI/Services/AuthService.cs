using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TaikoWebUI.Settings;
using Blazored.LocalStorage;

namespace TaikoWebUI.Services;

public sealed class AuthService
{
    public event EventHandler? LoginStatusChanged;
    public bool LoginRequired { get; }
    public bool OnlyAdmin { get; }
    private readonly int boundAccessCodeUpperLimit;
    public bool RegisterWithLastPlayTime { get; }
    public bool AllowUserDelete { get; }
    public bool AllowFreeProfileEditing { get; }
    public bool IsLoggedIn { get; private set; }
    private uint LoggedInBaid { get; set; }
    public bool IsAdmin { get; private set; }
    private readonly ILocalStorageService localStorage;
    private readonly HttpClient client;

    public AuthService(IOptions<WebUiSettings> settings, ILocalStorageService localStorage, HttpClient client)
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
        this.client = client;
    }

    private void OnLoginStatusChanged()
    {
        LoginStatusChanged?.Invoke(this, EventArgs.Empty);
    }
    
    private static (uint, bool) GetBaidAndIsAdminFromToken(string authToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(authToken);
        var baid = uint.Parse(jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Name).Value);
        var isAdmin = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value == "Admin";
        return (baid, isAdmin);
    }

    public async Task<int> Login(string inputAccessCode, string inputPassword)
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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            
            var (baid, isAdmin) = GetBaidAndIsAdminFromToken(authToken);
            IsLoggedIn = true;
            IsAdmin = isAdmin;
            LoggedInBaid = baid;
            OnLoginStatusChanged();
            return 1;
        }
    }

    public async Task LoginWithAuthToken()
    {
        var hasAuthToken = await localStorage.ContainKeyAsync("authToken");
        if (!hasAuthToken) return;
        
        // Attempt to get JWT token from local storage
        var authToken = await localStorage.GetItemAsync<string>("authToken");
        if (authToken == null) return;
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        var responseMessage = await client.PostAsync("api/Auth/LoginWithToken", null);
        if (!responseMessage.IsSuccessStatusCode)
        {
            // Clear JWT token
            await localStorage.RemoveItemAsync("authToken");
            return;
        }
        
        var (baid, isAdmin) = GetBaidAndIsAdminFromToken(authToken);

        IsLoggedIn = true;
        IsAdmin = isAdmin;
        LoggedInBaid = baid;
        OnLoginStatusChanged();
    }

    public async Task<int> Register(string inputCardNum, DateTime inputDateTime, string inputPassword,
        string inputConfirmPassword, string inviteCode)
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
        string inputConfirmNewPassword)
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
        LoggedInBaid = 0;
        IsAdmin = false;

        // Clear JWT token
        await localStorage.RemoveItemAsync("authToken");
        OnLoginStatusChanged();
    }

    public async Task<User?> GetLoggedInUser()
    {
        return await client.GetFromJsonAsync<User>($"api/Users/{LoggedInBaid}");
    }
    
    public uint GetLoggedInBaid()
    {
        return LoggedInBaid;
    }

    public async Task<int> BindAccessCode(string inputAccessCode, User user)
    {
        if (inputAccessCode.Trim() == "") return 4; /*Empty access code*/
        if (!IsLoggedIn && LoginRequired) return 0; /*User not connected and login is required*/
        var loggedInUser = await GetLoggedInUser();
        if (loggedInUser == null) return 0;
        if (LoginRequired && !IsAdmin && user.Baid != loggedInUser.Baid) return 5; /*User not admin trying to update someone else's Access Codes*/
        if (user.AccessCodes.Count >= boundAccessCodeUpperLimit) return 2; /*Limit of codes has been reached*/

        var request = new BindAccessCodeRequest
        {
            AccessCode = inputAccessCode,
            Baid = user.Baid
        };
        var responseMessage = await client.PostAsJsonAsync("api/Cards/BindAccessCode", request);
        return responseMessage.IsSuccessStatusCode ? 1 : 3;
    }
}
