using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetDanScoreQuery(uint Baid, GameEra Era, uint Type, uint[] DanIds) : IRequest<CommonDanScoreDataResponse>;

public class GetDanScoreQueryHandler : IRequestHandler<GetDanScoreQuery, CommonDanScoreDataResponse>
{
    private readonly ILogger<GetDanScoreQueryHandler> logger;
    private readonly ITaikoDbContext                   context;


    public GetDanScoreQueryHandler(ILogger<GetDanScoreQueryHandler> logger, ITaikoDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    public async ValueTask<CommonDanScoreDataResponse> Handle(GetDanScoreQuery request, CancellationToken cancellationToken)
    {
        var danType = (DanType)request.Type;
        danType.Throw().IfOutOfRange();

        var idList = request.DanIds.ToList();
        // Select the dan score data from the database where baid and type matches and danid is in the list of danids
        var danScoreData = await context.DanScoreDataNijiiro
            .Where(d => d.Baid == request.Baid && d.DanType == danType &&idList.Contains(d.DanId))
            .Include(d => d.DanStageScoreData)
            .ToListAsync(cancellationToken);
        var response = new CommonDanScoreDataResponse
        {
            Result = 1
        };
        foreach (var DanScoreDatumNijiiro in danScoreData)
        {
            var responseData = new CommonDanScoreDataResponse.DanScoreData
            {
                DanId = DanScoreDatumNijiiro.DanId,
                ArrivalSongCnt = DanScoreDatumNijiiro.ArrivalSongCount,
                ComboCntTotal = DanScoreDatumNijiiro.ComboCountTotal,
                SoulGaugeTotal = DanScoreDatumNijiiro.SoulGaugeTotal
            };
            for (int i = 0; i < DanScoreDatumNijiiro.ArrivalSongCount; i++)
            {
                var songNumber = i;
                var stageScoreDatum = DanScoreDatumNijiiro.DanStageScoreData.FirstOrDefault(d => d.SongNumber == songNumber);
                if (stageScoreDatum is null)
                {
                    logger.LogWarning("Stage score data for dan {DanId} song number {SongNumber} not found", DanScoreDatumNijiiro.DanId, songNumber);
                    stageScoreDatum = new DanStageScoreDatumNijiiro();
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


