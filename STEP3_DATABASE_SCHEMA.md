# Step 3: EF Core Database Schema

## Database Overview

InstallVibe uses **Entity Framework Core** with **SQLite** for the guide management system. The same database file used for authentication (`installvibe.db`) now contains additional tables for guides, steps, media, and progress tracking.

**Database Location:** `%LocalAppData%\InstallVibe\installvibe.db`

---

## Table Schemas

### Guides Table

Stores installation guide metadata.

```sql
CREATE TABLE "Guides" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Guides" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL CHECK(length("Title") <= 200),
    "Description" TEXT NOT NULL CHECK(length("Description") <= 1000),
    "Category" TEXT NOT NULL CHECK(length("Category") <= 100),
    "CreatedByUserId" INTEGER NOT NULL,
    "EstimatedDurationMinutes" INTEGER NULL,
    CONSTRAINT "FK_Guides_Users_CreatedByUserId"
        FOREIGN KEY ("CreatedByUserId")
        REFERENCES "Users" ("Id")
        ON DELETE RESTRICT
);

CREATE INDEX "IX_Guides_CreatedByUserId" ON "Guides" ("CreatedByUserId");
```

**Columns:**
- `Id` - Primary key, auto-increment
- `Title` - Guide title (max 200 chars)
- `Description` - Detailed description (max 1000 chars)
- `Category` - Category/type (max 100 chars)
- `CreatedByUserId` - Foreign key to Users table
- `EstimatedDurationMinutes` - Optional duration estimate

**Relationships:**
- Many-to-one with Users (creator)
- One-to-many with Steps (cascade delete)

---

### Steps Table

Stores individual steps within a guide.

```sql
CREATE TABLE "Steps" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Steps" PRIMARY KEY AUTOINCREMENT,
    "GuideId" INTEGER NOT NULL,
    "StepNumber" INTEGER NOT NULL,
    "Title" TEXT NOT NULL CHECK(length("Title") <= 200),
    "Instruction" TEXT NOT NULL CHECK(length("Instruction") <= 2000),
    "RequiredTools" TEXT NULL CHECK(length("RequiredTools") <= 500),
    "SafetyNotes" TEXT NULL CHECK(length("SafetyNotes") <= 500),
    CONSTRAINT "FK_Steps_Guides_GuideId"
        FOREIGN KEY ("GuideId")
        REFERENCES "Guides" ("Id")
        ON DELETE CASCADE
);

CREATE INDEX "IX_Steps_GuideId_StepNumber" ON "Steps" ("GuideId", "StepNumber");
```

**Columns:**
- `Id` - Primary key, auto-increment
- `GuideId` - Foreign key to Guides table
- `StepNumber` - Sequential step number within guide
- `Title` - Step title (max 200 chars)
- `Instruction` - Detailed instructions (max 2000 chars)
- `RequiredTools` - Optional tools list (max 500 chars)
- `SafetyNotes` - Optional safety warnings (max 500 chars)

**Relationships:**
- Many-to-one with Guides (parent guide)
- One-to-many with MediaItems (cascade delete)

**Delete Behavior:**
- When a Guide is deleted, all its Steps are **cascade deleted**

---

### MediaItems Table

Stores media (images, videos) associated with steps.

```sql
CREATE TABLE "MediaItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_MediaItems" PRIMARY KEY AUTOINCREMENT,
    "StepId" INTEGER NOT NULL,
    "MediaType" INTEGER NOT NULL,
    "FilePath" TEXT NOT NULL CHECK(length("FilePath") <= 500),
    CONSTRAINT "FK_MediaItems_Steps_StepId"
        FOREIGN KEY ("StepId")
        REFERENCES "Steps" ("Id")
        ON DELETE CASCADE
);

CREATE INDEX "IX_MediaItems_StepId" ON "MediaItems" ("StepId");
```

**Columns:**
- `Id` - Primary key, auto-increment
- `StepId` - Foreign key to Steps table
- `MediaType` - Integer: 0 = Image, 1 = Video
- `FilePath` - Path to media file (max 500 chars)

