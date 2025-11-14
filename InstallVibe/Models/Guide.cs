using System.Collections.Generic;

namespace InstallVibe.Models;

public class Guide
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public int? EstimatedDurationMinutes { get; set; }

    // Navigation properties
    public User CreatedByUser { get; set; } = null!;
    public ICollection<Step> Steps { get; set; } = new List<Step>();

    public Guide()
    {
    }

    public Guide(int id, string title, string description, string category, int createdByUserId)
    {
        Id = id;
        Title = title;
        Description = description;
        Category = category;
        CreatedByUserId = createdByUserId;
    }
}
