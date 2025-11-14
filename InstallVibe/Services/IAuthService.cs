using System.Threading.Tasks;
using InstallVibe.Models;

namespace InstallVibe.Services;

public interface IAuthService
{
    Task<User?> Login(string username, string password);
    Task Logout();
    User? GetCurrentUser();
    Task SeedAdmin();
}
