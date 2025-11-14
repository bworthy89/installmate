using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using InstallVibe.Models;
using Microsoft.Data.Sqlite;

namespace InstallVibe.Services;

public class AuthService : IAuthService
{
    private readonly IDatabaseService _databaseService;
    private User? _currentUser;

    // PBKDF2 parameters
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // OWASP recommended minimum

    public AuthService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<User?> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        using var connection = new SqliteConnection(_databaseService.GetConnectionString());
        await connection.OpenAsync();

        var query = "SELECT Id, Username, PasswordHash, Role FROM Users WHERE Username = @username COLLATE NOCASE";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var id = reader.GetInt32(0);
            var dbUsername = reader.GetString(1);
            var passwordHash = reader.GetString(2);
            var role = (UserRole)reader.GetInt32(3);

            // Verify password using PBKDF2
            if (VerifyPassword(password, passwordHash))
            {
                _currentUser = new User(id, dbUsername, passwordHash, role);
                return _currentUser;
            }
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

    public async Task SeedAdmin()
    {
        using var connection = new SqliteConnection(_databaseService.GetConnectionString());
        await connection.OpenAsync();

        // Check if admin user already exists
        var checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username COLLATE NOCASE";
        using var checkCommand = new SqliteCommand(checkQuery, connection);
        checkCommand.Parameters.AddWithValue("@username", "admin");

        var count = (long)(await checkCommand.ExecuteScalarAsync() ?? 0L);

        if (count > 0)
        {
            // Admin already exists
            return;
        }

        // Create admin user with password "admin123"
        var passwordHash = HashPassword("admin123");

        var insertQuery = @"
            INSERT INTO Users (Username, PasswordHash, Role)
            VALUES (@username, @passwordHash, @role)
        ";

        using var insertCommand = new SqliteCommand(insertQuery, connection);
        insertCommand.Parameters.AddWithValue("@username", "admin");
        insertCommand.Parameters.AddWithValue("@passwordHash", passwordHash);
        insertCommand.Parameters.AddWithValue("@role", (int)UserRole.Admin);

        await insertCommand.ExecuteNonQueryAsync();
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
