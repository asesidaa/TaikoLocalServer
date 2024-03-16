using GameDatabase.Context;
using GameDatabase.Entities;
using TaikoLocalServer.Models.Application;
using Throw;

namespace TaikoLocalServer.Handlers;

public record GetDanScoreQuery(uint Baid, uint Type, uint[] DanIds) : IRequest<CommonDanScoreDataResponse>;

public class GetDanScoreQueryHandler : IRequestHandler<GetDanScoreQuery, CommonDanScoreDataResponse>
{
    private readonly ILogger<GetDanScoreQueryHandler> logger;
    private readonly TaikoDbContext                   context;


    public GetDanScoreQueryHandler(ILogger<GetDanScoreQueryHandler> logger, TaikoDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    public async Task<CommonDanScoreDataResponse> Handle(GetDanScoreQuery request, CancellationToken cancellationToken)
    {
        var danType = (DanType)request.Type;
        danType.Throw().IfOutOfRange();

        var idList = request.DanIds.ToList();
        // Select the dan score data from the database where baid and type matches and danid is in the list of danids
        var danScoreData = await context.DanScoreData
            .Where(d => d.Baid == request.Baid && d.DanType == danType &&idList.Contains(d.DanId))
            .Include(d => d.DanStageScoreData)
            .ToListAsync(cancellationToken);
        var response = new CommonDanScoreDataResponse
        {
            Result = 1
        };
        foreach (var danScoreDatum in danScoreData)
        {
            var responseData = new CommonDanScoreDataResponse.DanScoreData
            {
                DanId = danScoreDatum.DanId,
                ArrivalSongCnt = danScoreDatum.ArrivalSongCount,
                ComboCntTotal = danScoreDatum.ComboCountTotal,
                SoulGaugeTotal = danScoreDatum.SoulGaugeTotal
            };
            for (int i = 0; i < danScoreDatum.ArrivalSongCount; i++)
            {
                var songNumber = i;
                var stageScoreDatum = danScoreDatum.DanStageScoreData.FirstOrDefault(d => d.SongNumber == songNumber);
                if (stageScoreDatum is null)
                {
                    logger.LogWarning("Stage score data for dan {DanId} song number {SongNumber} not found", danScoreDatum.DanId, songNumber);
                    stageScoreDatum = new DanStageScoreDatum();
                }
                responseData.AryDanScoreDataStages.Add(new CommonDanScoreDataResponse.DanScoreDataStage
                {
                    PlayScore = stageScoreDatum.PlayScore,
                    GoodCnt = stageScoreDatum.GoodCount,
                    OkCnt = stageScoreDatum.OkCount,
                    NgCnt = stageScoreDatum.BadCount,
                    PoundCnt = stageScoreDatum.DrumrollCount,
                    HitCnt = stageScoreDatum.TotalHitCount,
                    ComboCnt = stageScoreDatum.ComboCount,
                    HighScore = stageScoreDatum.HighScore
                });
            }
            response.AryDanScoreDatas.Add(responseData);
        }
        return response;
    }
}


