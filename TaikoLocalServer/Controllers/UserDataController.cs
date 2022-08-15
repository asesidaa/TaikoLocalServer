using Microsoft.AspNetCore.Http;

namespace TaikoLocalServer.Controllers;

[Route("/v12r03/chassis/userdata.php")]
[ApiController]
public class UserDataController : ControllerBase
{
    private readonly ILogger<UserDataController> logger;
    public UserDataController(ILogger<UserDataController> logger) {
        this.logger = logger;
    }

    [HttpPost]
    [Produces("application/protobuf")]
    public IActionResult GetUserData([FromBody] UserDataRequest request)
    {
        logger.LogInformation("UserDataController request : {Request}", request.Stringify());

        var response = new UserDataResponse()
        {
            Result = 1
        };

        return Ok(response);
    }
}