using GameDatabase.Context;
using TaikoLocalServer.Mappers;
using TaikoLocalServer.Models.Application;
using Throw;

namespace TaikoLocalServer.Handlers;

public record GetAiScoreQuery(uint Baid, uint SongId, uint Level) : IRequest<CommonAiScoreResponse>;

public class GetAiScoreQueryHandler : IRequestHandler<GetAiScoreQuery, CommonAiScoreResponse>
{
    private readonly TaikoDbContext context;
    
    private readonly ILogger<GetAiScoreQueryHandler> logger;

    public GetAiScoreQueryHandler(TaikoDbContext context, ILogger<GetAiScoreQueryHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<CommonAiScoreResponse> Handle(GetAiScoreQuery request, CancellationToken cancellationToken)
    {
        var difficulty = (Difficulty)request.Level;
        difficulty.Throw().IfOutOfRange();
        
        var aiData = await context.AiScoreData.Where(datum => datum.Baid == request.Baid &&
                                                             datum.SongId == request.SongId &&
                                                             datum.Difficulty == difficulty)
            .Include(datum => datum.AiSectionScoreData)
            .FirstOrDefaultAsync(cancellationToken);
        return aiData is null ? new CommonAiScoreResponse() : AiScoreMappers.MapToCommonAiScoreResponse(aiData);
    }
}