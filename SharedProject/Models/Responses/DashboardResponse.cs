namespace SharedProject.Models.Responses;

public class DashboardResponse
{
    public List<User> Users { get; set; } = new();
    
    public List<UserCredential> UserCredentials { get; set; } = new();
}