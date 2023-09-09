using GameDatabase.Entities;
using Microsoft.EntityFrameworkCore;
using SharedProject.Enums;

namespace GameDatabase.Context;

public partial class TaikoDbContext
{
    public virtual DbSet<DanScoreDatum> DanScoreData { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatum> DanStageScoreData { get; set; } = null!;
    public virtual DbSet<AiScoreDatum> AiScoreData { get; set; } = null!;
    public virtual DbSet<AiSectionScoreDatum> AiSectionScoreData { get; set; } = null!;

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DanScoreDatum>(entity =>
        {
            entity.HasKey(e => new { e.Baid, e.DanId });

            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.ClearState).HasConversion<uint>().HasDefaultValue(DanClearState.NotClear);
        });

        modelBuilder.Entity<DanStageScoreDatum>(entity =>
        {
            entity.HasKey(e => new { e.Baid, e.DanId, e.SongNumber });

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.DanStageScoreData)
                .HasPrincipalKey(p => new { p.Baid, p.DanId })
                .HasForeignKey(d => new { d.Baid, d.DanId })
                .OnDelete(DeleteBehavior.Cascade);
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