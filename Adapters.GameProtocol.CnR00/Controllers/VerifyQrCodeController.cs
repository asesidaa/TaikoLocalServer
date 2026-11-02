namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Controllers;

[ApiController]
public class VerifyQrCodeController : BaseProtocolController<VerifyQrCodeController>
{
    private readonly IGameDataCatalog gameDataService;

    public VerifyQrCodeController(IGameDataCatalog gameDataService)
    {
        this.gameDataService = gameDataService;
    }

    [HttpPost("/v12r00_cn/chassis/verifyqrcode.php")]
    [Produces("application/protobuf")]
    public IActionResult VerifyQrCodeCN00([FromBody] VerifyQrcodeRequest request)
    {
        Logger.LogInformation("VerifyQrCode request : {@Request}", request);

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
        var qrCodeDataDictionary = gameDataService.Nijiiro().GetQRCodeDataDictionary();

        qrCodeDataDictionary.TryGetValue(serial, out var qrCodeId);

        if (qrCodeId == 0)
        {
            Logger.LogWarning("Requested QR code serial {Serial} does not exist!", serial);
            return -1;
        }

        return (int)qrCodeId;
    }
}
