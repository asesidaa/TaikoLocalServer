namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/crownsdata.php")]
[ApiController]
public class CrownsDataController : ControllerBase
{
    private readonly ILogger<CrownsDataController> logger;
    
    private readonly TaikoDbContext context;

    public CrownsDataController(ILogger<CrownsDataController> logger, TaikoDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult CrownsData([FromBody] CrownsDataRequest request)
    {
        logger.LogInformation("CrownsData request : {Request}", request.Stringify());

        var songBestData = context.SongBestData.Where(datum => datum.Baid == request.Baid).ToList();
        
        var crown = new ushort[Constants.CROWN_FLAG_ARRAY_SIZE];
        var dondafulCrown = new byte[Constants.DONDAFUL_CROWN_FLAG_ARRAY_SIZE];

        for (var songId = 0; songId < Constants.MUSIC_ID_MAX; songId++)
        {
            var id = songId;
            dondafulCrown[songId] = songBestData
                // Select song of this song id with dondaful crown 
                .Where(datum => datum.SongId == id &&
                                datum.BestCrown == CrownType.Dondaful)
                // Calculate flag according to difficulty
                .Aggregate((byte)0, (flag, datum) => FlagCalculator.ComputeDondafulCrownFlag(flag, datum.Difficulty));
            
            crown[songId] = songBestData
                // Select song of this song id with clear or fc crown
                .Where(datum => datum.SongId == id &&
                                datum.BestCrown is CrownType.Clear or CrownType.Gold)
                // Calculate flag according to difficulty
                .Aggregate((ushort)0, (flag, datum) => FlagCalculator.ComputeCrownFlag(flag, datum.BestCrown, datum.Difficulty));
        }
        
        var response = new CrownsDataResponse
        {
            Result = 1,
            CrownFlg = GZipBytesUtil.GetGZipBytes(crown),
            DondafulCrownFlg = GZipBytesUtil.GetGZipBytes(dondafulCrown)
        };

        return Ok(response);
    }
}