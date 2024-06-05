namespace SharedProject.Models.Responses;

public class UsersResponse
{
    public List<User> Users { get; set; } = new();
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 0;
    public int TotalUsers { get; set; } = 0;
}