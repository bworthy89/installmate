using System.Collections.Generic;
using System.Threading.Tasks;
using InstallVibe.Models;

namespace InstallVibe.Services;

public interface IGuideRepository
{
    Task<IEnumerable<Guide>> GetAllGuides();
    Task<Guide?> GetGuide(int id);
    Task<int> CreateGuide(Guide guide);
    Task UpdateGuide(Guide guide);
    Task SaveProgress(int guideId, int userId, List<int> completedSteps);
    Task<GuideProgress?> GetProgress(int guideId, int userId);
}
