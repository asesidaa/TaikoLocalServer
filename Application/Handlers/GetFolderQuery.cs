namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetFolderQuery(GameEra Era, uint[] FolderIds) : IRequest<CommonGetFolderResponse>;

public partial class GetFolderQueryHandler(ILogger<GetFolderQueryHandler> logger, IGameDataCatalog gameDataService)
    : IRequestHandler<GetFolderQuery, CommonGetFolderResponse>
{
    public ValueTask<CommonGetFolderResponse> Handle(GetFolderQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private CommonGetFolderResponse BuildFolderResponse(
        IReadOnlyDictionary<uint, EventFolderData> eventFolders,
        IEnumerable<uint> requestedFolderIds)
    {
        var response = new CommonGetFolderResponse
        {
            Result = 1
        };

        foreach (var folderId in requestedFolderIds)
        {
            if (!eventFolders.TryGetValue(folderId, out var folderData))
            {
                logger.LogWarning("Folder data for folder {FolderId} not found", folderId);
                continue;
            }

            response.AryEventfolderDatas.Add(folderData);
        }

        return response;
    }

    private partial ValueTask<CommonGetFolderResponse> HandleNijiiro(GetFolderQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetFolderResponse> HandleGreen(GetFolderQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetFolderResponse> HandleBlue(GetFolderQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetFolderResponse> HandleYellow(GetFolderQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonGetFolderResponse> HandleRed(GetFolderQuery request, CancellationToken cancellationToken);
}
