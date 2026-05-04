namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Controllers;

public abstract class BaseProtocolController<T> : ControllerBase where T : BaseProtocolController<T>
{
    private ILogger<T>? logger;

    private IMediator? mediator;

    protected IMediator Mediator => (mediator ??= HttpContext.RequestServices.GetService<IMediator>()) ?? throw new InvalidOperationException();

    protected ILogger<T> Logger => (logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()) ?? throw new InvalidOperationException();
}
