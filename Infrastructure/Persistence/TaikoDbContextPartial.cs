using Microsoft.EntityFrameworkCore;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
    }
}
