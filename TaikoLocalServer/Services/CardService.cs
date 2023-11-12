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
		List<User> users = new();
		var found = false;
		foreach (var cardEntry in cardEntries)
		{
			foreach (var user in users.Where(user => user.Baid == cardEntry.Baid))
			{
				user.AccessCodes.Add(cardEntry.AccessCode);
				found = true;
			}

			if (!found)
			{
				var user = new User
				{
					Baid = (uint)cardEntry.Baid,
					AccessCodes = new List<string> {cardEntry.AccessCode}
				};
				users.Add(user);
			}

			found = false;
		}
		return users;
	}

	public async Task AddCard(Card card)
	{
		context.Add(card);
		await context.SaveChangesAsync();
	}

	public async Task<bool> DeleteCard(uint baid)
	{
		var cards = await context.Cards.ToListAsync();
		var deletingCards = cards.Where(card => card.Baid == baid).ToList();

		if (deletingCards.Count == 0) return false;

		context.RemoveRange(deletingCards);
		await context.SaveChangesAsync();

		return true;
	}
}