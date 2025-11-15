using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InstallVibe.Data;
using InstallVibe.Models;

namespace InstallVibe.Services;

/// <summary>
/// Implementation of feedback collection service
/// </summary>
public class FeedbackService : IFeedbackService
{
    private readonly InstallVibeDbContext _dbContext;

    public FeedbackService(InstallVibeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> SubmitFeedbackAsync(FeedbackSubmission feedback)
    {
        try
        {
            feedback.SubmittedAt = DateTime.UtcNow;
            feedback.Status = FeedbackStatus.Pending;

            _dbContext.Feedback.Add(feedback);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            // Log error
            System.Diagnostics.Debug.WriteLine($"Error submitting feedback: {ex.Message}");
            return false;
        }
    }

    public async Task<IEnumerable<FeedbackSubmission>> GetPendingFeedbackAsync()
    {
        return await _dbContext.Feedback
            .Where(f => f.Status == FeedbackStatus.Pending)
            .OrderByDescending(f => f.SubmittedAt)
            .ToListAsync();
    }

    public async Task<string> ExportFeedbackAsync(string outputPath)
    {
        var allFeedback = await _dbContext.Feedback
            .OrderByDescending(f => f.SubmittedAt)
            .ToListAsync();

        var exportData = new
        {
            ExportedAt = DateTime.UtcNow,
            TotalFeedback = allFeedback.Count,
            Feedback = allFeedback.Select(f => new
            {
                f.Id,
                f.UserId,
                f.GuideId,
                f.GuideName,
                f.FeedbackType,
                f.Rating,
                f.Category,
                f.Subject,
                f.Description,
                f.StepsToReproduce,
                f.ExpectedBehavior,
                f.ActualBehavior,
                f.Severity,
                f.Status,
                f.SubmittedAt,
                f.ReviewedAt,
                f.UserEmail,
                f.AppVersion
            })
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(exportData, options);
        await File.WriteAllTextAsync(outputPath, json);

        return outputPath;
    }

    public async Task MarkAsReviewedAsync(int feedbackId)
    {
        var feedback = await _dbContext.Feedback.FindAsync(feedbackId);
        if (feedback != null)
        {
            feedback.Status = FeedbackStatus.Reviewed;
            feedback.ReviewedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<FeedbackStatistics> GetStatisticsAsync()
    {
        var allFeedback = await _dbContext.Feedback.ToListAsync();

        return new FeedbackStatistics
        {
            TotalFeedback = allFeedback.Count,
            PendingCount = allFeedback.Count(f => f.Status == FeedbackStatus.Pending),
            ReviewedCount = allFeedback.Count(f => f.Status == FeedbackStatus.Reviewed),
            ResolvedCount = allFeedback.Count(f => f.Status == FeedbackStatus.Resolved),
            BugReports = allFeedback.Count(f => f.FeedbackType == FeedbackType.BugReport),
            FeatureRequests = allFeedback.Count(f => f.FeedbackType == FeedbackType.FeatureRequest),
            GeneralFeedback = allFeedback.Count(f => f.FeedbackType == FeedbackType.General),
            AverageRating = allFeedback.Where(f => f.Rating.HasValue).Average(f => f.Rating) ?? 0,
            CriticalIssues = allFeedback.Count(f => f.Severity == IssueSeverity.Critical),
            HighPriorityIssues = allFeedback.Count(f => f.Severity == IssueSeverity.High)
        };
    }
}
