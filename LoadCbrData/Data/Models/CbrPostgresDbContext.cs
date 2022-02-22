using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LoadCbrData.Data.Models
{
    public partial class CbrPostgresDbContext : CbrDbContext
    {
        public CbrPostgresDbContext(DbContextOptions<CbrPostgresDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Record>(entity =>
            {
                entity.ToTable("record");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Code)
                    .HasMaxLength(32)
                    .HasColumnName("code");

                entity.Property(e => e.Data).HasColumnName("data");

                entity.Property(e => e.Inn)
                    .HasMaxLength(32)
                    .HasColumnName("inn");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("modified_date");

                entity.Property(e => e.Name)
                    .HasMaxLength(512)
                    .HasColumnName("name");

                entity.Property(e => e.Ogrn)
                    .HasMaxLength(32)
                    .HasColumnName("ogrn");

                entity.Property(e => e.ShortName)
                    .HasMaxLength(512)
                    .HasColumnName("short_name");
            });

            modelBuilder.Entity<RecordId>(entity =>
            {
                entity.ToTable("record_id");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
