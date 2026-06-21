using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetSelfBestQuery(uint Baid, GameEra Era, uint Difficulty, uint[] SongIdList) : IRequest<CommonSelfBestResponse>;

public partial class GetSelfBestQueryHandler(IGameDataCatalog gameDataService, ITaikoDbContext context, ILogger<GetSelfBestQueryHandler> logger)
    : IRequestHandler<GetSelfBestQuery, CommonSelfBestResponse>
{
    public ValueTask<CommonSelfBestResponse> Handle(GetSelfBestQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        GameEra.White => HandleWhite(request, cancellationToken),
        GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonSelfBestResponse> HandleNijiiro(GetSelfBestQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonSelfBestResponse> HandleGreen(GetSelfBestQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonSelfBestResponse> HandleBlue(GetSelfBestQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonSelfBestResponse> HandleYellow(GetSelfBestQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonSelfBestResponse> HandleRed(GetSelfBestQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonSelfBestResponse> HandleWhite(GetSelfBestQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonSelfBestResponse> HandleMurasaki(GetSelfBestQuery request, CancellationToken cancellationToken);
}
