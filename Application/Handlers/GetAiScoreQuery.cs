using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetAiScoreQuery(uint Baid, GameEra Era, uint SongId, uint Level) : IRequest<CommonAiScoreResponse>;

#pragma warning disable CS9113 // Parameter is unread.
public partial class GetAiScoreQueryHandler(ITaikoDbContext context, ILogger<GetAiScoreQueryHandler> logger)
#pragma warning restore CS9113 // Parameter is unread.
    : IRequestHandler<GetAiScoreQuery, CommonAiScoreResponse>
{
    public ValueTask<CommonAiScoreResponse> Handle(GetAiScoreQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonAiScoreResponse> HandleNijiiro(GetAiScoreQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonAiScoreResponse> HandleGreen(GetAiScoreQuery request, CancellationToken cancellationToken);
}
