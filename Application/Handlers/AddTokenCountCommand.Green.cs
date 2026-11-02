namespace TaikoLocalServer.Application.Handlers;

public partial class AddTokenCountCommandHandler
{
    private partial ValueTask<Unit> HandleGreen(AddTokenCountCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green AddTokenCount stub for baid {Baid}, returning success", command.Request.Baid);
        return ValueTask.FromResult(Unit.Value);
    }
}
