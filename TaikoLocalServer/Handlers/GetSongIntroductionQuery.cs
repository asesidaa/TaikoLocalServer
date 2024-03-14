using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Handlers;

public record GetSongIntroductionQuery(uint[] SetIds) : IRequest<CommonGetSongIntroductionResponse>;

public class GetSongIntroductionQueryHandler(IGameDataService gameDataService, ILogger<GetSongIntroductionQueryHandler> logger) 
    : IRequestHandler<GetSongIntroductionQuery, CommonGetSongIntroductionResponse>
{

    public Task<CommonGetSongIntroductionResponse> Handle(GetSongIntroductionQuery request, CancellationToken cancellationToken)
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
        
        return Task.FromResult(response);
    }
}

