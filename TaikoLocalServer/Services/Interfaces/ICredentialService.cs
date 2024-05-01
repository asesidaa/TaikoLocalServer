using SharedProject.Models;

namespace TaikoLocalServer.Services.Interfaces;

public interface ICredentialService
{
    public Task<List<UserCredential>> GetUserCredentialsFromCredentials();
	
    public Task AddCredential(Credential credential);

    public Task<bool> DeleteCredential(uint baid);
	
    public Task<bool> UpdatePassword(uint baid, string password, string salt);

    public Task<Credential?> GetCredentialByBaid(uint baid);
}