
namespace TaikoLocalServer.Application.Handlers;
public readonly record struct AddMyDonEntryCommand(GameEra Era, string AccessCode, string Name, uint Language) : IRequest<CommonMyDonEntryResponse>;

public partial class AddMyDonEntryCommandHandler(
    ITaikoDbContext context,
    ILogger<AddMyDonEntryCommandHandler> logger,
    IGameDataCatalog gameDataService)
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
