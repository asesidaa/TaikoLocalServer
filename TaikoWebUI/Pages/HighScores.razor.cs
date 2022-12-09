namespace TaikoWebUI.Pages;

public partial class HighScores
{
    private const string IconStyle = "width:25px; height:25px;";

    private readonly List<BreadcrumbItem> breadcrumbs = new()
    {
        new BreadcrumbItem("Cards", "/Cards")
    };

    private SongBestResponse? response;

    private Dictionary<Difficulty, List<SongBestData>> songBestDataMap = new();

    [Parameter] public int Baid { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<SongBestResponse>($"api/PlayData/{Baid}");
        response.ThrowIfNull();

        response.SongBestData.ForEach(data =>
        {
            var songId = data.SongId;
            data.Genre = GameDataService.GetMusicGenreBySongId(songId);
            data.MusicName = GameDataService.GetMusicNameBySongId(songId);
            data.MusicArtist = GameDataService.GetMusicArtistBySongId(songId);
        });

        songBestDataMap = response.SongBestData.GroupBy(data => data.Difficulty)
            .ToDictionary(data => data.Key,
                data => data.ToList());
        foreach (var songBestDataList in songBestDataMap.Values)
            songBestDataList.Sort((data1, data2) => GameDataService.GetMusicIndexBySongId(data1.SongId)
                .CompareTo(GameDataService.GetMusicIndexBySongId(data2.SongId)));


        breadcrumbs.Add(new BreadcrumbItem($"Card: {Baid}", null, true));
        breadcrumbs.Add(new BreadcrumbItem("High Scores", $"/Cards/{Baid}/HighScores"));
    }

    private async Task OnFavoriteToggled(SongBestData data)
    {
        var request = new SetFavoriteRequest
        {
            Baid = (uint)Baid,
            IsFavorite = !data.IsFavorite,
            SongId = data.SongId
        };
        var result = await Client.PostAsJsonAsync("api/FavoriteSongs", request);
        if (result.IsSuccessStatusCode) data.IsFavorite = !data.IsFavorite;
    }

    private static string GetCrownText(CrownType crown)
    {
        return crown switch
        {
            CrownType.None => "Fail",
            CrownType.Clear => "Clear",
            CrownType.Gold => "Full Combo",
            CrownType.Dondaful => "Donderful Combo",
            _ => ""
        };
    }

    private static string GetRankText(ScoreRank rank)
    {
        return rank switch
        {
            ScoreRank.White => "Stylish",
            ScoreRank.Bronze => "Stylish",
            ScoreRank.Silver => "Stylish",
            ScoreRank.Gold => "Graceful",
            ScoreRank.Sakura => "Graceful",
            ScoreRank.Purple => "Graceful",
            ScoreRank.Dondaful => "Top Class",
            _ => ""
        };
    }

    private static string GetDifficultyTitle(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => "Easy",
            Difficulty.Normal => "Normal",
            Difficulty.Hard => "Hard",
            Difficulty.Oni => "Oni",
            Difficulty.UraOni => "Ura Oni",
            _ => ""
        };
    }

    private static string GetDifficultyIcon(Difficulty difficulty)
    {
        return $"<image href='/images/difficulty_{difficulty}.png' alt='{difficulty}' width='24' height='24'/>";
    }

    private static string GetGenreTitle(SongGenre genre)
    {
        return genre switch
        {
            SongGenre.Pop => "Pop",
            SongGenre.Anime => "Anime",
            SongGenre.Kids => "Kids",
            SongGenre.Vocaloid => "Vocaloid",
            SongGenre.GameMusic => "Game Music",
            SongGenre.NamcoOriginal => "NAMCO Original",
            SongGenre.Variety => "Variety",
            SongGenre.Classical => "Classical",
            _ => ""
        };
    }

    private static string GetGenreStyle(SongGenre genre)
    {
        return genre switch
        {
            SongGenre.Pop => "background: #42c0d2; color: #fff",
            SongGenre.Anime => "background: #ff90d3; color: #fff",
            SongGenre.Kids => "background: #fec000; color: #fff",
            SongGenre.Vocaloid => "background: #ddd",
            SongGenre.GameMusic => "background: #cc8aea; color: #fff",
            SongGenre.NamcoOriginal => "background: #ff7027; color: #fff",
            SongGenre.Variety => "background: #1dc83b; color: #fff",
            SongGenre.Classical => "background: #bfa356",
            _ => ""
        };
    }

    private static void ToggleShowAiData(SongBestData data)
    {
        data.ShowAiData = !data.ShowAiData;
    }

    private static bool IsAiDataPresent(SongBestData data)
    {
        var aiData = data.AiSectionBestData;

        return aiData.Count > 0;
    }
}