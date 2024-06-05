
namespace TaikoWebUI.Components.Song;

public partial class SongLeaderboardCard
{
    private SongLeaderboardResponse? response = null;
    
    [Parameter]
    public int SongId { get; set; }
    
    [Parameter]
    public int Baid { get; set; }

    [Parameter] 
    public Difficulty Difficulty { get; set; } = Difficulty.None;

    private string SelectedDifficulty { get; set; } = "None";
    private int TotalPages { get; set; } = 0;
    
    private bool isLoading = true;
    private int currentPage = 1;
    private int pageSize = 10;
    
    private async Task GetLeaderboardData()
    {
        isLoading = true;
        response = await Client.GetFromJsonAsync<SongLeaderboardResponse>($"api/SongLeaderboard/{(uint)SongId}?baid={(uint)Baid}&difficulty={(uint)Difficulty}&page={currentPage}&limit={pageSize}");
        response.ThrowIfNull();
        
        TotalPages = response.TotalPages;
        isLoading = false;
    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        // get highScoresTab from LocalStorage
        var songPageDifficulty = await LocalStorage.GetItemAsync<string>("songPageDifficulty");
        
        if (songPageDifficulty != null)
        {
            SelectedDifficulty = songPageDifficulty;
            Difficulty = Enum.Parse<Difficulty>(SelectedDifficulty);
        } 
        else
        {
            // set default difficulty to Easy
            SelectedDifficulty = Difficulty.Easy.ToString();
            Difficulty = Difficulty.Easy;
        }
        
        await GetLeaderboardData();
        
        isLoading = false;
    }
    
    private async Task OnDifficultyChange(string difficulty = "None")
    {
        isLoading = true;
        SelectedDifficulty = difficulty;
        Difficulty = Enum.Parse<Difficulty>(SelectedDifficulty);
        
        await LocalStorage.SetItemAsync("songPageDifficulty", SelectedDifficulty);
        await GetLeaderboardData();
        
        currentPage = 1;
        isLoading = false;
    }
    
    private async Task OnPageChange(int page)
    {
        currentPage = page;
        await GetLeaderboardData();
    }

    private string GetActiveRowClass(SongLeaderboard leaderboard, int index)
    {
        return leaderboard.Baid == Baid ? "is-current-user" : "";
    }
}