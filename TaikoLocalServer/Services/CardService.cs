using GameDatabase.Context;
using GameDatabase.Entities;
using OneOf.Types;
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

	public ulong GetNextBaid()
	{
		return context.Cards.Any() ? context.Cards.ToList().Max(card => card.Baid) + 1 : 1;
	}

	public async Task<List<User>> GetUsersFromCards()
	{
		var cardEntries = await context.Cards.ToListAsync();
		var userEntries = await context.UserData.ToListAsync();
		var users = userEntries.Select(userEntry => new User
		{
			Baid = (uint)userEntry.Baid,
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
}