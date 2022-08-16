using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ApiSample.Schema
{
    public partial class YourGasUtilityContext : DbContext
    {
        public YourGasUtilityContext()
        {
        }

        public YourGasUtilityContext(DbContextOptions<YourGasUtilityContext> options)
            : base(options)
        {
        }

        public virtual DbSet<LoadForecast> LoadForecasts { get; set; } = null!;
        public virtual DbSet<LoadForecastValue> LoadForecastValues { get; set; } = null!;
        public virtual DbSet<LoadObservation> LoadObservations { get; set; } = null!;
        public virtual DbSet<OpArea> OpAreas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoadForecast>(entity =>
            {
                entity.ToTable("LoadForecast");

                entity.HasIndex(e => new { e.Date, e.OpArea }, "AK_LoadForecast")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.HasOne(d => d.OpAreaNavigation)
                    .WithMany(p => p.LoadForecasts)
                    .HasForeignKey(d => d.OpArea)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LoadForecast_OpArea");
            });

            modelBuilder.Entity<LoadForecastValue>(entity =>
            {
                entity.HasKey(e => new { e.Forecast, e.Horizon });

                entity.ToTable("LoadForecastValue");

                entity.HasOne(d => d.ForecastNavigation)
                    .WithMany(p => p.LoadForecastValues)
                    .HasForeignKey(d => d.Forecast)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LoadForecastValue_LoadForecast");
            });

            modelBuilder.Entity<LoadObservation>(entity =>
            {
                entity.HasKey(e => new { e.Date, e.OpArea });

                entity.ToTable("LoadObservation");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.HasOne(d => d.OpAreaNavigation)
                    .WithMany(p => p.LoadObservations)
                    .HasForeignKey(d => d.OpArea)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LoadObservation_OpArea");
            });

            modelBuilder.Entity<OpArea>(entity =>
            {
                entity.ToTable("OpArea");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
