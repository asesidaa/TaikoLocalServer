namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getfolder.php")]
[ApiController]
public class GetFolderController : BaseController<GetFolderController>
{
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
            response.AryEventfolderDatas.Add(new GetfolderResponse.EventfolderData
            {
                FolderId = folderId,
                Priority = 1,
                SongNoes = new uint[] {1,2},
                VerupNo = 1
            });
        }

        return Ok(response);
    }
}