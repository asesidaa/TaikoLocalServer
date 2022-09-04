using SharedProject.Models;
using SharedProject.Models.Responses;
using Swan.Mapping;

namespace TaikoLocalServer.Controllers.Api;

[ApiController]
[Route("/api/[controller]")]
public class DashboardController : BaseController<DashboardController>
{
    private readonly TaikoDbContext context;

    public DashboardController(TaikoDbContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public DashboardResponse GetDashboard()
    {
        var cards = context.Cards.AsEnumerable();
        var users = cards.Select(card => new User
        {
            AccessCode = card.AccessCode,
            Baid = card.Baid
        }).ToList();
        return new DashboardResponse
        {
            Users = users
        };
    }
    
}