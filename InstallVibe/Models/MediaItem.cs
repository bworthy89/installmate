namespace InstallVibe.Models;

public class MediaItem
{
    public int Id { get; set; }
    public int StepId { get; set; }
    public MediaType MediaType { get; set; }
    public string FilePath { get; set; } = string.Empty;

    // Navigation property
    public Step Step { get; set; } = null!;

    public MediaItem()
    {
    }

    public MediaItem(int id, int stepId, MediaType mediaType, string filePath)
    {
        Id = id;
        StepId = stepId;
        MediaType = mediaType;
        FilePath = filePath;
    }
}
