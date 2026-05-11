using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetAiScoreQuery(uint Baid, uint SongId, uint Level) : IRequest<CommonAiScoreResponse>;

#pragma warning disable CS9113 // Parameter is unread.
public class GetAiScoreQueryHandler(ITaikoDbContext context, ILogger<GetAiScoreQueryHandler> logger)
#pragma warning restore CS9113 // Parameter is unread.
    : IRequestHandler<GetAiScoreQuery, CommonAiScoreResponse>
{
    public async ValueTask<CommonAiScoreResponse> Handle(GetAiScoreQuery request, CancellationToken cancellationToken)
    {
        var difficulty = (Difficulty)request.Level;
        difficulty.Throw().IfOutOfRange();

        var aiData = await context.AiScoreDataNijiiro.Where(datum => datum.Baid == request.Baid &&
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
        return new CommonAiScoreResponse
        {
            Result = 1,
            AryBestSectionDatas = aiData.AiSectionScoreData
                .Select(section => new CommonAiBestSectionData
                {
                    SectionIndex = (uint)section.SectionIndex,
                    Crown = (uint)section.Crown,
                    Score = section.Score,
                    GoodCount = section.GoodCount,
                    OkCount = section.OkCount,
                    MissCount = section.MissCount,
                    DrumrollCount = section.DrumrollCount,
                })
                .ToList()
        };
    }
}