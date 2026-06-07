namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetTaikojukuQuery(GameEra Era, IReadOnlyList<uint> RequestedDans) : IRequest<CommonTaikojukuResponse>;

public partial class GetTaikojukuQueryHandler(
    IGameDataCatalog gameDataService,
    ILogger<GetTaikojukuQueryHandler> logger)
    : IRequestHandler<GetTaikojukuQuery, CommonTaikojukuResponse>
{
    public ValueTask<CommonTaikojukuResponse> Handle(GetTaikojukuQuery request, CancellationToken cancellationToken)
        => request.Era switch
        {
            GameEra.Green => HandleGreen(request, cancellationToken),
            GameEra.Blue => HandleBlue(request, cancellationToken),
            GameEra.Yellow => HandleYellow(request, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
        };

    private partial ValueTask<CommonTaikojukuResponse> HandleGreen(GetTaikojukuQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonTaikojukuResponse> HandleBlue(GetTaikojukuQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonTaikojukuResponse> HandleYellow(GetTaikojukuQuery request, CancellationToken cancellationToken);
}
