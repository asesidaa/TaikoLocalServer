using SharedProject.Models;
using SharedProject.Models.Responses;
using Swan.Mapping;
using TaikoLocalServer.Services.Interfaces;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("/api/[controller]")]
public class DashboardController : BaseController<DashboardController>
{
    private readonly TaikoDbContext context;

    private readonly ICardService cardService;

    public DashboardController(TaikoDbContext context, ICardService cardService)
    {
        this.context = context;
        this.cardService = cardService;
    }

    [HttpGet]
    public async Task<DashboardResponse> GetDashboard()
    {
        var users = await cardService.GetUsersFromCards();
        return new DashboardResponse
        {
            Users = users
        };
    }
    
}