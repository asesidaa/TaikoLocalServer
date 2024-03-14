namespace TaikoLocalServer.Controllers.Game;

[ApiController]
public class VerifyQrCodeController : BaseController<VerifyQrCodeController>
{
    private readonly IGameDataService gameDataService;

    public VerifyQrCodeController(IGameDataService gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost("/v12r08_ww/chassis/verifyqrcode_ku5ra5q7.php")]
    [Produces("application/protobuf")]
    public IActionResult VerifyQrCode([FromBody] VerifyQrcodeRequest request)
    {
        Logger.LogInformation("VerifyQrCode request : {Request}", request.Stringify());

        var qrCodeId = VerifyQr(request.QrcodeSerial);
        var response = new VerifyQrcodeResponse
        {
            Result = 1,
            QrcodeId = (uint)qrCodeId
        };
        
        if (qrCodeId == -1)
        {
            response.Result = 51;
        }

        return Ok(response);
    }

    private int VerifyQr(string serial)
    {
        var qrCodeDataDictionary = gameDataService.GetQRCodeDataDictionary();

        qrCodeDataDictionary.TryGetValue(serial, out var qrCodeId);

        if (qrCodeId == 0)
        {
            Logger.LogWarning("Requested QR code serial {Serial} does not exist!", serial);
            return -1;
        }

        return (int)qrCodeId;
    }
}