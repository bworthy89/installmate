using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace InstallVibe.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public DatabaseService()
    {
        // Store database in local app data folder
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataFolder, "InstallVibe");

        // Ensure directory exists
        Directory.CreateDirectory(appFolder);

        _databasePath = Path.Combine(appFolder, "installvibe.db");
        _connectionString = $"Data Source={_databasePath}";
    }

    public string GetConnectionString() => _connectionString;

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Create Users table
        var createUsersTable = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE COLLATE NOCASE,
                PasswordHash TEXT NOT NULL,
                Role INTEGER NOT NULL
            );
        ";

        using var command = new SqliteCommand(createUsersTable, connection);
        await command.ExecuteNonQueryAsync();

        // Create index on Username for faster lookups
        var createIndex = @"
            CREATE INDEX IF NOT EXISTS idx_users_username
            ON Users(Username COLLATE NOCASE);
        ";

        using var indexCommand = new SqliteCommand(createIndex, connection);
        await indexCommand.ExecuteNonQueryAsync();
    }
}
