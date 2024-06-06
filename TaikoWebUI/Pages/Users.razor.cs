namespace TaikoWebUI.Pages;

public partial class Users
{
    private UsersResponse? response = new();
    
    private CancellationTokenSource? cts;
    private int TotalPages { get; set; } = 0;
    private bool isLoading = true;
    private int currentPage = 1;
    private readonly int pageSize = 12;
    private string? searchTerm = null;

    private async Task GetUsersData()
    {
        isLoading = true;
        response = await Client.GetFromJsonAsync<UsersResponse>($"api/Users?page={currentPage}&limit={pageSize}&searchTerm={searchTerm}");
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

    private async Task Debounce(Func<Task> action, int delayInMilliseconds)
    {
        // Cancel the previous task
        cts?.Cancel();

        // Create a new CancellationTokenSource
        cts = new CancellationTokenSource();

        try
        {
            // Wait for the delay
            await Task.Delay(delayInMilliseconds, cts.Token);

            // Execute the action
            await action();
        }
        catch (TaskCanceledException)
        {
            // Ignore the exception
        }
    }
    
    private CancellationTokenSource? cts;

    private async Task Debounce(Func<Task> action, int delayInMilliseconds)
    {
        // Cancel the previous task
        cts?.Cancel();

        // Create a new CancellationTokenSource
        cts = new CancellationTokenSource();

        try
        {
            // Wait for the delay
            await Task.Delay(delayInMilliseconds, cts.Token);

            // Execute the action
            await action();
        }
        catch (TaskCanceledException)
        {
            // Ignore the exception
        }
    }
    
    private async Task OnSearch(string search)
    {
        searchTerm = search;
        currentPage = 1;

        // Debounce the GetUsersData method
        await Debounce(GetUsersData, 500); // 500 milliseconds delay
    }
}