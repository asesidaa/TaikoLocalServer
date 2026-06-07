namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetRecommendQuery(GameEra Era, uint GenderType, uint PlayerAge) : IRequest<CommonRecommendResponse>;

public partial class GetRecommendQueryHandler(
    ILogger<GetRecommendQueryHandler> logger,
    IGameDataCatalog gameDataService)
    : IRequestHandler<GetRecommendQuery, CommonRecommendResponse>
{
    public ValueTask<CommonRecommendResponse> Handle(GetRecommendQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonRecommendResponse> HandleGreen(GetRecommendQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonRecommendResponse> HandleYellow(GetRecommendQuery request, CancellationToken cancellationToken);
}
