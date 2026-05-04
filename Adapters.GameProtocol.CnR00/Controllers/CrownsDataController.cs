using Microsoft.Extensions.Options;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class CrownsDataController(ITaikoDbContext context, IOptions<ServerSettings> settings)
    : BaseProtocolController<CrownsDataController>
{
    private readonly ServerSettings settings = settings.Value;

    [HttpPost("/v12r00_cn/chassis/crownsdata.php")]
    [Produces("application/protobuf")]
    public async Task<IActionResult> CrownsDataCN00([FromBody] CrownsDataRequest request)
    {
        Logger.LogInformation("CrownsData request : {Request}", request.Stringify());

        var crownData = await Handle((uint)request.Baid);

        var response = new CrownsDataResponse
        {
            Result = 1,
            CrownFlg = crownData.CrownFlg,
            DondafulCrownFlg = crownData.DondafulCrownFlg
        };

        return Ok(response);
    }

    public record CrownData(byte[] CrownFlg, byte[] DondafulCrownFlg);

    private async Task<CrownData> Handle(uint baid)
    {
        var songBestData = await context.SongBestData
            .Where(datum => datum.Baid == baid)
            .ToListAsync(HttpContext.RequestAborted);

        var songIdMax = settings.EnableMoreSongs ? settings.MoreSongsSize : DomainConstants.MusicIdMax;
        var crown = new ushort[songIdMax       + 1];
        var dondafulCrown = new byte[songIdMax + 1];

        for (var songId = 0; songId < songIdMax; songId++)
        {
            var id = songId;
            dondafulCrown[songId] = songBestData
                // Select song of this song id with dondaful crown
                .Where(datum => datum.SongId    == id &&
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

        return new CrownData(GZipBytesUtil.GetGZipBytes(crown), GZipBytesUtil.GetGZipBytes(dondafulCrown));
    }
}
