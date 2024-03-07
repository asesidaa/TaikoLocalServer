using System.Buffers;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Settings;
using System.Text.Json;
using System.Text;

using System.Text.Json.Serialization;

namespace TaikoLocalServer.Controllers.MuchaActivation;


public class OtkResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("otk")]
    public string? Otk { get; set; }
    
    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }
    
    [JsonPropertyName("expiredAt")]
    public string? ExpiredAt { get; set; }
}

[Route("/mucha_activation/otk")]
[ApiController]
public class OtkController : BaseController<OtkController>
{
    [HttpPost]
    public async Task<OtkResponse> Otk()
    {
        HttpContext.Request.EnableBuffering();
        var body = await HttpContext.Request.BodyReader.ReadAsync();
        var json = Encoding.UTF8.GetString(body.Buffer.ToArray());
        Logger.LogInformation("MuchaActivation request: {Request}", json);
        var expiredAt = DateTime.Now.AddDays(10).ToString("yyyy-MM-ddThh:mm:ssZ");
        // Generate a random string otk with randomNumberGenerator
        var random = new Random();
        var randomNumber = random.Next(10000000, 99999999);
        var otk = randomNumber.ToString();
        var g = Guid.NewGuid();
        var uuid = g.ToString();
        var response = new OtkResponse
        {
            Status = 200,
            Message = "Success",
            Otk = otk,
            Uuid = uuid,
            ExpiredAt = expiredAt
        };
        return response;
    }
}