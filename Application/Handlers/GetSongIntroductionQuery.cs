namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetSongIntroductionQuery(uint[] SetIds) : IRequest<CommonGetSongIntroductionResponse>;

public class GetSongIntroductionQueryHandler(IGameDataCatalog gameDataService, ILogger<GetSongIntroductionQueryHandler> logger) 
    : IRequestHandler<GetSongIntroductionQuery, CommonGetSongIntroductionResponse>
{

    public ValueTask<CommonGetSongIntroductionResponse> Handle(GetSongIntroductionQuery request, CancellationToken cancellationToken)
    {
        var response = new CommonGetSongIntroductionResponse
        {
            Result = 1
        };
        foreach (var setId in request.SetIds)
        {
            gameDataService.GetSongIntroductionDictionary().TryGetValue(setId, out var introData);
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

