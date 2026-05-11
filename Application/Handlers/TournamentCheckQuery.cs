namespace TaikoLocalServer.Application.Handlers;

public readonly record struct TournamentCheckQuery(GameEra Era, uint KitId) : IRequest<CommonTournamentCheckResponse>;

public partial class TournamentCheckQueryHandler(ILogger<TournamentCheckQueryHandler> logger)
    : IRequestHandler<TournamentCheckQuery, CommonTournamentCheckResponse>
{
    public ValueTask<CommonTournamentCheckResponse> Handle(TournamentCheckQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonTournamentCheckResponse> HandleNijiiro(TournamentCheckQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonTournamentCheckResponse> HandleGreen(TournamentCheckQuery request, CancellationToken cancellationToken);
}
