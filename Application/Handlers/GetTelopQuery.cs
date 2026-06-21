namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetTelopQuery(GameEra Era, uint TelopId) : IRequest<CommonGetTelopResponse>;

public partial class GetTelopQueryHandler(IGameDataCatalog gameDataService)
    : IRequestHandler<GetTelopQuery, CommonGetTelopResponse>
{
    public ValueTask<CommonGetTelopResponse> Handle(GetTelopQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        GameEra.White => HandleWhite(request, cancellationToken),
        GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
        _ => throw new InvalidOperationException($"GetTelopQuery is not implemented for era: {request.Era}")
    };

    private partial ValueTask<CommonGetTelopResponse> HandleGreen(GetTelopQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetTelopResponse> HandleBlue(GetTelopQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetTelopResponse> HandleYellow(GetTelopQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetTelopResponse> HandleRed(GetTelopQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetTelopResponse> HandleWhite(GetTelopQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetTelopResponse> HandleMurasaki(GetTelopQuery request, CancellationToken cancellationToken);
}
