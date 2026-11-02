namespace TaikoLocalServer.Tests.Nijiiro;

// A new card's player number (Application/Handlers/AddMyDonEntryCommand.cs), on an in-memory database.
public sealed class NewPlayerNumberTests
{
    [Fact]
    public async Task NewPlayer_DoesNotReuseTheNewestNumberAfterItsCardsAreRemoved()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var db = new TaikoDbContext(new DbContextOptionsBuilder<TaikoDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        var entry = new AddMyDonEntryCommandHandler(db, NullLogger<AddMyDonEntryCommandHandler>.Instance);
        await entry.Handle(new AddMyDonEntryCommand(GameEra.Nijiiro, "card-1", "A", 0), default);
        var second = await entry.Handle(new AddMyDonEntryCommand(GameEra.Nijiiro, "card-2", "B", 0), default);
        db.Cards.Remove(db.Cards.Single(c => c.Baid == second.Baid));
        await db.SaveChangesAsync();
        var third = await entry.Handle(new AddMyDonEntryCommand(GameEra.Nijiiro, "card-3", "C", 0), default);
        Assert.Equal(second.Baid + 1, third.Baid);
    }
}
