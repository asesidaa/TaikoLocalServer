using Garm;
using google.type;

namespace TaikoLocalServer.Controllers.Garmc;

[Route("/v1/s12-jp-dev/garm.Monitoring/Ping")]
[ApiController]
public class PingController : BaseController<PingController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<PingResponse> Ping()
    {
        HttpContext.Request.EnableBuffering();
        var body = await HttpContext.Request.BodyReader.ReadAsync();
        var request = Serializer.Deserialize<PingRequest>(body.Buffer);
        Logger.LogInformation("Ping request: {Request}", request.Stringify());
        var response = new PingResponse
        {
            ServerRecvTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(50),
            ServerSendTime = DateTime.UtcNow + TimeSpan.FromMilliseconds(50),
            ClientIp = "127.0.0.1",
            GeoData = new GeoData
            {
                Country = "JPN",
                Region = "Kanto",
                City = "Tokyo",
                Latlng = new LatLng
                {
                    Latitude = 114.0,
                    Longitude = 514.0
                }
            },
            UnderMaintenance = false,
            AcidAvailable = true
        };
        Response.Headers.Append("x-drpc-code", "0");
        return response;
    }
}