
namespace TaikoWebUI.Utilities
{
    public class ScoreUtils
    {
        public static string GetCrownText(CrownType crown)
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

        public static string GetRankText(ScoreRank rank)
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

        public static string GetDifficultyTitle(Difficulty difficulty)
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

        public static string GetDifficultyIcon(Difficulty difficulty)
        {
            return $"<image href='/images/difficulty_{difficulty}.png' alt='{difficulty}' width='24' height='24'/>";
        }

        public static string GetGenreTitle(SongGenre genre)
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

        public static string GetGenreStyle(SongGenre genre)
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

        public static bool IsAiDataPresent(SongBestData data)
        {
            var aiData = data.AiSectionBestData;

            return aiData.Count > 0;
        }
    }
}