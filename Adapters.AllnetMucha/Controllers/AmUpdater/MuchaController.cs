using TaikoLocalServer.Adapters.AllnetMucha.Common;
using TaikoLocalServer.Adapters.AllnetMucha.Wire;

namespace TaikoLocalServer.Adapters.AllnetMucha.Controllers.AmUpdater;

public class MuchaController : BaseProtocolController<MuchaController>
{
    private readonly AllnetSettings settings;

    public MuchaController(IOptions<AllnetSettings> settings)
    {
        this.settings = settings.Value;
    }

    [HttpPost("/mucha_front/boardauth.do")]

    public ContentResult BoardAuth([FromForm] MuchaBoardAuthRequest request)
    {
        Logger.LogInformation("Mucha request: {Request}", request.Stringify());
        // The dongle PRX parser only recognizes keys present in the table at
        // EBOOT.ELF 0x0103D188 — slot 1 is "SERVER_TIME" and slot 2 is
        // "SERVER_TIME_UTC" (NOT "UTC_SERVER_TIME"). Unrecognized keys are
        // dropped, leaving the corresponding MuchaAuthResult field empty.
        //
        // sub_92A510 (mucha::GetSleepTime) reads MuchaAuthResult+3787 and
        // feeds it to sub_35BE3C, a fixed-width slicer expecting
        // YYYYMMDDHHMMSS (offsets 0/4/6/8/10/12, lengths 4/2/2/2/2/2). On an
        // empty string the final substr(12,2) returns "", std::stoi throws
        // std::invalid_argument, and MuchaMainThread aborts via __cxa_throw.
        // Both 14-char format AND the correct key name are required.
        var serverTime = DateTime.Now.ToString("yyyyMMddHHmmss");
        var utcServerTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var response = new Dictionary<string, string>
        {
            { "RESULTS", "001" },
            { "AREA_0", "008" },
            { "AREA_0_EN", "" },
            { "AREA_1", "009" },
            { "AREA_1_EN", "" },
            { "AREA_2", "010" },
            { "AREA_2_EN", "" },
            { "AREA_3", "011" },
            { "AREA_3_EN", "" },
            { "AREA_FULL_0", "" },
            { "AREA_FULL_0_EN", "" },
            { "AREA_FULL_1", "" },
            { "AREA_FULL_1_EN", "" },
            { "AREA_FULL_2", "" },
            { "AREA_FULL_2_EN", "" },
            { "AREA_FULL_3", "" },
            { "AREA_FULL_3_EN", "" },
            { "AUTH_INTERVAL", "86400" },
            { "CHARGE_URL", $"{settings.MuchaUrl}/charge/" },
            { "CONSUME_TOKEN", "1" },
            { "COUNTRY_CD", "JPN" },
            { "DONGLE_FLG", "1" },
            { "EXPIRATION_DATE", "null" },
            { "FILE_URL", $"{settings.MuchaUrl}/file/" },
            { "FORCE_BOOT", "0" },
            { "PLACE_ID", request.PlaceId ?? "" },
            { "PREFECTURE_ID", "14" },
            { "SERVER_TIME", serverTime },
            { "SERVER_TIME_UTC", utcServerTime },
            { "SHOP_NAME", "NAMCO" },
            { "SHOP_NAME_EN", "NAMCO" },
            { "SHOP_NICKNAME", "W" },
            { "SHOP_NICKNAME_EN", "W" },
            { "URL_1", $"{settings.MuchaUrl}/url1/" },
            { "URL_2", $"{settings.MuchaUrl}/url2/" },
            { "URL_3", $"{settings.MuchaUrl}/url3/" },
            { "USE_TOKEN", "1" }
        };
        var formOutput = FormOutputUtil.ToFormOutput(response);
        return Content(formOutput);
    }

