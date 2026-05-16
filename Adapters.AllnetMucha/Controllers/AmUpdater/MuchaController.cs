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
            { "CONSUME_TOKEN", "0" },
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
            { "USE_TOKEN", "0" }
        };
        var formOutput = FormOutputUtil.ToFormOutput(response);
        return Content(formOutput);
    }

    [HttpPost("/mucha_front/updatacheck.do")]
    public ContentResult UpdateCheck(MuchaUpdateCheckRequest request)
    {
        Logger.LogInformation("Request is {Request}", request.Stringify());
        // EBOOT.ELF sub_4BCF78 parses the response with strict field ordering:
        //   RESULTS -> [UPDATE_VER_X UPDATE_URL_X UPDATE_SIZE_X UPDATE_CRC_X
        //               CHECK_URL_X CHECK_SIZE_X CHECK_CRC_X EXE_VER_X
        //               INFO_SIZE_X COM_SIZE_X COM_TIME_X LAN_INFO_SIZE_X]+
        //   -> USER_ID -> PASSWORD -> optional EXE_VER.
        //
        // Sending zero UPDATE_VER blocks hits the parser's LABEL_53 fast path
        // which leaves three output DWORDs uninitialized -- the PRX state
        // machine reads them after the parser returns and aborts. So we have
        // to provide at least one fully-populated update block (LABEL_67 path).
        //
        // The chain validator sub_4C5C18 then checks the first 2 bytes of each
        // 536-byte output record against a sequential slot index (record 0
        // expects WORD[0]==1, record 1 expects 2, ..., record N-1 expects N).
        // The parser writes (major<<16)|minor of UPDATE_VER into offset 0 of
        // each record; on PowerPC big-endian the high WORD -- i.e. the major
        // value -- is what the validator reads as the slot index.
        //
        // So we emit a sequential chain whose majors are 1..N where N matches
        // the game's own major version. The last entry's full version string
        // matches the running game (e.g. S1210JPN08.18) so the per-record
        // install filter at sub_4CDCA8 (compares each record's WORD against
        // dword_149AAA0, the installed-version threshold) skips every entry
        // as "already installed" and the global download queue stays empty.
        // With dword_149ABC8==0, sub_4CDCA8 returns 0, sub_4DC22C returns 0,
        // and sub_4BCBD4 takes its LABEL_12 early exit -- no downloadstate.do.
        var gameVersion = request.GameVersion ?? "S1210JPN08.18";
        var (prefix, major, minor) = ParseGameVersion(gameVersion);

        var response = new Dictionary<string, string>
        {
            { "RESULTS", "001" }
        };
        for (var slot = 1; slot <= major; slot++)
        {
            var ver = slot == major
                ? $"{prefix}{major:D2}.{minor:D2}"
                : $"{prefix}{slot:D2}.00";
            response[$"UPDATE_VER_{slot}"] = ver;
            response[$"UPDATE_URL_{slot}"] = $"{settings.MuchaUrl}/updUrl{slot}/";
            response[$"UPDATE_SIZE_{slot}"] = "0";
            response[$"UPDATE_CRC_{slot}"] = "00000000";
            response[$"CHECK_URL_{slot}"] = $"{settings.MuchaUrl}/checkUrl{slot}/";
            response[$"CHECK_SIZE_{slot}"] = "0";
            response[$"CHECK_CRC_{slot}"] = "00000000";
            response[$"EXE_VER_{slot}"] = ver;
            response[$"INFO_SIZE_{slot}"] = "0";
            response[$"COM_SIZE_{slot}"] = "0";
            response[$"COM_TIME_{slot}"] = "0";
            response[$"LAN_INFO_SIZE_{slot}"] = "0";
        }
        response["USER_ID"] = "NAMCO";
        response["PASSWORD"] = "NAMCO";
        response["EXE_VER"] = gameVersion;

        var formOutput = FormOutputUtil.ToFormOutput(response);
        return Content(formOutput);
    }

    private static (string Prefix, int Major, int Minor) ParseGameVersion(string gameVersion)
    {
        // gameVersion format is SxxxxJPNmm.nn (8-char prefix + "MM.NN"), per the
        // parser's "%*08s%02d.%02d" scanf pattern in sub_4BCF78.
        const string fallbackPrefix = "S1210JPN";
        const int fallbackMajor = 8;
        const int fallbackMinor = 18;

        if (gameVersion.Length < 8 + 5) return (fallbackPrefix, fallbackMajor, fallbackMinor);
        var prefix = gameVersion[..8];
        var majorSpan = gameVersion.AsSpan(8, 2);
        var minorSpan = gameVersion.AsSpan(11, 2);
        if (gameVersion[10] != '.') return (fallbackPrefix, fallbackMajor, fallbackMinor);
        if (!int.TryParse(majorSpan, out var major) || major <= 0 || major > 99)
            return (fallbackPrefix, fallbackMajor, fallbackMinor);
        if (!int.TryParse(minorSpan, out var minor) || minor < 0 || minor > 99)
            return (fallbackPrefix, fallbackMajor, fallbackMinor);
        return (prefix, major, minor);
    }

    [HttpPost("/mucha_front/downloadstate.do")]
    public ContentResult DownloadState()
    {
        // The game's downloadstate task (EBOOT.ELF sub_4BCBD4) polls this endpoint as
        // part of the network state machine. The response is parsed by sub_4B70BC which
        // only extracts the RESULTS code. Acknowledging with 001 keeps the poller happy.
        var response = new Dictionary<string, string>
        {
            { "RESULTS", "001" }
        };
        return Content(FormOutputUtil.ToFormOutput(response));
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
}