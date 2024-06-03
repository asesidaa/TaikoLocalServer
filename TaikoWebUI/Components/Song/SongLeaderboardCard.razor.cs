
namespace TaikoWebUI.Components.Song;

public partial class SongLeaderboardCard
{
    private SongLeaderboardResponse? response = null;
    private const string IconStyle = "width:25px; height:25px;";
    
    [Parameter]
    public int SongId { get; set; }
    
    [Parameter]
    public int Baid { get; set; }

    [Parameter] 
    public Difficulty Difficulty { get; set; } = Difficulty.Hard;

    private string SelectedDifficulty { get; set; } = "Hard";
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        response = await Client.GetFromJsonAsync<SongLeaderboardResponse>($"api/SongLeaderboardData/{(uint)Baid}/{(uint)SongId}/{(uint)Difficulty}");
        response.ThrowIfNull();
    }
    
    private async Task OnDifficultyChange(string difficulty)
    {
        SelectedDifficulty = difficulty;
        Difficulty = Enum.Parse<Difficulty>(SelectedDifficulty);
        response = await Client.GetFromJsonAsync<SongLeaderboardResponse>($"api/SongLeaderboardData/{(uint)Baid}/{(uint)SongId}/{(uint)Difficulty}");
        response.ThrowIfNull();
    }
}