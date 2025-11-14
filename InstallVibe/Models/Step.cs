using System.Collections.Generic;

namespace InstallVibe.Models;

public class Step
{
    public int Id { get; set; }
    public int GuideId { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public string? RequiredTools { get; set; }
    public string? SafetyNotes { get; set; }

    // Navigation properties
    public Guide Guide { get; set; } = null!;
    public ICollection<MediaItem> Media { get; set; } = new List<MediaItem>();

    public Step()
    {
    }

    public Step(int id, int guideId, int stepNumber, string title, string instruction)
    {
        Id = id;
        GuideId = guideId;
        StepNumber = stepNumber;
        Title = title;
        Instruction = instruction;
    }
}
