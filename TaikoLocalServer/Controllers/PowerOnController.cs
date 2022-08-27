namespace TaikoLocalServer.Controllers;

[ApiController]
[Route("/sys/servlet/PowerOn")]
public class PowerOnController : ControllerBase
{
    private const string HostStartup = "vsapi.taiko-p.jp";
    
    private readonly ILogger<PowerOnController> logger;
    
    public PowerOnController(ILogger<PowerOnController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    public ContentResult PowerOn([FromForm] PowerOnRequest request)
    {
        logger.LogInformation("Power on request: {Request}",request.Stringify());
        var now = DateTime.Now;
        var response = new Dictionary<string, string>
        {
            {"stat", "1"},
            {"uri", HostStartup},
            {"host", HostStartup},
            {"place_id", "JPN0123"},
            {"name", "NAMCO"},
            {"nickname", "NAMCO"},
            {"region0", "1"},
            {"region_name0", "NAMCO"},
            {"region_name1", "X"},
            {"region_name2", "Y"},
            {"region_name3", "Z"},
            {"country", "JPN"},
            {"allnet_id", "456"},
            {"timezone", "002,00"},
            {"setting", ""},
            {"year", now.Year.ToString()},
            {"month", now.Month.ToString()},
            {"day", now.Day.ToString()},
            {"hour", now.Hour.ToString()},
            {"minute", now.Minute.ToString()},
            {"second", now.Second.ToString()},
            {"res_class", "PowerOnResponseVer2"},
            {"token", "123"}
        };
        return Content(FormOutputUtil.ToFormOutput(response) + '\n');
    }
}