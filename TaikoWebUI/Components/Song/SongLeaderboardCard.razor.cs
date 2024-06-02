

namespace TaikoWebUI.Components.Song;

public partial class SongLeaderboardCard
{
    private SongLeaderboardResponse? response;
    
    [Parameter]
    public int SongId { get; set; }
    
    [Parameter]
    public int Baid { get; set; }

    [Parameter] 
    public Difficulty Difficulty { get; set; } = Difficulty.Easy;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        response = await Client.GetFromJsonAsync<SongLeaderboardResponse>($"api/SongLeaderboardData/{(uint)Baid}/{(uint)SongId}/{(uint)Difficulty}");
        response.ThrowIfNull();
        
        // log the leaderboard
        Console.WriteLine(response.Leaderboard);
    }
}