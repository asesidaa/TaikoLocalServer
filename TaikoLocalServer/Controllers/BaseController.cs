using MediatR;

namespace TaikoLocalServer.Controllers;

public abstract class BaseController<T> : ControllerBase where T : BaseController<T>
{
    private ILogger<T>? logger;
    
    private ISender? mediator;
    
    protected ISender Mediator => (mediator ??= HttpContext.RequestServices.GetService<ISender>()) ?? throw new InvalidOperationException();

    protected ILogger<T> Logger => (logger ??= HttpContext.RequestServices.GetService<ILogger<T>>()) ?? throw new InvalidOperationException();
}