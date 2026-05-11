
namespace TaikoLocalServer.Application.Handlers;
public readonly record struct AddMyDonEntryCommand(GameEra Era, string AccessCode, string Name, uint Language) : IRequest<CommonMyDonEntryResponse>;

#pragma warning disable CS9113 // Parameter is unread.
public partial class AddMyDonEntryCommandHandler(ITaikoDbContext context, ILogger<AddMyDonEntryCommandHandler> logger)
#pragma warning restore CS9113 // Parameter is unread.
    : IRequestHandler<AddMyDonEntryCommand, CommonMyDonEntryResponse>
{
    public ValueTask<CommonMyDonEntryResponse> Handle(AddMyDonEntryCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonMyDonEntryResponse> HandleNijiiro(AddMyDonEntryCommand request, CancellationToken cancellationToken);
    private partial ValueTask<CommonMyDonEntryResponse> HandleGreen(AddMyDonEntryCommand request, CancellationToken cancellationToken);
}
