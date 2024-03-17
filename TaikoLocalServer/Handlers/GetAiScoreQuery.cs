using GameDatabase.Context;
using TaikoLocalServer.Mappers;
using Throw;

namespace TaikoLocalServer.Handlers;

public record GetAiScoreQuery(uint Baid, uint SongId, uint Level) : IRequest<CommonAiScoreResponse>;

#pragma warning disable CS9113 // Parameter is unread.
public class GetAiScoreQueryHandler(TaikoDbContext context, ILogger<GetAiScoreQueryHandler> logger)
#pragma warning restore CS9113 // Parameter is unread.
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