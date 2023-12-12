using SharedProject.Models.Responses;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("/api/[controller]")]
public class DashboardController : BaseController<DashboardController>
{
    private readonly ICardService cardService;
    private readonly ICredentialService credentialService;

    public DashboardController(ICardService cardService, ICredentialService credentialService)
    {
        this.cardService = cardService;
        this.credentialService = credentialService;
    }

    [HttpGet]
    public async Task<DashboardResponse> GetDashboard()
    {
        var users = await cardService.GetUsersFromCards();
        var credentials = await credentialService.GetUserCredentialsFromCredentials();
        return new DashboardResponse
        {
            Users = users,
            UserCredentials = credentials
        };
    }
    
}