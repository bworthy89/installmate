using System;

namespace InstallVibe.Models;

/// <summary>
/// Represents a feedback submission from a technician
/// </summary>
public class FeedbackSubmission
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public int? GuideId { get; set; }

    public string? GuideName { get; set; }

    public FeedbackType FeedbackType { get; set; }

    public int? Rating { get; set; } // 1-5 stars

    public FeedbackCategory Category { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Bug report specific fields
    public string? StepsToReproduce { get; set; }

    public string? ExpectedBehavior { get; set; }

    public string? ActualBehavior { get; set; }

    public IssueSeverity Severity { get; set; }

    // Metadata
    public string AppVersion { get; set; } = string.Empty;

    public string? DeviceInfo { get; set; }

    public string? ScreenshotPath { get; set; }

    public DateTime SubmittedAt { get; set; }

    public FeedbackStatus Status { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? AdminNotes { get; set; }
}

public enum FeedbackType
{
    General,
    BugReport,
    FeatureRequest,
    UsabilityIssue,
    Performance,
    Documentation
}

public enum FeedbackCategory
{
    UserInterface,
    Functionality,
    Performance,
    Documentation,
    Installation,
    Navigation,
    Accessibility,
    Other
}

public enum IssueSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum FeedbackStatus
{
    Pending,
    Reviewed,
    InProgress,
    Resolved,
    WontFix
}

/// <summary>
/// Statistics about collected feedback
/// </summary>
public class FeedbackStatistics
{
    public int TotalFeedback { get; set; }
    public int PendingCount { get; set; }
    public int ReviewedCount { get; set; }
    public int ResolvedCount { get; set; }
    public int BugReports { get; set; }
    public int FeatureRequests { get; set; }
    public int GeneralFeedback { get; set; }
    public double AverageRating { get; set; }
    public int CriticalIssues { get; set; }
    public int HighPriorityIssues { get; set; }
}
