namespace TaikoLocalServer.Application.Handlers;

public readonly record struct UpdatePlayResultCommand(uint Baid, GameEra Era, CommonPlayResultData PlayResultData) : IRequest<uint>;

public partial class UpdatePlayResultCommandHandler(ITaikoDbContext context, ILogger<UpdatePlayResultCommandHandler> logger)
    : IRequestHandler<UpdatePlayResultCommand, uint>
{
    public ValueTask<uint> Handle(UpdatePlayResultCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<uint> HandleNijiiro(UpdatePlayResultCommand request, CancellationToken cancellationToken);
    private partial ValueTask<uint> HandleGreen(UpdatePlayResultCommand request, CancellationToken cancellationToken);
}
