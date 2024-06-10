

using Microsoft.Extensions.Options;
using TaikoWebUI.Settings;

namespace TaikoWebUI.Components.Song;

public partial class SongLeaderboardCard
{
    [Inject]
    IOptions<WebUiSettings> UiSettings { get; set; } = default!;
    
    [Parameter]
    public int SongId { get; set; }
    
    [Parameter]
    public int Baid { get; set; }

    [Parameter] 
    public Difficulty Difficulty { get; set; } = Difficulty.None;
    
    private SongLeaderboardResponse? response = null;
    private List<SongLeaderboard> LeaderboardScores { get; set; } = new();
    private int TotalRows { get; set; } = 0;
    private string SelectedDifficulty { get; set; } = "None";
    private bool isPaginationEnabled = true;
    private int TotalPages { get; set; } = 0;
    private bool isLoading = true;
    private int currentPage = 1;
    private int pageSize = 10;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        if (UiSettings.Value.SongLeaderboardSettings.DisablePagination)
        {
            isPaginationEnabled = false;
        }

        if (UiSettings.Value.SongLeaderboardSettings.PageSize > 200 |
            UiSettings.Value.SongLeaderboardSettings.PageSize <= 0)
        {
            Console.WriteLine("Invalid LeaderboardSettings.PageSize value in appsettings.json. The value must be between 1 and 200. Defaulting to 10.");
        }
        
        if (UiSettings.Value.SongLeaderboardSettings.PageSize > 0 & UiSettings.Value.SongLeaderboardSettings.PageSize <= 200)
        {
            pageSize = UiSettings.Value.SongLeaderboardSettings.PageSize;
        }
    }
    
    private async Task GetLeaderboardData()
    {
        isLoading = true;
        response = await Client.GetFromJsonAsync<SongLeaderboardResponse>($"api/SongLeaderboard/{(uint)SongId}?baid={(uint)Baid}&difficulty={(uint)Difficulty}&page={currentPage}&limit={pageSize}");
        response.ThrowIfNull();
        
        LeaderboardScores.Clear();
        LeaderboardScores.AddRange(response.LeaderboardData);
        
        // set TotalPages
        TotalPages = response.TotalPages;
    
        if (response.UserScore != null 
            && LeaderboardScores.All(x => x.Baid != response.UserScore.Baid) 
            && (LeaderboardScores.Count == 0 || response.UserScore.Rank >= LeaderboardScores[0].Rank))
        {
            LeaderboardScores.Add(new SongLeaderboard()); // Add an empty row
            LeaderboardScores.Add(response.UserScore);
        }

        TotalRows = LeaderboardScores.Count;
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