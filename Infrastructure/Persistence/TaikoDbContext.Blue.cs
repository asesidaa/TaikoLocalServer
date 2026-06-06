using Microsoft.EntityFrameworkCore;
using TaikoLocalServer.Domain.Entities;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.Persistence;

public partial class TaikoDbContext
{
    public virtual DbSet<UserSaveDataBlue> UserSaveDataBlue { get; set; } = null!;
    public virtual DbSet<SongBestDatumBlue> SongBestDataBlue { get; set; } = null!;
    public virtual DbSet<SongPlayDatumBlue> SongPlayDataBlue { get; set; } = null!;
    public virtual DbSet<BlueFavoriteSongs> BlueFavoriteSongs { get; set; } = null!;
    public virtual DbSet<BlueRecentSongs> BlueRecentSongs { get; set; } = null!;
    public virtual DbSet<DanScoreDatumBlue> DanScoreDataBlue { get; set; } = null!;
    public virtual DbSet<DanStageScoreDatumBlue> DanStageScoreDataBlue { get; set; } = null!;
    public virtual DbSet<BlueShopSeasonState> BlueShopSeasonStates { get; set; } = null!;
    public virtual DbSet<BlueShopItemState> BlueShopItemStates { get; set; } = null!;
    public virtual DbSet<BlueBattleUserState> BlueBattleUserStates { get; set; } = null!;
    public virtual DbSet<BlueBattleNpcState> BlueBattleNpcStates { get; set; } = null!;
    public virtual DbSet<BlueBattleTokenState> BlueBattleTokenStates { get; set; } = null!;
    public virtual DbSet<BlueBattleStageResult> BlueBattleStageResults { get; set; } = null!;
    public virtual DbSet<BlueTokkunStageResult> BlueTokkunStageResults { get; set; } = null!;

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

        modelBuilder.Entity<SongBestDatumBlue>(entity =>
        {
            entity.ToTable("SongBestDatum_Blue");
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

        modelBuilder.Entity<SongPlayDatumBlue>(entity =>
        {
            entity.ToTable("SongPlayDatum_Blue");
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

        modelBuilder.Entity<BlueFavoriteSongs>(entity =>
        {
            entity.ToTable("BlueFavoriteSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlueRecentSongs>(entity =>
        {
            entity.ToTable("BlueRecentSongs");
            entity.HasKey(e => new { e.Baid, e.SongNo });
            entity.Property(e => e.LastPlayed).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanScoreDatumBlue>(entity =>
        {
            entity.ToTable("DanScoreDatum_Blue");
            entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra });
            entity.HasIndex(e => e.MedleyUniqueId);
            entity.Property(e => e.ClearGrade)
                .HasConversion<uint>()
                .HasDefaultValue(BlueDanClearGrade.NotClear);
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanStageScoreDatumBlue>(entity =>
        {
            entity.ToTable("DanStageScoreDatum_Blue");
            entity.HasKey(e => new { e.Baid, e.DanId, e.IsExtra, e.StageIndex });
            entity.HasOne(d => d.Parent)
                .WithMany(p => p.DanStageScoreData)
                .HasForeignKey(d => new { d.Baid, d.DanId, d.IsExtra })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlueShopSeasonState>(entity =>
        {
            entity.ToTable("BlueShopSeasonStates");
            entity.HasKey(e => new { e.Baid, e.SeasonId });
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlueShopItemState>(entity =>
        {
            entity.ToTable("BlueShopItemStates");
            entity.HasKey(e => new { e.Baid, e.SeasonId, e.ItemType, e.ItemId });
            entity.Property(e => e.Status).HasConversion<uint>();
            entity.Property(e => e.PurchasedAt).HasColumnType("datetime");
            entity.Property(e => e.UnlockedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlueBattleUserState>(entity =>
        {
            entity.ToTable("BlueBattleUserStates");
            entity.HasKey(e => e.Baid);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlueBattleNpcState>(entity =>
        {
            entity.ToTable("BlueBattleNpcStates");
            entity.HasKey(e => new { e.Baid, e.NpcId });
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlueBattleTokenState>(entity =>
        {
            entity.ToTable("BlueBattleTokenStates");
            entity.HasKey(e => new { e.Baid, e.TokenId });
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlueBattleStageResult>(entity =>
        {
            entity.ToTable("BlueBattleStageResults");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Baid, e.PlayDatetime });
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PlayDatetime).HasColumnType("datetime");
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlueTokkunStageResult>(entity =>
        {
            entity.ToTable("BlueTokkunStageResults");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Baid, e.PlayDatetime });
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PlayDatetime).IsRequired();
            entity.Property(e => e.BanacoinDatetime).IsRequired();
            entity.Property(e => e.TookunSongnoesJson).IsRequired();
            entity.HasOne(d => d.Ba)
                .WithMany()
                .HasPrincipalKey(p => p.Baid)
                .HasForeignKey(d => d.Baid)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
