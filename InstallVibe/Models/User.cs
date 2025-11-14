namespace InstallVibe.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public User()
    {
    }

    public User(int id, string username, string passwordHash, UserRole role)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
    }
}
