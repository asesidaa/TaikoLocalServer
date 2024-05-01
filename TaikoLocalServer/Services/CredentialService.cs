using GameDatabase.Context;
using SharedProject.Models;
using Swan.Mapping;

namespace TaikoLocalServer.Services;

public class CredentialService(TaikoDbContext context) : ICredentialService
{
    public async Task<List<UserCredential>> GetUserCredentialsFromCredentials()
    {
        return await context.Credentials.Select(credential => credential.CopyPropertiesToNew<UserCredential>(null)).ToListAsync();
    }

    public async Task AddCredential(Credential credential)
    {
        context.Add(credential);
        await context.SaveChangesAsync();
    }

    public async Task<bool> DeleteCredential(uint baid)
    {
        var credential = await context.Credentials.FindAsync(baid);

        if (credential is null) return false;

        context.Credentials.Remove(credential);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdatePassword(uint baid, string password, string salt)
    {
        var credential = await context.Credentials.FindAsync(baid);

        if (credential is null) return false;

        credential.Password = password;
        credential.Salt = salt;
        await context.SaveChangesAsync();

        return true;
    }
    
    public async Task<Credential?> GetCredentialByBaid(uint baid)
    {
        return await context.Credentials.FindAsync(baid);
    }
}