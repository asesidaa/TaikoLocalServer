namespace TaikoLocalServer.Application.Abstractions;

public partial interface ITaikoDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
