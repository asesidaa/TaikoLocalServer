namespace TaikoLocalServer.Application.Handlers;

public partial class TournamentCheckQueryHandler
{
    private partial ValueTask<CommonTournamentCheckResponse> HandleGreen(TournamentCheckQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green TournamentCheck stub for kit {KitId}, returning empty", request.KitId);
        return ValueTask.FromResult(new CommonTournamentCheckResponse());
    }
}
