
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
    
    private bool isLoading = true;
    
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
        
        response = await Client.GetFromJsonAsync<SongLeaderboardResponse>($"api/SongLeaderboard/{(uint)SongId}?baid={(uint)Baid}&difficulty={(uint)Difficulty}");
        response.ThrowIfNull();
        
        isLoading = false;
    }
    
    private async Task OnDifficultyChange(string difficulty)
    {
        isLoading = true;
        SelectedDifficulty = difficulty;
        Difficulty = Enum.Parse<Difficulty>(SelectedDifficulty);
        
        await LocalStorage.SetItemAsync("songPageDifficulty", SelectedDifficulty);
        
        response = await Client.GetFromJsonAsync<SongLeaderboardResponse>($"api/SongLeaderboard/{(uint)SongId}?baid={(uint)Baid}&difficulty={(uint)Difficulty}");
        response.ThrowIfNull();
        isLoading = false;
    }
}