**MediaType Enum:**
```csharp
public enum MediaType
{
    Image = 0,
    Video = 1
}
```

**Relationships:**
- Many-to-one with Steps (parent step)

**Delete Behavior:**
- When a Step is deleted, all its MediaItems are **cascade deleted**
- When a Guide is deleted, Steps are deleted, which cascades to MediaItems

---

### GuideProgresses Table

Tracks per-user completion status for guides.

```sql
CREATE TABLE "GuideProgresses" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_GuideProgresses" PRIMARY KEY AUTOINCREMENT,
    "GuideId" INTEGER NOT NULL,
    "UserId" INTEGER NOT NULL,
    "CompletedStepIds" TEXT NOT NULL,
    "LastUpdated" TEXT NOT NULL,
    CONSTRAINT "FK_GuideProgresses_Guides_GuideId"
        FOREIGN KEY ("GuideId")
        REFERENCES "Guides" ("Id")
        ON DELETE CASCADE,
    CONSTRAINT "FK_GuideProgresses_Users_UserId"
        FOREIGN KEY ("UserId")
        REFERENCES "Users" ("Id")
        ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_GuideProgresses_GuideId_UserId"
    ON "GuideProgresses" ("GuideId", "UserId");
```

**Columns:**
- `Id` - Primary key, auto-increment
- `GuideId` - Foreign key to Guides table
- `UserId` - Foreign key to Users table
- `CompletedStepIds` - JSON array of completed step IDs (stored as TEXT)
- `LastUpdated` - DateTime of last progress update (ISO 8601 format)

**CompletedStepIds Storage:**
- Stored as JSON text: `[1,2,3]`
- Automatically converted to/from `List<int>` by EF Core value converter
- Example: `"[1,3,5]"` in database → `List<int> {1, 3, 5}` in C#

**Constraints:**
- Unique index on `(GuideId, UserId)` - one progress record per user per guide

**Relationships:**
- Many-to-one with Guides
- Many-to-one with Users

**Delete Behavior:**
- When a Guide is deleted, all associated progress records are **cascade deleted**
- When a User is deleted, all their progress records are **cascade deleted**

---

## Relationships Diagram

```
Users (existing from Step 2)
  ├── 1:N → Guides (created by user) [RESTRICT]
  └── 1:N → GuideProgresses (user's progress) [CASCADE]

Guides
  ├── 1:N → Steps [CASCADE]
  └── 1:N → GuideProgresses [CASCADE]

Steps
  └── 1:N → MediaItems [CASCADE]
```

**Delete Cascade Chain:**
```
Delete Guide
  ├── Cascades to → Steps
  │     └── Cascades to → MediaItems
  └── Cascades to → GuideProgresses
```

---

## EF Core Configuration Highlights

### JSON Value Conversion (CompletedStepIds)

```csharp
entity.Property(e => e.CompletedStepIds)
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null)
             ?? new List<int>()
    )
    .HasColumnType("TEXT");
```

This allows storing `List<int>` as JSON text in SQLite.

### Cascade Delete Configuration

```csharp
// Steps cascade delete when Guide is deleted
entity.HasMany(e => e.Steps)
    .WithOne(s => s.Guide)
    .HasForeignKey(s => s.GuideId)
    .OnDelete(DeleteBehavior.Cascade);

// MediaItems cascade delete when Step is deleted
entity.HasMany(e => e.Media)
    .WithOne(m => m.Step)
    .HasForeignKey(m => m.StepId)
    .OnDelete(DeleteBehavior.Cascade);
```

### Navigation Properties

EF Core automatically loads related data when using `Include()`:

```csharp
var guide = await context.Guides
    .Include(g => g.Steps.OrderBy(s => s.StepNumber))
        .ThenInclude(s => s.Media)
    .Include(g => g.CreatedByUser)
    .FirstOrDefaultAsync(g => g.Id == id);
```

This loads:
- The guide
- All its steps (ordered by StepNumber)
- All media for each step
- The user who created the guide

