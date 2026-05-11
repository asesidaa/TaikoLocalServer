namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetFolderQuery(GameEra Era, uint[] FolderIds) : IRequest<CommonGetFolderResponse>;

public partial class GetFolderQueryHandler(ILogger<GetFolderQueryHandler> logger, IGameDataCatalog gameDataService)
    : IRequestHandler<GetFolderQuery, CommonGetFolderResponse>
{
    public ValueTask<CommonGetFolderResponse> Handle(GetFolderQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonGetFolderResponse> HandleNijiiro(GetFolderQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetFolderResponse> HandleGreen(GetFolderQuery request, CancellationToken cancellationToken);
}
