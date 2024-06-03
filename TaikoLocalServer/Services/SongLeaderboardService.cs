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

    public async Task<List<SongLeaderboard>> GetSongLeaderboard(uint songId, Difficulty difficulty, int baid,
        int limit = 10)
    {
        if (baid == 0)
        {
            throw new ArgumentNullException(nameof(baid));
        }

        // get all scores for the song from every user
        var scores = await context.SongBestData
            .Where(x => x.SongId == songId && x.Difficulty == difficulty)
            .OrderByDescending(x => x.BestScore)
            .ThenByDescending(x => x.BestRate)
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

            leaderboard.Add(new SongLeaderboard
            {
                Rank = (uint)leaderboard.Count + 1,
                Baid = score.Baid,
                UserName = user?.MyDonName,
                BestScore = score.BestScore,
                BestRate = score.BestRate,
                BestCrown = score.BestCrown,
                BestScoreRank = score.BestScoreRank
            });
        }

        // get current user score if it exists
        var currentUserScore = await context.SongBestData
            .Where(x => x.SongId == songId && x.Difficulty == difficulty && x.Baid == baid)
            .FirstOrDefaultAsync();

        if (currentUserScore != null)
        {
            // check if they are in the limit
            var userRank = leaderboard.FindIndex(x => x.Baid == baid);

            if (userRank >= limit)
            {
                // get the user data for the current user
                var user = await context.UserData
                    .Where(x => x.Baid == baid)
                    .FirstOrDefaultAsync();

                leaderboard.Add(new SongLeaderboard
                {
                    Rank = (uint)userRank + 1,
                    Baid = currentUserScore.Baid,
                    UserName = user?.MyDonName,
                    BestScore = currentUserScore.BestScore,
                    BestRate = currentUserScore.BestRate,
                    BestCrown = currentUserScore.BestCrown,
                    BestScoreRank = currentUserScore.BestScoreRank
                });
            }
        }

        return leaderboard;
    }
}