namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private partial ValueTask<uint> HandleGreen(UpdatePlayResultCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green PlayResult stub for baid {Baid}, returning success", request.Baid);
        return ValueTask.FromResult(1u);
    }
}
