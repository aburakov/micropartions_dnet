using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Micropartions;

public partial class MicropartionsContext : DbContext
{
    public MicropartionsContext()
    {
    }

    public MicropartionsContext(DbContextOptions<MicropartionsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MicropartionsManager> Micropartions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=172.16.0.99:7890;Username=postgres;Password=SPMH?&BV58u&Gk[3;Database=micropartions");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MicropartionsManager>(entity =>
        {
            entity.HasKey(e => new { e.Micropartionguid, e.Skuserial, e.Operationguid, e.Operationnumber }).HasName("micropartions_pkey");

            entity.ToTable("micropartions");

            entity.Property(e => e.Micropartionguid)
                .HasColumnType("character varying")
                .HasColumnName("micropartionguid");
            entity.Property(e => e.Skuserial)
                .HasColumnType("character varying")
                .HasColumnName("skuserial");
            entity.Property(e => e.Operationguid)
                .HasColumnType("character varying")
                .HasColumnName("operationguid");
            entity.Property(e => e.Operationnumber).HasColumnName("operationnumber");
            entity.Property(e => e.Boxserial)
                .HasColumnType("character varying")
                .HasColumnName("boxserial");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
