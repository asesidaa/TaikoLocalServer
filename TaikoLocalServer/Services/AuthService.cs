using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameDatabase.Context;
using SharedProject.Models;
using Swan.Mapping;

namespace TaikoLocalServer.Services;

public class AuthService(TaikoDbContext context) : IAuthService
{
    public async Task<Card?> GetCardByAccessCode(string accessCode)
    {
        return await context.Cards.FindAsync(accessCode);
    }
    
    public async Task<User?> GetUserByBaid(uint baid)
    {
        var userDatum = await context.UserData.FindAsync(baid);
        if (userDatum == null) return null;
        var cardEntries = await context.Cards.Where(card => card.Baid == baid).ToListAsync();
        return new User
        {
            Baid = userDatum.Baid,
            AccessCodes = cardEntries.Select(card => card.AccessCode).ToList(),
            IsAdmin = userDatum.IsAdmin
        };
    }

    public async Task<List<User>> GetUsersFromCards()
    {
        var cardEntries = await context.Cards.ToListAsync();
        var userEntries = await context.UserData.ToListAsync();
        var users = userEntries.Select(userEntry => new User
        {
            Baid = userEntry.Baid,
            AccessCodes = cardEntries.Where(cardEntry => cardEntry.Baid == userEntry.Baid).Select(cardEntry => cardEntry.AccessCode).ToList(),
            IsAdmin = userEntry.IsAdmin
        }).ToList();
        return users;
    }
    
    public async Task AddCard(Card card)
    {
        context.Add(card);
        await context.SaveChangesAsync();
    }

    public async Task<bool> DeleteCard(string accessCode)
    {
        var card = await context.Cards.FindAsync(accessCode);
        if (card == null) return false;
        context.Cards.Remove(card);
        await context.SaveChangesAsync();
        return true;
    }
    
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
    
    public (uint baid, bool isAdmin)? ExtractTokenInfo(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            Console.WriteLine("Invalid auth header");
            return null;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            Console.WriteLine("Invalid token");
            return null;
        }
        
        var jwtToken = handler.ReadJwtToken(token);
        if (jwtToken.ValidTo < DateTime.UtcNow)
        {
            Console.WriteLine("Token expired");
            return null;
        }
        
        var claimBaid = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var claimRole = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (claimBaid == null || claimRole == null)
        {
            Console.WriteLine("Invalid token claims");
            return null;
        }

        if (!uint.TryParse(claimBaid, out var baid))
        {
            Console.WriteLine("Invalid baid");
            return null;
        }
        var isAdmin = claimRole == "Admin";
        return (baid, isAdmin);
    }
}