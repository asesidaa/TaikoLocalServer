using Throw;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetAiScoreQueryHandler
{
    private partial async ValueTask<CommonAiScoreResponse> HandleNijiiro(GetAiScoreQuery request, CancellationToken cancellationToken)
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
