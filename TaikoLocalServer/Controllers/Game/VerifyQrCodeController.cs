namespace TaikoLocalServer.Controllers.Game;

[Route("/v12r08_ww/chassis/verifyqrcode_ku5ra5q7.php")]
[ApiController]
public class VerifyQrCodeController : BaseController<VerifyQrCodeController>
{
    private readonly IGameDataService gameDataService;

    public VerifyQrCodeController(IGameDataService gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult VerifyQrCode([FromBody] VerifyQrcodeRequest request)
    {
        Logger.LogInformation("VerifyQrCode request : {Request}", request.Stringify());

        var qrCodeDataDictionary = gameDataService.GetQRCodeDataDictionary();

        qrCodeDataDictionary.TryGetValue(request.QrcodeSerial, out var qrCodeId);

        if (qrCodeId == 0)
        {
            Logger.LogWarning("Requested QR code serial {Serial} does not exist!", request.QrcodeSerial);
        }

        var response = new VerifyQrcodeResponse
        {
            Result = 1,
            QrcodeId = qrCodeId
        };

        return Ok(response);
    }
}