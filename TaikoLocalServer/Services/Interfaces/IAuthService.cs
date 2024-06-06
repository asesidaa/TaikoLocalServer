using SharedProject.Models;
using SharedProject.Models.Responses;	

namespace TaikoLocalServer.Services.Interfaces;

public interface IAuthService
{
	public Task<User?> GetUserByBaid(uint baid);
	
    public Task<Card?> GetCardByAccessCode(string accessCode);

    public Task<UsersResponse> GetUsersFromCards(int page = 0, int limit = 12, string? searchTerm = null);

    public Task AddCard(Card card);

    public Task<bool> DeleteCard(string accessCode);
    
    public Task<List<UserCredential>> GetUserCredentialsFromCredentials();
	
    public Task AddCredential(Credential credential);

    public Task<bool> DeleteCredential(uint baid);
	
    public Task<bool> UpdatePassword(uint baid, string password, string salt);

    public Task<Credential?> GetCredentialByBaid(uint baid);

    public (uint baid, bool isAdmin)? ExtractTokenInfo(HttpContext httpContext);
}