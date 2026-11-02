using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct AddTokenCountCommand(GameEra Era, CommonAddTokenCountRequest Request) : IRequest;

public partial class AddTokenCountCommandHandler : IRequestHandler<AddTokenCountCommand>
{
    private readonly ITaikoDbContext context;

    private readonly ILogger<AddTokenCountCommandHandler> logger;

    public AddTokenCountCommandHandler(ITaikoDbContext context, ILogger<AddTokenCountCommandHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public ValueTask<Unit> Handle(AddTokenCountCommand command, CancellationToken cancellationToken) => command.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(command, cancellationToken),
        GameEra.Green => HandleGreen(command, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {command.Era}")
    };

    private partial ValueTask<Unit> HandleNijiiro(AddTokenCountCommand command, CancellationToken cancellationToken);
    private partial ValueTask<Unit> HandleGreen(AddTokenCountCommand command, CancellationToken cancellationToken);
}
