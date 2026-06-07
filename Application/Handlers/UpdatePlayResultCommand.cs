using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct UpdatePlayResultCommand(uint Baid, GameEra Era, CommonPlayResultData PlayResultData) : IRequest<uint>;

public partial class UpdatePlayResultCommandHandler(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService,
    ILogger<UpdatePlayResultCommandHandler> logger,
    IOptions<ServerSettings>? settings = null)
    : IRequestHandler<UpdatePlayResultCommand, uint>
{
    private readonly ServerSettings settings = settings?.Value ?? new ServerSettings();

    public ValueTask<uint> Handle(UpdatePlayResultCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<uint> HandleNijiiro(UpdatePlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleGreen(UpdatePlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleBlue(UpdatePlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleYellow(UpdatePlayResultCommand request, CancellationToken cancellationToken);
}
