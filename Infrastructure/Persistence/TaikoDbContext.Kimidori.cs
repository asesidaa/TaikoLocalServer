using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<UserSaveDataKimidori> UserSaveDataKimidori { get; set; } = null!;
    public virtual DbSet<SongBestDatumKimidori> SongBestDataKimidori { get; set; } = null!;
    public virtual DbSet<SongPlayDatumKimidori> SongPlayDataKimidori { get; set; } = null!;
    public virtual DbSet<KimidoriFavoriteSongs> KimidoriFavoriteSongs { get; set; } = null!;
    public virtual DbSet<KimidoriRecentSongs> KimidoriRecentSongs { get; set; } = null!;
    public virtual DbSet<DanScoreDatumKimidori> DanScoreDataKimidori { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatumKimidori> DanStageScoreDataKimidori { get; set; } = null!;

    partial void OnModelCreatingKimidori(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSaveDataKimidori>(entity =>
        {
            entity.ToTable("UserSaveData_Kimidori");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SongBestDatumKimidori>(entity =>
        {
            entity.ToTable("SongBestDatum_Kimidori");
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

        modelBuilder.Entity<SongPlayDatumKimidori>(entity =>
        {
            entity.ToTable("SongPlayDatum_Kimidori");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Baid, e.SongId, e.Difficulty, e.PlayTime });
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PlayTime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Difficulty).HasConversion<uint>();
            entity.Property(e => e.Crown).HasConversion<uint>();
        });

        modelBuilder.Entity<KimidoriFavoriteSongs>(entity =>
        {
            entity.ToTable("KimidoriFavoriteSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<KimidoriRecentSongs>(entity =>
        {
            entity.ToTable("KimidoriRecentSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.Property(e => e.LastPlayed).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanScoreDatumKimidori>(entity =>
        {
            entity.ToTable("DanScoreDatum_Kimidori");
            entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra });
            entity.HasIndex(e => e.MedleyUniqueId);
            entity.Property(e => e.ClearGrade)
                .HasConversion<uint>()
                .HasDefaultValue(Ac15DanClearGrade.NotClear);
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanStageScoreDatumKimidori>(entity =>
        {
            entity.ToTable("DanStageScoreDatum_Kimidori");
            entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra, e.StageIndex });
            entity.HasOne(d => d.Parent)
                .WithMany(p => p.DanStageScoreData)
                .HasForeignKey(d => new { d.Baid, d.DanId, d.IsExtra })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
