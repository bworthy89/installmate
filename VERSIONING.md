# Versioning Strategy for InstallVibe

Complete guide to version management, branching, and release processes.

## Semantic Versioning

InstallVibe follows [Semantic Versioning 2.0.0](https://semver.org/):

```
MAJOR.MINOR.PATCH-PRERELEASE+BUILD
1    .0    .0    -beta.1     +20240115
```

### Version Components

| Component | Increment When | Example | Description |
|-----------|----------------|---------|-------------|
| **MAJOR** | Breaking changes | 1.0.0 → 2.0.0 | Incompatible API changes, database schema changes |
| **MINOR** | New features | 1.0.0 → 1.1.0 | New functionality, backward-compatible |
| **PATCH** | Bug fixes | 1.0.0 → 1.0.1 | Backward-compatible bug fixes |
| **BUILD** | CI/CD builds | 1.0.0.0 → 1.0.0.1 | Auto-incremented by build system |

### Pre-Release Tags

- `alpha` - Internal testing, unstable
- `beta` - External testing, feature-complete
- `rc` (Release Candidate) - Final testing before release
- `dev` - Development builds

Examples:
- `1.0.0-alpha.1` - First alpha of 1.0.0
- `1.0.0-beta.2` - Second beta of 1.0.0
- `1.0.0-rc.1` - First release candidate
- `1.1.0-dev` - Development build for 1.1.0

---

## Branching Model

### Main Branches

```
main (production-ready code)
├── develop (integration branch)
├── release/1.0.0 (release preparation)
└── hotfix/1.0.1 (production fixes)
```

#### `main`
- **Purpose**: Production-ready code
- **Protected**: Requires PR + code review
- **Builds**: Automatically tagged and deployed
- **Version**: Matches current release

#### `develop`
- **Purpose**: Integration branch for next release
- **Protection**: Requires PR
- **Builds**: Continuous integration testing
- **Version**: Next minor version + `-dev`

### Supporting Branches

#### Feature Branches
```
feature/user-authentication
feature/offline-sync
feature/add-export-pdf
```

- **Branch from**: `develop`
- **Merge to**: `develop`
- **Naming**: `feature/<description>`
- **Lifetime**: Until feature complete

#### Release Branches
```
release/1.1.0
release/2.0.0
```

- **Branch from**: `develop`
- **Merge to**: `main` AND `develop`
- **Naming**: `release/<version>`
- **Purpose**: Release preparation, bug fixes only
- **Version**: Bumped to release version

#### Hotfix Branches
```
hotfix/1.0.1-fix-crash
hotfix/1.0.2-security-patch
```

- **Branch from**: `main`
- **Merge to**: `main` AND `develop`
- **Naming**: `hotfix/<version>-<description>`
- **Purpose**: Critical production fixes
- **Version**: Bumped patch version

---

## Version Management Workflow

### Starting a New Feature

```bash
# Create feature branch from develop
git checkout develop
git pull origin develop
git checkout -b feature/new-guide-editor

# Work on feature...

# Update version in feature if needed (rare)
# Usually version is updated during release preparation
```

### Preparing a Release

```bash
# Create release branch
git checkout develop
git pull origin develop
git checkout -b release/1.1.0

# Update version
.\Scripts\Update-Version.ps1 -VersionPart Minor
# This updates: version.json, Package.appxmanifest, appinstaller, csproj

# Commit version bump
git add .
git commit -m "Bump version to 1.1.0"

# Final testing and bug fixes...

# Merge to main
git checkout main
git merge --no-ff release/1.1.0
git tag v1.1.0
git push origin main --tags

# Merge back to develop
git checkout develop
git merge --no-ff release/1.1.0
git push origin develop

# Delete release branch
git branch -d release/1.1.0
```

### Creating a Hotfix

```bash
# Create hotfix branch from main
git checkout main
git pull origin main
git checkout -b hotfix/1.0.1-crash-fix

# Update version
.\Scripts\Update-Version.ps1 -VersionPart Patch

# Fix the issue...

# Commit
git add .
git commit -m "Fix critical crash in step navigation"

# Merge to main
git checkout main
git merge --no-ff hotfix/1.0.1-crash-fix
git tag v1.0.1
git push origin main --tags

# Merge to develop
git checkout develop
git merge --no-ff hotfix/1.0.1-crash-fix
git push origin develop

# Delete hotfix branch
git branch -d hotfix/1.0.1-crash-fix
```

---

## Version Files Reference

### version.json

Central version tracking file:

```json
{
  "version": "1.1.0",
  "major": 1,
  "minor": 1,
  "patch": 0,
  "build": 0,
  "fullVersion": "1.1.0.0",
  "versionTag": "v1.1.0",
  "releaseDate": "2024-02-15",
  "releaseName": "Offline Mode Release",
  "changelog": [
    "Added offline mode support",
    "Improved accessibility",
    "Performance optimizations"
  ]
}
```

### Package.appxmanifest

MSIX package version (must be 4-part):

```xml
<Identity
  Name="InstallVibe"
  Publisher="CN=YourCompany"
  Version="1.1.0.0"
  ProcessorArchitecture="x64" />
```

### InstallVibe.appinstaller

Update manifest version:

```xml
<AppInstaller
  Version="1.1.0.0"
  Uri="https://updates.company.com/installvibe/InstallVibe.appinstaller">
  <MainBundle
    Version="1.1.0.0"
    Uri="https://updates.company.com/installvibe/InstallVibe_1.1.0.0_bundle.msixbundle" />
</AppInstaller>
```

### InstallVibe.csproj

Assembly version in project file:

```xml
<PropertyGroup>
  <Version>1.1.0</Version>
  <AssemblyVersion>1.1.0.0</AssemblyVersion>
  <FileVersion>1.1.0.0</FileVersion>
</PropertyGroup>
```

---

## CI/CD Version Management

### Automatic Version Increment

CI/CD pipelines can automatically increment build number:

```yaml
# GitHub Actions
- name: Update build number
  run: |
    .\Scripts\Update-Version.ps1 -VersionPart Build
    $version = (Get-Content version.json | ConvertFrom-Json).fullVersion
    echo "BUILD_VERSION=$version" >> $env:GITHUB_ENV
```

### Version from Git Tag

Extract version from tag for release builds:

```yaml
# On tag push (v1.1.0)
- name: Get version from tag
  if: startsWith(github.ref, 'refs/tags/v')
  run: |
    $version = "${{ github.ref_name }}" -replace '^v', ''
    .\Scripts\Update-Version.ps1 -SetVersion $version
```

---

## Version Comparison Rules

### Installation and Updates

Windows MSIX compares versions as 4-part numbers:

```
1.0.0.0 < 1.0.0.1 < 1.0.1.0 < 1.1.0.0 < 2.0.0.0
```

**Rules:**
- Each update MUST have higher version than previous
- Version numbers cannot decrease
- Pre-release tags are ignored by MSIX
- Build number (4th component) is critical for daily builds

### Update Service Comparison

UpdateService uses semantic versioning:

```csharp
// version.json: 1.1.0-beta.1 is LESS than 1.1.0
var current = new Version("1.1.0");
var latest = new Version("1.1.0-beta.1");  // Ignored by Version class
```

---

## Version Changelog Management

### Maintaining CHANGELOG.md

Follow [Keep a Changelog](https://keepachangelog.com/):

```markdown
# Changelog

## [Unreleased]
### Added
- New guide editor with drag-and-drop

## [1.1.0] - 2024-02-15
### Added
- Offline mode support
- Network connectivity detection
- Portable distribution option

### Changed
- Improved update notification UI
- Enhanced accessibility features

### Fixed
- Fixed crash when navigating to completed guides
- Resolved memory leak in image loading

## [1.0.0] - 2024-01-15
### Added
- Initial release
- Step-by-step installation guides
- Rich media support
```

### Generating Release Notes

Automatically extract from CHANGELOG:

```powershell
# Extract version section from CHANGELOG
$changelog = Get-Content CHANGELOG.md -Raw
$pattern = "## \[$version\][^\n]*\n(.*?)(?=## \[|$)"
$releaseNotes = [regex]::Match($changelog, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline).Groups[1].Value
```

---

## Best Practices

✅ **Do:**
- Always update version before merging to main
- Use semantic versioning consistently
- Tag releases with `v` prefix: `v1.0.0`
- Keep version.json as single source of truth
- Document breaking changes clearly
- Test upgrades from previous versions

❌ **Don't:**
- Skip version numbers
- Reuse version numbers
- Manually edit multiple version files (use script)
- Tag without updating version files
- Break semantic versioning conventions

---

## Version Checking Scripts

### Check if files are in sync

```powershell
# Check-VersionSync.ps1
$versionJson = (Get-Content version.json | ConvertFrom-Json).fullVersion
$manifest = ([xml](Get-Content InstallVibe.Package/Package.appxmanifest)).Package.Identity.Version

if ($versionJson -ne $manifest) {
    Write-Error "Version mismatch! version.json: $versionJson, manifest: $manifest"
    exit 1
}

Write-Host "✓ All versions in sync: $versionJson"
```

### Get current version

```powershell
# Get-Version.ps1
$version = (Get-Content version.json | ConvertFrom-Json)
Write-Output $version.fullVersion
```

---

## FAQ

**Q: Why do MSIX versions have 4 parts but semantic versions have 3?**
A: MSIX requires 4-part versions (Major.Minor.Build.Revision). We map semantic MAJOR.MINOR.PATCH to MSIX Major.Minor.Patch.0, and use the 4th part for CI build numbers.

**Q: Can I downgrade a version?**
A: No, MSIX installations cannot downgrade. Users must uninstall and reinstall.

**Q: How do I handle pre-release versions?**
A: Use separate distribution channels (dev, testing, production) with different appinstaller files. Pre-release tags in version.json are for documentation only.

**Q: What happens if I forget to update the version?**
A: MSIX installation will fail with "Version number must be higher than installed version" error. Always increment before release.

---

## Related Documentation

- [DEPLOYMENT_INSTRUCTIONS.md](InstallVibe.Package/DEPLOYMENT_INSTRUCTIONS.md)
- [SIGNING.md](SIGNING.md)
- [GitHub Actions Workflow](.github/workflows/release.yml)
- [Azure DevOps Pipeline](azure-pipelines.yml)
