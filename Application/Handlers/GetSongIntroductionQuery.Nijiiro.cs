namespace TaikoLocalServer.Application.Handlers;

public partial class GetSongIntroductionQueryHandler
{
    private partial ValueTask<CommonGetSongIntroductionResponse> HandleNijiiro(GetSongIntroductionQuery request, CancellationToken cancellationToken)
    {
        var response = new CommonGetSongIntroductionResponse
        {
            Result = 1
        };
        foreach (var setId in request.SetIds)
        {
            gameDataService.Nijiiro().GetSongIntroductionDictionary().TryGetValue(setId, out var introData);
            if (introData is null)
            {
                logger.LogWarning("Requested set id {Id} does not exist!", setId);
                continue;
            }

            response.ArySongIntroductionDatas.Add(introData);
        }
        
        return ValueTask.FromResult(response);
    }
}
