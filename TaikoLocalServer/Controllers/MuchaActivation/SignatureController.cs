using System.Buffers;
using System.Text;

using System.Text.Json.Serialization;

namespace TaikoLocalServer.Controllers.MuchaActivation;


public class SignatureResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

[Route("/mucha_activation/signature")]
[ApiController]
public class SignatureController : BaseController<SignatureController>
{
    [HttpPost]
    public async Task<SignatureResponse> Signature()
    {
        HttpContext.Request.EnableBuffering();
        var body = await HttpContext.Request.BodyReader.ReadAsync();
        var json = Encoding.UTF8.GetString(body.Buffer.ToArray());
        Logger.LogInformation("Signature request: {Request}", json);
        var response = new SignatureResponse
        {
            Status = 200,
            Message = "Success",
            Signature = "1220"
        };
        return response;
    }
}