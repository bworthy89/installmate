using System.Collections.Generic;
using System.Threading.Tasks;
using InstallVibe.Models;

namespace InstallVibe.Services;

public interface IGuideRepository
{
    // Guide operations
    Task<IEnumerable<Guide>> GetAllGuides();
    Task<Guide?> GetGuide(int id);
    Task<int> CreateGuide(Guide guide);
    Task UpdateGuide(Guide guide);
    Task DeleteGuide(int guideId);

    // Step operations
    Task<int> CreateStep(int guideId, Step step);
    Task UpdateStep(Step step);
    Task DeleteStep(int stepId);
    Task<Step?> GetStep(int stepId);

    // Media operations
    Task<int> AddMediaToStep(int stepId, MediaItem media);
    Task DeleteMedia(int mediaId);

    // Progress operations
    Task SaveProgress(int guideId, int userId, List<int> completedSteps);
    Task<GuideProgress?> GetProgress(int guideId, int userId);
}
