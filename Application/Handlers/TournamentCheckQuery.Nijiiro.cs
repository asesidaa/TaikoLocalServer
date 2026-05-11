namespace TaikoLocalServer.Application.Handlers;

public partial class TournamentCheckQueryHandler
{
    private partial ValueTask<CommonTournamentCheckResponse> HandleNijiiro(TournamentCheckQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Nijiiro TournamentCheck query for kit {KitId}, returning inline-controller default", request.KitId);
        return ValueTask.FromResult(new CommonTournamentCheckResponse());
    }
}
