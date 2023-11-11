﻿// <auto-generated />
using System;
using GameDatabase.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GameDatabase.Migrations
{
    [DbContext(typeof(TaikoDbContext))]
    [Migration("20231111155016_CopyPasswordSaltFromCardToCredential")]
    partial class CopyPasswordSaltFromCardToCredential
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0-rc.2.23480.1");

            modelBuilder.Entity("GameDatabase.Entities.AiScoreDatum", b =>
                {
                    b.Property<ulong>("Baid")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SongId")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Difficulty")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsWin")
                        .HasColumnType("INTEGER");

                    b.HasKey("Baid", "SongId", "Difficulty");

                    b.ToTable("AiScoreData");
                });

            modelBuilder.Entity("GameDatabase.Entities.AiSectionScoreDatum", b =>
                {
                    b.Property<ulong>("Baid")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SongId")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Difficulty")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SectionIndex")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Crown")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("DrumrollCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("GoodCount")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsWin")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("MissCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("OkCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Score")
                        .HasColumnType("INTEGER");

                    b.HasKey("Baid", "SongId", "Difficulty", "SectionIndex");

                    b.ToTable("AiSectionScoreData");
                });

            modelBuilder.Entity("GameDatabase.Entities.Card", b =>
                {
                    b.Property<string>("AccessCode")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("Baid")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("AccessCode");

                    b.HasIndex(new[] { "Baid" }, "IX_Card_Baid")
                        .IsUnique();

                    b.ToTable("Card", (string)null);
                });

            modelBuilder.Entity("GameDatabase.Entities.Credential", b =>
                {
                    b.Property<ulong>("Baid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Baid");

                    b.ToTable("Credential", (string)null);
                });

            modelBuilder.Entity("GameDatabase.Entities.DanScoreDatum", b =>
                {
                    b.Property<ulong>("Baid")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("DanId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DanType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(1);

                    b.Property<uint>("ArrivalSongCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("ClearState")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0u);

                    b.Property<uint>("ComboCountTotal")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SoulGaugeTotal")
                        .HasColumnType("INTEGER");

                    b.HasKey("Baid", "DanId", "DanType");

                    b.ToTable("DanScoreData");
                });

            modelBuilder.Entity("GameDatabase.Entities.DanStageScoreDatum", b =>
                {
                    b.Property<ulong>("Baid")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("DanId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DanType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(1);

                    b.Property<uint>("SongNumber")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("BadCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("ComboCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("DrumrollCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("GoodCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("HighScore")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("OkCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("PlayScore")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("TotalHitCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("Baid", "DanId", "DanType", "SongNumber");

                    b.ToTable("DanStageScoreData");
                });

            modelBuilder.Entity("GameDatabase.Entities.SongBestDatum", b =>
                {
                    b.Property<ulong>("Baid")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SongId")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Difficulty")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("BestCrown")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("BestRate")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("BestScore")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("BestScoreRank")
                        .HasColumnType("INTEGER");

                    b.HasKey("Baid", "SongId", "Difficulty");

                    b.ToTable("SongBestData");
                });

            modelBuilder.Entity("GameDatabase.Entities.SongPlayDatum", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Baid")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("ComboCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Crown")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("Difficulty")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("DrumrollCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("GoodCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("HitCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("MissCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("OkCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("PlayTime")
                        .HasColumnType("datetime");

                    b.Property<uint>("Score")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("ScoreRank")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("ScoreRate")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Skipped")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SongId")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SongNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Baid");

                    b.ToTable("SongPlayData");
                });

            modelBuilder.Entity("GameDatabase.Entities.UserDatum", b =>
                {
                    b.Property<ulong>("Baid")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("AchievementDisplayDifficulty")
                        .HasColumnType("INTEGER");

                    b.Property<int>("AiWinCount")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("ColorBody")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("ColorFace")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("ColorLimb")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CostumeData")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CostumeFlgArray")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("DifficultyPlayedArray")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("DifficultySettingArray")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("DisplayAchievement")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DisplayDan")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FavoriteSongsArray")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("GenericInfoFlgArray")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSkipOn")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsVoiceOn")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastPlayDatetime")
                        .HasColumnType("datetime");

                    b.Property<uint>("LastPlayMode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MyDonName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<uint>("MyDonNameLanguage")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NotesPosition")
                        .HasColumnType("INTEGER");

                    b.Property<short>("OptionSetting")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("SelectedToneId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TitleFlgArray")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<uint>("TitlePlateId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TokenCountDict")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ToneFlgArray")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UnlockedSongIdList")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Baid");

                    b.ToTable("UserData");
                });

            modelBuilder.Entity("GameDatabase.Entities.AiScoreDatum", b =>
                {
                    b.HasOne("GameDatabase.Entities.Card", "Ba")
                        .WithMany()
                        .HasForeignKey("Baid")
                        .HasPrincipalKey("Baid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ba");
                });

            modelBuilder.Entity("GameDatabase.Entities.AiSectionScoreDatum", b =>
                {
                    b.HasOne("GameDatabase.Entities.AiScoreDatum", "Parent")
                        .WithMany("AiSectionScoreData")
                        .HasForeignKey("Baid", "SongId", "Difficulty")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("GameDatabase.Entities.DanScoreDatum", b =>
                {
                    b.HasOne("GameDatabase.Entities.Card", "Ba")
                        .WithMany()
                        .HasForeignKey("Baid")
                        .HasPrincipalKey("Baid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ba");
                });

            modelBuilder.Entity("GameDatabase.Entities.DanStageScoreDatum", b =>
                {
                    b.HasOne("GameDatabase.Entities.DanScoreDatum", "Parent")
                        .WithMany("DanStageScoreData")
                        .HasForeignKey("Baid", "DanId", "DanType")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("GameDatabase.Entities.SongBestDatum", b =>
                {
                    b.HasOne("GameDatabase.Entities.Card", "Ba")
                        .WithMany()
                        .HasForeignKey("Baid")
                        .HasPrincipalKey("Baid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ba");
                });

            modelBuilder.Entity("GameDatabase.Entities.SongPlayDatum", b =>
                {
                    b.HasOne("GameDatabase.Entities.Card", "Ba")
                        .WithMany()
                        .HasForeignKey("Baid")
                        .HasPrincipalKey("Baid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ba");
                });

            modelBuilder.Entity("GameDatabase.Entities.UserDatum", b =>
                {
                    b.HasOne("GameDatabase.Entities.Card", "Ba")
                        .WithMany()
                        .HasForeignKey("Baid")
                        .HasPrincipalKey("Baid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ba");
                });

            modelBuilder.Entity("GameDatabase.Entities.AiScoreDatum", b =>
                {
                    b.Navigation("AiSectionScoreData");
                });

            modelBuilder.Entity("GameDatabase.Entities.DanScoreDatum", b =>
                {
                    b.Navigation("DanStageScoreData");
                });
#pragma warning restore 612, 618
        }
    }
}
