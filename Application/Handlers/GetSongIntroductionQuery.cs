namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetSongIntroductionQuery(GameEra Era, uint[] SetIds) : IRequest<CommonGetSongIntroductionResponse>;

public partial class GetSongIntroductionQueryHandler(IGameDataCatalog gameDataService, ILogger<GetSongIntroductionQueryHandler> logger) 
    : IRequestHandler<GetSongIntroductionQuery, CommonGetSongIntroductionResponse>
{

    public ValueTask<CommonGetSongIntroductionResponse> Handle(GetSongIntroductionQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonGetSongIntroductionResponse> HandleNijiiro(GetSongIntroductionQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetSongIntroductionResponse> HandleGreen(GetSongIntroductionQuery request, CancellationToken cancellationToken);
}

