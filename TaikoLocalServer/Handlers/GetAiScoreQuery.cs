using GameDatabase.Context;
using TaikoLocalServer.Mappers;
using TaikoLocalServer.Models.Application;
using Throw;

namespace TaikoLocalServer.Handlers;

public record GetAiScoreQuery(uint Baid, uint SongId, uint Level) : IRequest<CommonAiScoreResponse>;

public class GetAiScoreQueryHandler(TaikoDbContext context, ILogger<GetAiScoreQueryHandler> logger)
    : IRequestHandler<GetAiScoreQuery, CommonAiScoreResponse>
{
    public async Task<CommonAiScoreResponse> Handle(GetAiScoreQuery request, CancellationToken cancellationToken)
    {
        var difficulty = (Difficulty)request.Level;
        difficulty.Throw().IfOutOfRange();
        
        var aiData = await context.AiScoreData.Where(datum => datum.Baid == request.Baid &&
                                                             datum.SongId == request.SongId &&
                                                             datum.Difficulty == difficulty)
            .Include(datum => datum.AiSectionScoreData)
            .FirstOrDefaultAsync(cancellationToken);
        if (aiData is null)
        {
            return new CommonAiScoreResponse
            {
                Result = 1
            };
        }
        aiData.AiSectionScoreData.Sort((a, b) => a.SectionIndex.CompareTo(b.SectionIndex));
        return AiScoreMappers.MapAsSuccess(aiData);
    }
}