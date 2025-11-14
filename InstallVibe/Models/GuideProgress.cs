using System;
using System.Collections.Generic;

namespace InstallVibe.Models;

public class GuideProgress
{
    public int Id { get; set; }
    public int GuideId { get; set; }
    public int UserId { get; set; }
    public List<int> CompletedStepIds { get; set; } = new List<int>();
    public DateTime LastUpdated { get; set; }

    // Navigation properties
    public Guide Guide { get; set; } = null!;
    public User User { get; set; } = null!;

    public GuideProgress()
    {
    }

    public GuideProgress(int id, int guideId, int userId, List<int> completedStepIds, DateTime lastUpdated)
    {
        Id = id;
        GuideId = guideId;
        UserId = userId;
        CompletedStepIds = completedStepIds;
        LastUpdated = lastUpdated;
    }
}
