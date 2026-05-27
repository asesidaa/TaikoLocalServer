using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<UserSaveDataBlue> UserSaveDataBlue { get; set; } = null!;

    partial void OnModelCreatingBlue(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSaveDataBlue>(entity =>
        {
            entity.ToTable("UserSaveData_Blue");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
