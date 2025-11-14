using System.Threading.Tasks;

namespace InstallVibe.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    string GetConnectionString();
}