    [HttpPost("/mucha_front/updatacheck.do")]
    public ContentResult UpdateCheck(MuchaUpdateCheckRequest request)
    {
        Logger.LogInformation("Request is {Request}", request.Stringify());
        // Empty UPDATE chain (no UPDATE_VER_X blocks) is the only safe "no updates"
        // shape. Any chain we send is forwarded to the EBOOT's chunk subsystem
        // (chunk_update_processor_aborts at sub_4C66AC) which inserts one record per
        // entry into the in-memory chunk_record_table (qword_1498010) with
        // slot_count derived as (entry[+8] - 0x10) >> 2. With our zero-size fields
        // every record gets slot_count=0; chunkimg_flush_trigger (sub_4DB494) then
        // persists them to /dev_hdd0/.../mucha/chunk/chunk.img, and the next boot
        // crashes when chunk_record_alloc_slot (sub_4CFFBC) asks the hooked
        // allocator for 0 bytes and dereferences the 0xFFFFFFB0 sentinel.
        //
        // No chain -> sub_4BCF78 parser hits LABEL_53 (n2=2) -> sub_4C66AC sees
        // chunk_record_count==0 -> never calls chunk_table_upsert_record ->
        // sub_4DB494 early-exits -> no chunk.img write -> no second-boot crash.
        var response = new Dictionary<string, string>
        {
            { "RESULTS", "001" },
            { "USER_ID", "NAMCO" },
            { "PASSWORD", "NAMCO" },
            { "EXE_VER", request.GameVersion ?? "S1210JPN08.18" }
        };

        var formOutput = FormOutputUtil.ToFormOutput(response);
        return Content(formOutput);
    }

    [HttpPost("/mucha_front/downloadstate.do")]
    public IActionResult DownloadState()
    {
        // Real cabinets may call this from stale persisted Mucha chunk state. A
        // successful RESULTS=001 response can continue the updater path and crash, while
        // the observed HTTP 405 failure lets the game continue with an updater error
        // visible only in test mode.
        return StatusCode(405);
    }

    [HttpPost("/mucha_front/downloaderror.do")]
    public ContentResult DownloadError()
    {
        // Companion to downloadstate.do: triggered if the download manager surfaces an
        // error. Parsed by the same sub_4B70BC, so the same RESULTS=001 ack applies.
        var response = new Dictionary<string, string>
        {
            { "RESULTS", "001" }
        };
        return Content(FormOutputUtil.ToFormOutput(response));
    }

    [HttpPost("/mucha_front/regiauth.do")]
    public ContentResult RegiAuth([FromForm] MuchaRegiAuthRequest request)
    {
        // International Mucha token registration compatibility. Keep this
        // stateless: no wallet, balance, registration, or token persistence.
        if (!MuchaCrypto.HasUsableSendDate(request.SendDate))
        {
            return Content(FormOutputUtil.ToFormOutput(new Dictionary<string, string>
            {
                { "RESULTS", "000" }
            }));
        }

        var encryptedToken = MuchaCrypto.EncryptTokenValue("999", request.SendDate);
        var response = new Dictionary<string, string>
        {
            { "RESULTS", "001" },
            { "ALL_TOKEN", encryptedToken },
            { "ADD_TOKEN", encryptedToken }
        };
        return Content(FormOutputUtil.ToFormOutput(response));
    }

    [HttpPost("/mucha_front/tokenstate.do")]
    public ContentResult TokenState()
    {
        var response = new Dictionary<string, string>
        {
            { "RESULTS", "001" }
        };
        return Content(FormOutputUtil.ToFormOutput(response));
    }

    [HttpPost("/mucha_front/tokenmarginstate.do")]
    public ContentResult TokenMarginState()
    {
        var response = new Dictionary<string, string>
        {
            { "RESULTS", "001" },
            { "LIMIT_LOWER_TOKEN", "0" },
            { "LIMIT_UPPER_TOKEN", "0" },
            { "LAST_SETTLEMENT_MONTH", "0" },
            { "LAST_LIMIT_LOWER_TOKEN", "0" },
            { "LAST_LIMIT_UPPER_TOKEN", "0" },
            { "SETTLEMENT_MONTH", "0" }
        };
        return Content(FormOutputUtil.ToFormOutput(response));
    }
}
