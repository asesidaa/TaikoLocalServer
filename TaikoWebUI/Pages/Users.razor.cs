namespace TaikoWebUI.Pages;

public partial class Users
{
    private UsersResponse? response = new();
    
    private int TotalPages { get; set; } = 0;
    private bool isLoading = true;
    private int currentPage = 1;
    private int pageSize = 12;

    private async Task GetUsersData()
    {
        isLoading = true;
        response = await Client.GetFromJsonAsync<UsersResponse>($"api/Users?page={currentPage}&limit={pageSize}");
        response.ThrowIfNull();

        TotalPages = response.TotalPages;
        isLoading = false;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (AuthService.LoginRequired && !AuthService.IsLoggedIn)
        {
            await AuthService.LoginWithAuthToken();
        }
        
        if (AuthService.IsAdmin || !AuthService.LoginRequired)
        {
            await GetUsersData();
        }
    }
    
    private async Task OnPageChange(int page)
    {
        currentPage = page;
        await GetUsersData();
    }
}