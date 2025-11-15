using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using InstallVibe.Data;
using InstallVibe.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InstallVibe.Services;

public class AuthService : IAuthService
{
    private User? _currentUser;

    // PBKDF2 parameters
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // OWASP recommended minimum

    public async Task<User?> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InstallVibeDbContext>();

        // Find user by username (case-insensitive)
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user != null && VerifyPassword(password, user.PasswordHash))
        {
            _currentUser = user;
            return _currentUser;
        }

        return null;
    }

    public Task Logout()
    {
        _currentUser = null;
        return Task.CompletedTask;
    }

    public User? GetCurrentUser()
    {
        return _currentUser;
    }

    public bool IsAdmin()
    {
        return _currentUser?.Role == UserRole.Admin;
    }

    public async Task SeedAdmin()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InstallVibeDbContext>();

        // Check if admin user already exists
        var adminExists = await context.Users
            .AnyAsync(u => u.Username.ToLower() == "admin");

        if (adminExists)
        {
            return;
        }

        // Create admin user with password "admin123"
        var passwordHash = HashPassword("admin123");

        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = passwordHash,
            Role = UserRole.Admin
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Hashes a password using PBKDF2 with a random salt.
    /// Returns the hash in the format: salt:hash (both base64 encoded)
    /// </summary>
    private string HashPassword(string password)
    {
        // Generate random salt
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Generate hash using PBKDF2
        byte[] hash = GeneratePbkdf2Hash(password, salt);

        // Combine salt and hash with separator
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    private bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            // Split stored hash into salt and hash parts
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hash = Convert.FromBase64String(parts[1]);

            // Generate hash from provided password with stored salt
            byte[] testHash = GeneratePbkdf2Hash(password, salt);

            // Compare hashes using constant-time comparison
            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates PBKDF2 hash using the specified password and salt.
    /// </summary>
    private byte[] GeneratePbkdf2Hash(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(HashSize);
    }
}
