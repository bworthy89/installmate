using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InstallVibe.Data;
using InstallVibe.Models;

namespace InstallVibe.Services;

public class GuideRepository : IGuideRepository
{
    private readonly InstallVibeDbContext _context;

    public GuideRepository(InstallVibeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Guide>> GetAllGuides()
    {
        return await _context.Guides
            .Include(g => g.Steps)
                .ThenInclude(s => s.Media)
            .Include(g => g.CreatedByUser)
            .OrderBy(g => g.Title)
            .ToListAsync();
    }

    public async Task<Guide?> GetGuide(int id)
    {
        return await _context.Guides
            .Include(g => g.Steps.OrderBy(s => s.StepNumber))
                .ThenInclude(s => s.Media)
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<int> CreateGuide(Guide guide)
    {
        _context.Guides.Add(guide);
        await _context.SaveChangesAsync();
        return guide.Id;
    }

    public async Task UpdateGuide(Guide guide)
    {
        _context.Guides.Update(guide);
        await _context.SaveChangesAsync();
    }

    public async Task SaveProgress(int guideId, int userId, List<int> completedSteps)
    {
        var existingProgress = await _context.GuideProgresses
            .FirstOrDefaultAsync(gp => gp.GuideId == guideId && gp.UserId == userId);

        if (existingProgress != null)
        {
            // Update existing progress
            existingProgress.CompletedStepIds = completedSteps;
            existingProgress.LastUpdated = DateTime.UtcNow;
            _context.GuideProgresses.Update(existingProgress);
        }
        else
        {
            // Create new progress record
            var newProgress = new GuideProgress
            {
                GuideId = guideId,
                UserId = userId,
                CompletedStepIds = completedSteps,
                LastUpdated = DateTime.UtcNow
            };
            _context.GuideProgresses.Add(newProgress);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<GuideProgress?> GetProgress(int guideId, int userId)
    {
        return await _context.GuideProgresses
            .Include(gp => gp.Guide)
            .Include(gp => gp.User)
            .FirstOrDefaultAsync(gp => gp.GuideId == guideId && gp.UserId == userId);
    }
}
