# Step 2: Database Schema

## SQLite Database Structure

**Database Location:** `%LocalAppData%\InstallVibe\installvibe.db`

### Users Table

```sql
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE COLLATE NOCASE,
    PasswordHash TEXT NOT NULL,
    Role INTEGER NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_users_username
ON Users(Username COLLATE NOCASE);
```

#### Column Details

| Column | Type | Description |
|--------|------|-------------|
| `Id` | INTEGER | Primary key, auto-incrementing user ID |
| `Username` | TEXT | Unique username (case-insensitive) |
| `PasswordHash` | TEXT | PBKDF2 password hash in format: `salt:hash` (base64) |
| `Role` | INTEGER | User role: 0=Admin, 1=Technician |

### Password Hash Format

Passwords are hashed using **PBKDF2** with the following parameters:

- **Algorithm:** SHA256
- **Iterations:** 100,000 (OWASP recommended)
- **Salt Size:** 32 bytes (256 bits)
- **Hash Size:** 32 bytes (256 bits)
- **Storage Format:** `{base64_salt}:{base64_hash}`

Example hash:
```
rTxY7kP8mNqZ3wHjL5vR2dC9fA1bE6gK4sT0uY8iO7w=:vN2jK9fL4hR8pS6mT1xQ3wE5yD0zC7gA9bH8uI4kO6c=
```

### Seeded Data

On first run, an admin account is automatically created:

| Field | Value |
|-------|-------|
| Username | `admin` |
| Password | `admin123` |
| Role | Admin (0) |

**⚠️ Important:** Change the admin password in production!

## Data Access Pattern

All database operations use parameterized queries to prevent SQL injection:

```csharp
var query = "SELECT * FROM Users WHERE Username = @username COLLATE NOCASE";
using var command = new SqliteCommand(query, connection);
command.Parameters.AddWithValue("@username", username);
```

## Security Features

1. **Case-Insensitive Username Lookup:** `COLLATE NOCASE` prevents duplicate usernames with different casing
2. **Parameterized Queries:** All queries use parameters to prevent SQL injection
3. **PBKDF2 Hashing:** Industry-standard password hashing with random salt
4. **Constant-Time Comparison:** Uses `CryptographicOperations.FixedTimeEquals()` to prevent timing attacks
