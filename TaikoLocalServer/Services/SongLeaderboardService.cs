using GameDatabase.Context;
using SharedProject.Models;

namespace TaikoLocalServer.Services;

public class SongLeaderboardService : ISongLeaderboardService
{
    private readonly TaikoDbContext context;

    public SongLeaderboardService(TaikoDbContext context)
    {
        this.context = context;
    }

    public async Task<(List<SongLeaderboard>, SongLeaderboard?, int, int)> GetSongLeaderboard(uint songId, Difficulty difficulty, int baid = 0,
        int page = 1, int limit = 10)
    {
        // Get the total count of scores for the song
        var totalScores = await context.SongBestData
            .Where(x => x.SongId == songId && x.Difficulty == difficulty)
            .CountAsync();

        // Calculate the total pages
        var totalPages = totalScores / limit;

        // If there is a remainder, add one to the total pages
        if (totalScores % limit > 0)
        {
            totalPages++;
        }
        
        // get all scores for the song from every user
        var scores = await context.SongBestData
            .Where(x => x.SongId == songId && x.Difficulty == difficulty)
            .OrderByDescending(x => x.BestScore)
            .ThenByDescending(x => x.BestRate)
            .ThenByDescending(x => x.BestCrown)
            .Skip((page - 1) * limit) // Subtract 1 because page numbers now start at 1
            .Take(limit)
            .ToListAsync();

        // get the user data for each score
        var leaderboard = new List<SongLeaderboard>();

        // get the user data for each score
        foreach (var score in scores)
        {
            var user = await context.UserData
                .Where(x => x.Baid == score.Baid)
                .FirstOrDefaultAsync();
            
            // calculate Rank based on page/limit
            var rank = await context.SongBestData
                .Where(x => x.SongId == songId && x.Difficulty == difficulty && x.BestScore > score.BestScore)
                .CountAsync();

            leaderboard.Add(new SongLeaderboard
            {
                Rank = rank + 1,
                Baid = score.Baid,
                UserName = user?.MyDonName,
                BestScore = score.BestScore,
                BestRate = score.BestRate,
                BestCrown = score.BestCrown,
                BestScoreRank = score.BestScoreRank
            });
        }
        
        // Get UserScore if baid is provided
        SongLeaderboard? userBestScore = null;
        if (baid != 0)
        {
            var score = await context.SongBestData
                .Where(x => x.SongId == songId && x.Difficulty == difficulty && x.Baid == baid)
                .FirstOrDefaultAsync();

            if (score != null)
            {
                var user = await context.UserData
                    .Where(x => x.Baid == baid)
                    .FirstOrDefaultAsync();

                var rank = await context.SongBestData
                    .Where(x => x.SongId == songId && x.Difficulty == difficulty && x.BestScore > score.BestScore)
                    .CountAsync();

                userBestScore = new SongLeaderboard
                {
                    Rank = rank + 1,
                    Baid = score.Baid,
                    UserName = user?.MyDonName,
                    BestScore = score.BestScore,
                    BestRate = score.BestRate,
                    BestCrown = score.BestCrown,
                    BestScoreRank = score.BestScoreRank
                };
            }
        }

        return (leaderboard, userBestScore, totalPages, totalScores);
    }
}