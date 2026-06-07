namespace TaikoLocalServer.Application.Handlers;

public partial class TournamentCheckQueryHandler
{
    private partial ValueTask<CommonTournamentCheckResponse> HandleYellow(
        TournamentCheckQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Yellow TournamentCheck stub for kit {KitId}, returning empty", request.KitId);
        return ValueTask.FromResult(new CommonTournamentCheckResponse());
    }
}
