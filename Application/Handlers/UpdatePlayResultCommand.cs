using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Dtos.Ac15;
using TaikoLocalServer.Application.Settings;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct UpdatePlayResultCommand(uint Baid, GameEra Era, CommonPlayResultData PlayResultData) : IRequest<uint>;

public partial class UpdatePlayResultCommandHandler(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService,
    ILogger<UpdatePlayResultCommandHandler> logger,
    IOptions<ServerSettings>? settings = null)
    : IRequestHandler<UpdatePlayResultCommand, uint>,
      IRequestHandler<UpdateAc15PlayResultCommand, uint>
{
    private readonly ServerSettings settings = settings?.Value ?? new ServerSettings();

    public ValueTask<uint> Handle(UpdatePlayResultCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported non-Nijiiro playresult command era: {request.Era}")
    };

    public ValueTask<uint> Handle(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        GameEra.White => HandleWhite(request, cancellationToken),
        GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported AC15 playresult command era: {request.Era}")
    };

    private partial ValueTask<uint> HandleNijiiro(UpdatePlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleGreen(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleBlue(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleYellow(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleRed(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleWhite(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleMurasaki(UpdateAc15PlayResultCommand request, CancellationToken cancellationToken);
}
