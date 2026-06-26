using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<UserSaveDataMomoiro> UserSaveDataMomoiro { get; set; } = null!;
    public virtual DbSet<SongBestDatumMomoiro> SongBestDataMomoiro { get; set; } = null!;
    public virtual DbSet<MomoiroFavoriteSongs> MomoiroFavoriteSongs { get; set; } = null!;
    public virtual DbSet<MomoiroRecentSongs> MomoiroRecentSongs { get; set; } = null!;

    partial void OnModelCreatingMomoiro(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSaveDataMomoiro>(entity =>
        {
            entity.ToTable("UserSaveData_Momoiro");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SongBestDatumMomoiro>(entity =>
        {
            entity.ToTable("SongBestDatum_Momoiro");
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty, e.IsShin });
            entity.HasIndex(e => new { e.SongId, e.Difficulty, e.BestScore });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.BestCrown).HasConversion<uint>();
        });

        modelBuilder.Entity<MomoiroFavoriteSongs>(entity =>
        {
            entity.ToTable("MomoiroFavoriteSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.HasIndex(e => new { e.Baid, e.DisplayOrder });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MomoiroRecentSongs>(entity =>
        {
            entity.ToTable("MomoiroRecentSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.Property(e => e.LastPlayed).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
