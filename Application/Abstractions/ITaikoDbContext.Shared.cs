namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    DbSet<Card> Cards { get; }
    DbSet<Credential> Credentials { get; }
    DbSet<UserDatum> UserData { get; }
    DbSet<Token> Tokens { get; }
}
