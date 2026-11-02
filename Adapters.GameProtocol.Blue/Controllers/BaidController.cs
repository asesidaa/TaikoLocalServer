namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Controllers;

[ApiController]
[Route("/v10r03/chassis/baidcheck.php")]
public class BaidController : BaseProtocolController<BaidController>
{
    [HttpPost]
    [Produces("application/protobuf")]
    public async Task<IActionResult> Baid([FromBody] BAIDRequest request)
    {
        Logger.LogInformation("Blue BAID request: {@Request}", request);
        var common = await Mediator.Send(new Ac15BaidQuery(GameEra.Blue, request.AccessCode), HttpContext.RequestAborted);

        if (common.IsNewUser)
        {
            Logger.LogInformation("New Blue user with access code {AccessCode}", request.AccessCode);

            return Ok(new BAIDResponse
            {
                Result = 1,
                PlayerType = 1,
                Baid = common.Baid
            });
        }

        var response = new BAIDResponse
        {
            Result = common.Result,
            Baid = common.Baid,
            AccessCode = request.AccessCode,
            IsPublish = true,
            PlayerType = 0,
            ComSvrResult = 1,
            RegCountryId = "JPN",
            MbId = 1,
            PurposeId = 1,
            RegionId = 1,
            ContentInfo = new byte[BlueProtocolBytes.ContentInfoBytes]
        };
        ApplySections(common, response);
        response.Personid = "1";

        return Ok(response);
    }

    private static void ApplySections(Ac15BaidResponse common, BAIDResponse response)
    {
        if (common.Identity is { } identity)
        {
            BaidResponseMapper.Apply(identity, response);
        }

        if (common.MydonProfile is { } profile)
        {
            BaidResponseMapper.Apply(profile, response);
        }

        if (common.CustomizationInventory is { } inventory)
        {
            BaidResponseMapper.Apply(inventory, response);
        }

        if (common.ShopMedalBalance is { } medals)
        {
            BaidResponseMapper.Apply(medals, response);
        }

        if (common.DanStatus is { } dan)
        {
            BaidResponseMapper.Apply(dan, response);
        }

        if (common.CompatibilityProfile is { } compatibility)
        {
            BaidResponseMapper.Apply(compatibility, response);
        }
    }
}
