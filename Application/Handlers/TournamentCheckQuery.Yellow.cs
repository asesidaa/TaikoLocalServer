namespace TaikoLocalServer.Application.Handlers;

public partial class TournamentCheckQueryHandler
{
    private partial ValueTask<CommonTournamentCheckResponse> HandleYellow(
        TournamentCheckQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Yellow TournamentCheck experiment for kit {KitId}, returning accepted no-gacha result", request.KitId);
        return ValueTask.FromResult(new CommonTournamentCheckResponse { Result = 904 });
    }
}