---

## Seeded Data

On first run, the following sample data is automatically seeded:

### Sample Guide
- **Title:** Standard HVAC Unit Installation
- **Category:** HVAC
- **Duration:** 180 minutes
- **Created by:** admin (UserId = 1)

### Sample Steps

**Step 1: Pre-installation Safety Check**
- Instructions for power shutoff and PPE
- Required tools: Voltage tester, Safety glasses, Work gloves
- Safety notes about electrical hazards
- Has 1 media item (safety check image)

**Step 2: Mounting the Unit**
- Instructions for positioning and securing unit
- Required tools: Carpenter's level, Socket wrench set, Mounting bolts
- Safety notes about unit weight

**Step 3: Electrical Connection**
- Wiring instructions with color coding
- Required tools: Wire strippers, Screwdriver set, Wire nuts, Electrical tape
- Safety notes about electrical work

### Sample Media
- **FilePath:** `/media/hvac/safety-check.jpg`
- **Type:** Image
- **Linked to:** Step 1

---

## Database Initialization

The database is automatically created and seeded on first app launch via:

```csharp
await context.Database.EnsureCreatedAsync();
```

This:
1. Creates the database file if it doesn't exist
2. Creates all tables with proper schema
3. Inserts seed data

**No manual migrations required** - EF Core handles everything automatically.

---

## Query Examples

### Get Guide with All Related Data
```csharp
var guide = await context.Guides
    .Include(g => g.Steps.OrderBy(s => s.StepNumber))
        .ThenInclude(s => s.Media)
    .Include(g => g.CreatedByUser)
    .FirstOrDefaultAsync(g => g.Id == 1);
```

### Get User's Progress
```csharp
var progress = await context.GuideProgresses
    .Include(gp => gp.Guide)
    .Include(gp => gp.User)
    .FirstOrDefaultAsync(gp => gp.GuideId == 1 && gp.UserId == 1);
```

### Count Completed Steps
```csharp
int completedCount = progress.CompletedStepIds.Count;
int totalSteps = guide.Steps.Count;
double percentage = (completedCount / (double)totalSteps) * 100;
```

---

## Migration Notes

Since we're using `EnsureCreatedAsync()`, the database schema is created automatically. If you need to modify the schema in production:

1. **Option 1:** Use EF Core migrations
   ```bash
   dotnet ef migrations add YourMigrationName
   dotnet ef database update
   ```

2. **Option 2:** Delete the database file and let it recreate (development only)
   - Delete `%LocalAppData%\InstallVibe\installvibe.db`
   - Restart the app
   - Fresh database with seed data will be created

⚠️ **Warning:** Deleting the database removes all user data and progress!

---

## Performance Considerations

### Indexes
- `IX_Guides_CreatedByUserId` - Fast lookups of guides by creator
- `IX_Steps_GuideId_StepNumber` - Fast ordered retrieval of steps
- `IX_MediaItems_StepId` - Fast retrieval of media for a step
- `IX_GuideProgresses_GuideId_UserId` - Fast progress lookups (unique constraint)

### Eager Loading
Always use `.Include()` and `.ThenInclude()` to avoid N+1 query problems:

```csharp
// Good - 1 query with JOINs
var guides = await context.Guides
    .Include(g => g.Steps)
    .ToListAsync();

// Bad - N+1 queries (1 for guides + 1 per guide for steps)
var guides = await context.Guides.ToListAsync();
foreach (var guide in guides)
{
    var steps = guide.Steps; // Separate query for each guide!
}
```

---

## Security Notes

✅ **Parameterized queries:** EF Core automatically uses parameterized queries
✅ **SQL injection protection:** EF Core prevents SQL injection by design
✅ **Foreign key constraints:** Data integrity enforced at database level
✅ **Cascade deletes:** Orphaned records automatically cleaned up

⚠️ **User input validation:** Always validate before saving to database
⚠️ **File paths:** Validate media file paths to prevent directory traversal attacks
