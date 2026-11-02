namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetChallengeCompeQuery(GameEra Era, uint Baid) : IRequest<CommonChallengeCompeResponse>;

public partial class GetChallengeCompeQueryHandler(
    ILogger<GetChallengeCompeQueryHandler> logger)
    : IRequestHandler<GetChallengeCompeQuery, CommonChallengeCompeResponse>
{
    public ValueTask<CommonChallengeCompeResponse> Handle(GetChallengeCompeQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonChallengeCompeResponse> HandleGreen(GetChallengeCompeQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonChallengeCompeResponse> HandleRed(GetChallengeCompeQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonChallengeCompeResponse> HandleYellow(GetChallengeCompeQuery request, CancellationToken cancellationToken);
}
