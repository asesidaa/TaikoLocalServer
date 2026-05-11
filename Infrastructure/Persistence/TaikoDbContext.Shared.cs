using TaikoLocalServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<Card> Cards { get; set; } = null!;
    public virtual DbSet<Credential> Credentials { get; set; } = null!;
    public virtual DbSet<UserDatum> UserData { get; set; } = null!;
    public virtual DbSet<Token> Tokens { get; set; } = null!;

    partial void OnModelCreatingShared(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.AccessCode);

            entity.ToTable("Card");

            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Credential>(entity =>
        {
            entity.HasKey(e => e.Baid);

            entity.ToTable("Credential");

            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserDatum>(entity =>
        {
            entity.HasKey(e => e.Baid);
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => new { e.Baid, e.Id });

            entity.HasOne(d => d.Datum)
                .WithMany(p => p.Tokens)
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
