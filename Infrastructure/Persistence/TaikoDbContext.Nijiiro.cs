using TaikoLocalServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<SongBestDatum> SongBestData { get; set; } = null!;
    public virtual DbSet<SongPlayDatum> SongPlayData { get; set; } = null!;
    public virtual DbSet<DanScoreDatum> DanScoreData { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatum> DanStageScoreData { get; set; } = null!;
    public virtual DbSet<AiScoreDatum> AiScoreData { get; set; } = null!;
    public virtual DbSet<AiSectionScoreDatum> AiSectionScoreData { get; set; } = null!;

    partial void OnModelCreatingNijiiro(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SongBestDatum>(entity =>
        {
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty });

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

        modelBuilder.Entity<SongPlayDatum>(entity =>
        {
            entity.HasKey(e => e.Id);

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

        modelBuilder.Entity<DanScoreDatum>(entity =>
        {
            entity.HasKey(e => new { e.Baid, e.DanId, e.DanType });

            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.ClearState).HasConversion<uint>().HasDefaultValue(DanClearState.NotClear);
            entity.Property(e => e.DanType).HasConversion<int>().HasDefaultValue(DanType.Normal).HasSentinel((DanType)0);
        });

        modelBuilder.Entity<DanStageScoreDatum>(entity =>
        {
            entity.HasKey(e => new { e.Baid, e.DanId, e.DanType, e.SongNumber });

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.DanStageScoreData)
                .HasForeignKey(d => new { d.Baid, d.DanId, d.DanType })
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.DanType).HasConversion<int>().HasDefaultValue(DanType.Normal).HasSentinel((DanType)0);
        });

        modelBuilder.Entity<AiScoreDatum>(entity =>
        {
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty });

            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiSectionScoreDatum>(entity =>
        {
            entity.HasKey(e => new { e.Baid, e.SongId, e.Difficulty, e.SectionIndex });

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.AiSectionScoreData)
                .HasPrincipalKey(p => new { p.Baid, p.SongId, p.Difficulty })
                .HasForeignKey(d => new { d.Baid, d.SongId, d.Difficulty })
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
