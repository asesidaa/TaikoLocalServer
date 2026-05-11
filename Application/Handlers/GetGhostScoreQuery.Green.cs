namespace TaikoLocalServer.Application.Handlers;

public partial class GetGhostScoreQueryHandler
{
    public partial async ValueTask<CommonGhostScoreResponse> Handle(
        GetGhostScoreQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Reading Green ghost score for baid {Baid}, song {SongNo}, level {Level}",
            request.Baid,
            request.SongNo,
            request.Level);
        var difficulty = GreenPlayResultMapping.MapDifficulty(request.Level);
        var play = await context.SongPlayDataGreen
            .Where(row => row.Baid == request.Baid
                && row.SongId == request.SongNo
                && row.Difficulty == difficulty)
            .OrderByDescending(row => row.PlayTime)
            .ThenByDescending(row => row.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (play is null)
        {
            return new CommonGhostScoreResponse { Result = 1 };
        }

        var sections = await context.GhostStageSectionDataGreen
            .Where(row => row.PlayId == play.Id)
            .OrderBy(row => row.SectionNo)
            .Select(row => new CommonGhostScoreResponse.GhostBestSectionData
            {
                SectionNo = row.SectionNo,
                GoodCnt = row.GoodCount,
                OkCnt = row.OkCount,
                NgCnt = row.NgCount,
                PoundCnt = row.PoundCount
            })
            .ToListAsync(cancellationToken);

        return new CommonGhostScoreResponse
        {
            Result = 1,
            AryBestSectionData = sections
        };
    }
}
