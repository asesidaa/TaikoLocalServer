using SharedProject.Models;

namespace TaikoLocalServer.Services.Interfaces;

public interface IAuthService
{
	public Task<User?> GetUserByBaid(uint baid);
	
    public Task<Card?> GetCardByAccessCode(string accessCode);

    public Task<List<User>> GetUsersFromCards();

    public Task AddCard(Card card);

    public Task<bool> DeleteCard(string accessCode);
    
    public Task<List<UserCredential>> GetUserCredentialsFromCredentials();
	
    public Task AddCredential(Credential credential);

    public Task<bool> DeleteCredential(uint baid);
	
    public Task<bool> UpdatePassword(uint baid, string password, string salt);

    public Task<Credential?> GetCredentialByBaid(uint baid);

    public (uint baid, bool isAdmin)? ExtractTokenInfo(HttpContext httpContext);
}