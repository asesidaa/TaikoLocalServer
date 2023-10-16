namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r00_cn/chassis/getfolder.php")]
[ApiController]
public class GetFolderController : BaseController<GetFolderController>
{
    private readonly IGameDataService gameDataService;
    public GetFolderController(IGameDataService gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetFolder([FromBody] GetfolderRequest request)
    {
        Logger.LogInformation("GetFolder request : {Request}", request.Stringify());

        var response = new GetfolderResponse
        {
            Result = 1
        };

        foreach (var folderId in request.FolderIds)
        {
            gameDataService.GetFolderDictionary().TryGetValue(folderId, out var folderData);
            if (folderData is null)
            {
                Logger.LogWarning("Requested folder id {Id} does not exist!", folderId);
                continue;
            }

            response.AryEventfolderDatas.Add(folderData);
        }

        return Ok(response);
    }
}