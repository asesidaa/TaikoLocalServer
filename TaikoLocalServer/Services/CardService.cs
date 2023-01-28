using GameDatabase.Context;
using GameDatabase.Entities;
using SharedProject.Models;
using Swan.Mapping;

namespace TaikoLocalServer.Services;

public class CardService : ICardService
{
    private readonly TaikoDbContext context;

    public CardService(TaikoDbContext context)
    {
        this.context = context;
    }

    public async Task<Card?> GetCardByAccessCode(string accessCode)
    {
        return await context.Cards.FindAsync(accessCode);
    }

    public uint GetNextBaid()
    {
        return context.Cards.Any() ? context.Cards.Max(card => card.Baid) + 1 : 1;
    }

    public async Task<List<User>> GetUsersFromCards()
    {
        return await context.Cards.Select(card => card.CopyPropertiesToNew<User>(null)).ToListAsync();
    }

    public async Task AddCard(Card card)
    {
        context.Add(card);
        await context.SaveChangesAsync();
    }

    public async Task<bool> DeleteCard(string accessCode)
    {
        var card = await context.Cards.FindAsync(accessCode);

        if (card is null) return false;

        context.Cards.Remove(card);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdatePassword(string accessCode, string password, string salt)
    {
        var card = await context.Cards.FindAsync(accessCode);

        if (card is null) return false;

        card.Password = password;
        card.Salt = salt;
        await context.SaveChangesAsync();

        return true;
    }
}