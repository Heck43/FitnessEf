using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ExistingClient = FitnessEf.ExistingData.Models.Client;

namespace FitnessEf.ExistingData;

public partial class FitnessExistingContext : DbContext
{
    public FitnessExistingContext(DbContextOptions<FitnessExistingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ExistingClient> Clients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExistingClient>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("clients_pkey");

            entity.ToTable("clients");

            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}