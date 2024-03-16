using GameDatabase.Context;
using TaikoLocalServer.Models.Application;
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
        
        var selfbestScores = await context.SongBestData
            .Where(datum => datum.Baid == request.Baid && 
                            requestSet.Contains(datum.SongId) &&
                            (datum.Difficulty == requestDifficulty || 
                             (datum.Difficulty == Difficulty.UraOni && requestDifficulty == Difficulty.Oni)))
            .OrderBy(datum => datum.SongId)
            .ToListAsync(cancellationToken);

        var response = new CommonSelfBestResponse
        {
            Result = 1,
            Level = request.Difficulty,
            ArySelfbestScores = selfbestScores.ConvertAll(datum => new CommonSelfBestResponse.SelfBestData
            {
                SongNo = datum.SongId,
                SelfBestScore = datum.BestScore,
                UraBestScore = datum.Difficulty == Difficulty.UraOni ? datum.BestScore : 0,
                SelfBestScoreRate = datum.BestRate,
                UraBestScoreRate = datum.Difficulty == Difficulty.UraOni ? datum.BestRate : 0
            })
        };

        return response;
    }
}