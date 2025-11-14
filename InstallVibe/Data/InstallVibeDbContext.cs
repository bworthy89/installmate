using System;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using InstallVibe.Models;

namespace InstallVibe.Data;

public class InstallVibeDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Guide> Guides { get; set; } = null!;
    public DbSet<Step> Steps { get; set; } = null!;
    public DbSet<MediaItem> MediaItems { get; set; } = null!;
    public DbSet<GuideProgress> GuideProgresses { get; set; } = null!;

    public InstallVibeDbContext(DbContextOptions<InstallVibeDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired();
        });

        // Guide configuration
        modelBuilder.Entity<Guide>(entity =>
        {
            entity.ToTable("Guides");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedByUserId).IsRequired();
            entity.Property(e => e.EstimatedDurationMinutes);

            // Relationship to User
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship to Steps (cascade delete)
            entity.HasMany(e => e.Steps)
                .WithOne(s => s.Guide)
                .HasForeignKey(s => s.GuideId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Step configuration
        modelBuilder.Entity<Step>(entity =>
        {
            entity.ToTable("Steps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GuideId).IsRequired();
            entity.Property(e => e.StepNumber).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Instruction).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.RequiredTools).HasMaxLength(500);
            entity.Property(e => e.SafetyNotes).HasMaxLength(500);

            // Index for ordering steps
            entity.HasIndex(e => new { e.GuideId, e.StepNumber });

            // Relationship to MediaItems (cascade delete)
            entity.HasMany(e => e.Media)
                .WithOne(m => m.Step)
                .HasForeignKey(m => m.StepId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MediaItem configuration
        modelBuilder.Entity<MediaItem>(entity =>
        {
            entity.ToTable("MediaItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StepId).IsRequired();
            entity.Property(e => e.MediaType).IsRequired();
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
        });

        // GuideProgress configuration
        modelBuilder.Entity<GuideProgress>(entity =>
        {
            entity.ToTable("GuideProgresses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GuideId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.LastUpdated).IsRequired();

            // JSON converter for CompletedStepIds
            entity.Property(e => e.CompletedStepIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>()
                )
                .HasColumnType("TEXT");

            // Unique constraint: one progress record per user per guide
            entity.HasIndex(e => new { e.GuideId, e.UserId }).IsUnique();

            // Relationships
            entity.HasOne(e => e.Guide)
                .WithMany()
                .HasForeignKey(e => e.GuideId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed admin user for EF Core database
        // Note: The password hash here is for "admin123" using PBKDF2
        // This matches the legacy auth database seeding
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = "placeholder", // Not used for auth - legacy DB is used
                Role = UserRole.Admin
            }
        );

        // Seed sample guide
        modelBuilder.Entity<Guide>().HasData(
            new Guide
            {
                Id = 1,
                Title = "Standard HVAC Unit Installation",
                Description = "Complete installation guide for residential HVAC units. Includes safety procedures, mounting instructions, and electrical connections.",
                Category = "HVAC",
                CreatedByUserId = 1, // Admin user
                EstimatedDurationMinutes = 180
            }
        );

        // Seed steps
        modelBuilder.Entity<Step>().HasData(
            new Step
            {
                Id = 1,
                GuideId = 1,
                StepNumber = 1,
                Title = "Pre-installation Safety Check",
                Instruction = "Before beginning installation, ensure all power to the installation area is shut off at the circuit breaker. Verify the power is off using a voltage tester. Wear appropriate PPE including safety glasses and work gloves.",
                RequiredTools = "Voltage tester, Safety glasses, Work gloves",
                SafetyNotes = "DANGER: Always verify power is off before working with electrical equipment. Lock out and tag the breaker box to prevent accidental power restoration."
            },
            new Step
            {
                Id = 2,
                GuideId = 1,
                StepNumber = 2,
                Title = "Mounting the Unit",
                Instruction = "Position the HVAC unit on the mounting bracket, ensuring it is level. Use a carpenter's level to verify both horizontal and vertical alignment. Secure the unit using the provided mounting bolts, tightening in a cross pattern to ensure even pressure.",
                RequiredTools = "Carpenter's level, Socket wrench set, Mounting bolts (included)",
                SafetyNotes = "Unit weighs 75+ lbs. Use proper lifting technique or get assistance. Ensure mounting bracket is rated for unit weight."
            },
            new Step
            {
                Id = 3,
                GuideId = 1,
                StepNumber = 3,
                Title = "Electrical Connection",
                Instruction = "Connect the electrical wiring according to the wiring diagram provided with the unit. Match wire colors: black to black (hot), white to white (neutral), and green/bare to ground. Use wire nuts to secure all connections. Install the electrical cover plate.",
                RequiredTools = "Wire strippers, Screwdriver set, Wire nuts, Electrical tape",
                SafetyNotes = "DANGER: Ensure power remains off during all electrical work. Double-check all connections before restoring power. If unsure, consult a licensed electrician."
            }
        );

        // Seed media item
        modelBuilder.Entity<MediaItem>().HasData(
            new MediaItem
            {
                Id = 1,
                StepId = 1,
                MediaType = MediaType.Image,
                FilePath = "/media/hvac/safety-check.jpg"
            }
        );
    }
}
