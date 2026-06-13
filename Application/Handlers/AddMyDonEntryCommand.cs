
namespace TaikoLocalServer.Application.Handlers;
public readonly record struct AddMyDonEntryCommand(GameEra Era, string AccessCode, string Name, uint Language) : IRequest<CommonMyDonEntryResponse>;

public partial class AddMyDonEntryCommandHandler(
    ITaikoDbContext context,
    ILogger<AddMyDonEntryCommandHandler> logger)
    : IRequestHandler<AddMyDonEntryCommand, CommonMyDonEntryResponse>
{
    public ValueTask<CommonMyDonEntryResponse> Handle(AddMyDonEntryCommand request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonMyDonEntryResponse> HandleNijiiro(AddMyDonEntryCommand request, CancellationToken cancellationToken);
    private partial ValueTask<CommonMyDonEntryResponse> HandleGreen(AddMyDonEntryCommand request, CancellationToken cancellationToken);
    private partial ValueTask<CommonMyDonEntryResponse> HandleBlue(AddMyDonEntryCommand request, CancellationToken cancellationToken);
    private partial ValueTask<CommonMyDonEntryResponse> HandleYellow(AddMyDonEntryCommand request, CancellationToken cancellationToken);
    private partial ValueTask<CommonMyDonEntryResponse> HandleRed(AddMyDonEntryCommand request, CancellationToken cancellationToken);
}
