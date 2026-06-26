namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetRecommendQuery(GameEra Era, uint GenderType, uint PlayerAge) : IRequest<CommonRecommendResponse>;

public partial class GetRecommendQueryHandler(
    ILogger<GetRecommendQueryHandler> logger,
    IGameDataCatalog gameDataService)
    : IRequestHandler<GetRecommendQuery, CommonRecommendResponse>
{
    public ValueTask<CommonRecommendResponse> Handle(GetRecommendQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        GameEra.White => HandleWhite(request, cancellationToken),
        GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
        GameEra.Kimidori => HandleKimidori(request, cancellationToken),
        GameEra.Momoiro => HandleMomoiro(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonRecommendResponse> HandleBlue(GetRecommendQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonRecommendResponse> HandleGreen(GetRecommendQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonRecommendResponse> HandleYellow(GetRecommendQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonRecommendResponse> HandleRed(GetRecommendQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonRecommendResponse> HandleWhite(GetRecommendQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonRecommendResponse> HandleMurasaki(GetRecommendQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonRecommendResponse> HandleKimidori(GetRecommendQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonRecommendResponse> HandleMomoiro(GetRecommendQuery request, CancellationToken cancellationToken);
}
