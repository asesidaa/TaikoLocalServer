using Microsoft.EntityFrameworkCore;

namespace TaikoLocalServer.Context;

public partial class TaikoDbContext
{
    public virtual DbSet<DanScoreDatum> DanScoreData { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatum> DanStageScoreData { get; set; } = null!;

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
        });

        modelBuilder.Entity<DanStageScoreDatum>(entity =>
        {
            entity.HasKey(e => new { e.Baid, e.DanId, e.SongNumber });

            entity.HasOne(d => d.Parent)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}