using GameDatabase.Context;
using Throw;

namespace TaikoLocalServer.Handlers;

public record GetSelfBestQuery(uint Baid, uint Difficulty, uint[] SongIdList) : IRequest<CommonSelfBestResponse>;

public class GetSelfBestQueryHandler(IGameDataService gameDataService, TaikoDbContext context, ILogger<GetSelfBestQueryHandler> logger)
    : IRequestHandler<GetSelfBestQuery, CommonSelfBestResponse>
{
    public async Task<CommonSelfBestResponse> Handle(GetSelfBestQuery request, CancellationToken cancellationToken)
    {
        var requestDifficulty = (Difficulty)request.Difficulty;
        requestDifficulty.Throw().IfOutOfRange();

        var allSongSet = gameDataService.GetMusicList().ToHashSet();
        var requestSet = request.SongIdList.ToHashSet();
        if (!requestSet.IsSubsetOf(allSongSet))
        {
            var invalidSongIds = requestSet.Except(allSongSet);
            logger.LogWarning("Invalid song IDs: {InvalidSongIds}", invalidSongIds.Stringify());
            requestSet.ExceptWith(invalidSongIds);
        }

        var selfBestScores = await context.SongBestData
            .Where(datum => datum.Baid == request.Baid &&
                            requestSet.Contains(datum.SongId) &&
                            (datum.Difficulty == requestDifficulty ||
                             (datum.Difficulty == Difficulty.UraOni && requestDifficulty == Difficulty.Oni)))
            .ToListAsync(cancellationToken);
        var selfBestList = new List<CommonSelfBestResponse.SelfBestData>();
        foreach (var songId in request.SongIdList)
        {
            var selfBest = new CommonSelfBestResponse.SelfBestData();
            var selfBestScore = selfBestScores
                .FirstOrDefault(datum => datum.SongId == songId &&
                                         datum.Difficulty == requestDifficulty);
            var uraSelfBestScore = selfBestScores
                .FirstOrDefault(datum => datum.SongId == songId &&
                                         datum.Difficulty == Difficulty.UraOni && requestDifficulty == Difficulty.Oni);

            selfBest.SongNo = songId;
            if (selfBestScore is not null)
            {
                selfBest.SelfBestScore = selfBestScore.BestScore;
                selfBest.SelfBestScoreRate = selfBestScore.BestRate;
            }
            if (uraSelfBestScore is not null)
            {
                selfBest.UraBestScore = uraSelfBestScore.BestScore;
                selfBest.UraBestScoreRate = uraSelfBestScore.BestRate;
            }

            selfBestList.Add(selfBest);
        }

        var response = new CommonSelfBestResponse
        {
            Result = 1,
            Level = request.Difficulty,
            ArySelfbestScores = selfBestList
        };

        return response;
    }
}