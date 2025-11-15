using System.Collections.Generic;
using System.Threading.Tasks;
using InstallVibe.Models;

namespace InstallVibe.Services;

/// <summary>
/// Service for collecting and managing user feedback
/// </summary>
public interface IFeedbackService
{
    /// <summary>
    /// Submit feedback from a technician
    /// </summary>
    Task<bool> SubmitFeedbackAsync(FeedbackSubmission feedback);

    /// <summary>
    /// Get all pending feedback items
    /// </summary>
    Task<IEnumerable<FeedbackSubmission>> GetPendingFeedbackAsync();

    /// <summary>
    /// Export feedback to JSON file
    /// </summary>
    Task<string> ExportFeedbackAsync(string outputPath);

    /// <summary>
    /// Mark feedback as reviewed
    /// </summary>
    Task MarkAsReviewedAsync(int feedbackId);

    /// <summary>
    /// Get feedback statistics
    /// </summary>
    Task<FeedbackStatistics> GetStatisticsAsync();
}
