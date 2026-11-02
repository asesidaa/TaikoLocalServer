namespace TaikoLocalServer.Application.Handlers;

public partial class TournamentCheckQueryHandler
{
    private partial ValueTask<CommonTournamentCheckResponse> HandleGreen(TournamentCheckQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green TournamentCheck experiment for kit {KitId}, returning accepted no-gacha result", request.KitId);
        return ValueTask.FromResult(new CommonTournamentCheckResponse { Result = 904 });
    }
}
