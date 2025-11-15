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
    public DbSet<FeedbackSubmission> Feedback { get; set; } = null!;

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

        // FeedbackSubmission configuration
        modelBuilder.Entity<FeedbackSubmission>(entity =>
        {
            entity.ToTable("Feedback");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.StepsToReproduce).HasMaxLength(1000);
            entity.Property(e => e.ExpectedBehavior).HasMaxLength(500);
            entity.Property(e => e.ActualBehavior).HasMaxLength(500);
            entity.Property(e => e.AppVersion).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.ScreenshotPath).HasMaxLength(500);
            entity.Property(e => e.AdminNotes).HasMaxLength(1000);
            entity.Property(e => e.GuideName).HasMaxLength(200);
            entity.Property(e => e.SubmittedAt).IsRequired();
            entity.Property(e => e.FeedbackType).IsRequired();
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.Severity).IsRequired();
            entity.Property(e => e.Status).IsRequired();

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SubmittedAt);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Note: Seed data is now handled dynamically in App.xaml.cs
        // after admin user is created to avoid foreign key constraint issues
    }
}
