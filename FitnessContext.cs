using System;
using Microsoft.EntityFrameworkCore;

namespace FitnessEf;

public sealed class FitnessContext : DbContext
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Visit> Visits => Set<Visit>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("FITNESS_EF_CONNECTION")
                ?? "Host=localhost;Port=5432;Database=FitnessEfTraining;Username=heck43";

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Не задана переменная FITNESS_EF_CONNECTION.");

            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("clients");
            entity.HasKey(c => c.ClientId);
            entity.Property(c => c.ClientId).HasColumnName("client_id").ValueGeneratedOnAdd();
            entity.Property(c => c.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            entity.Property(c => c.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
            entity.Property(c => c.BirthDate).HasColumnName("birth_date").HasColumnType("date");
            entity.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(30);
            entity.Property(c => c.Email).HasColumnName("email").HasMaxLength(254);
            entity.Property(c => c.RegistrationDate).HasColumnName("registration_date").HasColumnType("date").IsRequired();
            entity.Property(c => c.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.ToTable("visits");
            entity.HasKey(v => v.VisitId);
            entity.Property(v => v.VisitId)
                .HasColumnName("visit_id")
                .ValueGeneratedOnAdd();

            entity.Property(v => v.ClientId)
                .HasColumnName("client_id")
                .IsRequired();

            entity.Property(v => v.VisitDate)
                .HasColumnName("visit_date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(v => v.Note)
                .HasColumnName("note")
                .HasMaxLength(500);

            entity.HasOne(v => v.Client)
                .WithMany(c => c.Visits)
                .HasForeignKey(v => v.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}