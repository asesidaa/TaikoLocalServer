namespace TaikoLocalServer.Application.Handlers;

public partial class GetRecommendQueryHandler
{
    public partial ValueTask<CommonRecommendResponse> Handle(GetRecommendQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Green recommend requested for gender {GenderType}, age {PlayerAge}", request.GenderType, request.PlayerAge);
        var green = gameDataService.Green();
        return ValueTask.FromResult(new CommonRecommendResponse
        {
            Result = 1,
            RecommendSong = green.Recommend.RecommendSong,
            RecommendBestSong = green.Recommend.RecommendBestSongs.ToList()
        });
    }
}
