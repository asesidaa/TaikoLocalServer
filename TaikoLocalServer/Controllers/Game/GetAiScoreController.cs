namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r03/chassis/getaiscore.php")]
[ApiController]
public class GetAiScoreController : BaseController<GetAiScoreController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetAiScore([FromBody] GetAiScoreRequest request)
    {
        Logger.LogInformation("GetAiScore request : {Request}", request.Stringify());

        var response = new GetAiScoreResponse
        {
            Result = 1
        };

        // There's either 3 or 5 total sections
        // SectionNo doesn't seem to actually affect which section is being assigned to, only the List order matters
        /*response.AryBestSectionDatas.Add(new GetAiScoreResponse.AiBestSectionData()
        {
            SectionNo = 1,
            Crown = (uint)CrownType.Clear,
            Score = 100000,
            GoodCnt = 100,
            OkCnt = 50,
            NgCnt = 25,
            PoundCnt = 12
        });
        response.AryBestSectionDatas.Add(new GetAiScoreResponse.AiBestSectionData()
        {
            SectionNo = 2,
            Crown = (uint)CrownType.Gold,
            Score = 100001,
            GoodCnt = 101,
            OkCnt = 50,
            NgCnt = 25,
            PoundCnt = 12
        });
        response.AryBestSectionDatas.Add(new GetAiScoreResponse.AiBestSectionData()
        {
            SectionNo = 3,
            Crown = (uint)CrownType.Dondaful,
            Score = 100002,
            GoodCnt = 102,
            OkCnt = 50,
            NgCnt = 25,
            PoundCnt = 12
        });
        response.AryBestSectionDatas.Add(new GetAiScoreResponse.AiBestSectionData()
        {
            SectionNo = 4,
            Crown = (uint)CrownType.Gold,
            Score = 100003,
            GoodCnt = 103,
            OkCnt = 50,
            NgCnt = 25,
            PoundCnt = 12
        });
        response.AryBestSectionDatas.Add(new GetAiScoreResponse.AiBestSectionData()
        {
            SectionNo = 5,
            Crown = (uint)CrownType.Clear,
            Score = 100004,
            GoodCnt = 104,
            OkCnt = 50,
            NgCnt = 25,
            PoundCnt = 12
        });*/

        return Ok(response);
    }
}