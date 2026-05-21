using TaikoLocalServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<SongBestDatumNijiiro> SongBestDataNijiiro { get; set; } = null!;
    public virtual DbSet<SongPlayDatumNijiiro> SongPlayDataNijiiro { get; set; } = null!;
    public virtual DbSet<DanScoreDatumNijiiro> DanScoreDataNijiiro { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatumNijiiro> DanStageScoreDataNijiiro { get; set; } = null!;
    public virtual DbSet<AiScoreDatumNijiiro> AiScoreDataNijiiro { get; set; } = null!;
    public virtual DbSet<AiSectionScoreDatumNijiiro> AiSectionScoreDataNijiiro { get; set; } = null!;
    public virtual DbSet<UserSaveDataNijiiro> UserSaveDataNijiiro { get; set; } = null!;

    partial void OnModelCreatingNijiiro(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SongBestDatumNijiiro>(entity =>
        {
            entity.ToTable("SongBestDatum_Nijiiro");
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty });

            // Leaderboard queries filter+order on (SongId, Difficulty, BestScore); the PK starts with Baid so it cannot serve them.
            entity.HasIndex(e => new { e.SongId, e.Difficulty, e.BestScore });

            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Difficulty)
                .HasConversion<uint>();

            entity.Property(e => e.BestCrown)
                .HasConversion<uint>();

            entity.Property(e => e.BestScoreRank)
                .HasConversion<uint>();
        });

        modelBuilder.Entity<SongPlayDatumNijiiro>(entity =>
        {
            entity.ToTable("SongPlayDatum_Nijiiro");
            entity.HasKey(e => e.Id);

            // Per-song history + ghost lookups filter by (Baid, SongId, Difficulty) and sort by PlayTime; existing index on Baid alone scans whole user history.
            entity.HasIndex(e => new { e.Baid, e.SongId, e.Difficulty, e.PlayTime });

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.PlayTime).HasColumnType("datetime");

            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Difficulty)
                .HasConversion<uint>();

            entity.Property(e => e.ScoreRank)
                .HasConversion<uint>();

            entity.Property(e => e.Crown)
                .HasConversion<uint>();
        });

        modelBuilder.Entity<UserSaveDataNijiiro>(entity =>
        {
            entity.ToTable("UserSaveData_Nijiiro");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.LastPlayDatetime).HasColumnType("datetime");
            entity.Property(e => e.AchievementDisplayDifficulty).HasConversion<uint>();
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanScoreDatumNijiiro>(entity =>
        {
            entity.ToTable("DanScoreDatum_Nijiiro");
            entity.HasKey(e => new { e.Baid, e.DanId, e.DanType });

            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.ClearState).HasConversion<uint>().HasDefaultValue(DanClearState.NotClear);
            entity.Property(e => e.DanType).HasConversion<int>().HasDefaultValue(DanType.Normal).HasSentinel((DanType)0);
        });

        modelBuilder.Entity<DanStageScoreDatumNijiiro>(entity =>
        {
            entity.ToTable("DanStageScoreDatum_Nijiiro");
            entity.HasKey(e => new { e.Baid, e.DanId, e.DanType, e.SongNumber });

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.DanStageScoreData)
                .HasForeignKey(d => new { d.Baid, d.DanId, d.DanType })
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.DanType).HasConversion<int>().HasDefaultValue(DanType.Normal).HasSentinel((DanType)0);
        });

        modelBuilder.Entity<AiScoreDatumNijiiro>(entity =>
        {
            entity.ToTable("AiScoreDatum_Nijiiro");
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty });

            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiSectionScoreDatumNijiiro>(entity =>
        {
            entity.ToTable("AiSectionScoreDatum_Nijiiro");
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty, e.SectionIndex });

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.AiSectionScoreData)
                .HasPrincipalKey(p => new { p.Baid, p.SongId, p.Difficulty })
                .HasForeignKey(d => new { d.Baid, d.SongId, d.Difficulty })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
