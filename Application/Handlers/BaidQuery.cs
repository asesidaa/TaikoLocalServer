using Throw;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct BaidQuery(GameEra Era, string AccessCode) : IRequest<CommonBaidResponse>;
public readonly record struct Ac15BaidQuery(GameEra Era, string AccessCode) : IRequest<Ac15BaidResponse>;

public partial class BaidQueryHandler(
    ITaikoDbContext context,
    ILogger<BaidQueryHandler> logger,
    IGameDataCatalog gameDataService)
    : IRequestHandler<BaidQuery, CommonBaidResponse>,
      IRequestHandler<Ac15BaidQuery, Ac15BaidResponse>
{
    public ValueTask<CommonBaidResponse> Handle(BaidQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        _ => throw new InvalidOperationException($"Use {nameof(Ac15BaidQuery)} for AC15 era: {request.Era}")
    };

    public ValueTask<Ac15BaidResponse> Handle(Ac15BaidQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        GameEra.White => HandleWhite(request, cancellationToken),
        GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonBaidResponse> HandleNijiiro(BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleGreen(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleBlue(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleYellow(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleRed(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleWhite(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleMurasaki(Ac15BaidQuery request, CancellationToken cancellationToken);
}
